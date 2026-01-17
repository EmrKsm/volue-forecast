namespace ForecastService.Application.DTOs;

public class ForecastResponse
{
    public Guid Id { get; set; }
    public Guid PowerPlantId { get; set; }
    public string PowerPlantName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime ForecastDateTime { get; set; }
    public decimal ProductionMWh { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
