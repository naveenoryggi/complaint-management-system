using ComplaintManagement.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Settings;

public class EmailServerSettingsConfiguration : IEntityTypeConfiguration<EmailServerSettings>
{
    public void Configure(EntityTypeBuilder<EmailServerSettings> builder)
    {
        builder.ToTable("EmailServerSettings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Host)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Port)
            .IsRequired();

        builder.Property(e => e.Username)
            .HasMaxLength(255);

        builder.Property(e => e.Password)
            .HasMaxLength(500); // Encrypted, may be longer

        builder.Property(e => e.FromEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FromName)
            .HasMaxLength(200);

        builder.Property(e => e.ReplyToEmail)
            .HasMaxLength(255);

        builder.Property(e => e.TestNotes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => new { e.IsActive, e.IsDefault });

        // Relationships
        builder.HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
