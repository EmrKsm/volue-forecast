using ForecastService.Application.DTOs;
using Shouldly;

namespace ForecastService.Application.Tests.DTOs;

public class CreateOrUpdateForecastRequestTests
{
    [Fact]
    public void ProductionMWh_WhenSetToPositiveValue_ShouldSetSuccessfully()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();

        // Act
        request.ProductionMWh = 150.5m;

        // Assert
        request.ProductionMWh.ShouldBe(150.5m);
    }

    [Fact]
    public void ProductionMWh_WhenSetToZero_ShouldSetSuccessfully()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();

        // Act
        request.ProductionMWh = 0m;

        // Assert
        request.ProductionMWh.ShouldBe(0m);
    }

    [Fact]
    public void ProductionMWh_WhenSetToNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => request.ProductionMWh = -10m);
        exception.Message.ShouldContain("Production value cannot be negative");
        exception.ParamName.ShouldBe("ProductionMWh");
    }

    [Fact]
    public void ProductionMWh_WhenSetToLargePositiveValue_ShouldSetSuccessfully()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();

        // Act
        request.ProductionMWh = 999999.99m;

        // Assert
        request.ProductionMWh.ShouldBe(999999.99m);
    }

    [Fact]
    public void ProductionMWh_WhenSetToSmallNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();

        // Act & Assert
        Should.Throw<ArgumentException>(() => request.ProductionMWh = -0.01m);
    }

    [Fact]
    public void PowerPlantId_ShouldBeSettableAndGettable()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();
        var powerPlantId = Guid.NewGuid();

        // Act
        request.PowerPlantId = powerPlantId;

        // Assert
        request.PowerPlantId.ShouldBe(powerPlantId);
    }

    [Fact]
    public void ForecastDateTime_ShouldBeSettableAndGettable()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest();
        var forecastDateTime = DateTime.UtcNow;

        // Act
        request.ForecastDateTime = forecastDateTime;

        // Assert
        request.ForecastDateTime.ShouldBe(forecastDateTime);
    }

    [Fact]
    public void Request_WhenFullyPopulated_ShouldHaveAllProperties()
    {
        // Arrange & Act
        var request = new CreateOrUpdateForecastRequest
        {
            PowerPlantId = Guid.NewGuid(),
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 250.75m
        };

        // Assert
        request.PowerPlantId.ShouldNotBe(Guid.Empty);
        request.ForecastDateTime.ShouldNotBe(default);
        request.ProductionMWh.ShouldBe(250.75m);
    }
}
