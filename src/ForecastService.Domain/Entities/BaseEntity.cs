namespace ForecastService.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    // C# 14: Using 'field' keyword instead of explicit backing fields
    public DateTime CreatedAt
    {
        get;
        set => field = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    // C# 14: Using 'field' keyword for cleaner property implementation
    public DateTime UpdatedAt
    {
        get;
        set => field = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    /// <summary>
    /// Concurrency token for optimistic concurrency control
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
