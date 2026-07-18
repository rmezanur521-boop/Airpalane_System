using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

/// <summary>
/// Public composite endpoint — Frontend শুধু একটা Call করলেই পুরো Homepage-এর Data পাবে।
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/homepage")]
[AllowAnonymous]
public class HomepageController : ControllerBase
{
    private readonly IHomepageService _homepageService;
    public HomepageController(IHomepageService homepageService) => _homepageService = homepageService;

    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(HomepageResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _homepageService.GetHomepageDataAsync(ct));
}