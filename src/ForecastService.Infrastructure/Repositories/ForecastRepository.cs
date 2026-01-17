using ForecastService.Domain.Entities;
using ForecastService.Domain.Interfaces;
using ForecastService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ForecastService.Infrastructure.Repositories;

public class ForecastRepository(ForecastDbContext context) : IForecastRepository
{

    public async Task<Forecast?> GetByIdAsync(Guid id)
    {
        return await context.Forecasts
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Forecast>> GetByPowerPlantAsync(Guid powerPlantId, DateTime startDate, DateTime endDate)
    {
        return await context.Forecasts
            .AsNoTracking()
            .Where(f => f.PowerPlantId == powerPlantId &&
                    f.ForecastDateTime >= startDate &&
                    f.ForecastDateTime <= endDate)
            .OrderBy(f => f.ForecastDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Forecast>> GetActiveByPowerPlantAsync(Guid powerPlantId, DateTime startDate, DateTime endDate)
    {
        return await context.Forecasts
            .AsNoTracking()
            .Where(f => f.PowerPlantId == powerPlantId &&
                    f.ForecastDateTime >= startDate &&
                    f.ForecastDateTime <= endDate &&
                    f.IsActive)
            .OrderBy(f => f.ForecastDateTime)
            .ToListAsync();
    }

    public async Task<Forecast?> GetByPowerPlantAndDateTimeAsync(Guid powerPlantId, DateTime forecastDateTime)
    {
        return await context.Forecasts
            .FirstOrDefaultAsync(f => f.PowerPlantId == powerPlantId &&
                                    f.ForecastDateTime == forecastDateTime &&
                                    f.IsActive);
    }

    public async Task<Forecast> CreateAsync(Forecast forecast)
    {
        context.Forecasts.Add(forecast);
        await context.SaveChangesAsync();
        return forecast;
    }

    public async Task<Forecast> UpdateAsync(Forecast forecast)
    {
        context.Forecasts.Update(forecast);
        await context.SaveChangesAsync();
        return forecast;
    }

    public async Task<IEnumerable<Forecast>> GetAllActiveForCompanyAsync(Guid companyId, DateTime startDate, DateTime endDate)
    {
        return await context.Forecasts
            .AsNoTracking()
            .Where(f => f.PowerPlant.CompanyId == companyId &&
                    f.ForecastDateTime >= startDate &&
                    f.ForecastDateTime <= endDate &&
                    f.IsActive)
            .OrderBy(f => f.ForecastDateTime)
            .ToListAsync();
    }

    /// <summary>
    /// Get forecasts grouped by power plant for a company (optimized, single query)
    /// </summary>
    public async Task<IEnumerable<PowerPlantForecastSummary>> GetForecastSummaryByCompanyAsync(
        Guid companyId, DateTime startDate, DateTime endDate)
    {
        return await context.Forecasts
            .AsNoTracking()
            .Where(f => f.PowerPlant.CompanyId == companyId &&
                    f.ForecastDateTime >= startDate &&
                    f.ForecastDateTime <= endDate &&
                    f.IsActive)
            .GroupBy(f => new
            {
                f.PowerPlantId,
                f.PowerPlant.Name,
                f.PowerPlant.Country
            })
            .Select(g => new PowerPlantForecastSummary
            {
                PowerPlantId = g.Key.PowerPlantId,
                PowerPlantName = g.Key.Name,
                Country = g.Key.Country,
                TotalProductionMWh = g.Sum(f => f.ProductionMWh),
                ForecastCount = g.Count()
            })
            .ToListAsync();
    }
}
