using AirplaneSystem.Domain.Entities.Tickets;

namespace AirplaneSystem.Application.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken ct = default);
    Task<List<Ticket>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
}
