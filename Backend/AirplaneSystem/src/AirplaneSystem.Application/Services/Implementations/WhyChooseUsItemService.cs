using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Entities.Cms;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Application.Services.Implementations;

public class WhyChooseUsItemService : IWhyChooseUsItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public WhyChooseUsItemService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<List<WhyChooseUsItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _unitOfWork.WhyChooseUsItems.Query()
            .AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct);
        return _mapper.Map<List<WhyChooseUsItemDto>>(items);
    }

    public async Task<WhyChooseUsItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.WhyChooseUsItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(WhyChooseUsItem), id);
        return _mapper.Map<WhyChooseUsItemDto>(entity);
    }

    public async Task<WhyChooseUsItemDto> CreateAsync(CreateWhyChooseUsItemDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<WhyChooseUsItem>(dto);
        await _unitOfWork.WhyChooseUsItems.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<WhyChooseUsItemDto>(entity);
    }

    public async Task<WhyChooseUsItemDto> UpdateAsync(Guid id, UpdateWhyChooseUsItemDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.WhyChooseUsItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(WhyChooseUsItem), id);
        _mapper.Map(dto, entity);
        _unitOfWork.WhyChooseUsItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<WhyChooseUsItemDto>(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.WhyChooseUsItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(WhyChooseUsItem), id);
        entity.IsDeleted = true;
        _unitOfWork.WhyChooseUsItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
    }
}