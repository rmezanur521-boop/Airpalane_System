using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class AnnouncementBarConfiguration : IEntityTypeConfiguration<AnnouncementBar>
{
    public void Configure(EntityTypeBuilder<AnnouncementBar> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
        builder.Property(x => x.BackgroundColor).HasMaxLength(20);
        builder.Property(x => x.TextColor).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.Priority);
        builder.Ignore(x => x.DomainEvents);
    }
}