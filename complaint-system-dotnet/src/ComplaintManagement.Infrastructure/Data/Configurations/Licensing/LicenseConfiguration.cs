using ComplaintManagement.Domain.Entities.Licensing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Licensing;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LicenseKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Name)
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.ActivatedBy)
            .HasMaxLength(255);

        builder.Property(l => l.Signature)
            .HasMaxLength(1000);

        builder.Property(l => l.MachineId)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => l.LicenseKey);
        builder.HasIndex(l => new { l.CompanyId, l.IsActive });
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.ExpiresAt);

        // Relationships
        builder.HasOne(l => l.Company)
            .WithMany()
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.ModuleActivations)
            .WithOne(m => m.License)
            .HasForeignKey(m => m.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
