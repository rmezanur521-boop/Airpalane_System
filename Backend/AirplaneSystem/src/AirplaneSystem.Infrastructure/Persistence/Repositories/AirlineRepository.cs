using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Flights;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class AirlineRepository : Repository<Airline>, IAirlineRepository
{
    public AirlineRepository(AppDbContext context) : base(context) { }

    public async Task<Airline?> GetByIataCodeAsync(string iataCode, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(a => a.IataCode == iataCode.ToUpperInvariant(), ct);

    public async Task<Airline?> GetByIdWithImagesAsync(Guid id, CancellationToken ct = default) =>
        await _dbSet.Include(a => a.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Airline>> GetAllWithImagesAsync(CancellationToken ct = default) =>
        await _dbSet.Include(a => a.Images.OrderBy(i => i.SortOrder)).ToListAsync(ct);
}