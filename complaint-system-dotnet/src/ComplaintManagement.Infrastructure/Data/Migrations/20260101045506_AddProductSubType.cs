using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSubType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubTypeId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductSubTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ProductSubTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSubTypes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSubTypes_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 422, DateTimeKind.Utc).AddTicks(9657), new DateTime(2026, 1, 1, 4, 55, 4, 422, DateTimeKind.Utc).AddTicks(9796) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(10), new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(10) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(12), new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(13) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(15), new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(15) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(17), new DateTime(2026, 1, 1, 4, 55, 4, 423, DateTimeKind.Utc).AddTicks(18) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(388), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(391) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(400), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(401) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(403), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(403) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(406), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(406) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(408), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(409) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(411), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(411) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(416), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(416) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(418), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(419) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(421), new DateTime(2026, 1, 1, 4, 55, 4, 425, DateTimeKind.Utc).AddTicks(421) });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubTypeId",
                table: "Products",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubTypes_CategoryId",
                table: "ProductSubTypes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubTypes_CompanyId_CategoryId_Code",
                table: "ProductSubTypes",
                columns: new[] { "CompanyId", "CategoryId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubTypes_IsActive",
                table: "ProductSubTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubTypes_Name",
                table: "ProductSubTypes",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductSubTypes_SubTypeId",
                table: "Products",
                column: "SubTypeId",
                principalTable: "ProductSubTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductSubTypes_SubTypeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductSubTypes");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubTypeId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubTypeId",
                table: "Products");

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
        }
    }
}
