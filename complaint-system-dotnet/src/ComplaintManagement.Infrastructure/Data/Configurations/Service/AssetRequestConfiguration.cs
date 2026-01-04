using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class AssetRequestConfiguration : IEntityTypeConfiguration<AssetRequest>
{
    public void Configure(EntityTypeBuilder<AssetRequest> builder)
    {
        builder.ToTable("AssetRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.RequestType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AssetRequestStatus.Draft);

        builder.Property(r => r.ReturnReason)
            .HasMaxLength(1000);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.EscalationLevel)
            .HasDefaultValue(0);

        builder.Property(r => r.Purpose)
            .HasConversion<int?>();

        // Unique constraint on RequestNumber per Company
        builder.HasIndex(r => new { r.CompanyId, r.RequestNumber })
            .IsUnique()
            .HasDatabaseName("IX_AssetRequests_CompanyId_RequestNumber");

        // Index for performance on common queries
        builder.HasIndex(r => new { r.CompanyId, r.Status })
            .HasDatabaseName("IX_AssetRequests_CompanyId_Status");

        builder.HasIndex(r => new { r.CurrentApproverId, r.Status })
            .HasDatabaseName("IX_AssetRequests_CurrentApproverId_Status");

        builder.HasIndex(r => r.ApprovalDeadline)
            .HasDatabaseName("IX_AssetRequests_ApprovalDeadline");

        // Foreign key relationships
        // Using NoAction to avoid SQL Server multiple cascade path issues
        builder.HasOne(r => r.Asset)
            .WithMany()
            .HasForeignKey(r => r.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssetAssignment)
            .WithMany()
            .HasForeignKey(r => r.AssetAssignmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Store)
            .WithMany(s => s.AssetRequests)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.RequestedBy)
            .WithMany()
            .HasForeignKey(r => r.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignToUser)
            .WithMany()
            .HasForeignKey(r => r.AssignToUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.CurrentApprover)
            .WithMany()
            .HasForeignKey(r => r.CurrentApproverId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.ApprovedBy)
            .WithMany()
            .HasForeignKey(r => r.ApprovedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.RejectedBy)
            .WithMany()
            .HasForeignKey(r => r.RejectedById)
            .OnDelete(DeleteBehavior.NoAction);

        // Soft delete filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
