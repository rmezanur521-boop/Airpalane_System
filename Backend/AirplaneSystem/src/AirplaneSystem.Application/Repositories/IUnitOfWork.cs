using AirplaneSystem.Application.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IFlightRepository Flights { get; }
    IBookingRepository Bookings { get; }
    IPaymentRepository Payments { get; }
    IAirportRepository Airports { get; }
    IAirlineRepository Airlines { get; }
    IRouteRepository Routes { get; }
    ITicketRepository Tickets { get; }
    IPromoCodeRepository PromoCodes { get; }
    IAuditLogRepository AuditLogs { get; }
    IAdminSettingRepository AdminSettings { get; }   // ← নতুন লাইন

    void MarkAdded<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}