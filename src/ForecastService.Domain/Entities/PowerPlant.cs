namespace ForecastService.Domain.Entities;

public class PowerPlant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<Forecast> Forecasts { get; set; } = [];
}
