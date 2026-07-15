using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(AppDbContext context) : base(context) { }

    public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken ct = default) =>
        await _dbSet
            .Include(t => t.Booking).ThenInclude(b => b.Payment)
            .Include(t => t.BookingPassenger).ThenInclude(p => p.Seat)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Airline)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, ct);

    public async Task<List<Ticket>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default) =>
        await _dbSet
            .Include(t => t.Booking).ThenInclude(b => b.Payment)
            .Include(t => t.BookingPassenger).ThenInclude(p => p.Seat)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Airline)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.OriginAirport)
            .Include(t => t.BookingSegment).ThenInclude(s => s.Flight).ThenInclude(f => f.Route)
                .ThenInclude(r => r.DestinationAirport)
            .Where(t => t.BookingId == bookingId)
            .ToListAsync(ct);
}