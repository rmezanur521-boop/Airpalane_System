using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class TravelServiceRepository : Repository<TravelService>, ITravelServiceRepository
{
    public TravelServiceRepository(AppDbContext context) : base(context) { }
}