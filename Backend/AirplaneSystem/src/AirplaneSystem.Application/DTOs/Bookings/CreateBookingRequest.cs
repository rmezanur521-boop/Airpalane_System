using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public TripType TripType { get; set; }
    public List<BookingSegmentRequest> Segments { get; set; } = new();
    public List<PassengerRequest> Passengers { get; set; } = new();
    public string? PromoCode { get; set; }
}

public class BookingSegmentRequest
{
    public Guid FlightId { get; set; }
    public SeatClass SeatClass { get; set; }
}

public class PassengerRequest
{
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
}
