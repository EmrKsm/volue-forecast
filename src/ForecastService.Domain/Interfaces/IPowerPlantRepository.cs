using ForecastService.Domain.Entities;

namespace ForecastService.Domain.Interfaces;

public interface IPowerPlantRepository
{
    Task<PowerPlant?> GetByIdAsync(Guid id);
    Task<IEnumerable<PowerPlant>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<PowerPlant>> GetAllAsync();
}
