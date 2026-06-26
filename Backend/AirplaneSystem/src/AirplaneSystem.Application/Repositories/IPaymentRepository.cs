using AirplaneSystem.Domain.Entities.Payments;

namespace AirplaneSystem.Application.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByStripeIntentIdAsync(string intentId, CancellationToken ct = default);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
}
