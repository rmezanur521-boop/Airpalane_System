using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class HeroSectionConfiguration : IEntityTypeConfiguration<HeroSection>
{
    public void Configure(EntityTypeBuilder<HeroSection> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Title).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Subtitle).HasMaxLength(500);
        builder.Property(h => h.BackgroundImage).HasMaxLength(1000);
        builder.Property(h => h.ButtonText).HasMaxLength(50);
        builder.Property(h => h.ButtonLink).HasMaxLength(500);
        builder.Property(h => h.OverlayOpacity).HasColumnType("float");
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(h => h.DisplayOrder);
        builder.Ignore(h => h.DomainEvents);
    }
}