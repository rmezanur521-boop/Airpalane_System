using AirplaneSystem.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(20);
        builder.Property(t => t.BoardingPassUrl).HasMaxLength(1000);
        builder.Property(t => t.QrCodeData).HasMaxLength(2000);

        builder.HasIndex(t => t.TicketNumber).IsUnique();

        builder.HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.BookingPassenger)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.BookingPassengerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.BookingSegment)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.BookingSegmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(t => t.DomainEvents);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<Domain.Entities.Audit.AuditLog>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Audit.AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(50);
        builder.Property(a => a.Action).HasMaxLength(20);
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CorrelationId).HasMaxLength(50);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
    }
}
