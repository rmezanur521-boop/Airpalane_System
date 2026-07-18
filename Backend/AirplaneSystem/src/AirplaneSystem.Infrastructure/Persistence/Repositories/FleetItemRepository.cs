using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class FleetItemRepository : Repository<FleetItem>, IFleetItemRepository
{
    public FleetItemRepository(AppDbContext context) : base(context) { }
}