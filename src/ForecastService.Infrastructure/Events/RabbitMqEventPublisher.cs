using ForecastService.Application.Interfaces;
using ForecastService.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ForecastService.Infrastructure.Events;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName = "forecast.events";
    private readonly string _routingKey = "position.changed";

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var host = configuration["RabbitMQ:Host"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var username = configuration["RabbitMQ:Username"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare a topic exchange
            _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false).GetAwaiter().GetResult();

            _logger.LogInformation(
                "Connected to RabbitMQ at {Host}:{Port}, Exchange: {Exchange}",
                host, port, _exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", host, port);
            throw;
        }
    }

    public async Task PublishPositionChangedEventAsync(PositionChangedEvent eventData)
    {
        try
        {
            var message = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = Guid.NewGuid().ToString(),
                Type = nameof(PositionChangedEvent)
            };

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: _routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published Position Changed Event to RabbitMQ: CompanyId={CompanyId}, TotalPosition={TotalPosition} MWh, Reason={Reason}",
                eventData.CompanyId,
                eventData.TotalPositionMWh,
                eventData.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to RabbitMQ");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
