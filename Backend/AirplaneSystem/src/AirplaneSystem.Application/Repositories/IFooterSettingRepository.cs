using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Application.Repositories;

public interface IFooterSettingRepository : IRepository<FooterSetting>
{
    Task<FooterSetting> GetSingletonAsync(CancellationToken ct = default);
}