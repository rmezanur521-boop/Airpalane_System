using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class NavbarSettingRepository : Repository<NavbarSetting>, INavbarSettingRepository
{
    public NavbarSettingRepository(AppDbContext context) : base(context) { }

    public async Task<NavbarSetting> GetSingletonAsync(CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(ct);
        if (entity != null) return entity;

        // প্রথমবার GET করলে Default Row Auto-Create হবে
        entity = new NavbarSetting { CompanyName = "AirplaneSystem" };
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }
}