using System.Globalization;
using CreditWorks.VehicleManagement.Modules.Categories.Entities;
using CreditWorks.VehicleManagement.Shared.Persistence;
using CreditWorks.VehicleManagement.Shared.Validation;

namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>
/// Every category rule, in one place (see "Category rules" in the README). None of them use the database, so each one
/// is unit-tested directly in CategoryRulesTests.
/// </summary>
public static class CategoryRules
{

    /// <summary>Returns why a vehicle weight is invalid, or null when it's fine.</summary>
    public static string? CheckWeight(decimal? weightKg)
    {
        if (weightKg is null)
        {
            return "Weight is required.";
        }

        if (weightKg <= 0)
        {
            return "Weight must be greater than 0.";
        }

        return CheckWeightFormat(weightKg.Value, "Weight");
    }

    /// <summary>
    /// The boundary rule: a category includes its minimum and excludes its maximum, so exactly 500 kg belongs to the
    /// category that starts at 500. A category with no maximum has no upper limit.
    /// </summary>
    public static bool Contains(Category category, decimal weightKg) =>
        weightKg >= category.MinWeightKg && (category.MaxWeightKg is null || weightKg < category.MaxWeightKg);

    /// <summary>
    /// Returns the one category a weight belongs to. If none or several match, the categories have a gap or an overlap
    /// (which <see cref="CheckAll"/> never lets be saved), so it throws instead of guessing.
    /// </summary>
    public static Category FindForWeight(IEnumerable<Category> categories, decimal weightKg)
    {
        var matches = categories.Where(category => Contains(category, weightKg)).ToList();

        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                $"{matches.Count} categories cover {Kg(weightKg)}; there must be exactly one. The categories have a gap or an overlap.");
        }

        return matches[0];
    }

    /// <summary>Checks a category's own fields (name, icon and weights), without comparing it with other categories.</summary>
    public static List<ValidationError> CheckFields(CategoryInput category, ICollection<string> iconKeys)
    {
        var errors = CheckNameAndIcon(category, iconKeys);
        errors.AddRange(CheckWeights(category));
        return errors;
    }

    /// <summary>Checks a category's name and icon.</summary>
    private static List<ValidationError> CheckNameAndIcon(CategoryInput category, ICollection<string> iconKeys)
    {
        var errors = new List<ValidationError>();

        var name = category.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add(new("Name", "Name is required."));
        }
        else if (name.Length > Category.NameMaxLength)
        {
            errors.Add(new("Name", $"Name must be at most {Category.NameMaxLength} characters."));
        }

        if (string.IsNullOrWhiteSpace(category.Icon))
        {
            errors.Add(new("Icon", "Icon is required."));
        }
        else if (!iconKeys.Contains(category.Icon))
        {
            errors.Add(new("Icon", $"Icon '{category.Icon}' is not one of: {string.Join(", ", iconKeys.Order())}."));
        }

        return errors;
    }

    /// <summary>Checks a category's minimum and maximum weight on their own.</summary>
    private static List<ValidationError> CheckWeights(CategoryInput category)
    {
        var errors = new List<ValidationError>();

        if (category.MinWeightKg is not { } min)
        {
            errors.Add(new("MinWeightKg", "Minimum weight is required."));
        }
        else if (min < 0)
        {
            errors.Add(new("MinWeightKg", "Minimum weight must be 0 or more."));
        }
        else if (CheckWeightFormat(min, "Minimum weight") is { } minError)
        {
            errors.Add(new("MinWeightKg", minError));
        }

        // No maximum is fine: it means no upper limit.
        if (category.MaxWeightKg is { } max)
        {
            if (CheckWeightFormat(max, "Maximum weight") is { } maxError)
            {
                errors.Add(new("MaxWeightKg", maxError));
            }
            else if (category.MinWeightKg is { } minimum && max <= minimum)
            {
                errors.Add(new("MaxWeightKg", "Maximum weight must be greater than the minimum weight."));
            }
        }

        return errors;
    }


    /// <summary>
    /// Checks all the categories together. Sorted by minimum weight, they must start at 0, each maximum must equal the
    /// next minimum (no gaps, no overlaps), and only the heaviest may have no maximum. Every weight then belongs to
    /// exactly one category. Names must be unique too.
    /// </summary>
    public static List<ValidationError> CheckAll(IReadOnlyList<Category> categories)
    {
        if (categories.Count == 0)
        {
            return [new(null, "At least one category is required.")];
        }

        var errors = new List<ValidationError>();

        var duplicateNames = categories
            .GroupBy(category => category.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);
        foreach (var group in duplicateNames)
        {
            errors.Add(new("Name", $"Name '{group.Key}' is used by more than one category."));
        }

        // Lightest first. If two start at the same weight, the one with the lower maximum comes first
        // (no maximum counts as the highest).
        var sorted = categories
            .OrderBy(category => category.MinWeightKg)
            .ThenBy(category => category.MaxWeightKg ?? decimal.MaxValue)
            .ToList();

        if (sorted[0].MinWeightKg != 0)
        {
            errors.Add(new(null, $"The lightest category must start at 0 kg, otherwise weights below {Kg(sorted[0].MinWeightKg)} have no category."));
        }

        for (var i = 0; i < sorted.Count; i++)
        {
            var current = sorted[i];
            var isHeaviest = i == sorted.Count - 1;

            // A neighbour that moved can end up with nothing left, such as Medium becoming 3000 kg to 3000 kg.
            if (current.MaxWeightKg is { } upper && upper <= current.MinWeightKg)
            {
                errors.Add(new(null, $"'{current.Name}' would have no weights left ({Kg(current.MinWeightKg)} to {Kg(upper)})."));
                continue;
            }

            if (current.MaxWeightKg is not { } max)
            {
                if (!isHeaviest)
                {
                    errors.Add(new(null, $"Only the heaviest category can have no upper limit, but '{current.Name}' does."));
                }
            }
            else if (isHeaviest)
            {
                errors.Add(new(null, $"The heaviest category ('{current.Name}') must have no upper limit, otherwise weights of {Kg(max)} and above have no category."));
            }
            else
            {
                var next = sorted[i + 1];
                if (max < next.MinWeightKg)
                {
                    errors.Add(new(null, $"Gap: weights from {Kg(max)} up to {Kg(next.MinWeightKg)} have no category."));
                }
                else if (max > next.MinWeightKg)
                {
                    errors.Add(new(null, $"Overlap: '{current.Name}' and '{next.Name}' both cover {Kg(next.MinWeightKg)} to {Kg(max)}."));
                }
            }
        }

        return errors;
    }

    /// <summary>A weight must fit in the database column: at most 2 decimal places, and no larger than it holds.</summary>
    private static string? CheckWeightFormat(decimal weightKg, string label)
    {
        if (decimal.Round(weightKg, WeightColumn.Scale) != weightKg)
        {
            return $"{label} can have at most {WeightColumn.Scale} decimal places.";
        }

        if (weightKg > WeightColumn.LargestKg)
        {
            return $"{label} must be at most {Kg(WeightColumn.LargestKg)}.";
        }

        return null;
    }

    public static string Kg(decimal weightKg) => weightKg.ToString("0.##", CultureInfo.InvariantCulture) + " kg";
}
