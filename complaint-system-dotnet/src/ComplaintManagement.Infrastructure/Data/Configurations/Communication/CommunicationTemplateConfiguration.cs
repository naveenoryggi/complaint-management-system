using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Communication;

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> builder)
    {
        builder.ToTable("CommunicationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Subject)
            .HasMaxLength(500);

        builder.Property(t => t.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.HtmlBody)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.AvailablePlaceholders)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Language)
            .HasMaxLength(10);

        builder.Property(t => t.Category)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => new { t.Channel, t.IsActive });
        builder.HasIndex(t => t.Category);

        // Relationships
        builder.HasOne(t => t.Company)
            .WithMany()
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
