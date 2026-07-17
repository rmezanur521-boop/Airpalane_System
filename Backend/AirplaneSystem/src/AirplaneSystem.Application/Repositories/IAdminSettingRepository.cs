using AirplaneSystem.Domain.Entities.Settings;

namespace AirplaneSystem.Application.Repositories;

public interface IAdminSettingRepository : IRepository<AdminSetting>
{
    /// <summary>
    /// Returns the single AdminSettings row (there is always exactly one).
    /// Returns null only if the table hasn't been seeded yet.
    /// </summary>
    Task<AdminSetting?> GetSettingsAsync(CancellationToken ct = default);
}