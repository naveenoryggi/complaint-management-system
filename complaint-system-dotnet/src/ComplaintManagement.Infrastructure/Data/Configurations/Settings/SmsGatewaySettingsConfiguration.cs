using ComplaintManagement.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Settings;

public class SmsGatewaySettingsConfiguration : IEntityTypeConfiguration<SmsGatewaySettings>
{
    public void Configure(EntityTypeBuilder<SmsGatewaySettings> builder)
    {
        builder.ToTable("SmsGatewaySettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ApiUrl)
            .HasMaxLength(500);

        builder.Property(s => s.AccountSid)
            .HasMaxLength(255);

        builder.Property(s => s.AuthToken)
            .HasMaxLength(500); // Encrypted

        builder.Property(s => s.FromNumber)
            .HasMaxLength(50);

        builder.Property(s => s.SenderName)
            .HasMaxLength(100);

        builder.Property(s => s.CostPerSms)
            .HasColumnType("decimal(10,4)");

        builder.Property(s => s.AdditionalConfig)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.TestNotes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => new { s.IsActive, s.IsDefault });
        builder.HasIndex(s => s.Provider);

        // Relationships
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
