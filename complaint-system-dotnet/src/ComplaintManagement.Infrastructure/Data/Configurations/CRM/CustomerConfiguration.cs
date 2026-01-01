using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        // Identity
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(300);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<int>();

        // Contact Information
        builder.Property(c => c.PrimaryEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.PrimaryPhone)
            .HasMaxLength(50);

        builder.Property(c => c.Website)
            .HasMaxLength(255);

        // Billing Address
        builder.Property(c => c.BillingAddressLine1)
            .HasMaxLength(255);

        builder.Property(c => c.BillingAddressLine2)
            .HasMaxLength(255);

        builder.Property(c => c.BillingCity)
            .HasMaxLength(100);

        builder.Property(c => c.BillingState)
            .HasMaxLength(100);

        builder.Property(c => c.BillingCountry)
            .HasMaxLength(100);

        builder.Property(c => c.BillingPostalCode)
            .HasMaxLength(20);

        // Business Information
        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.PanNumber)
            .HasMaxLength(20);

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        builder.Property(c => c.Segment)
            .HasConversion<int?>();

        builder.Property(c => c.AnnualRevenue)
            .HasPrecision(18, 2);

        builder.Property(c => c.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        // Account Information
        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CreditTerms)
            .HasMaxLength(50);

        builder.Property(c => c.CreditLimit)
            .HasPrecision(18, 2);

        // Portal Settings
        builder.Property(c => c.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(c => c.PreferredTimeZone)
            .HasMaxLength(50);

        // External
        builder.Property(c => c.ExternalCustomerId)
            .HasMaxLength(100);

        // Additional
        builder.Property(c => c.StatusReason)
            .HasMaxLength(500);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.Tags)
            .HasMaxLength(1000);

        builder.Property(c => c.CustomFields)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(c => new { c.CompanyId, c.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => c.PrimaryEmail);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ExternalCustomerId);
        builder.HasIndex(c => c.Type);

        // Relationships
        builder.HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AccountManager)
            .WithMany()
            .HasForeignKey(c => c.AccountManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SecondaryAccountManager)
            .WithMany()
            .HasForeignKey(c => c.SecondaryAccountManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.AuthenticationProvider)
            .WithMany()
            .HasForeignKey(c => c.AuthenticationProviderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.PartnerCustomers)
            .WithOne(pc => pc.Customer)
            .HasForeignKey(pc => pc.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Locations)
            .WithOne(l => l.Customer)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Contacts)
            .WithOne(ct => ct.Customer)
            .HasForeignKey(ct => ct.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
