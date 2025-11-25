using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3635), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3749) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3991), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3992) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3997), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3998) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4002), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4003) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4007), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4008) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5936), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5951), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5952) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5955), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5955) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5958), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5961), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5962) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5964), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5965) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5968), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5968) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5971), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5972) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5975), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5975) });
        }
    }
}
