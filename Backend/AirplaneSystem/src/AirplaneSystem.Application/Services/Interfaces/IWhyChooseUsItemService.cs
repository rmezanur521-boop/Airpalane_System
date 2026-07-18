using AirplaneSystem.Application.DTOs.Cms;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IWhyChooseUsItemService
{
    Task<List<WhyChooseUsItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<WhyChooseUsItemDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WhyChooseUsItemDto> CreateAsync(CreateWhyChooseUsItemDto dto, CancellationToken ct = default);
    Task<WhyChooseUsItemDto> UpdateAsync(Guid id, UpdateWhyChooseUsItemDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(ReorderRequestDto request, CancellationToken ct = default);
}