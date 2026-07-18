using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Application.Repositories;

public interface IHomepageSettingRepository : IRepository<HomepageSetting>
{
    Task<HomepageSetting> GetSingletonAsync(CancellationToken ct = default);
}