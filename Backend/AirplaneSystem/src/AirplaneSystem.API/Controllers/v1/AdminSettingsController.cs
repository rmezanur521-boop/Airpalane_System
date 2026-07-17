using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminSettingsController : ControllerBase
{
    private readonly IAdminSettingService _adminSettingService;

    public AdminSettingsController(IAdminSettingService adminSettingService)
    {
        _adminSettingService = adminSettingService;
    }

    /// <summary>
    /// Returns the current company/system settings.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _adminSettingService.GetSettingsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates general company info (name, contact, address, website, footer).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGeneralSettings(
        [FromBody] UpdateAdminSettingDto dto, CancellationToken ct)
    {
        var result = await _adminSettingService.UpdateGeneralSettingsAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates SMTP configuration. Leave "smtpPassword" empty to keep the
    /// existing stored password unchanged.
    /// </summary>
    [HttpPut("smtp")]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSmtpSettings(
        [FromBody] UpdateSmtpSettingDto dto, CancellationToken ct)
    {
        var result = await _adminSettingService.UpdateSmtpSettingsAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Uploads (or replaces) the company logo.
    /// </summary>
    [HttpPost("logo")]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLogo(
        [FromForm] IFormFile file, CancellationToken ct)
    {
        var result = await _adminSettingService.UploadLogoAsync(file, ct);
        return Ok(result);
    }

    /// <summary>
    /// Removes the current company logo.
    /// </summary>
    [HttpDelete("logo")]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteLogo(CancellationToken ct)
    {
        var result = await _adminSettingService.DeleteLogoAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Uploads (or replaces) the favicon.
    /// </summary>
    [HttpPost("favicon")]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadFavicon(
        [FromForm] IFormFile file, CancellationToken ct)
    {
        var result = await _adminSettingService.UploadFaviconAsync(file, ct);
        return Ok(result);
    }

    /// <summary>
    /// Removes the current favicon.
    /// </summary>
    [HttpDelete("favicon")]
    [ProducesResponseType(typeof(AdminSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteFavicon(CancellationToken ct)
    {
        var result = await _adminSettingService.DeleteFaviconAsync(ct);
        return Ok(result);
    }
}