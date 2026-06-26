using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Payments;

public class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public string Reason { get; set; } = string.Empty;
    public string? StripeRefundId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? DeniedAt { get; set; }
    public string? DenialReason { get; set; }

    public Payment Payment { get; set; } = null!;
}
