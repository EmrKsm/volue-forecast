using ForecastService.Api.Middleware;
using ForecastService.Application.Interfaces;
using ForecastService.Application.Services;
using ForecastService.Domain.Interfaces;
using ForecastService.Infrastructure.Data;
using ForecastService.Infrastructure.Events;
using ForecastService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

// Configure Serilog early to capture startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "../logs/forecast-service-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760, // 10 MB
        rollOnFileSizeLimit: true)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Forecast Service API");

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "../logs/forecast-service-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760, // 10 MB
        rollOnFileSizeLimit: true));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=forecastdb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ForecastDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    // Configure timestamp behavior for PostgreSQL
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);
});

// Register Repositories
builder.Services.AddScoped<IForecastRepository, ForecastRepository>();
builder.Services.AddScoped<IPowerPlantRepository, PowerPlantRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

// Register Services
builder.Services.AddScoped<IForecastService, ForecastService.Application.Services.ForecastService>();
builder.Services.AddScoped<ICompanyPositionService, CompanyPositionService>();

// Register Event Publisher - RabbitMQ (use InMemoryEventPublisher for local testing without RabbitMQ)
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// Configure CORS (optional, for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ForecastDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Global exception handler (replaces UseDeveloperExceptionPage)
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});

app.UseAuthorization();
app.MapControllers();

Log.Information("Forecast Service API started successfully");
app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
