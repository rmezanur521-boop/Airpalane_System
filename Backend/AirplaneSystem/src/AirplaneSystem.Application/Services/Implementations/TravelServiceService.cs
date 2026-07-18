using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TravelServiceEntity = AirplaneSystem.Domain.Entities.Cms.TravelService;

namespace AirplaneSystem.Application.Services.Implementations;

public class TravelServiceService : ITravelServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache;

    public TravelServiceService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<List<TravelServiceDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.TravelServices.Query()
            .AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct);
        return _mapper.Map<List<TravelServiceDto>>(items);
    }

    public async Task<TravelServiceDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.TravelServices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(TravelServiceEntity), id);
        return _mapper.Map<TravelServiceDto>(entity);
    }

    public async Task<TravelServiceDto> CreateAsync(CreateTravelServiceDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<TravelServiceEntity>(dto);
        await _unitOfWork.TravelServices.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<TravelServiceDto>(entity);
    }

    public async Task<TravelServiceDto> UpdateAsync(Guid id, UpdateTravelServiceDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.TravelServices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(TravelServiceEntity), id);
        _mapper.Map(dto, entity);
        _unitOfWork.TravelServices.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<TravelServiceDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.TravelServices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(TravelServiceEntity), id);
        entity.IsDeleted = true;
        _unitOfWork.TravelServices.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }

    public async Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.TravelServices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(TravelServiceEntity), id);
        var oldImage = entity.Image;
        entity.Image = await _fileStorageService.SaveAsync(file, "cms/services", ct);
        _unitOfWork.TravelServices.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        if (!string.IsNullOrWhiteSpace(oldImage)) await _fileStorageService.DeleteAsync(oldImage, ct);
        return entity.Image;
    }
}