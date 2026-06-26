using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Events;

public class PaymentCompletedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid PaymentId { get; }
    public Guid BookingId { get; }
    public decimal Amount { get; }

    public PaymentCompletedEvent(Guid paymentId, Guid bookingId, decimal amount)
    {
        PaymentId = paymentId;
        BookingId = bookingId;
        Amount = amount;
    }
}
