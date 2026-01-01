using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for Asset entity
/// </summary>
public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        #region Identity

        builder.Property(a => a.AssetTag)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.SerialNumber)
            .HasMaxLength(100);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.Barcode)
            .HasMaxLength(100);

        builder.Property(a => a.RFIDTag)
            .HasMaxLength(100);

        #endregion

        #region Product Details

        builder.Property(a => a.ProductName)
            .HasMaxLength(300);

        builder.Property(a => a.ProductCode)
            .HasMaxLength(100);

        builder.Property(a => a.Manufacturer)
            .HasMaxLength(200);

        builder.Property(a => a.Model)
            .HasMaxLength(200);

        builder.Property(a => a.Version)
            .HasMaxLength(50);

        builder.Property(a => a.SKU)
            .HasMaxLength(100);

        #endregion

        #region Financials

        builder.Property(a => a.PurchasePrice)
            .HasPrecision(18, 4);

        builder.Property(a => a.CurrentValue)
            .HasPrecision(18, 4);

        builder.Property(a => a.SalvageValue)
            .HasPrecision(18, 4);

        builder.Property(a => a.ReplacementCost)
            .HasPrecision(18, 4);

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(a => a.PONumber)
            .HasMaxLength(100);

        builder.Property(a => a.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(a => a.Vendor)
            .HasMaxLength(200);

        #endregion

        #region Depreciation

        builder.Property(a => a.DepreciationRate)
            .HasPrecision(5, 2);

        builder.Property(a => a.AccumulatedDepreciation)
            .HasPrecision(18, 4);

        #endregion

        #region Ownership

        builder.Property(a => a.LeaseAgreementNumber)
            .HasMaxLength(100);

        builder.Property(a => a.LeasePayment)
            .HasPrecision(18, 4);

        #endregion

        #region Status

        builder.Property(a => a.StatusReason)
            .HasMaxLength(500);

        #endregion

        #region Configuration

        builder.Property(a => a.Specifications)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Configuration)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.FirmwareVersion)
            .HasMaxLength(100);

        builder.Property(a => a.SoftwareVersion)
            .HasMaxLength(100);

        builder.Property(a => a.OperatingSystem)
            .HasMaxLength(200);

        builder.Property(a => a.IPAddress)
            .HasMaxLength(50);

        builder.Property(a => a.MACAddress)
            .HasMaxLength(50);

        builder.Property(a => a.Hostname)
            .HasMaxLength(200);

        #endregion

        #region Physical Location

        builder.Property(a => a.Building)
            .HasMaxLength(100);

        builder.Property(a => a.Floor)
            .HasMaxLength(50);

        builder.Property(a => a.Room)
            .HasMaxLength(100);

        builder.Property(a => a.Rack)
            .HasMaxLength(50);

        builder.Property(a => a.RackUnit)
            .HasMaxLength(20);

        builder.Property(a => a.Latitude)
            .HasPrecision(10, 7);

        builder.Property(a => a.Longitude)
            .HasPrecision(10, 7);

        #endregion

        #region Maintenance

        builder.Property(a => a.TotalDowntimeHours)
            .HasPrecision(10, 2);

        builder.Property(a => a.MTBF)
            .HasPrecision(10, 2);

        builder.Property(a => a.MTTR)
            .HasPrecision(10, 2);

        #endregion

        #region Usage Tracking

        builder.Property(a => a.CurrentMeterReading)
            .HasPrecision(18, 4);

        builder.Property(a => a.MeterUnit)
            .HasMaxLength(50);

        builder.Property(a => a.AverageDailyUsage)
            .HasPrecision(18, 4);

        #endregion

        #region Assignment

        builder.Property(a => a.CostCenter)
            .HasMaxLength(100);

        #endregion

        #region External References

        builder.Property(a => a.ExternalAssetId)
            .HasMaxLength(100);

        #endregion

        #region Additional Information

        builder.Property(a => a.Notes)
            .HasMaxLength(4000);

        builder.Property(a => a.Tags)
            .HasMaxLength(1000);

        builder.Property(a => a.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Documents)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Images)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Indexes

        builder.HasIndex(a => a.AssetTag)
            .IsUnique();

        builder.HasIndex(a => a.SerialNumber);

        builder.HasIndex(a => a.CompanyId);

        builder.HasIndex(a => a.CustomerId);

        builder.HasIndex(a => a.ProductId);

        builder.HasIndex(a => a.ContractId);

        builder.HasIndex(a => a.Status);

        builder.HasIndex(a => a.Type);

        builder.HasIndex(a => a.Condition);

        builder.HasIndex(a => new { a.CompanyId, a.Status });

        builder.HasIndex(a => new { a.CustomerId, a.Status });

        builder.HasIndex(a => new { a.CompanyId, a.CustomerId });

        builder.HasIndex(a => a.Barcode);

        builder.HasIndex(a => a.RFIDTag);

        builder.HasIndex(a => a.WarrantyEndDate);

        builder.HasIndex(a => a.NextServiceDate);

        #endregion

        // Query filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);

        #region Relationships

        builder.HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Location)
            .WithMany(l => l.Assets)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Product)
            .WithMany(p => p.Assets)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Contract)
            .WithMany()
            .HasForeignKey(a => a.ContractId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Warranty)
            .WithMany()
            .HasForeignKey(a => a.WarrantyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.ParentAsset)
            .WithMany(a => a.ChildAssets)
            .HasForeignKey(a => a.ParentAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.ServiceHistory)
            .WithOne(s => s.Asset)
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ContractItems)
            .WithOne(ci => ci.Asset)
            .HasForeignKey(ci => ci.AssetId)
            .OnDelete(DeleteBehavior.NoAction);

        #endregion
    }
}
