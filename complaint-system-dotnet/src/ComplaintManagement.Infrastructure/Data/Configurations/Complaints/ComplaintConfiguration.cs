using ComplaintManagement.Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Complaints;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complaints");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ComplaintNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(c => c.ResolutionNotes)
            .HasMaxLength(4000);

        builder.Property(c => c.Tags)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(c => c.ComplaintNumber)
            .IsUnique();

        builder.HasIndex(c => c.ComplainantId);

        builder.HasIndex(c => c.AssignedToId);

        builder.HasIndex(c => c.StatusMasterId);

        builder.HasIndex(c => c.PriorityMasterId);

        builder.HasIndex(c => c.SubmittedAt);

        builder.HasIndex(c => c.DueDate);

        builder.HasIndex(c => new { c.CompanyId, c.StatusMasterId });

        builder.HasIndex(c => c.CurrentEscalationLevel);

        // Relationships
        builder.HasOne(c => c.Category)
            .WithMany(cc => cc.Complaints)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Complainant)
            .WithMany(u => u.SubmittedComplaints)
            .HasForeignKey(c => c.ComplainantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Branch)
            .WithMany()
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedTo)
            .WithMany(u => u.AssignedComplaints)
            .HasForeignKey(c => c.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.StatusMaster)
            .WithMany()
            .HasForeignKey(c => c.StatusMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PriorityMaster)
            .WithMany()
            .HasForeignKey(c => c.PriorityMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RelatedComplaint)
            .WithMany()
            .HasForeignKey(c => c.RelatedComplaintId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Comments)
            .WithOne(cc => cc.Complaint)
            .HasForeignKey(cc => cc.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Attachments)
            .WithOne(ca => ca.Complaint)
            .HasForeignKey(ca => ca.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.EscalationHistories)
            .WithOne(eh => eh.Complaint)
            .HasForeignKey(eh => eh.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
