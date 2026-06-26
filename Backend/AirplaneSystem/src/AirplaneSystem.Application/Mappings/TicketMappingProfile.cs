using AirplaneSystem.Application.DTOs.Tickets;
using AirplaneSystem.Domain.Entities.Tickets;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.BookingReference, o => o.MapFrom(s => s.Booking.BookingReference))
            .ForMember(d => d.PassengerName, o => o.MapFrom(s => s.BookingPassenger.FullName))
            .ForMember(d => d.FlightNumber, o => o.MapFrom(s => s.BookingSegment.Flight.FlightNumber))
            .ForMember(d => d.AirlineName, o => o.MapFrom(s => s.BookingSegment.Flight.Airline.Name))
            .ForMember(d => d.OriginIata, o => o.MapFrom(s => s.BookingSegment.Flight.Route.OriginAirport.IataCode))
            .ForMember(d => d.DestinationIata, o => o.MapFrom(s => s.BookingSegment.Flight.Route.DestinationAirport.IataCode))
            .ForMember(d => d.DepartureTime, o => o.MapFrom(s => s.BookingSegment.Flight.DepartureTime))
            .ForMember(d => d.ArrivalTime, o => o.MapFrom(s => s.BookingSegment.Flight.ArrivalTime))
            .ForMember(d => d.SeatClass, o => o.MapFrom(s => s.BookingSegment.SeatClass.ToString()))
            .ForMember(d => d.SeatNumber, o => o.MapFrom(s => s.BookingPassenger.Seat != null ? s.BookingPassenger.Seat.SeatNumber : null));
    }
}
