using CreditWorks.VehicleManagement.Modules.Categories.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.VehicleManagement.Modules.Categories.Data;

public class CategoriesDbContext(DbContextOptions<CategoriesDbContext> options) : DbContext(options)
{
    public const string Schema = "categories";

    /// <summary>First id SQL Server hands out for categories.</summary>
    public const int FirstCategoryId = 1001;

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<CategoryIcon> CategoryIcons => Set<CategoryIcon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryIcon>(icon =>
        {
            icon.ToTable(table => table.HasCheckConstraint("CK_CategoryIcon_Key_NotBlank", "LEN(TRIM([Key])) > 0"));

            icon.HasData(CategoryIconSeed.Icons);
        });

        modelBuilder.Entity<Category>(category =>
        {
            // Column rules live on Category as attributes. The identity seed and check constraints below
            // have no attribute.
            category.Property(c => c.Id).UseIdentityColumn(seed: FirstCategoryId, increment: 1);

            // Only rules that hold for each row on its own. The rules that compare rows (start at 0, no gaps
            // or overlaps, unique names, a single open-ended category) are in CategoryRules: a check
            // constraint can only look at one row, and SQL Server checks constraints after every statement,
            // so it would reject a valid delete that moves a neighbour's boundary.
            category.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Categories_Name_NotBlank", "LEN(TRIM([Name])) > 0");
                table.HasCheckConstraint("CK_Categories_MinWeightKg_NotNegative", "[MinWeightKg] >= 0");
                table.HasCheckConstraint(
                    "CK_Categories_MaxWeightKg_AboveMin", "[MaxWeightKg] IS NULL OR [MaxWeightKg] > [MinWeightKg]");
            });

            // The defaults from the assignment brief. The ids follow on from the identity seed,
            category.HasData(
                new Category { Id = FirstCategoryId, Name = "Light", MinWeightKg = 0, MaxWeightKg = 500, IconId = CategoryIconSeed.MotorcycleId },
                new Category { Id = FirstCategoryId + 1, Name = "Medium", MinWeightKg = 500, MaxWeightKg = 2500, IconId = CategoryIconSeed.CarId },
                new Category { Id = FirstCategoryId + 2, Name = "Heavy", MinWeightKg = 2500, MaxWeightKg = null, IconId = CategoryIconSeed.TruckId });
        });
    }
}
