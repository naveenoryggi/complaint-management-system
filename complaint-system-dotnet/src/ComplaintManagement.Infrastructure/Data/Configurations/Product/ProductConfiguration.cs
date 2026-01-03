using ComplaintManagement.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Product;

public class ProductConfiguration : IEntityTypeConfiguration<Domain.Entities.Product.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Product.Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        #region Identity

        builder.Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<int>();

        #endregion

        #region SKU & Identification

        builder.Property(p => p.SKU)
            .HasMaxLength(100);

        builder.Property(p => p.PartNumber)
            .HasMaxLength(100);

        builder.Property(p => p.UPC)
            .HasMaxLength(50);

        builder.Property(p => p.EAN)
            .HasMaxLength(50);

        builder.Property(p => p.ISBN)
            .HasMaxLength(20);

        builder.Property(p => p.HSNCode)
            .HasMaxLength(20);

        builder.Property(p => p.SACCode)
            .HasMaxLength(20);

        #endregion

        #region Pricing

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.MSRP)
            .HasPrecision(18, 4);

        builder.Property(p => p.MinimumPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(p => p.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(p => p.TaxType)
            .HasConversion<int>();

        builder.Property(p => p.PriceBook)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Units & Quantity

        builder.Property(p => p.UnitOfMeasure)
            .HasConversion<int>();

        builder.Property(p => p.CustomUnitName)
            .HasMaxLength(50);

        builder.Property(p => p.MinOrderQuantity)
            .HasPrecision(18, 4);

        builder.Property(p => p.MaxOrderQuantity)
            .HasPrecision(18, 4);

        builder.Property(p => p.QuantityIncrement)
            .HasPrecision(18, 4);

        #endregion

        #region Product Details

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.Manufacturer)
            .HasMaxLength(200);

        builder.Property(p => p.Model)
            .HasMaxLength(100);

        builder.Property(p => p.Version)
            .HasMaxLength(50);

        builder.Property(p => p.Specifications)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Features)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Dimensions)
            .HasMaxLength(500);

        builder.Property(p => p.Weight)
            .HasPrecision(18, 4);

        builder.Property(p => p.CountryOfOrigin)
            .HasMaxLength(100);

        #endregion

        #region Media

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Images)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Videos)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Documents)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Warranty & Support

        builder.Property(p => p.WarrantyTerms)
            .HasMaxLength(2000);

        builder.Property(p => p.SupportUrl)
            .HasMaxLength(500);

        #endregion

        #region Subscription

        builder.Property(p => p.BillingFrequency)
            .HasConversion<int?>();

        builder.Property(p => p.SetupFee)
            .HasPrecision(18, 4);

        builder.Property(p => p.CancellationPolicy)
            .HasMaxLength(2000);

        #endregion

        #region Status

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.StatusReason)
            .HasMaxLength(500);

        #endregion

        #region SEO

        builder.Property(p => p.Slug)
            .HasMaxLength(300);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Tags)
            .HasMaxLength(1000);

        builder.Property(p => p.Keywords)
            .HasMaxLength(1000);

        #endregion

        #region External

        builder.Property(p => p.ExternalProductId)
            .HasMaxLength(100);

        builder.Property(p => p.VendorProductId)
            .HasMaxLength(100);

        builder.Property(p => p.VendorName)
            .HasMaxLength(200);

        builder.Property(p => p.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        #endregion

        #region Related Products

        builder.Property(p => p.RelatedProductIds)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CrossSellProductIds)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.UpSellProductIds)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.AccessoryProductIds)
            .HasColumnType("nvarchar(max)");

        #endregion

        #region Indexes

        builder.HasIndex(p => new { p.CompanyId, p.ProductCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(p => p.SKU);
        builder.HasIndex(p => p.PartNumber);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.SubTypeId);
        builder.HasIndex(p => p.ParentProductId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Brand);
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.Manufacturer);
        builder.HasIndex(p => p.Slug);
        builder.HasIndex(p => p.ExternalProductId);
        builder.HasIndex(p => p.HSNCode);
        builder.HasIndex(p => p.IsPublic);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.DefaultLocationId);
        builder.HasIndex(p => p.DefaultStockCategoryId);

        #endregion

        #region Relationships

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.SubType)
            .WithMany(st => st.Products)
            .HasForeignKey(p => p.SubTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.BrandEntity)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ParentProduct)
            .WithMany(p => p.Variants)
            .HasForeignKey(p => p.ParentProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DefaultSLASettings)
            .WithMany()
            .HasForeignKey(p => p.DefaultSLASettingsId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ReplacementProduct)
            .WithMany()
            .HasForeignKey(p => p.ReplacementProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Stock Management relationships
        builder.HasOne(p => p.DefaultLocation)
            .WithMany()
            .HasForeignKey(p => p.DefaultLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.DefaultStockCategory)
            .WithMany()
            .HasForeignKey(p => p.DefaultStockCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.StockItems)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        // Ignore computed properties (quantities are now in StockItems)
        builder.Ignore(p => p.TotalQuantityAvailable);
        builder.Ignore(p => p.TotalQuantityOnHand);
        builder.Ignore(p => p.TotalQuantityReserved);
        builder.Ignore(p => p.InStock);
        builder.Ignore(p => p.IsVariant);
        builder.Ignore(p => p.HasVariants);
        builder.Ignore(p => p.IsAvailable);
        builder.Ignore(p => p.IsDiscontinued);
        builder.Ignore(p => p.ProfitMargin);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
