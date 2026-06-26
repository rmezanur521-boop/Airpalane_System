using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Payments;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Guid bookingId, CancellationToken ct = default);
    Task<PaymentDto> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default);
    Task<PromoValidationResult> ValidatePromoCodeAsync(PromoValidationRequest request, CancellationToken ct = default);
    Task<RefundDto> RequestRefundAsync(RefundRequestDto request, CancellationToken ct = default);
    Task ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default);
    Task<PaymentDto> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> GetAllAsync(Common.Models.PaginationQuery query, CancellationToken ct = default);
    Task<RefundDto> ProcessRefundAsync(Guid refundId, bool approve, string? denialReason, CancellationToken ct = default);
}
