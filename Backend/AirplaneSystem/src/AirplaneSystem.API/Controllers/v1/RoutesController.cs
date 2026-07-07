using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Repositories;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

/// <summary>
/// Lightweight, read-only endpoint that exposes Routes with human-readable
/// Origin/Destination names — used to populate the Route dropdown on the
/// Flight create/edit form (Airline & Flight Management module).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/routes")]
public class RoutesController : ControllerBase
{ 
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RoutesController(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    [HttpGet]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(IEnumerable<RouteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var routes = await _uow.Routes.GetAllWithAirportsAsync(ct);
        return Ok(routes.Select(r => _mapper.Map<RouteDto>(r)));
    }
}