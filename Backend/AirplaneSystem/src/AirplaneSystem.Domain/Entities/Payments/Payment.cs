using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Stripe;

    // Stripe-specific — শুধু Method == Stripe হলে populated থাকবে
    public string? StripePaymentIntentId { get; set; }
    public string? StripeClientSecret { get; set; }

    // Reference-based (manual) payment fields
    public string? ReferenceNumber { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; } // Rejection reason হিসেবেও reuse হবে
    public string? ReceiptUrl { get; set; }

    public Booking.Booking Booking { get; set; } = null!;
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}