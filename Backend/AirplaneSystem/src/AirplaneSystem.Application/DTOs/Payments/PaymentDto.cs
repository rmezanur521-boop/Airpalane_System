using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;

    public PaymentMethod Method { get; set; }

    // Stripe-specific — Method == Stripe হলে populated থাকবে, নাহলে null
    public string? StripePaymentIntentId { get; set; }

    // Reference-based (manual) payment fields
    public string? ReferenceNumber { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }   // Rejection reason দেখানোর জন্যও কাজে লাগবে
    public string? ReceiptUrl { get; set; }
}

public class CreatePaymentIntentRequest
{
    public Guid BookingId { get; set; }
}

public class PaymentIntentResult
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}

public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class PromoValidationRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
}

public class PromoValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Code { get; set; }
}

public class RefundRequestDto
{
    public Guid BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RefundDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
