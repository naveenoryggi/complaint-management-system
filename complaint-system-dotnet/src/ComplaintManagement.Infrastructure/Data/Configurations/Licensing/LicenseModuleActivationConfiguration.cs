using ComplaintManagement.Domain.Entities.Licensing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Licensing;

public class LicenseModuleActivationConfiguration : IEntityTypeConfiguration<LicenseModuleActivation>
{
    public void Configure(EntityTypeBuilder<LicenseModuleActivation> builder)
    {
        builder.ToTable("LicenseModuleActivations");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Module)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.ModuleConfig)
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.ModuleLimits)
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(m => m.LicenseId);
        builder.HasIndex(m => new { m.LicenseId, m.Module }).IsUnique();
        builder.HasIndex(m => new { m.LicenseId, m.IsEnabled });
        builder.HasIndex(m => m.ExpiresAt);

        // Relationship to License is configured in LicenseConfiguration
    }
}
