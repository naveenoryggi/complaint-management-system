using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Communication;

public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> builder)
    {
        builder.ToTable("CommunicationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.RecipientEmail)
            .HasMaxLength(255);

        builder.Property(l => l.RecipientPhone)
            .HasMaxLength(50);

        builder.Property(l => l.Subject)
            .HasMaxLength(500);

        builder.Property(l => l.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(l => l.ExternalMessageId)
            .HasMaxLength(255);

        builder.Property(l => l.EntityType)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(l => l.TemplateId);
        builder.HasIndex(l => l.RecipientUserId);
        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => new { l.Status, l.CreatedAt });
        builder.HasIndex(l => new { l.EntityId, l.EntityType });
        builder.HasIndex(l => l.SentAt);

        // Relationships
        builder.HasOne(l => l.Template)
            .WithMany()
            .HasForeignKey(l => l.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.RecipientUser)
            .WithMany()
            .HasForeignKey(l => l.RecipientUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Company)
            .WithMany()
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
