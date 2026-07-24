using AirplaneSystem.Application.DTOs.Cms;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface ISmtpSettingService
{
    Task<SmtpSettingDto> GetAsync(CancellationToken ct = default);
    Task<SmtpSettingDto> UpdateSmtpSettingsAsync(UpdateSmtpSettingDto dto, CancellationToken ct = default);
}