using ComplaintManagement.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Roles;

public class ComplaintRoleConfiguration : IEntityTypeConfiguration<ComplaintRole>
{
    public void Configure(EntityTypeBuilder<ComplaintRole> builder)
    {
        builder.ToTable("ComplaintRoles");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cr => cr.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cr => cr.Description)
            .HasMaxLength(1000);

        builder.Property(cr => cr.RoleType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(cr => cr.Code)
            .IsUnique();

        builder.HasIndex(cr => cr.RoleType);

        builder.HasIndex(cr => cr.EscalationLevel);

        // Relationships
        builder.HasMany(cr => cr.UserRoles)
            .WithOne(ucr => ucr.ComplaintRole)
            .HasForeignKey(ucr => ucr.ComplaintRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cr => cr.RolePermissions)
            .WithOne(crp => crp.ComplaintRole)
            .HasForeignKey(crp => crp.ComplaintRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
