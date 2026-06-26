using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Flights;

public class CreateFlightRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public Guid AircraftId { get; set; }
    public Guid RouteId { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal EconomyBasePrice { get; set; }
    public decimal BusinessBasePrice { get; set; }
    public decimal FirstClassBasePrice { get; set; }
    public decimal AirportFee { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? GateNumber { get; set; }
}

public class UpdateFlightStatusRequest
{
    public FlightStatus Status { get; set; }
    public string? GateNumber { get; set; }
    public string? DelayReason { get; set; }
}
