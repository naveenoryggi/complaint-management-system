using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Settings;

/// <summary>
/// Settings for controlling what complainant information is visible to handlers, management, and in reports
/// </summary>
public class ComplaintInformationSettings : BaseEntity
{
    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    // Visibility for Handlers
    /// <summary>
    /// Show employee code to handlers
    /// </summary>
    public bool ShowEmployeeCodeToHandlers { get; set; } = true;

    /// <summary>
    /// Show email address to handlers
    /// </summary>
    public bool ShowEmailToHandlers { get; set; } = true;

    /// <summary>
    /// Show phone number to handlers
    /// </summary>
    public bool ShowPhoneToHandlers { get; set; } = true;

    /// <summary>
    /// Show alternate phone to handlers
    /// </summary>
    public bool ShowAlternatePhoneToHandlers { get; set; } = true;

    /// <summary>
    /// Show company name to handlers
    /// </summary>
    public bool ShowCompanyToHandlers { get; set; } = true;

    /// <summary>
    /// Show branch name to handlers
    /// </summary>
    public bool ShowBranchToHandlers { get; set; } = true;

    /// <summary>
    /// Show department name to handlers
    /// </summary>
    public bool ShowDepartmentToHandlers { get; set; } = true;

    /// <summary>
    /// Show section name to handlers
    /// </summary>
    public bool ShowSectionToHandlers { get; set; } = true;

    /// <summary>
    /// Show job title to handlers
    /// </summary>
    public bool ShowJobTitleToHandlers { get; set; } = true;

    /// <summary>
    /// Show manager details to handlers
    /// </summary>
    public bool ShowManagerDetailsToHandlers { get; set; } = true;

    /// <summary>
    /// Show date of joining to handlers
    /// </summary>
    public bool ShowDateOfJoiningToHandlers { get; set; } = false;

    /// <summary>
    /// Show previous complaints history to handlers
    /// </summary>
    public bool ShowPreviousComplaintsToHandlers { get; set; } = true;

    // Visibility for Management
    /// <summary>
    /// Show employee address to management
    /// </summary>
    public bool ShowEmployeeAddressToManagement { get; set; } = true;

    /// <summary>
    /// Show emergency contact to management
    /// </summary>
    public bool ShowEmergencyContactToManagement { get; set; } = true;

    /// <summary>
    /// Show performance metrics to management
    /// </summary>
    public bool ShowPerformanceMetricsToManagement { get; set; } = false;

    // Privacy Settings
    /// <summary>
    /// Mask personal information in logs and exports
    /// </summary>
    public bool MaskPersonalInfoInLogs { get; set; } = true;

    /// <summary>
    /// Redact complainant information after complaint closure
    /// </summary>
    public bool RedactInfoAfterClosure { get; set; } = false;

    /// <summary>
    /// Number of days to retain complaint data after closure (0 = retain indefinitely)
    /// </summary>
    public int DataRetentionDays { get; set; } = 0;

    // Report Settings
    /// <summary>
    /// Include employee code in reports
    /// </summary>
    public bool IncludeEmployeeCodeInReports { get; set; } = true;

    /// <summary>
    /// Include email in reports
    /// </summary>
    public bool IncludeEmailInReports { get; set; } = false;

    /// <summary>
    /// Include phone in reports
    /// </summary>
    public bool IncludePhoneInReports { get; set; } = false;

    /// <summary>
    /// Mask email addresses in reports (show as jo**@domain.com)
    /// </summary>
    public bool MaskEmailInReports { get; set; } = true;

    /// <summary>
    /// Mask phone numbers in reports (show as +91-XXX-XXX-1234)
    /// </summary>
    public bool MaskPhoneInReports { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Parent company
    /// </summary>
    public Company Company { get; set; } = null!;
}
