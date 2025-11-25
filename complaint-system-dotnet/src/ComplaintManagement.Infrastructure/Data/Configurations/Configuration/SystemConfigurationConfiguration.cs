using ComplaintManagement.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Configuration;

/// <summary>
/// Entity Framework configuration for SystemConfiguration
/// </summary>
public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.CompanyId)
            .IsRequired();

        // Create unique index on CompanyId to ensure one configuration per company
        builder.HasIndex(sc => sc.CompanyId)
            .IsUnique()
            .HasDatabaseName("IX_SystemConfigurations_CompanyId");

        // OAuth Token Management Settings
        builder.Property(sc => sc.OAuthTokenRefreshIntervalMinutes)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(sc => sc.OAuthTokenExpiryWarningDays)
            .IsRequired()
            .HasDefaultValue(7);

        // Email Polling Settings
        builder.Property(sc => sc.DefaultEmailPollingIntervalSeconds)
            .IsRequired()
            .HasDefaultValue(300);

        builder.Property(sc => sc.MaxEmailsFetchPerPoll)
            .IsRequired()
            .HasDefaultValue(50);

        // Auto-Response Settings
        builder.Property(sc => sc.AutoResponseEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sc => sc.AutoResponseMaxRetryAttempts)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(sc => sc.AutoResponseRetryDelaySeconds)
            .IsRequired()
            .HasDefaultValue(60);

        // Rate Limiting Settings
        builder.Property(sc => sc.EmailRateLimitingEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sc => sc.MaxEmailsPerHour)
            .IsRequired()
            .HasDefaultValue(100);

        // Notification Settings
        builder.Property(sc => sc.StatusChangeNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sc => sc.AssignmentNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sc => sc.EscalationNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Timezone Settings
        builder.Property(sc => sc.DefaultTimezone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Asia/Kolkata");

        builder.Property(sc => sc.DateFormat)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(sc => sc.TimeFormat)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("hh:mm tt");

        // BaseEntity properties
        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired(false);

        builder.Property(sc => sc.CreatedBy)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(sc => sc.UpdatedBy)
            .IsRequired(false)
            .HasMaxLength(100);

        // Soft delete
        builder.Property(sc => sc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sc => sc.DeletedAt)
            .IsRequired(false);

        builder.Property(sc => sc.DeletedBy)
            .IsRequired(false)
            .HasMaxLength(100);

        // Global query filter for soft delete
        builder.HasQueryFilter(sc => !sc.IsDeleted);
    }
}
