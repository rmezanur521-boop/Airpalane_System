using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Booking;

public class BookingPassenger : BaseEntity
{
    public Guid BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public PassengerType PassengerType { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? PassportNumber { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? PassportCountry { get; set; }
    public Guid? SeatId { get; set; }
    public string? MealPreference { get; set; }
    public string? SpecialAssistance { get; set; }

    public Booking Booking { get; set; } = null!;
    public Flights.Seat? Seat { get; set; }
    public ICollection<Tickets.Ticket> Tickets { get; set; } = new List<Tickets.Ticket>();

    public string FullName => $"{FirstName} {LastName}";
}
