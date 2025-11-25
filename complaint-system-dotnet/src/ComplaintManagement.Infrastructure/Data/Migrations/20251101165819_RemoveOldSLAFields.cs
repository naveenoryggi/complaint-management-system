using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldSLAFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlaResolutionHours",
                table: "ComplaintPriorityMasters");

            migrationBuilder.DropColumn(
                name: "SlaResponseHours",
                table: "ComplaintPriorityMasters");

            migrationBuilder.DropColumn(
                name: "DefaultSlaHours",
                table: "ComplaintCategories");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4209), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4484) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4926), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4927) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4932), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4932) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4937), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4937) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4940), new DateTime(2025, 11, 1, 16, 58, 17, 249, DateTimeKind.Utc).AddTicks(4941) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(639), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(647) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(656), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(656) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(660), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(661) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(664), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(664) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(667), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(667) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(670), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(671) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(673), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(674) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(676), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(677) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(680), new DateTime(2025, 11, 1, 16, 58, 17, 254, DateTimeKind.Utc).AddTicks(680) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlaResolutionHours",
                table: "ComplaintPriorityMasters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlaResponseHours",
                table: "ComplaintPriorityMasters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSlaHours",
                table: "ComplaintCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1234), 168, 72, new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1463) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1906), 120, 48, new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1907) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1916), 72, 24, new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1916) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1924), 24, 4, new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1925) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "SlaResolutionHours", "SlaResponseHours", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1932), 8, 1, new DateTime(2025, 11, 1, 3, 15, 11, 914, DateTimeKind.Utc).AddTicks(1933) });

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
        }
    }
}
