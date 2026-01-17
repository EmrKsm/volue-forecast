namespace ForecastService.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateOrUpdateForecastRequest
{
    [Required(ErrorMessage = "Power plant ID is required")]
    public Guid? PowerPlantId { get; set; }

    [Required(ErrorMessage = "Forecast date and time is required")]
    public DateTime? ForecastDateTime { get; set; }

    [Required(ErrorMessage = "Production value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Production value must be non-negative")]
    public decimal? ProductionMWh { get; set; }
}
