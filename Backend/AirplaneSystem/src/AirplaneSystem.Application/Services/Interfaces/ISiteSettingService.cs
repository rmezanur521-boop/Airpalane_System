using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface INavbarSettingService
{
    Task<NavbarSettingDto> GetAsync(CancellationToken ct = default);
    Task<NavbarSettingDto> UpdateAsync(UpdateNavbarSettingDto dto, CancellationToken ct = default);
    Task<NavbarSettingDto> UploadLogoAsync(IFormFile file, CancellationToken ct = default);
    Task<NavbarSettingDto> UploadFaviconAsync(IFormFile file, CancellationToken ct = default);
}

public interface IFooterSettingService
{
    Task<FooterSettingDto> GetAsync(CancellationToken ct = default);
    Task<FooterSettingDto> UpdateAsync(UpdateFooterSettingDto dto, CancellationToken ct = default);
}

public interface IHomepageSettingService
{
    Task<HomepageSettingDto> GetAsync(CancellationToken ct = default);
    Task<HomepageSettingDto> UpdateAsync(UpdateHomepageSettingDto dto, CancellationToken ct = default);
}