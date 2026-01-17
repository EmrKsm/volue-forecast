using ForecastService.Api.Controllers;
using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using ForecastService.Domain.Common;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace ForecastService.Api.Tests.Controllers;

public class ForecastsControllerTests
{
    private readonly IForecastService _forecastService;
    private readonly ForecastsController _sut;

    public ForecastsControllerTests()
    {
        _forecastService = Substitute.For<IForecastService>();
        _sut = new ForecastsController(_forecastService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task CreateOrUpdateForecast_WhenSuccess_ReturnsCreatedOrOk()
    {
        // Arrange
        var request = new CreateOrUpdateForecastRequest
        {
            PowerPlantId = Guid.NewGuid(),
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 100.5m
        };

        var now = DateTime.UtcNow;
        var forecastResponse = new ForecastResponse
        {
            Id = Guid.NewGuid(),
            PowerPlantId = request.PowerPlantId!.Value,
            PowerPlantName = "Test Plant",
            Country = "Test Country",
            ForecastDateTime = request.ForecastDateTime!.Value,
            ProductionMWh = request.ProductionMWh!.Value,
            CreatedAt = now,
            UpdatedAt = now // Same timestamps = new forecast = 201
        };

        _forecastService.CreateOrUpdateForecastAsync(request).Returns(Result.Success(forecastResponse));

        // Act
        var result = await _sut.CreateOrUpdateForecast(request);

        // Assert
        result.Result.ShouldBeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result.Result!;
        objectResult.StatusCode.ShouldBeOneOf(200, 201);

        var apiResult = objectResult.Value as ApiResult<ForecastResponse>;
        apiResult.ShouldNotBeNull();
        apiResult!.Success.ShouldBeTrue();
        apiResult.Data.ShouldBe(forecastResponse);
    }

    [Fact]
    public async Task GetForecast_WhenForecastExists_Returns200Ok()
    {
        // Arrange
        var forecastId = Guid.NewGuid();
        var forecastResponse = new ForecastResponse
        {
            Id = forecastId,
            PowerPlantId = Guid.NewGuid(),
            PowerPlantName = "Test Plant",
            Country = "Test Country",
            ForecastDateTime = DateTime.UtcNow,
            ProductionMWh = 100.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _forecastService.GetForecastAsync(forecastId).Returns(Result.Success(forecastResponse));

        // Act
        var result = await _sut.GetForecast(forecastId);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var apiResult = okResult.Value as ApiResult<ForecastResponse>;
        apiResult.ShouldNotBeNull();
        apiResult!.Success.ShouldBeTrue();
        apiResult.Data.ShouldBe(forecastResponse);
    }

    [Fact]
    public async Task GetForecast_WhenForecastNotFound_Returns404NotFound()
    {
        // Arrange
        var forecastId = Guid.NewGuid();
        var error = new Error("Forecast.NotFound", "Forecast not found");
        _forecastService.GetForecastAsync(forecastId).Returns(Result.Failure<ForecastResponse>(error));

        // Act
        var result = await _sut.GetForecast(forecastId);

        // Assert
        result.Result.ShouldBeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result.Result!;
        objectResult.StatusCode.ShouldBe(404);

        var apiResult = objectResult.Value as ApiResult<ForecastResponse>;
        apiResult.ShouldNotBeNull();
        apiResult!.Success.ShouldBeFalse();
        apiResult.Data.ShouldBeNull();
        apiResult.Error.ShouldNotBeNull();
    }
}

