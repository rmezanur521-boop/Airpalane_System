using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Events;

public class BookingCreatedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid BookingId { get; }
    public Guid UserId { get; }
    public string BookingReference { get; }

    public BookingCreatedEvent(Guid bookingId, Guid userId, string bookingReference)
    {
        BookingId = bookingId;
        UserId = userId;
        BookingReference = bookingReference;
    }
}
