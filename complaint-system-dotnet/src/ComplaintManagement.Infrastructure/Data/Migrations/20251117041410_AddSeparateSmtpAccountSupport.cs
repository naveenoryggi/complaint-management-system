using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparateSmtpAccountSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SmtpAuthenticationType",
                table: "EmailConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateFromEmail",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateFromName",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthAccessToken",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthClientId",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthClientSecret",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthRefreshToken",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthScopes",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateOAuthTenantId",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SmtpSeparateOAuthTokenExpiresAt",
                table: "EmailConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparatePassword",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpSeparateUsername",
                table: "EmailConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSeparateSmtpAccount",
                table: "EmailConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7134), new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7238) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7445), new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7446) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7450), new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7450) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7454), new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7454) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7457), new DateTime(2025, 11, 17, 4, 14, 8, 983, DateTimeKind.Utc).AddTicks(7458) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5370), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5371) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5390), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5391) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5393), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5393) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5395), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5396) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5399), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5399) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5401), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5401) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5404), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5405) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5408), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5409) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5410), new DateTime(2025, 11, 17, 4, 14, 8, 985, DateTimeKind.Utc).AddTicks(5411) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpAuthenticationType",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateFromEmail",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateFromName",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthAccessToken",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthClientId",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthClientSecret",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthRefreshToken",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthScopes",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthTenantId",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateOAuthTokenExpiresAt",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparatePassword",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "SmtpSeparateUsername",
                table: "EmailConfigurations");

            migrationBuilder.DropColumn(
                name: "UseSeparateSmtpAccount",
                table: "EmailConfigurations");

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
    }
}
