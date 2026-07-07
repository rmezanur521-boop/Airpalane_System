namespace AirplaneSystem.Application.DTOs.Flights;

public class RouteDto
{
    public Guid Id { get; set; }
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public string OriginIata { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public int DistanceKm { get; set; }
    public int AverageFlightMinutes { get; set; }

    /// <summary>Human-readable label, e.g. "Dhaka (DAC) → Dubai (DXB)".</summary>
    public string Name => $"{OriginCity} ({OriginIata}) \u2192 {DestinationCity} ({DestinationIata})";
}