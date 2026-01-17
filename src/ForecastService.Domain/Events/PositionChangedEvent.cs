namespace ForecastService.Domain.Events;

public class PositionChangedEvent
{
    public Guid CompanyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPositionMWh { get; set; }
    public DateTime EventTimestamp { get; set; }
    public string Reason { get; set; } = string.Empty;
}
