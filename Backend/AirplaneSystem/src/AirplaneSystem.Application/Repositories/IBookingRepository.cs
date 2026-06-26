using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Domain.Entities.Booking;

namespace AirplaneSystem.Application.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByReferenceAsync(string reference, CancellationToken ct = default);
    Task<Booking?> GetWithDetailsAsync(Guid bookingId, CancellationToken ct = default);
    Task<List<Booking>> GetExpiredHoldsAsync(CancellationToken ct = default);
    Task<List<Booking>> GetUserBookingsWithDetailsAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<Booking>> GetPagedAsync(PaginationQuery query, CancellationToken ct = default);
    Task<bool> ReferenceExistsAsync(string reference, CancellationToken ct = default);
}
