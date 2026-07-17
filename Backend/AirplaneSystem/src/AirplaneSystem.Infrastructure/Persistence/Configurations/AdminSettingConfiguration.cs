using AirplaneSystem.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class AdminSettingConfiguration : IEntityTypeConfiguration<AdminSetting>
{
    public void Configure(EntityTypeBuilder<AdminSetting> builder)
    {
        builder.ToTable("AdminSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CompanyLogoPath)
            .HasMaxLength(500);

        builder.Property(x => x.FaviconPath)
            .HasMaxLength(500);

        builder.Property(x => x.SupportEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SupportPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.CompanyAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.WebsiteUrl)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.SmtpHost)
            .HasMaxLength(200);

        builder.Property(x => x.SmtpUsername)
            .HasMaxLength(200);

        // Encrypted string can be longer than the raw password
        builder.Property(x => x.SmtpPasswordEncrypted)
            .HasMaxLength(1000);

        builder.Property(x => x.SmtpFromName)
            .HasMaxLength(200);

        builder.Property(x => x.SmtpFromEmail)
            .HasMaxLength(200);

        builder.Property(x => x.FooterText)
            .HasMaxLength(500);
    }
}