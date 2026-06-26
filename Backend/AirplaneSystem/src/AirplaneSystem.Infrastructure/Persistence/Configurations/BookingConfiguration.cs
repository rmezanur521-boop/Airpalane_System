using AirplaneSystem.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BookingReference).IsRequired().HasMaxLength(8);
        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
        builder.Property(b => b.DiscountAmount).HasPrecision(18, 2);
        builder.Property(b => b.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(b => b.CancellationReason).HasMaxLength(500);

        builder.HasIndex(b => b.BookingReference).IsUnique();
        builder.HasIndex(b => new { b.UserId, b.Status });
        builder.HasIndex(b => b.HoldExpiresAt);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.PromoCode)
            .WithMany()
            .HasForeignKey(b => b.PromoCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.BookingPassengers)
            .WithOne(p => p.Booking)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BookingSegments)
            .WithOne(s => s.Booking)
            .HasForeignKey(s => s.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(b => b.DomainEvents);
    }
}

public class BookingPassengerConfiguration : IEntityTypeConfiguration<BookingPassenger>
{
    public void Configure(EntityTypeBuilder<BookingPassenger> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PassportNumber).HasMaxLength(50);
        builder.Property(p => p.PassportCountry).HasMaxLength(100);
        builder.Property(p => p.MealPreference).HasMaxLength(50);
        builder.Property(p => p.SpecialAssistance).HasMaxLength(200);

        builder.HasOne(p => p.Seat)
            .WithMany()
            .HasForeignKey(p => p.SeatId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(p => p.FullName);
        builder.Ignore(p => p.DomainEvents);
    }
}

public class BookingSegmentConfiguration : IEntityTypeConfiguration<BookingSegment>
{
    public void Configure(EntityTypeBuilder<BookingSegment> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.BaseFare).HasPrecision(18, 2);
        builder.Property(s => s.Taxes).HasPrecision(18, 2);
        builder.Property(s => s.Fees).HasPrecision(18, 2);
        builder.Property(s => s.SegmentTotal).HasPrecision(18, 2);

        builder.HasOne(s => s.Flight)
            .WithMany()
            .HasForeignKey(s => s.FlightId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.DomainEvents);
    }
}
