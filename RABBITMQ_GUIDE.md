# RabbitMQ Integration Guide

## Overview

The Forecast Service now publishes events to RabbitMQ when position changes occur. This allows other services to consume these events asynchronously.

## Running with Docker Compose

### Start All Services

```bash
docker-compose up -d
```

This starts:
- **PostgreSQL** on port `5432`
- **RabbitMQ** on port `5672` (AMQP) and `15672` (Management UI)
- **Forecast API** on port `8080`

### Access RabbitMQ Management UI

Open your browser and navigate to:

```
http://localhost:15672
```

**Login Credentials:**
- Username: `admin`
- Password: `admin`

## Viewing Published Messages

### 1. Navigate to Queues Tab

In the RabbitMQ Management UI, click on **Queues** in the top menu.

### 2. Create a Queue to Capture Events

Since we're using a topic exchange, you need to create a queue and bind it to the exchange:

1. Click **Add a new queue**
2. **Name:** `position-events-queue` (or any name you prefer)
3. **Durability:** Durable
4. Click **Add queue**

### 3. Bind Queue to Exchange

1. Click on your newly created queue name
2. Scroll down to **Bindings**
3. In the **Add binding from this queue** section:
   - **From exchange:** `forecast.events`
   - **Routing key:** `position.changed` (or use `#` to capture all)
   - Click **Bind**

### 4. Test the Integration

Send a forecast update using the API:

```bash
curl -X POST http://localhost:8080/api/forecasts \
  -H "Content-Type: application/json" \
  -d '{
    "powerPlantId": "22222222-2222-2222-2222-222222222222",
    "forecastDateTime": "2025-01-20T10:00:00Z",
    "productionMWh": 850.5
  }'
```

### 5. View Messages in Queue

1. Go back to the **Queues** tab
2. Click on your queue name (`position-events-queue`)
3. Scroll down to **Get messages**
4. Set **Messages:** to `1` or more
5. Click **Get Message(s)**

You'll see the published event in JSON format:

```json
{
  "companyId": "11111111-1111-1111-1111-111111111111",
  "totalPositionMWh": 850.5,
  "reason": "Forecast created for power plant Turkey Plant 1",
  "timestamp": "2025-01-20T10:00:00Z"
}
```

## Event Details

### Exchange Configuration

- **Name:** `forecast.events`
- **Type:** Topic
- **Durable:** Yes

### Routing Key

- `position.changed` - All position change events

### Message Format

```json
{
  "companyId": "guid",
  "totalPositionMWh": 1234.56,
  "reason": "string explaining why position changed",
  "timestamp": "ISO 8601 datetime"
}
```

## Configuration

### Local Development (appsettings.json)

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "admin",
    "Password": "admin"
  }
}
```

### Docker Environment Variables

Set in `docker-compose.yml`:

```yaml
environment:
  - RabbitMQ__Host=rabbitmq
  - RabbitMQ__Port=5672
  - RabbitMQ__Username=admin
  - RabbitMQ__Password=admin
```

## Switching Between Event Publishers

### Use RabbitMQ (Default)

In [Program.cs](src/ForecastService.Api/Program.cs):

```csharp
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
```

### Use In-Memory (For Local Testing)

If you don't want to run RabbitMQ locally:

```csharp
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
```

## Monitoring

### RabbitMQ Management UI Features

- **Overview:** Connection stats, message rates
- **Connections:** Active connections from the API
- **Channels:** Communication channels
- **Exchanges:** View the `forecast.events` exchange
- **Queues:** View all queues and their messages

### Health Check

Check if RabbitMQ is healthy:

```bash
docker exec -it forecast-rabbitmq rabbitmq-diagnostics ping
```

## Troubleshooting

### Cannot Connect to RabbitMQ

1. Check if RabbitMQ container is running:
   ```bash
   docker ps | grep rabbitmq
   ```

2. Check RabbitMQ logs:
   ```bash
   docker logs forecast-rabbitmq
   ```

3. Verify connection settings in `appsettings.json`

### Messages Not Appearing

1. Ensure queue is properly bound to exchange with correct routing key
2. Check API logs for publish confirmations
3. Verify RabbitMQ exchange exists in Management UI

### Port Conflicts

If port 5672 or 15672 is already in use, modify `docker-compose.yml`:

```yaml
ports:
  - "5673:5672"   # Changed external port
  - "15673:15672" # Changed external port
```

## Production Considerations

1. **Authentication:** Use strong credentials (not `admin/admin`)
2. **SSL/TLS:** Enable encrypted connections
3. **Clustering:** Set up RabbitMQ cluster for high availability
4. **Monitoring:** Integrate with monitoring tools (Prometheus, Grafana)
5. **Message Persistence:** Already configured as durable
6. **Dead Letter Queues:** Consider adding DLQ for failed message handling
7. **Rate Limiting:** Implement connection rate limiting

## Example Consumer (C#)

Here's a simple example of consuming these events:

```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "admin"
};

var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

// Declare queue and bind to exchange
await channel.QueueDeclareAsync("position-events-queue", durable: true, exclusive: false, autoDelete: false);
await channel.QueueBindAsync("position-events-queue", "forecast.events", "position.changed");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var positionEvent = JsonSerializer.Deserialize<PositionChangedEvent>(message);
    
    Console.WriteLine($"Received: Company {positionEvent.CompanyId}, Position: {positionEvent.TotalPositionMWh} MWh");
    
    await channel.BasicAckAsync(ea.DeliveryTag, false);
};

await channel.BasicConsumeAsync("position-events-queue", autoAck: false, consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
```

## Useful Commands

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f forecast-api
docker-compose logs -f rabbitmq

# Restart RabbitMQ only
docker-compose restart rabbitmq

# Remove all data and start fresh
docker-compose down -v
docker-compose up -d
```
