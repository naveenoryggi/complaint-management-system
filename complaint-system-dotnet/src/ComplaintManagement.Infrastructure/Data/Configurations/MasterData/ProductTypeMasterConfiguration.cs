using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class ProductTypeMasterConfiguration : IEntityTypeConfiguration<ProductTypeMaster>
{
    public void Configure(EntityTypeBuilder<ProductTypeMaster> builder)
    {
        builder.ToTable("ProductTypeMasters");

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

        // Unique constraint for system types (CompanyId IS NULL)
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[CompanyId] IS NULL AND [IsDeleted] = 0");

        // Relationship with Company
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default system types
        builder.HasData(
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Name = "Physical",
                Code = "PHYSICAL",
                Description = "Tangible products that require shipping and inventory tracking",
                DisplayOrder = 1,
                ColorCode = "#2196F3",
                BackgroundColor = "#E3F2FD",
                IconClass = "fa-box",
                RequiresInventory = true,
                SupportsSerialNumbers = true,
                IsActive = true,
                IsSystem = true,
                IsDefault = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                Name = "Digital",
                Code = "DIGITAL",
                Description = "Digital products like software, downloads, or licenses",
                DisplayOrder = 2,
                ColorCode = "#9C27B0",
                BackgroundColor = "#F3E5F5",
                IconClass = "fa-cloud-download-alt",
                RequiresInventory = false,
                SupportsSerialNumbers = true,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Name = "Service",
                Code = "SERVICE",
                Description = "Professional services like consulting, installation, or support",
                DisplayOrder = 3,
                ColorCode = "#00BCD4",
                BackgroundColor = "#E0F7FA",
                IconClass = "fa-hands-helping",
                RequiresInventory = false,
                SupportsSerialNumbers = false,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                Name = "Subscription",
                Code = "SUBSCRIPTION",
                Description = "Recurring subscription products or services",
                DisplayOrder = 4,
                ColorCode = "#FF9800",
                BackgroundColor = "#FFF3E0",
                IconClass = "fa-sync-alt",
                RequiresInventory = false,
                SupportsSerialNumbers = false,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
                Name = "Bundle",
                Code = "BUNDLE",
                Description = "Product bundles combining multiple items",
                DisplayOrder = 5,
                ColorCode = "#4CAF50",
                BackgroundColor = "#E8F5E9",
                IconClass = "fa-boxes",
                RequiresInventory = false,
                SupportsSerialNumbers = false,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
                Name = "Spare Part",
                Code = "SPARE_PART",
                Description = "Replacement parts and components",
                DisplayOrder = 6,
                ColorCode = "#795548",
                BackgroundColor = "#EFEBE9",
                IconClass = "fa-cogs",
                RequiresInventory = true,
                SupportsSerialNumbers = true,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductTypeMaster
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000007"),
                Name = "Maintenance",
                Code = "MAINTENANCE",
                Description = "Maintenance contracts and AMC services",
                DisplayOrder = 7,
                ColorCode = "#607D8B",
                BackgroundColor = "#ECEFF1",
                IconClass = "fa-tools",
                RequiresInventory = false,
                SupportsSerialNumbers = false,
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
