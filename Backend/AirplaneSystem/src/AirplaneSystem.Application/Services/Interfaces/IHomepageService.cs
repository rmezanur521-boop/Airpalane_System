using AirplaneSystem.Application.DTOs.Cms;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IHomepageService
{
    Task<HomepageResponseDto> GetHomepageDataAsync(CancellationToken ct = default);
}