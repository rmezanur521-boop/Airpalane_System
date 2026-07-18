using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Application.Repositories;

public interface IHeroSectionRepository : IRepository<HeroSection>
{
    Task<List<HeroSection>> GetActiveOrderedAsync(CancellationToken ct = default);
}