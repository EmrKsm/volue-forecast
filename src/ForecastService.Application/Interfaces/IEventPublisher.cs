using ForecastService.Domain.Events;

namespace ForecastService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishPositionChangedEventAsync(PositionChangedEvent eventData);
}
