using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/navbar-settings")]
[Authorize(Roles = "Admin")]
public class NavbarSettingController : ControllerBase
{
    private readonly INavbarSettingService _service;
    public NavbarSettingController(INavbarSettingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await _service.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateNavbarSettingDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(dto, ct));

    [HttpPost("logo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo([FromForm] UploadFileRequest request, CancellationToken ct)
        => Ok(await _service.UploadLogoAsync(request.File, ct));
}