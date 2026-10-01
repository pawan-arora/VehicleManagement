using CreditWorks.VehicleManagement.Modules.Vehicles.Data;
using CreditWorks.VehicleManagement.Modules.Vehicles.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace CreditWorks.VehicleManagement.Modules.Vehicles;

public static class VehiclesModule
{
    /// <summary>Registers the Vehicles module's database context and services. Program.cs calls this.</summary>
    public static IServiceCollection AddVehiclesModule(this IServiceCollection services, string connectionString)
    {
        // EF records the migrations it has run in a history table. Each module keeps EF's usual table
        // (__EFMigrationsHistory) in its own schema, so the two modules' histories stay separate.
        services.AddDbContext<VehiclesDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, VehiclesDbContext.Schema)));

        services.AddScoped<VehicleService>();

        return services;
    }
}
