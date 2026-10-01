namespace CreditWorks.VehicleManagement.Modules.Vehicles.Services;

/// <summary>
/// A new vehicle as <see cref="VehicleService"/> takes it. Everything is nullable so a missing value can be reported as
/// "required" instead of silently becoming 0 or "".
/// </summary>
public sealed record VehicleInput(string? OwnerName, int? ManufacturerId, int? YearOfManufacture, decimal? WeightKg);
