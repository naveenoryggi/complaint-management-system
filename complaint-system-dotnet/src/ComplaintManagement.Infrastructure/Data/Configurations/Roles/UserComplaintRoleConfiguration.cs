using ComplaintManagement.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Roles;

public class UserComplaintRoleConfiguration : IEntityTypeConfiguration<UserComplaintRole>
{
    public void Configure(EntityTypeBuilder<UserComplaintRole> builder)
    {
        builder.ToTable("UserComplaintRoles");

        builder.HasKey(ucr => ucr.Id);

        builder.Property(ucr => ucr.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(ucr => ucr.UserId);

        builder.HasIndex(ucr => ucr.ComplaintRoleId);

        builder.HasIndex(ucr => new { ucr.UserId, ucr.ComplaintRoleId, ucr.IsActive });

        builder.HasIndex(ucr => ucr.CompanyId);

        builder.HasIndex(ucr => ucr.BranchId);

        builder.HasIndex(ucr => ucr.DepartmentId);

        builder.HasIndex(ucr => ucr.SectionId);

        builder.HasIndex(ucr => new { ucr.EffectiveFrom, ucr.EffectiveTo });

        // Relationships
        builder.HasOne(ucr => ucr.User)
            .WithMany(u => u.UserComplaintRoles)
            .HasForeignKey(ucr => ucr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ucr => ucr.ComplaintRole)
            .WithMany(cr => cr.UserRoles)
            .HasForeignKey(ucr => ucr.ComplaintRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ucr => ucr.Company)
            .WithMany()
            .HasForeignKey(ucr => ucr.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ucr => ucr.Branch)
            .WithMany()
            .HasForeignKey(ucr => ucr.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ucr => ucr.Department)
            .WithMany()
            .HasForeignKey(ucr => ucr.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ucr => ucr.Section)
            .WithMany()
            .HasForeignKey(ucr => ucr.SectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
