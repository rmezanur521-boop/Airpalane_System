using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AirplaneSystem.Application.Services.Implementations;

public class NavbarSettingService : INavbarSettingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cache;

    public NavbarSettingService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _cache = cache;
    }

    public async Task<NavbarSettingDto> GetAsync(CancellationToken ct = default)
    {
        var entity = await _unitOfWork.NavbarSettings.GetSingletonAsync(ct);
        return _mapper.Map<NavbarSettingDto>(entity);
    }

    public async Task<NavbarSettingDto> UpdateAsync(UpdateNavbarSettingDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.NavbarSettings.GetSingletonAsync(ct);
        _mapper.Map(dto, entity);
        _unitOfWork.NavbarSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<NavbarSettingDto>(entity);
    }

    public async Task<NavbarSettingDto> UploadLogoAsync(IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.NavbarSettings.GetSingletonAsync(ct);
        var oldLogo = entity.Logo;

        entity.Logo = await _fileStorageService.SaveAsync(file, "cms/navbar", ct);
        _unitOfWork.NavbarSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        if (!string.IsNullOrWhiteSpace(oldLogo))
            await _fileStorageService.DeleteAsync(oldLogo, ct);

        return _mapper.Map<NavbarSettingDto>(entity);
    }
    public async Task<NavbarSettingDto> UploadFaviconAsync(IFormFile file, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.NavbarSettings.GetSingletonAsync(ct);
        var oldFavicon = entity.FaviconPath;

        entity.FaviconPath = await _fileStorageService.SaveAsync(file, "cms/favicon", ct);
        _unitOfWork.NavbarSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");

        if (!string.IsNullOrWhiteSpace(oldFavicon))
            await _fileStorageService.DeleteAsync(oldFavicon, ct);

        return _mapper.Map<NavbarSettingDto>(entity);
    }
}



public class FooterSettingService : IFooterSettingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    public FooterSettingService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<FooterSettingDto> GetAsync(CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FooterSettings.GetSingletonAsync(ct);
        return _mapper.Map<FooterSettingDto>(entity);
    }

    public async Task<FooterSettingDto> UpdateAsync(UpdateFooterSettingDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.FooterSettings.GetSingletonAsync(ct);
        _mapper.Map(dto, entity);
        _unitOfWork.FooterSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<FooterSettingDto>(entity);
    }
}

public class HomepageSettingService : IHomepageSettingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    public HomepageSettingService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<HomepageSettingDto> GetAsync(CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HomepageSettings.GetSingletonAsync(ct);
        return _mapper.Map<HomepageSettingDto>(entity);
    }

    public async Task<HomepageSettingDto> UpdateAsync(UpdateHomepageSettingDto dto, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.HomepageSettings.GetSingletonAsync(ct);
        _mapper.Map(dto, entity);
        _unitOfWork.HomepageSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        _cache.Remove("homepage:composite");
        return _mapper.Map<HomepageSettingDto>(entity);
    }
}