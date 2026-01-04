using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasMaxLength(255);

        builder.Property(s => s.ApprovalTimeoutHours)
            .HasDefaultValue(48);

        builder.Property(s => s.AutoEscalateToSecondary)
            .HasDefaultValue(true);

        builder.Property(s => s.RequireDeptApprovalForAssignment)
            .HasDefaultValue(false);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        // Unique constraint on Code per Company
        builder.HasIndex(s => new { s.CompanyId, s.Code })
            .IsUnique()
            .HasDatabaseName("IX_Stores_CompanyId_Code");

        // Foreign key relationships
        // Using NoAction to avoid SQL Server multiple cascade path issues
        builder.HasOne(s => s.PrimaryManager)
            .WithMany()
            .HasForeignKey(s => s.PrimaryManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.SecondaryManager)
            .WithMany()
            .HasForeignKey(s => s.SecondaryManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.NoAction);

        // Soft delete filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
