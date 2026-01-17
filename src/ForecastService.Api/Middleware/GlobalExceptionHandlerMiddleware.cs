using System.Net;
using System.Text.Json;

namespace ForecastService.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Determine status code and error details based on exception type
        var (statusCode, title, detail) = exception switch
        {
            ArgumentNullException argNullEx => (
                HttpStatusCode.BadRequest,
                "Validation Error",
                argNullEx.Message
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                "Validation Error",
                argEx.Message
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An error occurred while processing your request. Please try again later."
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            data = (object?)null,
            error = new
            {
                title,
                detail,
                status = context.Response.StatusCode,
                instance = context.Request.Path.ToString(),
                timestamp = DateTime.UtcNow.ToString("O")
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
