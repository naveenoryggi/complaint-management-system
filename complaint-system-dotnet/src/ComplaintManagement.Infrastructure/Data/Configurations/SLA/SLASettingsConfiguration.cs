using ComplaintManagement.Domain.Entities.SLA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.SLA;

public class SLASettingsConfiguration : IEntityTypeConfiguration<SLASettings>
{
    public void Configure(EntityTypeBuilder<SLASettings> builder)
    {
        builder.ToTable("SLASettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.WorkingHoursOnly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.WorkingHoursStart)
            .HasColumnType("time");

        builder.Property(s => s.WorkingHoursEnd)
            .HasColumnType("time");

        builder.Property(s => s.WorkingDays)
            .HasMaxLength(50)
            .HasDefaultValue("1,2,3,4,5");

        builder.Property(s => s.ExcludeHolidays)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.AutoEscalateOnBreach)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.EscalationThresholdPercent)
            .IsRequired()
            .HasDefaultValue(80);

        builder.Property(s => s.NotifyBeforeBreach)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.NotifyBeforeBreachMinutes)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(s => s.PauseSLAOnPendingInfo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.Timezone)
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(s => s.CompanyId);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Indexes
        builder.HasIndex(s => s.CompanyId)
            .IsUnique();

        builder.HasIndex(s => s.IsEnabled);
    }
}
