using ComplaintManagement.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.Events;

public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("EventTypes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.AvailableFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IconClass)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => new { e.EntityType, e.IsActive });
        builder.HasIndex(e => e.Category);

        // Relationships
        builder.HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CommunicationRules)
            .WithOne(r => r.EventType)
            .HasForeignKey(r => r.EventTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
