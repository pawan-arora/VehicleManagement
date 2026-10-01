using CreditWorks.VehicleManagement.Modules.Vehicles.Entities;
using CreditWorks.VehicleManagement.Shared.Validation;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Services;

/// <summary>
/// Every vehicle rule, in one place. The weight rules belong to the Categories module (a weight must fit a category),
/// so <see cref="VehicleService"/> asks it through ICategoryResolver. None of these use the database, so each one is
/// unit-tested directly in VehicleRulesTests.
/// </summary>
public static class VehicleRules
{
    /// <summary>
    /// Checks the owner's name, manufacturer and year. <paramref name="currentYear"/> is passed in, rather than read
    /// from the clock here, so the tests can fix it.
    /// </summary>
    public static List<ValidationError> CheckFields(VehicleInput vehicle, int currentYear)
    {
        var errors = new List<ValidationError>();

        var ownerName = vehicle.OwnerName?.Trim();
        if (string.IsNullOrEmpty(ownerName))
        {
            errors.Add(new("OwnerName", "Owner's name is required."));
        }
        else if (ownerName.Length > Vehicle.OwnerNameMaxLength)
        {
            errors.Add(new("OwnerName", $"Owner's name must be at most {Vehicle.OwnerNameMaxLength} characters."));
        }

        if (vehicle.ManufacturerId is null)
        {
            errors.Add(new("ManufacturerId", "Manufacturer is required."));
        }

        var latestYear = currentYear + Vehicle.YearsAheadAllowed;
        if (vehicle.YearOfManufacture is not { } year)
        {
            errors.Add(new("YearOfManufacture", "Year of manufacture is required."));
        }
        else if (year < Vehicle.EarliestYear || year > latestYear)
        {
            errors.Add(new("YearOfManufacture", $"Year of manufacture must be between {Vehicle.EarliestYear} and {latestYear}."));
        }

        return errors;
    }
}
