using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirplaneSystem.Infrastructure.Persistence.Configurations;

public class SmtpSettingConfiguration : IEntityTypeConfiguration<SmtpSettings>
{
    public void Configure(EntityTypeBuilder<SmtpSettings> builder)
    {
        builder.ToTable("SmtpSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SmtpHost).HasMaxLength(200);
        builder.Property(x => x.SmtpUsername).HasMaxLength(200);
        builder.Property(x => x.SmtpPasswordEncrypted).HasMaxLength(1000);
        builder.Property(x => x.SmtpFromName).HasMaxLength(200);
        builder.Property(x => x.SmtpFromEmail).HasMaxLength(200);
        builder.Ignore(x => x.DomainEvents);
    }
}