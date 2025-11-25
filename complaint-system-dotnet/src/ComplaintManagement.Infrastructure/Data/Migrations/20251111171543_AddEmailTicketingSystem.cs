using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTicketingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImapHost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImapPort = table.Column<int>(type: "int", nullable: false),
                    ImapUseSsl = table.Column<bool>(type: "bit", nullable: false),
                    ImapUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImapPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImapFolder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpHost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    SmtpUseSsl = table.Column<bool>(type: "bit", nullable: false),
                    SmtpUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PollingIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastPolledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SendAutoAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    AutoAcknowledgementTemplateId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoAcknowledgementTemplateId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnableThreading = table.Column<bool>(type: "bit", nullable: false),
                    ThreadTimeoutDays = table.Column<int>(type: "int", nullable: false),
                    MaxAttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    AllowedAttachmentExtensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailConfigurations_CommunicationTemplates_AutoAcknowledgementTemplateId1",
                        column: x => x.AutoAcknowledgementTemplateId1,
                        principalTable: "CommunicationTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailConfigurations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InReplyTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    References = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CcEmails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BccEmails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHtml = table.Column<bool>(type: "bit", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThreadPosition = table.Column<int>(type: "int", nullable: false),
                    IsAutoAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsSpam = table.Column<bool>(type: "bit", nullable: false),
                    Failed = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessages_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailMessages_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailMessages_Users_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StorageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInline = table.Column<bool>(type: "bit", nullable: false),
                    IsScanned = table.Column<bool>(type: "bit", nullable: false),
                    IsSafe = table.Column<bool>(type: "bit", nullable: false),
                    ScanResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChecksumMd5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(8944), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9289) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9866), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9868) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9874), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9875) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9880), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9881) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9884), new DateTime(2025, 11, 11, 17, 15, 40, 365, DateTimeKind.Utc).AddTicks(9885) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1150), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1163) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1188), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1189) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1194), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1194) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1198), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1198) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1202), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1202) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1205), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1206) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1209), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1210) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1213), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1214) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1219), new DateTime(2025, 11, 11, 17, 15, 40, 372, DateTimeKind.Utc).AddTicks(1220) });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailMessageId",
                table: "EmailAttachments",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfigurations_AutoAcknowledgementTemplateId1",
                table: "EmailConfigurations",
                column: "AutoAcknowledgementTemplateId1");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfigurations_CompanyId",
                table: "EmailConfigurations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_CompanyId",
                table: "EmailMessages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ComplaintId",
                table: "EmailMessages",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_SentByUserId",
                table: "EmailMessages",
                column: "SentByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmailConfigurations");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1044), new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1208) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1438), new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1439) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1441), new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1441) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1444), new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1444) });

            migrationBuilder.UpdateData(
                table: "ComplaintPriorityMasters",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1446), new DateTime(2025, 11, 9, 17, 26, 3, 22, DateTimeKind.Utc).AddTicks(1447) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(684), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(686) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(692), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(692) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(694), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(695) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(697), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(697) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(700), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(700) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(734), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(735) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(737), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(738) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(740), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(740) });

            migrationBuilder.UpdateData(
                table: "ComplaintStatusMasters",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(743), new DateTime(2025, 11, 9, 17, 26, 3, 24, DateTimeKind.Utc).AddTicks(743) });
        }
    }
}
