using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Flights;

public class SeatMapDto
{
    public Guid FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public List<SeatDto> Seats { get; set; } = new();
}

public class SeatDto
{
    public Guid Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsWindowSeat { get; set; }
    public bool IsAisleSeat { get; set; }
    public bool IsExitRow { get; set; }
    public bool ExtraLegroom { get; set; }
}
