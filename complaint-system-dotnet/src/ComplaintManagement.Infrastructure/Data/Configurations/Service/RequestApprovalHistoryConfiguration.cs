using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class RequestApprovalHistoryConfiguration : IEntityTypeConfiguration<RequestApprovalHistory>
{
    public void Configure(EntityTypeBuilder<RequestApprovalHistory> builder)
    {
        builder.ToTable("RequestApprovalHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.ActionAt)
            .IsRequired();

        builder.Property(h => h.Comments)
            .HasMaxLength(2000);

        builder.Property(h => h.EscalationLevel)
            .HasDefaultValue(0);

        builder.Property(h => h.PreviousStatus)
            .HasConversion<int?>();

        builder.Property(h => h.NewStatus)
            .HasConversion<int?>();

        builder.Property(h => h.IpAddress)
            .HasMaxLength(50);

        builder.Property(h => h.UserAgent)
            .HasMaxLength(500);

        // Index for finding history by request
        builder.HasIndex(h => h.RequestId)
            .HasDatabaseName("IX_RequestApprovalHistory_RequestId");

        // Index for audit queries
        builder.HasIndex(h => new { h.ActorId, h.ActionAt })
            .HasDatabaseName("IX_RequestApprovalHistory_ActorId_ActionAt");

        // Foreign key relationships
        // Using NoAction to avoid SQL Server multiple cascade path issues
        builder.HasOne(h => h.Request)
            .WithMany(r => r.ApprovalHistory)
            .HasForeignKey(h => h.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Actor)
            .WithMany()
            .HasForeignKey(h => h.ActorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(h => h.PreviousApprover)
            .WithMany()
            .HasForeignKey(h => h.PreviousApproverId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(h => h.NewApprover)
            .WithMany()
            .HasForeignKey(h => h.NewApproverId)
            .OnDelete(DeleteBehavior.NoAction);

        // Soft delete filter
        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
