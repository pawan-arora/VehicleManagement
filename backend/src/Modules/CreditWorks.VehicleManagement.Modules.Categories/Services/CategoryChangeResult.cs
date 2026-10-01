using CreditWorks.VehicleManagement.Shared.Validation;

namespace CreditWorks.VehicleManagement.Modules.Categories.Services;

/// <summary>
/// What adding, editing or deleting a category returns: all categories after the save, or the errors that stopped it,
/// or that the category doesn't exist. Nothing is saved unless <see cref="Categories"/> is set.
/// </summary>
public sealed record CategoryChangeResult(List<CategoryDetails>? Categories, List<ValidationError> Errors, bool IsNotFound)
{
    public static CategoryChangeResult Saved(List<CategoryDetails> categories) => new(categories, [], false);

    public static CategoryChangeResult Invalid(List<ValidationError> errors) => new(null, errors, false);

    public static CategoryChangeResult NotFound() => new(null, [], true);
}
