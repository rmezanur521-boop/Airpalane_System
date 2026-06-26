using AirplaneSystem.Domain.Entities.Flights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.IataCode).IsRequired().HasMaxLength(3);
        builder.Property(a => a.IcaoCode).HasMaxLength(4);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
        builder.Property(a => a.CountryCode).HasMaxLength(2);
        builder.Property(a => a.TimeZone).HasMaxLength(50);
        builder.Property(a => a.Terminal).HasMaxLength(50);
        builder.Property(a => a.Latitude).HasPrecision(9, 6);
        builder.Property(a => a.Longitude).HasPrecision(9, 6);

        builder.HasIndex(a => a.IataCode).IsUnique();
        builder.HasIndex(a => a.Country);
        builder.Ignore(a => a.DomainEvents);
    }
}

public class AirlineConfiguration : IEntityTypeConfiguration<Airline>
{
    public void Configure(EntityTypeBuilder<Airline> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.IataCode).IsRequired().HasMaxLength(2);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Country).HasMaxLength(100);
        builder.Property(a => a.LogoUrl).HasMaxLength(500);
        builder.Property(a => a.ContactEmail).HasMaxLength(256);
        builder.Property(a => a.ContactPhone).HasMaxLength(20);

        builder.HasIndex(a => a.IataCode).IsUnique();
        builder.Ignore(a => a.DomainEvents);
    }
}

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Model).IsRequired().HasMaxLength(100);
        builder.Property(a => a.RegistrationNumber).IsRequired().HasMaxLength(20);

        builder.HasIndex(a => a.RegistrationNumber).IsUnique();

        builder.HasOne(a => a.Airline)
            .WithMany(al => al.Aircrafts)
            .HasForeignKey(a => a.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.DomainEvents);
    }
}

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.OriginAirportId, r.DestinationAirportId }).IsUnique();

        builder.HasOne(r => r.OriginAirport)
            .WithMany()
            .HasForeignKey(r => r.OriginAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.DestinationAirport)
            .WithMany()
            .HasForeignKey(r => r.DestinationAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(r => r.DomainEvents);
    }
}

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FlightNumber).IsRequired().HasMaxLength(10);
        builder.Property(f => f.EconomyBasePrice).HasPrecision(18, 2);
        builder.Property(f => f.BusinessBasePrice).HasPrecision(18, 2);
        builder.Property(f => f.FirstClassBasePrice).HasPrecision(18, 2);
        builder.Property(f => f.AirportFee).HasPrecision(18, 2);
        builder.Property(f => f.TaxPercentage).HasPrecision(5, 2);
        builder.Property(f => f.GateNumber).HasMaxLength(10);

        builder.HasIndex(f => f.FlightNumber).IsUnique();
        builder.HasIndex(f => new { f.DepartureTime, f.Status });
        builder.HasIndex(f => f.AircraftId);

        builder.HasOne(f => f.Airline)
            .WithMany()
            .HasForeignKey(f => f.AirlineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Aircraft)
            .WithMany(a => a.Flights)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Route)
            .WithMany(r => r.Flights)
            .HasForeignKey(f => f.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Seats)
            .WithOne(s => s.Flight)
            .HasForeignKey(s => s.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(f => f.DomainEvents);
        builder.Ignore(f => f.Duration);
    }
}

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SeatNumber).IsRequired().HasMaxLength(5);

        builder.HasIndex(s => new { s.FlightId, s.SeatNumber }).IsUnique();
        builder.Ignore(s => s.DomainEvents);
    }
}
