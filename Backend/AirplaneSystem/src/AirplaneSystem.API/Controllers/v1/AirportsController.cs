using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Repositories;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirplaneSystem.Domain.Entities.Flights;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/airports")]
public class AirportsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AirportsController(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AirportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken ct) =>
        Ok(await _uow.Airports.GetPagedAsync(query, ct)
            .ContinueWith(t => PagedResult<AirportDto>.Create(
                t.Result.Items.Select(a => _mapper.Map<AirportDto>(a)).ToList().AsReadOnly(),
                t.Result.TotalCount, t.Result.PageNumber, t.Result.PageSize), ct));

    [HttpGet("{iataCode}")]
    [ProducesResponseType(typeof(AirportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIata(string iataCode, CancellationToken ct)
    {
        var airport = await _uow.Airports.GetByIataCodeAsync(iataCode.ToUpperInvariant(), ct);
        if (airport == null) return NotFound(new { message = $"Airport '{iataCode}' not found." });
        return Ok(_mapper.Map<AirportDto>(airport));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirportDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAirportRequest request, CancellationToken ct)
    {
        var airport = _mapper.Map<Airport>(request);
        await _uow.Airports.AddAsync(airport, ct);
        await _uow.SaveChangesAsync(ct);
        return StatusCode(StatusCodes.Status201Created, _mapper.Map<AirportDto>(airport));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AirportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateAirportRequest request, CancellationToken ct)
    {
        var airport = await _uow.Airports.GetByIdAsync(id, ct);
        if (airport == null) return NotFound();
        airport.Name = request.Name; airport.City = request.City; airport.Country = request.Country;
        airport.Latitude = request.Latitude; airport.Longitude = request.Longitude;
        _uow.Airports.Update(airport);
        await _uow.SaveChangesAsync(ct);
        return Ok(_mapper.Map<AirportDto>(airport));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var airport = await _uow.Airports.GetByIdAsync(id, ct);
        if (airport == null) return NotFound();
        airport.IsActive = false;
        _uow.Airports.Update(airport);
        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
