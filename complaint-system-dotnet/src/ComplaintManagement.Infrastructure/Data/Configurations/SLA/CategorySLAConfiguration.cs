using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.SLA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.SLA;

public class CategorySLAConfiguration : IEntityTypeConfiguration<CategorySLA>
{
    public void Configure(EntityTypeBuilder<CategorySLA> builder)
    {
        builder.ToTable("CategorySLAs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CategoryId)
            .IsRequired();

        builder.Property(c => c.SLALevelId)
            .IsRequired();

        builder.Property(c => c.OverrideResponseTime);

        builder.Property(c => c.OverrideResolutionTime);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        // Indexes
        builder.HasIndex(c => c.CategoryId)
            .IsUnique();

        builder.HasIndex(c => c.SLALevelId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => new { c.CategoryId, c.SLALevelId });

        // Relationships
        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.SLALevel)
            .WithMany(l => l.CategorySLAs)
            .HasForeignKey(c => c.SLALevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
