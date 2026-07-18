using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class HomepageSettingRepository : Repository<HomepageSetting>, IHomepageSettingRepository
{
    public HomepageSettingRepository(AppDbContext context) : base(context) { }

    public async Task<HomepageSetting> GetSingletonAsync(CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(ct);
        if (entity != null) return entity;

        entity = new HomepageSetting();
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}