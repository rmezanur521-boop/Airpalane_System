using AirplaneSystem.Domain.Common;

namespace AirplaneSystem.Domain.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public string Email { get; }

    public UserRegisteredEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
