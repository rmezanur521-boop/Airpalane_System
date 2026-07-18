using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IFleetItemService
{
    Task<List<FleetItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<FleetItemDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FleetItemDto> CreateAsync(CreateFleetItemDto dto, CancellationToken ct = default);
    Task<FleetItemDto> UpdateAsync(Guid id, UpdateFleetItemDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default);
}