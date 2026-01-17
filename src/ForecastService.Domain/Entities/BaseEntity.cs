namespace ForecastService.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    private DateTime _updatedAt;
    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => _updatedAt = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    /// <summary>
    /// Concurrency token for optimistic concurrency control
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
