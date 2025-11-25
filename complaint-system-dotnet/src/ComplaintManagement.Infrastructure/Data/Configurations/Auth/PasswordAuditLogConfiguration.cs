using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity Framework configuration for PasswordAuditLog entity
/// </summary>
public class PasswordAuditLogConfiguration : IEntityTypeConfiguration<PasswordAuditLog>
{
    public void Configure(EntityTypeBuilder<PasswordAuditLog> builder)
    {
        // Table name
        builder.ToTable("PasswordAuditLog");

        // Primary key
        builder.HasKey(pal => pal.Id);

        // Properties
        builder.Property(pal => pal.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(pal => pal.UserId)
            .IsRequired();

        builder.Property(pal => pal.Action)
            .IsRequired()
            .HasConversion<string>() // Store as string in database
            .HasMaxLength(50);

        builder.Property(pal => pal.PerformedBy)
            .IsRequired(false);

        builder.Property(pal => pal.Success)
            .IsRequired();

        builder.Property(pal => pal.Details)
            .HasMaxLength(1000);

        builder.Property(pal => pal.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(pal => pal.UserAgent)
            .HasMaxLength(500);

        builder.Property(pal => pal.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes for efficient querying
        builder.HasIndex(pal => pal.UserId)
            .HasDatabaseName("IX_PasswordAuditLog_UserId");

        builder.HasIndex(pal => pal.CreatedAt)
            .HasDatabaseName("IX_PasswordAuditLog_CreatedAt");

        builder.HasIndex(pal => pal.Action)
            .HasDatabaseName("IX_PasswordAuditLog_Action");

        builder.HasIndex(pal => pal.Success)
            .HasDatabaseName("IX_PasswordAuditLog_Success");

        // Composite index for common queries
        builder.HasIndex(pal => new { pal.UserId, pal.CreatedAt })
            .HasDatabaseName("IX_PasswordAuditLog_UserId_CreatedAt");

        // Composite index for security monitoring
        builder.HasIndex(pal => new { pal.Success, pal.CreatedAt })
            .HasDatabaseName("IX_PasswordAuditLog_Success_CreatedAt");

        // Relationships
        builder.HasOne(pal => pal.User)
            .WithMany(u => u.PasswordAuditLogs)
            .HasForeignKey(pal => pal.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pal => pal.Performer)
            .WithMany()
            .HasForeignKey(pal => pal.PerformedBy)
            .OnDelete(DeleteBehavior.NoAction); // Avoid cascade conflicts
    }
}
