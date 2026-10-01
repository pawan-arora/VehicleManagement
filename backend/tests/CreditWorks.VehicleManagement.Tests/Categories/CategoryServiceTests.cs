using CreditWorks.VehicleManagement.Modules.Categories.Data;
using CreditWorks.VehicleManagement.Modules.Categories.Services;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.VehicleManagement.Tests.Categories;

/// <summary>
/// Tests CategoryService, which applies every category rule and saves the result. Each test gets a fresh in-memory
/// database holding the default categories: Light 0–500, Medium 500–2500, Heavy 2500 and above.
/// </summary>
public class CategoryServiceTests
{
    private const int LightId = 1001;
    private const int MediumId = 1002;
    private const int HeavyId = 1003;

    private readonly CategoryService service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<CategoriesDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new CategoriesDbContext(options);
        db.Database.EnsureCreated();   // adds the seeded categories and icons

        service = new CategoryService(db);
    }

    /// <summary>The saved categories as "Name min-max" strings, lightest first.</summary>
    private async Task<string[]> SavedRanges() =>
        (await service.GetAllAsync()).Select(c => $"{c.Name} {c.MinWeightKg:0.##}-{c.MaxWeightKg:0.##}").ToArray();

    private static readonly string[] DefaultRanges = ["Light 0-500", "Medium 500-2500", "Heavy 2500-"];

    // ---- Category determination and boundary values ----

    [Theory]
    [InlineData("0", "Light")]
    [InlineData("499.99", "Light")]
    [InlineData("500.00", "Medium")]     // a boundary belongs to the category that starts there
    [InlineData("2499.99", "Medium")]
    [InlineData("2500", "Heavy")]
    public async Task FindByWeight_ReturnsTheCorrectCategory(string weightKg, string expected)
    {
        var category = await service.FindByWeightAsync(decimal.Parse(weightKg));

        Assert.Equal(expected, category.Name);
    }

    // ---- Vehicle weight rules (the Vehicles module asks for this check) ----

    [Theory]
    [InlineData(null, "Weight is required.")]
    [InlineData("0", "Weight must be greater than 0.")]
    [InlineData("500.001", "Weight can have at most 2 decimal places.")]
    public void CheckWeight_InvalidWeight_SaysWhy(string? weightKg, string expected)
    {
        Assert.Equal(expected, service.CheckWeight(weightKg is null ? null : decimal.Parse(weightKg)));
    }

    [Fact]
    public void CheckWeight_ValidWeight_HasNoError()
    {
        Assert.Null(service.CheckWeight(1850.75m));
    }

    // ---- Adding ----

    [Fact]
    public async Task Add_AtTheTopOfTheHeaviest_SavesIt_AndShortensTheHeaviest()
    {
        var result = await service.AddAsync(new CategoryInput(null, "Super heavy", 3000, null, "bus"));

        Assert.Empty(result.Errors);
        Assert.Equal(["Light 0-500", "Medium 500-2500", "Heavy 2500-3000", "Super heavy 3000-"], await SavedRanges());
    }

    [Fact]
    public async Task Add_InTheMiddleOfACategory_IsRejected_BecauseItWouldLeaveAGap_AndNothingIsSaved()
    {
        var result = await service.AddAsync(new CategoryInput(null, "Light heavy", 3000, 3500, "tractor"));

        Assert.Contains("would split 'Heavy' in two", Assert.Single(result.Errors).Message);
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    [Fact]
    public async Task Add_AcrossTwoCategories_IsRejected_BecauseItWouldOverlap_AndNothingIsSaved()
    {
        var result = await service.AddAsync(new CategoryInput(null, "Big", 2000, 3000, "van"));

        Assert.Contains("overlaps more than one category", Assert.Single(result.Errors).Message);
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    [Fact]
    public async Task Add_WithAnExistingName_IsRejected_AndNothingIsSaved()
    {
        var result = await service.AddAsync(new CategoryInput(null, "heavy", 3000, null, "bus"));

        Assert.Equal("Name", Assert.Single(result.Errors).Field);
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    [Fact]
    public async Task Add_WithoutANameOrIcon_IsRejected_AndNothingIsSaved()
    {
        var result = await service.AddAsync(new CategoryInput(null, "", 3000, null, null));

        Assert.Equal(["Icon", "Name"], result.Errors.Select(error => error.Field).Order());
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    // ---- Editing a range ----

    [Fact]
    public async Task Update_ChangingARange_MovesTheNeighbour_SoVehiclesChangeCategory()
    {
        // The brief's example: Medium now ends at 2000, so Heavy begins at 2000.
        var result = await service.UpdateAsync(MediumId, new CategoryInput(MediumId, "Medium", 500, 2000, "car"));

        Assert.Empty(result.Errors);
        Assert.Equal(["Light 0-500", "Medium 500-2000", "Heavy 2000-"], await SavedRanges());

        // A 2200 kg vehicle was Medium; with the new ranges it's Heavy.
        Assert.Equal("Heavy", (await service.FindByWeightAsync(2200)).Name);
    }

    [Fact]
    public async Task Update_ThatSwallowsANeighbour_IsRejected_AndNothingIsSaved()
    {
        // Light up to 2500 would leave Medium with no weights (2500 to 2500).
        var result = await service.UpdateAsync(LightId, new CategoryInput(LightId, "Light", 0, 2500, "motorcycle"));

        Assert.Contains("'Medium' would have no weights left", Assert.Single(result.Errors).Message);
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    [Fact]
    public async Task Update_MovingTheLightestAwayFromZero_IsRejected_BecauseItWouldLeaveAGap()
    {
        var result = await service.UpdateAsync(LightId, new CategoryInput(LightId, "Light", 100, 500, "motorcycle"));

        Assert.StartsWith("The lightest category must start at 0 kg", Assert.Single(result.Errors).Message);
        Assert.Equal(DefaultRanges, await SavedRanges());
    }

    // ---- Deleting ----

    [Fact]
    public async Task Delete_AMiddleCategory_TheHeavierNeighbourTakesOverItsRange()
    {
        var result = await service.DeleteAsync(MediumId);

        Assert.Empty(result.Errors);
        Assert.Equal(["Light 0-500", "Heavy 500-"], await SavedRanges());
    }

    [Fact]
    public async Task Delete_TheHeaviest_TheLighterNeighbourLosesItsUpperLimit()
    {
        await service.DeleteAsync(HeavyId);

        Assert.Equal(["Light 0-500", "Medium 500-"], await SavedRanges());
    }

    [Fact]
    public async Task Delete_TheLastCategory_IsRejected()
    {
        await service.DeleteAsync(LightId);
        await service.DeleteAsync(MediumId);

        var result = await service.DeleteAsync(HeavyId);

        Assert.Equal("The last category can't be deleted: every weight needs a category.", Assert.Single(result.Errors).Message);
        Assert.Single(await SavedRanges());
    }
}
