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
    IHeroSectionRepository HeroSections { get; }
    INavbarSettingRepository NavbarSettings { get; }
    IFooterSettingRepository FooterSettings { get; }
    IHomepageSettingRepository HomepageSettings { get; }
    ISpecialOfferRepository SpecialOffers { get; }
    IPopularDestinationRepository PopularDestinations { get; }
    IWhyChooseUsItemRepository WhyChooseUsItems { get; }
    IFleetItemRepository FleetItems { get; }
    ITravelServiceRepository TravelServices { get; }
    IAnnouncementBarRepository AnnouncementBars { get; }
    void MarkAdded<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}