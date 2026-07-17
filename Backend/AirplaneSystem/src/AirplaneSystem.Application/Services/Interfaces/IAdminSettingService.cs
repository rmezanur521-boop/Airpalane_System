using AirplaneSystem.Application.DTOs.Admin;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IAdminSettingService
{
    Task<AdminSettingDto> GetSettingsAsync(CancellationToken ct = default);

    Task<AdminSettingDto> UpdateGeneralSettingsAsync(
        UpdateAdminSettingDto dto, CancellationToken ct = default);

    Task<AdminSettingDto> UpdateSmtpSettingsAsync(
        UpdateSmtpSettingDto dto, CancellationToken ct = default);

    Task<AdminSettingDto> UploadLogoAsync(
        IFormFile file, CancellationToken ct = default);

    Task<AdminSettingDto> DeleteLogoAsync(CancellationToken ct = default);

    Task<AdminSettingDto> UploadFaviconAsync(
        IFormFile file, CancellationToken ct = default);

    Task<AdminSettingDto> DeleteFaviconAsync(CancellationToken ct = default);
}