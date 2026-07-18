using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Cms;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Application.Services.Implementations;

public class FleetItemService : IFleetItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache;

    public FleetItemService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<List<FleetItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.FleetItems.Query()
            .AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct);
        return _mapper.Map<List<FleetItemDto>>(items);
    }

    public async Task<FleetItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FleetItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(FleetItem), id);
        return _mapper.Map<FleetItemDto>(entity);
    }

    public async Task<FleetItemDto> CreateAsync(CreateFleetItemDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<FleetItem>(dto);
        await _unitOfWork.FleetItems.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<FleetItemDto>(entity);
    }

    public async Task<FleetItemDto> UpdateAsync(Guid id, UpdateFleetItemDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FleetItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(FleetItem), id);
        _mapper.Map(dto, entity);
        _unitOfWork.FleetItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<FleetItemDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FleetItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(FleetItem), id);
        entity.IsDeleted = true;
        _unitOfWork.FleetItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }

    public async Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FleetItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(FleetItem), id);
        var oldImage = entity.Image;
        entity.Image = await _fileStorageService.SaveAsync(file, "cms/fleet", ct);
        _unitOfWork.FleetItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        if (!string.IsNullOrWhiteSpace(oldImage)) await _fileStorageService.DeleteAsync(oldImage, ct);
        return entity.Image;
    }
}