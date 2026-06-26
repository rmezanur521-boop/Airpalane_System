using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Bookings;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default);
    Task<BookingDto> GetByIdAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
    Task<BookingDto> GetByReferenceAsync(string reference, CancellationToken ct = default);
    Task<PagedResult<BookingDto>> GetUserBookingsAsync(Guid userId, PaginationQuery query, CancellationToken ct = default);
    Task<PagedResult<BookingDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
    Task CancelAsync(Guid userId, Guid bookingId, string reason, CancellationToken ct = default);
    Task SelectSeatAsync(Guid bookingId, SelectSeatRequest request, CancellationToken ct = default);
    Task ExpireHeldBookingsAsync(CancellationToken ct = default);
}
