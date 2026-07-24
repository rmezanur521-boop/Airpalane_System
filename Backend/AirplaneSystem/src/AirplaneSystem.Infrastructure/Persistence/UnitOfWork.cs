using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
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
    private IHeroSectionRepository? _heroSections;
    private INavbarSettingRepository? _navbarSettings;
    private IFooterSettingRepository? _footerSettings;
    private IHomepageSettingRepository? _homepageSettings;
    private ISpecialOfferRepository? _specialOffers;
    private IPopularDestinationRepository? _popularDestinations;
    private IWhyChooseUsItemRepository? _whyChooseUsItems;
    private IFleetItemRepository? _fleetItems;
    private ITravelServiceRepository? _travelServices;
    private IAnnouncementBarRepository? _announcementBars;
    private IPaymentGatewaySettingRepository? _paymentGatewaySettings;
    private ISmtpSettingRepository? _smtpSettings;
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
    public ISmtpSettingRepository SmtpSettings => _smtpSettings ??= new SmtpSettingRepository(_context);
    public IHeroSectionRepository HeroSections => _heroSections ??= new HeroSectionRepository(_context);
    public INavbarSettingRepository NavbarSettings => _navbarSettings ??= new NavbarSettingRepository(_context);
    public IFooterSettingRepository FooterSettings => _footerSettings ??= new FooterSettingRepository(_context);
    public IHomepageSettingRepository HomepageSettings => _homepageSettings ??= new HomepageSettingRepository(_context);
    public ISpecialOfferRepository SpecialOffers => _specialOffers ??= new SpecialOfferRepository(_context);
    public IPopularDestinationRepository PopularDestinations => _popularDestinations ??= new PopularDestinationRepository(_context);
    public IWhyChooseUsItemRepository WhyChooseUsItems => _whyChooseUsItems ??= new WhyChooseUsItemRepository(_context);
    public IFleetItemRepository FleetItems => _fleetItems ??= new FleetItemRepository(_context);
    public ITravelServiceRepository TravelServices => _travelServices ??= new TravelServiceRepository(_context);
    public IAnnouncementBarRepository AnnouncementBars => _announcementBars ??= new AnnouncementBarRepository(_context);
    public IPaymentGatewaySettingRepository PaymentGatewaySettings => _paymentGatewaySettings ??= new PaymentGatewaySettingRepository(_context);
    public void MarkAdded<TEntity>(TEntity entity) where TEntity : class
    {
        var entry = _context.Entry(entity);
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