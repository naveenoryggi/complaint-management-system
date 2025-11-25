using ComplaintManagement.Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Complaints;

public class ComplaintAttachmentConfiguration : IEntityTypeConfiguration<ComplaintAttachment>
{
    public void Configure(EntityTypeBuilder<ComplaintAttachment> builder)
    {
        builder.ToTable("ComplaintAttachments");

        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ca => ca.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ca => ca.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ca => ca.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ca => ca.FileExtension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ca => ca.Description)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(ca => ca.ComplaintId);

        builder.HasIndex(ca => ca.UploadedBy);

        builder.HasIndex(ca => ca.UploadedAt);

        // Relationships
        builder.HasOne(ca => ca.Complaint)
            .WithMany(c => c.Attachments)
            .HasForeignKey(ca => ca.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.Uploader)
            .WithMany()
            .HasForeignKey(ca => ca.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
