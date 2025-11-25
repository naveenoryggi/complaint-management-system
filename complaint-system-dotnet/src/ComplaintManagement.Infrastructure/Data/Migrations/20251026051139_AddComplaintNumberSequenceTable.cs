using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintNumberSequenceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ComplaintNumberSequences table for atomic complaint number generation
            migrationBuilder.CreateTable(
                name: "ComplaintNumberSequences",
                columns: table => new
                {
                    Year = table.Column<int>(nullable: false),
                    LastNumber = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintNumberSequences", x => x.Year);
                });

            // Initialize sequence for current year based on existing complaints
            migrationBuilder.Sql(@"
                DECLARE @CurrentYear INT = YEAR(GETUTCDATE());
                DECLARE @MaxNumber INT;

                SELECT @MaxNumber = ISNULL(MAX(
                    CASE
                        WHEN ComplaintNumber LIKE 'CMP-' + CAST(@CurrentYear AS VARCHAR) + '-%'
                        THEN CAST(RIGHT(ComplaintNumber, 4) AS INT)
                        ELSE 0
                    END
                ), 0)
                FROM Complaints
                WHERE IsDeleted = 0;

                INSERT INTO ComplaintNumberSequences (Year, LastNumber)
                VALUES (@CurrentYear, @MaxNumber);
            ");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6366), new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6483) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6702), new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6702) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6705), new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6705) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6709), new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6710) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6713), new DateTime(2025, 10, 26, 5, 11, 38, 488, DateTimeKind.Utc).AddTicks(6714) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4965), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4969) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4976), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4976) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4979), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4979) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4983), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4983) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4985), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4988), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4988) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4991), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4992) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4994), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4995) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4997), new DateTime(2025, 10, 26, 5, 11, 38, 490, DateTimeKind.Utc).AddTicks(4997) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop ComplaintNumberSequences table
            migrationBuilder.DropTable(
                name: "ComplaintNumberSequences");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8004), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8130) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8347), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8347) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8351), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8351) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8354), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8354) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8358), new DateTime(2025, 10, 22, 20, 5, 32, 911, DateTimeKind.Utc).AddTicks(8358) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7044), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7046) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7054), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7054) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7057), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7057) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7060), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7060) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7063), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7064) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7067), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7068) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7071), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7072) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7074), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7074) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7077), new DateTime(2025, 10, 22, 20, 5, 32, 913, DateTimeKind.Utc).AddTicks(7077) });
        }
    }
}
