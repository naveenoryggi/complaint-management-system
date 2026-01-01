using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class CustomerLocationConfiguration : IEntityTypeConfiguration<CustomerLocation>
{
    public void Configure(EntityTypeBuilder<CustomerLocation> builder)
    {
        builder.ToTable("CustomerLocations");

        builder.HasKey(l => l.Id);

        // Identity
        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        // Address
        builder.Property(l => l.AddressLine1)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.AddressLine2)
            .HasMaxLength(255);

        builder.Property(l => l.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.State)
            .HasMaxLength(100);

        builder.Property(l => l.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.PostalCode)
            .HasMaxLength(20);

        // Geolocation
        builder.Property(l => l.TimeZone)
            .HasMaxLength(50);

        // Contact
        builder.Property(l => l.ContactEmail)
            .HasMaxLength(255);

        builder.Property(l => l.ContactPhone)
            .HasMaxLength(50);

        builder.Property(l => l.Fax)
            .HasMaxLength(50);

        // Operational
        builder.Property(l => l.OperatingHours)
            .HasMaxLength(1000);

        builder.Property(l => l.AccessInstructions)
            .HasMaxLength(1000);

        builder.Property(l => l.EmergencyContactName)
            .HasMaxLength(100);

        builder.Property(l => l.EmergencyContactPhone)
            .HasMaxLength(50);

        // External
        builder.Property(l => l.ExternalLocationId)
            .HasMaxLength(100);

        builder.Property(l => l.Notes)
            .HasMaxLength(2000);

        builder.Property(l => l.Tags)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(l => new { l.CustomerId, l.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => l.IsPrimary);
        builder.HasIndex(l => l.City);

        // Relationships
        builder.HasOne(l => l.Customer)
            .WithMany(c => c.Locations)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Contacts)
            .WithOne(c => c.Location)
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filter for soft delete
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
