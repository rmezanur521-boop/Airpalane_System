using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Flight : AggregateRoot
{
    public string FlightNumber { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public Guid AircraftId { get; set; }
    public Guid RouteId { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public decimal EconomyBasePrice { get; set; }
    public decimal BusinessBasePrice { get; set; }
    public decimal FirstClassBasePrice { get; set; }
    public decimal AirportFee { get; set; }
    public decimal TaxPercentage { get; set; }
    public int AvailableEconomySeats { get; set; }
    public int AvailableBusinessSeats { get; set; }
    public int AvailableFirstClassSeats { get; set; }
    public string? GateNumber { get; set; }
    public bool IsCodeShare { get; set; } = false;

    public Airline Airline { get; set; } = null!;
    public Aircraft Aircraft { get; set; } = null!;
    public Route Route { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public TimeSpan Duration => ArrivalTime - DepartureTime;

    public decimal GetBasePriceForClass(SeatClass seatClass) => seatClass switch
    {
        SeatClass.Economy => EconomyBasePrice,
        SeatClass.Business => BusinessBasePrice,
        SeatClass.First => FirstClassBasePrice,
        _ => EconomyBasePrice
    };

    public int GetAvailableSeatsForClass(SeatClass seatClass) => seatClass switch
    {
        SeatClass.Economy => AvailableEconomySeats,
        SeatClass.Business => AvailableBusinessSeats,
        SeatClass.First => AvailableFirstClassSeats,
        _ => 0
    };
}
