using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<Payment?> GetByStripeIntentIdAsync(string intentId, CancellationToken ct = default) =>
        await _dbSet.Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intentId, ct);

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.BookingId == bookingId, ct);
}
