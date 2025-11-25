using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimezoneAndLocalizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredDateFormat",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocale",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeFormat",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeZone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLocale",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultTimeZone",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Asia/Kolkata");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5381), new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5566) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5921), new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5922) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5927), new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5927) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5931), new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5932) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5936), new DateTime(2025, 11, 15, 8, 0, 4, 417, DateTimeKind.Utc).AddTicks(5936) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9354), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9356) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9367), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9368) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9371), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9372) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9376), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9376) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9380), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9381) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9384), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9385) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9389), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9389) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9393), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9394) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9397), new DateTime(2025, 11, 15, 8, 0, 4, 420, DateTimeKind.Utc).AddTicks(9398) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredDateFormat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLocale",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredTimeFormat",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredTimeZone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultLocale",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DefaultTimeZone",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Branches");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(8181), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(8546) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9121), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9123) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9130), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9130) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9135), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9136) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9140), new DateTime(2025, 11, 14, 19, 3, 3, 762, DateTimeKind.Utc).AddTicks(9140) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4262), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4268) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4288), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4289) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4294), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4294) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4299), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4299) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4303), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4304) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4309), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4310) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4314), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4314) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4318), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4319) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4323), new DateTime(2025, 11, 14, 19, 3, 3, 768, DateTimeKind.Utc).AddTicks(4323) });
        }
    }
}
