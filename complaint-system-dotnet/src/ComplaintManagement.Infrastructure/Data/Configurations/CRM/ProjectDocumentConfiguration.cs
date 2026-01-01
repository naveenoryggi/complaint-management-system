using ComplaintManagement.Domain.Entities.CRM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.CRM;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("ProjectDocuments");

        builder.HasKey(d => d.Id);

        // File Information
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FileType)
            .HasMaxLength(100);

        builder.Property(d => d.FileSize)
            .IsRequired();

        // Categorization
        builder.Property(d => d.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        // Upload Information
        builder.Property(d => d.UploadedById)
            .IsRequired();

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(d => d.ProjectId);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.UploadedById);
        builder.HasIndex(d => d.UploadedAt);

        // Relationships
        builder.HasOne(d => d.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
