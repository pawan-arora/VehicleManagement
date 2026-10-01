using CreditWorks.VehicleManagement.Modules.Categories.Data;
using CreditWorks.VehicleManagement.Modules.Categories.Services;
using CreditWorks.VehicleManagement.Shared.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace CreditWorks.VehicleManagement.Modules.Categories;

public static class CategoriesModule
{
    /// <summary>Registers the Categories module's database context and services. Program.cs calls this.</summary>
    public static IServiceCollection AddCategoriesModule(this IServiceCollection services, string connectionString)
    {
        // EF records the migrations it has run in a history table. Each module keeps EF's usual table
        // (__EFMigrationsHistory) in its own schema, so the two modules' histories stay separate.
        services.AddDbContext<CategoriesDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, CategoriesDbContext.Schema)));

        // The Api uses CategoryService; the Vehicles module uses it through the ICategoryResolver interface.
        services.AddScoped<CategoryService>();
        services.AddScoped<ICategoryResolver, CategoryService>();

        return services;
    }
}
