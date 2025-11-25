using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Configuration;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Entities.Events;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Domain.Entities.SLA;
using ComplaintManagement.Domain.Entities.Sync;
using ComplaintManagement.Domain.Entities.Workflows;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ComplaintManagement.Infrastructure.Data;

/// <summary>
/// Main database context for the Complaint Management System
/// </summary>
public class ComplaintDbContext : DbContext
{
    public ComplaintDbContext(DbContextOptions<ComplaintDbContext> options) : base(options)
    {
    }

    #region Auth DbSets

    /// <summary>
    /// Refresh tokens for secure token rotation and long-lived authentication
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Password history to prevent password reuse
    /// </summary>
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

    /// <summary>
    /// Audit log for all password-related operations
    /// </summary>
    public DbSet<PasswordAuditLog> PasswordAuditLogs { get; set; }

    /// <summary>
    /// Company-wide password policy configuration
    /// </summary>
    public DbSet<PasswordPolicy> PasswordPolicies { get; set; }

    /// <summary>
    /// Authentication providers (AD, SSO, SAML, OAuth, etc.)
    /// </summary>
    public DbSet<AuthenticationProvider> AuthenticationProviders { get; set; }

    /// <summary>
    /// External user mappings for JIT provisioning
    /// </summary>
    public DbSet<ExternalUserMapping> ExternalUserMappings { get; set; }

    /// <summary>
    /// Password reset tokens for self-service password recovery
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    #endregion

    #region Master Data DbSets

    /// <summary>
    /// Tenants in the multi-tenant system
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Companies within tenants
    /// </summary>
    public DbSet<Company> Companies { get; set; }

    /// <summary>
    /// Branches within companies
    /// </summary>
    public DbSet<Branch> Branches { get; set; }

    /// <summary>
    /// Departments within branches
    /// </summary>
    public DbSet<Department> Departments { get; set; }

    /// <summary>
    /// Sections within departments
    /// </summary>
    public DbSet<Section> Sections { get; set; }

    /// <summary>
    /// Employee types/classifications
    /// </summary>
    public DbSet<EmployeeType> EmployeeTypes { get; set; }

    /// <summary>
    /// Users synced from Oryggi HRMS
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Employees synced from Oryggi HRMS
    /// </summary>
    public DbSet<Employee> Employees { get; set; }

    /// <summary>
    /// Complaint status master data
    /// </summary>
    public DbSet<ComplaintStatusMaster> ComplaintStatusMasters { get; set; }

    /// <summary>
    /// Complaint priority master data
    /// </summary>
    public DbSet<ComplaintPriorityMaster> ComplaintPriorityMasters { get; set; }

    #endregion

    #region Workflow DbSets

    /// <summary>
    /// Category-specific workflows
    /// </summary>
    public DbSet<CategoryWorkflow> CategoryWorkflows { get; set; }

    /// <summary>
    /// Statuses available in category workflows
    /// </summary>
    public DbSet<CategoryWorkflowStatus> CategoryWorkflowStatuses { get; set; }

    /// <summary>
    /// Allowed transitions between statuses in category workflows
    /// </summary>
    public DbSet<CategoryWorkflowTransition> CategoryWorkflowTransitions { get; set; }

    #endregion

    #region Sync DbSets

    /// <summary>
    /// Sync history logs for Oryggi integration
    /// </summary>
    public DbSet<SyncLog> SyncLogs { get; set; }

    /// <summary>
    /// Sync schedules for automatic Oryggi synchronization
    /// </summary>
    public DbSet<SyncSchedule> SyncSchedules { get; set; }

    /// <summary>
    /// Oryggi database connection settings
    /// </summary>
    public DbSet<OryggiConnectionSettings> OryggiConnectionSettings { get; set; }

    #endregion

    #region Complaint DbSets

    /// <summary>
    /// Complaint categories
    /// </summary>
    public DbSet<ComplaintCategory> ComplaintCategories { get; set; }

    /// <summary>
    /// Complaints submitted by users
    /// </summary>
    public DbSet<Complaint> Complaints { get; set; }

    /// <summary>
    /// Comments on complaints
    /// </summary>
    public DbSet<ComplaintComment> ComplaintComments { get; set; }

    /// <summary>
    /// File attachments to complaints
    /// </summary>
    public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }

    #endregion

    #region Escalation DbSets

    /// <summary>
    /// Escalation matrices for complaint escalation rules
    /// </summary>
    public DbSet<EscalationMatrix> EscalationMatrices { get; set; }

    /// <summary>
    /// Escalation levels within matrices
    /// </summary>
    public DbSet<EscalationLevel> EscalationLevels { get; set; }

    /// <summary>
    /// Historical records of complaint escalations
    /// </summary>
    public DbSet<EscalationHistory> EscalationHistories { get; set; }

    /// <summary>
    /// Escalation policies with hierarchical override support
    /// </summary>
    public DbSet<EscalationPolicy> EscalationPolicies { get; set; }

    /// <summary>
    /// Resource pools for pool-based escalation assignment
    /// </summary>
    public DbSet<ResourcePool> ResourcePools { get; set; }

    /// <summary>
    /// Members of resource pools
    /// </summary>
    public DbSet<ResourcePoolMember> ResourcePoolMembers { get; set; }

    #endregion

    #region SLA (Service Level Agreement) DbSets

    /// <summary>
    /// Global SLA configuration settings
    /// </summary>
    public DbSet<SLASettings> SLASettings { get; set; }

    /// <summary>
    /// SLA levels/tiers (e.g., Standard, Premium, Enterprise)
    /// </summary>
    public DbSet<SLALevel> SLALevels { get; set; }

    /// <summary>
    /// Category to SLA level mappings
    /// </summary>
    public DbSet<CategorySLA> CategorySLAs { get; set; }

    /// <summary>
    /// Priority to SLA level mappings
    /// </summary>
    public DbSet<PrioritySLA> PrioritySLAs { get; set; }

    #endregion

    #region Settings DbSets

    /// <summary>
    /// System-wide configuration settings (OAuth, email polling, notifications, etc.)
    /// </summary>
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    /// <summary>
    /// Complaint information visibility and privacy settings
    /// </summary>
    public DbSet<ComplaintInformationSettings> ComplaintInformationSettings { get; set; }

    /// <summary>
    /// User-specific dashboard preferences and layout settings
    /// </summary>
    public DbSet<DashboardPreferences> DashboardPreferences { get; set; }

    #endregion

    #region Communication & Notification DbSets

    /// <summary>
    /// Email (SMTP) server configurations
    /// </summary>
    public DbSet<EmailServerSettings> EmailServerSettings { get; set; }

    /// <summary>
    /// SMS gateway/provider configurations
    /// </summary>
    public DbSet<SmsGatewaySettings> SmsGatewaySettings { get; set; }

    /// <summary>
    /// WhatsApp Business API configurations
    /// </summary>
    public DbSet<WhatsAppSettings> WhatsAppSettings { get; set; }

    /// <summary>
    /// Communication message templates
    /// </summary>
    public DbSet<CommunicationTemplate> CommunicationTemplates { get; set; }

    /// <summary>
    /// Logs of all communications sent
    /// </summary>
    public DbSet<CommunicationLog> CommunicationLogs { get; set; }

    /// <summary>
    /// System event types that can trigger communications
    /// </summary>
    public DbSet<EventType> EventTypes { get; set; }

    /// <summary>
    /// Rules mapping events to communication actions
    /// </summary>
    public DbSet<EventCommunicationRule> EventCommunicationRules { get; set; }

    // Email Ticketing System
    /// <summary>
    /// Email configuration for IMAP/SMTP ticketing integration
    /// </summary>
    public DbSet<EmailConfiguration> EmailConfigurations { get; set; }

    /// <summary>
    /// Individual email messages (inbound and outbound)
    /// </summary>
    public DbSet<EmailMessage> EmailMessages { get; set; }

    /// <summary>
    /// Email attachments linked to messages
    /// </summary>
    public DbSet<EmailAttachment> EmailAttachments { get; set; }

    /// <summary>
    /// Email response history - tracks outbound emails sent from the system
    /// </summary>
    public DbSet<EmailResponseHistory> EmailResponseHistories { get; set; }

    /// <summary>
    /// Email thread participants - tracks all recipients in complaint email conversations
    /// </summary>
    public DbSet<ComplaintEmailParticipant> ComplaintEmailParticipants { get; set; }

    /// <summary>
    /// Canned responses - pre-written email templates for quick replies
    /// </summary>
    public DbSet<CannedResponse> CannedResponses { get; set; }

    #endregion

    #region Role & Permission DbSets

    /// <summary>
    /// Complaint management roles
    /// </summary>
    public DbSet<ComplaintRole> ComplaintRoles { get; set; }

    /// <summary>
    /// User role assignments with organizational scope
    /// </summary>
    public DbSet<UserComplaintRole> UserComplaintRoles { get; set; }

    /// <summary>
    /// Role permissions
    /// </summary>
    public DbSet<ComplaintRolePermission> ComplaintRolePermissions { get; set; }

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suppress PendingModelChangesWarning to allow migrations with dynamic seed data
        // TODO: Fix seed data to use static values instead of DateTime.Now and Guid.NewGuid()
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if entity has IsDeleted property
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var comparison = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically populate audit fields (CreatedAt, UpdatedAt, etc.)
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    break;
            }
        }
    }
}
