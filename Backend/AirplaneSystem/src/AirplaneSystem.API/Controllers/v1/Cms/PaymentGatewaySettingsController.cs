using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/payment-gateways")]
[Authorize(Roles = "Admin")]
public class PaymentGatewaySettingsController : ControllerBase
{
    private readonly IPaymentGatewaySettingService _service;

    public PaymentGatewaySettingsController(IPaymentGatewaySettingService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentGatewaySettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _service.GetAllAsync(ct));

    [HttpPut("{provider}")]
    [ProducesResponseType(typeof(PaymentGatewaySettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string provider,
        [FromBody] UpdatePaymentGatewaySettingRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = Guid.TryParse(userIdClaim, out var id) ? id : null;
        return Ok(await _service.UpdateAsync(provider, request, userId, ct));
    }
}