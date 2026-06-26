using AirplaneSystem.Application.DTOs.Tickets;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService) => _ticketService = ticketService;

    [HttpGet("booking/{bookingId:guid}")]
    [ProducesResponseType(typeof(List<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBooking(Guid bookingId, CancellationToken ct) =>
        Ok(await _ticketService.GetByBookingAsync(bookingId, ct));

    [HttpGet("{ticketNumber}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string ticketNumber, CancellationToken ct) =>
        Ok(await _ticketService.GetByTicketNumberAsync(ticketNumber, ct));

    [HttpGet("{ticketNumber}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf(string ticketNumber, CancellationToken ct)
    {
        var bytes = await _ticketService.GeneratePdfAsync(ticketNumber, ct);
        return File(bytes, "application/pdf", $"ticket-{ticketNumber}.pdf");
    }

    [HttpGet("{ticketNumber}/boarding-pass")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoardingPass(string ticketNumber, CancellationToken ct)
    {
        var bytes = await _ticketService.GenerateBoardingPassAsync(ticketNumber, ct);
        return File(bytes, "application/pdf", $"boarding-pass-{ticketNumber}.pdf");
    }

    [HttpPost("{ticketNumber}/check-in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(string ticketNumber, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _ticketService.CheckInAsync(ticketNumber, userId, ct);
        return NoContent();
    }
}
