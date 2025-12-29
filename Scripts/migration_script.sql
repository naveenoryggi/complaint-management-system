IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ParentCategoryId] uniqueidentifier NULL,
        [DefaultPriority] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [WorkflowId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintCategories_ComplaintCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [RoleType] nvarchar(50) NOT NULL,
        [EscalationLevel] int NOT NULL,
        [IsSystemRole] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SLALevels] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Order] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [ColorCode] nvarchar(7) NOT NULL DEFAULT N'#4CAF50',
        [DefaultResponseTime] int NOT NULL,
        [ResponseTimeUnit] nvarchar(20) NOT NULL,
        [DefaultResolutionTime] int NOT NULL,
        [ResolutionTimeUnit] nvarchar(20) NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SLALevels] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SLASettings] (
        [Id] uniqueidentifier NOT NULL,
        [IsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [WorkingHoursOnly] bit NOT NULL DEFAULT CAST(0 AS bit),
        [WorkingHoursStart] time NULL,
        [WorkingHoursEnd] time NULL,
        [WorkingDays] nvarchar(50) NOT NULL DEFAULT N'1,2,3,4,5',
        [AutoEscalateOnBreach] bit NOT NULL DEFAULT CAST(1 AS bit),
        [EscalationThresholdPercent] int NOT NULL DEFAULT 80,
        [NotifyBeforeBreach] bit NOT NULL DEFAULT CAST(1 AS bit),
        [NotifyBeforeBreachMinutes] int NOT NULL DEFAULT 30,
        [PauseSLAOnPendingInfo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [ExcludeHolidays] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Timezone] nvarchar(100) NOT NULL DEFAULT N'UTC',
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SLASettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [SyncLogId] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [SyncType] nvarchar(max) NOT NULL,
        [SyncStartedAt] datetime2 NOT NULL,
        [SyncCompletedAt] datetime2 NULL,
        [Status] nvarchar(max) NOT NULL,
        [CompaniesProcessed] int NOT NULL,
        [BranchesProcessed] int NOT NULL,
        [DepartmentsProcessed] int NOT NULL,
        [SectionsProcessed] int NOT NULL,
        [EmployeesProcessed] int NOT NULL,
        [UsersProcessed] int NOT NULL,
        [CompaniesCreated] int NOT NULL,
        [BranchesCreated] int NOT NULL,
        [DepartmentsCreated] int NOT NULL,
        [SectionsCreated] int NOT NULL,
        [EmployeesCreated] int NOT NULL,
        [UsersCreated] int NOT NULL,
        [CompaniesUpdated] int NOT NULL,
        [BranchesUpdated] int NOT NULL,
        [DepartmentsUpdated] int NOT NULL,
        [SectionsUpdated] int NOT NULL,
        [EmployeesUpdated] int NOT NULL,
        [UsersUpdated] int NOT NULL,
        [EmployeesFailed] int NOT NULL,
        [UsersFailed] int NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ErrorDetails] nvarchar(max) NULL,
        [Duration] time NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SyncLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SystemConfigurations] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [OAuthTokenRefreshIntervalMinutes] int NOT NULL DEFAULT 30,
        [OAuthTokenExpiryWarningDays] int NOT NULL DEFAULT 7,
        [DefaultEmailPollingIntervalSeconds] int NOT NULL DEFAULT 300,
        [MaxEmailsFetchPerPoll] int NOT NULL DEFAULT 50,
        [AutoResponseEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AutoResponseMaxRetryAttempts] int NOT NULL DEFAULT 3,
        [AutoResponseRetryDelaySeconds] int NOT NULL DEFAULT 60,
        [EmailRateLimitingEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [MaxEmailsPerHour] int NOT NULL DEFAULT 100,
        [StatusChangeNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AssignmentNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [EscalationNotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DefaultTimezone] nvarchar(100) NOT NULL DEFAULT N'Asia/Kolkata',
        [DateFormat] nvarchar(50) NOT NULL DEFAULT N'dd/MM/yyyy',
        [TimeFormat] nvarchar(50) NOT NULL DEFAULT N'hh:mm tt',
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SystemConfigurations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Tenants] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ContactEmail] nvarchar(255) NOT NULL,
        [ContactPhone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [OryggiTenantId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintRolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintRoleId] uniqueidentifier NOT NULL,
        [PermissionType] nvarchar(100) NOT NULL,
        [IsGranted] bit NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintRolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintRolePermissions_ComplaintRoles_ComplaintRoleId] FOREIGN KEY ([ComplaintRoleId]) REFERENCES [ComplaintRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CategorySLAs] (
        [Id] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [SLALevelId] uniqueidentifier NOT NULL,
        [OverrideResponseTime] int NULL,
        [OverrideResolutionTime] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CategorySLAs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CategorySLAs_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CategorySLAs_SLALevels_SLALevelId] FOREIGN KEY ([SLALevelId]) REFERENCES [SLALevels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [OryggiConnectionSettings] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [ServerAddress] nvarchar(max) NOT NULL,
        [Port] int NOT NULL,
        [DatabaseName] nvarchar(max) NOT NULL,
        [EncryptedUsername] nvarchar(max) NOT NULL,
        [EncryptedPassword] nvarchar(max) NOT NULL,
        [UseWindowsAuthentication] bit NOT NULL,
        [EncryptConnection] bit NOT NULL,
        [TrustServerCertificate] bit NOT NULL,
        [ConnectionTimeout] int NOT NULL,
        [IsActive] bit NOT NULL,
        [LastTestedAt] datetime2 NULL,
        [LastTestResult] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_OryggiConnectionSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OryggiConnectionSettings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SyncSchedules] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [ScheduleType] nvarchar(50) NOT NULL,
        [TimeOfDay] nvarchar(5) NOT NULL,
        [DayValue] int NULL,
        [IsEnabled] bit NOT NULL,
        [LastRunAt] datetime2 NULL,
        [NextRunAt] datetime2 NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SyncSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SyncSchedules_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [AuthenticationProviders] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [ProviderType] nvarchar(50) NOT NULL,
        [ProviderName] nvarchar(100) NOT NULL,
        [IsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Priority] int NOT NULL DEFAULT 0,
        [EmailDomain] nvarchar(200) NULL,
        [ADDomain] nvarchar(200) NULL,
        [ADServer] nvarchar(200) NULL,
        [ADPort] int NULL,
        [ADBaseDN] nvarchar(500) NULL,
        [ADUserFilter] nvarchar(500) NULL,
        [ADUseSSL] bit NULL,
        [ADServiceAccountUsername] nvarchar(200) NULL,
        [ADServiceAccountPasswordEncrypted] nvarchar(1000) NULL,
        [SAMLEntityId] nvarchar(500) NULL,
        [SAMLSSOUrl] nvarchar(500) NULL,
        [SAMLSLOUrl] nvarchar(500) NULL,
        [SAMLCertificate] nvarchar(max) NULL,
        [SAMLSigningAlgorithm] nvarchar(200) NULL,
        [OAuthClientId] nvarchar(500) NULL,
        [OAuthClientSecretEncrypted] nvarchar(1000) NULL,
        [OAuthAuthorizationUrl] nvarchar(500) NULL,
        [OAuthTokenUrl] nvarchar(500) NULL,
        [OAuthUserInfoUrl] nvarchar(500) NULL,
        [OAuthScopes] nvarchar(500) NULL,
        [OAuthRedirectUri] nvarchar(500) NULL,
        [AzureADTenantId] nvarchar(100) NULL,
        [AzureADClientId] nvarchar(500) NULL,
        [AzureADClientSecretEncrypted] nvarchar(1000) NULL,
        [AzureADInstance] nvarchar(500) NULL,
        [CustomAPIEndpoint] nvarchar(500) NULL,
        [CustomAPIMethod] nvarchar(10) NULL,
        [CustomAPIHeaders] nvarchar(2000) NULL,
        [CustomAPIRequestTemplate] nvarchar(2000) NULL,
        [CustomAPIKeyEncrypted] nvarchar(1000) NULL,
        [JITProvisioningEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AutoAssignRoleId] uniqueidentifier NULL,
        [SyncAttributesOnLogin] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AttributeMapping] nvarchar(max) NULL,
        [GroupToRoleMapping] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [CreatedBy] uniqueidentifier NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [LastUsedAt] datetime2 NULL,
        [LastHealthCheckAt] datetime2 NULL,
        [LastHealthCheckSuccess] bit NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_AuthenticationProviders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuthenticationProviders_ComplaintRoles_AutoAssignRoleId] FOREIGN KEY ([AutoAssignRoleId]) REFERENCES [ComplaintRoles] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ContactEmail] nvarchar(255) NULL,
        [ContactPhone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [City] nvarchar(100) NULL,
        [Country] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [OryggiBranchId] nvarchar(100) NULL,
        [ManagerId] uniqueidentifier NULL,
        [SecondaryManagerId] uniqueidentifier NULL,
        [HrResponsibleId] uniqueidentifier NULL,
        [TimeZone] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CannedResponses] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NULL,
        [Title] nvarchar(max) NOT NULL,
        [ShortCode] nvarchar(max) NULL,
        [Subject] nvarchar(max) NULL,
        [Body] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [UsageCount] int NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CannedResponses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CannedResponses_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoryWorkflows] (
        [Id] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsDefault] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CategoryWorkflows] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CategoryWorkflows_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoryWorkflowStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [WorkflowId] uniqueidentifier NOT NULL,
        [StatusMasterId] uniqueidentifier NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsInitialStatus] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DefaultSLAHours] int NULL,
        [EscalationHours] int NULL,
        [RequiresApproval] bit NOT NULL DEFAULT CAST(0 AS bit),
        [AllowedRoles] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CategoryWorkflowStatuses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CategoryWorkflowStatuses_CategoryWorkflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [CategoryWorkflows] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoryWorkflowTransitions] (
        [Id] uniqueidentifier NOT NULL,
        [WorkflowId] uniqueidentifier NOT NULL,
        [FromStatusId] uniqueidentifier NOT NULL,
        [ToStatusId] uniqueidentifier NOT NULL,
        [TransitionName] nvarchar(200) NULL,
        [Description] nvarchar(500) NULL,
        [RequiresComment] bit NOT NULL DEFAULT CAST(0 AS bit),
        [RequiresApproval] bit NOT NULL DEFAULT CAST(0 AS bit),
        [AllowedRoles] nvarchar(2000) NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsAutomatic] bit NOT NULL DEFAULT CAST(0 AS bit),
        [AutoTransitionAfterHours] int NULL,
        [TransitionConditions] nvarchar(4000) NULL,
        [ButtonColor] nvarchar(50) NULL,
        [IconClass] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CategoryWorkflowTransitions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CategoryWorkflowTransitions_CategoryWorkflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [CategoryWorkflows] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CommunicationLogs] (
        [Id] uniqueidentifier NOT NULL,
        [Channel] int NOT NULL,
        [TemplateId] uniqueidentifier NULL,
        [EventCommunicationRuleId] uniqueidentifier NULL,
        [RecipientEmail] nvarchar(255) NULL,
        [RecipientPhone] nvarchar(50) NULL,
        [RecipientUserId] uniqueidentifier NULL,
        [Subject] nvarchar(500) NULL,
        [Body] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [SentAt] datetime2 NULL,
        [DeliveredAt] datetime2 NULL,
        [ReadAt] datetime2 NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [RetryCount] int NOT NULL,
        [ExternalMessageId] nvarchar(255) NULL,
        [EntityId] uniqueidentifier NULL,
        [EntityType] nvarchar(100) NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CommunicationLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CommunicationTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Channel] int NOT NULL,
        [Subject] nvarchar(500) NULL,
        [Body] nvarchar(max) NOT NULL,
        [HtmlBody] nvarchar(max) NULL,
        [AvailablePlaceholders] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [Language] nvarchar(10) NULL,
        [Category] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CommunicationTemplates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ContactEmail] nvarchar(255) NULL,
        [ContactPhone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [OryggiCompanyId] nvarchar(100) NULL,
        [LogoUrl] nvarchar(max) NULL,
        [LogoFileName] nvarchar(max) NULL,
        [LogoContentType] nvarchar(max) NULL,
        [ManagerId] uniqueidentifier NULL,
        [SecondaryManagerId] uniqueidentifier NULL,
        [HrResponsibleId] uniqueidentifier NULL,
        [DefaultTimeZone] nvarchar(max) NOT NULL,
        [DefaultLocale] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Companies_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintInformationSettings] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [ShowEmployeeCodeToHandlers] bit NOT NULL,
        [ShowEmailToHandlers] bit NOT NULL,
        [ShowPhoneToHandlers] bit NOT NULL,
        [ShowAlternatePhoneToHandlers] bit NOT NULL,
        [ShowCompanyToHandlers] bit NOT NULL,
        [ShowBranchToHandlers] bit NOT NULL,
        [ShowDepartmentToHandlers] bit NOT NULL,
        [ShowSectionToHandlers] bit NOT NULL,
        [ShowJobTitleToHandlers] bit NOT NULL,
        [ShowManagerDetailsToHandlers] bit NOT NULL,
        [ShowDateOfJoiningToHandlers] bit NOT NULL,
        [ShowPreviousComplaintsToHandlers] bit NOT NULL,
        [ShowEmployeeAddressToManagement] bit NOT NULL,
        [ShowEmergencyContactToManagement] bit NOT NULL,
        [ShowPerformanceMetricsToManagement] bit NOT NULL,
        [MaskPersonalInfoInLogs] bit NOT NULL,
        [RedactInfoAfterClosure] bit NOT NULL,
        [DataRetentionDays] int NOT NULL,
        [IncludeEmployeeCodeInReports] bit NOT NULL,
        [IncludeEmailInReports] bit NOT NULL,
        [IncludePhoneInReports] bit NOT NULL,
        [MaskEmailInReports] bit NOT NULL,
        [MaskPhoneInReports] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintInformationSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintInformationSettings_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintPriorityMasters] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [Level] int NOT NULL,
        [ColorCode] nvarchar(50) NULL,
        [IconClass] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintPriorityMasters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintPriorityMasters_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintStatusMasters] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [ColorCode] nvarchar(50) NULL,
        [IconClass] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [IsFinal] bit NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintStatusMasters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintStatusMasters_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomFieldDefinition] (
        [Id] uniqueidentifier NOT NULL,
        [FieldName] nvarchar(max) NOT NULL,
        [FieldKey] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [FieldType] int NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsRequired] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [IsSearchable] bit NOT NULL,
        [DefaultValue] nvarchar(max) NULL,
        [ValidationRules] nvarchar(max) NULL,
        [Options] nvarchar(max) NULL,
        [Placeholder] nvarchar(max) NULL,
        [HelpText] nvarchar(max) NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [Section] nvarchar(max) NULL,
        [IconClass] nvarchar(max) NULL,
        [IsVisibleToComplainant] bit NOT NULL,
        [IsVisibleToHandler] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CustomFieldDefinition] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomFieldDefinition_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailConfigurations] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [AuthenticationType] int NOT NULL,
        [ImapHost] nvarchar(max) NOT NULL,
        [ImapPort] int NOT NULL,
        [ImapUseSsl] bit NOT NULL,
        [ImapUsername] nvarchar(max) NOT NULL,
        [ImapPassword] nvarchar(max) NOT NULL,
        [ImapFolder] nvarchar(max) NOT NULL,
        [SmtpHost] nvarchar(max) NOT NULL,
        [SmtpPort] int NOT NULL,
        [SmtpUseSsl] bit NOT NULL,
        [SmtpUsername] nvarchar(max) NOT NULL,
        [SmtpPassword] nvarchar(max) NOT NULL,
        [FromEmail] nvarchar(max) NOT NULL,
        [FromName] nvarchar(max) NOT NULL,
        [OAuthClientId] nvarchar(max) NULL,
        [OAuthClientSecret] nvarchar(max) NULL,
        [OAuthTenantId] nvarchar(max) NULL,
        [OAuthAccessToken] nvarchar(max) NULL,
        [OAuthRefreshToken] nvarchar(max) NULL,
        [OAuthTokenExpiresAt] datetime2 NULL,
        [OAuthScopes] nvarchar(max) NULL,
        [OAuthTokenRefreshIntervalMinutes] int NULL,
        [PollingIntervalMinutes] int NOT NULL,
        [PollingIntervalSeconds] int NULL,
        [IsEnabled] bit NOT NULL,
        [LastPolledAt] datetime2 NULL,
        [SendAutoAcknowledgement] bit NOT NULL,
        [AutoAcknowledgementTemplateId] nvarchar(max) NULL,
        [AutoAcknowledgementTemplateId1] uniqueidentifier NULL,
        [EnableThreading] bit NOT NULL,
        [ThreadTimeoutDays] int NOT NULL,
        [MaxAttachmentSizeBytes] bigint NOT NULL,
        [AllowedAttachmentExtensions] nvarchar(max) NOT NULL,
        [UseSeparateSmtpAccount] bit NOT NULL,
        [SmtpAuthenticationType] int NULL,
        [SmtpSeparateUsername] nvarchar(max) NULL,
        [SmtpSeparatePassword] nvarchar(max) NULL,
        [SmtpSeparateFromEmail] nvarchar(max) NULL,
        [SmtpSeparateFromName] nvarchar(max) NULL,
        [SmtpSeparateOAuthClientId] nvarchar(max) NULL,
        [SmtpSeparateOAuthClientSecret] nvarchar(max) NULL,
        [SmtpSeparateOAuthTenantId] nvarchar(max) NULL,
        [SmtpSeparateOAuthAccessToken] nvarchar(max) NULL,
        [SmtpSeparateOAuthRefreshToken] nvarchar(max) NULL,
        [SmtpSeparateOAuthTokenExpiresAt] datetime2 NULL,
        [SmtpSeparateOAuthScopes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailConfigurations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailConfigurations_CommunicationTemplates_AutoAcknowledgementTemplateId1] FOREIGN KEY ([AutoAcknowledgementTemplateId1]) REFERENCES [CommunicationTemplates] ([Id]),
        CONSTRAINT [FK_EmailConfigurations_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailServerSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [AuthenticationType] int NOT NULL,
        [Host] nvarchar(255) NOT NULL,
        [Port] int NOT NULL,
        [UseSsl] bit NOT NULL,
        [Username] nvarchar(255) NULL,
        [Password] nvarchar(500) NULL,
        [FromEmail] nvarchar(255) NOT NULL,
        [FromName] nvarchar(200) NULL,
        [ReplyToEmail] nvarchar(255) NULL,
        [OAuthClientId] nvarchar(max) NULL,
        [OAuthClientSecret] nvarchar(max) NULL,
        [OAuthTenantId] nvarchar(max) NULL,
        [OAuthAccessToken] nvarchar(max) NULL,
        [OAuthRefreshToken] nvarchar(max) NULL,
        [OAuthTokenExpiresAt] datetime2 NULL,
        [OAuthScopes] nvarchar(max) NULL,
        [OAuthTokenRefreshIntervalMinutes] int NULL,
        [IsActive] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [MaxEmailsPerHour] int NULL,
        [TimeoutSeconds] int NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [TestNotes] nvarchar(1000) NULL,
        [LastTestedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailServerSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailServerSettings_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmployeeTypes] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [OryggiEmployeeTypeId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmployeeTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmployeeTypes_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EventTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [Category] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [AvailableFields] nvarchar(max) NULL,
        [IconClass] nvarchar(100) NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EventTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EventTypes_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordPolicy] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [MinimumLength] int NOT NULL DEFAULT 8,
        [RequireUppercase] bit NOT NULL DEFAULT CAST(1 AS bit),
        [RequireLowercase] bit NOT NULL DEFAULT CAST(1 AS bit),
        [RequireDigit] bit NOT NULL DEFAULT CAST(1 AS bit),
        [RequireSpecialCharacter] bit NOT NULL DEFAULT CAST(1 AS bit),
        [PasswordExpirationDays] int NOT NULL DEFAULT 90,
        [PasswordExpirationWarningDays] int NOT NULL DEFAULT 7,
        [MaxFailedLoginAttempts] int NOT NULL DEFAULT 5,
        [AccountLockoutDurationMinutes] int NOT NULL DEFAULT 15,
        [PasswordHistoryCount] int NOT NULL DEFAULT 5,
        [MinimumPasswordAgeDays] int NOT NULL DEFAULT 0,
        [EnablePasswordComplexity] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AllowSkipPasswordChange] bit NOT NULL DEFAULT CAST(0 AS bit),
        [SendPasswordExpirationEmails] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SendPasswordSetEmails] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [CreatedBy] uniqueidentifier NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PasswordPolicy] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PasswordPolicy_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [SmsGatewaySettings] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Provider] nvarchar(100) NOT NULL,
        [ApiUrl] nvarchar(500) NULL,
        [AccountSid] nvarchar(255) NULL,
        [AuthToken] nvarchar(500) NULL,
        [FromNumber] nvarchar(50) NULL,
        [SenderName] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [MaxSmsPerHour] int NULL,
        [CostPerSms] decimal(10,4) NULL,
        [TimeoutSeconds] int NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [AdditionalConfig] nvarchar(max) NULL,
        [TestNotes] nvarchar(1000) NULL,
        [LastTestedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_SmsGatewaySettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SmsGatewaySettings_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [WhatsAppSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Provider] nvarchar(100) NOT NULL,
        [ApiUrl] nvarchar(500) NULL,
        [BusinessAccountId] nvarchar(255) NULL,
        [PhoneNumberId] nvarchar(255) NULL,
        [AccessToken] nvarchar(1000) NULL,
        [WebhookToken] nvarchar(500) NULL,
        [FromNumber] nvarchar(50) NULL,
        [BusinessName] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [MaxMessagesPerHour] int NULL,
        [TimeoutSeconds] int NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [MediaStorageType] nvarchar(max) NULL,
        [MediaStoragePath] nvarchar(max) NULL,
        [MediaPublicBaseUrl] nvarchar(max) NULL,
        [MediaStorageConfig] nvarchar(max) NULL,
        [MaxMediaSizeMB] int NULL,
        [MediaRetentionDays] int NULL,
        [AdditionalConfig] nvarchar(max) NULL,
        [TestNotes] nvarchar(1000) NULL,
        [LastTestedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_WhatsAppSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WhatsAppSettings_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [PrioritySLAs] (
        [Id] uniqueidentifier NOT NULL,
        [PriorityId] uniqueidentifier NOT NULL,
        [SLALevelId] uniqueidentifier NOT NULL,
        [OverrideResponseTime] int NULL,
        [OverrideResolutionTime] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PrioritySLAs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrioritySLAs_ComplaintPriorityMasters_PriorityId] FOREIGN KEY ([PriorityId]) REFERENCES [ComplaintPriorityMasters] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PrioritySLAs_SLALevels_SLALevelId] FOREIGN KEY ([SLALevelId]) REFERENCES [SLALevels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EventCommunicationRules] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EventTypeId] uniqueidentifier NOT NULL,
        [Channel] int NOT NULL,
        [TemplateId] uniqueidentifier NULL,
        [RecipientType] int NOT NULL,
        [SpecificUserIds] nvarchar(max) NULL,
        [SpecificRoleIds] nvarchar(max) NULL,
        [SpecificEmails] nvarchar(max) NULL,
        [Conditions] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [Priority] int NOT NULL,
        [DelayMinutes] int NOT NULL,
        [SendOnlyOnce] bit NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [AdditionalData] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EventCommunicationRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EventCommunicationRules_CommunicationTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [CommunicationTemplates] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_EventCommunicationRules_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EventCommunicationRules_EventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [EventTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [StoredFileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(1000) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [FileExtension] nvarchar(20) NOT NULL,
        [UploadedBy] uniqueidentifier NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsThumbnail] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintAttachments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintComments] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintId] uniqueidentifier NOT NULL,
        [CommentedBy] uniqueidentifier NOT NULL,
        [CommentText] nvarchar(4000) NOT NULL,
        [IsInternal] bit NOT NULL,
        [ParentCommentId] uniqueidentifier NULL,
        [CommentedAt] datetime2 NOT NULL,
        [IsEdited] bit NOT NULL,
        [EditedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplaintComments_ComplaintComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [ComplaintComments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ComplaintEmailParticipants] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintId] uniqueidentifier NOT NULL,
        [EmailAddress] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [ParticipantType] nvarchar(max) NOT NULL,
        [AddedBy] uniqueidentifier NULL,
        [AddedByUserId] uniqueidentifier NULL,
        [AddedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ComplaintEmailParticipants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Complaints] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintNumber] nvarchar(50) NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [ComplainantId] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [SectionId] uniqueidentifier NULL,
        [EmployeeCode] nvarchar(max) NULL,
        [ContactEmail] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [AlternatePhone] nvarchar(max) NULL,
        [PreferredContactMethod] int NOT NULL,
        [StatusMasterId] uniqueidentifier NOT NULL,
        [PriorityMasterId] uniqueidentifier NOT NULL,
        [CurrentEscalationLevel] int NOT NULL,
        [AssignedToId] uniqueidentifier NULL,
        [ResourcePoolId] uniqueidentifier NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [DueDate] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        [ClosedAt] datetime2 NULL,
        [ResolutionNotes] nvarchar(4000) NULL,
        [IsAnonymous] bit NOT NULL,
        [Tags] nvarchar(500) NULL,
        [RelatedComplaintId] uniqueidentifier NULL,
        [HasCustomerResponse] bit NOT NULL,
        [LastResponseFrom] nvarchar(max) NULL,
        [LastResponseAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Complaints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Complaints_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Complaints_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Complaints_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Complaints_ComplaintPriorityMasters_PriorityMasterId] FOREIGN KEY ([PriorityMasterId]) REFERENCES [ComplaintPriorityMasters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Complaints_ComplaintStatusMasters_StatusMasterId] FOREIGN KEY ([StatusMasterId]) REFERENCES [ComplaintStatusMasters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Complaints_Complaints_RelatedComplaintId] FOREIGN KEY ([RelatedComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomFieldValue] (
        [Id] uniqueidentifier NOT NULL,
        [CustomFieldDefinitionId] uniqueidentifier NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NULL,
        [NumericValue] decimal(18,2) NULL,
        [DateValue] datetime2 NULL,
        [BooleanValue] bit NULL,
        [JsonValue] nvarchar(max) NULL,
        [ComplaintId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_CustomFieldValue] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomFieldValue_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]),
        CONSTRAINT [FK_CustomFieldValue_CustomFieldDefinition_CustomFieldDefinitionId] FOREIGN KEY ([CustomFieldDefinitionId]) REFERENCES [CustomFieldDefinition] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [DashboardPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [StatusWidgets] nvarchar(max) NOT NULL,
        [Layout] nvarchar(max) NOT NULL,
        [ShowTrends] bit NOT NULL,
        [ShowPercentages] bit NOT NULL,
        [AutoRefreshInterval] int NOT NULL,
        [DateRangeDays] int NOT NULL,
        [Theme] nvarchar(max) NULL,
        [WidgetConfig] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_DashboardPreferences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ManagerId] uniqueidentifier NULL,
        [SecondaryManagerId] uniqueidentifier NULL,
        [HrResponsibleId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [OryggiDepartmentId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Departments_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EscalationMatrices] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Priority] int NOT NULL DEFAULT 0,
        [EnableAutoEscalation] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SendEmailNotifications] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EscalationMatrices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EscalationMatrices_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_EscalationMatrices_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscalationMatrices_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_EscalationMatrices_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [EmailMessageId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [FileExtension] nvarchar(max) NOT NULL,
        [StoragePath] nvarchar(max) NOT NULL,
        [StorageUrl] nvarchar(max) NULL,
        [ContentId] nvarchar(max) NULL,
        [IsInline] bit NOT NULL,
        [IsScanned] bit NOT NULL,
        [IsSafe] bit NOT NULL,
        [ScanResult] nvarchar(max) NULL,
        [ChecksumMd5] nvarchar(max) NULL,
        [UploadedAt] datetime2 NOT NULL,
        [DeletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailAttachments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailMessages] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [MessageId] nvarchar(max) NOT NULL,
        [InReplyTo] nvarchar(max) NULL,
        [References] nvarchar(max) NULL,
        [Subject] nvarchar(max) NOT NULL,
        [FromEmail] nvarchar(max) NOT NULL,
        [FromName] nvarchar(max) NOT NULL,
        [ToEmail] nvarchar(max) NOT NULL,
        [ToName] nvarchar(max) NULL,
        [CcEmails] nvarchar(max) NULL,
        [BccEmails] nvarchar(max) NULL,
        [TextBody] nvarchar(max) NOT NULL,
        [HtmlBody] nvarchar(max) NULL,
        [IsHtml] bit NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NOT NULL,
        [SentAt] datetime2 NULL,
        [ComplaintId] uniqueidentifier NULL,
        [SentByUserId] uniqueidentifier NULL,
        [ThreadId] uniqueidentifier NULL,
        [ThreadPosition] int NOT NULL,
        [IsAutoAcknowledgement] bit NOT NULL,
        [IsInternal] bit NOT NULL,
        [IsRead] bit NOT NULL,
        [IsSpam] bit NOT NULL,
        [Failed] bit NOT NULL,
        [FailureReason] nvarchar(max) NULL,
        [ReadBy] uniqueidentifier NULL,
        [ReadByUserId] uniqueidentifier NULL,
        [ReadAt] datetime2 NULL,
        [ToRecipientsJson] nvarchar(max) NULL,
        [CcRecipientsJson] nvarchar(max) NULL,
        [BccRecipientsJson] nvarchar(max) NULL,
        [RawHeaders] nvarchar(max) NULL,
        [RawBody] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailMessages_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EmailMessages_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailResponseHistories] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintId] uniqueidentifier NOT NULL,
        [EmailMessageId] uniqueidentifier NULL,
        [SentBy] uniqueidentifier NOT NULL,
        [SentTo] nvarchar(1000) NOT NULL,
        [CarbonCopy] nvarchar(1000) NULL,
        [BlindCarbonCopy] nvarchar(1000) NULL,
        [Subject] nvarchar(500) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [IsHtml] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SentAt] datetime2 NOT NULL,
        [DeliveryStatus] nvarchar(50) NULL DEFAULT N'Sent',
        [ErrorMessage] nvarchar(2000) NULL,
        [MessageId] nvarchar(500) NULL,
        [HasAttachments] bit NOT NULL,
        [AttachmentIds] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EmailResponseHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailResponseHistories_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EmailResponseHistories_EmailMessages_EmailMessageId] FOREIGN KEY ([EmailMessageId]) REFERENCES [EmailMessages] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] uniqueidentifier NOT NULL,
        [TenantId] uniqueidentifier NOT NULL,
        [SectionId] uniqueidentifier NULL,
        [EmployeeCode] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [AlternatePhone] nvarchar(max) NULL,
        [DateOfJoining] datetime2 NULL,
        [DateOfBirth] datetime2 NULL,
        [ManagerId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [OryggiEmployeeId] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Employees_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Employees] ([Id]),
        CONSTRAINT [FK_Employees_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EscalationHistories] (
        [Id] uniqueidentifier NOT NULL,
        [ComplaintId] uniqueidentifier NOT NULL,
        [EscalationLevelId] uniqueidentifier NOT NULL,
        [EscalationMatrixId] uniqueidentifier NOT NULL,
        [Level] int NOT NULL,
        [FromUserId] uniqueidentifier NULL,
        [ToUserId] uniqueidentifier NOT NULL,
        [EscalatedBy] uniqueidentifier NULL,
        [EscalatedAt] datetime2 NOT NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [IsAutoEscalation] bit NOT NULL DEFAULT CAST(0 AS bit),
        [AssignmentStrategy] nvarchar(50) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [AcknowledgedAt] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        [SlaHoursAtEscalation] int NULL,
        [HoursOverdue] int NULL,
        [EmailSent] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EmailSentAt] datetime2 NULL,
        [Notes] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EscalationHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EscalationHistories_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EscalationHistories_EscalationMatrices_EscalationMatrixId] FOREIGN KEY ([EscalationMatrixId]) REFERENCES [EscalationMatrices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EscalationLevels] (
        [Id] uniqueidentifier NOT NULL,
        [EscalationMatrixId] uniqueidentifier NOT NULL,
        [Level] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [TriggerAfterHours] int NOT NULL DEFAULT 0,
        [TriggerAfterValue] int NOT NULL,
        [TriggerTimeUnit] int NOT NULL,
        [AssignmentStrategy] nvarchar(50) NOT NULL,
        [AssignToUserId] uniqueidentifier NULL,
        [AssignToRole] nvarchar(50) NULL,
        [AssignToUserIds] nvarchar(500) NULL,
        [PrimaryContactId] uniqueidentifier NULL,
        [SecondaryContactId] uniqueidentifier NULL,
        [HrContactId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [ResourcePoolId] uniqueidentifier NULL,
        [ResourcePoolAssignmentMethod] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SendNotification] bit NOT NULL DEFAULT CAST(1 AS bit),
        [EmailTemplateId] uniqueidentifier NULL,
        [NotifyPreviousHandler] bit NOT NULL DEFAULT CAST(1 AS bit),
        [EscalationMessage] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EscalationLevels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EscalationLevels_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_EscalationLevels_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]),
        CONSTRAINT [FK_EscalationLevels_EscalationMatrices_EscalationMatrixId] FOREIGN KEY ([EscalationMatrixId]) REFERENCES [EscalationMatrices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [EscalationPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [SectionId] uniqueidentifier NULL,
        [CategoryId] uniqueidentifier NULL,
        [EnableAutoEscalation] bit NOT NULL,
        [RequireManualApproval] bit NOT NULL,
        [DefaultEscalationMatrixId] uniqueidentifier NULL,
        [MinimumSeverityForAutoEscalation] int NULL,
        [MaxAutoEscalationLevels] int NULL,
        [Priority] int NOT NULL,
        [IsActive] bit NOT NULL,
        [EffectiveFrom] datetime2 NULL,
        [EffectiveTo] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_EscalationPolicies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EscalationPolicies_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscalationPolicies_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscalationPolicies_ComplaintCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ComplaintCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscalationPolicies_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EscalationPolicies_EscalationMatrices_DefaultEscalationMatrixId] FOREIGN KEY ([DefaultEscalationMatrixId]) REFERENCES [EscalationMatrices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ExternalUserMappings] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [AuthenticationProviderId] uniqueidentifier NOT NULL,
        [ExternalUserId] nvarchar(500) NOT NULL,
        [ExternalUsername] nvarchar(200) NULL,
        [ExternalEmail] nvarchar(200) NULL,
        [ExternalDisplayName] nvarchar(200) NULL,
        [Attributes] nvarchar(max) NULL,
        [ExternalGroups] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [LastSyncedAt] datetime2 NULL,
        [LastSyncSuccess] bit NULL,
        [LastSyncDetails] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [LastLoginAt] datetime2 NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ExternalUserMappings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExternalUserMappings_AuthenticationProviders_AuthenticationProviderId] FOREIGN KEY ([AuthenticationProviderId]) REFERENCES [AuthenticationProviders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordAuditLog] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [PerformedBy] uniqueidentifier NULL,
        [Success] bit NOT NULL,
        [Details] nvarchar(1000) NULL,
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PasswordAuditLog] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordHistory] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [CreatedBy] uniqueidentifier NULL,
        [IpAddress] nvarchar(45) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PasswordHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordResetTokens] (
        [Id] uniqueidentifier NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL,
        [UsedAt] datetime2 NULL,
        [RequestIpAddress] nvarchar(max) NULL,
        [ResetIpAddress] nvarchar(max) NULL,
        [RequestUserAgent] nvarchar(max) NULL,
        [ResetUserAgent] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [Token] nvarchar(500) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        [RevokedAt] datetime2 NULL,
        [ReplacedByTokenId] uniqueidentifier NULL,
        [RevocationReason] nvarchar(200) NULL,
        [CreatedByIp] nvarchar(50) NULL,
        [RevokedByIp] nvarchar(50) NULL,
        [TokenFamily] uniqueidentifier NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ResourcePoolMembers] (
        [Id] uniqueidentifier NOT NULL,
        [ResourcePoolId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [AddedAt] datetime2 NOT NULL,
        [AddedBy] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ResourcePoolMembers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [ResourcePools] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [PoolType] int NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [SectionId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ResourcePools] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ResourcePools_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_ResourcePools_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ResourcePools_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Sections] (
        [Id] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [HeadId] uniqueidentifier NULL,
        [SecondaryHeadId] uniqueidentifier NULL,
        [HrResponsibleId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [OryggiSectionId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [SectionId] uniqueidentifier NULL,
        [EmployeeTypeId] uniqueidentifier NULL,
        [EmployeeCode] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [Phone] nvarchar(50) NULL,
        [AlternatePhone] nvarchar(max) NULL,
        [JobTitle] nvarchar(200) NULL,
        [DateOfJoining] datetime2 NULL,
        [DateOfBirth] datetime2 NULL,
        [ManagerId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [PasswordHash] nvarchar(500) NULL,
        [LastLoginAt] datetime2 NULL,
        [OryggiEmployeeId] nvarchar(100) NULL,
        [LastSyncedAt] datetime2 NULL,
        [PasswordExpiresAt] datetime2 NULL,
        [MustChangePasswordOnNextLogin] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PasswordNeverExpires] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PasswordChangedAt] datetime2 NULL,
        [PasswordChangedBy] uniqueidentifier NULL,
        [FailedLoginAttempts] int NOT NULL DEFAULT 0,
        [AccountLockedUntil] datetime2 NULL,
        [LastPasswordChangeRequiredNotificationSentAt] datetime2 NULL,
        [AuthenticationProviderType] nvarchar(50) NOT NULL DEFAULT N'Local',
        [ExternalUserId] nvarchar(500) NULL,
        [ExternalUsername] nvarchar(200) NULL,
        [IdentityProvider] nvarchar(100) NULL,
        [LastExternalSyncAt] datetime2 NULL,
        [ExternalSyncEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SSOEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [LocalPasswordEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [PreferredAuthMethod] nvarchar(50) NULL,
        [PreferredTimeZone] nvarchar(max) NULL,
        [PreferredLocale] nvarchar(max) NULL,
        [PreferredDateFormat] nvarchar(max) NULL,
        [PreferredTimeFormat] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_EmployeeTypes_EmployeeTypeId] FOREIGN KEY ([EmployeeTypeId]) REFERENCES [EmployeeTypes] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE TABLE [UserComplaintRoles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ComplaintRoleId] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [DepartmentId] uniqueidentifier NULL,
        [SectionId] uniqueidentifier NULL,
        [EffectiveFrom] datetime2 NOT NULL,
        [EffectiveTo] datetime2 NULL,
        [IsPrimary] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_UserComplaintRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserComplaintRoles_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_UserComplaintRoles_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserComplaintRoles_ComplaintRoles_ComplaintRoleId] FOREIGN KEY ([ComplaintRoleId]) REFERENCES [ComplaintRoles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserComplaintRoles_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_UserComplaintRoles_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_UserComplaintRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'ColorCode', N'CompanyId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IconClass', N'IsActive', N'IsDeleted', N'IsSystem', N'Level', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[ComplaintPriorityMasters]'))
        SET IDENTITY_INSERT [ComplaintPriorityMasters] ON;
    EXEC(N'INSERT INTO [ComplaintPriorityMasters] ([Id], [Code], [ColorCode], [CompanyId], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IconClass], [IsActive], [IsDeleted], [IsSystem], [Level], [Name], [UpdatedAt], [UpdatedBy])
    VALUES (''20000000-0000-0000-0000-000000000001'', N''LOW'', N''#4CAF50'', NULL, ''2025-12-28T19:44:11.6578837Z'', NULL, NULL, NULL, N''Low priority - No immediate action required'', 1, N''bi-arrow-down-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, N''Low'', ''2025-12-28T19:44:11.6579122Z'', NULL),
    (''20000000-0000-0000-0000-000000000002'', N''NORMAL'', N''#2196F3'', NULL, ''2025-12-28T19:44:11.6579480Z'', NULL, NULL, NULL, N''Normal priority - Standard processing time'', 2, N''bi-dash-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 3, N''Normal'', ''2025-12-28T19:44:11.6579481Z'', NULL),
    (''20000000-0000-0000-0000-000000000003'', N''HIGH'', N''#FF9800'', NULL, ''2025-12-28T19:44:11.6579486Z'', NULL, NULL, NULL, N''High priority - Requires expedited attention'', 3, N''bi-exclamation-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 5, N''High'', ''2025-12-28T19:44:11.6579486Z'', NULL),
    (''20000000-0000-0000-0000-000000000004'', N''CRITICAL'', N''#F44336'', NULL, ''2025-12-28T19:44:11.6579491Z'', NULL, NULL, NULL, N''Critical priority - Requires immediate attention'', 4, N''bi-exclamation-triangle'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 8, N''Critical'', ''2025-12-28T19:44:11.6579491Z'', NULL),
    (''20000000-0000-0000-0000-000000000005'', N''URGENT'', N''#9C27B0'', NULL, ''2025-12-28T19:44:11.6579495Z'', NULL, NULL, NULL, N''Urgent priority - Highest priority level'', 5, N''bi-lightning'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 10, N''Urgent'', ''2025-12-28T19:44:11.6579496Z'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'ColorCode', N'CompanyId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IconClass', N'IsActive', N'IsDeleted', N'IsSystem', N'Level', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[ComplaintPriorityMasters]'))
        SET IDENTITY_INSERT [ComplaintPriorityMasters] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'ColorCode', N'CompanyId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IconClass', N'IsActive', N'IsDeleted', N'IsFinal', N'IsSystem', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[ComplaintStatusMasters]'))
        SET IDENTITY_INSERT [ComplaintStatusMasters] ON;
    EXEC(N'INSERT INTO [ComplaintStatusMasters] ([Id], [Code], [ColorCode], [CompanyId], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IconClass], [IsActive], [IsDeleted], [IsFinal], [IsSystem], [Name], [UpdatedAt], [UpdatedBy])
    VALUES (''10000000-0000-0000-0000-000000000001'', N''SUBMITTED'', N''#9E9E9E'', NULL, ''2025-12-28T19:44:11.6620681Z'', NULL, NULL, NULL, N''Complaint has been submitted but not yet reviewed'', 1, N''bi-inbox'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Submitted'', ''2025-12-28T19:44:11.6620687Z'', NULL),
    (''10000000-0000-0000-0000-000000000002'', N''UNDER_REVIEW'', N''#2196F3'', NULL, ''2025-12-28T19:44:11.6620715Z'', NULL, NULL, NULL, N''Complaint is being reviewed by the assigned handler'', 2, N''bi-eye'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Under Review'', ''2025-12-28T19:44:11.6620716Z'', NULL),
    (''10000000-0000-0000-0000-000000000003'', N''IN_PROGRESS'', N''#FF9800'', NULL, ''2025-12-28T19:44:11.6620719Z'', NULL, NULL, NULL, N''Complaint is currently being investigated'', 3, N''bi-gear'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''In Progress'', ''2025-12-28T19:44:11.6620719Z'', NULL),
    (''10000000-0000-0000-0000-000000000004'', N''ESCALATED'', N''#FF5722'', NULL, ''2025-12-28T19:44:11.6620722Z'', NULL, NULL, NULL, N''Complaint has been escalated to a higher level'', 4, N''bi-arrow-up-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Escalated'', ''2025-12-28T19:44:11.6620722Z'', NULL),
    (''10000000-0000-0000-0000-000000000005'', N''PENDING_INFO'', N''#FFC107'', NULL, ''2025-12-28T19:44:11.6620725Z'', NULL, NULL, NULL, N''Complaint is awaiting information from the complainant'', 5, N''bi-question-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Pending Info'', ''2025-12-28T19:44:11.6620725Z'', NULL),
    (''10000000-0000-0000-0000-000000000006'', N''RESOLVED'', N''#4CAF50'', NULL, ''2025-12-28T19:44:11.6620729Z'', NULL, NULL, NULL, N''Complaint has been resolved'', 6, N''bi-check-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Resolved'', ''2025-12-28T19:44:11.6620729Z'', NULL),
    (''10000000-0000-0000-0000-000000000007'', N''CLOSED'', N''#607D8B'', NULL, ''2025-12-28T19:44:11.6620732Z'', NULL, NULL, NULL, N''Complaint has been closed (final state)'', 7, N''bi-lock'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), N''Closed'', ''2025-12-28T19:44:11.6620732Z'', NULL),
    (''10000000-0000-0000-0000-000000000008'', N''REJECTED'', N''#F44336'', NULL, ''2025-12-28T19:44:11.6620735Z'', NULL, NULL, NULL, N''Complaint has been rejected/dismissed'', 8, N''bi-x-circle'', CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(1 AS bit), N''Rejected'', ''2025-12-28T19:44:11.6620735Z'', NULL),
    (''10000000-0000-0000-0000-000000000009'', N''REOPENED'', N''#E91E63'', NULL, ''2025-12-28T19:44:11.6620738Z'', NULL, NULL, NULL, N''Complaint has been reopened after closure'', 9, N''bi-arrow-repeat'', CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Reopened'', ''2025-12-28T19:44:11.6620738Z'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'ColorCode', N'CompanyId', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'DeletedBy', N'Description', N'DisplayOrder', N'IconClass', N'IsActive', N'IsDeleted', N'IsFinal', N'IsSystem', N'Name', N'UpdatedAt', N'UpdatedBy') AND [object_id] = OBJECT_ID(N'[ComplaintStatusMasters]'))
        SET IDENTITY_INSERT [ComplaintStatusMasters] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_AutoAssignRoleId] ON [AuthenticationProviders] ([AutoAssignRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_CompanyId] ON [AuthenticationProviders] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_CompanyId_IsDefault] ON [AuthenticationProviders] ([CompanyId], [IsDefault]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_CompanyId_IsEnabled] ON [AuthenticationProviders] ([CompanyId], [IsEnabled]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_EmailDomain] ON [AuthenticationProviders] ([EmailDomain]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_IsDefault] ON [AuthenticationProviders] ([IsDefault]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_IsEnabled] ON [AuthenticationProviders] ([IsEnabled]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuthenticationProviders_ProviderType] ON [AuthenticationProviders] ([ProviderType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_CompanyId_Code] ON [Branches] ([CompanyId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_HrResponsibleId] ON [Branches] ([HrResponsibleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_ManagerId] ON [Branches] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_OryggiBranchId] ON [Branches] ([OryggiBranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_SecondaryManagerId] ON [Branches] ([SecondaryManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CannedResponses_CategoryId] ON [CannedResponses] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CannedResponses_CompanyId] ON [CannedResponses] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CannedResponses_CreatedByUserId] ON [CannedResponses] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CategorySLAs_CategoryId] ON [CategorySLAs] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CategorySLAs_CategoryId_SLALevelId] ON [CategorySLAs] ([CategoryId], [SLALevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CategorySLAs_IsActive] ON [CategorySLAs] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CategorySLAs_SLALevelId] ON [CategorySLAs] ([SLALevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflows_CategoryId] ON [CategoryWorkflows] ([CategoryId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflows_CategoryId_IsDefault] ON [CategoryWorkflows] ([CategoryId], [IsDefault]) WHERE [IsDeleted] = 0 AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflows_CompanyId] ON [CategoryWorkflows] ([CompanyId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowStatuses_StatusMasterId] ON [CategoryWorkflowStatuses] ([StatusMasterId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowStatuses_WorkflowId] ON [CategoryWorkflowStatuses] ([WorkflowId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowStatuses_WorkflowId_DisplayOrder] ON [CategoryWorkflowStatuses] ([WorkflowId], [DisplayOrder]) WHERE [IsDeleted] = 0 AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CategoryWorkflowStatuses_WorkflowId_StatusMasterId] ON [CategoryWorkflowStatuses] ([WorkflowId], [StatusMasterId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowTransitions_FromStatusId] ON [CategoryWorkflowTransitions] ([FromStatusId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowTransitions_ToStatusId] ON [CategoryWorkflowTransitions] ([ToStatusId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowTransitions_WorkflowId] ON [CategoryWorkflowTransitions] ([WorkflowId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId] ON [CategoryWorkflowTransitions] ([WorkflowId], [FromStatusId]) WHERE [IsDeleted] = 0 AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId_ToStatusId] ON [CategoryWorkflowTransitions] ([WorkflowId], [FromStatusId], [ToStatusId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_CompanyId] ON [CommunicationLogs] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_EntityId_EntityType] ON [CommunicationLogs] ([EntityId], [EntityType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_RecipientUserId] ON [CommunicationLogs] ([RecipientUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_SentAt] ON [CommunicationLogs] ([SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_Status_CreatedAt] ON [CommunicationLogs] ([Status], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationLogs_TemplateId] ON [CommunicationLogs] ([TemplateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationTemplates_Category] ON [CommunicationTemplates] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationTemplates_Channel_IsActive] ON [CommunicationTemplates] ([Channel], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CommunicationTemplates_Code] ON [CommunicationTemplates] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommunicationTemplates_CompanyId] ON [CommunicationTemplates] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_HrResponsibleId] ON [Companies] ([HrResponsibleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_ManagerId] ON [Companies] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_OryggiCompanyId] ON [Companies] ([OryggiCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Companies_SecondaryManagerId] ON [Companies] ([SecondaryManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_TenantId_Code] ON [Companies] ([TenantId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintAttachments_ComplaintId] ON [ComplaintAttachments] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintAttachments_UploadedAt] ON [ComplaintAttachments] ([UploadedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintAttachments_UploadedBy] ON [ComplaintAttachments] ([UploadedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComplaintCategories_Code] ON [ComplaintCategories] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintCategories_DisplayOrder] ON [ComplaintCategories] ([DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintCategories_ParentCategoryId] ON [ComplaintCategories] ([ParentCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintComments_CommentedAt] ON [ComplaintComments] ([CommentedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintComments_CommentedBy] ON [ComplaintComments] ([CommentedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintComments_ComplaintId] ON [ComplaintComments] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintComments_IsInternal] ON [ComplaintComments] ([IsInternal]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintComments_ParentCommentId] ON [ComplaintComments] ([ParentCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintEmailParticipants_AddedByUserId] ON [ComplaintEmailParticipants] ([AddedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintEmailParticipants_ComplaintId] ON [ComplaintEmailParticipants] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintInformationSettings_CompanyId] ON [ComplaintInformationSettings] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComplaintPriorityMasters_Code] ON [ComplaintPriorityMasters] ([Code]) WHERE [CompanyId] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComplaintPriorityMasters_Code_CompanyId] ON [ComplaintPriorityMasters] ([Code], [CompanyId]) WHERE [CompanyId] IS NOT NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintPriorityMasters_CompanyId] ON [ComplaintPriorityMasters] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintRolePermissions_ComplaintRoleId] ON [ComplaintRolePermissions] ([ComplaintRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComplaintRolePermissions_ComplaintRoleId_PermissionType] ON [ComplaintRolePermissions] ([ComplaintRoleId], [PermissionType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintRolePermissions_PermissionType] ON [ComplaintRolePermissions] ([PermissionType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComplaintRoles_Code] ON [ComplaintRoles] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintRoles_EscalationLevel] ON [ComplaintRoles] ([EscalationLevel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintRoles_RoleType] ON [ComplaintRoles] ([RoleType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_AssignedToId] ON [Complaints] ([AssignedToId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_BranchId] ON [Complaints] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_CategoryId] ON [Complaints] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_CompanyId_StatusMasterId] ON [Complaints] ([CompanyId], [StatusMasterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_ComplainantId] ON [Complaints] ([ComplainantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Complaints_ComplaintNumber] ON [Complaints] ([ComplaintNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_CurrentEscalationLevel] ON [Complaints] ([CurrentEscalationLevel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_DepartmentId] ON [Complaints] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_DueDate] ON [Complaints] ([DueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_PriorityMasterId] ON [Complaints] ([PriorityMasterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_RelatedComplaintId] ON [Complaints] ([RelatedComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_ResourcePoolId] ON [Complaints] ([ResourcePoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_SectionId] ON [Complaints] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_StatusMasterId] ON [Complaints] ([StatusMasterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Complaints_SubmittedAt] ON [Complaints] ([SubmittedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComplaintStatusMasters_Code] ON [ComplaintStatusMasters] ([Code]) WHERE [CompanyId] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ComplaintStatusMasters_Code_CompanyId] ON [ComplaintStatusMasters] ([Code], [CompanyId]) WHERE [CompanyId] IS NOT NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ComplaintStatusMasters_CompanyId] ON [ComplaintStatusMasters] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomFieldDefinition_CompanyId] ON [CustomFieldDefinition] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomFieldValue_ComplaintId] ON [CustomFieldValue] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomFieldValue_CustomFieldDefinitionId] ON [CustomFieldValue] ([CustomFieldDefinitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DashboardPreferences_UserId] ON [DashboardPreferences] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_BranchId_Code] ON [Departments] ([BranchId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_HrResponsibleId] ON [Departments] ([HrResponsibleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_ManagerId] ON [Departments] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_OryggiDepartmentId] ON [Departments] ([OryggiDepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_SecondaryManagerId] ON [Departments] ([SecondaryManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailAttachments_EmailMessageId] ON [EmailAttachments] ([EmailMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailConfigurations_AutoAcknowledgementTemplateId1] ON [EmailConfigurations] ([AutoAcknowledgementTemplateId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailConfigurations_CompanyId] ON [EmailConfigurations] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailMessages_CompanyId] ON [EmailMessages] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailMessages_ComplaintId] ON [EmailMessages] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailMessages_ReadByUserId] ON [EmailMessages] ([ReadByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailMessages_SentByUserId] ON [EmailMessages] ([SentByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_ComplaintId] ON [EmailResponseHistories] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_ComplaintId_SentAt] ON [EmailResponseHistories] ([ComplaintId], [SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_DeliveryStatus] ON [EmailResponseHistories] ([DeliveryStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_EmailMessageId] ON [EmailResponseHistories] ([EmailMessageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_SentAt] ON [EmailResponseHistories] ([SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailResponseHistories_SentBy] ON [EmailResponseHistories] ([SentBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailServerSettings_CompanyId] ON [EmailServerSettings] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailServerSettings_IsActive_IsDefault] ON [EmailServerSettings] ([IsActive], [IsDefault]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_ManagerId] ON [Employees] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_SectionId] ON [Employees] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_TenantId] ON [Employees] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmployeeTypes_CompanyId_Code] ON [EmployeeTypes] ([CompanyId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeeTypes_OryggiEmployeeTypeId] ON [EmployeeTypes] ([OryggiEmployeeTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_ComplaintId] ON [EscalationHistories] ([ComplaintId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_ComplaintId_EscalatedAt] ON [EscalationHistories] ([ComplaintId], [EscalatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_ComplaintId_Level] ON [EscalationHistories] ([ComplaintId], [Level]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_EscalatedAt] ON [EscalationHistories] ([EscalatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_EscalatedBy] ON [EscalationHistories] ([EscalatedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_EscalationLevelId] ON [EscalationHistories] ([EscalationLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_EscalationMatrixId] ON [EscalationHistories] ([EscalationMatrixId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_FromUserId] ON [EscalationHistories] ([FromUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_Status] ON [EscalationHistories] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationHistories_ToUserId] ON [EscalationHistories] ([ToUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_AssignToUserId] ON [EscalationLevels] ([AssignToUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_BranchId] ON [EscalationLevels] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_DepartmentId] ON [EscalationLevels] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_EscalationMatrixId] ON [EscalationLevels] ([EscalationMatrixId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_EscalationMatrixId_Level] ON [EscalationLevels] ([EscalationMatrixId], [Level]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_HrContactId] ON [EscalationLevels] ([HrContactId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_IsActive] ON [EscalationLevels] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_PrimaryContactId] ON [EscalationLevels] ([PrimaryContactId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_ResourcePoolId] ON [EscalationLevels] ([ResourcePoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationLevels_SecondaryContactId] ON [EscalationLevels] ([SecondaryContactId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_BranchId] ON [EscalationMatrices] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_CategoryId] ON [EscalationMatrices] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_CompanyId] ON [EscalationMatrices] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_CompanyId_IsActive_Priority] ON [EscalationMatrices] ([CompanyId], [IsActive], [Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_DepartmentId] ON [EscalationMatrices] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationMatrices_IsActive] ON [EscalationMatrices] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_BranchId] ON [EscalationPolicies] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CategoryId] ON [EscalationPolicies] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CompanyId_BranchId_IsActive] ON [EscalationPolicies] ([CompanyId], [BranchId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CompanyId_CategoryId_IsActive] ON [EscalationPolicies] ([CompanyId], [CategoryId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CompanyId_DepartmentId_IsActive] ON [EscalationPolicies] ([CompanyId], [DepartmentId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CompanyId_IsActive] ON [EscalationPolicies] ([CompanyId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_CompanyId_SectionId_IsActive] ON [EscalationPolicies] ([CompanyId], [SectionId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_DefaultEscalationMatrixId] ON [EscalationPolicies] ([DefaultEscalationMatrixId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_DepartmentId] ON [EscalationPolicies] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_FullHierarchy] ON [EscalationPolicies] ([CompanyId], [BranchId], [DepartmentId], [SectionId], [CategoryId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EscalationPolicies_SectionId] ON [EscalationPolicies] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventCommunicationRules_Channel] ON [EventCommunicationRules] ([Channel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventCommunicationRules_CompanyId] ON [EventCommunicationRules] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventCommunicationRules_EventTypeId] ON [EventCommunicationRules] ([EventTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventCommunicationRules_IsActive_Priority] ON [EventCommunicationRules] ([IsActive], [Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventCommunicationRules_TemplateId] ON [EventCommunicationRules] ([TemplateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventTypes_Category] ON [EventTypes] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EventTypes_Code] ON [EventTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventTypes_CompanyId] ON [EventTypes] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EventTypes_EntityType_IsActive] ON [EventTypes] ([EntityType], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalUserMappings_AuthenticationProviderId] ON [ExternalUserMappings] ([AuthenticationProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalUserMappings_ExternalUserId] ON [ExternalUserMappings] ([ExternalUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalUserMappings_IsActive] ON [ExternalUserMappings] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExternalUserMappings_ProviderId_ExternalUserId] ON [ExternalUserMappings] ([AuthenticationProviderId], [ExternalUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalUserMappings_UserId] ON [ExternalUserMappings] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalUserMappings_UserId_ProviderId] ON [ExternalUserMappings] ([UserId], [AuthenticationProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OryggiConnectionSettings_TenantId] ON [OryggiConnectionSettings] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_Action] ON [PasswordAuditLog] ([Action]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_CreatedAt] ON [PasswordAuditLog] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_PerformedBy] ON [PasswordAuditLog] ([PerformedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_Success] ON [PasswordAuditLog] ([Success]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_Success_CreatedAt] ON [PasswordAuditLog] ([Success], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_UserId] ON [PasswordAuditLog] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordAuditLog_UserId_CreatedAt] ON [PasswordAuditLog] ([UserId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistory_CreatedAt] ON [PasswordHistory] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistory_CreatedBy] ON [PasswordHistory] ([CreatedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistory_UserId] ON [PasswordHistory] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistory_UserId_CreatedAt] ON [PasswordHistory] ([UserId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordPolicy_CompanyId] ON [PasswordPolicy] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrioritySLAs_IsActive] ON [PrioritySLAs] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PrioritySLAs_PriorityId] ON [PrioritySLAs] ([PriorityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrioritySLAs_PriorityId_SLALevelId] ON [PrioritySLAs] ([PriorityId], [SLALevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrioritySLAs_SLALevelId] ON [PrioritySLAs] ([SLALevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [RefreshTokens] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_TokenFamily] ON [RefreshTokens] ([TokenFamily]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePoolMembers_ResourcePoolId] ON [ResourcePoolMembers] ([ResourcePoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePoolMembers_UserId] ON [ResourcePoolMembers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePools_BranchId] ON [ResourcePools] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePools_CompanyId] ON [ResourcePools] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePools_DepartmentId] ON [ResourcePools] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ResourcePools_SectionId] ON [ResourcePools] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sections_DepartmentId_Code] ON [Sections] ([DepartmentId], [Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sections_HeadId] ON [Sections] ([HeadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sections_HrResponsibleId] ON [Sections] ([HrResponsibleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sections_OryggiSectionId] ON [Sections] ([OryggiSectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sections_SecondaryHeadId] ON [Sections] ([SecondaryHeadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLALevels_CompanyId] ON [SLALevels] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLALevels_CompanyId_Name] ON [SLALevels] ([CompanyId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLALevels_CompanyId_Order] ON [SLALevels] ([CompanyId], [Order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLALevels_IsActive] ON [SLALevels] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLALevels_Order] ON [SLALevels] ([Order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SLASettings_CompanyId] ON [SLASettings] ([CompanyId]) WHERE [CompanyId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SLASettings_IsEnabled] ON [SLASettings] ([IsEnabled]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SmsGatewaySettings_CompanyId] ON [SmsGatewaySettings] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SmsGatewaySettings_IsActive_IsDefault] ON [SmsGatewaySettings] ([IsActive], [IsDefault]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SmsGatewaySettings_Provider] ON [SmsGatewaySettings] ([Provider]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SyncSchedules_TenantId] ON [SyncSchedules] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemConfigurations_CompanyId] ON [SystemConfigurations] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tenants_Code] ON [Tenants] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tenants_OryggiTenantId] ON [Tenants] ([OryggiTenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_BranchId] ON [UserComplaintRoles] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_CompanyId] ON [UserComplaintRoles] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_ComplaintRoleId] ON [UserComplaintRoles] ([ComplaintRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_DepartmentId] ON [UserComplaintRoles] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_EffectiveFrom_EffectiveTo] ON [UserComplaintRoles] ([EffectiveFrom], [EffectiveTo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_SectionId] ON [UserComplaintRoles] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_UserId] ON [UserComplaintRoles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserComplaintRoles_UserId_ComplaintRoleId_IsActive] ON [UserComplaintRoles] ([UserId], [ComplaintRoleId], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_AccountLockedUntil] ON [Users] ([AccountLockedUntil]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_AuthenticationProviderType] ON [Users] ([AuthenticationProviderType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_AuthProviderType_ExternalUserId] ON [Users] ([AuthenticationProviderType], [ExternalUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_BranchId] ON [Users] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_CompanyId] ON [Users] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_EmployeeCode] ON [Users] ([EmployeeCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_EmployeeTypeId] ON [Users] ([EmployeeTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_ExternalUserId] ON [Users] ([ExternalUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_ManagerId] ON [Users] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_OryggiEmployeeId] ON [Users] ([OryggiEmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_PasswordExpiresAt] ON [Users] ([PasswordExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_SectionId] ON [Users] ([SectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WhatsAppSettings_CompanyId] ON [WhatsAppSettings] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WhatsAppSettings_IsActive_IsDefault] ON [WhatsAppSettings] ([IsActive], [IsDefault]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WhatsAppSettings_Provider] ON [WhatsAppSettings] ([Provider]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [AuthenticationProviders] ADD CONSTRAINT [FK_AuthenticationProviders_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [FK_Branches_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [FK_Branches_Users_HrResponsibleId] FOREIGN KEY ([HrResponsibleId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [FK_Branches_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [FK_Branches_Users_SecondaryManagerId] FOREIGN KEY ([SecondaryManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CannedResponses] ADD CONSTRAINT [FK_CannedResponses_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CannedResponses] ADD CONSTRAINT [FK_CannedResponses_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CategoryWorkflows] ADD CONSTRAINT [FK_CategoryWorkflows_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CategoryWorkflowStatuses] ADD CONSTRAINT [FK_CategoryWorkflowStatuses_ComplaintStatusMasters_StatusMasterId] FOREIGN KEY ([StatusMasterId]) REFERENCES [ComplaintStatusMasters] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CategoryWorkflowTransitions] ADD CONSTRAINT [FK_CategoryWorkflowTransitions_ComplaintStatusMasters_FromStatusId] FOREIGN KEY ([FromStatusId]) REFERENCES [ComplaintStatusMasters] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CategoryWorkflowTransitions] ADD CONSTRAINT [FK_CategoryWorkflowTransitions_ComplaintStatusMasters_ToStatusId] FOREIGN KEY ([ToStatusId]) REFERENCES [ComplaintStatusMasters] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CommunicationLogs] ADD CONSTRAINT [FK_CommunicationLogs_CommunicationTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [CommunicationTemplates] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CommunicationLogs] ADD CONSTRAINT [FK_CommunicationLogs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CommunicationLogs] ADD CONSTRAINT [FK_CommunicationLogs_Users_RecipientUserId] FOREIGN KEY ([RecipientUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [CommunicationTemplates] ADD CONSTRAINT [FK_CommunicationTemplates_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Companies] ADD CONSTRAINT [FK_Companies_Users_HrResponsibleId] FOREIGN KEY ([HrResponsibleId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Companies] ADD CONSTRAINT [FK_Companies_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Companies] ADD CONSTRAINT [FK_Companies_Users_SecondaryManagerId] FOREIGN KEY ([SecondaryManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintAttachments] ADD CONSTRAINT [FK_ComplaintAttachments_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintAttachments] ADD CONSTRAINT [FK_ComplaintAttachments_Users_UploadedBy] FOREIGN KEY ([UploadedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintComments] ADD CONSTRAINT [FK_ComplaintComments_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintComments] ADD CONSTRAINT [FK_ComplaintComments_Users_CommentedBy] FOREIGN KEY ([CommentedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintEmailParticipants] ADD CONSTRAINT [FK_ComplaintEmailParticipants_Complaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [Complaints] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ComplaintEmailParticipants] ADD CONSTRAINT [FK_ComplaintEmailParticipants_Users_AddedByUserId] FOREIGN KEY ([AddedByUserId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Complaints] ADD CONSTRAINT [FK_Complaints_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Complaints] ADD CONSTRAINT [FK_Complaints_ResourcePools_ResourcePoolId] FOREIGN KEY ([ResourcePoolId]) REFERENCES [ResourcePools] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Complaints] ADD CONSTRAINT [FK_Complaints_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Complaints] ADD CONSTRAINT [FK_Complaints_Users_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Complaints] ADD CONSTRAINT [FK_Complaints_Users_ComplainantId] FOREIGN KEY ([ComplainantId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [DashboardPreferences] ADD CONSTRAINT [FK_DashboardPreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Departments] ADD CONSTRAINT [FK_Departments_Users_HrResponsibleId] FOREIGN KEY ([HrResponsibleId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Departments] ADD CONSTRAINT [FK_Departments_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Departments] ADD CONSTRAINT [FK_Departments_Users_SecondaryManagerId] FOREIGN KEY ([SecondaryManagerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EmailAttachments] ADD CONSTRAINT [FK_EmailAttachments_EmailMessages_EmailMessageId] FOREIGN KEY ([EmailMessageId]) REFERENCES [EmailMessages] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EmailMessages] ADD CONSTRAINT [FK_EmailMessages_Users_ReadByUserId] FOREIGN KEY ([ReadByUserId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EmailMessages] ADD CONSTRAINT [FK_EmailMessages_Users_SentByUserId] FOREIGN KEY ([SentByUserId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EmailResponseHistories] ADD CONSTRAINT [FK_EmailResponseHistories_Users_SentBy] FOREIGN KEY ([SentBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationHistories] ADD CONSTRAINT [FK_EscalationHistories_EscalationLevels_EscalationLevelId] FOREIGN KEY ([EscalationLevelId]) REFERENCES [EscalationLevels] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationHistories] ADD CONSTRAINT [FK_EscalationHistories_Users_EscalatedBy] FOREIGN KEY ([EscalatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationHistories] ADD CONSTRAINT [FK_EscalationHistories_Users_FromUserId] FOREIGN KEY ([FromUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationHistories] ADD CONSTRAINT [FK_EscalationHistories_Users_ToUserId] FOREIGN KEY ([ToUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationLevels] ADD CONSTRAINT [FK_EscalationLevels_ResourcePools_ResourcePoolId] FOREIGN KEY ([ResourcePoolId]) REFERENCES [ResourcePools] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationLevels] ADD CONSTRAINT [FK_EscalationLevels_Users_AssignToUserId] FOREIGN KEY ([AssignToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationLevels] ADD CONSTRAINT [FK_EscalationLevels_Users_HrContactId] FOREIGN KEY ([HrContactId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationLevels] ADD CONSTRAINT [FK_EscalationLevels_Users_PrimaryContactId] FOREIGN KEY ([PrimaryContactId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationLevels] ADD CONSTRAINT [FK_EscalationLevels_Users_SecondaryContactId] FOREIGN KEY ([SecondaryContactId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [EscalationPolicies] ADD CONSTRAINT [FK_EscalationPolicies_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ExternalUserMappings] ADD CONSTRAINT [FK_ExternalUserMappings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [PasswordAuditLog] ADD CONSTRAINT [FK_PasswordAuditLog_Users_PerformedBy] FOREIGN KEY ([PerformedBy]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [PasswordAuditLog] ADD CONSTRAINT [FK_PasswordAuditLog_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [PasswordResetTokens] ADD CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ResourcePoolMembers] ADD CONSTRAINT [FK_ResourcePoolMembers_ResourcePools_ResourcePoolId] FOREIGN KEY ([ResourcePoolId]) REFERENCES [ResourcePools] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ResourcePoolMembers] ADD CONSTRAINT [FK_ResourcePoolMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [ResourcePools] ADD CONSTRAINT [FK_ResourcePools_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Sections] ADD CONSTRAINT [FK_Sections_Users_HeadId] FOREIGN KEY ([HeadId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Sections] ADD CONSTRAINT [FK_Sections_Users_HrResponsibleId] FOREIGN KEY ([HrResponsibleId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    ALTER TABLE [Sections] ADD CONSTRAINT [FK_Sections_Users_SecondaryHeadId] FOREIGN KEY ([SecondaryHeadId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228194412_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228194412_InitialCreate', N'9.0.9');
END;

COMMIT;
GO

