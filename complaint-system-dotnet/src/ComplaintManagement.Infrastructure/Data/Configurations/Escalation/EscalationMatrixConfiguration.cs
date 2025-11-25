using ComplaintManagement.Domain.Entities.Escalation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Escalation;

public class EscalationMatrixConfiguration : IEntityTypeConfiguration<EscalationMatrix>
{
    public void Configure(EntityTypeBuilder<EscalationMatrix> builder)
    {
        builder.ToTable("EscalationMatrices");

        builder.HasKey(em => em.Id);

        builder.Property(em => em.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(em => em.Description)
            .HasMaxLength(1000);

        builder.Property(em => em.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(em => em.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(em => em.EnableAutoEscalation)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(em => em.SendEmailNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(em => em.CompanyId);
        builder.HasIndex(em => em.CategoryId);
        builder.HasIndex(em => em.BranchId);
        builder.HasIndex(em => em.DepartmentId);
        builder.HasIndex(em => em.IsActive);
        builder.HasIndex(em => new { em.CompanyId, em.IsActive, em.Priority });

        // Relationships
        builder.HasOne(em => em.Company)
            .WithMany()
            .HasForeignKey(em => em.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(em => em.Category)
            .WithMany()
            .HasForeignKey(em => em.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(em => em.Branch)
            .WithMany()
            .HasForeignKey(em => em.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(em => em.Department)
            .WithMany()
            .HasForeignKey(em => em.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(em => em.EscalationLevels)
            .WithOne(el => el.EscalationMatrix)
            .HasForeignKey(el => el.EscalationMatrixId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
