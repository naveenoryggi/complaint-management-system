using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualReturnDate",
                table: "Assets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentDate",
                table: "Assets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentNotes",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentPurpose",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedReturnDate",
                table: "Assets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StockCategoryId",
                table: "Assets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsForSale = table.Column<bool>(type: "bit", nullable: false),
                    IsFixedAsset = table.Column<bool>(type: "bit", nullable: false),
                    IsServiceStock = table.Column<bool>(type: "bit", nullable: false),
                    IsFaulty = table.Column<bool>(type: "bit", nullable: false),
                    IsDemo = table.Column<bool>(type: "bit", nullable: false),
                    RequiresSpecialHandling = table.Column<bool>(type: "bit", nullable: false),
                    AllowsCustomerAssignment = table.Column<bool>(type: "bit", nullable: false),
                    AllowsEmployeeAssignment = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_StockCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCategories_Companies_CompanyId",
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
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3570), new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3715) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3923), new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3923) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3926), new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3926) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3928), new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3928) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3931), new DateTime(2026, 1, 2, 4, 29, 35, 472, DateTimeKind.Utc).AddTicks(3931) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2965), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2966) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2973), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2973) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2976), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2976) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2978), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2978) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2981), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2981) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2983), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2984) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2986), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2988), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2989) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2991), new DateTime(2026, 1, 2, 4, 29, 35, 474, DateTimeKind.Utc).AddTicks(2991) });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_StockCategoryId",
                table: "Assets",
                column: "StockCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCategories_CompanyId_Code",
                table: "StockCategories",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_StockCategories_StockCategoryId",
                table: "Assets",
                column: "StockCategoryId",
                principalTable: "StockCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_StockCategories_StockCategoryId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "StockCategories");

            migrationBuilder.DropIndex(
                name: "IX_Assets_StockCategoryId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ActualReturnDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssignmentDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssignmentNotes",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssignmentPurpose",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ExpectedReturnDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "StockCategoryId",
                table: "Assets");

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
        }
    }
}
