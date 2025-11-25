using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Company
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public string? OryggiCompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public string? LogoContentType { get; set; }

    public string? LogoFileName { get; set; }

    public string? LogoUrl { get; set; }

    public Guid? ManagerId { get; set; }

    public Guid? SecondaryManagerId { get; set; }

    public Guid? HrResponsibleId { get; set; }

    public string? DefaultLocale { get; set; }

    public string DefaultTimeZone { get; set; } = null!;

    public virtual ICollection<AuthenticationProvider> AuthenticationProviders { get; set; } = new List<AuthenticationProvider>();

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public virtual ICollection<CannedResponse> CannedResponses { get; set; } = new List<CannedResponse>();

    public virtual ICollection<CategoryWorkflow> CategoryWorkflows { get; set; } = new List<CategoryWorkflow>();

    public virtual ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();

    public virtual ICollection<CommunicationTemplate> CommunicationTemplates { get; set; } = new List<CommunicationTemplate>();

    public virtual ICollection<ComplaintInformationSetting> ComplaintInformationSettings { get; set; } = new List<ComplaintInformationSetting>();

    public virtual ICollection<ComplaintPriorityMaster> ComplaintPriorityMasters { get; set; } = new List<ComplaintPriorityMaster>();

    public virtual ICollection<ComplaintStatusMaster> ComplaintStatusMasters { get; set; } = new List<ComplaintStatusMaster>();

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual ICollection<CustomFieldDefinition> CustomFieldDefinitions { get; set; } = new List<CustomFieldDefinition>();

    public virtual ICollection<EmailConfiguration> EmailConfigurations { get; set; } = new List<EmailConfiguration>();

    public virtual ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();

    public virtual ICollection<EmailServerSetting> EmailServerSettings { get; set; } = new List<EmailServerSetting>();

    public virtual ICollection<EmployeeType> EmployeeTypes { get; set; } = new List<EmployeeType>();

    public virtual ICollection<EscalationMatrix> EscalationMatrices { get; set; } = new List<EscalationMatrix>();

    public virtual ICollection<EscalationPolicy> EscalationPolicies { get; set; } = new List<EscalationPolicy>();

    public virtual ICollection<EventCommunicationRule> EventCommunicationRules { get; set; } = new List<EventCommunicationRule>();

    public virtual ICollection<EventType> EventTypes { get; set; } = new List<EventType>();

    public virtual User? HrResponsible { get; set; }

    public virtual User? Manager { get; set; }

    public virtual PasswordPolicy? PasswordPolicy { get; set; }

    public virtual ICollection<ResourcePool> ResourcePools { get; set; } = new List<ResourcePool>();

    public virtual User? SecondaryManager { get; set; }

    public virtual ICollection<SmsGatewaySetting> SmsGatewaySettings { get; set; } = new List<SmsGatewaySetting>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<WhatsAppSetting> WhatsAppSettings { get; set; } = new List<WhatsAppSetting>();
}
