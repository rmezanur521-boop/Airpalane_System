using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context) => _context = context;

    public async Task<List<AuditLog>> GetPagedAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        var q = BuildQuery(query);
        return await q
            .OrderByDescending(a => a.Timestamp)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(AuditLogQuery query, CancellationToken ct = default) =>
        await BuildQuery(query).CountAsync(ct);

    private IQueryable<AuditLog> BuildQuery(AuditLogQuery query)
    {
        var q = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            q = q.Where(a => a.EntityName == query.EntityName);

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action == query.Action);

        if (query.From.HasValue)
            q = q.Where(a => a.Timestamp >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(a => a.Timestamp <= query.To.Value);

        return q;
    }
}
