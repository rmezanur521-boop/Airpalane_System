using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetWithRefreshTokensAsync(Guid userId, CancellationToken ct = default) =>
        await _dbSet.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

    public async Task<User?> GetByRefreshTokenAsync(string hashedToken, CancellationToken ct = default) =>
        await _dbSet.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == hashedToken), ct);

    public async Task<User?> GetWithPassportAsync(Guid userId, CancellationToken ct = default) =>
        await _dbSet.Include(u => u.PassportInfo)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
}
