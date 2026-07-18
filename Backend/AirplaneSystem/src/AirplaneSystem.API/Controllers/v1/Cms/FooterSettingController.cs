using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/footer-settings")]
[Authorize(Roles = "Admin")]
public class FooterSettingController : ControllerBase
{
    private readonly IFooterSettingService _service;
    public FooterSettingController(IFooterSettingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await _service.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateFooterSettingDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(dto, ct));
}