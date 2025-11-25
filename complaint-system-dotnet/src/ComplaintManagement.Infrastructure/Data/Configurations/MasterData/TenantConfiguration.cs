using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.ContactEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.ContactPhone)
            .HasMaxLength(50);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        builder.Property(t => t.OryggiTenantId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.HasIndex(t => t.OryggiTenantId);

        // Relationships
        builder.HasMany(t => t.Companies)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
