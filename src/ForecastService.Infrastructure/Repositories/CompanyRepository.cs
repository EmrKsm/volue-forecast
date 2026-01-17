using ForecastService.Domain.Entities;
using ForecastService.Domain.Interfaces;
using ForecastService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ForecastService.Infrastructure.Repositories;

public class CompanyRepository(ForecastDbContext context) : ICompanyRepository
{
    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await context.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
