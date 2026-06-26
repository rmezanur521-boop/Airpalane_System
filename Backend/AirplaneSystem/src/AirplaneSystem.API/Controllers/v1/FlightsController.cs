using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService) => _flightService = flightService;

    [HttpGet]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(PagedResult<FlightDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken ct) =>
        Ok(await _flightService.GetAllAsync(query, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await _flightService.GetByIdAsync(id, ct));

    [HttpGet("{id:guid}/seats")]
    [Authorize]
    [ProducesResponseType(typeof(SeatMapDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeatMap(Guid id, CancellationToken ct) =>
        Ok(await _flightService.GetSeatMapAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFlightRequest request, CancellationToken ct)
    {
        var flight = await _flightService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, flight);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateFlightRequest request, CancellationToken ct) =>
        Ok(await _flightService.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateFlightStatusRequest request, CancellationToken ct)
    {
        await _flightService.UpdateStatusAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _flightService.DeleteAsync(id, ct);
        return NoContent();
    }
}
