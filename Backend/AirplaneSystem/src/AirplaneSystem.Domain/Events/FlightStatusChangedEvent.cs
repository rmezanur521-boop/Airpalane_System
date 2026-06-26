using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Events;

public class FlightStatusChangedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid FlightId { get; }
    public string FlightNumber { get; }
    public FlightStatus OldStatus { get; }
    public FlightStatus NewStatus { get; }

    public FlightStatusChangedEvent(Guid flightId, string flightNumber, FlightStatus oldStatus, FlightStatus newStatus)
    {
        FlightId = flightId;
        FlightNumber = flightNumber;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
