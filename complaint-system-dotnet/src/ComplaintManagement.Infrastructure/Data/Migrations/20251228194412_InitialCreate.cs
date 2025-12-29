using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComplaintManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultPriority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintCategories_ComplaintCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RoleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EscalationLevel = table.Column<int>(type: "int", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SLALevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ColorCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#4CAF50"),
                    DefaultResponseTime = table.Column<int>(type: "int", nullable: false),
                    ResponseTimeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultResolutionTime = table.Column<int>(type: "int", nullable: false),
                    ResolutionTimeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_SLALevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SLASettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WorkingHoursOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WorkingHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingDays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "1,2,3,4,5"),
                    AutoEscalateOnBreach = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EscalationThresholdPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 80),
                    NotifyBeforeBreach = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotifyBeforeBreachMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    PauseSLAOnPendingInfo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExcludeHolidays = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "UTC"),
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
                    table.PrimaryKey("PK_SLASettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyncStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompaniesProcessed = table.Column<int>(type: "int", nullable: false),
                    BranchesProcessed = table.Column<int>(type: "int", nullable: false),
                    DepartmentsProcessed = table.Column<int>(type: "int", nullable: false),
                    SectionsProcessed = table.Column<int>(type: "int", nullable: false),
                    EmployeesProcessed = table.Column<int>(type: "int", nullable: false),
                    UsersProcessed = table.Column<int>(type: "int", nullable: false),
                    CompaniesCreated = table.Column<int>(type: "int", nullable: false),
                    BranchesCreated = table.Column<int>(type: "int", nullable: false),
                    DepartmentsCreated = table.Column<int>(type: "int", nullable: false),
                    SectionsCreated = table.Column<int>(type: "int", nullable: false),
                    EmployeesCreated = table.Column<int>(type: "int", nullable: false),
                    UsersCreated = table.Column<int>(type: "int", nullable: false),
                    CompaniesUpdated = table.Column<int>(type: "int", nullable: false),
                    BranchesUpdated = table.Column<int>(type: "int", nullable: false),
                    DepartmentsUpdated = table.Column<int>(type: "int", nullable: false),
                    SectionsUpdated = table.Column<int>(type: "int", nullable: false),
                    EmployeesUpdated = table.Column<int>(type: "int", nullable: false),
                    UsersUpdated = table.Column<int>(type: "int", nullable: false),
                    EmployeesFailed = table.Column<int>(type: "int", nullable: false),
                    UsersFailed = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
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
                    table.PrimaryKey("PK_SyncLogs", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiTenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintRolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ComplaintRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintRolePermissions_ComplaintRoles_ComplaintRoleId",
                        column: x => x.ComplaintRoleId,
                        principalTable: "ComplaintRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategorySLAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SLALevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverrideResponseTime = table.Column<int>(type: "int", nullable: true),
                    OverrideResolutionTime = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_CategorySLAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySLAs_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySLAs_SLALevels_SLALevelId",
                        column: x => x.SLALevelId,
                        principalTable: "SLALevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OryggiConnectionSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseWindowsAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    EncryptConnection = table.Column<bool>(type: "bit", nullable: false),
                    TrustServerCertificate = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionTimeout = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTestResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_OryggiConnectionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OryggiConnectionSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeOfDay = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DayValue = table.Column<int>(type: "int", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SyncSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncSchedules_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_AuthenticationProviders_ComplaintRoles_AutoAssignRoleId",
                        column: x => x.AutoAssignRoleId,
                        principalTable: "ComplaintRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiBranchId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HrResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CannedResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannedResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CannedResponses_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflows_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryWorkflowStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsInitialStatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DefaultSLAHours = table.Column<int>(type: "int", nullable: true),
                    EscalationHours = table.Column<int>(type: "int", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowedRoles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflowStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowStatuses_CategoryWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "CategoryWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryWorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransitionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresComment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowedRoles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoTransitionAfterHours = table.Column<int>(type: "int", nullable: true),
                    TransitionConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ButtonColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryWorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryWorkflowTransitions_CategoryWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "CategoryWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiCompanyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HrResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefaultTimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultLocale = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintInformationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowEmployeeCodeToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmailToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowPhoneToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowAlternatePhoneToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowCompanyToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowBranchToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowDepartmentToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowSectionToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowJobTitleToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowManagerDetailsToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowDateOfJoiningToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowPreviousComplaintsToHandlers = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmployeeAddressToManagement = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmergencyContactToManagement = table.Column<bool>(type: "bit", nullable: false),
                    ShowPerformanceMetricsToManagement = table.Column<bool>(type: "bit", nullable: false),
                    MaskPersonalInfoInLogs = table.Column<bool>(type: "bit", nullable: false),
                    RedactInfoAfterClosure = table.Column<bool>(type: "bit", nullable: false),
                    DataRetentionDays = table.Column<int>(type: "int", nullable: false),
                    IncludeEmployeeCodeInReports = table.Column<bool>(type: "bit", nullable: false),
                    IncludeEmailInReports = table.Column<bool>(type: "bit", nullable: false),
                    IncludePhoneInReports = table.Column<bool>(type: "bit", nullable: false),
                    MaskEmailInReports = table.Column<bool>(type: "bit", nullable: false),
                    MaskPhoneInReports = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintInformationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintInformationSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintPriorityMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintPriorityMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintPriorityMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintStatusMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintStatusMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintStatusMasters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Placeholder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisibleToComplainant = table.Column<bool>(type: "bit", nullable: false),
                    IsVisibleToHandler = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CustomFieldDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldDefinition_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthenticationType = table.Column<int>(type: "int", nullable: false),
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
                    OAuthClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OAuthScopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTokenRefreshIntervalMinutes = table.Column<int>(type: "int", nullable: true),
                    PollingIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    PollingIntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastPolledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SendAutoAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    AutoAcknowledgementTemplateId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoAcknowledgementTemplateId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnableThreading = table.Column<bool>(type: "bit", nullable: false),
                    ThreadTimeoutDays = table.Column<int>(type: "int", nullable: false),
                    MaxAttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    AllowedAttachmentExtensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseSeparateSmtpAccount = table.Column<bool>(type: "bit", nullable: false),
                    SmtpAuthenticationType = table.Column<int>(type: "int", nullable: true),
                    SmtpSeparateUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparatePassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateFromEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateFromName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthTenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmtpSeparateOAuthTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SmtpSeparateOAuthScopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "EmailServerSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthenticationType = table.Column<int>(type: "int", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    UseSsl = table.Column<bool>(type: "bit", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReplyToEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OAuthClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OAuthScopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthTokenRefreshIntervalMinutes = table.Column<int>(type: "int", nullable: true),
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
                name: "EmployeeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiEmployeeTypeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_EmployeeTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTypes_Companies_CompanyId",
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
                    MediaStorageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaStoragePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaPublicBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaStorageConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxMediaSizeMB = table.Column<int>(type: "int", nullable: true),
                    MediaRetentionDays = table.Column<int>(type: "int", nullable: true),
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
                name: "PrioritySLAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SLALevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverrideResponseTime = table.Column<int>(type: "int", nullable: true),
                    OverrideResolutionTime = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_PrioritySLAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrioritySLAs_ComplaintPriorityMasters_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "ComplaintPriorityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrioritySLAs_SLALevels_SLALevelId",
                        column: x => x.SLALevelId,
                        principalTable: "SLALevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "ComplaintAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsThumbnail = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintComments_ComplaintComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ComplaintComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintEmailParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComplaintEmailParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplainantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredContactMethod = table.Column<int>(type: "int", nullable: false),
                    StatusMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentEscalationLevel = table.Column<int>(type: "int", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResourcePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HasCustomerResponse = table.Column<bool>(type: "bit", nullable: false),
                    LastResponseFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_ComplaintPriorityMasters_PriorityMasterId",
                        column: x => x.PriorityMasterId,
                        principalTable: "ComplaintPriorityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_ComplaintStatusMasters_StatusMasterId",
                        column: x => x.StatusMasterId,
                        principalTable: "ComplaintStatusMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Complaints_RelatedComplaintId",
                        column: x => x.RelatedComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    JsonValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_CustomFieldValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldValue_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomFieldValue_CustomFieldDefinition_CustomFieldDefinitionId",
                        column: x => x.CustomFieldDefinitionId,
                        principalTable: "CustomFieldDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusWidgets = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layout = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowTrends = table.Column<bool>(type: "bit", nullable: false),
                    ShowPercentages = table.Column<bool>(type: "bit", nullable: false),
                    AutoRefreshInterval = table.Column<int>(type: "int", nullable: false),
                    DateRangeDays = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WidgetConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_DashboardPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HrResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiDepartmentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscalationMatrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EnableAutoEscalation = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SendEmailNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_EscalationMatrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalationMatrices_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    ReadBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReadByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToRecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CcRecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BccRecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiEmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employees_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscalationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsAutoEscalation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignmentStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SlaHoursAtEscalation = table.Column<int>(type: "int", nullable: true),
                    HoursOverdue = table.Column<int>(type: "int", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_EscalationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_EscalationMatrices_EscalationMatrixId",
                        column: x => x.EscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscalationLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TriggerAfterHours = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TriggerAfterValue = table.Column<int>(type: "int", nullable: false),
                    TriggerTimeUnit = table.Column<int>(type: "int", nullable: false),
                    AssignmentStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignToRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignToUserIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HrContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResourcePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResourcePoolAssignmentMethod = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SendNotification = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotifyPreviousHandler = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EscalationMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_EscalationLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationLevels_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EscalationLevels_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EscalationLevels_EscalationMatrices_EscalationMatrixId",
                        column: x => x.EscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscalationPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnableAutoEscalation = table.Column<bool>(type: "bit", nullable: false),
                    RequireManualApproval = table.Column<bool>(type: "bit", nullable: false),
                    DefaultEscalationMatrixId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MinimumSeverityForAutoEscalation = table.Column<int>(type: "int", nullable: true),
                    MaxAutoEscalationLevels = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EscalationPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscalationPolicies_EscalationMatrices_DefaultEscalationMatrixId",
                        column: x => x.DefaultEscalationMatrixId,
                        principalTable: "EscalationMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                });

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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "ResourcePoolMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourcePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ResourcePoolMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourcePools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoolType = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ResourcePools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourcePools_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResourcePools_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourcePools_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryHeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HrResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OryggiSectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OryggiEmployeeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePasswordOnNextLogin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordNeverExpires = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AccountLockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPasswordChangeRequiredNotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthenticationProviderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Local"),
                    ExternalUserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalUsername = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdentityProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastExternalSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalSyncEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SSOEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LocalPasswordEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PreferredAuthMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreferredTimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredLocale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredDateFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredTimeFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_EmployeeTypes_EmployeeTypeId",
                        column: x => x.EmployeeTypeId,
                        principalTable: "EmployeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserComplaintRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_UserComplaintRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_ComplaintRoles_ComplaintRoleId",
                        column: x => x.ComplaintRoleId,
                        principalTable: "ComplaintRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserComplaintRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ComplaintPriorityMasters",
                columns: new[] { "Id", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDeleted", "IsSystem", "Level", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "LOW", "#4CAF50", null, new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(8837), null, null, null, "Low priority - No immediate action required", 1, "bi-arrow-down-circle", true, false, true, 1, "Low", new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9122), null },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "NORMAL", "#2196F3", null, new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9480), null, null, null, "Normal priority - Standard processing time", 2, "bi-dash-circle", true, false, true, 3, "Normal", new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9481), null },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "HIGH", "#FF9800", null, new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9486), null, null, null, "High priority - Requires expedited attention", 3, "bi-exclamation-circle", true, false, true, 5, "High", new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9486), null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "CRITICAL", "#F44336", null, new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9491), null, null, null, "Critical priority - Requires immediate attention", 4, "bi-exclamation-triangle", true, false, true, 8, "Critical", new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9491), null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "URGENT", "#9C27B0", null, new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9495), null, null, null, "Urgent priority - Highest priority level", 5, "bi-lightning", true, false, true, 10, "Urgent", new DateTime(2025, 12, 28, 19, 44, 11, 657, DateTimeKind.Utc).AddTicks(9496), null }
                });

            migrationBuilder.InsertData(
                table: "ComplaintStatusMasters",
                columns: new[] { "Id", "Code", "ColorCode", "CompanyId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IconClass", "IsActive", "IsDeleted", "IsFinal", "IsSystem", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "SUBMITTED", "#9E9E9E", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(681), null, null, null, "Complaint has been submitted but not yet reviewed", 1, "bi-inbox", true, false, false, true, "Submitted", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(687), null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "UNDER_REVIEW", "#2196F3", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(715), null, null, null, "Complaint is being reviewed by the assigned handler", 2, "bi-eye", true, false, false, true, "Under Review", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(716), null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "IN_PROGRESS", "#FF9800", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(719), null, null, null, "Complaint is currently being investigated", 3, "bi-gear", true, false, false, true, "In Progress", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(719), null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "ESCALATED", "#FF5722", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(722), null, null, null, "Complaint has been escalated to a higher level", 4, "bi-arrow-up-circle", true, false, false, true, "Escalated", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(722), null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "PENDING_INFO", "#FFC107", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(725), null, null, null, "Complaint is awaiting information from the complainant", 5, "bi-question-circle", true, false, false, true, "Pending Info", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(725), null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "RESOLVED", "#4CAF50", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(729), null, null, null, "Complaint has been resolved", 6, "bi-check-circle", true, false, false, true, "Resolved", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(729), null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "CLOSED", "#607D8B", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(732), null, null, null, "Complaint has been closed (final state)", 7, "bi-lock", true, false, true, true, "Closed", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(732), null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "REJECTED", "#F44336", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(735), null, null, null, "Complaint has been rejected/dismissed", 8, "bi-x-circle", true, false, true, true, "Rejected", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(735), null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "REOPENED", "#E91E63", null, new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(738), null, null, null, "Complaint has been reopened after closure", 9, "bi-arrow-repeat", true, false, false, true, "Reopened", new DateTime(2025, 12, 28, 19, 44, 11, 662, DateTimeKind.Utc).AddTicks(738), null }
                });

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
                name: "IX_Branches_CompanyId_Code",
                table: "Branches",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_HrResponsibleId",
                table: "Branches",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ManagerId",
                table: "Branches",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_OryggiBranchId",
                table: "Branches",
                column: "OryggiBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_SecondaryManagerId",
                table: "Branches",
                column: "SecondaryManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CategoryId",
                table: "CannedResponses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CompanyId",
                table: "CannedResponses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CannedResponses_CreatedByUserId",
                table: "CannedResponses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_CategoryId",
                table: "CategorySLAs",
                column: "CategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_CategoryId_SLALevelId",
                table: "CategorySLAs",
                columns: new[] { "CategoryId", "SLALevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_IsActive",
                table: "CategorySLAs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySLAs_SLALevelId",
                table: "CategorySLAs",
                column: "SLALevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CategoryId",
                table: "CategoryWorkflows",
                column: "CategoryId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CategoryId_IsDefault",
                table: "CategoryWorkflows",
                columns: new[] { "CategoryId", "IsDefault" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflows_CompanyId",
                table: "CategoryWorkflows",
                column: "CompanyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_StatusMasterId",
                table: "CategoryWorkflowStatuses",
                column: "StatusMasterId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId",
                table: "CategoryWorkflowStatuses",
                column: "WorkflowId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId_DisplayOrder",
                table: "CategoryWorkflowStatuses",
                columns: new[] { "WorkflowId", "DisplayOrder" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowStatuses_WorkflowId_StatusMasterId",
                table: "CategoryWorkflowStatuses",
                columns: new[] { "WorkflowId", "StatusMasterId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_FromStatusId",
                table: "CategoryWorkflowTransitions",
                column: "FromStatusId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_ToStatusId",
                table: "CategoryWorkflowTransitions",
                column: "ToStatusId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId",
                table: "CategoryWorkflowTransitions",
                column: "WorkflowId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId",
                table: "CategoryWorkflowTransitions",
                columns: new[] { "WorkflowId", "FromStatusId" },
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId_ToStatusId",
                table: "CategoryWorkflowTransitions",
                columns: new[] { "WorkflowId", "FromStatusId", "ToStatusId" },
                unique: true,
                filter: "[IsDeleted] = 0");

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
                name: "IX_Companies_HrResponsibleId",
                table: "Companies",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ManagerId",
                table: "Companies",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OryggiCompanyId",
                table: "Companies",
                column: "OryggiCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SecondaryManagerId",
                table: "Companies",
                column: "SecondaryManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TenantId_Code",
                table: "Companies",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_ComplaintId",
                table: "ComplaintAttachments",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_UploadedAt",
                table: "ComplaintAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_UploadedBy",
                table: "ComplaintAttachments",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintCategories_Code",
                table: "ComplaintCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintCategories_DisplayOrder",
                table: "ComplaintCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintCategories_ParentCategoryId",
                table: "ComplaintCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintComments_CommentedAt",
                table: "ComplaintComments",
                column: "CommentedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintComments_CommentedBy",
                table: "ComplaintComments",
                column: "CommentedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintComments_ComplaintId",
                table: "ComplaintComments",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintComments_IsInternal",
                table: "ComplaintComments",
                column: "IsInternal");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintComments_ParentCommentId",
                table: "ComplaintComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintEmailParticipants_AddedByUserId",
                table: "ComplaintEmailParticipants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintEmailParticipants_ComplaintId",
                table: "ComplaintEmailParticipants",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintInformationSettings_CompanyId",
                table: "ComplaintInformationSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_Code",
                table: "ComplaintPriorityMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_Code_CompanyId",
                table: "ComplaintPriorityMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintPriorityMasters_CompanyId",
                table: "ComplaintPriorityMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRolePermissions_ComplaintRoleId",
                table: "ComplaintRolePermissions",
                column: "ComplaintRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRolePermissions_ComplaintRoleId_PermissionType",
                table: "ComplaintRolePermissions",
                columns: new[] { "ComplaintRoleId", "PermissionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRolePermissions_PermissionType",
                table: "ComplaintRolePermissions",
                column: "PermissionType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRoles_Code",
                table: "ComplaintRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRoles_EscalationLevel",
                table: "ComplaintRoles",
                column: "EscalationLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintRoles_RoleType",
                table: "ComplaintRoles",
                column: "RoleType");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AssignedToId",
                table: "Complaints",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BranchId",
                table: "Complaints",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CategoryId",
                table: "Complaints",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CompanyId_StatusMasterId",
                table: "Complaints",
                columns: new[] { "CompanyId", "StatusMasterId" });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantId",
                table: "Complaints",
                column: "ComplainantId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplaintNumber",
                table: "Complaints",
                column: "ComplaintNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CurrentEscalationLevel",
                table: "Complaints",
                column: "CurrentEscalationLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_DepartmentId",
                table: "Complaints",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_DueDate",
                table: "Complaints",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_PriorityMasterId",
                table: "Complaints",
                column: "PriorityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_RelatedComplaintId",
                table: "Complaints",
                column: "RelatedComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ResourcePoolId",
                table: "Complaints",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SectionId",
                table: "Complaints",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_StatusMasterId",
                table: "Complaints",
                column: "StatusMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SubmittedAt",
                table: "Complaints",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_Code",
                table: "ComplaintStatusMasters",
                column: "Code",
                unique: true,
                filter: "[CompanyId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_Code_CompanyId",
                table: "ComplaintStatusMasters",
                columns: new[] { "Code", "CompanyId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintStatusMasters_CompanyId",
                table: "ComplaintStatusMasters",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinition_CompanyId",
                table: "CustomFieldDefinition",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValue_ComplaintId",
                table: "CustomFieldValue",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldValue_CustomFieldDefinitionId",
                table: "CustomFieldValue",
                column: "CustomFieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardPreferences_UserId",
                table: "DashboardPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId_Code",
                table: "Departments",
                columns: new[] { "BranchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HrResponsibleId",
                table: "Departments",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OryggiDepartmentId",
                table: "Departments",
                column: "OryggiDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SecondaryManagerId",
                table: "Departments",
                column: "SecondaryManagerId");

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
                name: "IX_EmailMessages_ReadByUserId",
                table: "EmailMessages",
                column: "ReadByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_SentByUserId",
                table: "EmailMessages",
                column: "SentByUserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_EmailServerSettings_CompanyId",
                table: "EmailServerSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailServerSettings_IsActive_IsDefault",
                table: "EmailServerSettings",
                columns: new[] { "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SectionId",
                table: "Employees",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TenantId",
                table: "Employees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_CompanyId_Code",
                table: "EmployeeTypes",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_OryggiEmployeeTypeId",
                table: "EmployeeTypes",
                column: "OryggiEmployeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId",
                table: "EscalationHistories",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId_EscalatedAt",
                table: "EscalationHistories",
                columns: new[] { "ComplaintId", "EscalatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ComplaintId_Level",
                table: "EscalationHistories",
                columns: new[] { "ComplaintId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalatedAt",
                table: "EscalationHistories",
                column: "EscalatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalatedBy",
                table: "EscalationHistories",
                column: "EscalatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalationLevelId",
                table: "EscalationHistories",
                column: "EscalationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalationMatrixId",
                table: "EscalationHistories",
                column: "EscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_FromUserId",
                table: "EscalationHistories",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_Status",
                table: "EscalationHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ToUserId",
                table: "EscalationHistories",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_AssignToUserId",
                table: "EscalationLevels",
                column: "AssignToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_BranchId",
                table: "EscalationLevels",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_DepartmentId",
                table: "EscalationLevels",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_EscalationMatrixId",
                table: "EscalationLevels",
                column: "EscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_EscalationMatrixId_Level",
                table: "EscalationLevels",
                columns: new[] { "EscalationMatrixId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_HrContactId",
                table: "EscalationLevels",
                column: "HrContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_IsActive",
                table: "EscalationLevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_PrimaryContactId",
                table: "EscalationLevels",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_ResourcePoolId",
                table: "EscalationLevels",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_SecondaryContactId",
                table: "EscalationLevels",
                column: "SecondaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_BranchId",
                table: "EscalationMatrices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CategoryId",
                table: "EscalationMatrices",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CompanyId",
                table: "EscalationMatrices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_CompanyId_IsActive_Priority",
                table: "EscalationMatrices",
                columns: new[] { "CompanyId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_DepartmentId",
                table: "EscalationMatrices",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationMatrices_IsActive",
                table: "EscalationMatrices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_BranchId",
                table: "EscalationPolicies",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CategoryId",
                table: "EscalationPolicies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_BranchId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_CategoryId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_DepartmentId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "DepartmentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_CompanyId_SectionId_IsActive",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "SectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_DefaultEscalationMatrixId",
                table: "EscalationPolicies",
                column: "DefaultEscalationMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_DepartmentId",
                table: "EscalationPolicies",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_FullHierarchy",
                table: "EscalationPolicies",
                columns: new[] { "CompanyId", "BranchId", "DepartmentId", "SectionId", "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EscalationPolicies_SectionId",
                table: "EscalationPolicies",
                column: "SectionId");

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
                name: "IX_OryggiConnectionSettings_TenantId",
                table: "OryggiConnectionSettings",
                column: "TenantId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_IsActive",
                table: "PrioritySLAs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_PriorityId",
                table: "PrioritySLAs",
                column: "PriorityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_PriorityId_SLALevelId",
                table: "PrioritySLAs",
                columns: new[] { "PriorityId", "SLALevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySLAs_SLALevelId",
                table: "PrioritySLAs",
                column: "SLALevelId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePoolMembers_ResourcePoolId",
                table: "ResourcePoolMembers",
                column: "ResourcePoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePoolMembers_UserId",
                table: "ResourcePoolMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_BranchId",
                table: "ResourcePools",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_CompanyId",
                table: "ResourcePools",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_DepartmentId",
                table: "ResourcePools",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePools_SectionId",
                table: "ResourcePools",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId_Code",
                table: "Sections",
                columns: new[] { "DepartmentId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_HeadId",
                table: "Sections",
                column: "HeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_HrResponsibleId",
                table: "Sections",
                column: "HrResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_OryggiSectionId",
                table: "Sections",
                column: "OryggiSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SecondaryHeadId",
                table: "Sections",
                column: "SecondaryHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId",
                table: "SLALevels",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId_Name",
                table: "SLALevels",
                columns: new[] { "CompanyId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_CompanyId_Order",
                table: "SLALevels",
                columns: new[] { "CompanyId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_IsActive",
                table: "SLALevels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SLALevels_Order",
                table: "SLALevels",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_SLASettings_CompanyId",
                table: "SLASettings",
                column: "CompanyId",
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SLASettings_IsEnabled",
                table: "SLASettings",
                column: "IsEnabled");

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
                name: "IX_SyncSchedules_TenantId",
                table: "SyncSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_CompanyId",
                table: "SystemConfigurations",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_OryggiTenantId",
                table: "Tenants",
                column: "OryggiTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_BranchId",
                table: "UserComplaintRoles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_CompanyId",
                table: "UserComplaintRoles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_ComplaintRoleId",
                table: "UserComplaintRoles",
                column: "ComplaintRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_DepartmentId",
                table: "UserComplaintRoles",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_EffectiveFrom_EffectiveTo",
                table: "UserComplaintRoles",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_SectionId",
                table: "UserComplaintRoles",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_UserId",
                table: "UserComplaintRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComplaintRoles_UserId_ComplaintRoleId_IsActive",
                table: "UserComplaintRoles",
                columns: new[] { "UserId", "ComplaintRoleId", "IsActive" });

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
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeCode",
                table: "Users",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeTypeId",
                table: "Users",
                column: "EmployeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalUserId",
                table: "Users",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OryggiEmployeeId",
                table: "Users",
                column: "OryggiEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordExpiresAt",
                table: "Users",
                column: "PasswordExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SectionId",
                table: "Users",
                column: "SectionId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AuthenticationProviders_Companies_CompanyId",
                table: "AuthenticationProviders",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_HrResponsibleId",
                table: "Branches",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_ManagerId",
                table: "Branches",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Users_SecondaryManagerId",
                table: "Branches",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CannedResponses_Companies_CompanyId",
                table: "CannedResponses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CannedResponses_Users_CreatedByUserId",
                table: "CannedResponses",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWorkflows_Companies_CompanyId",
                table: "CategoryWorkflows",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWorkflowStatuses_ComplaintStatusMasters_StatusMasterId",
                table: "CategoryWorkflowStatuses",
                column: "StatusMasterId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWorkflowTransitions_ComplaintStatusMasters_FromStatusId",
                table: "CategoryWorkflowTransitions",
                column: "FromStatusId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWorkflowTransitions_ComplaintStatusMasters_ToStatusId",
                table: "CategoryWorkflowTransitions",
                column: "ToStatusId",
                principalTable: "ComplaintStatusMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationLogs_CommunicationTemplates_TemplateId",
                table: "CommunicationLogs",
                column: "TemplateId",
                principalTable: "CommunicationTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationLogs_Companies_CompanyId",
                table: "CommunicationLogs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationLogs_Users_RecipientUserId",
                table: "CommunicationLogs",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationTemplates_Companies_CompanyId",
                table: "CommunicationTemplates",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_HrResponsibleId",
                table: "Companies",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_ManagerId",
                table: "Companies",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_SecondaryManagerId",
                table: "Companies",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintAttachments_Complaints_ComplaintId",
                table: "ComplaintAttachments",
                column: "ComplaintId",
                principalTable: "Complaints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintAttachments_Users_UploadedBy",
                table: "ComplaintAttachments",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintComments_Complaints_ComplaintId",
                table: "ComplaintComments",
                column: "ComplaintId",
                principalTable: "Complaints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintComments_Users_CommentedBy",
                table: "ComplaintComments",
                column: "CommentedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintEmailParticipants_Complaints_ComplaintId",
                table: "ComplaintEmailParticipants",
                column: "ComplaintId",
                principalTable: "Complaints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintEmailParticipants_Users_AddedByUserId",
                table: "ComplaintEmailParticipants",
                column: "AddedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Departments_DepartmentId",
                table: "Complaints",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ResourcePools_ResourcePoolId",
                table: "Complaints",
                column: "ResourcePoolId",
                principalTable: "ResourcePools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Sections_SectionId",
                table: "Complaints",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_AssignedToId",
                table: "Complaints",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints",
                column: "ComplainantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DashboardPreferences_Users_UserId",
                table: "DashboardPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HrResponsibleId",
                table: "Departments",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_SecondaryManagerId",
                table: "Departments",
                column: "SecondaryManagerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailAttachments_EmailMessages_EmailMessageId",
                table: "EmailAttachments",
                column: "EmailMessageId",
                principalTable: "EmailMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessages_Users_ReadByUserId",
                table: "EmailMessages",
                column: "ReadByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessages_Users_SentByUserId",
                table: "EmailMessages",
                column: "SentByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailResponseHistories_Users_SentBy",
                table: "EmailResponseHistories",
                column: "SentBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Sections_SectionId",
                table: "Employees",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationHistories_EscalationLevels_EscalationLevelId",
                table: "EscalationHistories",
                column: "EscalationLevelId",
                principalTable: "EscalationLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationHistories_Users_EscalatedBy",
                table: "EscalationHistories",
                column: "EscalatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationHistories_Users_FromUserId",
                table: "EscalationHistories",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationHistories_Users_ToUserId",
                table: "EscalationHistories",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_ResourcePools_ResourcePoolId",
                table: "EscalationLevels",
                column: "ResourcePoolId",
                principalTable: "ResourcePools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_AssignToUserId",
                table: "EscalationLevels",
                column: "AssignToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_HrContactId",
                table: "EscalationLevels",
                column: "HrContactId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_PrimaryContactId",
                table: "EscalationLevels",
                column: "PrimaryContactId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationLevels_Users_SecondaryContactId",
                table: "EscalationLevels",
                column: "SecondaryContactId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalationPolicies_Sections_SectionId",
                table: "EscalationPolicies",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalUserMappings_Users_UserId",
                table: "ExternalUserMappings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordAuditLog_Users_PerformedBy",
                table: "PasswordAuditLog",
                column: "PerformedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordAuditLog_Users_UserId",
                table: "PasswordAuditLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistory_Users_CreatedBy",
                table: "PasswordHistory",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistory_Users_UserId",
                table: "PasswordHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetTokens_Users_UserId",
                table: "PasswordResetTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourcePoolMembers_ResourcePools_ResourcePoolId",
                table: "ResourcePoolMembers",
                column: "ResourcePoolId",
                principalTable: "ResourcePools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourcePoolMembers_Users_UserId",
                table: "ResourcePoolMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResourcePools_Sections_SectionId",
                table: "ResourcePools",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections",
                column: "HeadId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HrResponsibleId",
                table: "Sections",
                column: "HrResponsibleId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_SecondaryHeadId",
                table: "Sections",
                column: "SecondaryHeadId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTypes_Companies_CompanyId",
                table: "EmployeeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_HrResponsibleId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_ManagerId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Users_SecondaryManagerId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HrResponsibleId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_SecondaryManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HrResponsibleId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_SecondaryHeadId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "CannedResponses");

            migrationBuilder.DropTable(
                name: "CategorySLAs");

            migrationBuilder.DropTable(
                name: "CategoryWorkflowStatuses");

            migrationBuilder.DropTable(
                name: "CategoryWorkflowTransitions");

            migrationBuilder.DropTable(
                name: "CommunicationLogs");

            migrationBuilder.DropTable(
                name: "ComplaintAttachments");

            migrationBuilder.DropTable(
                name: "ComplaintComments");

            migrationBuilder.DropTable(
                name: "ComplaintEmailParticipants");

            migrationBuilder.DropTable(
                name: "ComplaintInformationSettings");

            migrationBuilder.DropTable(
                name: "ComplaintRolePermissions");

            migrationBuilder.DropTable(
                name: "CustomFieldValue");

            migrationBuilder.DropTable(
                name: "DashboardPreferences");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "EmailConfigurations");

            migrationBuilder.DropTable(
                name: "EmailResponseHistories");

            migrationBuilder.DropTable(
                name: "EmailServerSettings");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "EscalationHistories");

            migrationBuilder.DropTable(
                name: "EscalationPolicies");

            migrationBuilder.DropTable(
                name: "EventCommunicationRules");

            migrationBuilder.DropTable(
                name: "ExternalUserMappings");

            migrationBuilder.DropTable(
                name: "OryggiConnectionSettings");

            migrationBuilder.DropTable(
                name: "PasswordAuditLog");

            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "PasswordPolicy");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PrioritySLAs");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ResourcePoolMembers");

            migrationBuilder.DropTable(
                name: "SLASettings");

            migrationBuilder.DropTable(
                name: "SmsGatewaySettings");

            migrationBuilder.DropTable(
                name: "SyncLogs");

            migrationBuilder.DropTable(
                name: "SyncSchedules");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "UserComplaintRoles");

            migrationBuilder.DropTable(
                name: "WhatsAppSettings");

            migrationBuilder.DropTable(
                name: "CategoryWorkflows");

            migrationBuilder.DropTable(
                name: "CustomFieldDefinition");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "EscalationLevels");

            migrationBuilder.DropTable(
                name: "CommunicationTemplates");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "AuthenticationProviders");

            migrationBuilder.DropTable(
                name: "SLALevels");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "EscalationMatrices");

            migrationBuilder.DropTable(
                name: "ComplaintRoles");

            migrationBuilder.DropTable(
                name: "ComplaintPriorityMasters");

            migrationBuilder.DropTable(
                name: "ComplaintStatusMasters");

            migrationBuilder.DropTable(
                name: "ResourcePools");

            migrationBuilder.DropTable(
                name: "ComplaintCategories");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "EmployeeTypes");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
