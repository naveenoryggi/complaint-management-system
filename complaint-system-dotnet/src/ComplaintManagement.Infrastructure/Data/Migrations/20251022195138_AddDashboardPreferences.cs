using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3210), new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3331) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3545), new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3545) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3550), new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3550) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3554), new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3557), new DateTime(2025, 10, 22, 14, 9, 14, 188, DateTimeKind.Utc).AddTicks(3557) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4540), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4544) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4552), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4552) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4554), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4555) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4594), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4594) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4598), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4599) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4601), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4601) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4603), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4604) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4606), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4606) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4609), new DateTime(2025, 10, 22, 14, 9, 14, 190, DateTimeKind.Utc).AddTicks(4609) });
        }
    }
}
