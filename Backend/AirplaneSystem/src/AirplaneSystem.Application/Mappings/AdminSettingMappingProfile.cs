using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Domain.Entities.Settings;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class AdminSettingMappingProfile : Profile
{
    public AdminSettingMappingProfile()
    {
        CreateMap<AdminSetting, AdminSettingDto>()
            .ForMember(dest => dest.CompanyLogoUrl, opt => opt.MapFrom(src => src.CompanyLogoPath))
            .ForMember(dest => dest.FaviconUrl, opt => opt.MapFrom(src => src.FaviconPath))
            .ForMember(dest => dest.IsSmtpPasswordConfigured,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.SmtpPasswordEncrypted)));
    }
}