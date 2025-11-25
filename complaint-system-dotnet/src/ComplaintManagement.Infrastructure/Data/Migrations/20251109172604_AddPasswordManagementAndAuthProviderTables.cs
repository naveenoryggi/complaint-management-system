using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordManagementAndAuthProviderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountLockedUntil",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationProviderType",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Local");

            migrationBuilder.AddColumn<bool>(
                name: "ExternalSyncEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserId",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUsername",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdentityProvider",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExternalSyncAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangeRequiredNotificationSentAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LocalPasswordEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePasswordOnNextLogin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasswordChangedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PasswordNeverExpires",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredAuthMethod",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SSOEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "AuthenticationProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EmailDomain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ADDomain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ADServer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ADPort = table.Column<int>(type: "int", nullable: true),
                    ADBaseDN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ADUserFilter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ADUseSSL = table.Column<bool>(type: "bit", nullable: true),
                    ADServiceAccountUsername = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ADServiceAccountPasswordEncrypted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SAMLEntityId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SAMLSSOUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SAMLSLOUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SAMLCertificate = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    SAMLSigningAlgorithm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OAuthClientId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuthClientSecretEncrypted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OAuthAuthorizationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuthTokenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuthUserInfoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuthScopes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuthRedirectUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AzureADTenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AzureADClientId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AzureADClientSecretEncrypted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AzureADInstance = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomAPIEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomAPIMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CustomAPIHeaders = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomAPIRequestTemplate = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CustomAPIKeyEncrypted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    JITProvisioningEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AutoAssignRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SyncAttributesOnLogin = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AttributeMapping = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    GroupToRoleMapping = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHealthCheckAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHealthCheckSuccess = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationProviders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationProviders_ComplaintRoles_AutoAssignRoleId",
                        column: x => x.AutoAssignRoleId,
                        principalTable: "ComplaintRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PasswordAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordAuditLog_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PasswordAuditLog_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PasswordHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordPolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinimumLength = table.Column<int>(type: "int", nullable: false, defaultValue: 8),
                    RequireUppercase = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequireLowercase = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequireDigit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequireSpecialCharacter = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PasswordExpirationDays = table.Column<int>(type: "int", nullable: false, defaultValue: 90),
                    PasswordExpirationWarningDays = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    MaxFailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    AccountLockoutDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    PasswordHistoryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MinimumPasswordAgeDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EnablePasswordComplexity = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowSkipPasswordChange = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SendPasswordExpirationEmails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SendPasswordSetEmails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordPolicy_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalUserMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthenticationProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExternalUsername = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    ExternalGroups = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSyncSuccess = table.Column<bool>(type: "bit", nullable: true),
                    LastSyncDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalUserMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalUserMappings_AuthenticationProviders_AuthenticationProviderId",
                        column: x => x.AuthenticationProviderId,
                        principalTable: "AuthenticationProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalUserMappings_Users_UserId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccountLockedUntil",
                table: "Users",
                column: "AccountLockedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthenticationProviderType",
                table: "Users",
                column: "AuthenticationProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthProviderType_ExternalUserId",
                table: "Users",
                columns: new[] { "AuthenticationProviderType", "ExternalUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalUserId",
                table: "Users",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordExpiresAt",
                table: "Users",
                column: "PasswordExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_AutoAssignRoleId",
                table: "AuthenticationProviders",
                column: "AutoAssignRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_CompanyId",
                table: "AuthenticationProviders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_CompanyId_IsDefault",
                table: "AuthenticationProviders",
                columns: new[] { "CompanyId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_CompanyId_IsEnabled",
                table: "AuthenticationProviders",
                columns: new[] { "CompanyId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_EmailDomain",
                table: "AuthenticationProviders",
                column: "EmailDomain");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_IsDefault",
                table: "AuthenticationProviders",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_IsEnabled",
                table: "AuthenticationProviders",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationProviders_ProviderType",
                table: "AuthenticationProviders",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_AuthenticationProviderId",
                table: "ExternalUserMappings",
                column: "AuthenticationProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_ExternalUserId",
                table: "ExternalUserMappings",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_IsActive",
                table: "ExternalUserMappings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_ProviderId_ExternalUserId",
                table: "ExternalUserMappings",
                columns: new[] { "AuthenticationProviderId", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_UserId",
                table: "ExternalUserMappings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUserMappings_UserId_ProviderId",
                table: "ExternalUserMappings",
                columns: new[] { "UserId", "AuthenticationProviderId" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_Action",
                table: "PasswordAuditLog",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_CreatedAt",
                table: "PasswordAuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_PerformedBy",
                table: "PasswordAuditLog",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_Success",
                table: "PasswordAuditLog",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_Success_CreatedAt",
                table: "PasswordAuditLog",
                columns: new[] { "Success", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_UserId",
                table: "PasswordAuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordAuditLog_UserId_CreatedAt",
                table: "PasswordAuditLog",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_CreatedAt",
                table: "PasswordHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_CreatedBy",
                table: "PasswordHistory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId",
                table: "PasswordHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId_CreatedAt",
                table: "PasswordHistory",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordPolicy_CompanyId",
                table: "PasswordPolicy",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalUserMappings");

            migrationBuilder.DropTable(
                name: "PasswordAuditLog");

            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "PasswordPolicy");

            migrationBuilder.DropTable(
                name: "AuthenticationProviders");

            migrationBuilder.DropIndex(
                name: "IX_Users_AccountLockedUntil",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AuthenticationProviderType",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AuthProviderType_ExternalUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ExternalUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PasswordExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccountLockedUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthenticationProviderType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalSyncEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdentityProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastExternalSyncAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastPasswordChangeRequiredNotificationSentAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LocalPasswordEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MustChangePasswordOnNextLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordNeverExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredAuthMethod",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SSOEnabled",
                table: "Users");

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
        }
    }
}
