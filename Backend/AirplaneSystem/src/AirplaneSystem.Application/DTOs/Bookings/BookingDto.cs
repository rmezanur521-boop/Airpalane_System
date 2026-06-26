using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Bookings;

public class BookingDto
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public TripType TripType { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? HoldExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BookingSegmentDto> Segments { get; set; } = new();
    public List<BookingPassengerDto> Passengers { get; set; } = new();
    public PaymentSummaryDto? Payment { get; set; }
}

public class BookingSegmentDto
{
    public Guid Id { get; set; }
    public int SegmentOrder { get; set; }
    public SeatClass SeatClass { get; set; }
    public decimal BaseFare { get; set; }
    public decimal Taxes { get; set; }
    public decimal Fees { get; set; }
    public decimal SegmentTotal { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
}

public class BookingPassengerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public PassengerType PassengerType { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? SeatNumber { get; set; }
    public string? MealPreference { get; set; }
}

public class PaymentSummaryDto
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ReceiptUrl { get; set; }
}
