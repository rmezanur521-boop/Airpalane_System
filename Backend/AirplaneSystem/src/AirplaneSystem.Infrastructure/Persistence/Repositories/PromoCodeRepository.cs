using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class PromoCodeRepository : Repository<PromoCode>, IPromoCodeRepository
{
    public PromoCodeRepository(AppDbContext context) : base(context) { }

    public async Task<PromoCode?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.Code == code.ToUpperInvariant(), ct);
}
