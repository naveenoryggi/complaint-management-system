using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class ComplaintStatusMasterConfiguration : IEntityTypeConfiguration<ComplaintStatusMaster>
{
    public void Configure(EntityTypeBuilder<ComplaintStatusMaster> builder)
    {
        builder.ToTable("ComplaintStatusMasters");

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

        // Unique constraint on Code per Company (or globally for system statuses)
        builder.HasIndex(x => new { x.Code, x.CompanyId })
            .IsUnique()
            .HasFilter("[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

        // Unique constraint for system statuses (CompanyId IS NULL)
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[CompanyId] IS NULL AND [IsDeleted] = 0");

        // Relationship with Company
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default system statuses
        builder.HasData(
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Submitted",
                Code = "SUBMITTED",
                Description = "Complaint has been submitted but not yet reviewed",
                DisplayOrder = 1,
                ColorCode = "#9E9E9E",
                IconClass = "bi-inbox",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "Under Review",
                Code = "UNDER_REVIEW",
                Description = "Complaint is being reviewed by the assigned handler",
                DisplayOrder = 2,
                ColorCode = "#2196F3",
                IconClass = "bi-eye",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "In Progress",
                Code = "IN_PROGRESS",
                Description = "Complaint is currently being investigated",
                DisplayOrder = 3,
                ColorCode = "#FF9800",
                IconClass = "bi-gear",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "Escalated",
                Code = "ESCALATED",
                Description = "Complaint has been escalated to a higher level",
                DisplayOrder = 4,
                ColorCode = "#FF5722",
                IconClass = "bi-arrow-up-circle",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Name = "Pending Info",
                Code = "PENDING_INFO",
                Description = "Complaint is awaiting information from the complainant",
                DisplayOrder = 5,
                ColorCode = "#FFC107",
                IconClass = "bi-question-circle",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Name = "Resolved",
                Code = "RESOLVED",
                Description = "Complaint has been resolved",
                DisplayOrder = 6,
                ColorCode = "#4CAF50",
                IconClass = "bi-check-circle",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                Name = "Closed",
                Code = "CLOSED",
                Description = "Complaint has been closed (final state)",
                DisplayOrder = 7,
                ColorCode = "#607D8B",
                IconClass = "bi-lock",
                IsActive = true,
                IsSystem = true,
                IsFinal = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                Name = "Rejected",
                Code = "REJECTED",
                Description = "Complaint has been rejected/dismissed",
                DisplayOrder = 8,
                ColorCode = "#F44336",
                IconClass = "bi-x-circle",
                IsActive = true,
                IsSystem = true,
                IsFinal = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ComplaintStatusMaster
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000009"),
                Name = "Reopened",
                Code = "REOPENED",
                Description = "Complaint has been reopened after closure",
                DisplayOrder = 9,
                ColorCode = "#E91E63",
                IconClass = "bi-arrow-repeat",
                IsActive = true,
                IsSystem = true,
                IsFinal = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
