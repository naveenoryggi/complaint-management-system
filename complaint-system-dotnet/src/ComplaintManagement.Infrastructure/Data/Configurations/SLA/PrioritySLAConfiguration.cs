using ComplaintManagement.Domain.Entities.SLA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.SLA;

public class PrioritySLAConfiguration : IEntityTypeConfiguration<PrioritySLA>
{
    public void Configure(EntityTypeBuilder<PrioritySLA> builder)
    {
        builder.ToTable("PrioritySLAs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PriorityId)
            .IsRequired();

        builder.Property(p => p.SLALevelId)
            .IsRequired();

        builder.Property(p => p.OverrideResponseTime);

        builder.Property(p => p.OverrideResolutionTime);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Indexes
        builder.HasIndex(p => p.PriorityId)
            .IsUnique();

        builder.HasIndex(p => p.SLALevelId);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.PriorityId, p.SLALevelId });

        // Relationships
        builder.HasOne(p => p.Priority)
            .WithMany()
            .HasForeignKey(p => p.PriorityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.SLALevel)
            .WithMany(l => l.PrioritySLAs)
            .HasForeignKey(p => p.SLALevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
