using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Escalation;

public class EscalationHistoryConfiguration : IEntityTypeConfiguration<EscalationHistory>
{
    public void Configure(EntityTypeBuilder<EscalationHistory> builder)
    {
        builder.ToTable("EscalationHistories");

        builder.HasKey(eh => eh.Id);

        builder.Property(eh => eh.Level)
            .IsRequired();

        builder.Property(eh => eh.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(eh => eh.EscalatedAt)
            .IsRequired();

        builder.Property(eh => eh.IsAutoEscalation)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(eh => eh.AssignmentStrategy)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Custom converter to handle legacy "Active" status values
        var escalationStatusConverter = new ValueConverter<EscalationStatus, string>(
            v => v.ToString(),
            v => v == "Active" ? EscalationStatus.Triggered : Enum.Parse<EscalationStatus>(v)
        );

        builder.Property(eh => eh.Status)
            .IsRequired()
            .HasConversion(escalationStatusConverter)
            .HasMaxLength(50);

        builder.Property(eh => eh.EmailSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(eh => eh.Notes)
            .HasMaxLength(4000);

        // Indexes
        builder.HasIndex(eh => eh.ComplaintId);
        builder.HasIndex(eh => eh.EscalationLevelId);
        builder.HasIndex(eh => eh.EscalationMatrixId);
        builder.HasIndex(eh => eh.ToUserId);
        builder.HasIndex(eh => eh.FromUserId);
        builder.HasIndex(eh => eh.EscalatedAt);
        builder.HasIndex(eh => eh.Status);
        builder.HasIndex(eh => new { eh.ComplaintId, eh.Level });
        builder.HasIndex(eh => new { eh.ComplaintId, eh.EscalatedAt });

        // Relationships
        builder.HasOne(eh => eh.Complaint)
            .WithMany(c => c.EscalationHistories)
            .HasForeignKey(eh => eh.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eh => eh.EscalationLevel)
            .WithMany(el => el.EscalationHistories)
            .HasForeignKey(eh => eh.EscalationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eh => eh.EscalationMatrix)
            .WithMany()
            .HasForeignKey(eh => eh.EscalationMatrixId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eh => eh.FromUser)
            .WithMany()
            .HasForeignKey(eh => eh.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eh => eh.ToUser)
            .WithMany()
            .HasForeignKey(eh => eh.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eh => eh.EscalatedByUser)
            .WithMany()
            .HasForeignKey(eh => eh.EscalatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
