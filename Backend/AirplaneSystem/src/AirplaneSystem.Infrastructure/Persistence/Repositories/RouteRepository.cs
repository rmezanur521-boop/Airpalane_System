using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Flights;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Route>> GetAllWithAirportsAsync(CancellationToken ct = default) =>
        await _dbSet
            .Where(r => r.IsActive)
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .OrderBy(r => r.OriginAirport.IataCode)
            .ToListAsync(ct);
}