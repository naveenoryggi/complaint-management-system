using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Settings;
using MediatR;

namespace ComplaintManagement.Application.Features.Settings.Commands;

/// <summary>
/// Command to update complaint information settings
/// </summary>
public class UpdateComplaintInfoSettingsCommand : IRequest<Result<ComplaintInformationSettingsDto>>
{
    public Guid CompanyId { get; set; }

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
}
