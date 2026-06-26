using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Events;

public class BookingExpiredEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid BookingId { get; }
    public string BookingReference { get; }

    public BookingExpiredEvent(Guid bookingId, string bookingReference)
    {
        BookingId = bookingId;
        BookingReference = bookingReference;
    }
}
