using ForecastService.Application.DTOs;
using ForecastService.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ForecastService.Api.Controllers;

/// <summary>
/// Base controller with common functionality for error handling and response mapping
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Creates a standardized error response with appropriate HTTP status code
    /// </summary>
    protected ActionResult<ApiResult<T>> HandleError<T>(Result<T> result)
    {
        var statusCode = MapErrorToStatusCode(result.Error);
        var title = GetErrorTitle(statusCode);

        return StatusCode(statusCode, ApiResult<T>.Fail(
            title,
            result.Error.Message,
            statusCode,
            HttpContext.Request.Path));
    }

    /// <summary>
    /// Maps domain errors to appropriate HTTP status codes
    /// </summary>
    private static int MapErrorToStatusCode(Error error)
    {
        // C# 14: Using ReadOnlySpan<char> for efficient string operations
        ReadOnlySpan<char> code = error.Code.AsSpan();
        
        return error.Code switch
        {
            // Not Found errors -> 404 (using Span<T> for efficient suffix check)
            var c when code.EndsWith(".NotFound".AsSpan()) => StatusCodes.Status404NotFound,

            // Concurrency errors -> 409 Conflict
            var c when code.EndsWith(".ConcurrencyConflict".AsSpan()) => StatusCodes.Status409Conflict,

            // Database errors -> 503 Service Unavailable
            "Database.ConnectionError" => StatusCodes.Status503ServiceUnavailable,
            "Database.TimeoutError" => StatusCodes.Status504GatewayTimeout,

            // Constraint violations -> 409 Conflict
            "Database.UniqueConstraintViolation" => StatusCodes.Status409Conflict,
            "Database.ForeignKeyViolation" => StatusCodes.Status409Conflict,
            "Database.ConstraintViolation" => StatusCodes.Status409Conflict,

            // Database errors -> 500 Internal Server Error
            var c when code.Contains(".DatabaseError".AsSpan(), StringComparison.Ordinal) => StatusCodes.Status500InternalServerError,

            // Validation errors -> 400 Bad Request
            _ => StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// Gets appropriate error title based on status code
    /// </summary>
    private static string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            StatusCodes.Status504GatewayTimeout => "Gateway Timeout",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Validation Error"
        };
    }
}
