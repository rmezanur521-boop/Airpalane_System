using AirplaneSystem.Application.Repositories;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/airlines")]
public class AirlinesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AirlinesController(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var airlines = await _uow.Airlines.GetAllAsync(ct);
        return Ok(airlines.Select(a => new
        {
            a.Id, a.IataCode, a.Name, a.Country, a.LogoUrl, a.ContactEmail, a.IsActive
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdAsync(id, ct);
        if (airline == null) return NotFound();
        return Ok(new { airline.Id, airline.IataCode, airline.Name, airline.Country, airline.LogoUrl, airline.IsActive });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAirlineRequest request, CancellationToken ct)
    {
        var airline = new AirplaneSystem.Domain.Entities.Flights.Airline
        {
            IataCode = request.IataCode.ToUpperInvariant(),
            Name = request.Name,
            Country = request.Country,
            LogoUrl = request.LogoUrl,
            ContactEmail = request.ContactEmail
        };
        await _uow.Airlines.AddAsync(airline, ct);
        await _uow.SaveChangesAsync(ct);
        return StatusCode(201, new { airline.Id, airline.IataCode, airline.Name });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateAirlineRequest request, CancellationToken ct)
    {
        var airline = await _uow.Airlines.GetByIdAsync(id, ct);
        if (airline == null) return NotFound();
        airline.Name = request.Name; airline.Country = request.Country;
        airline.LogoUrl = request.LogoUrl; airline.ContactEmail = request.ContactEmail;
        _uow.Airlines.Update(airline);
        await _uow.SaveChangesAsync(ct);
        return Ok(new { airline.Id, airline.IataCode, airline.Name });
    }
}

public class CreateAirlineRequest
{
    public string IataCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
}
