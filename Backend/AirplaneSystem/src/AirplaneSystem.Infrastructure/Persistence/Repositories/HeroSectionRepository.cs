using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using AirplaneSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class HeroSectionRepository : Repository<HeroSection>, IHeroSectionRepository
{
    public HeroSectionRepository(AppDbContext context) : base(context) { }

    public async Task<List<HeroSection>> GetActiveOrderedAsync(CancellationToken ct = default) =>
        await _dbSet
            .Where(h => h.Status == ContentStatus.Active)
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync(ct);
}