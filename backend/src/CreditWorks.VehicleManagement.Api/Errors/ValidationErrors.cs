using CreditWorks.VehicleManagement.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CreditWorks.VehicleManagement.Api.Errors;

/// <summary>Turns the modules' validation errors into a 400 response (RFC 7807, see docs/api-contract.md).</summary>
public static class ValidationErrors
{
    /// <summary>The key for errors that aren't about one field, such as a gap between categories.</summary>
    private const string GeneralKey = "General";

    /// <summary>
    /// A 400 whose <c>errors</c> are grouped by field (such as <c>WeightKg</c>). Errors that aren't about one field go
    /// under <paramref name="generalKey"/>.
    /// </summary>
    public static ValidationProblem ToProblem(
        IEnumerable<ValidationError> errors, string generalKey = GeneralKey) =>
        TypedResults.ValidationProblem(errors
            .GroupBy(error => error.Field ?? generalKey)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray()));
}
