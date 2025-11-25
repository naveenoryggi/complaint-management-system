using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSLATables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SLALevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ColorCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#4CAF50"),
                    DefaultResponseTime = table.Column<int>(type: "int", nullable: false),
                    ResponseTimeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultResolutionTime = table.Column<int>(type: "int", nullable: false),
                    ResolutionTimeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_SLALevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SLASettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WorkingHoursOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WorkingHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingDays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "1,2,3,4,5"),
                    AutoEscalateOnBreach = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EscalationThresholdPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 80),
                    NotifyBeforeBreach = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotifyBeforeBreachMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    PauseSLAOnPendingInfo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExcludeHolidays = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "UTC"),
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
                    table.PrimaryKey("PK_SLASettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategorySLAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SLALevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverrideResponseTime = table.Column<int>(type: "int", nullable: true),
                    OverrideResolutionTime = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_CategorySLAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySLAs_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySLAs_SLALevels_SLALevelId",
                        column: x => x.SLALevelId,
                        principalTable: "SLALevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrioritySLAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SLALevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverrideResponseTime = table.Column<int>(type: "int", nullable: true),
                    OverrideResolutionTime = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_PrioritySLAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrioritySLAs_ComplaintPriorityMasters_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "ComplaintPriorityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrioritySLAs_SLALevels_SLALevelId",
                        column: x => x.SLALevelId,
                        principalTable: "SLALevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1234), new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1463) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1906), new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1907) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1916), new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1916) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1924), new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1925) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1932), new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1933) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9608), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9613) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9623), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9624) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9628), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9629) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9634), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9635) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9639), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9640) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9645), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9646) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9650), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9651) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9656), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9656) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9661), new DateTime(2025, 11, 1, 3, 15, 11, 918, DateTimeKind.Utc).AddTicks(9662) });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_CategoryId",
                table: "CategorySLAs",
                column: "CategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_CategoryId_SLALevelId",
                table: "CategorySLAs",
                columns: new[] { "CategoryId", "SLALevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_IsActive",
                table: "CategorySLAs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_SLALevelId",
                table: "CategorySLAs",
                column: "SLALevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_IsActive",
                table: "PrioritySLAs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_PriorityId",
                table: "PrioritySLAs",
                column: "PriorityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_PriorityId_SLALevelId",
                table: "PrioritySLAs",
                columns: new[] { "PriorityId", "SLALevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_SLALevelId",
                table: "PrioritySLAs",
                column: "SLALevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId",
                table: "SLALevels",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId_Name",
                table: "SLALevels",
                columns: new[] { "CompanyId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId_Order",
                table: "SLALevels",
                columns: new[] { "CompanyId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_IsActive",
                table: "SLALevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_Order",
                table: "SLALevels",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_SLASettings_CompanyId",
                table: "SLASettings",
                column: "CompanyId",
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SLASettings_IsEnabled",
                table: "SLASettings",
                column: "IsEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorySLAs");

            migrationBuilder.DropTable(
                name: "PrioritySLAs");

            migrationBuilder.DropTable(
                name: "SLASettings");

            migrationBuilder.DropTable(
                name: "SLALevels");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(7978), new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8070) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8268), new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8269) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8271), new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8272) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8274), new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8275) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8277), new DateTime(2025, 10, 31, 10, 8, 12, 496, DateTimeKind.Utc).AddTicks(8278) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2347), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2348) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2355), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2356) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2358), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2358) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2360), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2361) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2363), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2363) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2366), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2366) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2368), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2369) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2371), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2371) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2373), new DateTime(2025, 10, 31, 10, 8, 12, 499, DateTimeKind.Utc).AddTicks(2373) });
        }
    }
}
