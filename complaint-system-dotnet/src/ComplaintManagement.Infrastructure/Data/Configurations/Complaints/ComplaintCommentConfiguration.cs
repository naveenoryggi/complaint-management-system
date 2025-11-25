using ComplaintManagement.Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Complaints;

public class ComplaintCommentConfiguration : IEntityTypeConfiguration<ComplaintComment>
{
    public void Configure(EntityTypeBuilder<ComplaintComment> builder)
    {
        builder.ToTable("ComplaintComments");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.CommentText)
            .IsRequired()
            .HasMaxLength(4000);

        // Indexes
        builder.HasIndex(cc => cc.ComplaintId);

        builder.HasIndex(cc => cc.CommentedBy);

        builder.HasIndex(cc => cc.CommentedAt);

        builder.HasIndex(cc => cc.ParentCommentId);

        builder.HasIndex(cc => cc.IsInternal);

        // Relationships
        builder.HasOne(cc => cc.Complaint)
            .WithMany(c => c.Comments)
            .HasForeignKey(cc => cc.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Commenter)
            .WithMany(u => u.Comments)
            .HasForeignKey(cc => cc.CommentedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cc => cc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(cc => cc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
