using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class User
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? SectionId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? JobTitle { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string? OryggiEmployeeId { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Guid? EmployeeTypeId { get; set; }

    public string? AlternatePhone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime? DateOfJoining { get; set; }

    public DateTime? AccountLockedUntil { get; set; }

    public string AuthenticationProviderType { get; set; } = null!;

    public bool ExternalSyncEnabled { get; set; }

    public string? ExternalUserId { get; set; }

    public string? ExternalUsername { get; set; }

    public int FailedLoginAttempts { get; set; }

    public string? IdentityProvider { get; set; }

    public DateTime? LastExternalSyncAt { get; set; }

    public DateTime? LastPasswordChangeRequiredNotificationSentAt { get; set; }

    public bool LocalPasswordEnabled { get; set; }

    public bool MustChangePasswordOnNextLogin { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public Guid? PasswordChangedBy { get; set; }

    public DateTime? PasswordExpiresAt { get; set; }

    public bool PasswordNeverExpires { get; set; }

    public string? PreferredAuthMethod { get; set; }

    public bool Ssoenabled { get; set; }

    public string? PreferredDateFormat { get; set; }

    public string? PreferredLocale { get; set; }

    public string? PreferredTimeFormat { get; set; }

    public string? PreferredTimeZone { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Branch> BranchHrResponsibles { get; set; } = new List<Branch>();

    public virtual ICollection<Branch> BranchManagers { get; set; } = new List<Branch>();

    public virtual ICollection<Branch> BranchSecondaryManagers { get; set; } = new List<Branch>();

    public virtual ICollection<CannedResponse> CannedResponses { get; set; } = new List<CannedResponse>();

    public virtual ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Company> CompanyHrResponsibles { get; set; } = new List<Company>();

    public virtual ICollection<Company> CompanyManagers { get; set; } = new List<Company>();

    public virtual ICollection<Company> CompanySecondaryManagers { get; set; } = new List<Company>();

    public virtual ICollection<Complaint> ComplaintAssignedTos { get; set; } = new List<Complaint>();

    public virtual ICollection<ComplaintAttachment> ComplaintAttachments { get; set; } = new List<ComplaintAttachment>();

    public virtual ICollection<ComplaintComment> ComplaintComments { get; set; } = new List<ComplaintComment>();

    public virtual ICollection<Complaint> ComplaintComplainants { get; set; } = new List<Complaint>();

    public virtual ICollection<ComplaintEmailParticipant> ComplaintEmailParticipants { get; set; } = new List<ComplaintEmailParticipant>();

    public virtual ICollection<DashboardPreference> DashboardPreferences { get; set; } = new List<DashboardPreference>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Department> DepartmentHrResponsibles { get; set; } = new List<Department>();

    public virtual ICollection<Department> DepartmentManagers { get; set; } = new List<Department>();

    public virtual ICollection<Department> DepartmentSecondaryManagers { get; set; } = new List<Department>();

    public virtual ICollection<EmailMessage> EmailMessageReadByUsers { get; set; } = new List<EmailMessage>();

    public virtual ICollection<EmailMessage> EmailMessageSentByUsers { get; set; } = new List<EmailMessage>();

    public virtual ICollection<EmailResponseHistory> EmailResponseHistories { get; set; } = new List<EmailResponseHistory>();

    public virtual EmployeeType? EmployeeType { get; set; }

    public virtual ICollection<EscalationHistory> EscalationHistoryEscalatedByNavigations { get; set; } = new List<EscalationHistory>();

    public virtual ICollection<EscalationHistory> EscalationHistoryFromUsers { get; set; } = new List<EscalationHistory>();

    public virtual ICollection<EscalationHistory> EscalationHistoryToUsers { get; set; } = new List<EscalationHistory>();

    public virtual ICollection<EscalationLevel> EscalationLevelAssignToUsers { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<EscalationLevel> EscalationLevelHrContacts { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<EscalationLevel> EscalationLevelPrimaryContacts { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<EscalationLevel> EscalationLevelSecondaryContacts { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<ExternalUserMapping> ExternalUserMappings { get; set; } = new List<ExternalUserMapping>();

    public virtual ICollection<User> InverseManager { get; set; } = new List<User>();

    public virtual User? Manager { get; set; }

    public virtual ICollection<PasswordAuditLog> PasswordAuditLogPerformedByNavigations { get; set; } = new List<PasswordAuditLog>();

    public virtual ICollection<PasswordAuditLog> PasswordAuditLogUsers { get; set; } = new List<PasswordAuditLog>();

    public virtual ICollection<PasswordHistory> PasswordHistoryCreatedByNavigations { get; set; } = new List<PasswordHistory>();

    public virtual ICollection<PasswordHistory> PasswordHistoryUsers { get; set; } = new List<PasswordHistory>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<ResourcePoolMember> ResourcePoolMembers { get; set; } = new List<ResourcePoolMember>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<Section> SectionHeads { get; set; } = new List<Section>();

    public virtual ICollection<Section> SectionHrResponsibles { get; set; } = new List<Section>();

    public virtual ICollection<Section> SectionSecondaryHeads { get; set; } = new List<Section>();

    public virtual ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();
}
