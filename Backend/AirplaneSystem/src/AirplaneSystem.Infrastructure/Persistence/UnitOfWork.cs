using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AirplaneSystem.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _users;
    private IFlightRepository? _flights;
    private IBookingRepository? _bookings;
    private IPaymentRepository? _payments;
    private IAirportRepository? _airports;
    private IAirlineRepository? _airlines;
    private IRouteRepository? _routes;
    private ITicketRepository? _tickets;
    private IPromoCodeRepository? _promoCodes;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IFlightRepository Flights => _flights ??= new FlightRepository(_context);
    public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public IAirportRepository Airports => _airports ??= new AirportRepository(_context);
    public IAirlineRepository Airlines => _airlines ??= new AirlineRepository(_context);
    public IRouteRepository Routes => _routes ??= new RouteRepository(_context);
    public ITicketRepository Tickets => _tickets ??= new TicketRepository(_context);
    public IPromoCodeRepository PromoCodes => _promoCodes ??= new PromoCodeRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public void MarkAdded<TEntity>(TEntity entity) where TEntity : class
    {
        var entry = _context.Entry(entity);
        // If it's somehow already tracked (e.g. loaded then re-created),
        // don't fight the tracker — just ensure the state is Added.
        entry.State = EntityState.Added;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}