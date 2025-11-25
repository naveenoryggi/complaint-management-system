using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthSupportToEmailServerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthenticationType",
                table: "EmailServerSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OAuthAccessToken",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthClientId",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthClientSecret",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthRefreshToken",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthScopes",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthTenantId",
                table: "EmailServerSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OAuthTokenExpiresAt",
                table: "EmailServerSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OAuthTokenRefreshIntervalMinutes",
                table: "EmailServerSettings",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5300), new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5555) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5977), new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5980) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5985), new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5986) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5992), new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5993) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(5999), new DateTime(2025, 11, 17, 3, 44, 12, 175, DateTimeKind.Utc).AddTicks(6000) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5640), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5646) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5688), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5689) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5693), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5694) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5701), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5702) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5707), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5708) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5712), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5713) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5718), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5719) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5724), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5725) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5729), new DateTime(2025, 11, 17, 3, 44, 12, 179, DateTimeKind.Utc).AddTicks(5730) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationType",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthAccessToken",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthClientId",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthClientSecret",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthRefreshToken",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthScopes",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthTenantId",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthTokenExpiresAt",
                table: "EmailServerSettings");

            migrationBuilder.DropColumn(
                name: "OAuthTokenRefreshIntervalMinutes",
                table: "EmailServerSettings");

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
    }
}
