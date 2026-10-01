using CreditWorks.VehicleManagement.Modules.Vehicles.Services;
using CreditWorks.VehicleManagement.Shared.Categories;

namespace CreditWorks.VehicleManagement.Api.Contracts;

/// <summary>A vehicle as the API returns it (see docs/api-contract.md).</summary>
public sealed record VehicleResponse(
    int Id,
    string OwnerName,
    ManufacturerResponse Manufacturer,
    int YearOfManufacture,
    decimal WeightKg,
    VehicleCategoryResponse Category)
{
    public static VehicleResponse From(VehicleDetails vehicle) => new(
        vehicle.Id,
        vehicle.OwnerName,
        ManufacturerResponse.From(vehicle.Manufacturer),
        vehicle.YearOfManufacture,
        vehicle.WeightKg,
        VehicleCategoryResponse.From(vehicle.Category));
}

/// <summary>A manufacturer as the API returns it.</summary>
public sealed record ManufacturerResponse(int Id, string Name)
{
    public static ManufacturerResponse From(ManufacturerDetails manufacturer) => new(manufacturer.Id, manufacturer.Name);
}

/// <summary>The category shown with each vehicle. Its image is in <c>GET /api/icons</c>, looked up by <see cref="Icon"/>.</summary>
public sealed record VehicleCategoryResponse(int Id, string Name, string Icon)
{
    public static VehicleCategoryResponse From(CategorySummary category) => new(category.Id, category.Name, category.Icon);
}

/// <summary>The body of <c>POST /api/vehicles</c>. Everything is nullable so a missing value can be reported as "required".</summary>
public sealed record CreateVehicleRequest(string? OwnerName, int? ManufacturerId, int? YearOfManufacture, decimal? WeightKg)
{
    public VehicleInput ToInput() => new(OwnerName, ManufacturerId, YearOfManufacture, WeightKg);
}
