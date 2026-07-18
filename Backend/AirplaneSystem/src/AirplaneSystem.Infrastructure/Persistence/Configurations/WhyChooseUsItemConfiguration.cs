using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class WhyChooseUsItemConfiguration : IEntityTypeConfiguration<WhyChooseUsItem>
{
    public void Configure(EntityTypeBuilder<WhyChooseUsItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.IconColor).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.DisplayOrder);
        builder.Ignore(x => x.DomainEvents);
    }
}