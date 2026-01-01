using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        // Identity
        builder.Property(p => p.ProjectCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        // Budget
        builder.Property(p => p.Budget)
            .HasPrecision(18, 2);

        builder.Property(p => p.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        // Additional Information
        builder.Property(p => p.Tags)
            .HasMaxLength(1000);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.CustomFields)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(p => new { p.CompanyId, p.ProjectCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ProjectManagerId);
        builder.HasIndex(p => p.PlannedStartDate);
        builder.HasIndex(p => p.PlannedEndDate);

        // Relationships
        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProjectManager)
            .WithMany()
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Milestones)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Documents)
            .WithOne(d => d.Project)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.TeamMembers)
            .WithOne(tm => tm.Project)
            .HasForeignKey(tm => tm.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Complaints)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
