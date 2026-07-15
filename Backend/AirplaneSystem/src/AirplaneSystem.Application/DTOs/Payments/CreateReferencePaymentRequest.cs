using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Payments;

public class CreateReferencePaymentRequest
{
    public Guid BookingId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public PaymentMethod Method { get; set; } // BankTransfer / MobileBanking
}

public class ApprovePaymentRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}