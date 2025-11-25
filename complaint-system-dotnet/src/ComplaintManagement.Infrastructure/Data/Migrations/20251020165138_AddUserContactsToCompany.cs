using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserContactsToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6112), new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6203) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6395), new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6396) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6398), new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6399) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6401), new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6402) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6404), new DateTime(2025, 10, 20, 16, 51, 37, 657, DateTimeKind.Utc).AddTicks(6405) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6116), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6118) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6126), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6127) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6130), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6131) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6134), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6135) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6138), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6139) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6142), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6142) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6145), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6146) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6149), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6150) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6153), new DateTime(2025, 10, 20, 16, 51, 37, 659, DateTimeKind.Utc).AddTicks(6153) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(6863), new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7075) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7505), new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7507) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7514), new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7515) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7521), new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7522) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7529), new DateTime(2025, 10, 19, 20, 12, 31, 794, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8139), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8144) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8164), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8165) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8170), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8171) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8176), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8176) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8183), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8184) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8190), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8191) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8195), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8196) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8202), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8203) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8208), new DateTime(2025, 10, 19, 20, 12, 31, 798, DateTimeKind.Utc).AddTicks(8209) });
        }
    }
}
