using AirplaneSystem.Domain.Entities.Users;

namespace AirplaneSystem.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRefreshTokensAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string hashedToken, CancellationToken ct = default);
    Task<User?> GetWithPassportAsync(Guid userId, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
