using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.OryggiSectionId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(s => new { s.DepartmentId, s.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(s => s.OryggiSectionId);

        builder.HasIndex(s => s.HeadId);
        builder.HasIndex(s => s.SecondaryHeadId);
        builder.HasIndex(s => s.HrResponsibleId);

        // Relationships
        builder.HasOne(s => s.Department)
            .WithMany(d => d.Sections)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Head/Manager relationships - NoAction to avoid multiple cascade paths
        builder.HasOne(s => s.Head)
            .WithMany()
            .HasForeignKey(s => s.HeadId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.SecondaryHead)
            .WithMany()
            .HasForeignKey(s => s.SecondaryHeadId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.HrResponsible)
            .WithMany()
            .HasForeignKey(s => s.HrResponsibleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(s => s.Users)
            .WithOne(u => u.Section)
            .HasForeignKey(u => u.SectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
