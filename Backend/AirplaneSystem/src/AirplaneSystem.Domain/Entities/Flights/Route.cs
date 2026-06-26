using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Route : BaseEntity
{
    public Guid OriginAirportId { get; set; }
    public Guid DestinationAirportId { get; set; }
    public int DistanceKm { get; set; }
    public int AverageFlightMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public Airport OriginAirport { get; set; } = null!;
    public Airport DestinationAirport { get; set; } = null!;
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
