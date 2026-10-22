using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NWSDB.API.Data;
using NWSDB.API.HttpClients;
using NWSDB.API.Interfaces;
using NWSDB.API.Middleware;
using NWSDB.API.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<NWSDBDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IWaterUsageService, WaterUsageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ── Bank.API Typed HttpClient ─────────────────────────────────────────────────
// Reads base URL from "BankApi:BaseUrl" in appsettings.
// In Development, SSL certificate validation is disabled so the self-signed
// dev certificate on https://localhost:7001 does not block requests.
var bankApiBaseUrl = builder.Configuration["BankApi:BaseUrl"]
    ?? throw new InvalidOperationException("BankApi:BaseUrl is not configured.");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<IBankApiClient, BankApiClient>(client =>
    {
        client.BaseAddress = new Uri(bankApiBaseUrl);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Accept the self-signed dev certificate — Development only
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}
else
{
    builder.Services.AddHttpClient<IBankApiClient, BankApiClient>(client =>
    {
        client.BaseAddress = new Uri(bankApiBaseUrl);
    });
}

// Configure OpenAPI for Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NWSDBDbContext>();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<NWSDBDbContext>();
    DbInitializer.Initialize(context);
}

// Global Exception Handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health/db");

app.Run();
