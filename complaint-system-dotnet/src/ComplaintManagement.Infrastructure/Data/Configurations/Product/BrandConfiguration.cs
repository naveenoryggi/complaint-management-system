using ComplaintManagement.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Product;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);

        // Identity
        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        // Media
        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.WebsiteUrl)
            .HasMaxLength(500);

        // Location
        builder.Property(b => b.Country)
            .HasMaxLength(100);

        // Notes
        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(b => new { b.CompanyId, b.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.IsActive);

        // Relationships
        builder.HasOne(b => b.Company)
            .WithMany()
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
