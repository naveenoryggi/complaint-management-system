using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHasCustomerResponseToComplaint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCustomerResponse",
                table: "Complaints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastResponseAt",
                table: "Complaints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastResponseFrom",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7588), new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7705) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7976), new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7977) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7980), new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7981) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7983), new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7983) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7986), new DateTime(2025, 12, 8, 20, 22, 10, 438, DateTimeKind.Utc).AddTicks(7986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8292), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8298) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8327), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8328) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8333), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8333) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8338), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8338) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8341), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8341) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8344), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8344) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8347), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8347) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8350), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8350) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8353), new DateTime(2025, 12, 8, 20, 22, 10, 440, DateTimeKind.Utc).AddTicks(8353) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCustomerResponse",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "LastResponseAt",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "LastResponseFrom",
                table: "Complaints");

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
    }
}
