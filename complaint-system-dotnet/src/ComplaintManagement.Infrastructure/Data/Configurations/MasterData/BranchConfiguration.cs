using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.ContactEmail)
            .HasMaxLength(255);

        builder.Property(b => b.ContactPhone)
            .HasMaxLength(50);

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.City)
            .HasMaxLength(100);

        builder.Property(b => b.Country)
            .HasMaxLength(100);

        builder.Property(b => b.OryggiBranchId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(b => new { b.CompanyId, b.Code })
            .IsUnique();

        builder.HasIndex(b => b.OryggiBranchId);

        // Relationships
        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Departments)
            .WithOne(d => d.Branch)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Users)
            .WithOne(u => u.Branch)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Manager relationships - NoAction to avoid multiple cascade paths
        builder.HasOne(b => b.Manager)
            .WithMany()
            .HasForeignKey(b => b.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.SecondaryManager)
            .WithMany()
            .HasForeignKey(b => b.SecondaryManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.HrResponsible)
            .WithMany()
            .HasForeignKey(b => b.HrResponsibleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
