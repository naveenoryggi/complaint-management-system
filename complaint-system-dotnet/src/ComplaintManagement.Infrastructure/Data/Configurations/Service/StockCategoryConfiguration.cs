using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StockCategoryConfiguration : IEntityTypeConfiguration<StockCategory>
{
    public void Configure(EntityTypeBuilder<StockCategory> builder)
    {
        builder.ToTable("StockCategories");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.ColorCode)
            .HasMaxLength(20);

        builder.Property(s => s.Icon)
            .HasMaxLength(50);

        // Unique constraint on Code per company
        builder.HasIndex(s => new { s.CompanyId, s.Code })
            .IsUnique();

        // Relationship with Company
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Assets
        builder.HasMany(s => s.Assets)
            .WithOne(a => a.StockCategory)
            .HasForeignKey(a => a.StockCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
