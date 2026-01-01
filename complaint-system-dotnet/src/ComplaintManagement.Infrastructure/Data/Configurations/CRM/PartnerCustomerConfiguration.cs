using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class PartnerCustomerConfiguration : IEntityTypeConfiguration<PartnerCustomer>
{
    public void Configure(EntityTypeBuilder<PartnerCustomer> builder)
    {
        builder.ToTable("PartnerCustomers");

        builder.HasKey(pc => pc.Id);

        // Relationship Details
        builder.Property(pc => pc.CommissionPercent)
            .HasPrecision(5, 2);

        builder.Property(pc => pc.DiscountPercent)
            .HasPrecision(5, 2);

        // Additional Information
        builder.Property(pc => pc.EndReason)
            .HasMaxLength(500);

        builder.Property(pc => pc.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(pc => new { pc.PartnerId, pc.CustomerId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(pc => pc.IsPrimaryPartner);
        builder.HasIndex(pc => pc.IsActive);

        // Relationships
        builder.HasOne(pc => pc.Partner)
            .WithMany(p => p.PartnerCustomers)
            .HasForeignKey(pc => pc.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Customer)
            .WithMany(c => c.PartnerCustomers)
            .HasForeignKey(pc => pc.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter for soft delete
        builder.HasQueryFilter(pc => !pc.IsDeleted);
    }
}
