using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalAssetsAndAssetAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockItems_Products_ProductId1",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_ProductId1",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "StockItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Assets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AssetAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConditionAtAssignment = table.Column<int>(type: "int", nullable: false),
                    ConditionAtReturn = table.Column<int>(type: "int", nullable: true),
                    ConditionNotesAtAssignment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ConditionNotesAtReturn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignmentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgementReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAssignments_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAssignments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatusMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatusMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductStatusMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypeMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequiresInventory = table.Column<bool>(type: "bit", nullable: false),
                    SupportsSerialNumbers = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypeMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTypeMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2481), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2643) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2922), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2922) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2927), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2927) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2931), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2931) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2934), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2935) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(537), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(545) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(554), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(554) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(557), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(558) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(561), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(561) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(564), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(565) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(568), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(568) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(571), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(572) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(575), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(575) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(578), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(579) });

            migrationBuilder.InsertData(
                table: "ProductStatusMasters",
                columns: new[] { "Id", "BackgroundColor", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDefault", "IsDeleted", "IsSystem", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "#FFF8E1", "DRAFT", "#FFC107", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product is in draft mode and not visible to customers", 1, "fa-file-alt", true, true, false, true, "Draft", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "#E8F5E9", "ACTIVE", "#4CAF50", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product is active and available for sale", 2, "fa-check-circle", true, false, false, true, "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "#F5F5F5", "INACTIVE", "#9E9E9E", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product is temporarily inactive", 3, "fa-pause-circle", true, false, false, true, "Inactive", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "#FBE9E7", "DISCONTINUED", "#FF5722", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product has been discontinued and is no longer available", 4, "fa-ban", true, false, false, true, "Discontinued", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "#EFEBE9", "END_OF_LIFE", "#795548", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product has reached end of life", 5, "fa-times-circle", true, false, false, true, "End of Life", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "#ECEFF1", "ARCHIVED", "#607D8B", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product has been archived for historical records", 6, "fa-archive", true, false, false, true, "Archived", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "ProductTypeMasters",
                columns: new[] { "Id", "BackgroundColor", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDefault", "IsDeleted", "IsSystem", "Name", "RequiresInventory", "SupportsSerialNumbers", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "#E3F2FD", "PHYSICAL", "#2196F3", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Tangible products that require shipping and inventory tracking", 1, "fa-box", true, true, false, true, "Physical", true, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "#F3E5F5", "DIGITAL", "#9C27B0", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Digital products like software, downloads, or licenses", 2, "fa-cloud-download-alt", true, false, false, true, "Digital", false, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "#E0F7FA", "SERVICE", "#00BCD4", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Professional services like consulting, installation, or support", 3, "fa-hands-helping", true, false, false, true, "Service", false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "#FFF3E0", "SUBSCRIPTION", "#FF9800", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Recurring subscription products or services", 4, "fa-sync-alt", true, false, false, true, "Subscription", false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "#E8F5E9", "BUNDLE", "#4CAF50", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Product bundles combining multiple items", 5, "fa-boxes", true, false, false, true, "Bundle", false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "#EFEBE9", "SPARE_PART", "#795548", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Replacement parts and components", 6, "fa-cogs", true, false, false, true, "Spare Part", true, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("30000000-0000-0000-0000-000000000007"), "#ECEFF1", "MAINTENANCE", "#607D8B", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Maintenance contracts and AMC services", 7, "fa-tools", true, false, false, true, "Maintenance", false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "UnitOfMeasureMasters",
                columns: new[] { "Id", "Category", "Code", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "IsDefault", "IsDeleted", "IsSystem", "Name", "Symbol", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "Count", "EACH", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Individual unit", 1, true, true, false, true, "Each", "ea", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "Count", "BOX", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Box containing multiple items", 2, true, false, false, true, "Box", "box", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000003"), "Count", "PACK", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Package containing items", 3, true, false, false, true, "Pack", "pk", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "Count", "SET", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Set of items", 4, true, false, false, true, "Set", "set", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000005"), "Weight", "KG", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Kilogram (1000 grams)", 10, true, false, false, true, "Kilogram", "kg", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000006"), "Weight", "GRAM", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Gram", 11, true, false, false, true, "Gram", "g", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000007"), "Volume", "LITER", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Liter", 20, true, false, false, true, "Liter", "L", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000008"), "Length", "METER", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Meter", 30, true, false, false, true, "Meter", "m", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000009"), "Time", "HOUR", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Hour of service", 40, true, false, false, true, "Hour", "hr", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000010"), "Time", "DAY", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Day of service", 41, true, false, false, true, "Day", "day", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000011"), "Licensing", "LICENSE", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Software license", 50, true, false, false, true, "License", "lic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("40000000-0000-0000-0000-000000000012"), "Licensing", "USER", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Per user license", 51, true, false, false, true, "User", "user", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedUserId_IsInternal",
                table: "Assets",
                columns: new[] { "AssignedUserId", "IsInternal" },
                filter: "[AssignedUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CompanyId_IsInternal",
                table: "Assets",
                columns: new[] { "CompanyId", "IsInternal" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_IsInternal",
                table: "Assets",
                column: "IsInternal");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssetId",
                table: "AssetAssignments",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssetId_IsActive",
                table: "AssetAssignments",
                columns: new[] { "AssetId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssignedByUserId",
                table: "AssetAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssignedToUserId",
                table: "AssetAssignments",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssignedToUserId_IsActive",
                table: "AssetAssignments",
                columns: new[] { "AssignedToUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_CompanyId",
                table: "AssetAssignments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_CompanyId_AssignmentDate",
                table: "AssetAssignments",
                columns: new[] { "CompanyId", "AssignmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_CompanyId_AssignmentNumber",
                table: "AssetAssignments",
                columns: new[] { "CompanyId", "AssignmentNumber" },
                unique: true,
                filter: "[AssignmentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ExpectedReturnDate_IsActive",
                table: "AssetAssignments",
                columns: new[] { "ExpectedReturnDate", "IsActive" },
                filter: "[ExpectedReturnDate] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatusMasters_Code",
                table: "ProductStatusMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatusMasters_Code_CompanyId",
                table: "ProductStatusMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatusMasters_CompanyId",
                table: "ProductStatusMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeMasters_Code",
                table: "ProductTypeMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeMasters_Code_CompanyId",
                table: "ProductTypeMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypeMasters_CompanyId",
                table: "ProductTypeMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureMasters_Code",
                table: "UnitOfMeasureMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureMasters_Code_CompanyId",
                table: "UnitOfMeasureMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureMasters_CompanyId",
                table: "UnitOfMeasureMasters",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAssignments");

            migrationBuilder.DropTable(
                name: "ProductStatusMasters");

            migrationBuilder.DropTable(
                name: "ProductTypeMasters");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureMasters");

            migrationBuilder.DropIndex(
                name: "IX_Assets_AssignedUserId_IsInternal",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CompanyId_IsInternal",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_IsInternal",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                table: "Assets");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "StockItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Assets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8173), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8277) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8480), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8480) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8483), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8483) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8485), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8486) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8488), new DateTime(2026, 1, 3, 3, 23, 20, 630, DateTimeKind.Utc).AddTicks(8488) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7767), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7769) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7775), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7775) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7777), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7778) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7780), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7780) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7782), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7783) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7785), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7785) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7787), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7788) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7790), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7790) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7792), new DateTime(2026, 1, 3, 3, 23, 20, 632, DateTimeKind.Utc).AddTicks(7793) });

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ProductId1",
                table: "StockItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StockItems_Products_ProductId1",
                table: "StockItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
