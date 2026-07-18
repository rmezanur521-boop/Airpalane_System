using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Cms;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Application.Services.Implementations;

public class AnnouncementBarService : IAnnouncementBarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public AnnouncementBarService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<List<AnnouncementBarDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.AnnouncementBars.Query()
            .AsNoTracking().OrderByDescending(x => x.Priority).ToListAsync(ct);
        return _mapper.Map<List<AnnouncementBarDto>>(items);
    }

    public async Task<AnnouncementBarDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.AnnouncementBars.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(AnnouncementBar), id);
        return _mapper.Map<AnnouncementBarDto>(entity);
    }

    public async Task<AnnouncementBarDto> CreateAsync(CreateAnnouncementBarDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<AnnouncementBar>(dto);
        await _unitOfWork.AnnouncementBars.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<AnnouncementBarDto>(entity);
    }

    public async Task<AnnouncementBarDto> UpdateAsync(Guid id, UpdateAnnouncementBarDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.AnnouncementBars.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(AnnouncementBar), id);
        _mapper.Map(dto, entity);
        _unitOfWork.AnnouncementBars.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<AnnouncementBarDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.AnnouncementBars.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(AnnouncementBar), id);
        entity.IsDeleted = true;
        _unitOfWork.AnnouncementBars.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }
}