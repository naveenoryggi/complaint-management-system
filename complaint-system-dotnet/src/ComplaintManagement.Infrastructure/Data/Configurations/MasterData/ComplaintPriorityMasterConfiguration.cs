using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class ComplaintPriorityMasterConfiguration : IEntityTypeConfiguration<ComplaintPriorityMaster>
{
    public void Configure(EntityTypeBuilder<ComplaintPriorityMaster> builder)
    {
        builder.ToTable("ComplaintPriorityMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(50);

        builder.Property(x => x.IconClass)
            .HasMaxLength(100);

        // Unique constraint on Code per Company (or globally for system priorities)
        builder.HasIndex(x => new { x.Code, x.CompanyId })
            .IsUnique()
            .HasFilter("[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

        // Unique constraint for system priorities (CompanyId IS NULL)
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[CompanyId] IS NULL AND [IsDeleted] = 0");

        // Relationship with Company
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default system priorities
        // Note: SLA times are now managed through the SLA Management system
        builder.HasData(
            new ComplaintPriorityMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "Low",
                Code = "LOW",
                Description = "Low priority - No immediate action required",
                DisplayOrder = 1,
                Level = 1,
                ColorCode = "#4CAF50",
                IconClass = "bi-arrow-down-circle",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintPriorityMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Normal",
                Code = "NORMAL",
                Description = "Normal priority - Standard processing time",
                DisplayOrder = 2,
                Level = 3,
                ColorCode = "#2196F3",
                IconClass = "bi-dash-circle",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintPriorityMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "High",
                Code = "HIGH",
                Description = "High priority - Requires expedited attention",
                DisplayOrder = 3,
                Level = 5,
                ColorCode = "#FF9800",
                IconClass = "bi-exclamation-circle",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintPriorityMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Name = "Critical",
                Code = "CRITICAL",
                Description = "Critical priority - Requires immediate attention",
                DisplayOrder = 4,
                Level = 8,
                ColorCode = "#F44336",
                IconClass = "bi-exclamation-triangle",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintPriorityMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                Name = "Urgent",
                Code = "URGENT",
                Description = "Urgent priority - Highest priority level",
                DisplayOrder = 5,
                Level = 10,
                ColorCode = "#9C27B0",
                IconClass = "bi-lightning",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
