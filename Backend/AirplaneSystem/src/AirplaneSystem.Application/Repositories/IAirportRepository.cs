using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Domain.Entities.Flights;

namespace AirplaneSystem.Application.Repositories;

public interface IAirportRepository : IRepository<Airport>
{
    Task<Airport?> GetByIataCodeAsync(string iataCode, CancellationToken ct = default);
    Task<PagedResult<Airport>> GetPagedAsync(PaginationQuery query, CancellationToken ct = default);
}
