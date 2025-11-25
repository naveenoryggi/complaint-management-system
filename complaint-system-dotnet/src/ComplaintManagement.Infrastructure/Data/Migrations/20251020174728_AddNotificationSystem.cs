using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunicationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailablePlaceholders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_CommunicationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationTemplates_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailServerSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    UseSsl = table.Column<bool>(type: "bit", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReplyToEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MaxEmailsPerHour = table.Column<int>(type: "int", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EmailServerSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailServerSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    AvailableFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTypes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsGatewaySettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccountSid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AuthToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MaxSmsPerHour = table.Column<int>(type: "int", nullable: true),
                    CostPerSms = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdditionalConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_SmsGatewaySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsGatewaySettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessAccountId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumberId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WebhookToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MaxMessagesPerHour = table.Column<int>(type: "int", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdditionalConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_WhatsAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventCommunicationRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RecipientPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_CommunicationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationLogs_CommunicationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CommunicationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommunicationLogs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationLogs_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EventCommunicationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientType = table.Column<int>(type: "int", nullable: false),
                    SpecificUserIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificRoleIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificEmails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    DelayMinutes = table.Column<int>(type: "int", nullable: false),
                    SendOnlyOnce = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EventCommunicationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventCommunicationRules_CommunicationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CommunicationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventCommunicationRules_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventCommunicationRules_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_CompanyId",
                table: "CommunicationLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_EntityId_EntityType",
                table: "CommunicationLogs",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_RecipientUserId",
                table: "CommunicationLogs",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_SentAt",
                table: "CommunicationLogs",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_Status_CreatedAt",
                table: "CommunicationLogs",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_TemplateId",
                table: "CommunicationLogs",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_Category",
                table: "CommunicationTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_Channel_IsActive",
                table: "CommunicationTemplates",
                columns: new[] { "Channel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_Code",
                table: "CommunicationTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTemplates_CompanyId",
                table: "CommunicationTemplates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailServerSettings_CompanyId",
                table: "EmailServerSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailServerSettings_IsActive_IsDefault",
                table: "EmailServerSettings",
                columns: new[] { "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_EventCommunicationRules_Channel",
                table: "EventCommunicationRules",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_EventCommunicationRules_CompanyId",
                table: "EventCommunicationRules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCommunicationRules_EventTypeId",
                table: "EventCommunicationRules",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCommunicationRules_IsActive_Priority",
                table: "EventCommunicationRules",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_EventCommunicationRules_TemplateId",
                table: "EventCommunicationRules",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Category",
                table: "EventTypes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Code",
                table: "EventTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_CompanyId",
                table: "EventTypes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_EntityType_IsActive",
                table: "EventTypes",
                columns: new[] { "EntityType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsGatewaySettings_CompanyId",
                table: "SmsGatewaySettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsGatewaySettings_IsActive_IsDefault",
                table: "SmsGatewaySettings",
                columns: new[] { "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsGatewaySettings_Provider",
                table: "SmsGatewaySettings",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppSettings_CompanyId",
                table: "WhatsAppSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppSettings_IsActive_IsDefault",
                table: "WhatsAppSettings",
                columns: new[] { "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppSettings_Provider",
                table: "WhatsAppSettings",
                column: "Provider");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationLogs");

            migrationBuilder.DropTable(
                name: "EmailServerSettings");

            migrationBuilder.DropTable(
                name: "EventCommunicationRules");

            migrationBuilder.DropTable(
                name: "SmsGatewaySettings");

            migrationBuilder.DropTable(
                name: "WhatsAppSettings");

            migrationBuilder.DropTable(
                name: "CommunicationTemplates");

            migrationBuilder.DropTable(
                name: "EventTypes");

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
    }
}
