using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsAndWhatsAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(366), new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(464) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(672), new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(673) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(676), new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(676) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(680), new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(680) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(684), new DateTime(2025, 10, 20, 17, 47, 27, 522, DateTimeKind.Utc).AddTicks(685) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(8990), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(8993) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9001), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9002) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9004), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9004) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9007), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9008) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9010), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9010) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9013), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9013) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9015), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9015) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9018), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9018) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9020), new DateTime(2025, 10, 20, 17, 47, 27, 523, DateTimeKind.Utc).AddTicks(9021) });
        }
    }
}
