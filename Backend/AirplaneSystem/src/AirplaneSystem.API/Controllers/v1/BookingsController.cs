using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Bookings;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _bookingService.CreateAsync(userId, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBookings([FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _bookingService.GetUserBookingsAsync(userId, query, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        return Ok(await _bookingService.GetByIdAsync(userId, id, ct));
    }

    [HttpGet("reference/{reference}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string reference, CancellationToken ct) =>
        Ok(await _bookingService.GetByReferenceAsync(reference.ToUpperInvariant(), ct));

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        await _bookingService.CancelAsync(userId, id, request.Reason, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/select-seat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SelectSeat(Guid id, [FromBody] SelectSeatRequest request, CancellationToken ct)
    {
        await _bookingService.SelectSeatAsync(id, request, ct);
        return NoContent();
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken ct) =>
        Ok(await _bookingService.GetAllAsync(query, ct));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
