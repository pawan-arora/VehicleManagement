namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>
/// A category as <see cref="CategoryService"/> takes it when adding or editing. Everything is nullable so a missing
/// value can be reported as "required" instead of silently becoming 0 or "".
/// </summary>
public sealed record CategoryInput(
    int? Id, string? Name, decimal? MinWeightKg, decimal? MaxWeightKg, string? Icon);
