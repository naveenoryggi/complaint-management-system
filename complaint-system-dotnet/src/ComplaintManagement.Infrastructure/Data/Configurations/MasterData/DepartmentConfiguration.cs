using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.OryggiDepartmentId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(d => new { d.BranchId, d.Code })
            .IsUnique();

        builder.HasIndex(d => d.OryggiDepartmentId);

        builder.HasIndex(d => d.ManagerId);
        builder.HasIndex(d => d.SecondaryManagerId);
        builder.HasIndex(d => d.HrResponsibleId);

        // Relationships
        builder.HasOne(d => d.Branch)
            .WithMany(b => b.Departments)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Manager relationships - NoAction to avoid multiple cascade paths
        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.SecondaryManager)
            .WithMany()
            .HasForeignKey(d => d.SecondaryManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.HrResponsible)
            .WithMany()
            .HasForeignKey(d => d.HrResponsibleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(d => d.Sections)
            .WithOne(s => s.Department)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Users)
            .WithOne(u => u.Department)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
