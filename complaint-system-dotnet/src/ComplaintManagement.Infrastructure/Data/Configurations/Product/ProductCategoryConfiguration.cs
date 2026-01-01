using ComplaintManagement.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Product;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        // Identity
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        // Hierarchy
        builder.Property(c => c.Path)
            .HasMaxLength(500);

        builder.Property(c => c.FullPath)
            .HasMaxLength(1000);

        // Display
        builder.Property(c => c.Icon)
            .HasMaxLength(100);

        builder.Property(c => c.Color)
            .HasMaxLength(20);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.ThumbnailUrl)
            .HasMaxLength(500);

        // Defaults
        builder.Property(c => c.DefaultTaxRate)
            .HasPrecision(5, 2);

        builder.Property(c => c.DefaultHSNCode)
            .HasMaxLength(20);

        builder.Property(c => c.DefaultSACCode)
            .HasMaxLength(20);

        // SEO
        builder.Property(c => c.Slug)
            .HasMaxLength(200);

        builder.Property(c => c.MetaTitle)
            .HasMaxLength(200);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(500);

        builder.Property(c => c.Keywords)
            .HasMaxLength(1000);

        // External
        builder.Property(c => c.ExternalCategoryId)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.CustomAttributes)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(c => new { c.CompanyId, c.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => c.ParentCategoryId);
        builder.HasIndex(c => c.Path);
        builder.HasIndex(c => c.Level);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.Slug);

        // Relationships
        builder.HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.DefaultSLASettings)
            .WithMany()
            .HasForeignKey(c => c.DefaultSLASettingsId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed properties
        builder.Ignore(c => c.IsRoot);
        builder.Ignore(c => c.HasChildren);
        builder.Ignore(c => c.ChildCount);

        // Global query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
