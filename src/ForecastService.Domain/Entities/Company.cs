namespace ForecastService.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<PowerPlant> PowerPlants { get; set; } = [];
}
