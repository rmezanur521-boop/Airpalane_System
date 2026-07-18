using AirplaneSystem.Application.DTOs.Cms;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface ISpecialOfferService
{
    Task<List<SpecialOfferDto>> GetAllAsync(CancellationToken ct = default);
    Task<SpecialOfferDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SpecialOfferDto> CreateAsync(CreateSpecialOfferDto dto, CancellationToken ct = default);
    Task<SpecialOfferDto> UpdateAsync(Guid id, UpdateSpecialOfferDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default);
}