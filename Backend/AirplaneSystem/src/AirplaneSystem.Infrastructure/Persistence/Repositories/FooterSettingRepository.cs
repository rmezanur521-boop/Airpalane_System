using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class FooterSettingRepository : Repository<FooterSetting>, IFooterSettingRepository
{
    public FooterSettingRepository(AppDbContext context) : base(context) { }

    public async Task<FooterSetting> GetSingletonAsync(CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(ct);
        if (entity != null) return entity;

        entity = new FooterSetting();
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}