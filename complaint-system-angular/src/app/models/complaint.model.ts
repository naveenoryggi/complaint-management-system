export interface Complaint {
  id: string;
  complaintNumber: string;
  title: string;
  description: string;
  categoryId: string;
  categoryName: string;
  complainantId: string;
  complainantName: string;
  complainantEmail: string;
  companyId: string;
  companyName: string;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  complainantBranchId?: string;
  complainantBranchName?: string;
  complainantDepartmentId?: string;
  complainantDepartmentName?: string;
  complainantSectionId?: string;
  complainantSectionName?: string;
  complainantEmployeeCode?: string;

  // Contact information
  contactEmail?: string;
  contactPhone?: string;
  alternatePhone?: string;
  preferredContactMethod: PreferredContactMethod;

  // Manager details
  complainantManagerId?: string;
  complainantManagerName?: string;
  complainantManagerEmail?: string;
  complainantManagerPhone?: string;

  // Status and Priority (master-based system)
  status: string;  // Display name (e.g., "In Progress")
  statusId: string;  // Master ID (GUID)
  priority: string;  // Display name (e.g., "High")
  priorityId: string;  // Master ID (GUID)

  currentEscalationLevel: number;
  assignedToId?: string;
  assignedToName?: string;
  submittedAt: string;
  dueDate?: string;
  resolvedAt?: string;
  closedAt?: string;
  resolutionNotes?: string;
  isAnonymous: boolean;
  tags?: string;
  commentCount: number;
  attachmentCount: number;

  // Customer response tracking
  hasCustomerResponse?: boolean;  // True if customer has replied and awaiting handler response
  lastResponseFrom?: 'customer' | 'handler' | 'system';  // Who sent the last response
  lastResponseAt?: string;  // Timestamp of last response

  // Enterprise fields
  customerId?: string;
  customerName?: string;
  projectId?: string;
  projectName?: string;
  productId?: string;
  productName?: string;
}

export interface CreateComplaintRequest {
  title: string;
  description: string;
  categoryId: string;
  priorityMasterId: string;  // Changed from enum to Master ID (GUID)
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  isAnonymous: boolean;
  tags?: string;

  // Contact information (optional - will be auto-populated from user if not provided)
  employeeCode?: string;
  contactEmail?: string;
  contactPhone?: string;
  alternatePhone?: string;
  preferredContactMethod?: PreferredContactMethod;
}

export interface UpdateComplaintRequest {
  id: string;
  title: string;
  description: string;
  categoryId: string;
  priorityMasterId: string;  // Changed from enum to Master ID (GUID)
  statusMasterId?: string;  // Changed from enum to Master ID (GUID)
  assignedToId?: string;
  resolutionNotes?: string;
  tags?: string;
}

export enum ComplaintStatus {
  Submitted = 0,
  UnderReview = 1,
  InProgress = 2,
  Escalated = 3,
  PendingInfo = 4,
  Resolved = 5,
  Closed = 6,
  Rejected = 7,
  Reopened = 8
}

export enum ComplaintPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Critical = 3,
  Urgent = 4
}

export enum PreferredContactMethod {
  Email = 0,
  Phone = 1,
  Both = 2,
  SMS = 3,
  InApp = 4
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  errors?: string[];
  data: T;
}

export interface ComplaintHistory {
  complaintId: string;
  complaintNumber: string;
  events: ComplaintHistoryEvent[];
}

export interface ComplaintHistoryEvent {
  timestamp: string;
  eventType: string;
  description: string;
  performedBy?: string;
  performedByName?: string;
  metadata?: { [key: string]: string };
}
