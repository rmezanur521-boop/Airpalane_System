using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Flights;

public class FlightDto
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public string AirlineName { get; set; } = string.Empty;
    public string AirlineIata { get; set; } = string.Empty;
    public string? AirlineLogoUrl { get; set; }
    public Guid AircraftId { get; set; }
    public string AircraftModel { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string OriginIata { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMinutes { get; set; }
    public FlightStatus Status { get; set; }
    public decimal EconomyBasePrice { get; set; }
    public decimal BusinessBasePrice { get; set; }
    public decimal FirstClassBasePrice { get; set; }
    public decimal AirportFee { get; set; }
    public decimal TaxPercentage { get; set; }
    public int AvailableEconomySeats { get; set; }
    public int AvailableBusinessSeats { get; set; }
    public int AvailableFirstClassSeats { get; set; }
    public string? GateNumber { get; set; }
}

public class FlightSearchResult : FlightDto
{
    public decimal TotalPrice { get; set; }
    public SeatClass RequestedClass { get; set; }
    public int Stops { get; set; } = 0;
}

public class RoundTripResult
{
    public FlightSearchResult OutboundFlight { get; set; } = null!;
    public FlightSearchResult ReturnFlight { get; set; } = null!;
    public decimal TotalPrice { get; set; }
}

public class MultiCityResult
{
    public List<FlightSearchResult> Flights { get; set; } = new();
    public decimal TotalPrice { get; set; }
}
