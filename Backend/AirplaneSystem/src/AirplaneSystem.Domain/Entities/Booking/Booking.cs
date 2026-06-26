using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Booking;

public class Booking : AggregateRoot
{
    public string BookingReference { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public TripType TripType { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? HoldExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? PromoCodeId { get; set; }
    public decimal DiscountAmount { get; set; } = 0;

    public Users.User User { get; set; } = null!;
    public Payments.PromoCode? PromoCode { get; set; }
    public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    public ICollection<BookingSegment> BookingSegments { get; set; } = new List<BookingSegment>();
    public Payments.Payment? Payment { get; set; }
    public ICollection<Tickets.Ticket> Tickets { get; set; } = new List<Tickets.Ticket>();
}
