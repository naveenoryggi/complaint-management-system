using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

/// <summary>
/// EF Core configuration for ContractRenewalHistory entity
/// </summary>
public class ContractRenewalHistoryConfiguration : IEntityTypeConfiguration<ContractRenewalHistory>
{
    public void Configure(EntityTypeBuilder<ContractRenewalHistory> builder)
    {
        builder.ToTable("ContractRenewalHistory");

        builder.HasKey(rh => rh.Id);

        // Financials
        builder.Property(rh => rh.PreviousValue)
            .HasPrecision(18, 4);

        builder.Property(rh => rh.NewValue)
            .HasPrecision(18, 4);

        builder.Property(rh => rh.PriceChangePercent)
            .HasPrecision(5, 2);

        builder.Property(rh => rh.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(rh => rh.RenewalDiscount)
            .HasPrecision(5, 2);

        builder.Property(rh => rh.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(rh => rh.PONumber)
            .HasMaxLength(100);

        // Changes
        builder.Property(rh => rh.ChangesSummary)
            .HasColumnType("nvarchar(max)");

        // Communication
        builder.Property(rh => rh.CustomerResponse)
            .HasMaxLength(2000);

        // Notes
        builder.Property(rh => rh.Reason)
            .HasMaxLength(1000);

        builder.Property(rh => rh.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(rh => rh.ContractId);
        builder.HasIndex(rh => rh.RenewalDate);
        builder.HasIndex(rh => rh.Action);
        builder.HasIndex(rh => new { rh.ContractId, rh.RenewalNumber });

        // Query filter for soft delete
        builder.HasQueryFilter(rh => !rh.IsDeleted);

        // Relationships
        builder.HasOne(rh => rh.Contract)
            .WithMany(c => c.RenewalHistory)
            .HasForeignKey(rh => rh.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
