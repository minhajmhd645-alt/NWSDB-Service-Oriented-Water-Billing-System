using Bank.API.Data;
using Bank.API.Interfaces;
using Bank.API.Middleware;
using Bank.API.Repositories;
using Bank.API.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Dependency Injection ──────────────────────────────────────────────────────
builder.Services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IBankPaymentService, BankPaymentService>();

// ── OpenAPI (required by Scalar) ──────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── CORS ──────────────────────────────────────────────────────────────────────
var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000", "http://localhost:5173", "http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("BankApiCors", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BankDbContext>("BankDb");

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed Database ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    DbInitializer.Initialize(ctx);
}

// ── Global Exception Handler ──────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── CORS ──────────────────────────────────────────────────────────────────────
app.UseCors("BankApiCors");

// ── OpenAPI + Scalar (available in all environments for demo) ─────────────────
app.MapOpenApi();                   // → /openapi/v1.json
app.MapScalarApiReference();        // → /scalar/v1

// ── HTTPS redirect ────────────────────────────────────────────────────────────
app.UseHttpsRedirection();

// ── Authorization ─────────────────────────────────────────────────────────────
app.UseAuthorization();

// ── Controllers ───────────────────────────────────────────────────────────────
app.MapControllers();

// ── Health Check ──────────────────────────────────────────────────────────────
app.MapHealthChecks("/health");

app.Run();

// Needed for integration test WebApplicationFactory access
public partial class Program { }
