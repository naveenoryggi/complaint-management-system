using System;
using System.Collections.Generic;
using ComplaintManagement.API.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Data;

public partial class ComplaintManagementDbContext : DbContext
{
    public ComplaintManagementDbContext(DbContextOptions<ComplaintManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuthenticationProvider> AuthenticationProviders { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<CannedResponse> CannedResponses { get; set; }

    public virtual DbSet<CategorySla> CategorySlas { get; set; }

    public virtual DbSet<CategoryWorkflow> CategoryWorkflows { get; set; }

    public virtual DbSet<CategoryWorkflowStatus> CategoryWorkflowStatuses { get; set; }

    public virtual DbSet<CategoryWorkflowTransition> CategoryWorkflowTransitions { get; set; }

    public virtual DbSet<CommunicationLog> CommunicationLogs { get; set; }

    public virtual DbSet<CommunicationTemplate> CommunicationTemplates { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }

    public virtual DbSet<ComplaintCategory> ComplaintCategories { get; set; }

    public virtual DbSet<ComplaintComment> ComplaintComments { get; set; }

    public virtual DbSet<ComplaintEmailParticipant> ComplaintEmailParticipants { get; set; }

    public virtual DbSet<ComplaintInformationSetting> ComplaintInformationSettings { get; set; }

    public virtual DbSet<ComplaintNumberSequence> ComplaintNumberSequences { get; set; }

    public virtual DbSet<ComplaintPriorityMaster> ComplaintPriorityMasters { get; set; }

    public virtual DbSet<ComplaintRole> ComplaintRoles { get; set; }

    public virtual DbSet<ComplaintRolePermission> ComplaintRolePermissions { get; set; }

    public virtual DbSet<ComplaintStatusMaster> ComplaintStatusMasters { get; set; }

    public virtual DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }

    public virtual DbSet<CustomFieldValue> CustomFieldValues { get; set; }

    public virtual DbSet<DashboardPreference> DashboardPreferences { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<EmailAttachment> EmailAttachments { get; set; }

    public virtual DbSet<EmailConfiguration> EmailConfigurations { get; set; }

    public virtual DbSet<EmailMessage> EmailMessages { get; set; }

    public virtual DbSet<EmailResponseHistory> EmailResponseHistories { get; set; }

    public virtual DbSet<EmailServerSetting> EmailServerSettings { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeType> EmployeeTypes { get; set; }

    public virtual DbSet<EscalationHistory> EscalationHistories { get; set; }

    public virtual DbSet<EscalationLevel> EscalationLevels { get; set; }

    public virtual DbSet<EscalationMatrix> EscalationMatrices { get; set; }

    public virtual DbSet<EscalationPolicy> EscalationPolicies { get; set; }

    public virtual DbSet<EventCommunicationRule> EventCommunicationRules { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<ExternalUserMapping> ExternalUserMappings { get; set; }

    public virtual DbSet<OryggiConnectionSetting> OryggiConnectionSettings { get; set; }

    public virtual DbSet<PasswordAuditLog> PasswordAuditLogs { get; set; }

    public virtual DbSet<PasswordHistory> PasswordHistories { get; set; }

    public virtual DbSet<PasswordPolicy> PasswordPolicies { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PrioritySla> PrioritySlas { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ResourcePool> ResourcePools { get; set; }

    public virtual DbSet<ResourcePoolMember> ResourcePoolMembers { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Slalevel> Slalevels { get; set; }

    public virtual DbSet<Slasetting> Slasettings { get; set; }

    public virtual DbSet<SmsGatewaySetting> SmsGatewaySettings { get; set; }

    public virtual DbSet<SyncLog> SyncLogs { get; set; }

    public virtual DbSet<SyncSchedule> SyncSchedules { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserComplaintRole> UserComplaintRoles { get; set; }

    public virtual DbSet<WhatsAppSetting> WhatsAppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthenticationProvider>(entity =>
        {
            entity.HasIndex(e => e.AutoAssignRoleId, "IX_AuthenticationProviders_AutoAssignRoleId");

            entity.HasIndex(e => e.CompanyId, "IX_AuthenticationProviders_CompanyId");

            entity.HasIndex(e => new { e.CompanyId, e.IsDefault }, "IX_AuthenticationProviders_CompanyId_IsDefault");

            entity.HasIndex(e => new { e.CompanyId, e.IsEnabled }, "IX_AuthenticationProviders_CompanyId_IsEnabled");

            entity.HasIndex(e => e.EmailDomain, "IX_AuthenticationProviders_EmailDomain");

            entity.HasIndex(e => e.IsDefault, "IX_AuthenticationProviders_IsDefault");

            entity.HasIndex(e => e.IsEnabled, "IX_AuthenticationProviders_IsEnabled");

            entity.HasIndex(e => e.ProviderType, "IX_AuthenticationProviders_ProviderType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AdbaseDn)
                .HasMaxLength(500)
                .HasColumnName("ADBaseDN");
            entity.Property(e => e.Addomain)
                .HasMaxLength(200)
                .HasColumnName("ADDomain");
            entity.Property(e => e.Adport).HasColumnName("ADPort");
            entity.Property(e => e.Adserver)
                .HasMaxLength(200)
                .HasColumnName("ADServer");
            entity.Property(e => e.AdserviceAccountPasswordEncrypted)
                .HasMaxLength(1000)
                .HasColumnName("ADServiceAccountPasswordEncrypted");
            entity.Property(e => e.AdserviceAccountUsername)
                .HasMaxLength(200)
                .HasColumnName("ADServiceAccountUsername");
            entity.Property(e => e.AduseSsl).HasColumnName("ADUseSSL");
            entity.Property(e => e.AduserFilter)
                .HasMaxLength(500)
                .HasColumnName("ADUserFilter");
            entity.Property(e => e.AzureAdclientId)
                .HasMaxLength(500)
                .HasColumnName("AzureADClientId");
            entity.Property(e => e.AzureAdclientSecretEncrypted)
                .HasMaxLength(1000)
                .HasColumnName("AzureADClientSecretEncrypted");
            entity.Property(e => e.AzureAdinstance)
                .HasMaxLength(500)
                .HasColumnName("AzureADInstance");
            entity.Property(e => e.AzureAdtenantId)
                .HasMaxLength(100)
                .HasColumnName("AzureADTenantId");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CustomApiendpoint)
                .HasMaxLength(500)
                .HasColumnName("CustomAPIEndpoint");
            entity.Property(e => e.CustomApiheaders)
                .HasMaxLength(2000)
                .HasColumnName("CustomAPIHeaders");
            entity.Property(e => e.CustomApikeyEncrypted)
                .HasMaxLength(1000)
                .HasColumnName("CustomAPIKeyEncrypted");
            entity.Property(e => e.CustomApimethod)
                .HasMaxLength(10)
                .HasColumnName("CustomAPIMethod");
            entity.Property(e => e.CustomApirequestTemplate)
                .HasMaxLength(2000)
                .HasColumnName("CustomAPIRequestTemplate");
            entity.Property(e => e.EmailDomain).HasMaxLength(200);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.JitprovisioningEnabled)
                .HasDefaultValue(true)
                .HasColumnName("JITProvisioningEnabled");
            entity.Property(e => e.OauthAuthorizationUrl)
                .HasMaxLength(500)
                .HasColumnName("OAuthAuthorizationUrl");
            entity.Property(e => e.OauthClientId)
                .HasMaxLength(500)
                .HasColumnName("OAuthClientId");
            entity.Property(e => e.OauthClientSecretEncrypted)
                .HasMaxLength(1000)
                .HasColumnName("OAuthClientSecretEncrypted");
            entity.Property(e => e.OauthRedirectUri)
                .HasMaxLength(500)
                .HasColumnName("OAuthRedirectUri");
            entity.Property(e => e.OauthScopes)
                .HasMaxLength(500)
                .HasColumnName("OAuthScopes");
            entity.Property(e => e.OauthTokenUrl)
                .HasMaxLength(500)
                .HasColumnName("OAuthTokenUrl");
            entity.Property(e => e.OauthUserInfoUrl)
                .HasMaxLength(500)
                .HasColumnName("OAuthUserInfoUrl");
            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.ProviderType).HasMaxLength(50);
            entity.Property(e => e.Samlcertificate).HasColumnName("SAMLCertificate");
            entity.Property(e => e.SamlentityId)
                .HasMaxLength(500)
                .HasColumnName("SAMLEntityId");
            entity.Property(e => e.SamlsigningAlgorithm)
                .HasMaxLength(200)
                .HasColumnName("SAMLSigningAlgorithm");
            entity.Property(e => e.Samlslourl)
                .HasMaxLength(500)
                .HasColumnName("SAMLSLOUrl");
            entity.Property(e => e.Samlssourl)
                .HasMaxLength(500)
                .HasColumnName("SAMLSSOUrl");
            entity.Property(e => e.SyncAttributesOnLogin).HasDefaultValue(true);

            entity.HasOne(d => d.AutoAssignRole).WithMany(p => p.AuthenticationProviders)
                .HasForeignKey(d => d.AutoAssignRoleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.AuthenticationProviders).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.Code }, "IX_Branches_CompanyId_Code").IsUnique();

            entity.HasIndex(e => e.HrResponsibleId, "IX_Branches_HrResponsibleId");

            entity.HasIndex(e => e.ManagerId, "IX_Branches_ManagerId");

            entity.HasIndex(e => e.OryggiBranchId, "IX_Branches_OryggiBranchId");

            entity.HasIndex(e => e.SecondaryManagerId, "IX_Branches_SecondaryManagerId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiBranchId).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Branches)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.HrResponsible).WithMany(p => p.BranchHrResponsibles).HasForeignKey(d => d.HrResponsibleId);

            entity.HasOne(d => d.Manager).WithMany(p => p.BranchManagers).HasForeignKey(d => d.ManagerId);

            entity.HasOne(d => d.SecondaryManager).WithMany(p => p.BranchSecondaryManagers).HasForeignKey(d => d.SecondaryManagerId);
        });

        modelBuilder.Entity<CannedResponse>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_CannedResponses_CategoryId");

            entity.HasIndex(e => e.CompanyId, "IX_CannedResponses_CompanyId");

            entity.HasIndex(e => e.CreatedByUserId, "IX_CannedResponses_CreatedByUserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Category).WithMany(p => p.CannedResponses).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Company).WithMany(p => p.CannedResponses).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CannedResponses).HasForeignKey(d => d.CreatedByUserId);
        });

        modelBuilder.Entity<CategorySla>(entity =>
        {
            entity.ToTable("CategorySLAs");

            entity.HasIndex(e => e.CategoryId, "IX_CategorySLAs_CategoryId").IsUnique();

            entity.HasIndex(e => new { e.CategoryId, e.SlalevelId }, "IX_CategorySLAs_CategoryId_SLALevelId");

            entity.HasIndex(e => e.IsActive, "IX_CategorySLAs_IsActive");

            entity.HasIndex(e => e.SlalevelId, "IX_CategorySLAs_SLALevelId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SlalevelId).HasColumnName("SLALevelId");

            entity.HasOne(d => d.Category).WithOne(p => p.CategorySla).HasForeignKey<CategorySla>(d => d.CategoryId);

            entity.HasOne(d => d.Slalevel).WithMany(p => p.CategorySlas)
                .HasForeignKey(d => d.SlalevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CategoryWorkflow>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_CategoryWorkflows_CategoryId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.CategoryId, e.IsDefault }, "IX_CategoryWorkflows_CategoryId_IsDefault").HasFilter("([IsDeleted]=(0) AND [IsActive]=(1))");

            entity.HasIndex(e => e.CompanyId, "IX_CategoryWorkflows_CompanyId").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryWorkflows)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.CategoryWorkflows).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<CategoryWorkflowStatus>(entity =>
        {
            entity.HasIndex(e => e.StatusMasterId, "IX_CategoryWorkflowStatuses_StatusMasterId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.WorkflowId, "IX_CategoryWorkflowStatuses_WorkflowId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.WorkflowId, e.DisplayOrder }, "IX_CategoryWorkflowStatuses_WorkflowId_DisplayOrder").HasFilter("([IsDeleted]=(0) AND [IsActive]=(1))");

            entity.HasIndex(e => new { e.WorkflowId, e.StatusMasterId }, "IX_CategoryWorkflowStatuses_WorkflowId_StatusMasterId")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AllowedRoles).HasMaxLength(2000);
            entity.Property(e => e.DefaultSlahours).HasColumnName("DefaultSLAHours");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.StatusMaster).WithMany(p => p.CategoryWorkflowStatuses)
                .HasForeignKey(d => d.StatusMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Workflow).WithMany(p => p.CategoryWorkflowStatuses).HasForeignKey(d => d.WorkflowId);
        });

        modelBuilder.Entity<CategoryWorkflowTransition>(entity =>
        {
            entity.HasIndex(e => e.FromStatusId, "IX_CategoryWorkflowTransitions_FromStatusId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ToStatusId, "IX_CategoryWorkflowTransitions_ToStatusId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.WorkflowId, "IX_CategoryWorkflowTransitions_WorkflowId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.WorkflowId, e.FromStatusId }, "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId").HasFilter("([IsDeleted]=(0) AND [IsActive]=(1))");

            entity.HasIndex(e => new { e.WorkflowId, e.FromStatusId, e.ToStatusId }, "IX_CategoryWorkflowTransitions_WorkflowId_FromStatusId_ToStatusId")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AllowedRoles).HasMaxLength(2000);
            entity.Property(e => e.ButtonColor).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TransitionConditions).HasMaxLength(4000);
            entity.Property(e => e.TransitionName).HasMaxLength(200);

            entity.HasOne(d => d.FromStatus).WithMany(p => p.CategoryWorkflowTransitionFromStatuses)
                .HasForeignKey(d => d.FromStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ToStatus).WithMany(p => p.CategoryWorkflowTransitionToStatuses)
                .HasForeignKey(d => d.ToStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Workflow).WithMany(p => p.CategoryWorkflowTransitions).HasForeignKey(d => d.WorkflowId);
        });

        modelBuilder.Entity<CommunicationLog>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_CommunicationLogs_CompanyId");

            entity.HasIndex(e => new { e.EntityId, e.EntityType, e.IsDeleted }, "IX_CommunicationLogs_Entity");

            entity.HasIndex(e => new { e.EntityId, e.EntityType }, "IX_CommunicationLogs_EntityId_EntityType");

            entity.HasIndex(e => e.RecipientUserId, "IX_CommunicationLogs_RecipientUserId");

            entity.HasIndex(e => e.SentAt, "IX_CommunicationLogs_SentAt");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_CommunicationLogs_Status_CreatedAt");

            entity.HasIndex(e => e.TemplateId, "IX_CommunicationLogs_TemplateId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.ExternalMessageId).HasMaxLength(255);
            entity.Property(e => e.RecipientEmail).HasMaxLength(255);
            entity.Property(e => e.RecipientPhone).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.CommunicationLogs).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.RecipientUser).WithMany(p => p.CommunicationLogs)
                .HasForeignKey(d => d.RecipientUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Template).WithMany(p => p.CommunicationLogs)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CommunicationTemplate>(entity =>
        {
            entity.HasIndex(e => e.Category, "IX_CommunicationTemplates_Category");

            entity.HasIndex(e => new { e.Channel, e.IsActive }, "IX_CommunicationTemplates_Channel_IsActive");

            entity.HasIndex(e => e.Code, "IX_CommunicationTemplates_Code").IsUnique();

            entity.HasIndex(e => e.CompanyId, "IX_CommunicationTemplates_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.CommunicationTemplates).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.HrResponsibleId, "IX_Companies_HrResponsibleId");

            entity.HasIndex(e => e.ManagerId, "IX_Companies_ManagerId");

            entity.HasIndex(e => e.OryggiCompanyId, "IX_Companies_OryggiCompanyId");

            entity.HasIndex(e => e.SecondaryManagerId, "IX_Companies_SecondaryManagerId");

            entity.HasIndex(e => new { e.TenantId, e.Code }, "IX_Companies_TenantId_Code").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.DefaultTimeZone).HasDefaultValue("Asia/Kolkata");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiCompanyId).HasMaxLength(100);

            entity.HasOne(d => d.HrResponsible).WithMany(p => p.CompanyHrResponsibles).HasForeignKey(d => d.HrResponsibleId);

            entity.HasOne(d => d.Manager).WithMany(p => p.CompanyManagers).HasForeignKey(d => d.ManagerId);

            entity.HasOne(d => d.SecondaryManager).WithMany(p => p.CompanySecondaryManagers).HasForeignKey(d => d.SecondaryManagerId);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Companies)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasIndex(e => new { e.AssignedToId, e.IsDeleted }, "IX_Complaints_AssignedTo");

            entity.HasIndex(e => e.AssignedToId, "IX_Complaints_AssignedToId");

            entity.HasIndex(e => e.BranchId, "IX_Complaints_BranchId");

            entity.HasIndex(e => e.CategoryId, "IX_Complaints_CategoryId");

            entity.HasIndex(e => new { e.CompanyId, e.StatusMasterId }, "IX_Complaints_CompanyId_StatusMasterId");

            entity.HasIndex(e => new { e.ComplainantId, e.IsDeleted }, "IX_Complaints_Complainant");

            entity.HasIndex(e => e.ComplainantId, "IX_Complaints_ComplainantId");

            entity.HasIndex(e => e.ComplaintNumber, "IX_Complaints_ComplaintNumber").IsUnique();

            entity.HasIndex(e => e.CurrentEscalationLevel, "IX_Complaints_CurrentEscalationLevel");

            entity.HasIndex(e => e.DepartmentId, "IX_Complaints_DepartmentId");

            entity.HasIndex(e => e.DueDate, "IX_Complaints_DueDate");

            entity.HasIndex(e => e.PriorityMasterId, "IX_Complaints_PriorityMasterId");

            entity.HasIndex(e => e.RelatedComplaintId, "IX_Complaints_RelatedComplaintId");

            entity.HasIndex(e => e.ResourcePoolId, "IX_Complaints_ResourcePoolId");

            entity.HasIndex(e => new { e.DueDate, e.StatusMasterId, e.IsDeleted }, "IX_Complaints_SLA");

            entity.HasIndex(e => new { e.CompanyId, e.StatusMasterId, e.PriorityMasterId, e.IsDeleted }, "IX_Complaints_Search");

            entity.HasIndex(e => e.SectionId, "IX_Complaints_SectionId");

            entity.HasIndex(e => e.StatusMasterId, "IX_Complaints_StatusMasterId");

            entity.HasIndex(e => e.SubmittedAt, "IX_Complaints_SubmittedAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ComplaintNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(4000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(d => d.AssignedTo).WithMany(p => p.ComplaintAssignedTos)
                .HasForeignKey(d => d.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Branch).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Complainant).WithMany(p => p.ComplaintComplainants)
                .HasForeignKey(d => d.ComplainantId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Department).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.PriorityMaster).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.PriorityMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.RelatedComplaint).WithMany(p => p.InverseRelatedComplaint).HasForeignKey(d => d.RelatedComplaintId);

            entity.HasOne(d => d.ResourcePool).WithMany(p => p.Complaints).HasForeignKey(d => d.ResourcePoolId);

            entity.HasOne(d => d.Section).WithMany(p => p.Complaints).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.StatusMaster).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.StatusMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ComplaintAttachment>(entity =>
        {
            entity.HasIndex(e => new { e.ComplaintId, e.IsDeleted }, "IX_ComplaintAttachments_Complaint");

            entity.HasIndex(e => e.ComplaintId, "IX_ComplaintAttachments_ComplaintId");

            entity.HasIndex(e => e.UploadedAt, "IX_ComplaintAttachments_UploadedAt");

            entity.HasIndex(e => e.UploadedBy, "IX_ComplaintAttachments_UploadedBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FileExtension).HasMaxLength(20);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.StoredFileName).HasMaxLength(255);

            entity.HasOne(d => d.Complaint).WithMany(p => p.ComplaintAttachments).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.ComplaintAttachments)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ComplaintCategory>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_ComplaintCategories_Code").IsUnique();

            entity.HasIndex(e => e.DisplayOrder, "IX_ComplaintCategories_DisplayOrder");

            entity.HasIndex(e => e.ParentCategoryId, "IX_ComplaintCategories_ParentCategoryId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasForeignKey(d => d.ParentCategoryId);
        });

        modelBuilder.Entity<ComplaintComment>(entity =>
        {
            entity.HasIndex(e => e.CommentedAt, "IX_ComplaintComments_CommentedAt");

            entity.HasIndex(e => e.CommentedBy, "IX_ComplaintComments_CommentedBy");

            entity.HasIndex(e => new { e.ComplaintId, e.IsDeleted }, "IX_ComplaintComments_Complaint");

            entity.HasIndex(e => e.ComplaintId, "IX_ComplaintComments_ComplaintId");

            entity.HasIndex(e => e.IsInternal, "IX_ComplaintComments_IsInternal");

            entity.HasIndex(e => e.ParentCommentId, "IX_ComplaintComments_ParentCommentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CommentText).HasMaxLength(4000);

            entity.HasOne(d => d.CommentedByNavigation).WithMany(p => p.ComplaintComments)
                .HasForeignKey(d => d.CommentedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Complaint).WithMany(p => p.ComplaintComments).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment).HasForeignKey(d => d.ParentCommentId);
        });

        modelBuilder.Entity<ComplaintEmailParticipant>(entity =>
        {
            entity.HasIndex(e => e.AddedByUserId, "IX_ComplaintEmailParticipants_AddedByUserId");

            entity.HasIndex(e => e.ComplaintId, "IX_ComplaintEmailParticipants_ComplaintId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.AddedByUser).WithMany(p => p.ComplaintEmailParticipants).HasForeignKey(d => d.AddedByUserId);

            entity.HasOne(d => d.Complaint).WithMany(p => p.ComplaintEmailParticipants).HasForeignKey(d => d.ComplaintId);
        });

        modelBuilder.Entity<ComplaintInformationSetting>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_ComplaintInformationSettings_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Company).WithMany(p => p.ComplaintInformationSettings).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<ComplaintNumberSequence>(entity =>
        {
            entity.HasKey(e => e.Year);

            entity.Property(e => e.Year).ValueGeneratedNever();
        });

        modelBuilder.Entity<ComplaintPriorityMaster>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_ComplaintPriorityMasters_Code")
                .IsUnique()
                .HasFilter("([CompanyId] IS NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => new { e.Code, e.CompanyId }, "IX_ComplaintPriorityMasters_Code_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => e.CompanyId, "IX_ComplaintPriorityMasters_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.ComplaintPriorityMasters).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<ComplaintRole>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_ComplaintRoles_Code").IsUnique();

            entity.HasIndex(e => e.EscalationLevel, "IX_ComplaintRoles_EscalationLevel");

            entity.HasIndex(e => e.RoleType, "IX_ComplaintRoles_RoleType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.RoleType).HasMaxLength(50);
        });

        modelBuilder.Entity<ComplaintRolePermission>(entity =>
        {
            entity.HasIndex(e => e.ComplaintRoleId, "IX_ComplaintRolePermissions_ComplaintRoleId");

            entity.HasIndex(e => new { e.ComplaintRoleId, e.PermissionType }, "IX_ComplaintRolePermissions_ComplaintRoleId_PermissionType").IsUnique();

            entity.HasIndex(e => e.PermissionType, "IX_ComplaintRolePermissions_PermissionType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PermissionType).HasMaxLength(100);

            entity.HasOne(d => d.ComplaintRole).WithMany(p => p.ComplaintRolePermissions).HasForeignKey(d => d.ComplaintRoleId);
        });

        modelBuilder.Entity<ComplaintStatusMaster>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_ComplaintStatusMasters_Code")
                .IsUnique()
                .HasFilter("([CompanyId] IS NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => new { e.Code, e.CompanyId }, "IX_ComplaintStatusMasters_Code_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => e.CompanyId, "IX_ComplaintStatusMasters_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.ComplaintStatusMasters).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<CustomFieldDefinition>(entity =>
        {
            entity.ToTable("CustomFieldDefinition");

            entity.HasIndex(e => e.CompanyId, "IX_CustomFieldDefinition_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Company).WithMany(p => p.CustomFieldDefinitions).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<CustomFieldValue>(entity =>
        {
            entity.ToTable("CustomFieldValue");

            entity.HasIndex(e => e.ComplaintId, "IX_CustomFieldValue_ComplaintId");

            entity.HasIndex(e => e.CustomFieldDefinitionId, "IX_CustomFieldValue_CustomFieldDefinitionId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NumericValue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Complaint).WithMany(p => p.CustomFieldValues).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.CustomFieldDefinition).WithMany(p => p.CustomFieldValues).HasForeignKey(d => d.CustomFieldDefinitionId);
        });

        modelBuilder.Entity<DashboardPreference>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_DashboardPreferences_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithMany(p => p.DashboardPreferences).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.Code }, "IX_Departments_BranchId_Code").IsUnique();

            entity.HasIndex(e => e.HrResponsibleId, "IX_Departments_HrResponsibleId");

            entity.HasIndex(e => e.ManagerId, "IX_Departments_ManagerId");

            entity.HasIndex(e => e.OryggiDepartmentId, "IX_Departments_OryggiDepartmentId");

            entity.HasIndex(e => e.SecondaryManagerId, "IX_Departments_SecondaryManagerId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiDepartmentId).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.Departments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.HrResponsible).WithMany(p => p.DepartmentHrResponsibles).HasForeignKey(d => d.HrResponsibleId);

            entity.HasOne(d => d.Manager).WithMany(p => p.DepartmentManagers).HasForeignKey(d => d.ManagerId);

            entity.HasOne(d => d.SecondaryManager).WithMany(p => p.DepartmentSecondaryManagers).HasForeignKey(d => d.SecondaryManagerId);
        });

        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasIndex(e => e.EmailMessageId, "IX_EmailAttachments_EmailMessageId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.EmailMessage).WithMany(p => p.EmailAttachments).HasForeignKey(d => d.EmailMessageId);
        });

        modelBuilder.Entity<EmailConfiguration>(entity =>
        {
            entity.HasIndex(e => e.AutoAcknowledgementTemplateId1, "IX_EmailConfigurations_AutoAcknowledgementTemplateId1");

            entity.HasIndex(e => e.CompanyId, "IX_EmailConfigurations_CompanyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.OauthAccessToken).HasColumnName("OAuthAccessToken");
            entity.Property(e => e.OauthClientId).HasColumnName("OAuthClientId");
            entity.Property(e => e.OauthClientSecret).HasColumnName("OAuthClientSecret");
            entity.Property(e => e.OauthRefreshToken).HasColumnName("OAuthRefreshToken");
            entity.Property(e => e.OauthScopes).HasColumnName("OAuthScopes");
            entity.Property(e => e.OauthTenantId).HasColumnName("OAuthTenantId");
            entity.Property(e => e.OauthTokenExpiresAt).HasColumnName("OAuthTokenExpiresAt");
            entity.Property(e => e.OauthTokenRefreshIntervalMinutes).HasColumnName("OAuthTokenRefreshIntervalMinutes");
            entity.Property(e => e.SmtpSeparateOauthAccessToken).HasColumnName("SmtpSeparateOAuthAccessToken");
            entity.Property(e => e.SmtpSeparateOauthClientId).HasColumnName("SmtpSeparateOAuthClientId");
            entity.Property(e => e.SmtpSeparateOauthClientSecret).HasColumnName("SmtpSeparateOAuthClientSecret");
            entity.Property(e => e.SmtpSeparateOauthRefreshToken).HasColumnName("SmtpSeparateOAuthRefreshToken");
            entity.Property(e => e.SmtpSeparateOauthScopes).HasColumnName("SmtpSeparateOAuthScopes");
            entity.Property(e => e.SmtpSeparateOauthTenantId).HasColumnName("SmtpSeparateOAuthTenantId");
            entity.Property(e => e.SmtpSeparateOauthTokenExpiresAt).HasColumnName("SmtpSeparateOAuthTokenExpiresAt");

            entity.HasOne(d => d.AutoAcknowledgementTemplateId1Navigation).WithMany(p => p.EmailConfigurations).HasForeignKey(d => d.AutoAcknowledgementTemplateId1);

            entity.HasOne(d => d.Company).WithMany(p => p.EmailConfigurations).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_EmailMessages_CompanyId");

            entity.HasIndex(e => new { e.ComplaintId, e.IsDeleted }, "IX_EmailMessages_Complaint");

            entity.HasIndex(e => e.ComplaintId, "IX_EmailMessages_ComplaintId");

            entity.HasIndex(e => e.ReadByUserId, "IX_EmailMessages_ReadByUserId");

            entity.HasIndex(e => e.SentByUserId, "IX_EmailMessages_SentByUserId");

            entity.HasIndex(e => new { e.ThreadId, e.IsDeleted }, "IX_EmailMessages_Thread");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Company).WithMany(p => p.EmailMessages).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Complaint).WithMany(p => p.EmailMessages).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.ReadByUser).WithMany(p => p.EmailMessageReadByUsers).HasForeignKey(d => d.ReadByUserId);

            entity.HasOne(d => d.SentByUser).WithMany(p => p.EmailMessageSentByUsers).HasForeignKey(d => d.SentByUserId);
        });

        modelBuilder.Entity<EmailResponseHistory>(entity =>
        {
            entity.HasIndex(e => e.ComplaintId, "IX_EmailResponseHistories_ComplaintId");

            entity.HasIndex(e => new { e.ComplaintId, e.SentAt }, "IX_EmailResponseHistories_ComplaintId_SentAt");

            entity.HasIndex(e => e.DeliveryStatus, "IX_EmailResponseHistories_DeliveryStatus");

            entity.HasIndex(e => e.EmailMessageId, "IX_EmailResponseHistories_EmailMessageId");

            entity.HasIndex(e => e.SentAt, "IX_EmailResponseHistories_SentAt");

            entity.HasIndex(e => e.SentBy, "IX_EmailResponseHistories_SentBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AttachmentIds).HasMaxLength(2000);
            entity.Property(e => e.BlindCarbonCopy).HasMaxLength(1000);
            entity.Property(e => e.CarbonCopy).HasMaxLength(1000);
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Sent");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.IsHtml).HasDefaultValue(true);
            entity.Property(e => e.MessageId).HasMaxLength(500);
            entity.Property(e => e.SentTo).HasMaxLength(1000);
            entity.Property(e => e.Subject).HasMaxLength(500);

            entity.HasOne(d => d.Complaint).WithMany(p => p.EmailResponseHistories).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.EmailMessage).WithMany(p => p.EmailResponseHistories)
                .HasForeignKey(d => d.EmailMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.SentByNavigation).WithMany(p => p.EmailResponseHistories)
                .HasForeignKey(d => d.SentBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<EmailServerSetting>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_EmailServerSettings_CompanyId");

            entity.HasIndex(e => new { e.IsActive, e.IsDefault }, "IX_EmailServerSettings_IsActive_IsDefault");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FromEmail).HasMaxLength(255);
            entity.Property(e => e.FromName).HasMaxLength(200);
            entity.Property(e => e.Host).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OauthAccessToken).HasColumnName("OAuthAccessToken");
            entity.Property(e => e.OauthClientId).HasColumnName("OAuthClientId");
            entity.Property(e => e.OauthClientSecret).HasColumnName("OAuthClientSecret");
            entity.Property(e => e.OauthRefreshToken).HasColumnName("OAuthRefreshToken");
            entity.Property(e => e.OauthScopes).HasColumnName("OAuthScopes");
            entity.Property(e => e.OauthTenantId).HasColumnName("OAuthTenantId");
            entity.Property(e => e.OauthTokenExpiresAt).HasColumnName("OAuthTokenExpiresAt");
            entity.Property(e => e.OauthTokenRefreshIntervalMinutes).HasColumnName("OAuthTokenRefreshIntervalMinutes");
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.ReplyToEmail).HasMaxLength(255);
            entity.Property(e => e.TestNotes).HasMaxLength(1000);
            entity.Property(e => e.Username).HasMaxLength(255);

            entity.HasOne(d => d.Company).WithMany(p => p.EmailServerSettings).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.ManagerId, "IX_Employees_ManagerId");

            entity.HasIndex(e => e.SectionId, "IX_Employees_SectionId");

            entity.HasIndex(e => e.TenantId, "IX_Employees_TenantId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager).HasForeignKey(d => d.ManagerId);

            entity.HasOne(d => d.Section).WithMany(p => p.Employees).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Employees).HasForeignKey(d => d.TenantId);
        });

        modelBuilder.Entity<EmployeeType>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.Code }, "IX_EmployeeTypes_CompanyId_Code").IsUnique();

            entity.HasIndex(e => e.OryggiEmployeeTypeId, "IX_EmployeeTypes_OryggiEmployeeTypeId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiEmployeeTypeId).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.EmployeeTypes)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<EscalationHistory>(entity =>
        {
            entity.HasIndex(e => e.ComplaintId, "IX_EscalationHistories_ComplaintId");

            entity.HasIndex(e => new { e.ComplaintId, e.EscalatedAt }, "IX_EscalationHistories_ComplaintId_EscalatedAt");

            entity.HasIndex(e => new { e.ComplaintId, e.Level }, "IX_EscalationHistories_ComplaintId_Level");

            entity.HasIndex(e => e.EscalatedAt, "IX_EscalationHistories_EscalatedAt");

            entity.HasIndex(e => e.EscalatedBy, "IX_EscalationHistories_EscalatedBy");

            entity.HasIndex(e => e.EscalationLevelId, "IX_EscalationHistories_EscalationLevelId");

            entity.HasIndex(e => e.EscalationMatrixId, "IX_EscalationHistories_EscalationMatrixId");

            entity.HasIndex(e => e.FromUserId, "IX_EscalationHistories_FromUserId");

            entity.HasIndex(e => e.Status, "IX_EscalationHistories_Status");

            entity.HasIndex(e => e.ToUserId, "IX_EscalationHistories_ToUserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssignmentStrategy).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(4000);
            entity.Property(e => e.Reason).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Complaint).WithMany(p => p.EscalationHistories).HasForeignKey(d => d.ComplaintId);

            entity.HasOne(d => d.EscalatedByNavigation).WithMany(p => p.EscalationHistoryEscalatedByNavigations).HasForeignKey(d => d.EscalatedBy);

            entity.HasOne(d => d.EscalationLevel).WithMany(p => p.EscalationHistories)
                .HasForeignKey(d => d.EscalationLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EscalationMatrix).WithMany(p => p.EscalationHistories)
                .HasForeignKey(d => d.EscalationMatrixId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.FromUser).WithMany(p => p.EscalationHistoryFromUsers).HasForeignKey(d => d.FromUserId);

            entity.HasOne(d => d.ToUser).WithMany(p => p.EscalationHistoryToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<EscalationLevel>(entity =>
        {
            entity.HasIndex(e => e.AssignToUserId, "IX_EscalationLevels_AssignToUserId");

            entity.HasIndex(e => e.BranchId, "IX_EscalationLevels_BranchId");

            entity.HasIndex(e => e.DepartmentId, "IX_EscalationLevels_DepartmentId");

            entity.HasIndex(e => e.EscalationMatrixId, "IX_EscalationLevels_EscalationMatrixId");

            entity.HasIndex(e => new { e.EscalationMatrixId, e.Level }, "IX_EscalationLevels_EscalationMatrixId_Level");

            entity.HasIndex(e => e.HrContactId, "IX_EscalationLevels_HrContactId");

            entity.HasIndex(e => e.IsActive, "IX_EscalationLevels_IsActive");

            entity.HasIndex(e => e.PrimaryContactId, "IX_EscalationLevels_PrimaryContactId");

            entity.HasIndex(e => e.ResourcePoolId, "IX_EscalationLevels_ResourcePoolId");

            entity.HasIndex(e => e.SecondaryContactId, "IX_EscalationLevels_SecondaryContactId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssignToRole).HasMaxLength(50);
            entity.Property(e => e.AssignToUserIds).HasMaxLength(500);
            entity.Property(e => e.AssignmentStrategy).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EscalationMessage).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NotifyPreviousHandler).HasDefaultValue(true);
            entity.Property(e => e.SendNotification).HasDefaultValue(true);

            entity.HasOne(d => d.AssignToUser).WithMany(p => p.EscalationLevelAssignToUsers)
                .HasForeignKey(d => d.AssignToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Branch).WithMany(p => p.EscalationLevels).HasForeignKey(d => d.BranchId);

            entity.HasOne(d => d.Department).WithMany(p => p.EscalationLevels).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.EscalationMatrix).WithMany(p => p.EscalationLevels).HasForeignKey(d => d.EscalationMatrixId);

            entity.HasOne(d => d.HrContact).WithMany(p => p.EscalationLevelHrContacts).HasForeignKey(d => d.HrContactId);

            entity.HasOne(d => d.PrimaryContact).WithMany(p => p.EscalationLevelPrimaryContacts).HasForeignKey(d => d.PrimaryContactId);

            entity.HasOne(d => d.ResourcePool).WithMany(p => p.EscalationLevels).HasForeignKey(d => d.ResourcePoolId);

            entity.HasOne(d => d.SecondaryContact).WithMany(p => p.EscalationLevelSecondaryContacts).HasForeignKey(d => d.SecondaryContactId);
        });

        modelBuilder.Entity<EscalationMatrix>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_EscalationMatrices_BranchId");

            entity.HasIndex(e => e.CategoryId, "IX_EscalationMatrices_CategoryId");

            entity.HasIndex(e => e.CompanyId, "IX_EscalationMatrices_CompanyId");

            entity.HasIndex(e => new { e.CompanyId, e.IsActive, e.Priority }, "IX_EscalationMatrices_CompanyId_IsActive_Priority");

            entity.HasIndex(e => e.DepartmentId, "IX_EscalationMatrices_DepartmentId");

            entity.HasIndex(e => e.IsActive, "IX_EscalationMatrices_IsActive");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EnableAutoEscalation).HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.SendEmailNotifications).HasDefaultValue(true);

            entity.HasOne(d => d.Branch).WithMany(p => p.EscalationMatrices)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.EscalationMatrices)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.EscalationMatrices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Department).WithMany(p => p.EscalationMatrices)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EscalationPolicy>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_EscalationPolicies_BranchId");

            entity.HasIndex(e => e.CategoryId, "IX_EscalationPolicies_CategoryId");

            entity.HasIndex(e => new { e.CompanyId, e.BranchId, e.IsActive }, "IX_EscalationPolicies_CompanyId_BranchId_IsActive");

            entity.HasIndex(e => new { e.CompanyId, e.CategoryId, e.IsActive }, "IX_EscalationPolicies_CompanyId_CategoryId_IsActive");

            entity.HasIndex(e => new { e.CompanyId, e.DepartmentId, e.IsActive }, "IX_EscalationPolicies_CompanyId_DepartmentId_IsActive");

            entity.HasIndex(e => new { e.CompanyId, e.IsActive }, "IX_EscalationPolicies_CompanyId_IsActive");

            entity.HasIndex(e => new { e.CompanyId, e.SectionId, e.IsActive }, "IX_EscalationPolicies_CompanyId_SectionId_IsActive");

            entity.HasIndex(e => e.DefaultEscalationMatrixId, "IX_EscalationPolicies_DefaultEscalationMatrixId");

            entity.HasIndex(e => e.DepartmentId, "IX_EscalationPolicies_DepartmentId");

            entity.HasIndex(e => new { e.CompanyId, e.BranchId, e.DepartmentId, e.SectionId, e.CategoryId, e.IsActive }, "IX_EscalationPolicies_FullHierarchy");

            entity.HasIndex(e => e.SectionId, "IX_EscalationPolicies_SectionId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Branch).WithMany(p => p.EscalationPolicies).HasForeignKey(d => d.BranchId);

            entity.HasOne(d => d.Category).WithMany(p => p.EscalationPolicies).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Company).WithMany(p => p.EscalationPolicies)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DefaultEscalationMatrix).WithMany(p => p.EscalationPolicies).HasForeignKey(d => d.DefaultEscalationMatrixId);

            entity.HasOne(d => d.Department).WithMany(p => p.EscalationPolicies).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Section).WithMany(p => p.EscalationPolicies).HasForeignKey(d => d.SectionId);
        });

        modelBuilder.Entity<EventCommunicationRule>(entity =>
        {
            entity.HasIndex(e => e.Channel, "IX_EventCommunicationRules_Channel");

            entity.HasIndex(e => e.CompanyId, "IX_EventCommunicationRules_CompanyId");

            entity.HasIndex(e => new { e.EventTypeId, e.IsActive, e.IsDeleted }, "IX_EventCommunicationRules_Event");

            entity.HasIndex(e => e.EventTypeId, "IX_EventCommunicationRules_EventTypeId");

            entity.HasIndex(e => new { e.IsActive, e.Priority }, "IX_EventCommunicationRules_IsActive_Priority");

            entity.HasIndex(e => e.TemplateId, "IX_EventCommunicationRules_TemplateId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Company).WithMany(p => p.EventCommunicationRules).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.EventType).WithMany(p => p.EventCommunicationRules).HasForeignKey(d => d.EventTypeId);

            entity.HasOne(d => d.Template).WithMany(p => p.EventCommunicationRules)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasIndex(e => e.Category, "IX_EventTypes_Category");

            entity.HasIndex(e => e.Code, "IX_EventTypes_Code").IsUnique();

            entity.HasIndex(e => e.CompanyId, "IX_EventTypes_CompanyId");

            entity.HasIndex(e => new { e.EntityType, e.IsActive }, "IX_EventTypes_EntityType_IsActive");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Company).WithMany(p => p.EventTypes).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<ExternalUserMapping>(entity =>
        {
            entity.HasIndex(e => e.AuthenticationProviderId, "IX_ExternalUserMappings_AuthenticationProviderId");

            entity.HasIndex(e => e.ExternalUserId, "IX_ExternalUserMappings_ExternalUserId");

            entity.HasIndex(e => e.IsActive, "IX_ExternalUserMappings_IsActive");

            entity.HasIndex(e => new { e.AuthenticationProviderId, e.ExternalUserId }, "IX_ExternalUserMappings_ProviderId_ExternalUserId").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_ExternalUserMappings_UserId");

            entity.HasIndex(e => new { e.UserId, e.AuthenticationProviderId }, "IX_ExternalUserMappings_UserId_ProviderId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ExternalDisplayName).HasMaxLength(200);
            entity.Property(e => e.ExternalEmail).HasMaxLength(200);
            entity.Property(e => e.ExternalUserId).HasMaxLength(500);
            entity.Property(e => e.ExternalUsername).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastSyncDetails).HasMaxLength(1000);

            entity.HasOne(d => d.AuthenticationProvider).WithMany(p => p.ExternalUserMappings).HasForeignKey(d => d.AuthenticationProviderId);

            entity.HasOne(d => d.User).WithMany(p => p.ExternalUserMappings).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<OryggiConnectionSetting>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_OryggiConnectionSettings_TenantId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Tenant).WithMany(p => p.OryggiConnectionSettings).HasForeignKey(d => d.TenantId);
        });

        modelBuilder.Entity<PasswordAuditLog>(entity =>
        {
            entity.ToTable("PasswordAuditLog");

            entity.HasIndex(e => e.Action, "IX_PasswordAuditLog_Action");

            entity.HasIndex(e => e.CreatedAt, "IX_PasswordAuditLog_CreatedAt");

            entity.HasIndex(e => e.PerformedBy, "IX_PasswordAuditLog_PerformedBy");

            entity.HasIndex(e => e.Success, "IX_PasswordAuditLog_Success");

            entity.HasIndex(e => new { e.Success, e.CreatedAt }, "IX_PasswordAuditLog_Success_CreatedAt");

            entity.HasIndex(e => e.UserId, "IX_PasswordAuditLog_UserId");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_PasswordAuditLog_UserId_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.PasswordAuditLogPerformedByNavigations).HasForeignKey(d => d.PerformedBy);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordAuditLogUsers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("PasswordHistory");

            entity.HasIndex(e => e.CreatedAt, "IX_PasswordHistory_CreatedAt");

            entity.HasIndex(e => e.CreatedBy, "IX_PasswordHistory_CreatedBy");

            entity.HasIndex(e => e.UserId, "IX_PasswordHistory_UserId");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_PasswordHistory_UserId_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PasswordHistoryCreatedByNavigations).HasForeignKey(d => d.CreatedBy);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordHistoryUsers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<PasswordPolicy>(entity =>
        {
            entity.ToTable("PasswordPolicy");

            entity.HasIndex(e => e.CompanyId, "IX_PasswordPolicy_CompanyId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccountLockoutDurationMinutes).HasDefaultValue(15);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.EnablePasswordComplexity).HasDefaultValue(true);
            entity.Property(e => e.MaxFailedLoginAttempts).HasDefaultValue(5);
            entity.Property(e => e.MinimumLength).HasDefaultValue(8);
            entity.Property(e => e.PasswordExpirationDays).HasDefaultValue(90);
            entity.Property(e => e.PasswordExpirationWarningDays).HasDefaultValue(7);
            entity.Property(e => e.PasswordHistoryCount).HasDefaultValue(5);
            entity.Property(e => e.RequireDigit).HasDefaultValue(true);
            entity.Property(e => e.RequireLowercase).HasDefaultValue(true);
            entity.Property(e => e.RequireSpecialCharacter).HasDefaultValue(true);
            entity.Property(e => e.RequireUppercase).HasDefaultValue(true);
            entity.Property(e => e.SendPasswordExpirationEmails).HasDefaultValue(true);
            entity.Property(e => e.SendPasswordSetEmails).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithOne(p => p.PasswordPolicy).HasForeignKey<PasswordPolicy>(d => d.CompanyId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_PasswordResetTokens_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<PrioritySla>(entity =>
        {
            entity.ToTable("PrioritySLAs");

            entity.HasIndex(e => e.IsActive, "IX_PrioritySLAs_IsActive");

            entity.HasIndex(e => e.PriorityId, "IX_PrioritySLAs_PriorityId").IsUnique();

            entity.HasIndex(e => new { e.PriorityId, e.SlalevelId }, "IX_PrioritySLAs_PriorityId_SLALevelId");

            entity.HasIndex(e => e.SlalevelId, "IX_PrioritySLAs_SLALevelId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SlalevelId).HasColumnName("SLALevelId");

            entity.HasOne(d => d.Priority).WithOne(p => p.PrioritySla).HasForeignKey<PrioritySla>(d => d.PriorityId);

            entity.HasOne(d => d.Slalevel).WithMany(p => p.PrioritySlas)
                .HasForeignKey(d => d.SlalevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.ExpiresAt, "IX_RefreshTokens_ExpiresAt");

            entity.HasIndex(e => e.Token, "IX_RefreshTokens_Token").IsUnique();

            entity.HasIndex(e => e.TokenFamily, "IX_RefreshTokens_TokenFamily");

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.RevocationReason).HasMaxLength(200);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ResourcePool>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_ResourcePools_BranchId");

            entity.HasIndex(e => e.CompanyId, "IX_ResourcePools_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_ResourcePools_DepartmentId");

            entity.HasIndex(e => e.SectionId, "IX_ResourcePools_SectionId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Branch).WithMany(p => p.ResourcePools).HasForeignKey(d => d.BranchId);

            entity.HasOne(d => d.Company).WithMany(p => p.ResourcePools).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.Department).WithMany(p => p.ResourcePools).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Section).WithMany(p => p.ResourcePools).HasForeignKey(d => d.SectionId);
        });

        modelBuilder.Entity<ResourcePoolMember>(entity =>
        {
            entity.HasIndex(e => e.ResourcePoolId, "IX_ResourcePoolMembers_ResourcePoolId");

            entity.HasIndex(e => e.UserId, "IX_ResourcePoolMembers_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ResourcePool).WithMany(p => p.ResourcePoolMembers).HasForeignKey(d => d.ResourcePoolId);

            entity.HasOne(d => d.User).WithMany(p => p.ResourcePoolMembers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasIndex(e => new { e.DepartmentId, e.Code }, "IX_Sections_DepartmentId_Code")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.HeadId, "IX_Sections_HeadId");

            entity.HasIndex(e => e.HrResponsibleId, "IX_Sections_HrResponsibleId");

            entity.HasIndex(e => e.OryggiSectionId, "IX_Sections_OryggiSectionId");

            entity.HasIndex(e => e.SecondaryHeadId, "IX_Sections_SecondaryHeadId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiSectionId).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Sections)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Head).WithMany(p => p.SectionHeads).HasForeignKey(d => d.HeadId);

            entity.HasOne(d => d.HrResponsible).WithMany(p => p.SectionHrResponsibles).HasForeignKey(d => d.HrResponsibleId);

            entity.HasOne(d => d.SecondaryHead).WithMany(p => p.SectionSecondaryHeads).HasForeignKey(d => d.SecondaryHeadId);
        });

        modelBuilder.Entity<Slalevel>(entity =>
        {
            entity.ToTable("SLALevels");

            entity.HasIndex(e => e.CompanyId, "IX_SLALevels_CompanyId");

            entity.HasIndex(e => new { e.CompanyId, e.Name }, "IX_SLALevels_CompanyId_Name");

            entity.HasIndex(e => new { e.CompanyId, e.Order }, "IX_SLALevels_CompanyId_Order");

            entity.HasIndex(e => e.IsActive, "IX_SLALevels_IsActive");

            entity.HasIndex(e => e.Order, "IX_SLALevels_Order");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ColorCode)
                .HasMaxLength(7)
                .HasDefaultValue("#4CAF50");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ResolutionTimeUnit).HasMaxLength(20);
            entity.Property(e => e.ResponseTimeUnit).HasMaxLength(20);
        });

        modelBuilder.Entity<Slasetting>(entity =>
        {
            entity.ToTable("SLASettings");

            entity.HasIndex(e => e.CompanyId, "IX_SLASettings_CompanyId")
                .IsUnique()
                .HasFilter("([CompanyId] IS NOT NULL)");

            entity.HasIndex(e => e.IsEnabled, "IX_SLASettings_IsEnabled");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AutoEscalateOnBreach).HasDefaultValue(true);
            entity.Property(e => e.EscalationThresholdPercent).HasDefaultValue(80);
            entity.Property(e => e.ExcludeHolidays).HasDefaultValue(true);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.NotifyBeforeBreach).HasDefaultValue(true);
            entity.Property(e => e.NotifyBeforeBreachMinutes).HasDefaultValue(30);
            entity.Property(e => e.PauseSlaonPendingInfo)
                .HasDefaultValue(true)
                .HasColumnName("PauseSLAOnPendingInfo");
            entity.Property(e => e.Timezone)
                .HasMaxLength(100)
                .HasDefaultValue("UTC");
            entity.Property(e => e.WorkingDays)
                .HasMaxLength(50)
                .HasDefaultValue("1,2,3,4,5");
        });

        modelBuilder.Entity<SmsGatewaySetting>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_SmsGatewaySettings_CompanyId");

            entity.HasIndex(e => new { e.IsActive, e.IsDefault }, "IX_SmsGatewaySettings_IsActive_IsDefault");

            entity.HasIndex(e => e.Provider, "IX_SmsGatewaySettings_Provider");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccountSid).HasMaxLength(255);
            entity.Property(e => e.ApiUrl).HasMaxLength(500);
            entity.Property(e => e.AuthToken).HasMaxLength(500);
            entity.Property(e => e.CostPerSms).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.FromNumber).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.SenderName).HasMaxLength(100);
            entity.Property(e => e.TestNotes).HasMaxLength(1000);

            entity.HasOne(d => d.Company).WithMany(p => p.SmsGatewaySettings).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SyncSchedule>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_SyncSchedules_TenantId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ScheduleType).HasMaxLength(50);
            entity.Property(e => e.TimeOfDay).HasMaxLength(5);

            entity.HasOne(d => d.Tenant).WithMany(p => p.SyncSchedules).HasForeignKey(d => d.TenantId);
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_SystemConfigurations_CompanyId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AssignmentNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.AutoResponseEnabled).HasDefaultValue(true);
            entity.Property(e => e.AutoResponseMaxRetryAttempts).HasDefaultValue(3);
            entity.Property(e => e.AutoResponseRetryDelaySeconds).HasDefaultValue(60);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DateFormat)
                .HasMaxLength(50)
                .HasDefaultValue("dd/MM/yyyy");
            entity.Property(e => e.DefaultEmailPollingIntervalSeconds).HasDefaultValue(300);
            entity.Property(e => e.DefaultTimezone)
                .HasMaxLength(100)
                .HasDefaultValue("Asia/Kolkata");
            entity.Property(e => e.EmailRateLimitingEnabled).HasDefaultValue(true);
            entity.Property(e => e.EscalationNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.MaxEmailsFetchPerPoll).HasDefaultValue(50);
            entity.Property(e => e.MaxEmailsPerHour).HasDefaultValue(100);
            entity.Property(e => e.OauthTokenExpiryWarningDays)
                .HasDefaultValue(7)
                .HasColumnName("OAuthTokenExpiryWarningDays");
            entity.Property(e => e.OauthTokenRefreshIntervalMinutes)
                .HasDefaultValue(30)
                .HasColumnName("OAuthTokenRefreshIntervalMinutes");
            entity.Property(e => e.StatusChangeNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.TimeFormat)
                .HasMaxLength(50)
                .HasDefaultValue("hh:mm tt");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_Tenants_Code").IsUnique();

            entity.HasIndex(e => e.OryggiTenantId, "IX_Tenants_OryggiTenantId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OryggiTenantId).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.AccountLockedUntil, "IX_Users_AccountLockedUntil");

            entity.HasIndex(e => new { e.IsActive, e.IsDeleted }, "IX_Users_Active");

            entity.HasIndex(e => new { e.AuthenticationProviderType, e.ExternalUserId }, "IX_Users_AuthProviderType_ExternalUserId");

            entity.HasIndex(e => e.AuthenticationProviderType, "IX_Users_AuthenticationProviderType");

            entity.HasIndex(e => e.BranchId, "IX_Users_BranchId");

            entity.HasIndex(e => new { e.CompanyId, e.IsDeleted }, "IX_Users_Company");

            entity.HasIndex(e => e.CompanyId, "IX_Users_CompanyId");

            entity.HasIndex(e => e.DepartmentId, "IX_Users_DepartmentId");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.EmployeeCode, "IX_Users_EmployeeCode").IsUnique();

            entity.HasIndex(e => e.EmployeeTypeId, "IX_Users_EmployeeTypeId");

            entity.HasIndex(e => e.ExternalUserId, "IX_Users_ExternalUserId");

            entity.HasIndex(e => e.ManagerId, "IX_Users_ManagerId");

            entity.HasIndex(e => e.OryggiEmployeeId, "IX_Users_OryggiEmployeeId");

            entity.HasIndex(e => e.PasswordExpiresAt, "IX_Users_PasswordExpiresAt");

            entity.HasIndex(e => e.SectionId, "IX_Users_SectionId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AuthenticationProviderType)
                .HasMaxLength(50)
                .HasDefaultValue("Local");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.ExternalSyncEnabled).HasDefaultValue(true);
            entity.Property(e => e.ExternalUserId).HasMaxLength(500);
            entity.Property(e => e.ExternalUsername).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IdentityProvider).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(200);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.LocalPasswordEnabled).HasDefaultValue(true);
            entity.Property(e => e.OryggiEmployeeId).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PreferredAuthMethod).HasMaxLength(50);
            entity.Property(e => e.Ssoenabled)
                .HasDefaultValue(true)
                .HasColumnName("SSOEnabled");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.Users)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.EmployeeType).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager).HasForeignKey(d => d.ManagerId);

            entity.HasOne(d => d.Section).WithMany(p => p.Users)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserComplaintRole>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_UserComplaintRoles_BranchId");

            entity.HasIndex(e => e.CompanyId, "IX_UserComplaintRoles_CompanyId");

            entity.HasIndex(e => e.ComplaintRoleId, "IX_UserComplaintRoles_ComplaintRoleId");

            entity.HasIndex(e => e.DepartmentId, "IX_UserComplaintRoles_DepartmentId");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo }, "IX_UserComplaintRoles_EffectiveFrom_EffectiveTo");

            entity.HasIndex(e => e.SectionId, "IX_UserComplaintRoles_SectionId");

            entity.HasIndex(e => e.UserId, "IX_UserComplaintRoles_UserId");

            entity.HasIndex(e => new { e.UserId, e.ComplaintRoleId, e.IsActive }, "IX_UserComplaintRoles_UserId_ComplaintRoleId_IsActive");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(d => d.Branch).WithMany(p => p.UserComplaintRoles)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.UserComplaintRoles).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.ComplaintRole).WithMany(p => p.UserComplaintRoles)
                .HasForeignKey(d => d.ComplaintRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Department).WithMany(p => p.UserComplaintRoles)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Section).WithMany(p => p.UserComplaintRoles)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User).WithMany(p => p.UserComplaintRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<WhatsAppSetting>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_WhatsAppSettings_CompanyId");

            entity.HasIndex(e => new { e.IsActive, e.IsDefault }, "IX_WhatsAppSettings_IsActive_IsDefault");

            entity.HasIndex(e => e.Provider, "IX_WhatsAppSettings_Provider");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccessToken).HasMaxLength(1000);
            entity.Property(e => e.ApiUrl).HasMaxLength(500);
            entity.Property(e => e.BusinessAccountId).HasMaxLength(255);
            entity.Property(e => e.BusinessName).HasMaxLength(200);
            entity.Property(e => e.FromNumber).HasMaxLength(50);
            entity.Property(e => e.MaxMediaSizeMb).HasColumnName("MaxMediaSizeMB");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PhoneNumberId).HasMaxLength(255);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.TestNotes).HasMaxLength(1000);
            entity.Property(e => e.WebhookToken).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.WhatsAppSettings).HasForeignKey(d => d.CompanyId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
