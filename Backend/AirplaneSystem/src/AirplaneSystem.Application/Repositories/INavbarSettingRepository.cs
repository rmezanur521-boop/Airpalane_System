using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Application.Repositories;

public interface INavbarSettingRepository : IRepository<NavbarSetting>
{
    Task<NavbarSetting> GetSingletonAsync(CancellationToken ct = default);
}