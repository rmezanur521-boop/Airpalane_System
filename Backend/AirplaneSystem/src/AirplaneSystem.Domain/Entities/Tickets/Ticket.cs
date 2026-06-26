using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Entities.Tickets;

public class Ticket : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid BookingPassengerId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid BookingSegmentId { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string? BoardingPassUrl { get; set; }
    public string QrCodeData { get; set; } = string.Empty;
    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CheckedInAt { get; set; }

    public Booking.Booking Booking { get; set; } = null!;
    public Booking.BookingPassenger BookingPassenger { get; set; } = null!;
    public Booking.BookingSegment BookingSegment { get; set; } = null!;
}
