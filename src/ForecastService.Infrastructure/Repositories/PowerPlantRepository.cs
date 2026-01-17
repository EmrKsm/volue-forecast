using ForecastService.Domain.Entities;
using ForecastService.Domain.Interfaces;
using ForecastService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ForecastService.Infrastructure.Repositories;

public class PowerPlantRepository(ForecastDbContext context) : IPowerPlantRepository
{
    public async Task<PowerPlant?> GetByIdAsync(Guid id)
    {
        return await context.PowerPlants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PowerPlant>> GetByCompanyIdAsync(Guid companyId)
    {
        return await context.PowerPlants
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<PowerPlant>> GetAllAsync()
    {
        return await context.PowerPlants
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
