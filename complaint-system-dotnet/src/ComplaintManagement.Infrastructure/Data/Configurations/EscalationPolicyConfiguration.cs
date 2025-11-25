using ComplaintManagement.Domain.Entities.Escalation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations;

public class EscalationPolicyConfiguration : IEntityTypeConfiguration<EscalationPolicy>
{
    public void Configure(EntityTypeBuilder<EscalationPolicy> builder)
    {
        builder.ToTable("EscalationPolicies");

        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ep => ep.Description)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(ep => ep.Company)
            .WithMany()
            .HasForeignKey(ep => ep.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ep => ep.Branch)
            .WithMany()
            .HasForeignKey(ep => ep.BranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ep => ep.Department)
            .WithMany()
            .HasForeignKey(ep => ep.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ep => ep.Section)
            .WithMany()
            .HasForeignKey(ep => ep.SectionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ep => ep.Category)
            .WithMany()
            .HasForeignKey(ep => ep.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ep => ep.DefaultEscalationMatrix)
            .WithMany()
            .HasForeignKey(ep => ep.DefaultEscalationMatrixId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes for efficient policy lookup
        builder.HasIndex(ep => new { ep.CompanyId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_CompanyId_IsActive");

        builder.HasIndex(ep => new { ep.CompanyId, ep.BranchId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_CompanyId_BranchId_IsActive");

        builder.HasIndex(ep => new { ep.CompanyId, ep.DepartmentId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_CompanyId_DepartmentId_IsActive");

        builder.HasIndex(ep => new { ep.CompanyId, ep.SectionId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_CompanyId_SectionId_IsActive");

        builder.HasIndex(ep => new { ep.CompanyId, ep.CategoryId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_CompanyId_CategoryId_IsActive");

        // Composite index for most common query pattern
        builder.HasIndex(ep => new { ep.CompanyId, ep.BranchId, ep.DepartmentId, ep.SectionId, ep.CategoryId, ep.IsActive })
            .HasDatabaseName("IX_EscalationPolicies_FullHierarchy");
    }
}
