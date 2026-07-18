using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Domain.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Application.Services.Implementations;

public class HomepageService : IHomepageService
{
    private const string CacheKey = "homepage:composite";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public HomepageService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<HomepageResponseDto> GetHomepageDataAsync(CancellationToken ct = default)
    {
        if (_cache.TryGet<HomepageResponseDto>(CacheKey, out var cached) && cached != null)
            return cached;

        var settings = await _unitOfWork.HomepageSettings.GetSingletonAsync(ct);
        var now = DateTime.UtcNow;

        var result = new HomepageResponseDto
        {
            Navbar = _mapper.Map<NavbarSettingDto>(await _unitOfWork.NavbarSettings.GetSingletonAsync(ct)),
            Footer = settings.ShowFooter
                ? _mapper.Map<FooterSettingDto>(await _unitOfWork.FooterSettings.GetSingletonAsync(ct))
                : null!,

            Hero = settings.ShowHero
                ? _mapper.Map<List<HeroSectionDto>>(
                    await _unitOfWork.HeroSections.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active)
                        .OrderBy(x => x.DisplayOrder).ToListAsync(ct))
                : new(),

            Offers = settings.ShowOffers
                ? _mapper.Map<List<SpecialOfferDto>>(
                    await _unitOfWork.SpecialOffers.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active
                            && (x.StartDate == null || x.StartDate <= now)
                            && (x.EndDate == null || x.EndDate >= now))
                        .OrderByDescending(x => x.Priority).ToListAsync(ct))
                : new(),

            Destinations = settings.ShowDestinations
                ? _mapper.Map<List<PopularDestinationDto>>(
                    await _unitOfWork.PopularDestinations.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active)
                        .OrderBy(x => x.DisplayOrder).ToListAsync(ct))
                : new(),

            WhyChooseUs = settings.ShowWhyChooseUs
                ? _mapper.Map<List<WhyChooseUsItemDto>>(
                    await _unitOfWork.WhyChooseUsItems.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active)
                        .OrderBy(x => x.DisplayOrder).ToListAsync(ct))
                : new(),

            Fleet = settings.ShowFleet
                ? _mapper.Map<List<FleetItemDto>>(
                    await _unitOfWork.FleetItems.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active)
                        .OrderBy(x => x.DisplayOrder).ToListAsync(ct))
                : new(),

            Services = settings.ShowServices
                ? _mapper.Map<List<TravelServiceDto>>(
                    await _unitOfWork.TravelServices.Query().AsNoTracking()
                        .Where(x => x.Status == ContentStatus.Active)
                        .OrderBy(x => x.DisplayOrder).ToListAsync(ct))
                : new()
        };

        var announcement = await _unitOfWork.AnnouncementBars.Query().AsNoTracking()
            .Where(x => x.Status == ContentStatus.Active
                && (x.StartDate == null || x.StartDate <= now)
                && (x.EndDate == null || x.EndDate >= now))
            .OrderByDescending(x => x.Priority)
            .FirstOrDefaultAsync(ct);

        result.Announcement = announcement != null ? _mapper.Map<AnnouncementBarDto>(announcement) : null;

        _cache.Set(CacheKey, result, CacheDuration);
        return result;
    }
}