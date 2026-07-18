using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface ITravelServiceService
{
    Task<List<TravelServiceDto>> GetAllAsync(CancellationToken ct = default);
    Task<TravelServiceDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TravelServiceDto> CreateAsync(CreateTravelServiceDto dto, CancellationToken ct = default);
    Task<TravelServiceDto> UpdateAsync(Guid id, UpdateTravelServiceDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default);
    Task ReorderAsync(ReorderRequestDto request, CancellationToken ct = default);
}