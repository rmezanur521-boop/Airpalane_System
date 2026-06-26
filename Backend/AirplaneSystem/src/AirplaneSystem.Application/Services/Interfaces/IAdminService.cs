using AirplaneSystem.Application.Common.Models;
using AirplaneSystem.Application.DTOs.Admin;
using AirplaneSystem.Application.DTOs.Users;

namespace AirplaneSystem.Application.Services.Interfaces;

public interface IAdminService
{
    Task<DashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<BookingReportDto> GetBookingReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken ct = default);
    Task<UserDto> CreateAgentAsync(CreateAgentRequest request, CancellationToken ct = default);
}
