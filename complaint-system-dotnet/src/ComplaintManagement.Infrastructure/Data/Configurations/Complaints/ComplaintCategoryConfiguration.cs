using ComplaintManagement.Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Complaints;

public class ComplaintCategoryConfiguration : IEntityTypeConfiguration<ComplaintCategory>
{
    public void Configure(EntityTypeBuilder<ComplaintCategory> builder)
    {
        builder.ToTable("ComplaintCategories");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cc => cc.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cc => cc.Description)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(cc => cc.Code)
            .IsUnique();

        builder.HasIndex(cc => cc.ParentCategoryId);

        builder.HasIndex(cc => cc.DisplayOrder);

        // Relationships
        builder.HasOne(cc => cc.ParentCategory)
            .WithMany(pc => pc.SubCategories)
            .HasForeignKey(cc => cc.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cc => cc.Complaints)
            .WithOne(c => c.Category)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
