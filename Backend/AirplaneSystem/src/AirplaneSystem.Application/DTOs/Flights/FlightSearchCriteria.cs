using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Flights;

public class FlightSearchCriteria
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public SeatClass SeatClass { get; set; } = SeatClass.Economy;
    public int PassengerCount { get; set; } = 1;
    public int MaxStops { get; set; } = 2;
}
