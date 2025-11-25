using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthToEmailConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthenticationType",
                table: "EmailConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OAuthAccessToken",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthClientId",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthClientSecret",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthRefreshToken",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthScopes",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthTenantId",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OAuthTokenExpiresAt",
                table: "EmailConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(8775), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9032) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9466), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9467) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9474), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9474) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9480), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9481) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9486), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9487) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(857), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(863) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(985), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(992), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(993) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(998), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(999) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1004), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1005) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1010), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1011) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1016), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1017) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1022), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1023) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1029), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1030) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationType",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthAccessToken",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthClientId",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthClientSecret",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthRefreshToken",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthScopes",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthTenantId",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "OAuthTokenExpiresAt",
                table: "EmailConfigurations");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(8944), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9289) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9866), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9868) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9874), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9875) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9880), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9881) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9884), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9885) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1150), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1163) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1188), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1189) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1194), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1194) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1198), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1198) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1202), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1202) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1205), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1206) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1209), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1210) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1213), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1214) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1219), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1220) });
        }
    }
}
