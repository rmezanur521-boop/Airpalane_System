using AirplaneSystem.Domain.Entities.Flights;

namespace AirplaneSystem.Application.Repositories;

public interface IRouteRepository : IRepository<Route>
{
    Task<IReadOnlyList<Route>> GetAllWithAirportsAsync(CancellationToken ct = default);
}