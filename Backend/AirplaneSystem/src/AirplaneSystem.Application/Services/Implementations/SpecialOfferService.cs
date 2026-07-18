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

public class SpecialOfferService : ISpecialOfferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache;

    public SpecialOfferService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<List<SpecialOfferDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.SpecialOffers.Query()
            .AsNoTracking().OrderByDescending(x => x.Priority).ToListAsync(ct);
        return _mapper.Map<List<SpecialOfferDto>>(items);
    }

    public async Task<SpecialOfferDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.SpecialOffers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(SpecialOffer), id);
        return _mapper.Map<SpecialOfferDto>(entity);
    }

    public async Task<SpecialOfferDto> CreateAsync(CreateSpecialOfferDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<SpecialOffer>(dto);
        await _unitOfWork.SpecialOffers.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<SpecialOfferDto>(entity);
    }

    public async Task<SpecialOfferDto> UpdateAsync(Guid id, UpdateSpecialOfferDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.SpecialOffers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(SpecialOffer), id);
        _mapper.Map(dto, entity);
        _unitOfWork.SpecialOffers.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<SpecialOfferDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.SpecialOffers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(SpecialOffer), id);
        entity.IsDeleted = true;
        _unitOfWork.SpecialOffers.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }

    public async Task<string> UploadImageAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.SpecialOffers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(SpecialOffer), id);
        var oldImage = entity.OfferImage;
        entity.OfferImage = await _fileStorageService.SaveAsync(file, "cms/offers", ct);
        _unitOfWork.SpecialOffers.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        if (!string.IsNullOrWhiteSpace(oldImage)) await _fileStorageService.DeleteAsync(oldImage, ct);
        return entity.OfferImage;
    }
}