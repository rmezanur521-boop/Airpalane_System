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

public class HeroSectionService : IHeroSectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache; 

    public HeroSectionService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<List<HeroSectionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.HeroSections.Query()
            .AsNoTracking()
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync(ct);

        return _mapper.Map<List<HeroSectionDto>>(items);
    }

    public async Task<HeroSectionDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HeroSections.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(HeroSection), id);

        return _mapper.Map<HeroSectionDto>(entity);
    }

    public async Task<HeroSectionDto> CreateAsync(CreateHeroSectionDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<HeroSection>(dto);
        await _unitOfWork.HeroSections.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        return _mapper.Map<HeroSectionDto>(entity);
    }

    public async Task<HeroSectionDto> UpdateAsync(Guid id, UpdateHeroSectionDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HeroSections.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(HeroSection), id);

        _mapper.Map(dto, entity);
        _unitOfWork.HeroSections.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        return _mapper.Map<HeroSectionDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HeroSections.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(HeroSection), id);

        entity.IsDeleted = true; // Soft Delete — HasQueryFilter দিয়ে List থেকে বাদ পড়ে যাবে
        _unitOfWork.HeroSections.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }

    public async Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HeroSections.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(HeroSection), id);

        var oldImage = entity.BackgroundImage;
        var url = await _fileStorageService.SaveAsync(file, "cms/hero", ct);

        entity.BackgroundImage = url;
        _unitOfWork.HeroSections.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        if (!string.IsNullOrWhiteSpace(oldImage))
            await _fileStorageService.DeleteAsync(oldImage, ct); // পুরনো Orphan File Cleanup

        return url;
    }
}