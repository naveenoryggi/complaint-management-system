using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProductCode)
            .HasMaxLength(100);

        builder.Property(s => s.ProductName)
            .HasMaxLength(200);

        builder.Property(s => s.SKU)
            .HasMaxLength(100);

        builder.Property(s => s.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(s => s.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(s => s.LastPurchasePrice)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityOnHand)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityReserved)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityInTransit)
            .HasPrecision(18, 4);

        builder.Property(s => s.QuantityOnOrder)
            .HasPrecision(18, 4);

        builder.Property(s => s.MinimumQuantity)
            .HasPrecision(18, 4);

        builder.Property(s => s.MaximumQuantity)
            .HasPrecision(18, 4);

        builder.Property(s => s.ReorderQuantity)
            .HasPrecision(18, 4);

        builder.Property(s => s.SafetyStock)
            .HasPrecision(18, 4);

        builder.Property(s => s.LastCountQuantity)
            .HasPrecision(18, 4);

        builder.Property(s => s.LastCountVariance)
            .HasPrecision(18, 4);

        builder.Property(s => s.BatchNumber)
            .HasMaxLength(100);

        builder.Property(s => s.AttentionReason)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Unique constraint on Product + Location + Category per company
        builder.HasIndex(s => new { s.CompanyId, s.ProductId, s.LocationId, s.StockCategoryId })
            .IsUnique();

        // Relationship with Company
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Product
        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with StockCategory
        builder.HasOne(s => s.StockCategory)
            .WithMany()
            .HasForeignKey(s => s.StockCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Location
        builder.HasOne(s => s.Location)
            .WithMany(l => l.StockItems)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
