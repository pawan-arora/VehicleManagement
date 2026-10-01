using CreditWorks.VehicleManagement.Modules.Vehicles.Entities;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Data;

/// <summary>
/// The manufacturers the database starts with, from the assignment brief. Adding one later means adding it here
/// and creating a migration; no other code refers to a particular manufacturer.
/// </summary>
internal static class ManufacturerSeed
{
    public static readonly Manufacturer[] Manufacturers =
    [
        new() { Id = 1, Name = "Mazda" },
        new() { Id = 2, Name = "Mercedes" },
        new() { Id = 3, Name = "Honda" },
        new() { Id = 4, Name = "Ferrari" },
        new() { Id = 5, Name = "Toyota" },
    ];
}
