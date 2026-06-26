using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
}
