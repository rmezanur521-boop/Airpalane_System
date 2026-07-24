using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService) => _searchService = searchService;

    [HttpPost("one-way")]
    [ProducesResponseType(typeof(List<FlightSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchOneWay([FromBody] FlightSearchRequest request, CancellationToken ct) =>
        Ok(await _searchService.SearchOneWayAsync(request, ct));

    [HttpPost("round-trip")]
    [ProducesResponseType(typeof(List<RoundTripResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchRoundTrip([FromBody] FlightSearchRequest request, CancellationToken ct) =>
        Ok(await _searchService.SearchRoundTripAsync(request, ct));

    [HttpPost("multi-city")]
    [ProducesResponseType(typeof(List<MultiCityResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchMultiCity([FromBody] MultiCitySearchRequest request, CancellationToken ct) =>
        Ok(await _searchService.SearchMultiCityAsync(request, ct));
}
