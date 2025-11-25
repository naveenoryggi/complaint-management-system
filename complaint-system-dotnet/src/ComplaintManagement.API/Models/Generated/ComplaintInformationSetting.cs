using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintInformationSetting
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public bool ShowEmployeeCodeToHandlers { get; set; }

    public bool ShowEmailToHandlers { get; set; }

    public bool ShowPhoneToHandlers { get; set; }

    public bool ShowAlternatePhoneToHandlers { get; set; }

    public bool ShowCompanyToHandlers { get; set; }

    public bool ShowBranchToHandlers { get; set; }

    public bool ShowDepartmentToHandlers { get; set; }

    public bool ShowSectionToHandlers { get; set; }

    public bool ShowJobTitleToHandlers { get; set; }

    public bool ShowManagerDetailsToHandlers { get; set; }

    public bool ShowDateOfJoiningToHandlers { get; set; }

    public bool ShowPreviousComplaintsToHandlers { get; set; }

    public bool ShowEmployeeAddressToManagement { get; set; }

    public bool ShowEmergencyContactToManagement { get; set; }

    public bool ShowPerformanceMetricsToManagement { get; set; }

    public bool MaskPersonalInfoInLogs { get; set; }

    public bool RedactInfoAfterClosure { get; set; }

    public int DataRetentionDays { get; set; }

    public bool IncludeEmployeeCodeInReports { get; set; }

    public bool IncludeEmailInReports { get; set; }

    public bool IncludePhoneInReports { get; set; }

    public bool MaskEmailInReports { get; set; }

    public bool MaskPhoneInReports { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company Company { get; set; } = null!;
}
