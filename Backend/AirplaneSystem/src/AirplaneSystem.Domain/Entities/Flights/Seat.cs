using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Flights;

public class Seat : BaseEntity
{
    public Guid FlightId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsWindowSeat { get; set; } = false;
    public bool IsAisleSeat { get; set; } = false;
    public bool IsExitRow { get; set; } = false;
    public bool ExtraLegroom { get; set; } = false;

    public Flight Flight { get; set; } = null!;
}
