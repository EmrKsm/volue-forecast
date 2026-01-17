using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using ForecastService.Application.Services;
using ForecastService.Domain.Entities;
using ForecastService.Domain.Events;
using ForecastService.Domain.Interfaces;
using Shouldly;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ForecastService.Application.Tests.Services;

public class ForecastServiceTests
{
    private readonly IForecastRepository _forecastRepository;
    private readonly IPowerPlantRepository _powerPlantRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly Application.Services.ForecastService _sut;

    public ForecastServiceTests()
    {
        _forecastRepository = Substitute.For<IForecastRepository>();
        _powerPlantRepository = Substitute.For<IPowerPlantRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _sut = new Application.Services.ForecastService(
            _forecastRepository,
            _powerPlantRepository,
            _eventPublisher);
    }

    [Fact]
    public async Task CreateOrUpdateForecastAsync_WhenPowerPlantNotFound_ReturnsFailure()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            PowerPlantId = Guid.NewGuid(),
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 100.5m
        };

        _powerPlantRepository.GetByIdAsync(request.PowerPlantId!.Value).ReturnsNull();

        // Act
        var result = await _sut.CreateOrUpdateForecastAsync(request);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("PowerPlant.NotFound");
        await _powerPlantRepository.Received(1).GetByIdAsync(request.PowerPlantId!.Value);
    }

    [Fact]
    public async Task CreateOrUpdateForecastAsync_WhenForecastDoesNotExist_CreatesNewForecast()
    {
        // Arrange
        var powerPlant = new PowerPlant
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            Country = "Test Country",
            CompanyId = Guid.NewGuid()
        };

        var request = new CreateOrUpdateForecastRequest
        {
            PowerPlantId = powerPlant.Id,
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 100.5m
        };

        var createdForecast = new Forecast
        {
            Id = Guid.NewGuid(),
            PowerPlantId = powerPlant.Id,
            PowerPlant = powerPlant,
            ForecastDateTime = request.ForecastDateTime!.Value,
            ProductionMWh = request.ProductionMWh!.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _powerPlantRepository.GetByIdAsync(request.PowerPlantId!.Value).Returns(powerPlant);
        _forecastRepository.GetByPowerPlantAndDateTimeAsync(request.PowerPlantId!.Value, request.ForecastDateTime!.Value).ReturnsNull();
        _forecastRepository.CreateAsync(Arg.Any<Forecast>()).Returns(createdForecast);

        // Act
        var result = await _sut.CreateOrUpdateForecastAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PowerPlantId.ShouldBe(powerPlant.Id);
        result.Value.ProductionMWh.ShouldBe(request.ProductionMWh!.Value);

        await _forecastRepository.Received(1).CreateAsync(Arg.Any<Forecast>());
        await _eventPublisher.Received(1).PublishPositionChangedEventAsync(Arg.Any<PositionChangedEvent>());
    }

    [Fact]
    public async Task GetForecastAsync_WhenForecastExists_ReturnsSuccess()
    {
        // Arrange
        var forecastId = Guid.NewGuid();
        var powerPlant = new PowerPlant
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            Country = "Test Country",
            CompanyId = Guid.NewGuid()
        };

        var forecast = new Forecast
        {
            Id = forecastId,
            PowerPlantId = powerPlant.Id,
            PowerPlant = powerPlant,
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 100.5m
        };

        _forecastRepository.GetByIdAsync(forecastId).Returns(forecast);
        _powerPlantRepository.GetByIdAsync(powerPlant.Id).Returns(powerPlant);

        // Act
        var result = await _sut.GetForecastAsync(forecastId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(forecastId);
        result.Value.PowerPlantName.ShouldBe("Test Plant");
        result.Value.Country.ShouldBe("Test Country");
    }

    [Fact]
    public async Task GetForecastAsync_WhenForecastNotFound_ReturnsFailure()
    {
        // Arrange
        var forecastId = Guid.NewGuid();
        _forecastRepository.GetByIdAsync(forecastId).ReturnsNull();

        // Act
        var result = await _sut.GetForecastAsync(forecastId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("Forecast.NotFound");
    }

    [Fact]
    public async Task GetForecastsByPowerPlantAsync_WhenForecastsExist_ReturnsSuccess()
    {
        // Arrange
        var powerPlantId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var powerPlant = new PowerPlant
        {
            Id = powerPlantId,
            Name = "Test Plant",
            Country = "Test Country",
            CompanyId = Guid.NewGuid()
        };

        var forecasts = new List<Forecast>
        {
            new() { Id = Guid.NewGuid(), PowerPlantId = powerPlantId, PowerPlant = powerPlant, ForecastDateTime = startDate, ProductionMWh = 100 },
            new() { Id = Guid.NewGuid(), PowerPlantId = powerPlantId, PowerPlant = powerPlant, ForecastDateTime = startDate.AddHours(12), ProductionMWh = 150 }
        };

        _powerPlantRepository.GetByIdAsync(powerPlantId).Returns(powerPlant);
        _forecastRepository.GetActiveByPowerPlantAsync(powerPlantId, startDate, endDate).Returns(forecasts);

        // Act
        var result = await _sut.GetForecastsByPowerPlantAsync(powerPlantId, startDate, endDate);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(2);
        foreach (var f in result.Value)
        {
            f.PowerPlantId.ShouldBe(powerPlantId);
            f.PowerPlantName.ShouldBe("Test Plant");
        }
    }

    [Fact]
    public async Task GetForecastsByPowerPlantAsync_WhenPowerPlantNotFound_ReturnsFailure()
    {
        // Arrange
        var powerPlantId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        _powerPlantRepository.GetByIdAsync(powerPlantId).Returns((PowerPlant?)null);

        // Act
        var result = await _sut.GetForecastsByPowerPlantAsync(powerPlantId, startDate, endDate);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("PowerPlant.NotFound");
    }

    [Fact]
    public async Task GetForecastsByPowerPlantAsync_WhenInvalidDateRange_ReturnsFailure()
    {
        // Arrange
        var powerPlantId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(-1); // End date before start date

        // Act
        var result = await _sut.GetForecastsByPowerPlantAsync(powerPlantId, startDate, endDate);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("InvalidDateRange");
    }

    [Fact]
    public async Task CreateOrUpdateForecastAsync_WhenUpdatingExistingForecast_UpdatesSuccessfully()
    {
        // Arrange
        var powerPlantId = Guid.NewGuid();
        var forecastDateTime = DateTime.UtcNow.Date;
        var existingForecastId = Guid.NewGuid();

        var powerPlant = new PowerPlant
        {
            Id = powerPlantId,
            Name = "Test Plant",
            Country = "Test Country",
            CompanyId = Guid.NewGuid()
        };

        var existingForecast = new Forecast
        {
            Id = existingForecastId,
            PowerPlantId = powerPlantId,
            PowerPlant = powerPlant,
            ForecastDateTime = forecastDateTime,
            ProductionMWh = 100
        };

        var request = new CreateOrUpdateForecastRequest
        {
            PowerPlantId = powerPlantId,
            ForecastDateTime = forecastDateTime,
            ProductionMWh = 200 // Updated value
        };

        _powerPlantRepository.GetByIdAsync(powerPlantId).Returns(powerPlant);
        _forecastRepository.GetByPowerPlantAndDateTimeAsync(powerPlantId, forecastDateTime).Returns(existingForecast);
        _forecastRepository.UpdateAsync(Arg.Any<Forecast>()).Returns(existingForecast);

        // Act
        var result = await _sut.CreateOrUpdateForecastAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(existingForecastId); // Should return existing ID
        result.Value.ProductionMWh.ShouldBe(200); // Should reflect updated value
        await _forecastRepository.Received(1).UpdateAsync(Arg.Is<Forecast>(f =>
            f.Id == existingForecastId && f.ProductionMWh == 200));
        await _forecastRepository.DidNotReceive().CreateAsync(Arg.Any<Forecast>());
    }
}

