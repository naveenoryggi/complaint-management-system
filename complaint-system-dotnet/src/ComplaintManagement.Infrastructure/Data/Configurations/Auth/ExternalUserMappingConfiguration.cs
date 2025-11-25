using ComplaintManagement.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity Framework configuration for ExternalUserMapping entity
/// </summary>
public class ExternalUserMappingConfiguration : IEntityTypeConfiguration<ExternalUserMapping>
{
    public void Configure(EntityTypeBuilder<ExternalUserMapping> builder)
    {
        // Table name
        builder.ToTable("ExternalUserMappings");

        // Primary key
        builder.HasKey(eum => eum.Id);

        // Properties
        builder.Property(eum => eum.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(eum => eum.UserId)
            .IsRequired();

        builder.Property(eum => eum.AuthenticationProviderId)
            .IsRequired();

        builder.Property(eum => eum.ExternalUserId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(eum => eum.ExternalUsername)
            .HasMaxLength(200);

        builder.Property(eum => eum.ExternalEmail)
            .HasMaxLength(200);

        builder.Property(eum => eum.ExternalDisplayName)
            .HasMaxLength(200);

        builder.Property(eum => eum.Attributes)
            .HasMaxLength(5000); // JSON

        builder.Property(eum => eum.ExternalGroups)
            .HasMaxLength(5000); // JSON array

        builder.Property(eum => eum.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(eum => eum.LastSyncedAt);

        builder.Property(eum => eum.LastSyncSuccess);

        builder.Property(eum => eum.LastSyncDetails)
            .HasMaxLength(1000);

        builder.Property(eum => eum.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(eum => eum.LastLoginAt);

        // Indexes
        builder.HasIndex(eum => eum.UserId)
            .HasDatabaseName("IX_ExternalUserMappings_UserId");

        builder.HasIndex(eum => eum.AuthenticationProviderId)
            .HasDatabaseName("IX_ExternalUserMappings_AuthenticationProviderId");

        builder.HasIndex(eum => eum.ExternalUserId)
            .HasDatabaseName("IX_ExternalUserMappings_ExternalUserId");

        builder.HasIndex(eum => eum.IsActive)
            .HasDatabaseName("IX_ExternalUserMappings_IsActive");

        // Unique constraint: One external user ID per provider
        builder.HasIndex(eum => new { eum.AuthenticationProviderId, eum.ExternalUserId })
            .IsUnique()
            .HasDatabaseName("IX_ExternalUserMappings_ProviderId_ExternalUserId");

        // Composite index for efficient lookups
        builder.HasIndex(eum => new { eum.UserId, eum.AuthenticationProviderId })
            .HasDatabaseName("IX_ExternalUserMappings_UserId_ProviderId");

        // Relationships
        builder.HasOne(eum => eum.User)
            .WithMany(u => u.ExternalUserMappings)
            .HasForeignKey(eum => eum.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eum => eum.AuthenticationProvider)
            .WithMany(ap => ap.ExternalUserMappings)
            .HasForeignKey(eum => eum.AuthenticationProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
