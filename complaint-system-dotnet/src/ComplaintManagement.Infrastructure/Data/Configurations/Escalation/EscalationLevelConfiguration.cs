using ComplaintManagement.Domain.Entities.Escalation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Escalation;

public class EscalationLevelConfiguration : IEntityTypeConfiguration<EscalationLevel>
{
    public void Configure(EntityTypeBuilder<EscalationLevel> builder)
    {
        builder.ToTable("EscalationLevels");

        builder.HasKey(el => el.Id);

        builder.Property(el => el.Level)
            .IsRequired();

        builder.Property(el => el.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(el => el.Description)
            .HasMaxLength(1000);

        builder.Property(el => el.TriggerAfterHours)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(el => el.AssignmentStrategy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(el => el.AssignToRole)
            .HasMaxLength(50);

        builder.Property(el => el.AssignToUserIds)
            .HasMaxLength(500);

        builder.Property(el => el.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(el => el.SendNotification)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(el => el.NotifyPreviousHandler)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(el => el.EscalationMessage)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(el => el.EscalationMatrixId);
        builder.HasIndex(el => new { el.EscalationMatrixId, el.Level });
        builder.HasIndex(el => el.IsActive);

        // Relationships
        builder.HasOne(el => el.EscalationMatrix)
            .WithMany(em => em.EscalationLevels)
            .HasForeignKey(el => el.EscalationMatrixId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(el => el.AssignToUser)
            .WithMany()
            .HasForeignKey(el => el.AssignToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(el => el.EscalationHistories)
            .WithOne(eh => eh.EscalationLevel)
            .HasForeignKey(eh => eh.EscalationLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
