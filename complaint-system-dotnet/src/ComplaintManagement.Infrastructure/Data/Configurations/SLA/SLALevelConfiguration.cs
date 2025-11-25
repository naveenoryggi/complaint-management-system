using ComplaintManagement.Domain.Entities.SLA;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.SLA;

public class SLALevelConfiguration : IEntityTypeConfiguration<SLALevel>
{
    public void Configure(EntityTypeBuilder<SLALevel> builder)
    {
        builder.ToTable("SLALevels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.ColorCode)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#4CAF50");

        builder.Property(l => l.DefaultResponseTime)
            .IsRequired();

        builder.Property(l => l.ResponseTimeUnit)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.DefaultResolutionTime)
            .IsRequired();

        builder.Property(l => l.ResolutionTimeUnit)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.CompanyId);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt);

        // Indexes
        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => l.IsActive);
        builder.HasIndex(l => l.Order);
        builder.HasIndex(l => new { l.CompanyId, l.Name });
        builder.HasIndex(l => new { l.CompanyId, l.Order });

        // Relationships
        builder.HasMany(l => l.CategorySLAs)
            .WithOne(c => c.SLALevel)
            .HasForeignKey(c => c.SLALevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.PrioritySLAs)
            .WithOne(p => p.SLALevel)
            .HasForeignKey(p => p.SLALevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
