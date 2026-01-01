using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for ContractItem entity
/// </summary>
public class ContractItemConfiguration : IEntityTypeConfiguration<ContractItem>
{
    public void Configure(EntityTypeBuilder<ContractItem> builder)
    {
        builder.ToTable("ContractItems");

        builder.HasKey(ci => ci.Id);

        // Item Details
        builder.Property(ci => ci.ItemDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ci => ci.SerialNumbers)
            .HasColumnType("nvarchar(max)");

        builder.Property(ci => ci.UnitOfMeasure)
            .HasMaxLength(50);

        // Financials
        builder.Property(ci => ci.ItemValue)
            .HasPrecision(18, 4);

        builder.Property(ci => ci.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(ci => ci.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        // Location
        builder.Property(ci => ci.SiteInfo)
            .HasMaxLength(500);

        // Notes
        builder.Property(ci => ci.SpecialConditions)
            .HasMaxLength(1000);

        builder.Property(ci => ci.Notes)
            .HasMaxLength(2000);

        builder.Property(ci => ci.CustomFields)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(ci => ci.ContractId);
        builder.HasIndex(ci => ci.ProductId);
        builder.HasIndex(ci => ci.AssetId);
        builder.HasIndex(ci => ci.Status);
        builder.HasIndex(ci => new { ci.ContractId, ci.Status });

        // Query filter for soft delete
        builder.HasQueryFilter(ci => !ci.IsDeleted);

        // Relationships
        builder.HasOne(ci => ci.Contract)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Asset relationship will be added when Asset entity is created
    }
}
