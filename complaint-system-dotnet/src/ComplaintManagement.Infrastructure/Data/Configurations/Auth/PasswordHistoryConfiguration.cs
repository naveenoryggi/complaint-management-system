using ComplaintManagement.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity Framework configuration for PasswordHistory entity
/// </summary>
public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        // Table name
        builder.ToTable("PasswordHistory");

        // Primary key
        builder.HasKey(ph => ph.Id);

        // Properties
        builder.Property(ph => ph.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(ph => ph.UserId)
            .IsRequired();

        builder.Property(ph => ph.PasswordHash)
            .IsRequired()
            .HasMaxLength(500); // bcrypt hashes are typically ~60 characters

        builder.Property(ph => ph.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ph => ph.CreatedBy)
            .IsRequired(false);

        builder.Property(ph => ph.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        // Indexes
        builder.HasIndex(ph => ph.UserId)
            .HasDatabaseName("IX_PasswordHistory_UserId");

        builder.HasIndex(ph => ph.CreatedAt)
            .HasDatabaseName("IX_PasswordHistory_CreatedAt");

        // Composite index for efficient history queries
        builder.HasIndex(ph => new { ph.UserId, ph.CreatedAt })
            .HasDatabaseName("IX_PasswordHistory_UserId_CreatedAt");

        // Relationships
        builder.HasOne(ph => ph.User)
            .WithMany(u => u.PasswordHistories)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ph => ph.Creator)
            .WithMany()
            .HasForeignKey(ph => ph.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction); // Avoid cascade conflicts
    }
}
