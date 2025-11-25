using ComplaintManagement.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Events;

public class EventCommunicationRuleConfiguration : IEntityTypeConfiguration<EventCommunicationRule>
{
    public void Configure(EntityTypeBuilder<EventCommunicationRule> builder)
    {
        builder.ToTable("EventCommunicationRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.RecipientType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.SpecificUserIds)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.SpecificRoleIds)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.SpecificEmails)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Conditions)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.AdditionalData)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(r => r.EventTypeId);
        builder.HasIndex(r => r.TemplateId);
        builder.HasIndex(r => r.CompanyId);
        builder.HasIndex(r => new { r.IsActive, r.Priority });
        builder.HasIndex(r => r.Channel);

        // Relationships
        builder.HasOne(r => r.EventType)
            .WithMany(e => e.CommunicationRules)
            .HasForeignKey(r => r.EventTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Template)
            .WithMany()
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Company)
            .WithMany()
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
