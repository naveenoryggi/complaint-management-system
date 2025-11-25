export interface CategoryWorkflow {
  id: string;
  categoryId: string;
  categoryName: string;
  name: string;
  description?: string;
  isActive: boolean;
  isDefault: boolean;
  companyId?: string;
  companyName?: string;
  workflowStatuses: CategoryWorkflowStatus[];
  transitions: CategoryWorkflowTransition[];
  createdAt: Date;
  updatedAt?: Date;
}

export interface CategoryWorkflowStatus {
  id: string;
  workflowId: string;
  statusMasterId: string;
  statusName: string;
  statusCode: string;
  statusColorCode?: string;
  statusIconClass?: string;
  displayOrder: number;
  isInitialStatus: boolean;
  isActive: boolean;
  defaultSLAHours?: number;
  escalationHours?: number;
  requiresApproval: boolean;
  allowedRoles?: string[];
  createdAt: Date;
}

export interface CategoryWorkflowTransition {
  id: string;
  workflowId: string;
  fromStatusId: string;
  fromStatusName?: string;
  toStatusId: string;
  toStatusName?: string;
  transitionName?: string;
  description?: string;
  requiresComment: boolean;
  requiresApproval: boolean;
  allowedRoles?: string[];
  displayOrder: number;
  isActive: boolean;
  isAutomatic: boolean;
  autoTransitionAfterHours?: number;
  transitionConditions?: string;
  buttonColor?: string;
  iconClass?: string;
  createdAt: Date;
}

// Request DTOs
export interface CreateWorkflowRequest {
  categoryId: string;
  name: string;
  description?: string;
  isActive: boolean;
  isDefault: boolean;
  companyId?: string;
}

export interface AddStatusRequest {
  workflowId: string;
  statusMasterId: string;
  displayOrder: number;
  isInitialStatus: boolean;
  defaultSLAHours?: number;
  escalationHours?: number;
  requiresApproval: boolean;
  allowedRoles?: string[];
}

export interface AddTransitionRequest {
  workflowId: string;
  fromStatusId: string;
  toStatusId: string;
  transitionName?: string;
  description?: string;
  requiresComment: boolean;
  requiresApproval: boolean;
  allowedRoles?: string[];
  displayOrder?: number;
  isAutomatic?: boolean;
  autoTransitionAfterHours?: number;
  buttonColor?: string;
  iconClass?: string;
}

export interface TransitionComplaintRequest {
  newStatusId: string;
  comment?: string;
}

// Response DTOs
export interface AllowedTransitionsResponse {
  success: boolean;
  data: {
    transitions: CategoryWorkflowTransition[];
    currentStatus: any;
  };
  message?: string;
}

export interface TransitionValidationResponse {
  success: boolean;
  data: {
    isAllowed: boolean;
    requiresComment: boolean;
    requiresApproval: boolean;
    message?: string;
  };
  message?: string;
}
