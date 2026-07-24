using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Application.Repositories;

public interface ISmtpSettingRepository : IRepository<SmtpSettings>
{
    Task<SmtpSettings> GetSingletonAsync(CancellationToken ct = default);
}