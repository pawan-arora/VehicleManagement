using CreditWorks.VehicleManagement.Shared.Validation;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Services;

/// <summary>What adding a vehicle returns: the saved vehicle, or the errors that stopped it. Nothing is saved unless <see cref="Vehicle"/> is set.</summary>
public sealed record AddVehicleResult(VehicleDetails? Vehicle, List<ValidationError> Errors)
{
    public static AddVehicleResult Saved(VehicleDetails vehicle) => new(vehicle, []);

    public static AddVehicleResult Invalid(List<ValidationError> errors) => new(null, errors);
}
