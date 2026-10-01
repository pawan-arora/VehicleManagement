using CreditWorks.VehicleManagement.Modules.Vehicles.Data;
using CreditWorks.VehicleManagement.Modules.Vehicles.Entities;
using CreditWorks.VehicleManagement.Shared.Categories;
using CreditWorks.VehicleManagement.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Services;

/// <summary>
/// Reads and adds vehicles, and lists manufacturers. A vehicle's category is never stored: it's asked from the
/// Categories module (through <see cref="ICategoryResolver"/>) every time vehicles are read.
/// </summary>
public sealed class VehicleService(VehiclesDbContext db, ICategoryResolver categories)
{
    /// <summary>All manufacturers, in name order.</summary>
    public Task<List<ManufacturerDetails>> GetManufacturersAsync(CancellationToken cancellationToken = default) =>
        db.Manufacturers
            .AsNoTracking()
            .OrderBy(manufacturer => manufacturer.Name)
            .Select(manufacturer => new ManufacturerDetails(manufacturer.Id, manufacturer.Name))
            .ToListAsync(cancellationToken);

    /// <summary>All vehicles, sorted, each with the category its weight belongs to now.</summary>
    public async Task<List<VehicleDetails>> GetAllAsync(
        VehicleSortField sortBy, bool descending, CancellationToken cancellationToken = default)
    {
        var vehicles = await db.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Manufacturer)
            .SortBy(sortBy, descending)
            .ToListAsync(cancellationToken);

        // One question to the Categories module for all the weights, rather than one per vehicle.
        var categoriesByWeight = await categories.GetForWeightsAsync(vehicles.Select(vehicle => vehicle.WeightKg), cancellationToken);

        return vehicles.Select(vehicle => ToDetails(vehicle, categoriesByWeight[vehicle.WeightKg])).ToList();
    }

    /// <summary>Checks every rule, then saves the vehicle. Nothing is saved if a rule is broken.</summary>
    public async Task<AddVehicleResult> AddAsync(VehicleInput input, CancellationToken cancellationToken = default)
    {
        var errors = VehicleRules.CheckFields(input, DateTime.Today.Year);

        if (categories.CheckWeight(input.WeightKg) is { } weightError)
        {
            errors.Add(new ValidationError("WeightKg", weightError));
        }

        var manufacturer = input.ManufacturerId is { } manufacturerId
            ? await db.Manufacturers.FindAsync([manufacturerId], cancellationToken)
            : null;
        if (input.ManufacturerId is not null && manufacturer is null)
        {
            errors.Add(new ValidationError("ManufacturerId", $"Manufacturer {input.ManufacturerId} does not exist."));
        }

        if (errors.Count > 0)
        {
            return AddVehicleResult.Invalid(errors);
        }

        var vehicle = new Vehicle
        {
            OwnerName = input.OwnerName!.Trim(),
            Manufacturer = manufacturer!,
            YearOfManufacture = input.YearOfManufacture!.Value,
            WeightKg = input.WeightKg!.Value,
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);

        var categoriesByWeight = await categories.GetForWeightsAsync([vehicle.WeightKg], cancellationToken);
        return AddVehicleResult.Saved(ToDetails(vehicle, categoriesByWeight[vehicle.WeightKg]));
    }

    private static VehicleDetails ToDetails(Vehicle vehicle, CategorySummary category) => new(
        vehicle.Id,
        vehicle.OwnerName,
        new ManufacturerDetails(vehicle.Manufacturer.Id, vehicle.Manufacturer.Name),
        vehicle.YearOfManufacture,
        vehicle.WeightKg,
        category);
}
