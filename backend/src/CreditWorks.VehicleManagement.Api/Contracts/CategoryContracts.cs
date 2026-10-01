using CreditWorks.VehicleManagement.Modules.Categories.Services;

namespace CreditWorks.VehicleManagement.Api.Contracts;

/// <summary>
/// A category as the API returns it (see docs/api-contract.md). <see cref="IconSvg"/> is sent here, once per
/// category, rather than with every vehicle; clients look it up by <see cref="Icon"/>.
/// </summary>
public sealed record CategoryResponse(
    int Id, string Name, decimal MinWeightKg, decimal? MaxWeightKg, string Icon, string IconSvg)
{
    public static CategoryResponse From(CategoryDetails category) => new(
        category.Id, category.Name, category.MinWeightKg, category.MaxWeightKg, category.Icon, category.IconSvg);
}

/// <summary>An icon as <c>GET /api/icons</c> returns it. A category's <c>icon</c> is set to <see cref="Key"/>.</summary>
public sealed record IconResponse(string Key, string Svg)
{
    public static IconResponse From(IconDetails icon) => new(icon.Key, icon.Svg);
}

/// <summary>
/// The body of <c>POST /api/categories</c>. Everything is nullable so a missing value can be reported as "required";
/// a null <see cref="MaxWeightKg"/> means no upper limit.
/// </summary>
public sealed record CreateCategoryRequest(string? Name, decimal? MinWeightKg, decimal? MaxWeightKg, string? Icon)
{
    public CategoryInput ToInput() => new(null, Name, MinWeightKg, MaxWeightKg, Icon);
}

/// <summary>
/// The body of <c>PUT /api/categories/{id}</c>: the category's name, icon and range. A null maximum means no upper
/// limit. If the range changes, the neighbouring categories move with it.
/// </summary>
public sealed record UpdateCategoryRequest(string? Name, string? Icon, decimal? MinWeightKg, decimal? MaxWeightKg)
{
    public CategoryInput ToInput(int id) => new(id, Name, MinWeightKg, MaxWeightKg, Icon);
}
