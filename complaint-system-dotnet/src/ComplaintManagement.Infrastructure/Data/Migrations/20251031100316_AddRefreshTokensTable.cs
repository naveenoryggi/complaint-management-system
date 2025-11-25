using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TokenFamily = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3635), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3749) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3991), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3992) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3997), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(3998) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4002), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4003) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4007), new DateTime(2025, 10, 31, 10, 3, 15, 539, DateTimeKind.Utc).AddTicks(4008) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5936), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5951), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5952) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5955), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5955) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5958), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5961), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5962) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5964), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5965) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5968), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5968) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5971), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5972) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5975), new DateTime(2025, 10, 31, 10, 3, 15, 542, DateTimeKind.Utc).AddTicks(5975) });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens",
                column: "TokenFamily");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

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
    }
}
