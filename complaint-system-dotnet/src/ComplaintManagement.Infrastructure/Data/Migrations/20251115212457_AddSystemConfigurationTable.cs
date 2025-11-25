using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfigurationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestUserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetUserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OAuthTokenRefreshIntervalMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    OAuthTokenExpiryWarningDays = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    DefaultEmailPollingIntervalSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    MaxEmailsFetchPerPoll = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    AutoResponseEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AutoResponseMaxRetryAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    AutoResponseRetryDelaySeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    EmailRateLimitingEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxEmailsPerHour = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    StatusChangeNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignmentNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EscalationNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DefaultTimezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Asia/Kolkata"),
                    DateFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "dd/MM/yyyy"),
                    TimeFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "hh:mm tt"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3112), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3271) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3559), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3561) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3565), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3565) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3569), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3569) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3572), new DateTime(2025, 11, 15, 21, 24, 53, 504, DateTimeKind.Utc).AddTicks(3573) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(892), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(894) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(922), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(923) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(926), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(926) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(929), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(930) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(978), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(979) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(982), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(983) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(986), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(989), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(989) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(992), new DateTime(2025, 11, 15, 21, 24, 53, 507, DateTimeKind.Utc).AddTicks(993) });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_CompanyId",
                table: "SystemConfigurations",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

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
    }
}
