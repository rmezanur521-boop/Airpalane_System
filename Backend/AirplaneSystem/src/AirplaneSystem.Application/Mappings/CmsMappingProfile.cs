using AirplaneSystem.Application.DTOs.Cms;
using AirplaneSystem.Domain.Entities.Cms;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class CmsMappingProfile : Profile
{
    public CmsMappingProfile()
    {
        CreateMap<HeroSection, HeroSectionDto>();
        CreateMap<CreateHeroSectionDto, HeroSection>();
        CreateMap<UpdateHeroSectionDto, HeroSection>();
        CreateMap<NavbarSetting, NavbarSettingDto>();
        CreateMap<UpdateNavbarSettingDto, NavbarSetting>();

        CreateMap<FooterSetting, FooterSettingDto>();
        CreateMap<UpdateFooterSettingDto, FooterSetting>();

        CreateMap<HomepageSetting, HomepageSettingDto>();
        CreateMap<UpdateHomepageSettingDto, HomepageSetting>();

        CreateMap<SpecialOffer, SpecialOfferDto>();
        CreateMap<CreateSpecialOfferDto, SpecialOffer>();
        CreateMap<UpdateSpecialOfferDto, SpecialOffer>();

        CreateMap<PopularDestination, PopularDestinationDto>();
        CreateMap<CreatePopularDestinationDto, PopularDestination>();
        CreateMap<UpdatePopularDestinationDto, PopularDestination>();

        CreateMap<WhyChooseUsItem, WhyChooseUsItemDto>();
        CreateMap<CreateWhyChooseUsItemDto, WhyChooseUsItem>();
        CreateMap<UpdateWhyChooseUsItemDto, WhyChooseUsItem>();

        CreateMap<FleetItem, FleetItemDto>();
        CreateMap<CreateFleetItemDto, FleetItem>();
        CreateMap<UpdateFleetItemDto, FleetItem>();

        CreateMap<AirplaneSystem.Domain.Entities.Cms.TravelService, TravelServiceDto>();
        CreateMap<CreateTravelServiceDto, AirplaneSystem.Domain.Entities.Cms.TravelService>();
        CreateMap<UpdateTravelServiceDto, AirplaneSystem.Domain.Entities.Cms.TravelService>();

        CreateMap<AnnouncementBar, AnnouncementBarDto>();
        CreateMap<CreateAnnouncementBarDto, AnnouncementBar>();
        CreateMap<UpdateAnnouncementBarDto, AnnouncementBar>();
    }
}
