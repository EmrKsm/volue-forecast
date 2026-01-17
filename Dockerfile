# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ForecastService.Api/ForecastService.Api.csproj", "ForecastService.Api/"]
COPY ["src/ForecastService.Application/ForecastService.Application.csproj", "ForecastService.Application/"]
COPY ["src/ForecastService.Domain/ForecastService.Domain.csproj", "ForecastService.Domain/"]
COPY ["src/ForecastService.Infrastructure/ForecastService.Infrastructure.csproj", "ForecastService.Infrastructure/"]

RUN dotnet restore "ForecastService.Api/ForecastService.Api.csproj"

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/ForecastService.Api"
RUN dotnet build "ForecastService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ForecastService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForecastService.Api.dll"]
