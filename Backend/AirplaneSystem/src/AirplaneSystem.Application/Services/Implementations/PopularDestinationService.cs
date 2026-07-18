using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Cms;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Application.Services.Implementations;

public class PopularDestinationService : IPopularDestinationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache;

    public PopularDestinationService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<List<PopularDestinationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.PopularDestinations.Query()
            .AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct);
        return _mapper.Map<List<PopularDestinationDto>>(items);
    }

    public async Task<PopularDestinationDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.PopularDestinations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(PopularDestination), id);
        return _mapper.Map<PopularDestinationDto>(entity);
    }

    public async Task<PopularDestinationDto> CreateAsync(CreatePopularDestinationDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<PopularDestination>(dto);
        await _unitOfWork.PopularDestinations.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<PopularDestinationDto>(entity);
    }

    public async Task<PopularDestinationDto> UpdateAsync(Guid id, UpdatePopularDestinationDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.PopularDestinations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(PopularDestination), id);
        _mapper.Map(dto, entity);
        _unitOfWork.PopularDestinations.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<PopularDestinationDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.PopularDestinations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(PopularDestination), id);
        entity.IsDeleted = true;
        _unitOfWork.PopularDestinations.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }

    public async Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.PopularDestinations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(PopularDestination), id);
        var oldImage = entity.Image;
        entity.Image = await _fileStorageService.SaveAsync(file, "cms/destinations", ct);
        _unitOfWork.PopularDestinations.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        if (!string.IsNullOrWhiteSpace(oldImage)) await _fileStorageService.DeleteAsync(oldImage, ct);
        return entity.Image;
    }
}