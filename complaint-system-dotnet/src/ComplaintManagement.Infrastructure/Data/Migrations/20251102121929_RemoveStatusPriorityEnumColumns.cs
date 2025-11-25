using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusPriorityEnumColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CompanyId_Status",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_Priority",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_Status",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Complaints");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PriorityMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(6925), new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(7564) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9011), new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9016) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9026), new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9027) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9033), new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9034) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9040), new DateTime(2025, 11, 2, 12, 19, 24, 196, DateTimeKind.Utc).AddTicks(9041) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6547), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6560) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6631), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6633) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6641), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6642) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6650), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6651) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6658), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6659) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6666), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6667) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6672), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6673) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6679), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6680) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6686), new DateTime(2025, 11, 2, 12, 19, 24, 204, DateTimeKind.Utc).AddTicks(6687) });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CompanyId_StatusMasterId",
                table: "Complaints",
                columns: new[] { "CompanyId", "StatusMasterId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints",
                column: "PriorityMasterId",
                principalTable: "ComplaintPriorityMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints",
                column: "StatusMasterId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CompanyId_StatusMasterId",
                table: "Complaints");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PriorityMasterId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Complaints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Complaints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(4634), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(4843) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5215), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5216) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5222), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5223) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5228), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5229) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5234), new DateTime(2025, 11, 2, 9, 11, 47, 52, DateTimeKind.Utc).AddTicks(5235) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3931), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3938) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3951), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3952) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3956), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(3957) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4038), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4039) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4043), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4044) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4048), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4049) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4053), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4054) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4058), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4058) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4062), new DateTime(2025, 11, 2, 9, 11, 47, 57, DateTimeKind.Utc).AddTicks(4063) });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CompanyId_Status",
                table: "Complaints",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Priority",
                table: "Complaints",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_Status",
                table: "Complaints",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                table: "Complaints",
                column: "PriorityMasterId",
                principalTable: "ComplaintPriorityMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                table: "Complaints",
                column: "StatusMasterId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
