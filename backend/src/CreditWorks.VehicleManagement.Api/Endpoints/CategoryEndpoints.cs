using CreditWorks.VehicleManagement.Api.Contracts;
using CreditWorks.VehicleManagement.Api.Errors;
using CreditWorks.VehicleManagement.Modules.Categories.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CreditWorks.VehicleManagement.Api.Endpoints;

/// <summary>
/// The category endpoints from docs/api-contract.md. Adding or deleting a category can move a neighbour's boundary,
/// so every change returns the full list.
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>The error key for rules about all categories together, such as a gap (see docs/api-contract.md).</summary>
    private const string CategoriesKey = "Categories";

    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/api/categories");

        categories.MapGet("/all", GetCategoriesAsync);
        categories.MapGet("/findByWeight", FindCategoryByWeightAsync);
        categories.MapPost("/", AddCategoryAsync);
        categories.MapPut("/{id:int}", UpdateCategoryAsync);
        categories.MapDelete("/{id:int}", DeleteCategoryAsync);

        // The icons a category can be given. Read-only: icons are seeded by migration (see README, "Icons").
        app.MapGet("/api/icons", GetIconsAsync);

        return app;
    }

    private static async Task<Ok<List<IconResponse>>> GetIconsAsync(CategoryService service, CancellationToken cancellationToken)
    {
        var icons = await service.GetIconsAsync(cancellationToken);

        return TypedResults.Ok(icons.Select(IconResponse.From).ToList());
    }

    private static async Task<Ok<List<CategoryResponse>>> GetCategoriesAsync(
        CategoryService service, CancellationToken cancellationToken)
    {
        var categories = await service.GetAllAsync(cancellationToken);

        return TypedResults.Ok(categories.Select(CategoryResponse.From).ToList());
    }

    /// <summary><c>GET /api/categories/findByWeight?weightKg=500</c>: the category a vehicle of that weight belongs to.</summary>
    private static async Task<Results<Ok<CategoryResponse>, ValidationProblem>> FindCategoryByWeightAsync(
        decimal? weightKg, CategoryService service, CancellationToken cancellationToken)
    {
        if (CategoryRules.CheckWeight(weightKg) is { } error)
        {
            return ValidationErrors.ToProblem([new("WeightKg", error)]);
        }

        var category = await service.FindByWeightAsync(weightKg!.Value, cancellationToken);

        return TypedResults.Ok(CategoryResponse.From(category));
    }

    /// <summary><c>POST /api/categories</c>: adds a category, shortening the one it's carved out of.</summary>
    private static async Task<Results<Created<List<CategoryResponse>>, ValidationProblem>> AddCategoryAsync(
        CreateCategoryRequest category, CategoryService service, CancellationToken cancellationToken)
    {
        var result = await service.AddAsync(category.ToInput(), cancellationToken);

        if (result.Categories is null)
        {
            return ValidationErrors.ToProblem(result.Errors, CategoriesKey);
        }

        return TypedResults.Created("/api/categories/all", result.Categories.Select(CategoryResponse.From).ToList());
    }

    /// <summary><c>PUT /api/categories/{id}</c>: changes a category's name and icon.</summary>
    private static async Task<Results<Ok<List<CategoryResponse>>, ValidationProblem, NotFound>> UpdateCategoryAsync(
        int id, UpdateCategoryRequest changes, CategoryService service, CancellationToken cancellationToken) =>
        ToHttpResult(await service.UpdateAsync(id, changes.ToInput(id), cancellationToken));

    /// <summary><c>DELETE /api/categories/{id}</c>: deletes a category; a neighbour takes over its range.</summary>
    private static async Task<Results<Ok<List<CategoryResponse>>, ValidationProblem, NotFound>> DeleteCategoryAsync(
        int id, CategoryService service, CancellationToken cancellationToken) =>
        ToHttpResult(await service.DeleteAsync(id, cancellationToken));

    private static Results<Ok<List<CategoryResponse>>, ValidationProblem, NotFound> ToHttpResult(CategoryChangeResult result)
    {
        if (result.IsNotFound)
        {
            return TypedResults.NotFound();
        }

        if (result.Categories is null)
        {
            return ValidationErrors.ToProblem(result.Errors, CategoriesKey);
        }

        return TypedResults.Ok(result.Categories.Select(CategoryResponse.From).ToList());
    }
}
