using ForecastService.Application.DTOs;
using ForecastService.Application.Services;
using ForecastService.Domain.Entities;
using ForecastService.Domain.Interfaces;
using Shouldly;
using NSubstitute;

namespace ForecastService.Application.Tests.Services;

public class CompanyPositionServiceTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IPowerPlantRepository _powerPlantRepository;
    private readonly IForecastRepository _forecastRepository;
    private readonly CompanyPositionService _sut;

    public CompanyPositionServiceTests()
    {
        _companyRepository = Substitute.For<ICompanyRepository>();
        _powerPlantRepository = Substitute.For<IPowerPlantRepository>();
        _forecastRepository = Substitute.For<IForecastRepository>();
        _sut = new CompanyPositionService(_companyRepository, _powerPlantRepository, _forecastRepository);
    }

    [Fact]
    public async Task GetCompanyPositionAsync_WhenCompanyNotFound_ReturnsFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        _companyRepository.GetByIdAsync(companyId).Returns((Company?)null);

        // Act
        var result = await _sut.GetCompanyPositionAsync(companyId, startDate, endDate);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("Company.NotFound");
    }

    [Fact]
    public async Task GetCompanyPositionAsync_WhenInvalidDateRange_ReturnsFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(-1); // End date before start date

        // Act
        var result = await _sut.GetCompanyPositionAsync(companyId, startDate, endDate);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldContain("InvalidDateRange");
    }

    [Fact]
    public async Task GetCompanyPositionAsync_WhenCompanyHasNoForecasts_ReturnsZeroPosition()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var company = new Company
        {
            Id = companyId,
            Name = "Test Company"
        };

        var powerPlants = new List<PowerPlant>
        {
            new() { Id = Guid.NewGuid(), Name = "Plant1", CompanyId = companyId, Country = "Test" },
            new() { Id = Guid.NewGuid(), Name = "Plant2", CompanyId = companyId, Country = "Test" }
        };

        _companyRepository.GetByIdAsync(companyId).Returns(company);
        _powerPlantRepository.GetByCompanyIdAsync(companyId).Returns(powerPlants);
        _forecastRepository.GetForecastSummaryByCompanyAsync(companyId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<PowerPlantForecastSummary>());

        // Act
        var result = await _sut.GetCompanyPositionAsync(companyId, startDate, endDate);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CompanyId.ShouldBe(companyId);
        result.Value.CompanyName.ShouldBe("Test Company");
        result.Value.TotalPositionMWh.ShouldBe(0);
        result.Value.PowerPlantPositions.Count.ShouldBe(2);
        foreach (var p in result.Value.PowerPlantPositions)
        {
            p.TotalProductionMWh.ShouldBe(0);
        }
    }

    [Fact]
    public async Task GetCompanyPositionAsync_WhenCompanyHasForecasts_AggregatesCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var company = new Company
        {
            Id = companyId,
            Name = "Test Company"
        };

        var powerPlant1Id = Guid.NewGuid();
        var powerPlant2Id = Guid.NewGuid();

        var powerPlants = new List<PowerPlant>
        {
            new() { Id = powerPlant1Id, Name = "Plant1", CompanyId = companyId, Country = "Test" },
            new() { Id = powerPlant2Id, Name = "Plant2", CompanyId = companyId, Country = "Test" }
        };

        var forecastSummaries = new List<PowerPlantForecastSummary>
        {
            new() { PowerPlantId = powerPlant1Id, PowerPlantName = "Plant1", TotalProductionMWh = 150.5m },
            new() { PowerPlantId = powerPlant2Id, PowerPlantName = "Plant2", TotalProductionMWh = 249.5m }
        };

        _companyRepository.GetByIdAsync(companyId).Returns(company);
        _powerPlantRepository.GetByCompanyIdAsync(companyId).Returns(powerPlants);
        _forecastRepository.GetForecastSummaryByCompanyAsync(companyId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(forecastSummaries);

        // Act
        var result = await _sut.GetCompanyPositionAsync(companyId, startDate, endDate);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CompanyId.ShouldBe(companyId);
        result.Value.CompanyName.ShouldBe("Test Company");
        result.Value.TotalPositionMWh.ShouldBe(400); // 150.5 + 249.5 = 400
        result.Value.PowerPlantPositions.Count.ShouldBe(2);

        var plant1Position = result.Value.PowerPlantPositions.First(p => p.PowerPlantId == powerPlant1Id);
        plant1Position.TotalProductionMWh.ShouldBe(150.5m);
        plant1Position.PowerPlantName.ShouldBe("Plant1");

        var plant2Position = result.Value.PowerPlantPositions.First(p => p.PowerPlantId == powerPlant2Id);
        plant2Position.TotalProductionMWh.ShouldBe(249.5m);
        plant2Position.PowerPlantName.ShouldBe("Plant2");
    }

    [Fact]
    public async Task GetCompanyPositionAsync_WhenSomePowerPlantsHaveNoForecasts_IncludesAllPowerPlants()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(1);

        var company = new Company
        {
            Id = companyId,
            Name = "Test Company"
        };

        var powerPlant1Id = Guid.NewGuid();
        var powerPlant2Id = Guid.NewGuid();
        var powerPlant3Id = Guid.NewGuid();

        var powerPlants = new List<PowerPlant>
        {
            new() { Id = powerPlant1Id, Name = "Plant1", CompanyId = companyId, Country = "Test" },
            new() { Id = powerPlant2Id, Name = "Plant2", CompanyId = companyId, Country = "Test" },
            new() { Id = powerPlant3Id, Name = "Plant3", CompanyId = companyId, Country = "Test" }
        };

        // Only Plant1 and Plant3 have forecasts
        var forecastSummaries = new List<PowerPlantForecastSummary>
        {
            new() { PowerPlantId = powerPlant1Id, PowerPlantName = "Plant1", TotalProductionMWh = 100m },
            new() { PowerPlantId = powerPlant3Id, PowerPlantName = "Plant3", TotalProductionMWh = 50m }
        };

        _companyRepository.GetByIdAsync(companyId).Returns(company);
        _powerPlantRepository.GetByCompanyIdAsync(companyId).Returns(powerPlants);
        _forecastRepository.GetForecastSummaryByCompanyAsync(companyId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(forecastSummaries);

        // Act
        var result = await _sut.GetCompanyPositionAsync(companyId, startDate, endDate);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPositionMWh.ShouldBe(150); // 100 + 50
        result.Value.PowerPlantPositions.Count.ShouldBe(3); // All plants included

        // Plant2 should have 0 position
        var plant2Position = result.Value.PowerPlantPositions.First(p => p.PowerPlantId == powerPlant2Id);
        plant2Position.TotalProductionMWh.ShouldBe(0);
        plant2Position.PowerPlantName.ShouldBe("Plant2");
    }
}

