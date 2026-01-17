namespace ForecastService.Domain.Entities;

public class Forecast : BaseEntity
{
    public Guid PowerPlantId { get; set; }
    public PowerPlant PowerPlant { get; set; } = null!;

    private DateTime _forecastDateTime;
    public DateTime ForecastDateTime
    {
        get => _forecastDateTime;
        set => _forecastDateTime = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    public decimal ProductionMWh { get; set; }
    public bool IsActive { get; set; } = true;
}
