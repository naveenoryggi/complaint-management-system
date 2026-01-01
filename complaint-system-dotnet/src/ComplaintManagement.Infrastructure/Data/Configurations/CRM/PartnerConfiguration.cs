using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");

        builder.HasKey(p => p.Id);

        // Identity
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.LegalName)
            .HasMaxLength(300);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Tier)
            .IsRequired()
            .HasConversion<int>();

        // Contact Information
        builder.Property(p => p.PrimaryEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.PrimaryPhone)
            .HasMaxLength(50);

        builder.Property(p => p.Website)
            .HasMaxLength(255);

        // Address
        builder.Property(p => p.AddressLine1)
            .HasMaxLength(255);

        builder.Property(p => p.AddressLine2)
            .HasMaxLength(255);

        builder.Property(p => p.City)
            .HasMaxLength(100);

        builder.Property(p => p.State)
            .HasMaxLength(100);

        builder.Property(p => p.Country)
            .HasMaxLength(100);

        builder.Property(p => p.PostalCode)
            .HasMaxLength(20);

        // Business Information
        builder.Property(p => p.TaxId)
            .HasMaxLength(50);

        builder.Property(p => p.PanNumber)
            .HasMaxLength(20);

        builder.Property(p => p.IndustrySegment)
            .HasMaxLength(100);

        builder.Property(p => p.PaymentTerms)
            .HasMaxLength(50);

        builder.Property(p => p.CreditLimit)
            .HasPrecision(18, 2);

        builder.Property(p => p.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(p => p.CommissionPercent)
            .HasPrecision(5, 2);

        // Portal Settings
        builder.Property(p => p.PortalBranding)
            .HasColumnType("nvarchar(max)");

        // External
        builder.Property(p => p.ExternalPartnerId)
            .HasMaxLength(100);

        // Status
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.StatusReason)
            .HasMaxLength(500);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.Tags)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(p => new { p.CompanyId, p.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(p => p.PrimaryEmail);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ExternalPartnerId);

        // Relationships
        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AuthenticationProvider)
            .WithMany()
            .HasForeignKey(p => p.AuthenticationProviderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.AccountManager)
            .WithMany()
            .HasForeignKey(p => p.AccountManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.SecondaryAccountManager)
            .WithMany()
            .HasForeignKey(p => p.SecondaryAccountManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.PartnerCustomers)
            .WithOne(pc => pc.Partner)
            .HasForeignKey(pc => pc.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Contacts)
            .WithOne(c => c.Partner)
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
