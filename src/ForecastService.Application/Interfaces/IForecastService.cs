using ForecastService.Application.DTOs;
using ForecastService.Domain.Common;

namespace ForecastService.Application.Interfaces;

public interface IForecastService
{
    Task<Result<ForecastResponse>> CreateOrUpdateForecastAsync(CreateOrUpdateForecastRequest request);
    Task<Result<ForecastResponse>> GetForecastAsync(Guid id);
    Task<Result<IEnumerable<ForecastResponse>>> GetForecastsByPowerPlantAsync(Guid powerPlantId, DateTime startDate, DateTime endDate);
}
