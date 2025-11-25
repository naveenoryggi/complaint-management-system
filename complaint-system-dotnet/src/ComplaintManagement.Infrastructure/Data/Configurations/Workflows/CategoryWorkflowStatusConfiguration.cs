using ComplaintManagement.Domain.Entities.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Workflows;

public class CategoryWorkflowStatusConfiguration : IEntityTypeConfiguration<CategoryWorkflowStatus>
{
    public void Configure(EntityTypeBuilder<CategoryWorkflowStatus> builder)
    {
        builder.ToTable("CategoryWorkflowStatuses");

        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.DisplayOrder)
            .IsRequired();

        builder.Property(ws => ws.IsInitialStatus)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ws => ws.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ws => ws.RequiresApproval)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ws => ws.AllowedRoles)
            .HasMaxLength(2000);

        builder.Property(ws => ws.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(ws => ws.Workflow)
            .WithMany(w => w.WorkflowStatuses)
            .HasForeignKey(ws => ws.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ws => ws.StatusMaster)
            .WithMany()
            .HasForeignKey(ws => ws.StatusMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ws => ws.WorkflowId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(ws => ws.StatusMasterId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(ws => new { ws.WorkflowId, ws.DisplayOrder })
            .HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");

        // Unique constraint: Only one status per workflow
        builder.HasIndex(ws => new { ws.WorkflowId, ws.StatusMasterId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Query Filter (Global)
        builder.HasQueryFilter(ws => !ws.IsDeleted);
    }
}
