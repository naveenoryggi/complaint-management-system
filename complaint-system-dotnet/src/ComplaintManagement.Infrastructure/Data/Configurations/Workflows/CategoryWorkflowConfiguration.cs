using ComplaintManagement.Domain.Entities.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Workflows;

public class CategoryWorkflowConfiguration : IEntityTypeConfiguration<CategoryWorkflow>
{
    public void Configure(EntityTypeBuilder<CategoryWorkflow> builder)
    {
        builder.ToTable("CategoryWorkflows");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.IsDefault)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(w => w.Category)
            .WithMany(c => c.Workflows)
            .HasForeignKey(w => w.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Company)
            .WithMany()
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.WorkflowStatuses)
            .WithOne(ws => ws.Workflow)
            .HasForeignKey(ws => ws.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transitions)
            .WithOne(t => t.Workflow)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.CategoryId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => w.CompanyId)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => new { w.CategoryId, w.IsDefault })
            .HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");

        // Query Filter (Global)
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
