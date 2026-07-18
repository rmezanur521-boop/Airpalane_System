using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/homepage-settings")]
[Authorize(Roles = "Admin")]
public class HomepageSettingController : ControllerBase
{
    private readonly IHomepageSettingService _service;
    public HomepageSettingController(IHomepageSettingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await _service.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateHomepageSettingDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(dto, ct));
}