using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Communication;

/// <summary>
/// Entity Framework configuration for EmailResponseHistory
/// </summary>
public class EmailResponseHistoryConfiguration : IEntityTypeConfiguration<EmailResponseHistory>
{
    public void Configure(EntityTypeBuilder<EmailResponseHistory> builder)
    {
        builder.ToTable("EmailResponseHistories");

        builder.HasKey(e => e.Id);

        // Property configurations
        builder.Property(e => e.ComplaintId)
            .IsRequired();

        builder.Property(e => e.SentBy)
            .IsRequired();

        builder.Property(e => e.SentTo)
            .IsRequired()
            .HasMaxLength(1000); // Support multiple recipients

        builder.Property(e => e.CarbonCopy)
            .HasMaxLength(1000);

        builder.Property(e => e.BlindCarbonCopy)
            .HasMaxLength(1000);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IsHtml)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.SentAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(e => e.DeliveryStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Sent");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.MessageId)
            .HasMaxLength(500); // SMTP Message-ID can be long

        builder.Property(e => e.AttachmentIds)
            .HasMaxLength(2000); // Store comma-separated GUIDs

        // Indexes for performance
        builder.HasIndex(e => e.ComplaintId)
            .HasDatabaseName("IX_EmailResponseHistories_ComplaintId");

        builder.HasIndex(e => e.EmailMessageId)
            .HasDatabaseName("IX_EmailResponseHistories_EmailMessageId");

        builder.HasIndex(e => e.SentBy)
            .HasDatabaseName("IX_EmailResponseHistories_SentBy");

        builder.HasIndex(e => e.SentAt)
            .HasDatabaseName("IX_EmailResponseHistories_SentAt");

        builder.HasIndex(e => new { e.ComplaintId, e.SentAt })
            .HasDatabaseName("IX_EmailResponseHistories_ComplaintId_SentAt");

        builder.HasIndex(e => e.DeliveryStatus)
            .HasDatabaseName("IX_EmailResponseHistories_DeliveryStatus");

        // Relationships
        builder.HasOne(e => e.Complaint)
            .WithMany()
            .HasForeignKey(e => e.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade); // Delete history when complaint is deleted

        builder.HasOne(e => e.EmailMessage)
            .WithMany()
            .HasForeignKey(e => e.EmailMessageId)
            .OnDelete(DeleteBehavior.SetNull); // Keep history even if original message is deleted

        builder.HasOne(e => e.SentByUser)
            .WithMany()
            .HasForeignKey(e => e.SentBy)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete history if user is deleted
    }
}
