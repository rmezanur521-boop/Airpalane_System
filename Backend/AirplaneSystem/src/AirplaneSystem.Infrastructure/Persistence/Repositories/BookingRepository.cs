using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Booking;
using AirplaneSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context) { }

    public async Task<Booking?> GetByReferenceAsync(string reference, CancellationToken ct = default) =>
        await GetWithDetailsQuery()
            .FirstOrDefaultAsync(b => b.BookingReference == reference, ct);

    public async Task<Booking?> GetWithDetailsAsync(Guid bookingId, CancellationToken ct = default) =>
        await GetWithDetailsQuery()
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

    public async Task<List<Booking>> GetExpiredHoldsAsync(CancellationToken ct = default) =>
        await _dbSet
            .Include(b => b.BookingPassengers)
            .Include(b => b.BookingSegments)
            .Where(b => b.Status == BookingStatus.PendingPayment && b.HoldExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<List<Booking>> GetUserBookingsWithDetailsAsync(Guid userId, CancellationToken ct = default) =>
        await GetWithDetailsQuery()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<PagedResult<Booking>> GetPagedAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var queryable = GetWithDetailsQuery();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            queryable = queryable.Where(b => b.BookingReference.Contains(query.SearchTerm));

        var total = await queryable.CountAsync(ct);
        var items = await queryable
            .OrderByDescending(b => b.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<Booking>.Create(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<bool> ReferenceExistsAsync(string reference, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(b => b.BookingReference == reference, ct);

    private IQueryable<Booking> GetWithDetailsQuery() =>
        _dbSet
            .Include(b => b.User)
            .Include(b => b.BookingPassengers).ThenInclude(p => p.Seat)
            .Include(b => b.BookingSegments).ThenInclude(s => s.Flight).ThenInclude(f => f.Airline)
            .Include(b => b.BookingSegments).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(b => b.BookingSegments).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .Include(b => b.Payment)
            .Include(b => b.PromoCode);
}
