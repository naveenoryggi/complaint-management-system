// Asset Assignment Models

export interface AssetAssignment {
  id: string;
  companyId: string;
  assetId: string;
  assetTag: string;
  assetName: string;
  assetSerialNumber?: string;
  assetType: number;
  assetTypeName: string;

  // Internal Assignment (Employee)
  assignedToUserId?: string;
  assignedToUserName?: string;
  assignedToUserEmail?: string;
  assignedToEmployeeCode?: string;

  // External Assignment (Customer/SI)
  assignedToCustomerId?: string;
  assignedToCustomerName?: string;
  assignedToCustomerCode?: string;

  // Responsible User (for external assignments)
  responsibleUserId?: string;
  responsibleUserName?: string;
  responsibleUserEmail?: string;

  // Whether this is an external assignment
  isExternalAssignment: boolean;

  // Display name for assignee (customer or employee name)
  assigneeName: string;

  assignedByUserId: string;
  assignedByUserName: string;

  action: AssetAssignmentAction;
  actionName: string;

  purpose: AssetAssignmentPurpose;
  purposeName: string;

  assignmentDate: Date;
  expectedReturnDate?: Date;
  actualReturnDate?: Date;

  returnedToUserId?: string;
  returnedToUserName?: string;

  isActive: boolean;

  // Condition Tracking
  conditionAtAssignment: AssetCondition;
  conditionAtAssignmentName: string;
  conditionAtReturn?: AssetCondition;
  conditionAtReturnName?: string;
  conditionNotesAtAssignment?: string;
  conditionNotesAtReturn?: string;

  // Location
  location?: string;
  departmentId?: string;
  departmentName?: string;
  costCenter?: string;

  // Documentation
  assignmentNumber?: string;
  notes?: string;
  isAcknowledged: boolean;
  acknowledgedAt?: Date;
  acknowledgementReference?: string;

  // Computed Properties
  isOverdue: boolean;
  daysUntilDue?: number;
  daysOverdue?: number;

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface AssetAssignmentSummary {
  id: string;
  assetId: string;
  assetTag: string;
  assetName: string;
  assetSerialNumber?: string;
  assetType: number;
  assetTypeName: string;

  // Internal Assignment (Employee)
  assignedToUserId?: string;
  assignedToUserName?: string;
  assignedToEmployeeCode?: string;

  // External Assignment (Customer/SI)
  assignedToCustomerId?: string;
  assignedToCustomerName?: string;
  assignedToCustomerCode?: string;

  // Responsible User (for external assignments)
  responsibleUserId?: string;
  responsibleUserName?: string;

  // Whether this is an external assignment
  isExternalAssignment: boolean;

  // Display name for assignee (customer or employee name)
  assigneeName: string;

  assignedByUserId?: string;
  assignedByUserName?: string;

  action: AssetAssignmentAction;
  actionName: string;

  purpose: AssetAssignmentPurpose;
  purposeName: string;

  assignmentDate: Date;
  expectedReturnDate?: Date;
  actualReturnDate?: Date;

  isActive: boolean;
  isOverdue: boolean;

  conditionAtAssignment: AssetCondition;
  conditionAtAssignmentName: string;

  isAcknowledged: boolean;
  assignmentNumber?: string;
}

// Request DTOs
export interface AssignAssetRequest {
  assetId: string;

  // For internal assignments (employee)
  assignedToUserId?: string;

  // For external assignments (customer/SI)
  assignedToCustomerId?: string;

  // Internal user responsible for tracking (required for external assignments)
  responsibleUserId?: string;

  purpose?: AssetAssignmentPurpose;
  assignmentDate?: Date;
  expectedReturnDate?: Date;
  conditionAtAssignment?: AssetCondition;
  conditionNotesAtAssignment?: string;
  location?: string;
  departmentId?: string;
  costCenter?: string;
  notes?: string;
}

export interface ReturnAssetRequest {
  conditionAtReturn?: AssetCondition;
  conditionNotesAtReturn?: string;
  returnDate?: Date;
  notes?: string;
}

export interface TransferAssetRequest {
  newAssignedToUserId: string;
  purpose?: AssetAssignmentPurpose;
  transferDate?: Date;
  expectedReturnDate?: Date;
  conditionAtTransfer?: AssetCondition;
  conditionNotes?: string;
  location?: string;
  departmentId?: string;
  costCenter?: string;
  notes?: string;
}

export interface ExtendAssignmentRequest {
  newExpectedReturnDate: Date;
  reason?: string;
}

export interface AcknowledgeAssetRequest {
  acknowledgementReference?: string;
  notes?: string;
}

export interface ReportAssetIssueRequest {
  issueType: AssetAssignmentAction;
  currentCondition: AssetCondition;
  description: string;
  issueDate?: Date;
}

export interface ReportIssueRequest {
  issueType: AssetIssueType;
  severity: AssetIssueSeverity;
  description: string;
  isAssetUsable?: boolean;
}

// Dashboard DTOs
export interface EmployeeAssetDashboard {
  userId: string;
  userName: string;
  employeeCode?: string;
  currentAssignments: AssetAssignmentSummary[];
  responsibleForAssignments: AssetAssignmentSummary[]; // External assignments user is responsible for
  pendingAcknowledgement: AssetAssignmentSummary[];
  overdueReturns: AssetAssignmentSummary[];
  upcomingReturns: AssetAssignmentSummary[];
  recentHistory: AssetAssignmentSummary[];
  statistics: AssetAssignmentStatistics;
}

export interface AssetAssignmentStatistics {
  totalActiveAssignments: number;
  permanentAssignments: number;
  temporaryAssignments: number;
  overdueCount: number;
  pendingAcknowledgementCount: number;
  responsibleForCount: number;           // External assignments user is responsible for
  responsibleForOverdueCount: number;    // Overdue external assignments
}

export interface AdminAssignmentStatistics {
  totalAssignments: number;
  activeAssignments: number;
  returnedAssignments: number;
  overdueAssignments: number;
  pendingAcknowledgements: number;
  byPurpose: { [key: string]: number };
  byAssetType: { [key: string]: number };
  byDepartment: { [key: string]: number };
  topAssignees: EmployeeAssignmentCount[];
}

export interface EmployeeAssignmentCount {
  userId: string;
  userName: string;
  employeeCode?: string;
  assignmentCount: number;
}

// Enums
export enum AssetAssignmentAction {
  Assigned = 1,
  Returned = 2,
  Transferred = 3,
  Extended = 4,
  LostReported = 5,
  DamageReported = 6,
  Acknowledged = 7
}

export enum AssetAssignmentPurpose {
  Permanent = 1,
  Temporary = 2,
  Project = 3,
  Training = 4,
  Demo = 5,
  Replacement = 6,
  Emergency = 7
}

export enum AssetCondition {
  New = 1,
  Excellent = 2,
  Good = 3,
  Fair = 4,
  Poor = 5,
  NonFunctional = 6,
  BeyondRepair = 7,
  Refurbished = 8
}

export enum AssetIssueType {
  Malfunction = 1,
  Damage = 2,
  Lost = 3,
  Stolen = 4,
  Software = 5,
  Hardware = 6,
  Performance = 7,
  Other = 8
}

export enum AssetIssueSeverity {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

// Label mappings
export const AssetAssignmentActionLabels: { [key: number]: string } = {
  [AssetAssignmentAction.Assigned]: 'Assigned',
  [AssetAssignmentAction.Returned]: 'Returned',
  [AssetAssignmentAction.Transferred]: 'Transferred',
  [AssetAssignmentAction.Extended]: 'Extended',
  [AssetAssignmentAction.LostReported]: 'Lost Reported',
  [AssetAssignmentAction.DamageReported]: 'Damage Reported',
  [AssetAssignmentAction.Acknowledged]: 'Acknowledged'
};

export const AssetAssignmentPurposeLabels: { [key: number]: string } = {
  [AssetAssignmentPurpose.Permanent]: 'Permanent',
  [AssetAssignmentPurpose.Temporary]: 'Temporary',
  [AssetAssignmentPurpose.Project]: 'Project',
  [AssetAssignmentPurpose.Training]: 'Training',
  [AssetAssignmentPurpose.Demo]: 'Demo',
  [AssetAssignmentPurpose.Replacement]: 'Replacement',
  [AssetAssignmentPurpose.Emergency]: 'Emergency'
};

export const AssetConditionLabels: { [key: number]: string } = {
  [AssetCondition.New]: 'New',
  [AssetCondition.Excellent]: 'Excellent',
  [AssetCondition.Good]: 'Good',
  [AssetCondition.Fair]: 'Fair',
  [AssetCondition.Poor]: 'Poor',
  [AssetCondition.NonFunctional]: 'Non-Functional',
  [AssetCondition.BeyondRepair]: 'Beyond Repair',
  [AssetCondition.Refurbished]: 'Refurbished'
};

export const AssetIssueTypeLabels: { [key: number]: string } = {
  [AssetIssueType.Malfunction]: 'Malfunction',
  [AssetIssueType.Damage]: 'Physical Damage',
  [AssetIssueType.Lost]: 'Lost',
  [AssetIssueType.Stolen]: 'Stolen',
  [AssetIssueType.Software]: 'Software Issue',
  [AssetIssueType.Hardware]: 'Hardware Issue',
  [AssetIssueType.Performance]: 'Performance Issue',
  [AssetIssueType.Other]: 'Other'
};

export const AssetIssueSeverityLabels: { [key: number]: string } = {
  [AssetIssueSeverity.Low]: 'Low',
  [AssetIssueSeverity.Medium]: 'Medium',
  [AssetIssueSeverity.High]: 'High',
  [AssetIssueSeverity.Critical]: 'Critical'
};

// Helper functions
export function getAssignmentActionLabel(action: AssetAssignmentAction): string {
  return AssetAssignmentActionLabels[action] || 'Unknown';
}

export function getAssignmentPurposeLabel(purpose: AssetAssignmentPurpose): string {
  return AssetAssignmentPurposeLabels[purpose] || 'Unknown';
}

export function getConditionLabel(condition: AssetCondition): string {
  return AssetConditionLabels[condition] || 'Unknown';
}

export function getPurposeColorClass(purpose: AssetAssignmentPurpose): string {
  switch (purpose) {
    case AssetAssignmentPurpose.Permanent:
      return 'bg-blue-100 text-blue-800';
    case AssetAssignmentPurpose.Temporary:
      return 'bg-yellow-100 text-yellow-800';
    case AssetAssignmentPurpose.Project:
      return 'bg-purple-100 text-purple-800';
    case AssetAssignmentPurpose.Training:
      return 'bg-green-100 text-green-800';
    case AssetAssignmentPurpose.Demo:
      return 'bg-indigo-100 text-indigo-800';
    case AssetAssignmentPurpose.Replacement:
      return 'bg-orange-100 text-orange-800';
    case AssetAssignmentPurpose.Emergency:
      return 'bg-red-100 text-red-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

export function getConditionColorClass(condition: AssetCondition): string {
  switch (condition) {
    case AssetCondition.New:
    case AssetCondition.Excellent:
      return 'bg-green-100 text-green-800';
    case AssetCondition.Good:
      return 'bg-blue-100 text-blue-800';
    case AssetCondition.Fair:
      return 'bg-yellow-100 text-yellow-800';
    case AssetCondition.Poor:
      return 'bg-orange-100 text-orange-800';
    case AssetCondition.NonFunctional:
    case AssetCondition.BeyondRepair:
      return 'bg-red-100 text-red-800';
    case AssetCondition.Refurbished:
      return 'bg-teal-100 text-teal-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}
