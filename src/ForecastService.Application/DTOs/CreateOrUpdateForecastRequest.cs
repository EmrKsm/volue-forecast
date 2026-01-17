namespace ForecastService.Application.DTOs;

public class CreateOrUpdateForecastRequest
{
    public Guid PowerPlantId { get; set; }
    public DateTime ForecastDateTime { get; set; }
    public decimal ProductionMWh { get; set; }
}
