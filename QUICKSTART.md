# Quick Start Guide

## 🚀 Running the Application in 5 Minutes

### Option 1: Docker (Fastest - Recommended)

1. **Ensure Docker is running**
   ```powershell
   docker --version
   ```

2. **Navigate to project directory**
   ```powershell
   cd e:\Repo\volue-forecast
   ```

3. **Start the application**
   ```powershell
   docker-compose up --build
   ```

4. **Wait for the message**: `Now listening on: http://[::]:8080`

5. **Test the API**
   ```powershell
   # In a new terminal
   .\test-api.ps1
   ```

6. **Access the application**
   - API: http://localhost:8080
   - OpenAPI: http://localhost:8080/openapi/v1.json

---

### Option 2: Local Development

1. **Start PostgreSQL**
   ```powershell
   docker run -d --name forecast-postgres -e POSTGRES_DB=forecastdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16-alpine
   ```

2. **Run the application**
   ```powershell
   cd src\ForecastService.Api
   dotnet run
   ```

3. **Access the application**
   - HTTPS: https://localhost:7299
   - HTTP: http://localhost:5299

---

## 📋 Pre-seeded Test Data

Use these IDs for testing:

**Company:**
```
ID: 11111111-1111-1111-1111-111111111111
Name: Energy Trading Corp
```

**Power Plants:**
```
Turkey:    22222222-2222-2222-2222-222222222222
Bulgaria:  33333333-3333-3333-3333-333333333333
Spain:     44444444-4444-4444-4444-444444444444
```

---

## 🧪 Quick API Tests

### Create a Forecast
```powershell
$body = @{
    powerPlantId = "22222222-2222-2222-2222-222222222222"
    forecastDateTime = "2026-01-17T12:00:00Z"
    productionMWh = 150.5
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/api/forecasts" -Method Post -Body $body -ContentType "application/json"
```

### Get Company Position
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/companyposition/11111111-1111-1111-1111-111111111111?startDate=2026-01-16T00:00:00Z&endDate=2026-01-18T00:00:00Z"
```

---

## 🛠️ Useful Commands

### Docker
```powershell
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild
docker-compose up --build
```

### Database
```powershell
# Connect to PostgreSQL
docker exec -it forecast-postgres psql -U postgres -d forecastdb

# View tables
\dt

# Query data
SELECT * FROM "Companies";
SELECT * FROM "PowerPlants";
SELECT * FROM "Forecasts";
```

### Build & Run
```powershell
# Build solution
dotnet build

# Run API
cd src\ForecastService.Api
dotnet run

# Create migration
cd src\ForecastService.Api
dotnet ef migrations add MigrationName --project ..\ForecastService.Infrastructure --context ForecastDbContext
```

---

## ✅ Checklist

- [ ] Docker Desktop is installed and running
- [ ] .NET 10 SDK is installed
- [ ] PostgreSQL is accessible (via Docker or local)
- [ ] Port 8080 (or 5299/7299) is available
- [ ] Application builds successfully (`dotnet build`)
- [ ] Docker compose starts successfully
- [ ] API responds to test requests
- [ ] Database migrations are applied
- [ ] Sample data is seeded

---

## 🐛 Troubleshooting

### Port Already in Use
```powershell
# Change port in docker-compose.yml
ports:
  - "8081:8080"  # Use 8081 instead
```

### Database Connection Failed
```powershell
# Check PostgreSQL is running
docker ps | grep postgres

# Restart database
docker restart forecast-postgres
```

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Next Steps

1. ✅ Run the application
2. ✅ Test with `test-api.ps1` or `sample-requests.http`
3. ✅ Review [ARCHITECTURE.md](ARCHITECTURE.md) for system design
4. ✅ Review [DECISION_LOG.md](DECISION_LOG.md) for technology choices
5. ✅ Explore the API endpoints
6. ✅ Check logs for PositionChanged events

---

## 🎯 Interview Demo Script

1. **Show the running application**
   - `docker-compose up`
   - Show logs with PositionChanged events

2. **Demonstrate Create/Update Forecast**
   - Create a forecast → Show 201 Created
   - Update same forecast → Show 200 OK
   - Show event emission in logs

3. **Demonstrate Get Company Position**
   - Create forecasts for all 3 power plants
   - Call position endpoint
   - Show aggregated totals

4. **Show Architecture**
   - Navigate through project structure
   - Explain Clean Architecture layers
   - Show Repository Pattern implementation

5. **Highlight Key Features**
   - Thread-safe (scoped DbContext)
   - Async/await throughout
   - Event-driven architecture
   - Docker ready
   - Production-quality code

---

**Ready to impress! 🚀**
