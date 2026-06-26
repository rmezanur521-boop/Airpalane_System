using AirplaneSystem.Application.DTOs.Bookings;
using AirplaneSystem.Application.DTOs.Payments;
using AirplaneSystem.Domain.Entities.Payments;
using AutoMapper;

namespace AirplaneSystem.Application.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.BookingReference, o => o.MapFrom(s => s.Booking.BookingReference));

        CreateMap<Payment, PaymentSummaryDto>();

        CreateMap<Refund, RefundDto>();
    }
}
