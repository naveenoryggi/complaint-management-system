using ComplaintManagement.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Roles;

public class ComplaintRolePermissionConfiguration : IEntityTypeConfiguration<ComplaintRolePermission>
{
    public void Configure(EntityTypeBuilder<ComplaintRolePermission> builder)
    {
        builder.ToTable("ComplaintRolePermissions");

        builder.HasKey(crp => crp.Id);

        builder.Property(crp => crp.PermissionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(crp => crp.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(crp => crp.ComplaintRoleId);

        builder.HasIndex(crp => crp.PermissionType);

        builder.HasIndex(crp => new { crp.ComplaintRoleId, crp.PermissionType })
            .IsUnique();

        // Relationships
        builder.HasOne(crp => crp.ComplaintRole)
            .WithMany(cr => cr.RolePermissions)
            .HasForeignKey(crp => crp.ComplaintRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
