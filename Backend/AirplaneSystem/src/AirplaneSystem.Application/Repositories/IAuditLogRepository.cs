using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Domain.Entities.Audit;

namespace AirplaneSystem.Application.Repositories;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetPagedAsync(AuditLogQuery query, CancellationToken ct = default);
    Task<int> CountAsync(AuditLogQuery query, CancellationToken ct = default);
}
