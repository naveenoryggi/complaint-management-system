using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.ToTable("ProjectMilestones");

        builder.HasKey(m => m.Id);

        // Identity
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        // Status & Timeline
        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.DueDate)
            .IsRequired();

        // Additional Information
        builder.Property(m => m.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(m => m.ProjectId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.DueDate);
        builder.HasIndex(m => new { m.ProjectId, m.SortOrder });

        // Relationships
        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Tasks)
            .WithOne(t => t.Milestone)
            .HasForeignKey(t => t.MilestoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filter for soft delete
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
