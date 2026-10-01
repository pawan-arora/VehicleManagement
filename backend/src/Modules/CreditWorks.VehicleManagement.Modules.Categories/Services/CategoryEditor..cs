using CreditWorks.VehicleManagement.Modules.Categories.Entities;
using CreditWorks.VehicleManagement.Shared.Validation;

namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>
/// What adding, deleting or changing the range of a category does to its neighbours, so the ranges keep touching (see "Changing categories"
/// in the README). Both work on a list in memory; <see cref="CategoryService"/> then checks the rules and saves.
/// Unit-tested in CategoryChangesTests.
/// </summary>
public static class CategoryEditor
{
    /// <summary>
    /// Adds <paramref name="newCategory"/> to the list, shortening the category it's carved out of. It must fit inside
    /// one category and start or end where that one does: adding 3000+ to Heavy (2500+) makes Heavy 2500–3000.
    /// Returns why it can't be added, or null when it was added.
    /// </summary>
    public static ValidationError? Add(List<Category> categories, Category newCategory)
    {
        var min = newCategory.MinWeightKg;
        var max = newCategory.MaxWeightKg;

        // The category the new one starts in.
        var host = CategoryRules.FindForWeight(categories, min);

        var endsPastHost = host.MaxWeightKg is { } hostMax && (max is null || max > hostMax);
        if (endsPastHost)
        {
            return new("MaxWeightKg",
                $"'{newCategory.Name}' overlaps more than one category: it starts in '{host.Name}' and continues past " +
                $"{CategoryRules.Kg(host.MaxWeightKg!.Value)}. A new category must fit inside one existing category.");
        }

        var startsWithHost = min == host.MinWeightKg;
        var endsWithHost = max == host.MaxWeightKg;

        if (startsWithHost && endsWithHost)
        {
            return new("MaxWeightKg",
                $"'{newCategory.Name}' covers all of '{host.Name}', which would leave '{host.Name}' with no weights.");
        }

        if (startsWithHost)
        {
            // The new category takes the bottom of the host.
            host.MinWeightKg = max!.Value;
        }
        else if (endsWithHost)
        {
            // The new category takes the top of the host.
            host.MaxWeightKg = min;
        }
        else
        {
            return new("MaxWeightKg",
                $"'{newCategory.Name}' would split '{host.Name}' in two, leaving weights from " +
                $"{CategoryRules.Kg(max!.Value)} with no category. Start or end it where '{host.Name}' starts or ends.");
        }

        categories.Add(newCategory);
        return null;
    }

    /// <summary>
    /// Gives <paramref name="category"/> a new range, and moves its neighbours so the ranges keep touching: the lighter
    /// neighbour now ends at the new minimum, and the heavier neighbour starts at the new maximum. For example, changing
    /// Medium's maximum from 3000 to 2500 makes the next category start at 2500. CategoryRules.CheckAll then rejects a
    /// change that leaves a neighbour with no weights, or moves the lightest away from 0.
    /// </summary>
    public static void ChangeRange(List<Category> categories, Category category, decimal newMin, decimal? newMax)
    {
        var sorted = categories.OrderBy(c => c.MinWeightKg).ToList();
        var index = sorted.IndexOf(category);

        var hasLighterNeighbour = index > 0;
        if (hasLighterNeighbour)
        {
            sorted[index - 1].MaxWeightKg = newMin;
        }

        var hasHeavierNeighbour = index < sorted.Count - 1;
        if (hasHeavierNeighbour && newMax is not null)
        {
            sorted[index + 1].MinWeightKg = newMax.Value;
        }

        category.MinWeightKg = newMin;
        category.MaxWeightKg = newMax;
    }

    /// <summary>
    /// Removes <paramref name="category"/> from the list. Its heavier neighbour takes over its range (deleting Medium
    /// 500–2500 makes Heavy start at 500); if it's the heaviest, the lighter neighbour loses its upper limit instead.
    /// The last category can't be deleted. Returns why it can't be deleted, or null when it was removed.
    /// </summary>
    public static ValidationError? Delete(List<Category> categories, Category category)
    {
        if (categories.Count == 1)
        {
            return new(null, "The last category can't be deleted: every weight needs a category.");
        }

        var sorted = categories.OrderBy(c => c.MinWeightKg).ToList();
        var index = sorted.IndexOf(category);

        var isHeaviest = index == sorted.Count - 1;
        if (isHeaviest)
        {
            sorted[index - 1].MaxWeightKg = category.MaxWeightKg;
        }
        else
        {
            sorted[index + 1].MinWeightKg = category.MinWeightKg;
        }

        categories.Remove(category);
        return null;
    }
}
