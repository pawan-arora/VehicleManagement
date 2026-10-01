using CreditWorks.VehicleManagement.Modules.Vehicles.Data;
using CreditWorks.VehicleManagement.Modules.Vehicles.Services;
using CreditWorks.VehicleManagement.Shared.Categories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CreditWorks.VehicleManagement.Tests.Vehicles;

/// <summary>
/// Tests VehicleService on its own. The Categories module (ICategoryResolver) is replaced with a Moq mock, so these
/// tests control what it answers. The database is EF Core's in-memory database, a fresh one for each test, holding the
/// seeded manufacturers.
/// </summary>
public class VehicleServiceTests
{
    private static readonly CategorySummary Medium = new(1002, "Medium", "car");
    private static readonly CategorySummary Heavy = new(1003, "Heavy", "truck");

    private const int MazdaId = 1;

    private readonly VehiclesDbContext db;
    private readonly Mock<ICategoryResolver> categories;   // Mockito: mock(ICategoryResolver.class)
    private readonly VehicleService service;

    public VehicleServiceTests()
    {
        var options = new DbContextOptionsBuilder<VehiclesDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        db = new VehiclesDbContext(options);
        db.Database.EnsureCreated();   // adds the seeded manufacturers

        categories = new Mock<ICategoryResolver>();
        service = new VehicleService(db, categories.Object);   // .Object is the fake to pass in

        // Unless a test says otherwise, every weight is Medium.
        CategoryForAnyWeightIs(Medium);
    }

    /// <summary>Makes the mock answer "every weight belongs to this category" (Mockito: when(...).thenReturn(...)).</summary>
    private void CategoryForAnyWeightIs(CategorySummary category) =>
        categories
            .Setup(resolver => resolver.GetForWeightsAsync(It.IsAny<IEnumerable<decimal>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<decimal> weights, CancellationToken _) => weights.Distinct().ToDictionary(weight => weight, _ => category));

    private static VehicleInput ValidVehicle(decimal weightKg = 1850.75m) => new("John Smith", MazdaId, 2019, weightKg);

    // ---- Adding a vehicle ----

    [Fact]
    public async Task Add_ValidVehicle_SavesIt_WithTheCategoryFromTheCategoriesModule()
    {
        var result = await service.AddAsync(ValidVehicle());

        Assert.Empty(result.Errors);
        Assert.Equal("Medium", result.Vehicle!.Category.Name);
        Assert.Equal("Mazda", result.Vehicle.Manufacturer.Name);
        Assert.Equal(1, await db.Vehicles.CountAsync());
    }

    [Fact]
    public async Task Add_WithEverythingMissing_ReportsEachRequiredField_AndSavesNothing()
    {
        categories.Setup(resolver => resolver.CheckWeight(null)).Returns("Weight is required.");

        var result = await service.AddAsync(new VehicleInput(null, null, null, null));

        Assert.Equal(["ManufacturerId", "OwnerName", "WeightKg", "YearOfManufacture"], result.Errors.Select(error => error.Field).Order());
        Assert.Equal(0, await db.Vehicles.CountAsync());
    }

    [Fact]
    public async Task Add_WhenTheCategoriesModuleRejectsTheWeight_ShowsItsMessage_AndSavesNothing()
    {
        // Mockito: when(categories.checkWeight(-5)).thenReturn("Weight must be greater than 0.")
        categories.Setup(resolver => resolver.CheckWeight(-5m)).Returns("Weight must be greater than 0.");

        var result = await service.AddAsync(ValidVehicle(weightKg: -5m));

        var error = Assert.Single(result.Errors);
        Assert.Equal(("WeightKg", "Weight must be greater than 0."), (error.Field, error.Message));
        Assert.Equal(0, await db.Vehicles.CountAsync());

        // Mockito: verify(categories, never()).getForWeightsAsync(any(), any())
        categories.Verify(
            resolver => resolver.GetForWeightsAsync(It.IsAny<IEnumerable<decimal>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Add_UnknownManufacturer_IsRejected_AndSavesNothing()
    {
        var result = await service.AddAsync(ValidVehicle() with { ManufacturerId = 99 });

        var error = Assert.Single(result.Errors);
        Assert.Equal(("ManufacturerId", "Manufacturer 99 does not exist."), (error.Field, error.Message));
        Assert.Equal(0, await db.Vehicles.CountAsync());
    }

    [Theory]
    [InlineData(1886, true)]    // the first petrol car
    [InlineData(1885, false)]
    [InlineData(1, true)]       // next year: next year's models go on sale this year
    [InlineData(2, false)]      // the year after next
    public async Task Add_YearOfManufacture_MustBeFrom1886ToNextYear(int year, bool isAllowed)
    {
        // Small numbers mean "this many years from now", so the test keeps working in future years.
        var yearOfManufacture = year < 100 ? DateTime.Today.Year + year : year;

        var result = await service.AddAsync(ValidVehicle() with { YearOfManufacture = yearOfManufacture });

        Assert.Equal(isAllowed, result.Errors.Count == 0);
    }

    // ---- Reading the vehicle list ----

    [Fact]
    public async Task GetAll_ShowsTheCurrentCategory_SoACategoryChangeAppliesToExistingVehicles()
    {
        // The brief's example: a 2200 kg vehicle is Medium, until the categories change and it becomes Heavy.
        await service.AddAsync(ValidVehicle(weightKg: 2200m));
        Assert.Equal("Medium", (await service.GetAllAsync(VehicleSortField.OwnerName, false)).Single().Category.Name);

        // The categories change: the Categories module now says 2200 kg is Heavy.
        CategoryForAnyWeightIs(Heavy);

        // The same stored vehicle now shows Heavy, because the category is looked up on every read, never stored.
        Assert.Equal("Heavy", (await service.GetAllAsync(VehicleSortField.OwnerName, false)).Single().Category.Name);
    }

    [Theory]
    [InlineData(VehicleSortField.OwnerName, false, new[] { "Aroha", "Bob", "Cara" })]
    [InlineData(VehicleSortField.OwnerName, true, new[] { "Cara", "Bob", "Aroha" })]
    [InlineData(VehicleSortField.Manufacturer, false, new[] { "Cara", "Aroha", "Bob" })]        // Ferrari, Mazda, Toyota
    [InlineData(VehicleSortField.YearOfManufacture, false, new[] { "Bob", "Aroha", "Cara" })]   // 2015, 2019, 2022
    [InlineData(VehicleSortField.WeightKg, true, new[] { "Bob", "Aroha", "Cara" })]             // 3000, 1500, 500
    public async Task GetAll_ReturnsTheVehiclesInTheRequestedOrder(VehicleSortField sortBy, bool descending, string[] expectedOwners)
    {
        await service.AddAsync(new VehicleInput("Aroha", MazdaId, 2019, 1500m));
        await service.AddAsync(new VehicleInput("Bob", 5, 2015, 3000m));     // Toyota
        await service.AddAsync(new VehicleInput("Cara", 4, 2022, 500m));     // Ferrari

        var vehicles = await service.GetAllAsync(sortBy, descending);

        Assert.Equal(expectedOwners, vehicles.Select(vehicle => vehicle.OwnerName));
    }
}
