using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDashboardPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusWidgets = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layout = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowTrends = table.Column<bool>(type: "bit", nullable: false),
                    ShowPercentages = table.Column<bool>(type: "bit", nullable: false),
                    AutoRefreshInterval = table.Column<int>(type: "int", nullable: false),
                    DateRangeDays = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WidgetConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_DashboardPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8004), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8130) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8347), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8347) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8351), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8351) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8354), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8354) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8358), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8358) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7044), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7046) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7054), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7054) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7057), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7057) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7060), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7060) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7063), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7064) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7067), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7068) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7071), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7072) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7074), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7074) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7077), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7077) });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardPreferences_UserId",
                table: "DashboardPreferences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardPreferences");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(776), new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(868) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1075), new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1075) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1079), new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1080) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1084), new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1084) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1088), new DateTime(2025, 10, 22, 19, 51, 37, 596, DateTimeKind.Utc).AddTicks(1088) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9474), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9476) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9483), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9484) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9487), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9488) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9491), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9492) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9495), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9496) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9498), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9498) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9500), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9501) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9504), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9504) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9508), new DateTime(2025, 10, 22, 19, 51, 37, 597, DateTimeKind.Utc).AddTicks(9509) });
        }
    }
}
