using CreditWorks.VehicleManagement.Modules.Categories.Data;
using CreditWorks.VehicleManagement.Modules.Categories.Entities;
using CreditWorks.VehicleManagement.Shared.Categories;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>
/// Reads and changes categories. Adding, editing and deleting all end the same way: check every rule on the changed
/// categories (CategoryRules.CheckAll), and save only if they pass. It also answers the Vehicles module's questions
/// through ICategoryResolver.
/// </summary>
public sealed class CategoryService(CategoriesDbContext db) : ICategoryResolver
{
    /// <summary>All categories, lightest first.</summary>
    public async Task<List<CategoryDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await LoadCategoriesWithIconsAsync(cancellationToken);

        return categories.OrderBy(category => category.MinWeightKg).Select(ToDetails).ToList();
    }

    /// <summary>Every icon a category can use. Icons are seeded by migration (Data/CategoryIconSeed.cs).</summary>
    public async Task<List<IconDetails>> GetIconsAsync(CancellationToken cancellationToken = default)
    {
        var icons = await db.CategoryIcons.AsNoTracking().OrderBy(icon => icon.Key).ToListAsync(cancellationToken);

        return icons.Select(icon => new IconDetails(icon.Key, icon.Svg)).ToList();
    }

    /// <summary>The category a weight belongs to. Check the weight first with CategoryRules.CheckWeight.</summary>
    public async Task<CategoryDetails> FindByWeightAsync(decimal weightKg, CancellationToken cancellationToken = default)
    {
        var categories = await LoadCategoriesWithIconsAsync(cancellationToken);

        var category = CategoryRules.FindForWeight(categories, weightKg);
        return ToDetails(category);
    }

    /// <summary>Adds a category. The category it's carved out of gets shorter to make room.</summary>
    public async Task<CategoryChangeResult> AddAsync(CategoryInput input, CancellationToken cancellationToken = default)
    {
        // 1. Check the new category's own fields: name, icon and weights.
        var icons = await db.CategoryIcons.ToListAsync(cancellationToken);
        var errors = CategoryRules.CheckFields(input, IconKeys(icons));
        if (errors.Count > 0)
        {
            return CategoryChangeResult.Invalid(errors);
        }

        // 2. Make room for it by shortening the category it's carved out of.
        var categories = await db.Categories.ToListAsync(cancellationToken);
        var newCategory = new Category
        {
            Name = input.Name!.Trim(),
            MinWeightKg = input.MinWeightKg!.Value,
            MaxWeightKg = input.MaxWeightKg,
            IconId = IconId(icons, input.Icon!),
        };

        var error = CategoryEditor.Add(categories, newCategory);
        if (error is not null)
        {
            return CategoryChangeResult.Invalid([error]);
        }

        db.Categories.Add(newCategory);

        // 3. Check every rule on the result, and save.
        return await SaveIfValidAsync(categories, cancellationToken);
    }

    /// <summary>Changes a category's name, icon and weight range. Its neighbours move so the ranges keep touching.</summary>
    public async Task<CategoryChangeResult> UpdateAsync(int id, CategoryInput input, CancellationToken cancellationToken = default)
    {
        // 1. Find the category.
        var categories = await db.Categories.ToListAsync(cancellationToken);
        var category = categories.Find(c => c.Id == id);
        if (category is null)
        {
            return CategoryChangeResult.NotFound();
        }

        // 2. Check the new values on their own: name, icon and weights.
        var icons = await db.CategoryIcons.ToListAsync(cancellationToken);
        var errors = CategoryRules.CheckFields(input, IconKeys(icons));
        if (errors.Count > 0)
        {
            return CategoryChangeResult.Invalid(errors);
        }

        // 3. Make the change. If the range changed, the neighbours move with it.
        category.Name = input.Name!.Trim();
        category.IconId = IconId(icons, input.Icon!);
        CategoryEditor.ChangeRange(categories, category, input.MinWeightKg!.Value, input.MaxWeightKg);

        // 4. Check every rule on the result (no gaps, no overlaps, unique names), and save.
        return await SaveIfValidAsync(categories, cancellationToken);
    }

    /// <summary>Deletes a category. A neighbour takes over its weight range.</summary>
    public async Task<CategoryChangeResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // 1. Find the category.
        var categories = await db.Categories.ToListAsync(cancellationToken);
        var category = categories.Find(c => c.Id == id);
        if (category is null)
        {
            return CategoryChangeResult.NotFound();
        }

        // 2. Give its range to a neighbour, and remove it.
        var error = CategoryEditor.Delete(categories, category);
        if (error is not null)
        {
            return CategoryChangeResult.Invalid([error]);
        }

        db.Categories.Remove(category);

        // 3. Check every rule on the result, and save.
        return await SaveIfValidAsync(categories, cancellationToken);
    }

    /// <summary>For the Vehicles module: why a vehicle weight is invalid, or null when it's fine.</summary>
    public string? CheckWeight(decimal? weightKg) => CategoryRules.CheckWeight(weightKg);

    /// <summary>For the Vehicles module: the category of each weight, with one database query for all of them.</summary>
    public async Task<IReadOnlyDictionary<decimal, CategorySummary>> GetForWeightsAsync(
        IEnumerable<decimal> weightsKg, CancellationToken cancellationToken = default)
    {
        var categories = await LoadCategoriesWithIconsAsync(cancellationToken);

        var result = new Dictionary<decimal, CategorySummary>();
        foreach (var weightKg in weightsKg)
        {
            var category = CategoryRules.FindForWeight(categories, weightKg);
            result[weightKg] = new CategorySummary(category.Id, category.Name, category.Icon.Key);
        }

        return result;
    }

    /// <summary>
    /// Checks every rule on the changed categories, and saves only if they all pass. SaveChangesAsync saves all the
    /// changes together (the new or deleted category and the neighbour that moved), or none of them.
    /// </summary>
    private async Task<CategoryChangeResult> SaveIfValidAsync(List<Category> categories, CancellationToken cancellationToken)
    {
        var errors = CategoryRules.CheckAll(categories);
        if (errors.Count > 0)
        {
            return CategoryChangeResult.Invalid(errors);
        }

        await db.SaveChangesAsync(cancellationToken);

        return CategoryChangeResult.Saved(await GetAllAsync(cancellationToken));
    }

    /// <summary>All categories with their icons, read-only. There are only a handful, so they're loaded together.</summary>
    private Task<List<Category>> LoadCategoriesWithIconsAsync(CancellationToken cancellationToken) =>
        db.Categories.AsNoTracking().Include(category => category.Icon).ToListAsync(cancellationToken);

    private static List<string> IconKeys(List<CategoryIcon> icons) => icons.Select(icon => icon.Key).ToList();

    private static int IconId(List<CategoryIcon> icons, string key) => icons.First(icon => icon.Key == key).Id;

    private static CategoryDetails ToDetails(Category category) => new(
        category.Id,
        category.Name,
        category.MinWeightKg,
        category.MaxWeightKg,
        category.Icon.Key,
        category.Icon.Svg);
}
