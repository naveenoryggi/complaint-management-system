using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAndResponsibleUserToAssetAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedToUserId",
                table: "AssetAssignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToCustomerId",
                table: "AssetAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleUserId",
                table: "AssetAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7141), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7268) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7489), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7490) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7492), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7492) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7494), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7494) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7496), new DateTime(2026, 1, 3, 16, 39, 22, 539, DateTimeKind.Utc).AddTicks(7497) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6120), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6122) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6127), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6127) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6129), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6130) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6132), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6132) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6134), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6135) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6137), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6137) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6139), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6140) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6142), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6142) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6144), new DateTime(2026, 1, 3, 16, 39, 22, 541, DateTimeKind.Utc).AddTicks(6144) });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssignedToCustomerId",
                table: "AssetAssignments",
                column: "AssignedToCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_AssignedToCustomerId_IsActive",
                table: "AssetAssignments",
                columns: new[] { "AssignedToCustomerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ResponsibleUserId",
                table: "AssetAssignments",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ResponsibleUserId_IsActive",
                table: "AssetAssignments",
                columns: new[] { "ResponsibleUserId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Customers_AssignedToCustomerId",
                table: "AssetAssignments",
                column: "AssignedToCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Users_ResponsibleUserId",
                table: "AssetAssignments",
                column: "ResponsibleUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Customers_AssignedToCustomerId",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Users_ResponsibleUserId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_AssignedToCustomerId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_AssignedToCustomerId_IsActive",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_ResponsibleUserId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_ResponsibleUserId_IsActive",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "AssignedToCustomerId",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "AssetAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedToUserId",
                table: "AssetAssignments",
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
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2481), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2643) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2922), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2922) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2927), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2927) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2931), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2931) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2934), new DateTime(2026, 1, 3, 13, 42, 11, 624, DateTimeKind.Utc).AddTicks(2935) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(537), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(545) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(554), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(554) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(557), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(558) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(561), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(561) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(564), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(565) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(568), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(568) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(571), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(572) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(575), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(575) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(578), new DateTime(2026, 1, 3, 13, 42, 11, 627, DateTimeKind.Utc).AddTicks(579) });
        }
    }
}
