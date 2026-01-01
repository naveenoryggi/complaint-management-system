using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for Contract entity
/// </summary>
public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        // Identity
        builder.Property(c => c.ContractNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.ExternalContractId)
            .HasMaxLength(100);

        builder.Property(c => c.PONumber)
            .HasMaxLength(100);

        builder.Property(c => c.InvoiceNumber)
            .HasMaxLength(100);

        // Financials
        builder.Property(c => c.ContractValue)
            .HasPrecision(18, 4);

        builder.Property(c => c.AnnualValue)
            .HasPrecision(18, 4);

        builder.Property(c => c.MonthlyValue)
            .HasPrecision(18, 4);

        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(c => c.PaymentTerms)
            .HasMaxLength(100);

        builder.Property(c => c.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(c => c.AmountPaid)
            .HasPrecision(18, 4);

        builder.Property(c => c.OutstandingBalance)
            .HasPrecision(18, 4);

        // Coverage - JSON fields
        builder.Property(c => c.ExcludedItems)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.IncludedItems)
            .HasColumnType("nvarchar(max)");

        // SLA
        builder.Property(c => c.SupportHoursDefinition)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.UptimeGuarantee)
            .HasPrecision(5, 2);

        // Status
        builder.Property(c => c.TerminationReason)
            .HasMaxLength(1000);

        // Documents
        builder.Property(c => c.DocumentUrl)
            .HasMaxLength(500);

        builder.Property(c => c.AdditionalDocuments)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Terms)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.SpecialConditions)
            .HasMaxLength(2000);

        builder.Property(c => c.Notes)
            .HasMaxLength(4000);

        // Custom Fields
        builder.Property(c => c.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Tags)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(c => c.ContractNumber)
            .IsUnique();

        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.PartnerId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Type);
        builder.HasIndex(c => c.EndDate);
        builder.HasIndex(c => new { c.CompanyId, c.Status });
        builder.HasIndex(c => new { c.CustomerId, c.Status });

        // Query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Relationships
        builder.HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Customer)
            .WithMany(cu => cu.Contracts)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Partner)
            .WithMany()
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SLASettings)
            .WithMany()
            .HasForeignKey(c => c.SLASettingsId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Contract)
            .HasForeignKey(i => i.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.RenewalHistory)
            .WithOne(r => r.Contract)
            .HasForeignKey(r => r.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property
        builder.Ignore(c => c.RemainingHours);
    }
}
