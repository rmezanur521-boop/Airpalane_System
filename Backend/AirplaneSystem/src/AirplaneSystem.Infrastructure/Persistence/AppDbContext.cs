using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Entities.Audit;
using AirplaneSystem.Domain.Entities.Booking;
using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Entities.Payments;
using AirplaneSystem.Domain.Entities.Tickets;
using AirplaneSystem.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace AirplaneSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PassportInfo> PassportInfos => Set<PassportInfo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Aircraft> Aircrafts => Set<Aircraft>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();
    public DbSet<BookingSegment> BookingSegments => Set<BookingSegment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Flight>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Booking>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        SetAuditableProperties();

        var result = await base.SaveChangesAsync(cancellationToken);

        await OnAfterSaveChangesAsync(auditEntries);
        return result;
    }

    private void SetAuditableProperties()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser?.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Entity.GetType().Name,
                UserId = _currentUser?.UserId,
                UserEmail = _currentUser?.Email,
                Action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Deleted => "Deleted",
                    _ => "Updated"
                }
            };

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary) { auditEntry.TemporaryProperties.Add(prop); continue; }
                var propName = prop.Metadata.Name;
                if (entry.State == EntityState.Added) auditEntry.NewValues[propName] = prop.CurrentValue;
                else if (entry.State == EntityState.Deleted) auditEntry.OldValues[propName] = prop.OriginalValue;
                else if (prop.IsModified) { auditEntry.OldValues[propName] = prop.OriginalValue; auditEntry.NewValues[propName] = prop.CurrentValue; }
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries.Where(e => !e.TemporaryProperties.Any()).ToList();
    }

    private async Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
    {
        if (!auditEntries.Any()) return;
        foreach (var entry in auditEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey()) entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                else entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
            }

            AuditLogs.Add(new AuditLog
            {
                EntityName = entry.EntityName,
                EntityId = entry.KeyValues.Values.FirstOrDefault()?.ToString() ?? "",
                Action = entry.Action,
                OldValues = entry.OldValues.Any() ? JsonSerializer.Serialize(entry.OldValues) : null,
                NewValues = entry.NewValues.Any() ? JsonSerializer.Serialize(entry.NewValues) : null,
                UserId = entry.UserId,
                UserEmail = entry.UserEmail,
                IpAddress = "system",
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString("N")[..16]
            });
        }
        await base.SaveChangesAsync();
    }
}

internal class AuditEntry
{
    public AuditEntry(EntityEntry entry) { Entry = entry; }
    public EntityEntry Entry { get; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public Dictionary<string, object?> KeyValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();
}
