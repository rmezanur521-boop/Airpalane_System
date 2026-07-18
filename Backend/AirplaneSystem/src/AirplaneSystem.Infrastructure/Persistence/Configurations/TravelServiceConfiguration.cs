using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelServiceEntity = AirplaneSystem.Domain.Entities.Cms.TravelService;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class TravelServiceConfiguration : IEntityTypeConfiguration<TravelServiceEntity>
{
    public void Configure(EntityTypeBuilder<TravelServiceEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Image).HasMaxLength(1000);
        builder.Property(x => x.ButtonText).HasMaxLength(50);
        builder.Property(x => x.RedirectUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.DisplayOrder);
        builder.Ignore(x => x.DomainEvents);
    }
}