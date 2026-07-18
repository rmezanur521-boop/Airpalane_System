using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IPopularDestinationService
{
    Task<List<PopularDestinationDto>> GetAllAsync(CancellationToken ct = default);
    Task<PopularDestinationDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PopularDestinationDto> CreateAsync(CreatePopularDestinationDto dto, CancellationToken ct = default);
    Task<PopularDestinationDto> UpdateAsync(Guid id, UpdatePopularDestinationDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default);
}