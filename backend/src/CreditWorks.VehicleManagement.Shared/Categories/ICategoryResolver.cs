namespace CreditWorks.VehicleManagement.Shared.Categories;

/// <summary>
/// What other modules (such as Vehicles) can ask the Categories module. The Categories module implements it, so they
/// never reference that module directly.
/// </summary>
public interface ICategoryResolver
{
    /// <summary>
    /// Returns why <paramref name="weightKg"/> isn't a valid vehicle weight (missing, not above 0, more than 2
    /// decimal places, or too large), or null when it is. The Categories module owns the weight rules.
    /// </summary>
    string? CheckWeight(decimal? weightKg);

    /// <summary>
    /// Returns the category each weight belongs to, in one database query. A category includes its minimum and
    /// excludes its maximum, so exactly 500.00 kg belongs to the category that starts at 500. Check each weight with
    /// <see cref="CheckWeight"/> first.
    /// </summary>
    Task<IReadOnlyDictionary<decimal, CategorySummary>> GetForWeightsAsync(
        IEnumerable<decimal> weightsKg, CancellationToken cancellationToken = default);
}

/// <summary>The category details other modules need. <see cref="Icon"/> is a key such as "car" (see docs/api-contract.md).</summary>
public sealed record CategorySummary(int Id, string Name, string Icon);
