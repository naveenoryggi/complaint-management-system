using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Configuration;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Infrastructure.Repositories;
using ComplaintManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ComplaintManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Configuration
        // Note: For simplicity, Oryggi services are always registered
        // They can be enabled/disabled via appsettings.json OryggiSettings:Enabled flag
        services.Configure<OryggiSettings>(opts =>
        {
            var section = configuration.GetSection("OryggiSettings");
            opts.Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled;
            opts.SyncOnStartup = bool.TryParse(section["SyncOnStartup"], out var startup) && startup;
            opts.SyncIntervalHours = int.TryParse(section["SyncIntervalHours"], out var hours) ? hours : 6;
        });

        // Register DbContext
        services.AddDbContext<ComplaintDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                }
            )
        );

        // Register Oryggi DbContext
        services.AddDbContext<OryggiDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("OryggiConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                }
            )
        );

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<IComplaintRepository, ComplaintRepository>();
        services.AddScoped<IComplaintCategoryRepository, ComplaintCategoryRepository>();
        services.AddScoped<IComplaintCommentRepository, ComplaintCommentRepository>();
        services.AddScoped<IComplaintAttachmentRepository, ComplaintAttachmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IComplaintRoleRepository, ComplaintRoleRepository>();
        services.AddScoped<IUserComplaintRoleRepository, UserComplaintRoleRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IEmployeeTypeRepository, EmployeeTypeRepository>();
        services.AddScoped<IEscalationMatrixRepository, EscalationMatrixRepository>();
        services.AddScoped<IEscalationHistoryRepository, EscalationHistoryRepository>();

        // Register Services
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEscalationService, EscalationService>();
        services.AddScoped<IAutoEscalationService, AutoEscalationService>();
        services.AddScoped<Application.Interfaces.IEscalationPolicyService, EscalationPolicyService>();
        services.AddScoped<IResourcePoolService, ResourcePoolService>();
        services.AddScoped<Services.Interfaces.IFileStorageService, FileStorageService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Register Assignment Engine
        // Using Advanced Assignment Engine with intelligent routing and resource allocation
        services.AddScoped<IAssignmentEngine, AdvancedAssignmentEngine>();

        // Register Timezone Service for timezone-aware SLA calculations
        services.AddScoped<ITimeZoneService, TimeZoneService>();

        // Register SLA Calculator Service
        services.AddScoped<ISLACalculatorService, SLACalculatorService>();

        // Register Workflow Engine
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();

        // Register Notification Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        // Register Email Ticketing Service
        services.AddScoped<IEmailTicketingService, EmailTicketingService>();

        // Register Email Threading Service for Reply/Reply All/Forward
        services.AddScoped<IEmailThreadingService, EmailThreadingService>();

        // Register Auto-Response Service for automated email notifications
        services.AddScoped<IAutoResponseService, AutoResponseService>();

        // Register License Service for module-based licensing
        services.AddScoped<ILicenseService, LicenseService>();

        // Register Product Service for CRM_Product module
        services.AddScoped<IProductService, ProductService>();

        // Register Contract Service for ServiceContract module
        services.AddScoped<IContractService, ContractService>();

        // Register Asset Service for AssetManagement module
        services.AddScoped<IAssetService, AssetService>();

        // Register Portal Service for CustomerPortal module
        services.AddScoped<IPortalService, PortalService>();

        // Register Customer Service for CRM_Customer module
        services.AddScoped<ICustomerService, CustomerService>();

        // Register OAuth Service for Office 365 Email Authentication
        services.AddSingleton<EmailOAuthService>();

        // Register OAuth Token Refresh Background Service
        services.AddHostedService<OAuthTokenRefreshBackgroundService>();

        // Register Oryggi Sync Services
        services.AddScoped<OryggiSyncService>();
        services.AddHostedService<OryggiSyncBackgroundService>();
        services.AddScoped<ISqlDiagnosticsService, SqlDiagnosticsService>();
        services.AddScoped<IOryggiConnectionSettingsService, OryggiConnectionSettingsService>();

        return services;
    }
}
