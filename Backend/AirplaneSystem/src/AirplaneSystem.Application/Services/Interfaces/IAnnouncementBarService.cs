using AirplaneSystem.Application.DTOs.Cms;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IAnnouncementBarService
{
    Task<List<AnnouncementBarDto>> GetAllAsync(CancellationToken ct = default);
    Task<AnnouncementBarDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AnnouncementBarDto> CreateAsync(CreateAnnouncementBarDto dto, CancellationToken ct = default);
    Task<AnnouncementBarDto> UpdateAsync(Guid id, UpdateAnnouncementBarDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(ReorderRequestDto request, CancellationToken ct = default);
}