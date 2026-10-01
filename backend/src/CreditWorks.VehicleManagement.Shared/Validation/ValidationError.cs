namespace CreditWorks.VehicleManagement.Shared.Validation;

/// <summary>
/// A broken rule, as every module reports it. <see cref="Field"/> is the name of the field it's about (such as
/// "WeightKg"), or null for a rule about several records together (such as a gap between categories). The Api turns
/// these into the <c>errors</c> of a 400 response.
/// </summary>
public sealed record ValidationError(string? Field, string Message);
