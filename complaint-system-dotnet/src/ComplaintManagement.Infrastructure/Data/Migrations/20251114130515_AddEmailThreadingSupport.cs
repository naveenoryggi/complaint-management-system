using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailThreadingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PollingIntervalSeconds column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[EmailConfigurations]')
                    AND name = 'PollingIntervalSeconds'
                )
                BEGIN
                    ALTER TABLE [EmailConfigurations] ADD [PollingIntervalSeconds] int NULL
                END
            ");

            migrationBuilder.CreateTable(
                name: "EmailResponseHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentTo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CarbonCopy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BlindCarbonCopy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHtml = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Sent"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasAttachments = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_EmailResponseHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailResponseHistories_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailResponseHistories_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmailResponseHistories_Users_SentBy",
                        column: x => x.SentBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9297), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9416) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9628), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9629) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9633), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9633) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9636), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9637) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9640), new DateTime(2025, 11, 14, 13, 5, 13, 819, DateTimeKind.Utc).AddTicks(9640) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7758), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7760) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7766), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7767) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7769), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7770) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7772), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7772) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7776), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7776) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7778), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7779) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7781), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7781) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7783), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7784) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7787), new DateTime(2025, 11, 14, 13, 5, 13, 821, DateTimeKind.Utc).AddTicks(7787) });

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_ComplaintId",
                table: "EmailResponseHistories",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_ComplaintId_SentAt",
                table: "EmailResponseHistories",
                columns: new[] { "ComplaintId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_DeliveryStatus",
                table: "EmailResponseHistories",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_EmailMessageId",
                table: "EmailResponseHistories",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_SentAt",
                table: "EmailResponseHistories",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailResponseHistories_SentBy",
                table: "EmailResponseHistories",
                column: "SentBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailResponseHistories");

            // Drop PollingIntervalSeconds column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[EmailConfigurations]')
                    AND name = 'PollingIntervalSeconds'
                )
                BEGIN
                    ALTER TABLE [EmailConfigurations] DROP COLUMN [PollingIntervalSeconds]
                END
            ");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(8775), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9032) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9466), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9467) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9474), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9474) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9480), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9481) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9486), new DateTime(2025, 11, 11, 19, 34, 58, 443, DateTimeKind.Utc).AddTicks(9487) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(857), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(863) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(985), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(986) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(992), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(993) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(998), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(999) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1004), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1005) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1010), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1011) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1016), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1017) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1022), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1023) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1029), new DateTime(2025, 11, 11, 19, 34, 58, 448, DateTimeKind.Utc).AddTicks(1030) });
        }
    }
}
