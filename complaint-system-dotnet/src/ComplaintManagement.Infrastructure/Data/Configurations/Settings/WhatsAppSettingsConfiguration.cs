using ComplaintManagement.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Settings;

public class WhatsAppSettingsConfiguration : IEntityTypeConfiguration<WhatsAppSettings>
{
    public void Configure(EntityTypeBuilder<WhatsAppSettings> builder)
    {
        builder.ToTable("WhatsAppSettings");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.ApiUrl)
            .HasMaxLength(500);

        builder.Property(w => w.BusinessAccountId)
            .HasMaxLength(255);

        builder.Property(w => w.PhoneNumberId)
            .HasMaxLength(255);

        builder.Property(w => w.AccessToken)
            .HasMaxLength(1000); // Encrypted, may be long

        builder.Property(w => w.WebhookToken)
            .HasMaxLength(500);

        builder.Property(w => w.FromNumber)
            .HasMaxLength(50);

        builder.Property(w => w.BusinessName)
            .HasMaxLength(200);

        builder.Property(w => w.AdditionalConfig)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.TestNotes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(w => w.CompanyId);
        builder.HasIndex(w => new { w.IsActive, w.IsDefault });
        builder.HasIndex(w => w.Provider);

        // Relationships
        builder.HasOne(w => w.Company)
            .WithMany()
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
