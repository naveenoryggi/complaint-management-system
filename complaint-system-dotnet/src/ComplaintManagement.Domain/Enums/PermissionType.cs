namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the type of permission a role can have
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// View complaints
    /// </summary>
    ViewComplaints = 0,

    /// <summary>
    /// Create/submit new complaints
    /// </summary>
    CreateComplaint = 1,

    /// <summary>
    /// Edit complaint details
    /// </summary>
    EditComplaint = 2,

    /// <summary>
    /// Delete complaints
    /// </summary>
    DeleteComplaint = 3,

    /// <summary>
    /// Assign complaints to handlers
    /// </summary>
    AssignComplaint = 4,

    /// <summary>
    /// Escalate complaints to higher level
    /// </summary>
    EscalateComplaint = 5,

    /// <summary>
    /// Close/resolve complaints
    /// </summary>
    CloseComplaint = 6,

    /// <summary>
    /// Reopen closed complaints
    /// </summary>
    ReopenComplaint = 7,

    /// <summary>
    /// Add comments to complaints
    /// </summary>
    AddComment = 8,

    /// <summary>
    /// View all comments
    /// </summary>
    ViewComments = 9,

    /// <summary>
    /// Attach files to complaints
    /// </summary>
    AddAttachment = 10,

    /// <summary>
    /// View attachments
    /// </summary>
    ViewAttachments = 11,

    /// <summary>
    /// Manage complaint categories
    /// </summary>
    ManageCategories = 12,

    /// <summary>
    /// Manage roles and permissions
    /// </summary>
    ManageRoles = 13,

    /// <summary>
    /// View reports and analytics
    /// </summary>
    ViewReports = 14,

    /// <summary>
    /// Manage system settings
    /// </summary>
    ManageSettings = 15,

    /// <summary>
    /// Manage users in the system
    /// </summary>
    ManageUsers = 16,

    /// <summary>
    /// View audit logs
    /// </summary>
    ViewAuditLogs = 17,

    /// <summary>
    /// View escalation configuration (matrices, policies, resource pools)
    /// </summary>
    ViewEscalation = 18,

    /// <summary>
    /// Manage escalation configuration (matrices, policies, resource pools)
    /// </summary>
    ManageEscalation = 19,

    /// <summary>
    /// Manage company settings and configuration
    /// </summary>
    ManageCompany = 20,

    /// <summary>
    /// View SLA (Service Level Agreement) settings and metrics
    /// </summary>
    ViewSLA = 21,

    /// <summary>
    /// Manage SLA (Service Level Agreement) configuration
    /// </summary>
    ManageSLA = 22,

    /// <summary>
    /// Create SLA (Service Level Agreement) levels and configurations
    /// </summary>
    CreateSLA = 23,

    /// <summary>
    /// Update SLA (Service Level Agreement) levels and configurations
    /// </summary>
    UpdateSLA = 24,

    /// <summary>
    /// Delete SLA (Service Level Agreement) levels and configurations
    /// </summary>
    DeleteSLA = 25
}
