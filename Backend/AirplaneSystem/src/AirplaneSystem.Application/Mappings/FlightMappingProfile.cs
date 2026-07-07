using AirplaneSystem.Application.DTOs.Flights;
using AirplaneSystem.Domain.Entities.Flights;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class FlightMappingProfile : Profile
{
    public FlightMappingProfile()
    {
        CreateMap<Flight, FlightDto>()
            .ForMember(d => d.AirlineName, o => o.MapFrom(s => s.Airline.Name))
            .ForMember(d => d.AirlineIata, o => o.MapFrom(s => s.Airline.IataCode))
            .ForMember(d => d.AirlineLogoUrl, o => o.MapFrom(s => s.Airline.LogoUrl))
            .ForMember(d => d.AircraftModel, o => o.MapFrom(s => s.Aircraft.Model))
            .ForMember(d => d.OriginIata, o => o.MapFrom(s => s.Route.OriginAirport.IataCode))
            .ForMember(d => d.OriginCity, o => o.MapFrom(s => s.Route.OriginAirport.City))
            .ForMember(d => d.OriginCountry, o => o.MapFrom(s => s.Route.OriginAirport.Country))
            .ForMember(d => d.DestinationIata, o => o.MapFrom(s => s.Route.DestinationAirport.IataCode))
            .ForMember(d => d.DestinationCity, o => o.MapFrom(s => s.Route.DestinationAirport.City))
            .ForMember(d => d.DestinationCountry, o => o.MapFrom(s => s.Route.DestinationAirport.Country))
            .ForMember(d => d.DurationMinutes, o => o.MapFrom(s => (int)(s.ArrivalTime - s.DepartureTime).TotalMinutes));

        CreateMap<Flight, FlightSearchResult>()
            .IncludeBase<Flight, FlightDto>();

        CreateMap<Airport, AirportDto>();
        CreateMap<CreateAirportRequest, Airport>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true));

        CreateMap<Route, RouteDto>()
            .ForMember(d => d.OriginIata, o => o.MapFrom(s => s.OriginAirport.IataCode))
            .ForMember(d => d.OriginCity, o => o.MapFrom(s => s.OriginAirport.City))
            .ForMember(d => d.DestinationIata, o => o.MapFrom(s => s.DestinationAirport.IataCode))
            .ForMember(d => d.DestinationCity, o => o.MapFrom(s => s.DestinationAirport.City));

        CreateMap<Airline, AirlineDto>()
            .ForMember(d => d.PrimaryImageUrl, o => o.Ignore());
        CreateMap<AirlineImage, AirlineImageDto>();

        CreateMap<Seat, SeatDto>();
    }
}