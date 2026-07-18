using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class PopularDestinationConfiguration : IEntityTypeConfiguration<PopularDestination>
{
    public void Configure(EntityTypeBuilder<PopularDestination> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DestinationName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Country).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Image).HasMaxLength(1000);
        builder.Property(x => x.StartingPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.DisplayOrder);
        builder.Ignore(x => x.DomainEvents);
    }
}