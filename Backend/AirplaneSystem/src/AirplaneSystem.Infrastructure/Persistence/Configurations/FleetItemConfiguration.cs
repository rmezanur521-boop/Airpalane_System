using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class FleetItemConfiguration : IEntityTypeConfiguration<FleetItem>
{
    public void Configure(EntityTypeBuilder<FleetItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AircraftName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Manufacturer).HasMaxLength(150);
        builder.Property(x => x.Image).HasMaxLength(1000);
        builder.Property(x => x.Range).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.DisplayOrder);
        builder.Ignore(x => x.DomainEvents);
    }
}