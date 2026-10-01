using CreditWorks.VehicleManagement.Api.Endpoints;
using CreditWorks.VehicleManagement.Api.Errors;
using CreditWorks.VehicleManagement.Modules.Categories;
using CreditWorks.VehicleManagement.Modules.Vehicles;

var builder = WebApplication.CreateBuilder(args);

// The connection string comes from appsettings.Development.json, user secrets or an environment variable (see README).
var connectionString = builder.Configuration.GetConnectionString("VehicleManagement")
    ?? throw new InvalidOperationException("The connection string 'VehicleManagement' is not configured. See README.md.");

// Errors are returned as RFC 7807 problem details, without stack traces (see docs/api-contract.md).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCategoriesModule(connectionString);
builder.Services.AddVehiclesModule(connectionString);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); // gives empty error responses (such as a 404) a problem details body
app.UseHttpsRedirection();

app.MapCategoryEndpoints();
app.MapVehicleEndpoints();

app.Run();
