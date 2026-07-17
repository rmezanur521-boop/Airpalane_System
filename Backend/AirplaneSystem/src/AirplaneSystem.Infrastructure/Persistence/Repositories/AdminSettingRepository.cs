using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class AdminSettingRepository : Repository<AdminSetting>, IAdminSettingRepository
{
    public AdminSettingRepository(AppDbContext context) : base(context) { }

    public async Task<AdminSetting?> GetSettingsAsync(CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(ct);
}