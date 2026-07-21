using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.DTOs.Users;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IFlightService _flightService;
    private readonly INotificationService _notification;

    public AdminController(IAdminService adminService, IFlightService flightService, INotificationService notification)
    {
        _adminService = adminService;
        _flightService = flightService;
        _notification = notification;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        Ok(await _adminService.GetDashboardAsync(from, to, ct));

    [HttpGet("reports/revenue")]
    [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _adminService.GetRevenueReportAsync(from, to, ct));

    [HttpGet("reports/bookings")]
    [ProducesResponseType(typeof(BookingReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookingReport(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(await _adminService.GetBookingReportAsync(from, to, ct));

    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQuery query, CancellationToken ct) =>
        Ok(await _adminService.GetAuditLogsAsync(query, ct));

    [HttpPost("agents")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request, CancellationToken ct)
    {
        var agent = await _adminService.CreateAgentAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, agent);
    }

    [HttpPost("notifications/flight-alert")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendFlightAlert([FromBody] FlightAlertRequest request, CancellationToken ct)
    {
        if (request.IsCancellation)
            await _notification.SendFlightCancellationAlertAsync(request.FlightId, ct);
        else
            await _notification.SendFlightDelayAlertAsync(request.FlightId, ct);
        return NoContent();
    }
}

public class FlightAlertRequest
{
    public Guid FlightId { get; set; }
    public bool IsCancellation { get; set; } = false;
}
