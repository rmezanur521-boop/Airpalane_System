using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public string StripeClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public string? ReceiptUrl { get; set; }

    public Booking.Booking Booking { get; set; } = null!;
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
