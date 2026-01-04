using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class ProductStatusMasterConfiguration : IEntityTypeConfiguration<ProductStatusMaster>
{
    public void Configure(EntityTypeBuilder<ProductStatusMaster> builder)
    {
        builder.ToTable("ProductStatusMasters");

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

        builder.Property(x => x.BackgroundColor)
            .HasMaxLength(50);

        builder.Property(x => x.IconClass)
            .HasMaxLength(100);

        // Unique constraint on Code per Company
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
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "Draft",
                Code = "DRAFT",
                Description = "Product is in draft mode and not visible to customers",
                DisplayOrder = 1,
                ColorCode = "#FFC107",
                BackgroundColor = "#FFF8E1",
                IconClass = "fa-file-alt",
                IsActive = true,
                IsSystem = true,
                IsDefault = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Active",
                Code = "ACTIVE",
                Description = "Product is active and available for sale",
                DisplayOrder = 2,
                ColorCode = "#4CAF50",
                BackgroundColor = "#E8F5E9",
                IconClass = "fa-check-circle",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "Inactive",
                Code = "INACTIVE",
                Description = "Product is temporarily inactive",
                DisplayOrder = 3,
                ColorCode = "#9E9E9E",
                BackgroundColor = "#F5F5F5",
                IconClass = "fa-pause-circle",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                Name = "Discontinued",
                Code = "DISCONTINUED",
                Description = "Product has been discontinued and is no longer available",
                DisplayOrder = 4,
                ColorCode = "#FF5722",
                BackgroundColor = "#FBE9E7",
                IconClass = "fa-ban",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                Name = "End of Life",
                Code = "END_OF_LIFE",
                Description = "Product has reached end of life",
                DisplayOrder = 5,
                ColorCode = "#795548",
                BackgroundColor = "#EFEBE9",
                IconClass = "fa-times-circle",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductStatusMaster
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                Name = "Archived",
                Code = "ARCHIVED",
                Description = "Product has been archived for historical records",
                DisplayOrder = 6,
                ColorCode = "#607D8B",
                BackgroundColor = "#ECEFF1",
                IconClass = "fa-archive",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
