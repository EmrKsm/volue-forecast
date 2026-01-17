namespace ForecastService.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreateOrUpdateForecastRequest
{
    public Guid PowerPlantId { get; set; }
    
    public DateTime ForecastDateTime { get; set; }
    
    // C# 14: Using 'field' keyword for property with validation
    [Range(0, double.MaxValue, ErrorMessage = "Production must be non-negative")]
    public decimal ProductionMWh
    {
        get;
        set => field = value >= 0 ? value : throw new ArgumentException("Production value cannot be negative", nameof(ProductionMWh));
    }
}
