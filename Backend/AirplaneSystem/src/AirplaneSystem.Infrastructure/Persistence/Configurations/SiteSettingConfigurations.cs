using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class NavbarSettingConfiguration : IEntityTypeConfiguration<NavbarSetting>
{
    public void Configure(EntityTypeBuilder<NavbarSetting> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Logo).HasMaxLength(1000);
        builder.Property(n => n.SupportPhone).HasMaxLength(30);
        builder.Property(n => n.SupportEmail).HasMaxLength(256);
        builder.Property(n => n.FaviconPath).HasMaxLength(1000);
        builder.Property(n => n.WebsiteUrl).HasMaxLength(300);
        builder.Ignore(n => n.DomainEvents);
    }
}

public class FooterSettingConfiguration : IEntityTypeConfiguration<FooterSetting>
{
    public void Configure(EntityTypeBuilder<FooterSetting> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.About).HasMaxLength(2000);
        builder.Property(f => f.Address).HasMaxLength(500);
        builder.Property(f => f.Phone).HasMaxLength(30);
        builder.Property(f => f.Email).HasMaxLength(256);
        builder.Property(f => f.Copyright).HasMaxLength(300);
        builder.Ignore(f => f.DomainEvents);
    }
}

public class HomepageSettingConfiguration : IEntityTypeConfiguration<HomepageSetting>
{
    public void Configure(EntityTypeBuilder<HomepageSetting> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Ignore(h => h.DomainEvents);
    }
}