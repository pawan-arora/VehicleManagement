using CreditWorks.VehicleManagement.Api.Contracts;
using CreditWorks.VehicleManagement.Api.Errors;
using CreditWorks.VehicleManagement.Modules.Vehicles.Data;
using CreditWorks.VehicleManagement.Modules.Vehicles.Services;
using CreditWorks.VehicleManagement.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CreditWorks.VehicleManagement.Api.Endpoints;

/// <summary>The vehicle and manufacturer endpoints from docs/api-contract.md.</summary>
public static class VehicleEndpoints
{
    /// <summary>The <c>sortBy</c> values the API accepts, and the field each one sorts by.</summary>
    private static readonly Dictionary<string, VehicleSortField> SortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ownerName"] = VehicleSortField.OwnerName,
        ["manufacturer"] = VehicleSortField.Manufacturer,
        ["yearOfManufacture"] = VehicleSortField.YearOfManufacture,
        ["weightKg"] = VehicleSortField.WeightKg,
    };

    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var vehicles = app.MapGroup("/api/vehicles");

        vehicles.MapGet("/", GetVehiclesAsync);
        vehicles.MapPost("/", AddVehicleAsync);

        // Manufacturers belong to the Vehicles module. Read-only: they're seeded by migration.
        app.MapGet("/api/manufacturers", GetManufacturersAsync);

        return app;
    }

    /// <summary><c>GET /api/vehicles?sortBy=ownerName&amp;sortDirection=asc</c>: all vehicles, sorted, with their current category.</summary>
    private static async Task<Results<Ok<List<VehicleResponse>>, ValidationProblem>> GetVehiclesAsync(
        string? sortBy, string? sortDirection, VehicleService service, CancellationToken cancellationToken)
    {
        // The defaults are ownerName and asc (see docs/api-contract.md).
        sortBy ??= "ownerName";
        sortDirection ??= "asc";

        var errors = new List<ValidationError>();
        if (!SortFields.TryGetValue(sortBy, out var sortField))
        {
            errors.Add(new("SortBy", $"sortBy must be one of: {string.Join(", ", SortFields.Keys)}."));
        }

        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        if (!descending && !sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new("SortDirection", "sortDirection must be asc or desc."));
        }

        if (errors.Count > 0)
        {
            return ValidationErrors.ToProblem(errors);
        }

        var vehicles = await service.GetAllAsync(sortField, descending, cancellationToken);

        return TypedResults.Ok(vehicles.Select(VehicleResponse.From).ToList());
    }

    /// <summary><c>POST /api/vehicles</c>: adds a vehicle.</summary>
    private static async Task<Results<Created<VehicleResponse>, ValidationProblem>> AddVehicleAsync(
        CreateVehicleRequest vehicle, VehicleService service, CancellationToken cancellationToken)
    {
        var result = await service.AddAsync(vehicle.ToInput(), cancellationToken);

        if (result.Vehicle is null)
        {
            return ValidationErrors.ToProblem(result.Errors);
        }

        return TypedResults.Created("/api/vehicles", VehicleResponse.From(result.Vehicle));
    }

    /// <summary><c>GET /api/manufacturers</c>: the manufacturers a vehicle can have.</summary>
    private static async Task<Ok<List<ManufacturerResponse>>> GetManufacturersAsync(
        VehicleService service, CancellationToken cancellationToken)
    {
        var manufacturers = await service.GetManufacturersAsync(cancellationToken);

        return TypedResults.Ok(manufacturers.Select(ManufacturerResponse.From).ToList());
    }
}
