using ForecastService.Domain.Entities;

namespace ForecastService.Domain.Interfaces;

public interface IForecastRepository
{
    Task<Forecast?> GetByIdAsync(Guid id);
    Task<IEnumerable<Forecast>> GetByPowerPlantAsync(Guid powerPlantId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Forecast>> GetActiveByPowerPlantAsync(Guid powerPlantId, DateTime startDate, DateTime endDate);
    Task<Forecast?> GetByPowerPlantAndDateTimeAsync(Guid powerPlantId, DateTime forecastDateTime);
    Task<Forecast> CreateAsync(Forecast forecast);
    Task<Forecast> UpdateAsync(Forecast forecast);
    Task<IEnumerable<Forecast>> GetAllActiveForCompanyAsync(Guid companyId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<PowerPlantForecastSummary>> GetForecastSummaryByCompanyAsync(Guid companyId, DateTime startDate, DateTime endDate);
}

/// <summary>
/// DTO for optimized forecast summary query (to avoid N+1 problem)
/// </summary>
public class PowerPlantForecastSummary
{
    public Guid PowerPlantId { get; set; }
    public string PowerPlantName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal TotalProductionMWh { get; set; }
    public int ForecastCount { get; set; }
}
