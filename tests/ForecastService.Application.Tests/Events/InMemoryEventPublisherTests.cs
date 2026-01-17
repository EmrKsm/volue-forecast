using ForecastService.Domain.Events;
using ForecastService.Infrastructure.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace ForecastService.Application.Tests.Events;

public class InMemoryEventPublisherTests
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisherTests()
    {
        _logger = Substitute.For<ILogger<InMemoryEventPublisher>>();
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_ShouldStoreEvent()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 150.5m,
            Reason = "Forecast updated",
            EventTimestamp = now
        };

        // Act
        await publisher.PublishPositionChangedEventAsync(eventData);

        // Assert
        var events = publisher.GetEvents();
        events.Count.ShouldBe(1);
        events[0].CompanyId.ShouldBe(eventData.CompanyId);
        events[0].TotalPositionMWh.ShouldBe(eventData.TotalPositionMWh);
        events[0].Reason.ShouldBe(eventData.Reason);
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_WhenCalledMultipleTimes_ShouldStoreAllEvents()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var event1 = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 100,
            Reason = "First update",
            EventTimestamp = now
        };
        var event2 = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 200,
            Reason = "Second update",
            EventTimestamp = now
        };

        // Act
        await publisher.PublishPositionChangedEventAsync(event1);
        await publisher.PublishPositionChangedEventAsync(event2);

        // Assert
        var events = publisher.GetEvents();
        events.Count.ShouldBe(2);
        events[0].CompanyId.ShouldBe(event1.CompanyId);
        events[1].CompanyId.ShouldBe(event2.CompanyId);
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_ShouldLogInformation()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 150.5m,
            Reason = "Forecast updated",
            EventTimestamp = now
        };

        // Act
        await publisher.PublishPositionChangedEventAsync(eventData);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Position Changed Event Published")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 150.5m,
            Reason = "Forecast updated",
            EventTimestamp = now
        };

        // Act
        var task = publisher.PublishPositionChangedEventAsync(eventData);

        // Assert
        task.IsCompleted.ShouldBeTrue();
        await task;
    }

    [Fact]
    public async Task GetEvents_ShouldReturnReadOnlyList()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 150.5m,
            Reason = "Forecast updated",
            EventTimestamp = now
        };
        await publisher.PublishPositionChangedEventAsync(eventData);

        // Act
        var events = publisher.GetEvents();

        // Assert
        events.ShouldBeOfType<System.Collections.ObjectModel.ReadOnlyCollection<PositionChangedEvent>>();
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_WithZeroPosition_ShouldWork()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = 0,
            Reason = "All forecasts removed",
            EventTimestamp = now
        };

        // Act
        await publisher.PublishPositionChangedEventAsync(eventData);

        // Assert
        var events = publisher.GetEvents();
        events.Count.ShouldBe(1);
        events[0].TotalPositionMWh.ShouldBe(0);
    }

    [Fact]
    public async Task PublishPositionChangedEventAsync_WithNegativePosition_ShouldWork()
    {
        // Arrange
        var publisher = new InMemoryEventPublisher(_logger);
        var now = DateTime.UtcNow;
        var eventData = new PositionChangedEvent
        {
            CompanyId = Guid.NewGuid(),
            StartDate = now,
            EndDate = now.AddDays(7),
            TotalPositionMWh = -50.5m,
            Reason = "Adjustment made",
            EventTimestamp = now
        };

        // Act
        await publisher.PublishPositionChangedEventAsync(eventData);

        // Assert
        var events = publisher.GetEvents();
        events.Count.ShouldBe(1);
        events[0].TotalPositionMWh.ShouldBe(-50.5m);
    }
}
