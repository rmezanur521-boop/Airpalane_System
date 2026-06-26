using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Flights;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class AirportRepository : Repository<Airport>, IAirportRepository
{
    public AirportRepository(AppDbContext context) : base(context) { }

    public async Task<Airport?> GetByIataCodeAsync(string iataCode, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(a => a.IataCode == iataCode.ToUpperInvariant(), ct);

    public async Task<PagedResult<Airport>> GetPagedAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var q = _dbSet.Where(a => a.IsActive);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            q = q.Where(a => a.IataCode.Contains(query.SearchTerm) || a.Name.Contains(query.SearchTerm)
                || a.City.Contains(query.SearchTerm) || a.Country.Contains(query.SearchTerm));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(a => a.IataCode)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResult<Airport>.Create(items, total, query.PageNumber, query.PageSize);
    }
}
