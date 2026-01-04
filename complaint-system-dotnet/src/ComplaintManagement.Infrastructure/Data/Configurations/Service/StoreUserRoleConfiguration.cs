using ComplaintManagement.Domain.Entities.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Service;

public class StoreUserRoleConfiguration : IEntityTypeConfiguration<StoreUserRole>
{
    public void Configure(EntityTypeBuilder<StoreUserRole> builder)
    {
        builder.ToTable("StoreUserRoles");

        builder.HasKey(sur => sur.Id);

        builder.Property(sur => sur.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sur => sur.IsActive)
            .HasDefaultValue(true);

        builder.Property(sur => sur.AssignedAt)
            .IsRequired();

        builder.Property(sur => sur.Notes)
            .HasMaxLength(500);

        // Unique constraint - user can have only one active role per store
        builder.HasIndex(sur => new { sur.StoreId, sur.UserId, sur.Role })
            .IsUnique()
            .HasFilter("[IsActive] = 1 AND [IsDeleted] = 0")
            .HasDatabaseName("IX_StoreUserRoles_StoreId_UserId_Role_Active");

        // Index for finding users by store
        builder.HasIndex(sur => sur.StoreId)
            .HasDatabaseName("IX_StoreUserRoles_StoreId");

        // Index for finding stores by user
        builder.HasIndex(sur => sur.UserId)
            .HasDatabaseName("IX_StoreUserRoles_UserId");

        // Foreign key relationships
        // Using NoAction to avoid SQL Server multiple cascade path issues
        builder.HasOne(sur => sur.Store)
            .WithMany(s => s.StoreUserRoles)
            .HasForeignKey(sur => sur.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sur => sur.User)
            .WithMany()
            .HasForeignKey(sur => sur.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sur => sur.AssignedBy)
            .WithMany()
            .HasForeignKey(sur => sur.AssignedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sur => sur.RevokedBy)
            .WithMany()
            .HasForeignKey(sur => sur.RevokedById)
            .OnDelete(DeleteBehavior.NoAction);

        // Soft delete filter
        builder.HasQueryFilter(sur => !sur.IsDeleted);
    }
}
