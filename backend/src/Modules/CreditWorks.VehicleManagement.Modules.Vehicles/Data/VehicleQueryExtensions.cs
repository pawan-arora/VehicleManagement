using CreditWorks.VehicleManagement.Modules.Vehicles.Entities;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Data;

/// <summary>The fields the vehicle list can be sorted by.</summary>
public enum VehicleSortField
{
    OwnerName,
    Manufacturer,
    YearOfManufacture,
    WeightKg,
}

/// <summary>
/// Sorting for vehicle queries, written as an extension method so it reads like EF's own methods:
/// <c>db.Vehicles.SortBy(VehicleSortField.WeightKg, descending: true)</c>. EF turns it into SQL ORDER BY.
/// </summary>
public static class VehicleQueryExtensions
{
    /// <summary>
    /// Sorts by the chosen field, then by id, so vehicles with the same value (two owners called John Smith) always
    /// come back in the same order.
    /// </summary>
    public static IQueryable<Vehicle> SortBy(this IQueryable<Vehicle> vehicles, VehicleSortField sortBy, bool descending)
    {
        IOrderedQueryable<Vehicle> sorted;

        if (sortBy == VehicleSortField.Manufacturer)
        {
            sorted = descending ? vehicles.OrderByDescending(v => v.Manufacturer.Name) : vehicles.OrderBy(v => v.Manufacturer.Name);
        }
        else if (sortBy == VehicleSortField.YearOfManufacture)
        {
            sorted = descending ? vehicles.OrderByDescending(v => v.YearOfManufacture) : vehicles.OrderBy(v => v.YearOfManufacture);
        }
        else if (sortBy == VehicleSortField.WeightKg)
        {
            sorted = descending ? vehicles.OrderByDescending(v => v.WeightKg) : vehicles.OrderBy(v => v.WeightKg);
        }
        else
        {
            sorted = descending ? vehicles.OrderByDescending(v => v.OwnerName) : vehicles.OrderBy(v => v.OwnerName);
        }

        return sorted.ThenBy(v => v.Id);
    }
}
