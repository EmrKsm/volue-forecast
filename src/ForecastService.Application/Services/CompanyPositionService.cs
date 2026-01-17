using ForecastService.Application.DTOs;
using ForecastService.Application.Interfaces;
using ForecastService.Domain.Common;
using ForecastService.Domain.Errors;
using ForecastService.Domain.Interfaces;

namespace ForecastService.Application.Services;

public class CompanyPositionService(
    ICompanyRepository companyRepository,
    IPowerPlantRepository powerPlantRepository,
    IForecastRepository forecastRepository) : ICompanyPositionService
{

    public async Task<Result<CompanyPositionResponse>> GetCompanyPositionAsync(
        Guid companyId,
        DateTime startDate,
        DateTime endDate)
    {
        // Validate date range
        if (startDate > endDate)
        {
            return Result.Failure<CompanyPositionResponse>(ForecastErrors.InvalidDateRange);
        }

        // Validate company exists
        var company = await companyRepository.GetByIdAsync(companyId);
        if (company == null)
        {
            return Result.Failure<CompanyPositionResponse>(CompanyErrors.NotFound(companyId));
        }

        // Normalize dates to start of day
        startDate = startDate.Date;
        endDate = endDate.Date.AddDays(1).AddTicks(-1);

        // Use optimized single query to get all forecast summaries (fixes N+1 problem)
        var forecastSummaries = await forecastRepository.GetForecastSummaryByCompanyAsync(
            companyId,
            startDate,
            endDate);

        // Get all power plants for the company (for plants with no forecasts)
        var allPowerPlants = await powerPlantRepository.GetByCompanyIdAsync(companyId);

        var response = new CompanyPositionResponse
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            StartDate = startDate,
            EndDate = endDate,
            TotalPositionMWh = 0,
            PowerPlantPositions = new List<PowerPlantPositionDto>()
        };

        decimal totalPosition = 0;
        var summaryDict = forecastSummaries.ToDictionary(s => s.PowerPlantId);

        // Build response including plants with and without forecasts
        foreach (var powerPlant in allPowerPlants)
        {
            if (summaryDict.TryGetValue(powerPlant.Id, out var summary))
            {
                totalPosition += summary.TotalProductionMWh;
                response.PowerPlantPositions.Add(new PowerPlantPositionDto
                {
                    PowerPlantId = summary.PowerPlantId,
                    PowerPlantName = summary.PowerPlantName,
                    Country = summary.Country,
                    TotalProductionMWh = summary.TotalProductionMWh,
                    ForecastCount = summary.ForecastCount
                });
            }
            else
            {
                // Power plant with no forecasts in date range
                response.PowerPlantPositions.Add(new PowerPlantPositionDto
                {
                    PowerPlantId = powerPlant.Id,
                    PowerPlantName = powerPlant.Name,
                    Country = powerPlant.Country,
                    TotalProductionMWh = 0,
                    ForecastCount = 0
                });
            }
        }

        response.TotalPositionMWh = totalPosition;
        return Result.Success(response);
    }
}
