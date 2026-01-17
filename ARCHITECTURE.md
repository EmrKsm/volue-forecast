# Forecast Service - Architecture Document

## 1. Overview

The Forecast Service is a microservice designed for managing power plant production forecasts in an energy trading platform. It provides RESTful APIs for creating, updating, and retrieving forecast data, as well as calculating company-wide position aggregations across multiple power plants.

## 2. System Architecture

### 2.1 Architectural Pattern

The service follows **Clean Architecture** principles with a clear separation of concerns across four layers:

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│  (Controllers, Request/Response Handling, Middleware)   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│               Application Layer                         │
│    (Business Logic, Services, DTOs, Interfaces)        │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                Domain Layer                             │
│       (Entities, Domain Events, Interfaces)            │
└─────────────────────────────────────────────────────────┘
                     ▲
┌────────────────────┴────────────────────────────────────┐
│            Infrastructure Layer                         │
│  (Data Access, Repositories, Event Publishing, EF Core)│
└─────────────────────────────────────────────────────────┘
```

### 2.2 System Flow Diagram

```mermaid
flowchart TB
    Client[Client Application]
    API[API Gateway / Load Balancer]
    
    subgraph "Forecast Service Container"
        Controller[Controllers Layer]
        Service[Service Layer]
        Repo[Repository Layer]
        Event[Event Publisher]
    end
    
    DB[(PostgreSQL Database)]
    MQ[Message Queue/Event Bus]
    
    Client -->|HTTP/REST| API
    API -->|HTTP/REST| Controller
    Controller -->|Business Logic| Service
    Service -->|Data Access| Repo
    Repo -->|SQL Queries| DB
    Service -->|Publish Events| Event
    Event -.->|PositionChanged Event| MQ
    
    style Controller fill:#e1f5ff
    style Service fill:#fff4e1
    style Repo fill:#e8f5e9
    style DB fill:#f3e5f5
    style Event fill:#ffe0b2
```

### 2.3 Domain Model

```mermaid
erDiagram
    Company ||--o{ PowerPlant : "owns"
    PowerPlant ||--o{ Forecast : "has"
    
    Company {
        guid Id PK
        string Name
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    PowerPlant {
        guid Id PK
        string Name
        string Country
        guid CompanyId FK
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Forecast {
        guid Id PK
        guid PowerPlantId FK
        datetime ForecastDateTime
        decimal ProductionMWh
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
```

## 3. API Endpoints

### 3.1 Forecast Management

#### POST /api/forecasts
Create or update a forecast for a power plant.

**Request Body:**
```json
{
  "powerPlantId": "guid",
  "forecastDateTime": "2026-01-16T12:00:00Z",
  "productionMWh": 150.5
}
```

**Response (201 Created / 200 OK):**
```json
{
  "id": "guid",
  "powerPlantId": "guid",
  "powerPlantName": "Turkey Power Plant",
  "country": "Turkey",
  "forecastDateTime": "2026-01-16T12:00:00Z",
  "productionMWh": 150.5,
  "createdAt": "2026-01-16T10:00:00Z",
  "updatedAt": "2026-01-16T10:00:00Z"
}
```

#### GET /api/forecasts/{id}
Retrieve a specific forecast by ID.

#### GET /api/forecasts/power-plant/{powerPlantId}
Get all forecasts for a power plant within a date range.

**Query Parameters:**
- `startDate`: DateTime
- `endDate`: DateTime

### 3.2 Company Position

#### GET /api/companyposition/{companyId}
Calculate and retrieve the aggregated position for a company.

**Query Parameters:**
- `startDate`: DateTime
- `endDate`: DateTime

**Response:**
```json
{
  "companyId": "guid",
  "companyName": "Energy Trading Corp",
  "startDate": "2026-01-16T00:00:00Z",
  "endDate": "2026-01-17T00:00:00Z",
  "totalPositionMWh": 1250.75,
  "powerPlantPositions": [
    {
      "powerPlantId": "guid",
      "powerPlantName": "Turkey Power Plant",
      "country": "Turkey",
      "totalProductionMWh": 450.25,
      "forecastCount": 24
    }
  ]
}
```

## 4. Data Flow

### 4.1 Create/Update Forecast Flow

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant ForecastService
    participant Repository
    participant Database
    participant EventPublisher
    
    Client->>Controller: POST /api/forecasts
    Controller->>ForecastService: CreateOrUpdateForecastAsync()
    ForecastService->>Repository: GetByPowerPlantAndDateTimeAsync()
    Repository->>Database: Query existing forecast
    Database-->>Repository: Forecast or null
    
    alt Forecast exists
        ForecastService->>Repository: UpdateAsync()
    else New forecast
        ForecastService->>Repository: CreateAsync()
    end
    
    Repository->>Database: Save changes
    Database-->>Repository: Success
    Repository-->>ForecastService: Forecast entity
    ForecastService->>EventPublisher: PublishPositionChangedEventAsync()
    EventPublisher-->>ForecastService: Event published
    ForecastService-->>Controller: ForecastResponse
    Controller-->>Client: 200 OK / 201 Created
```

### 4.2 Get Company Position Flow

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant PositionService
    participant CompanyRepo
    participant PowerPlantRepo
    participant ForecastRepo
    participant Database
    
    Client->>Controller: GET /api/companyposition/{id}
    Controller->>PositionService: GetCompanyPositionAsync()
    PositionService->>CompanyRepo: GetByIdAsync()
    CompanyRepo->>Database: Query company
    Database-->>CompanyRepo: Company
    PositionService->>PowerPlantRepo: GetByCompanyIdAsync()
    PowerPlantRepo->>Database: Query power plants
    Database-->>PowerPlantRepo: List of power plants
    
    loop For each power plant
        PositionService->>ForecastRepo: GetActiveByPowerPlantAsync()
        ForecastRepo->>Database: Query forecasts
        Database-->>ForecastRepo: List of forecasts
    end
    
    PositionService->>PositionService: Calculate totals
    PositionService-->>Controller: CompanyPositionResponse
    Controller-->>Client: 200 OK
```

## 5. Event-Driven Architecture

### 5.1 PositionChanged Event

When a forecast is created or updated, the service emits a `PositionChangedEvent`:

```csharp
{
    "CompanyId": "guid",
    "StartDate": "2026-01-16T00:00:00Z",
    "EndDate": "2026-01-17T00:00:00Z",
    "TotalPositionMWh": 1250.75,
    "EventTimestamp": "2026-01-16T10:30:00Z",
    "Reason": "Forecast Created"
}
```

**Current Implementation:** In-memory event publisher with logging (for demonstration)
**Production Ready:** Can be replaced with RabbitMQ, Azure Service Bus, Apache Kafka, or AWS EventBridge

## 6. Data Persistence

### 6.1 Database Strategy

- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core 10.0
- **Migration Strategy:** Code-First with automatic migrations on startup
- **Connection Pooling:** Managed by Npgsql provider
- **Indexing Strategy:**
  - Composite index on `(PowerPlantId, ForecastDateTime, IsActive)` for fast forecast queries
  - Composite index on `(CompanyId, Country)` for power plant queries

### 6.2 Data Seeding

Initial data includes:
- 1 Company: "Energy Trading Corp"
- 3 Power Plants: Turkey, Bulgaria, Spain

## 7. Thread Safety & Concurrency

### 7.1 Thread Safety Considerations

1. **DbContext Scoping:** Each HTTP request gets its own scoped DbContext instance
2. **Repository Pattern:** Prevents shared state issues
3. **Async/Await:** All I/O operations are async for better scalability
4. **Optimistic Concurrency:** Can be added using row versioning if needed

### 7.2 Scalability

- **Stateless Design:** Service can be horizontally scaled
- **Database Connection Pooling:** Efficient resource utilization
- **Docker Support:** Easy container orchestration with Kubernetes or Docker Swarm

## 8. Security Considerations

### 8.1 Current Implementation
- Input validation in controllers
- Model validation using data annotations
- Exception handling middleware
- CORS configuration for cross-origin requests

### 8.2 Production Recommendations
- Add JWT authentication
- Implement rate limiting
- Add API versioning
- Enable HTTPS only
- Add request logging and monitoring
- Implement API key authentication

## 9. Deployment Architecture

```mermaid
flowchart LR
    LB[Load Balancer]
    
    subgraph "Container Orchestration (Docker/Kubernetes)"
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance N]
    end
    
    DB[(PostgreSQL<br/>Primary)]
    DBR[(PostgreSQL<br/>Replica)]
    
    LB --> API1
    LB --> API2
    LB --> API3
    
    API1 --> DB
    API2 --> DB
    API3 --> DB
    
    DB -.Replication.-> DBR
    
    style LB fill:#ff9800
    style API1 fill:#4caf50
    style API2 fill:#4caf50
    style API3 fill:#4caf50
    style DB fill:#2196f3
    style DBR fill:#03a9f4
```

## 10. Monitoring & Observability

### 10.1 Recommended Metrics
- Request latency (p50, p95, p99)
- Error rates by endpoint
- Database query performance
- Event publishing success rate
- Active connections

### 10.2 Logging
- Structured logging using Serilog (can be added)
- Log levels: Debug, Information, Warning, Error
- Correlation IDs for request tracing

## 11. Technology Stack Summary

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 / ASP.NET Core |
| Language | C# 13 |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 10.0 |
| Containerization | Docker & Docker Compose |
| API Documentation | OpenAPI / Swagger |
| Design Pattern | Clean Architecture, Repository Pattern |

## 12. Future Enhancements

1. **Caching Layer:** Redis for frequently accessed data
2. **Message Queue:** RabbitMQ or Azure Service Bus for event publishing
3. **GraphQL Support:** Alternative to REST API
4. **Real-time Updates:** SignalR for live position updates
5. **Time Series Optimization:** TimescaleDB for historical data
6. **Authentication:** Identity Server or Azure AD B2C
7. **Monitoring:** Application Insights, Prometheus + Grafana
