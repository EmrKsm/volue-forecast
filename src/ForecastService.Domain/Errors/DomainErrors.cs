using ForecastService.Domain.Common;

namespace ForecastService.Domain.Errors;

/// <summary>
/// Domain errors for Forecast operations
/// </summary>
public static class ForecastErrors
{
    public static Error NotFound(Guid id) => new(
        "Forecast.NotFound",
        $"Forecast with ID {id} not found");

    public static Error NegativeProduction => new(
        "Forecast.NegativeProduction",
        "Production value cannot be negative");

    public static Error InvalidDateRange => new(
        "Forecast.InvalidDateRange",
        "Start date must be before or equal to end date");

    public static Error ConcurrencyConflict => new(
        "Forecast.ConcurrencyConflict",
        "The forecast was modified by another user. Please refresh and try again");

    public static Error DatabaseError => new(
        "Forecast.DatabaseError",
        "A database error occurred while processing the forecast");
}

/// <summary>
/// Domain errors for PowerPlant operations
/// </summary>
public static class PowerPlantErrors
{
    public static Error NotFound(Guid id) => new(
        "PowerPlant.NotFound",
        $"Power plant with ID {id} not found");

    public static Error ConcurrencyConflict => new(
        "PowerPlant.ConcurrencyConflict",
        "The power plant was modified by another user. Please refresh and try again");
}

/// <summary>
/// Domain errors for Company operations
/// </summary>
public static class CompanyErrors
{
    public static Error NotFound(Guid id) => new(
        "Company.NotFound",
        $"Company with ID {id} not found");

    public static Error ConcurrencyConflict => new(
        "Company.ConcurrencyConflict",
        "The company was modified by another user. Please refresh and try again");
}

/// <summary>
/// General database errors
/// </summary>
public static class DatabaseErrors
{
    public static Error ConnectionError => new(
        "Database.ConnectionError",
        "Unable to connect to the database. Please try again later");

    public static Error TimeoutError => new(
        "Database.TimeoutError",
        "The database operation timed out. Please try again");

    public static Error ConstraintViolation(string details) => new(
        "Database.ConstraintViolation",
        $"A database constraint was violated: {details}");

    public static Error UniqueConstraintViolation => new(
        "Database.UniqueConstraintViolation",
        "A record with the same unique value already exists");

    public static Error ForeignKeyViolation => new(
        "Database.ForeignKeyViolation",
        "The operation violates a foreign key constraint");
}
