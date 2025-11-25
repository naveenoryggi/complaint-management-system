namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the type of role in the complaint management system
/// </summary>
public enum RoleType
{
    /// <summary>
    /// System administrator with full access
    /// </summary>
    SystemAdmin = 0,

    /// <summary>
    /// Tenant administrator
    /// </summary>
    TenantAdmin = 1,

    /// <summary>
    /// Company administrator
    /// </summary>
    CompanyAdmin = 2,

    /// <summary>
    /// User who can submit complaints
    /// </summary>
    Complainant = 3,

    /// <summary>
    /// Level 1 complaint handler
    /// </summary>
    Level1Handler = 4,

    /// <summary>
    /// Level 2 complaint handler
    /// </summary>
    Level2Handler = 5,

    /// <summary>
    /// Level 3 complaint handler
    /// </summary>
    Level3Handler = 6,

    /// <summary>
    /// Level 4 complaint handler
    /// </summary>
    Level4Handler = 7,

    /// <summary>
    /// Level 5 complaint handler (highest escalation level)
    /// </summary>
    Level5Handler = 8,

    /// <summary>
    /// User who can view complaints in read-only mode
    /// </summary>
    Viewer = 9,

    /// <summary>
    /// HR representative who can manage complaints
    /// </summary>
    HRRepresentative = 10,

    /// <summary>
    /// Department manager
    /// </summary>
    DepartmentManager = 11,

    /// <summary>
    /// Reporting manager (team lead, direct supervisor)
    /// </summary>
    ReportingManager = 12,

    /// <summary>
    /// Primary contact for branch/department (main point of contact)
    /// </summary>
    PrimaryContact = 13,

    /// <summary>
    /// Secondary contact for branch/department (backup contact)
    /// </summary>
    SecondaryContact = 14,

    /// <summary>
    /// Custom role defined by organization
    /// </summary>
    Custom = 15
}
