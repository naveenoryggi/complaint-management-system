using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for AssetServiceHistory entity
/// </summary>
public class AssetServiceHistoryConfiguration : IEntityTypeConfiguration<AssetServiceHistory>
{
    public void Configure(EntityTypeBuilder<AssetServiceHistory> builder)
    {
        builder.ToTable("AssetServiceHistory");

        builder.HasKey(s => s.Id);

        #region Service Details

        builder.Property(s => s.ServiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.WorkPerformed)
            .HasMaxLength(4000);

        builder.Property(s => s.ProblemDescription)
            .HasMaxLength(2000);

        builder.Property(s => s.RootCause)
            .HasMaxLength(2000);

        builder.Property(s => s.Resolution)
            .HasMaxLength(2000);

        #endregion

        #region Timing

        builder.Property(s => s.DurationHours)
            .HasPrecision(10, 2);

        builder.Property(s => s.TravelTimeHours)
            .HasPrecision(10, 2);

        builder.Property(s => s.DowntimeHours)
            .HasPrecision(10, 2);

        #endregion

        #region Location

        builder.Property(s => s.ServiceAddress)
            .HasMaxLength(500);

        #endregion

        #region Costs

        builder.Property(s => s.LaborCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.PartsCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.TravelCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.OtherCosts)
            .HasPrecision(18, 4);

        builder.Property(s => s.TotalCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(s => s.InvoiceNumber)
            .HasMaxLength(100);

        #endregion

        #region Parts

        builder.Property(s => s.PartsUsed)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.PartsReplaced)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.ReplacedPartSerials)
            .HasMaxLength(2000);

        #endregion

        #region Asset Updates

        builder.Property(s => s.MeterReadingBefore)
            .HasPrecision(18, 4);

        builder.Property(s => s.MeterReadingAfter)
            .HasPrecision(18, 4);

        builder.Property(s => s.FirmwareVersionBefore)
            .HasMaxLength(100);

        builder.Property(s => s.FirmwareVersionAfter)
            .HasMaxLength(100);

        builder.Property(s => s.SoftwareVersionBefore)
            .HasMaxLength(100);

        builder.Property(s => s.SoftwareVersionAfter)
            .HasMaxLength(100);

        builder.Property(s => s.ConfigurationChanges)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Signatures & Approval

        builder.Property(s => s.SignedOffBy)
            .HasMaxLength(200);

        builder.Property(s => s.CustomerFeedback)
            .HasMaxLength(2000);

        #endregion

        #region Additional Information

        builder.Property(s => s.InternalNotes)
            .HasMaxLength(4000);

        builder.Property(s => s.Recommendations)
            .HasMaxLength(2000);

        builder.Property(s => s.FollowUpActions)
            .HasMaxLength(2000);

        builder.Property(s => s.Attachments)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Tags)
            .HasMaxLength(1000);

        builder.Property(s => s.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.ExternalReferenceNumber)
            .HasMaxLength(100);

        #endregion

        #region Indexes

        builder.HasIndex(s => s.ServiceNumber)
            .IsUnique();

        builder.HasIndex(s => s.CompanyId);

        builder.HasIndex(s => s.AssetId);

        builder.HasIndex(s => s.ComplaintId);

        builder.HasIndex(s => s.ContractId);

        builder.HasIndex(s => s.TechnicianId);

        builder.HasIndex(s => s.ServiceType);

        builder.HasIndex(s => s.Result);

        builder.HasIndex(s => s.ServiceStartDate);

        builder.HasIndex(s => s.ServiceEndDate);

        builder.HasIndex(s => new { s.AssetId, s.ServiceStartDate });

        builder.HasIndex(s => new { s.CompanyId, s.ServiceStartDate });

        #endregion

        // Query filter for soft delete
        builder.HasQueryFilter(s => !s.IsDeleted);

        #region Relationships

        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Asset)
            .WithMany(a => a.ServiceHistory)
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Contract)
            .WithMany()
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        #endregion
    }
}
