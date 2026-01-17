namespace ForecastService.Application.DTOs;

public class CompanyPositionResponse
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPositionMWh { get; set; }
    public List<PowerPlantPositionDto> PowerPlantPositions { get; set; } = new();
}

public class PowerPlantPositionDto
{
    public Guid PowerPlantId { get; set; }
    public string PowerPlantName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal TotalProductionMWh { get; set; }
    public int ForecastCount { get; set; }
}
