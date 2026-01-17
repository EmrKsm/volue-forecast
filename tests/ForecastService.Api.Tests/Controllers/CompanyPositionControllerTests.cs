using ForecastService.Api.Controllers;
using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using ForecastService.Domain.Common;
using ForecastService.Domain.Errors;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace ForecastService.Api.Tests.Controllers;

public class CompanyPositionControllerTests
{
    private readonly ICompanyPositionService _companyPositionService;
    private readonly CompanyPositionController _sut;

    public CompanyPositionControllerTests()
    {
        _companyPositionService = Substitute.For<ICompanyPositionService>();
        _sut = new CompanyPositionController(_companyPositionService);

        // Setup HttpContext for error handling
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetCompanyPosition_WhenSuccess_Returns200Ok()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var response = new CompanyPositionResponse
        {
            CompanyId = companyId,
            CompanyName = "Test Company",
            StartDate = startDate,
            EndDate = endDate,
            TotalPositionMWh = 500,
            PowerPlantPositions = new List<PowerPlantPositionDto>
            {
                new() { PowerPlantId = Guid.NewGuid(), PowerPlantName = "Plant1", TotalProductionMWh = 200 },
                new() { PowerPlantId = Guid.NewGuid(), PowerPlantName = "Plant2", TotalProductionMWh = 300 }
            }
        };

        _companyPositionService.GetCompanyPositionAsync(companyId, startDate, endDate)
            .Returns(Result.Success(response));

        // Act
        var result = await _sut.GetCompanyPosition(companyId, startDate, endDate);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);

        var apiResult = okResult.Value.ShouldBeAssignableTo<ApiResult<CompanyPositionResponse>>();
        apiResult.Success.ShouldBeTrue();
        apiResult.Data.ShouldNotBeNull();
        apiResult.Data!.CompanyId.ShouldBe(companyId);
        apiResult.Data.TotalPositionMWh.ShouldBe(500);
        apiResult.Data.PowerPlantPositions.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetCompanyPosition_WhenCompanyNotFound_Returns404NotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        _companyPositionService.GetCompanyPositionAsync(companyId, startDate, endDate)
            .Returns(Result.Failure<CompanyPositionResponse>(CompanyErrors.NotFound(companyId)));

        // Act
        var result = await _sut.GetCompanyPosition(companyId, startDate, endDate);

        // Assert
        var objectResult = result.Result.ShouldBeAssignableTo<ObjectResult>();
        objectResult.StatusCode.ShouldBe(404);

        var apiResult = objectResult.Value.ShouldBeAssignableTo<ApiResult<CompanyPositionResponse>>();
        apiResult.Success.ShouldBeFalse();
        apiResult.Data.ShouldBeNull();
        apiResult.Error.ShouldNotBeNull();
        apiResult.Error!.Status.ShouldBe(404);
    }

    [Fact]
    public async Task GetCompanyPosition_WhenInvalidDateRange_Returns400BadRequest()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(-1); // Invalid: end before start

        _companyPositionService.GetCompanyPositionAsync(companyId, startDate, endDate)
            .Returns(Result.Failure<CompanyPositionResponse>(ForecastErrors.InvalidDateRange));

        // Act
        var result = await _sut.GetCompanyPosition(companyId, startDate, endDate);

        // Assert
        var objectResult = result.Result.ShouldBeAssignableTo<ObjectResult>();
        objectResult.StatusCode.ShouldBe(400);

        var apiResult = objectResult.Value.ShouldBeAssignableTo<ApiResult<CompanyPositionResponse>>();
        apiResult.Success.ShouldBeFalse();
        apiResult.Data.ShouldBeNull();
        apiResult.Error.ShouldNotBeNull();
        apiResult.Error!.Status.ShouldBe(400);
    }

    [Fact]
    public async Task GetCompanyPosition_WhenServiceReturnsEmptyPosition_Returns200WithZeroTotal()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var response = new CompanyPositionResponse
        {
            CompanyId = companyId,
            CompanyName = "Test Company",
            StartDate = startDate,
            EndDate = endDate,
            TotalPositionMWh = 0,
            PowerPlantPositions = new List<PowerPlantPositionDto>()
        };

        _companyPositionService.GetCompanyPositionAsync(companyId, startDate, endDate)
            .Returns(Result.Success(response));

        // Act
        var result = await _sut.GetCompanyPosition(companyId, startDate, endDate);

        // Assert
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();

        var apiResult = okResult!.Value.ShouldBeAssignableTo<ApiResult<CompanyPositionResponse>>();
        apiResult.Success.ShouldBeTrue();
        apiResult.Data!.TotalPositionMWh.ShouldBe(0);
        apiResult.Data.PowerPlantPositions.ShouldBeEmpty();
    }
}

