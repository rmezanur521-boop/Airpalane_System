using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Domain.Entities.Audit;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
