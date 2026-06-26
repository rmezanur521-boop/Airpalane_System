using AirplaneSystem.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Nationality).HasMaxLength(3);
        builder.Property(u => u.ProfilePictureUrl).HasMaxLength(1000);
        builder.Property(u => u.EmailVerificationToken).HasMaxLength(100);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(100);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Role);

        builder.HasOne(u => u.PassportInfo)
            .WithOne(p => p.User)
            .HasForeignKey<PassportInfo>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.DomainEvents);
    }
}

public class PassportInfoConfiguration : IEntityTypeConfiguration<PassportInfo>
{
    public void Configure(EntityTypeBuilder<PassportInfo> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PassportNumber).IsRequired().HasMaxLength(100);
        builder.Property(p => p.IssuingCountry).IsRequired().HasMaxLength(100);
        builder.Ignore(p => p.DomainEvents);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
        builder.Property(r => r.CreatedByIp).HasMaxLength(45);
        builder.Property(r => r.RevokedByIp).HasMaxLength(45);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(500);

        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => new { r.UserId, r.IsRevoked });
        builder.Ignore(r => r.DomainEvents);
        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsActive);
    }
}
