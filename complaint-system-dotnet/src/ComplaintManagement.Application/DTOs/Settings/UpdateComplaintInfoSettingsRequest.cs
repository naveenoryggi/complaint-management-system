using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Settings;

/// <summary>
/// Request DTO for updating complaint information settings
/// </summary>
public class UpdateComplaintInfoSettingsRequest
{
    // Handler visibility settings
    public bool ShowEmployeeCodeToHandlers { get; set; }
    public bool ShowEmailToHandlers { get; set; }
    public bool ShowPhoneToHandlers { get; set; }
    public bool ShowBranchToHandlers { get; set; }
    public bool ShowDepartmentToHandlers { get; set; }
    public bool ShowSectionToHandlers { get; set; }
    public bool ShowJobTitleToHandlers { get; set; }
    public bool ShowManagerDetailsToHandlers { get; set; }
    public bool ShowPreviousComplaintsToHandlers { get; set; }

    // Management visibility settings
    public bool ShowEmployeeAddressToManagement { get; set; }
    public bool ShowEmergencyContactToManagement { get; set; }
    public bool ShowPerformanceMetricsToManagement { get; set; }

    // Privacy settings
    public bool MaskPersonalInfoInLogs { get; set; }
    public bool RedactInfoAfterClosure { get; set; }
    public int DataRetentionDays { get; set; }

    // Report settings
    public bool IncludeEmployeeCodeInReports { get; set; }
    public bool IncludeEmailInReports { get; set; }
    public bool IncludePhoneInReports { get; set; }
    public bool MaskEmailInReports { get; set; }
    public bool MaskPhoneInReports { get; set; }

    // Attachment settings
    /// <summary>
    /// Maximum number of attachments allowed per complaint (must be between 1 and 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Max attachments must be between 1 and 100")]
    public int? MaxAttachments { get; set; }
}
