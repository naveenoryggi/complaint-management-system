using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthRefreshIntervalToEmailConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OAuthTokenRefreshIntervalMinutes",
                table: "EmailConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(6686), new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(6931) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7364), new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7366) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7373), new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7374) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7379), new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7380) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7386), new DateTime(2025, 11, 16, 6, 45, 42, 992, DateTimeKind.Utc).AddTicks(7387) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(951), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(958) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1007), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1007) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1013), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1014) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1019), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1019) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1025), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1026) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1032), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1033) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1039), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1040) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1118), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1118) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1123), new DateTime(2025, 11, 16, 6, 45, 42, 997, DateTimeKind.Utc).AddTicks(1124) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OAuthTokenRefreshIntervalMinutes",
                table: "EmailConfigurations");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3112), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3271) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3559), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3561) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3565), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3565) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3569), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3569) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3572), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3573) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(892), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(894) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(922), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(923) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(926), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(926) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(929), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(930) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(978), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(979) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(982), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(983) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(986), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(989), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(989) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(992), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(993) });
        }
    }
}
