using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class PopularDestinationRepository : Repository<PopularDestination>, IPopularDestinationRepository
{
    public PopularDestinationRepository(AppDbContext context) : base(context) { }
}