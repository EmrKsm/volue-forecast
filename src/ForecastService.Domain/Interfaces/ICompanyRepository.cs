using ForecastService.Domain.Entities;

namespace ForecastService.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<IEnumerable<Company>> GetAllAsync();
}
