using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Payments;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Payments;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using DomainRefund = AirplaneSystem.Domain.Entities.Payments.Refund;
using PaymentStatus = AirplaneSystem.Domain.Enums.PaymentStatus;
using StripeEvents = Stripe.Events;

namespace AirplaneSystem.Infrastructure.ExternalServices.Stripe;

public class StripePaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IEncryptionService _encryption;
    private readonly ITicketService _ticketService;
    private readonly INotificationService _notification;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StripePaymentService(IUnitOfWork uow, IMapper mapper, IConfiguration config,
    ILogger<StripePaymentService> logger, IEncryptionService encryption,
    ITicketService ticketService, INotificationService notification,
    IHttpContextAccessor httpContextAccessor)
    {
        _uow = uow;
        _mapper = mapper;
        _config = config;
        _logger = logger;
        _encryption = encryption;
        _ticketService = ticketService;
        _notification = notification;
        _httpContextAccessor = httpContextAccessor;

        // InitializeStripe(); ← constructor থেকে সরিয়ে ফেলুন
    }

    private bool _stripeInitialized;
    private readonly object _stripeInitLock = new();

    private void EnsureStripeInitialized()
    {
        if (_stripeInitialized) return;
        lock (_stripeInitLock)
        {
            if (_stripeInitialized) return;

            var secretKeyRaw = _config["Stripe:SecretKey"] ?? string.Empty;
            var secretKey = _encryption.IsEncrypted(secretKeyRaw) ? _encryption.Decrypt(secretKeyRaw) : secretKeyRaw;
            StripeConfiguration.ApiKey = secretKey;
            _stripeInitialized = true;
        }
    }
    private void InitializeStripe()
    {
        var secretKeyRaw = _config["Stripe:SecretKey"] ?? string.Empty;
        var secretKey = _encryption.IsEncrypted(secretKeyRaw) ? _encryption.Decrypt(secretKeyRaw) : secretKeyRaw;
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Guid bookingId, CancellationToken ct = default)
    {
        EnsureStripeInitialized();
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.Status != BookingStatus.PendingPayment)
            throw new ConflictException("Booking is not in a payable state.");

        if (booking.HoldExpiresAt < DateTime.UtcNow)
            throw new ConflictException("Booking hold has expired. Please create a new booking.");

        var amountInCents = (long)(booking.TotalAmount * 100);
        var user = await _uow.Users.GetByIdAsync(booking.UserId, ct);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                ["booking_reference"] = booking.BookingReference,
                ["booking_id"] = booking.Id.ToString(),
                ["user_id"] = booking.UserId.ToString()
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: ct);

        var payment = new Payment
        {
            BookingId = bookingId,
            StripePaymentIntentId = intent.Id,
            StripeClientSecret = intent.ClientSecret,
            Amount = booking.TotalAmount,
            Status = PaymentStatus.Pending
        };

        await _uow.Payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return new PaymentIntentResult
        {
            PaymentIntentId = intent.Id,
            ClientSecret = intent.ClientSecret,
            Amount = booking.TotalAmount,
            Currency = "usd"
        };
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        EnsureStripeInitialized();
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId, cancellationToken: ct);

        var payment = await _uow.Payments.GetByStripeIntentIdAsync(paymentIntentId, ct)
            ?? throw new NotFoundException($"Payment for intent '{paymentIntentId}' not found.");

        if (intent.Status == "succeeded")
        {
            await ConfirmPaymentInternalAsync(payment, intent.LatestChargeId, ct);
        }
        else
        {
            throw new PaymentException($"Payment not successful. Status: {intent.Status}");
        }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PromoValidationResult> ValidatePromoCodeAsync(PromoValidationRequest request, CancellationToken ct = default)
    {
        var promo = await _uow.PromoCodes.GetByCodeAsync(request.Code.ToUpperInvariant(), ct);

        if (promo == null || !promo.IsValid)
            return new PromoValidationResult { IsValid = false, Message = "Promo code is invalid or expired." };

        if (request.CartTotal < promo.MinimumAmount)
            return new PromoValidationResult { IsValid = false, Message = $"Minimum order amount for this promo is ${promo.MinimumAmount:F2}." };

        var discount = promo.CalculateDiscount(request.CartTotal);
        return new PromoValidationResult
        {
            IsValid = true,
            Code = promo.Code,
            DiscountAmount = discount,
            FinalAmount = request.CartTotal - discount,
            Message = $"Promo code applied! You save ${discount:F2}."
        };
    }

    public async Task<RefundDto> RequestRefundAsync(RefundRequestDto request, CancellationToken ct = default)
    {
        var payment = await _uow.Payments.GetByBookingIdAsync(request.BookingId, ct)
            ?? throw new NotFoundException("Payment for booking not found.");

        if (payment.Status != PaymentStatus.Succeeded)
            throw new ConflictException("Cannot request refund for a non-successful payment.");

        var refund = new DomainRefund
        {
            PaymentId = payment.Id,
            BookingId = request.BookingId,
            Amount = payment.Amount,
            Status = RefundStatus.Pending,
            Reason = request.Reason,
            RequestedAt = DateTime.UtcNow
        };

        payment.Refunds.Add(refund);
        _uow.Payments.Update(payment);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<RefundDto>(refund);
    }

    public async Task ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        EnsureStripeInitialized();
        var webhookSecretRaw = _config["Stripe:WebhookSecret"] ?? string.Empty;
        var webhookSecret = _encryption.IsEncrypted(webhookSecretRaw) ? _encryption.Decrypt(webhookSecretRaw) : webhookSecretRaw;

        global::Stripe.Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature");
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent != null)
            {
                var payment = await _uow.Payments.GetByStripeIntentIdAsync(intent.Id, ct);
                if (payment != null && payment.Status != PaymentStatus.Succeeded)
                    await ConfirmPaymentInternalAsync(payment, intent.LatestChargeId, ct);
            }
        }
    }

    public async Task<PaymentDto> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await _uow.Payments.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment", paymentId);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PagedResult<PaymentDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var payments = await _uow.Payments.GetAllAsync(ct);
        var total = payments.Count;
        var items = payments.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .Select(p => _mapper.Map<PaymentDto>(p)).ToList();
        return PagedResult<PaymentDto>.Create(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<RefundDto> ProcessRefundAsync(Guid refundId, bool approve, string? denialReason, CancellationToken ct = default)
    {
        EnsureStripeInitialized();
        var allPayments = await _uow.Payments.GetAllAsync(ct);
        var refund = allPayments.SelectMany(p => p.Refunds).FirstOrDefault(r => r.Id == refundId)
            ?? throw new NotFoundException("Refund", refundId);

        if (approve)
        {
            var service = new RefundService();
            var options = new RefundCreateOptions
            {
                PaymentIntent = (await _uow.Payments.GetByIdAsync(refund.PaymentId, ct))?.StripePaymentIntentId,
                Amount = (long)(refund.Amount * 100)
            };
            var stripeRefund = await service.CreateAsync(options, cancellationToken: ct);
            refund.StripeRefundId = stripeRefund.Id;
            refund.Status = RefundStatus.Processed;
            refund.ProcessedAt = DateTime.UtcNow;
        }
        else
        {
            refund.Status = RefundStatus.Denied;
            refund.DeniedAt = DateTime.UtcNow;
            refund.DenialReason = denialReason;
        }

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<RefundDto>(refund);
    }

    public async Task<PaymentDto> CreateReferencePaymentAsync(
    CreateReferencePaymentRequest request, CancellationToken ct)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(request.BookingId, ct)
            ?? throw new NotFoundException("Booking", request.BookingId);

        if (booking.Status != BookingStatus.PendingPayment)
            throw new ConflictException("Booking is not in a payable state.");

        if (booking.HoldExpiresAt < DateTime.UtcNow)
            throw new ConflictException("Booking hold has expired. Please create a new booking.");

        // একই booking-এ duplicate reference submit ঠেকানো
        var existing = await _uow.Payments.GetByBookingIdAsync(request.BookingId, ct);
        if (existing != null && existing.Status is PaymentStatus.PendingApproval or PaymentStatus.Succeeded)
            throw new ConflictException("A payment for this booking already exists.");

        var payment = new Payment
        {
            BookingId = request.BookingId,
            Method = request.Method,
            ReferenceNumber = request.ReferenceNumber,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            Status = PaymentStatus.PendingApproval
        };

        await _uow.Payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> ApproveReferencePaymentAsync(
        Guid id, bool approve, string? rejectionReason, CancellationToken ct)
    {
        var payment = await _uow.Payments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Payment", id);

        if (payment.Status != PaymentStatus.PendingApproval)
            throw new ConflictException("Only payments awaiting approval can be reviewed.");

        if (approve)
        {
            payment.Status = PaymentStatus.Succeeded;
            payment.PaidAt = DateTime.UtcNow;
            payment.ApprovedBy = GetCurrentUserId();
            payment.ApprovedAt = DateTime.UtcNow;

            _uow.Payments.Update(payment);

            var booking = await _uow.Bookings.GetWithDetailsAsync(payment.BookingId, ct);
            if (booking != null)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;
                booking.HoldExpiresAt = null;
                _uow.Bookings.Update(booking);
            }

            await _uow.SaveChangesAsync(ct);

            try
            {
                await _ticketService.GenerateAndPersistTicketsAsync(payment.BookingId, ct);
                await _notification.SendBookingConfirmationAsync(payment.BookingId, ct);
                await _notification.SendPaymentReceiptAsync(payment.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-approval actions failed for booking {BookingId}", payment.BookingId);
            }
        }
        else
        {
            payment.Status = PaymentStatus.Rejected;
            payment.FailureReason = rejectionReason;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync(ct);
        }

        return _mapper.Map<PaymentDto>(payment);
    }
    private async Task ConfirmPaymentInternalAsync(Payment payment, string? chargeId, CancellationToken ct)
    {
        payment.Status = PaymentStatus.Succeeded;
        payment.PaidAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(chargeId))
        {
            var chargeService = new ChargeService();
            var charge = await chargeService.GetAsync(chargeId, cancellationToken: ct);
            payment.ReceiptUrl = charge.ReceiptUrl;
        }

        _uow.Payments.Update(payment);

        var booking = await _uow.Bookings.GetWithDetailsAsync(payment.BookingId, ct);
        if (booking != null)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;
            booking.HoldExpiresAt = null;
            _uow.Bookings.Update(booking);
        }

        await _uow.SaveChangesAsync(ct);

        try
        {
            await _ticketService.GenerateAndPersistTicketsAsync(payment.BookingId, ct);
            await _notification.SendBookingConfirmationAsync(payment.BookingId, ct);
            await _notification.SendPaymentReceiptAsync(payment.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post-payment actions failed for booking {BookingId}", payment.BookingId);
        }
    }
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
