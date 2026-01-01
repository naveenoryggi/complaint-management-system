using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for Warranty entity
/// </summary>
public class WarrantyConfiguration : IEntityTypeConfiguration<Warranty>
{
    public void Configure(EntityTypeBuilder<Warranty> builder)
    {
        builder.ToTable("Warranties");

        builder.HasKey(w => w.Id);

        // Identity
        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        // Coverage - JSON fields
        builder.Property(w => w.CoveredItems)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.ExcludedItems)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.Terms)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.Conditions)
            .HasColumnType("nvarchar(max)");

        // Extended Warranty Pricing
        builder.Property(w => w.ExtendedWarrantyPrice)
            .HasPrecision(18, 4);

        builder.Property(w => w.ExtendedWarrantyPricePercent)
            .HasPrecision(5, 2);

        builder.Property(w => w.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        // Claims
        builder.Property(w => w.MaxClaimValue)
            .HasPrecision(18, 4);

        builder.Property(w => w.MaxClaimValuePercent)
            .HasPrecision(5, 2);

        builder.Property(w => w.DeductibleAmount)
            .HasPrecision(18, 4);

        // Metadata
        builder.Property(w => w.Notes)
            .HasMaxLength(2000);

        builder.Property(w => w.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.ExternalWarrantyId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(w => new { w.CompanyId, w.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => w.CompanyId);
        builder.HasIndex(w => w.ProductId);
        builder.HasIndex(w => w.CategoryId);
        builder.HasIndex(w => w.Type);
        builder.HasIndex(w => w.IsActive);
        builder.HasIndex(w => w.IsDefault);

        // Query filter for soft delete
        builder.HasQueryFilter(w => !w.IsDeleted);

        // Relationships
        builder.HasOne(w => w.Company)
            .WithMany()
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Category)
            .WithMany()
            .HasForeignKey(w => w.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
