using AirplaneSystem.Application.DTOs.Bookings;
using AirplaneSystem.Domain.Entities.Booking;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.Segments, o => o.MapFrom(s => s.BookingSegments))
            .ForMember(d => d.Passengers, o => o.MapFrom(s => s.BookingPassengers))
            .ForMember(d => d.Payment, o => o.MapFrom(s => s.Payment));

        CreateMap<BookingSegment, BookingSegmentDto>()
            .ForMember(d => d.FlightNumber, o => o.MapFrom(s => s.Flight.FlightNumber))
            .ForMember(d => d.OriginIata, o => o.MapFrom(s => s.Flight.Route.OriginAirport.IataCode))
            .ForMember(d => d.DestinationIata, o => o.MapFrom(s => s.Flight.Route.DestinationAirport.IataCode))
            .ForMember(d => d.DepartureTime, o => o.MapFrom(s => s.Flight.DepartureTime))
            .ForMember(d => d.ArrivalTime, o => o.MapFrom(s => s.Flight.ArrivalTime));

        CreateMap<BookingPassenger, BookingPassengerDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.SeatNumber, o => o.MapFrom(s => s.Seat != null ? s.Seat.SeatNumber : null));
    }
}
