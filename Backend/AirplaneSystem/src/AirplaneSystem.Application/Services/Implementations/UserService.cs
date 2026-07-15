using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Users;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Users;
using AutoMapper;
using System.Diagnostics.CodeAnalysis;

namespace AirplaneSystem.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User", id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var users = await _uow.Users.GetAllAsync(ct);
        var filtered = string.IsNullOrWhiteSpace(query.SearchTerm) ? users :
            users.Where(u => u.Email.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)
                || u.FirstName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)
                || u.LastName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        var total = filtered.Count();
        var items = filtered
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => _mapper.Map<UserDto>(u))
            .ToList();

        return PagedResult<UserDto>.Create(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        
        if (request.ProfilePictureUrl != null)
            user.ProfilePictureUrl = request.ProfilePictureUrl;

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth.Value;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }
    public async Task<PassportDto?> GetPassportAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithPassportAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
        return user.PassportInfo == null ? null : _mapper.Map<PassportDto>(user.PassportInfo);
    }
    public async Task<PassportDto> UpdatePassportAsync(Guid userId, PassportDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithPassportAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (user.PassportInfo == null)
        {
            user.PassportInfo = new PassportInfo
            {
                UserId = userId,
                PassportNumber = request.PassportNumber,
                IssuingCountry = request.IssuingCountry,
                IssuedDate = request.IssuedDate,
                ExpiryDate = request.ExpiryDate
            };
        }
        else
        {
            user.PassportInfo.PassportNumber = request.PassportNumber;
            user.PassportInfo.IssuingCountry = request.IssuingCountry;
            user.PassportInfo.IssuedDate = request.IssuedDate;
            user.PassportInfo.ExpiryDate = request.ExpiryDate;
        }

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<PassportDto>(user.PassportInfo);
    }

    public async Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
        user.IsActive = isActive;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
        user.IsDeleted = true;
        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    
}
