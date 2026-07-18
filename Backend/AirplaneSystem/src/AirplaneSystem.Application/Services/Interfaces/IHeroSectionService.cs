using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IHeroSectionService
{
    Task<List<HeroSectionDto>> GetAllAsync(CancellationToken ct = default);
    Task<HeroSectionDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HeroSectionDto> CreateAsync(CreateHeroSectionDto dto, CancellationToken ct = default);
    Task<HeroSectionDto> UpdateAsync(Guid id, UpdateHeroSectionDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default);
}