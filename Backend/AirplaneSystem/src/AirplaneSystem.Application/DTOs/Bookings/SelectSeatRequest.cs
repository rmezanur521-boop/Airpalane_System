namespace AirplaneSystem.Application.DTOs.Bookings;

public class SelectSeatRequest
{
    public Guid PassengerId { get; set; }
    public Guid SeatId { get; set; }
}
