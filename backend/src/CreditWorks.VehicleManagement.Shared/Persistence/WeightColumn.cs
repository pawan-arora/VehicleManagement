namespace CreditWorks.VehicleManagement.Shared.Persistence;

/// <summary>
/// The format of every weight column: <c>decimal(10, 2)</c>, so 10 digits, 2 of them after the decimal point.
/// Vehicle weights and category boundaries use the same format, because every vehicle weight has to fit a category.
/// Defined once here so the two modules' columns can't differ. Changing it needs a migration in both modules.
/// </summary>
public static class WeightColumn
{
    public const int Precision = 10;

    public const int Scale = 2;

    /// <summary>
    /// The largest weight a decimal(10, 2) column can hold: 8 digits before the point and 2 after. (Not decimal.MaxValue:
    /// that's the largest number C# can hold, far more than the database column can store.)
    /// </summary>
    public const decimal LargestKg = 99999999.99m;
}
