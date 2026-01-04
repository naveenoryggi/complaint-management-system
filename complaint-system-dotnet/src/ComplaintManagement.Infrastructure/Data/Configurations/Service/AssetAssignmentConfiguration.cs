using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for AssetAssignment entity
/// Tracks the complete assignment history of assets to employees
/// </summary>
public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("AssetAssignments");

        builder.HasKey(a => a.Id);

        #region Assignment Details

        builder.Property(a => a.AssignmentDate)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        #endregion

        #region Condition Tracking

        builder.Property(a => a.ConditionNotesAtAssignment)
            .HasMaxLength(2000);

        builder.Property(a => a.ConditionNotesAtReturn)
            .HasMaxLength(2000);

        #endregion

        #region Location

        builder.Property(a => a.Location)
            .HasMaxLength(500);

        builder.Property(a => a.CostCenter)
            .HasMaxLength(100);

        #endregion

        #region Documentation

        builder.Property(a => a.AssignmentNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Notes)
            .HasMaxLength(4000);

        builder.Property(a => a.AcknowledgementReference)
            .HasMaxLength(200);

        builder.Property(a => a.Documents)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Indexes

        // Primary lookups
        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => a.AssetId);
        builder.HasIndex(a => a.AssignedToUserId);
        builder.HasIndex(a => a.AssignedByUserId);
        builder.HasIndex(a => a.AssignedToCustomerId);
        builder.HasIndex(a => a.ResponsibleUserId);

        // Active assignments per user (employee asset dashboard)
        builder.HasIndex(a => new { a.AssignedToUserId, a.IsActive });

        // Active assignments per customer
        builder.HasIndex(a => new { a.AssignedToCustomerId, a.IsActive });

        // Active assignments tracked by responsible user
        builder.HasIndex(a => new { a.ResponsibleUserId, a.IsActive });

        // Active assignments per asset
        builder.HasIndex(a => new { a.AssetId, a.IsActive });

        // Assignment history by date
        builder.HasIndex(a => new { a.CompanyId, a.AssignmentDate });

        // Overdue returns tracking
        builder.HasIndex(a => new { a.ExpectedReturnDate, a.IsActive })
            .HasFilter("[ExpectedReturnDate] IS NOT NULL AND [IsActive] = 1");

        // Assignment number uniqueness (per company)
        builder.HasIndex(a => new { a.CompanyId, a.AssignmentNumber })
            .IsUnique()
            .HasFilter("[AssignmentNumber] IS NOT NULL");

        #endregion

        // Query filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);

        #region Relationships

        builder.HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Asset)
            .WithMany(asset => asset.AssignmentHistory)
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AssignedToCustomer)
            .WithMany()
            .HasForeignKey(a => a.AssignedToCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ResponsibleUser)
            .WithMany()
            .HasForeignKey(a => a.ResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
