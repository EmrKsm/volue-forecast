namespace ForecastService.Application.DTOs;

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ProblemDetails? Error { get; set; }

    public static ApiResult<T> Ok(T data)
    {
        return new ApiResult<T>
        {
            Success = true,
            Data = data,
            Error = null
        };
    }

    public static ApiResult<T> Fail(string title, string detail, int statusCode = 400, string? instance = null)
    {
        return new ApiResult<T>
        {
            Success = false,
            Data = default,
            Error = new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = instance
            }
        };
    }
}

/// <summary>
/// Problem details for error responses
/// </summary>
public class ProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Instance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
