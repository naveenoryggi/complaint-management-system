// Asset Request Workflow Models

export enum AssetRequestType {
  Return = 1,
  Assignment = 2,
  Transfer = 3
}

export enum AssetRequestStatus {
  Draft = 0,
  Pending = 1,
  Escalated = 2,
  Approved = 3,
  Rejected = 4,
  Cancelled = 5,
  Completed = 6
}

export enum AssignmentPurpose {
  InternalUse = 1,
  Demo = 2,
  Testing = 3,
  Loaner = 4,
  Project = 5,
  Training = 6
}

export enum ApprovalAction {
  Created = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
  Escalated = 4,
  Reassigned = 5,
  Cancelled = 6,
  Completed = 7,
  Comment = 8
}

export interface AssetRequest {
  id: string;
  companyId: string;
  requestNumber: string;
  requestType: AssetRequestType;
  requestTypeName: string;
  status: AssetRequestStatus;
  statusName: string;
  assetId: string;
  assetTag: string;
  assetName: string;
  assetAssignmentId?: string;
  storeId?: string;
  storeName?: string;
  requestedById: string;
  requestedByName: string;
  requestedAt: Date;
  assignToUserId?: string;
  assignToUserName?: string;
  assignToCustomerId?: string;
  assignToCustomerName?: string;
  returnReason?: string;
  notes?: string;
  currentApproverId?: string;
  currentApproverName?: string;
  approvalDeadline?: Date;
  escalationLevel: number;
  approvedAt?: Date;
  approvedByName?: string;
  rejectedAt?: Date;
  rejectedByName?: string;
  rejectionReason?: string;
  completedAt?: Date;
  purpose?: AssignmentPurpose;
  purposeName?: string;
  expectedReturnDate?: Date;
  transferToStoreId?: string;
  transferToStoreName?: string;
  transferToUserId?: string;
  transferToUserName?: string;
  approvalHistory?: RequestApprovalHistory[];
  createdAt: Date;
  updatedAt?: Date;
}

export interface AssetRequestListItem {
  id: string;
  requestNumber: string;
  requestType: AssetRequestType;
  requestTypeName: string;
  status: AssetRequestStatus;
  statusName: string;
  assetTag: string;
  assetCode?: string;
  assetName: string;
  storeId?: string;
  storeName?: string;
  requestedByName: string;
  requestedAt: Date;
  createdAt: Date;
  currentApproverName?: string;
  approvalDeadline?: Date;
  escalationLevel: number;
}

export interface RequestApprovalHistory {
  id: string;
  requestId: string;
  actorId: string;
  actorName: string;
  action: ApprovalAction;
  actionName: string;
  actionAt: Date;
  comments?: string;
  escalationLevel: number;
  previousStatus?: AssetRequestStatus;
  previousStatusName?: string;
  newStatus?: AssetRequestStatus;
  newStatusName?: string;
  previousApproverName?: string;
  newApproverName?: string;
}

// Request Creation DTOs
export interface CreateReturnRequestDto {
  assetId: string;
  assetAssignmentId?: string;
  storeId?: string;
  returnReason: string;
  notes?: string;
}

export interface CreateAssignmentRequestDto {
  assetId: string;
  storeId?: string;
  assignToUserId?: string;
  assignToCustomerId?: string;
  purpose: AssignmentPurpose;
  expectedReturnDate?: Date;
  notes?: string;
}

export interface CreateTransferRequestDto {
  assetId: string;
  storeId?: string;
  transferToStoreId?: string;
  transferToUserId?: string;
  notes?: string;
}

// Workflow DTOs
export interface ApproveRequestDto {
  comments?: string;
}

export interface RejectRequestDto {
  reason: string;
  comments?: string;
}

export interface EscalateRequestDto {
  reason: string;
  escalateToUserId?: string;
}

export interface ReassignRequestDto {
  newApproverId: string;
  reason: string;
}

export interface AddRequestCommentDto {
  comments: string;
}

// Filter DTO
export interface AssetRequestFilterDto {
  requestType?: AssetRequestType;
  status?: AssetRequestStatus;
  storeId?: string;
  assetId?: string;
  requestedById?: string;
  currentApproverId?: string;
  fromDate?: Date;
  toDate?: Date;
  page?: number;
  pageSize?: number;
}

// Pending Approvals
export interface PendingApprovals {
  totalCount: number;
  returnRequests: AssetRequestListItem[];
  assignmentRequests: AssetRequestListItem[];
  transferRequests: AssetRequestListItem[];
}

// Labels
export const AssetRequestTypeLabels: { [key: number]: string } = {
  [AssetRequestType.Return]: 'Return',
  [AssetRequestType.Assignment]: 'Assignment',
  [AssetRequestType.Transfer]: 'Transfer'
};

export const AssetRequestStatusLabels: { [key: number]: string } = {
  [AssetRequestStatus.Draft]: 'Draft',
  [AssetRequestStatus.Pending]: 'Pending Approval',
  [AssetRequestStatus.Escalated]: 'Escalated',
  [AssetRequestStatus.Approved]: 'Approved',
  [AssetRequestStatus.Rejected]: 'Rejected',
  [AssetRequestStatus.Cancelled]: 'Cancelled',
  [AssetRequestStatus.Completed]: 'Completed'
};

export const AssignmentPurposeLabels: { [key: number]: string } = {
  [AssignmentPurpose.InternalUse]: 'Internal Use',
  [AssignmentPurpose.Demo]: 'Demo',
  [AssignmentPurpose.Testing]: 'Testing',
  [AssignmentPurpose.Loaner]: 'Loaner',
  [AssignmentPurpose.Project]: 'Project',
  [AssignmentPurpose.Training]: 'Training'
};

export const ApprovalActionLabels: { [key: number]: string } = {
  [ApprovalAction.Created]: 'Created',
  [ApprovalAction.Submitted]: 'Submitted',
  [ApprovalAction.Approved]: 'Approved',
  [ApprovalAction.Rejected]: 'Rejected',
  [ApprovalAction.Escalated]: 'Escalated',
  [ApprovalAction.Reassigned]: 'Reassigned',
  [ApprovalAction.Cancelled]: 'Cancelled',
  [ApprovalAction.Completed]: 'Completed',
  [ApprovalAction.Comment]: 'Comment Added'
};

// Status colors for UI
export const AssetRequestStatusColors: { [key: number]: string } = {
  [AssetRequestStatus.Draft]: 'secondary',
  [AssetRequestStatus.Pending]: 'warning',
  [AssetRequestStatus.Escalated]: 'danger',
  [AssetRequestStatus.Approved]: 'success',
  [AssetRequestStatus.Rejected]: 'danger',
  [AssetRequestStatus.Cancelled]: 'secondary',
  [AssetRequestStatus.Completed]: 'primary'
};
