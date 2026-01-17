using ForecastService.Domain.Entities;
using Shouldly;

namespace ForecastService.Application.Tests.Domain;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void CreatedAt_WhenSetWithUtcDateTime_ShouldStoreAsUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var utcTime = DateTime.UtcNow;

        // Act
        entity.CreatedAt = utcTime;

        // Assert
        entity.CreatedAt.ShouldBe(utcTime);
        entity.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void CreatedAt_WhenSetWithLocalDateTime_ShouldConvertToUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var localTime = DateTime.Now;

        // Act
        entity.CreatedAt = localTime;

        // Assert
        entity.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.CreatedAt.ShouldBe(localTime.ToUniversalTime());
    }

    [Fact]
    public void CreatedAt_WhenSetWithUnspecifiedDateTime_ShouldTreatAsUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var unspecifiedTime = new DateTime(2026, 1, 17, 12, 0, 0, DateTimeKind.Unspecified);

        // Act
        entity.CreatedAt = unspecifiedTime;

        // Assert
        entity.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.CreatedAt.Year.ShouldBe(2026);
        entity.CreatedAt.Month.ShouldBe(1);
        entity.CreatedAt.Day.ShouldBe(17);
        entity.CreatedAt.Hour.ShouldBe(12);
    }

    [Fact]
    public void UpdatedAt_WhenSetWithUtcDateTime_ShouldStoreAsUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var utcTime = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = utcTime;

        // Assert
        entity.UpdatedAt.ShouldBe(utcTime);
        entity.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void UpdatedAt_WhenSetWithLocalDateTime_ShouldConvertToUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var localTime = DateTime.Now;

        // Act
        entity.UpdatedAt = localTime;

        // Assert
        entity.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.UpdatedAt.ShouldBe(localTime.ToUniversalTime());
    }

    [Fact]
    public void UpdatedAt_WhenSetWithUnspecifiedDateTime_ShouldTreatAsUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var unspecifiedTime = new DateTime(2026, 1, 17, 15, 30, 0, DateTimeKind.Unspecified);

        // Act
        entity.UpdatedAt = unspecifiedTime;

        // Assert
        entity.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.UpdatedAt.Year.ShouldBe(2026);
        entity.UpdatedAt.Month.ShouldBe(1);
        entity.UpdatedAt.Day.ShouldBe(17);
        entity.UpdatedAt.Hour.ShouldBe(15);
        entity.UpdatedAt.Minute.ShouldBe(30);
    }

    [Fact]
    public void Id_ShouldBeSettableAndGettable()
    {
        // Arrange
        var entity = new TestEntity();
        var id = Guid.NewGuid();

        // Act
        entity.Id = id;

        // Assert
        entity.Id.ShouldBe(id);
    }

    [Fact]
    public void RowVersion_ShouldBeInitializedAsEmptyArray()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.RowVersion.ShouldNotBeNull();
        entity.RowVersion.ShouldBeEmpty();
    }

    [Fact]
    public void RowVersion_ShouldBeSettableAndGettable()
    {
        // Arrange
        var entity = new TestEntity();
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        // Act
        entity.RowVersion = rowVersion;

        // Assert
        entity.RowVersion.ShouldBe(rowVersion);
    }

    [Fact]
    public void BothTimestamps_WhenSetWithDifferentTimeZones_ShouldBothBeUtc()
    {
        // Arrange
        var entity = new TestEntity();
        var localTime = DateTime.Now;
        var unspecifiedTime = new DateTime(2026, 1, 17, 10, 0, 0, DateTimeKind.Unspecified);

        // Act
        entity.CreatedAt = localTime;
        entity.UpdatedAt = unspecifiedTime;

        // Assert
        entity.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
