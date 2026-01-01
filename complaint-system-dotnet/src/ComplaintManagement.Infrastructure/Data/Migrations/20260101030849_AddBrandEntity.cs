using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_Companies_CompanyId",
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
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8199), new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8316) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8520), new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8520) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8523), new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8523) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8525), new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8526) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8528), new DateTime(2026, 1, 1, 3, 8, 42, 543, DateTimeKind.Utc).AddTicks(8528) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5293), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5297) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5303), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5303) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5305), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5306) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5308), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5308) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5311), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5311) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5313), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5314) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5316), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5316) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5318), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5319) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5321), new DateTime(2026, 1, 1, 3, 8, 42, 546, DateTimeKind.Utc).AddTicks(5321) });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_CompanyId_Code",
                table: "Brands",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_IsActive",
                table: "Brands",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9219), new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9332) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9531), new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9532) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9536), new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9536) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9539), new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9540) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9542), new DateTime(2025, 12, 31, 16, 28, 21, 592, DateTimeKind.Utc).AddTicks(9542) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9653), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9655) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9660), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9661) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9663), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9663) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9666), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9666) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9668), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9669) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9671), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9671) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9674), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9674) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9676), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9677) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9679), new DateTime(2025, 12, 31, 16, 28, 21, 594, DateTimeKind.Utc).AddTicks(9679) });
        }
    }
}
