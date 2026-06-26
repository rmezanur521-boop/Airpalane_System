using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Payments;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("intent")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentIntentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentRequest request, CancellationToken ct) =>
        Ok(await _paymentService.CreatePaymentIntentAsync(request.BookingId, ct));

    [HttpPost("confirm")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request, CancellationToken ct) =>
        Ok(await _paymentService.ConfirmPaymentAsync(request.PaymentIntentId, ct));

    [HttpPost("promo/validate")]
    [Authorize]
    [ProducesResponseType(typeof(PromoValidationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePromo([FromBody] PromoValidationRequest request, CancellationToken ct) =>
        Ok(await _paymentService.ValidatePromoCodeAsync(request, ct));

    [HttpPost("refund")]
    [Authorize]
    [ProducesResponseType(typeof(RefundDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestRefund([FromBody] RefundRequestDto request, CancellationToken ct) =>
        Ok(await _paymentService.RequestRefundAsync(request, ct));

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await _paymentService.GetByIdAsync(id, ct));

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken ct) =>
        Ok(await _paymentService.GetAllAsync(query, ct));

    [HttpPatch("refund/{id:guid}/process")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RefundDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] ProcessRefundRequest request, CancellationToken ct) =>
        Ok(await _paymentService.ProcessRefundAsync(id, request.Approve, request.DenialReason, ct));

    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        try
        {
            await _paymentService.ProcessWebhookAsync(payload, signature, ct);
            return Ok(new { received = true });
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest(new { error = "Invalid webhook signature." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing error");
            return Ok(new { received = true }); // Return 200 to prevent Stripe retries
        }
    }
}

public class ProcessRefundRequest
{
    public bool Approve { get; set; }
    public string? DenialReason { get; set; }
}
