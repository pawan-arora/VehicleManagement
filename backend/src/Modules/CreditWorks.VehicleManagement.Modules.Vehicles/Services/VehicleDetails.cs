using CreditWorks.VehicleManagement.Shared.Categories;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Services;

/// <summary>
/// A vehicle as <see cref="VehicleService"/> returns it. <see cref="Category"/> is worked out from the weight each time
/// the vehicle is read, never stored, so it always matches the current categories.
/// </summary>
public sealed record VehicleDetails(
    int Id, string OwnerName, ManufacturerDetails Manufacturer, int YearOfManufacture, decimal WeightKg, CategorySummary Category);

/// <summary>A manufacturer as <see cref="VehicleService"/> returns it.</summary>
public sealed record ManufacturerDetails(int Id, string Name);
