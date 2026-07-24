using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirplaneSystem.API.Controllers.v1.Cms
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/cms/smtp-settings")]
    [Authorize(Roles = "Admin")]
    public class SmtpSettingController : ControllerBase
    {
        private readonly ISmtpSettingService _smtpSettingService;

        public SmtpSettingController(ISmtpSettingService smtpSettingService)
        {
            _smtpSettingService = smtpSettingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SmtpSettingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSmtpSettings(CancellationToken ct)
        {
            var result = await _smtpSettingService.GetAsync(ct);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(SmtpSettingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSmtpSettings([FromBody] UpdateSmtpSettingDto dto, CancellationToken ct)
        {
            var result = await _smtpSettingService.UpdateSmtpSettingsAsync(dto, ct);
            return Ok(result);
        }
    }
}