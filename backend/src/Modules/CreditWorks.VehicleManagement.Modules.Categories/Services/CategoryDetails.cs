namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>A category as <see cref="CategoryService"/> returns it. <see cref="Icon"/> is a key such as "car".</summary>
public sealed record CategoryDetails(
    int Id, string Name, decimal MinWeightKg, decimal? MaxWeightKg, string Icon, string IconSvg);
