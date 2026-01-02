using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.MovementNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(s => s.SerialNumber)
            .HasMaxLength(200);

        builder.Property(s => s.BatchNumber)
            .HasMaxLength(100);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(s => s.Reason)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        builder.Property(s => s.Quantity)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityBefore)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityAfter)
            .HasPrecision(18, 4);

        builder.Property(s => s.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.TotalValue)
            .HasPrecision(18, 4);

        // Unique constraint on MovementNumber per company
        builder.HasIndex(s => new { s.CompanyId, s.MovementNumber })
            .IsUnique();

        // Index for faster lookups
        builder.HasIndex(s => s.MovementDate);
        builder.HasIndex(s => s.MovementType);
        builder.HasIndex(s => new { s.ProductId, s.MovementDate });

        // Relationship with Company
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with StockItem (NoAction to avoid cascade path issues)
        builder.HasOne(s => s.StockItem)
            .WithMany(si => si.Movements)
            .HasForeignKey(s => s.StockItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship with Product
        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Locations (NoAction to avoid cascade path issues)
        builder.HasOne(s => s.FromLocation)
            .WithMany(l => l.MovementsFrom)
            .HasForeignKey(s => s.FromLocationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.ToLocation)
            .WithMany(l => l.MovementsTo)
            .HasForeignKey(s => s.ToLocationId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship with Stock Categories (NoAction to avoid cascade path issues)
        builder.HasOne(s => s.FromStockCategory)
            .WithMany()
            .HasForeignKey(s => s.FromStockCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.ToStockCategory)
            .WithMany()
            .HasForeignKey(s => s.ToStockCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship with Customer (NoAction to avoid cascade path issues)
        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship with Asset (NoAction to avoid cascade path issues)
        builder.HasOne(s => s.Asset)
            .WithMany()
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
