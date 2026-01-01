using ComplaintManagement.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Product;

public class ProductSubTypeConfiguration : IEntityTypeConfiguration<ProductSubType>
{
    public void Configure(EntityTypeBuilder<ProductSubType> builder)
    {
        builder.ToTable("ProductSubTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .HasMaxLength(20);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.CompanyId, x.CategoryId, x.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Name);

        // Relationships
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.SubTypes)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
