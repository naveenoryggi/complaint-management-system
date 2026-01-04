using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComplaintManagement.Infrastructure.Data.Configurations.MasterData;

public class UnitOfMeasureMasterConfiguration : IEntityTypeConfiguration<UnitOfMeasureMaster>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureMaster> builder)
    {
        builder.ToTable("UnitOfMeasureMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Symbol)
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Category)
            .HasMaxLength(50);

        // Unique constraint on Code per Company
        builder.HasIndex(x => new { x.Code, x.CompanyId })
            .IsUnique()
            .HasFilter("[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

        // Unique constraint for system units (CompanyId IS NULL)
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[CompanyId] IS NULL AND [IsDeleted] = 0");

        // Relationship with Company
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default system units
        builder.HasData(
            // Count units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Name = "Each",
                Code = "EACH",
                Symbol = "ea",
                Description = "Individual unit",
                DisplayOrder = 1,
                Category = "Count",
                IsActive = true,
                IsSystem = true,
                IsDefault = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Name = "Box",
                Code = "BOX",
                Symbol = "box",
                Description = "Box containing multiple items",
                DisplayOrder = 2,
                Category = "Count",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                Name = "Pack",
                Code = "PACK",
                Symbol = "pk",
                Description = "Package containing items",
                DisplayOrder = 3,
                Category = "Count",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                Name = "Set",
                Code = "SET",
                Symbol = "set",
                Description = "Set of items",
                DisplayOrder = 4,
                Category = "Count",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Weight units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
                Name = "Kilogram",
                Code = "KG",
                Symbol = "kg",
                Description = "Kilogram (1000 grams)",
                DisplayOrder = 10,
                Category = "Weight",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000006"),
                Name = "Gram",
                Code = "GRAM",
                Symbol = "g",
                Description = "Gram",
                DisplayOrder = 11,
                Category = "Weight",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Volume units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000007"),
                Name = "Liter",
                Code = "LITER",
                Symbol = "L",
                Description = "Liter",
                DisplayOrder = 20,
                Category = "Volume",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Length units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000008"),
                Name = "Meter",
                Code = "METER",
                Symbol = "m",
                Description = "Meter",
                DisplayOrder = 30,
                Category = "Length",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Time units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000009"),
                Name = "Hour",
                Code = "HOUR",
                Symbol = "hr",
                Description = "Hour of service",
                DisplayOrder = 40,
                Category = "Time",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000010"),
                Name = "Day",
                Code = "DAY",
                Symbol = "day",
                Description = "Day of service",
                DisplayOrder = 41,
                Category = "Time",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // Licensing units
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000011"),
                Name = "License",
                Code = "LICENSE",
                Symbol = "lic",
                Description = "Software license",
                DisplayOrder = 50,
                Category = "Licensing",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasureMaster
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000012"),
                Name = "User",
                Code = "USER",
                Symbol = "user",
                Description = "Per user license",
                DisplayOrder = 51,
                Category = "Licensing",
                IsActive = true,
                IsSystem = true,
                IsDefault = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
