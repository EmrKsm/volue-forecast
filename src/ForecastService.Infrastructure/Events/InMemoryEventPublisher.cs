using ForecastService.Application.Interfaces;
using ForecastService.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ForecastService.Infrastructure.Events;

public class InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger) : IEventPublisher
{
    private readonly List<PositionChangedEvent> _events = [];

    public Task PublishPositionChangedEventAsync(PositionChangedEvent eventData)
    {
        _events.Add(eventData);
        logger.LogInformation(
            "Position Changed Event Published: CompanyId={CompanyId}, TotalPosition={TotalPosition} MWh, Reason={Reason}",
            eventData.CompanyId,
            eventData.TotalPositionMWh,
            eventData.Reason);

        // In a real implementation, this would publish to a message broker like RabbitMQ, Azure Service Bus, or Kafka
        // For now, we're just logging and storing in memory for demonstration
        return Task.CompletedTask;
    }

    // Method to retrieve events for testing/debugging purposes
    public IReadOnlyList<PositionChangedEvent> GetEvents() => _events.AsReadOnly();
}
