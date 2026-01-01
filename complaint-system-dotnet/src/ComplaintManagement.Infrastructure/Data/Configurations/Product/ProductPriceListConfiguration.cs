using ComplaintManagement.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Product;

public class ProductPriceListConfiguration : IEntityTypeConfiguration<ProductPriceList>
{
    public void Configure(EntityTypeBuilder<ProductPriceList> builder)
    {
        builder.ToTable("ProductPriceLists");

        builder.HasKey(p => p.Id);

        // Identity
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<int>();

        // Target Audience
        builder.Property(p => p.PartnerTier)
            .HasMaxLength(50);

        builder.Property(p => p.CustomerSegment)
            .HasMaxLength(50);

        // Pricing
        builder.Property(p => p.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(p => p.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(p => p.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(p => p.MarkupPercent)
            .HasPrecision(5, 2);

        // Quantity Tiers
        builder.Property(p => p.MinQuantity)
            .HasPrecision(18, 4);

        builder.Property(p => p.MaxQuantity)
            .HasPrecision(18, 4);

        builder.Property(p => p.TierPricing)
            .HasColumnType("nvarchar(max)");

        // Conditions
        builder.Property(p => p.MinOrderValue)
            .HasPrecision(18, 4);

        builder.Property(p => p.PromoCode)
            .HasMaxLength(50);

        builder.Property(p => p.Conditions)
            .HasColumnType("nvarchar(max)");

        // Metadata
        builder.Property(p => p.ApprovalStatus)
            .HasMaxLength(50);

        builder.Property(p => p.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(p => p.ProductId);
        builder.HasIndex(p => p.PartnerId);
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.ValidFrom);
        builder.HasIndex(p => p.ValidTo);
        builder.HasIndex(p => p.Priority);
        builder.HasIndex(p => p.PromoCode);

        // Unique constraint for standard pricing per product
        builder.HasIndex(p => new { p.ProductId, p.Type, p.PartnerId, p.CustomerId, p.MinQuantity })
            .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasOne(p => p.Product)
            .WithMany(prod => prod.PriceLists)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Partner)
            .WithMany()
            .HasForeignKey(p => p.PartnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed properties
        builder.Ignore(p => p.IsValid);
        builder.Ignore(p => p.IsPromotional);
        builder.Ignore(p => p.EffectiveDiscount);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
