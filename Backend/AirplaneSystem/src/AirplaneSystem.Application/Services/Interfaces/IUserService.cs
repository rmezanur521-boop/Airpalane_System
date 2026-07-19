using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Users;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateProfileImageAsync(Guid userId, IFormFile file, CancellationToken ct = default);
    Task<PassportDto> UpdatePassportAsync(Guid userId,PassportDto request, CancellationToken ct = default);
    Task<PassportDto?> GetPassportAsync(Guid userId, CancellationToken ct = default);
    Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid userId, CancellationToken ct = default);
}
