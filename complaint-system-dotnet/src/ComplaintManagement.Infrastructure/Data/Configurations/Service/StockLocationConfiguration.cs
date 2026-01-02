using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        builder.ToTable("StockLocations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.Country)
            .HasMaxLength(100);

        builder.Property(s => s.PostalCode)
            .HasMaxLength(20);

        builder.Property(s => s.ContactPerson)
            .HasMaxLength(200);

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(50);

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(200);

        builder.Property(s => s.CapacityUnit)
            .HasMaxLength(50);

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Unique constraint on Code per company
        builder.HasIndex(s => new { s.CompanyId, s.Code })
            .IsUnique();

        // Self-referencing relationship for hierarchy
        builder.HasOne(s => s.ParentLocation)
            .WithMany(s => s.ChildLocations)
            .HasForeignKey(s => s.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Company
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
