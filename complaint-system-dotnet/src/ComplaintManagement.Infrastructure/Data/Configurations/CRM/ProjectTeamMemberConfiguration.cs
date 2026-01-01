using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class ProjectTeamMemberConfiguration : IEntityTypeConfiguration<ProjectTeamMember>
{
    public void Configure(EntityTypeBuilder<ProjectTeamMember> builder)
    {
        builder.ToTable("ProjectTeamMembers");

        builder.HasKey(tm => tm.Id);

        // Role Information
        builder.Property(tm => tm.Role)
            .HasMaxLength(100);

        // Timeline
        builder.Property(tm => tm.JoinedDate)
            .IsRequired();

        // Additional Information
        builder.Property(tm => tm.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(tm => tm.ProjectId);
        builder.HasIndex(tm => tm.EmployeeId);
        builder.HasIndex(tm => tm.IsActive);
        builder.HasIndex(tm => new { tm.ProjectId, tm.EmployeeId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasOne(tm => tm.Project)
            .WithMany(p => p.TeamMembers)
            .HasForeignKey(tm => tm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tm => tm.Employee)
            .WithMany()
            .HasForeignKey(tm => tm.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(tm => !tm.IsDeleted);
    }
}
