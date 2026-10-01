using CreditWorks.VehicleManagement.Modules.Vehicles.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.VehicleManagement.Modules.Vehicles.Data;


public class VehiclesDbContext(DbContextOptions<VehiclesDbContext> options) : DbContext(options)
{
    public const string Schema = "vehicles";

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Column rules live on the entities as attributes. Non-nullable properties already make every column
        // NOT NULL; the check constraints below also reject values that are present but empty or out of range.
        modelBuilder.Entity<Manufacturer>(manufacturer =>
        {
            manufacturer.ToTable(table => table.HasCheckConstraint("CK_Manufacturers_Name_NotBlank", "LEN(TRIM([Name])) > 0"));

            manufacturer.HasData(ManufacturerSeed.Manufacturers);
        });

        modelBuilder.Entity<Vehicle>(vehicle =>
        {
            // The upper year limit (current year + 1) moves every year, so it's checked in code only.
            vehicle.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Vehicles_OwnerName_NotBlank", "LEN(TRIM([OwnerName])) > 0");
                table.HasCheckConstraint("CK_Vehicles_YearOfManufacture_Min", $"[YearOfManufacture] >= {Vehicle.EarliestYear}");
                table.HasCheckConstraint("CK_Vehicles_WeightKg_Positive", "[WeightKg] > 0");
            });
        });
    }
}
