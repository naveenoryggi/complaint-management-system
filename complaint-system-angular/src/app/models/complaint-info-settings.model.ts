export interface ComplaintInformationSettings {
  id: string;
  companyId: string;
  companyName: string;

  // Handler visibility settings
  showEmployeeCodeToHandlers: boolean;
  showEmailToHandlers: boolean;
  showPhoneToHandlers: boolean;
  showBranchToHandlers: boolean;
  showDepartmentToHandlers: boolean;
  showSectionToHandlers: boolean;
  showJobTitleToHandlers: boolean;
  showManagerDetailsToHandlers: boolean;
  showPreviousComplaintsToHandlers: boolean;

  // Management visibility settings
  showEmployeeAddressToManagement: boolean;
  showEmergencyContactToManagement: boolean;
  showPerformanceMetricsToManagement: boolean;

  // Privacy settings
  maskPersonalInfoInLogs: boolean;
  redactInfoAfterClosure: boolean;
  dataRetentionDays: number;

  // Report settings
  includeEmployeeCodeInReports: boolean;
  includeEmailInReports: boolean;
  includePhoneInReports: boolean;
  maskEmailInReports: boolean;
  maskPhoneInReports: boolean;
}

export interface UpdateComplaintInfoSettingsRequest {
  // Handler visibility settings
  showEmployeeCodeToHandlers: boolean;
  showEmailToHandlers: boolean;
  showPhoneToHandlers: boolean;
  showBranchToHandlers: boolean;
  showDepartmentToHandlers: boolean;
  showSectionToHandlers: boolean;
  showJobTitleToHandlers: boolean;
  showManagerDetailsToHandlers: boolean;
  showPreviousComplaintsToHandlers: boolean;

  // Management visibility settings
  showEmployeeAddressToManagement: boolean;
  showEmergencyContactToManagement: boolean;
  showPerformanceMetricsToManagement: boolean;

  // Privacy settings
  maskPersonalInfoInLogs: boolean;
  redactInfoAfterClosure: boolean;
  dataRetentionDays: number;

  // Report settings
  includeEmployeeCodeInReports: boolean;
  includeEmailInReports: boolean;
  includePhoneInReports: boolean;
  maskEmailInReports: boolean;
  maskPhoneInReports: boolean;
}
