using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class PartnerContactConfiguration : IEntityTypeConfiguration<PartnerContact>
{
    public void Configure(EntityTypeBuilder<PartnerContact> builder)
    {
        builder.ToTable("PartnerContacts");

        builder.HasKey(c => c.Id);

        // Personal Information
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Mobile)
            .HasMaxLength(50);

        builder.Property(c => c.JobTitle)
            .HasMaxLength(100);

        builder.Property(c => c.Department)
            .HasMaxLength(100);

        builder.Property(c => c.Role)
            .IsRequired()
            .HasConversion<int>();

        // Portal Authentication
        builder.Property(c => c.PasswordHash)
            .HasMaxLength(500);

        builder.Property(c => c.PortalRole)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.AuthType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.ExternalUserId)
            .HasMaxLength(255);

        // Preferences
        builder.Property(c => c.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(c => c.PreferredTimeZone)
            .HasMaxLength(50);

        // Notes
        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.PartnerId);
        builder.HasIndex(c => c.IsPortalUser);
        builder.HasIndex(c => c.IsPrimaryContact);

        // Unique constraint on email within partner
        builder.HasIndex(c => new { c.PartnerId, c.Email })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasOne(c => c.Partner)
            .WithMany(p => p.Contacts)
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property
        builder.Ignore(c => c.FullName);

        // Global query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
