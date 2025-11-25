using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.ContactEmail)
            .HasMaxLength(255);

        builder.Property(c => c.ContactPhone)
            .HasMaxLength(50);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.OryggiCompanyId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique();

        builder.HasIndex(c => c.OryggiCompanyId);

        // Relationships
        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Companies)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Branches)
            .WithOne(b => b.Company)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Manager relationships - NoAction to avoid multiple cascade paths
        builder.HasOne(c => c.Manager)
            .WithMany()
            .HasForeignKey(c => c.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.SecondaryManager)
            .WithMany()
            .HasForeignKey(c => c.SecondaryManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.HrResponsible)
            .WithMany()
            .HasForeignKey(c => c.HrResponsibleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
