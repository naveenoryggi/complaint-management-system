using ComplaintManagement.Domain.Entities.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Workflows;

public class CategoryWorkflowTransitionConfiguration : IEntityTypeConfiguration<CategoryWorkflowTransition>
{
    public void Configure(EntityTypeBuilder<CategoryWorkflowTransition> builder)
    {
        builder.ToTable("CategoryWorkflowTransitions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransitionName)
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.RequiresComment)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.RequiresApproval)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.AllowedRoles)
            .HasMaxLength(2000);

        builder.Property(t => t.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.IsAutomatic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.TransitionConditions)
            .HasMaxLength(4000);

        builder.Property(t => t.ButtonColor)
            .HasMaxLength(50);

        builder.Property(t => t.IconClass)
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(t => t.Workflow)
            .WithMany(w => w.Transitions)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.FromStatus)
            .WithMany()
            .HasForeignKey(t => t.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToStatus)
            .WithMany()
            .HasForeignKey(t => t.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.WorkflowId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(t => t.FromStatusId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(t => t.ToStatusId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(t => new { t.WorkflowId, t.FromStatusId })
            .HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");

        // Unique constraint: Only one transition per from-to pair per workflow
        builder.HasIndex(t => new { t.WorkflowId, t.FromStatusId, t.ToStatusId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Query Filter (Global)
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
