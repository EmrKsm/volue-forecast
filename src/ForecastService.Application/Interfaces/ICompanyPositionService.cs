using ForecastService.Application.DTOs;
using ForecastService.Domain.Common;

namespace ForecastService.Application.Interfaces;

public interface ICompanyPositionService
{
    Task<Result<CompanyPositionResponse>> GetCompanyPositionAsync(Guid companyId, DateTime startDate, DateTime endDate);
}
