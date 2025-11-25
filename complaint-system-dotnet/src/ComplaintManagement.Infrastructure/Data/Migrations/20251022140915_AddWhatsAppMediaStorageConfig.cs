using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppMediaStorageConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxMediaSizeMB",
                table: "WhatsAppSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaPublicBaseUrl",
                table: "WhatsAppSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaRetentionDays",
                table: "WhatsAppSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaStorageConfig",
                table: "WhatsAppSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaStoragePath",
                table: "WhatsAppSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaStorageType",
                table: "WhatsAppSettings",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxMediaSizeMB",
                table: "WhatsAppSettings");

            migrationBuilder.DropColumn(
                name: "MediaPublicBaseUrl",
                table: "WhatsAppSettings");

            migrationBuilder.DropColumn(
                name: "MediaRetentionDays",
                table: "WhatsAppSettings");

            migrationBuilder.DropColumn(
                name: "MediaStorageConfig",
                table: "WhatsAppSettings");

            migrationBuilder.DropColumn(
                name: "MediaStoragePath",
                table: "WhatsAppSettings");

            migrationBuilder.DropColumn(
                name: "MediaStorageType",
                table: "WhatsAppSettings");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(8440), new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(8904) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9827), new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9830) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9844), new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9845) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9854), new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9855) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9861), new DateTime(2025, 10, 22, 12, 24, 11, 364, DateTimeKind.Utc).AddTicks(9862) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4362), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4375) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4400), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4401) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4405), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4406) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4409), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4414), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4415) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4420), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4420) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4422), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4422) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4425), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4425) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4427), new DateTime(2025, 10, 22, 12, 24, 11, 377, DateTimeKind.Utc).AddTicks(4428) });
        }
    }
}
