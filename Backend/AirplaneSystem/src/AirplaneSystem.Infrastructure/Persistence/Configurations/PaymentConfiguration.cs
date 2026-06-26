using AirplaneSystem.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.StripePaymentIntentId).HasMaxLength(200);
        builder.Property(p => p.StripeClientSecret).HasMaxLength(500);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.ReceiptUrl).HasMaxLength(1000);

        builder.HasIndex(p => p.StripePaymentIntentId).IsUnique();
        builder.HasIndex(p => p.BookingId).IsUnique();

        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(20);
        builder.Property(p => p.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(p => p.DiscountAmount).HasPrecision(18, 2);
        builder.Property(p => p.MinimumAmount).HasPrecision(18, 2);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.IsValid);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.StripeRefundId).HasMaxLength(200);
        builder.Property(r => r.DenialReason).HasMaxLength(500);
        builder.Ignore(r => r.DomainEvents);
    }
}
