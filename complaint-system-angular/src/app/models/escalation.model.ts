export enum TimeUnit {
  Minutes = 0,
  Hours = 1,
  Days = 2,
  Weeks = 3
}

export enum AssignmentStrategy {
  ReportingManager = 0,
  SectionIncharge = 1,
  BranchContacts = 2,
  DepartmentContacts = 3,
  AdminEscalation = 4,
  ResourcePool = 5
}

export interface ContactHierarchy {
  primaryContactId?: string;
  secondaryContactId?: string;
  hrContactId?: string;
}

export enum ResourcePoolAssignmentMethod {
  Manual = 'Manual',
  RoundRobin = 'RoundRobin',
  LeastBusy = 'LeastBusy'
}

export interface EscalationLevel {
  id: string;
  escalationMatrixId: string;
  level: number;
  name: string;
  description?: string;
  triggerAfterHours: number;
  triggerAfterValue: number;
  triggerTimeUnit: TimeUnit;
  triggerTimeDisplay: string;
  assignmentStrategy: AssignmentStrategy;
  assignToUserId?: string;
  assignToUserName?: string;
  assignToRole?: string;
  assignToUserIds?: string;

  // Contact Hierarchy fields (for SectionIncharge, BranchContacts, DepartmentContacts)
  primaryContactId?: string;
  primaryContactName?: string;
  secondaryContactId?: string;
  secondaryContactName?: string;
  hrContactId?: string;
  hrContactName?: string;

  // Resource Pool fields (for ResourcePool strategy)
  resourcePoolId?: string;
  resourcePoolName?: string;
  resourcePoolAssignmentMethod?: ResourcePoolAssignmentMethod;

  isActive: boolean;
  sendNotification: boolean;
  emailTemplateId?: string;
  notifyPreviousHandler: boolean;
  escalationMessage?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface EscalationMatrix {
  id: string;
  companyId: string;
  companyName?: string;
  name: string;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  isActive: boolean;
  isDefault?: boolean;
  priority: number;
  enableAutoEscalation: boolean;
  sendEmailNotifications: boolean;
  escalationLevels: EscalationLevel[];  // Changed from 'levels' to 'escalationLevels'
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateEscalationMatrixRequest {
  name: string;
  description?: string;
  isDefault?: boolean;
  escalationLevels: CreateEscalationLevelRequest[];  // Changed from 'levels' to 'escalationLevels' to match backend
}

export interface CreateEscalationLevelRequest {
  level: number;
  name: string;
  description?: string;
  triggerAfterHours?: number;
  triggerAfterValue: number;
  triggerTimeUnit: TimeUnit;
  assignmentStrategy: AssignmentStrategy;
  assignToUserId?: string;
  assignToRole?: string;
  assignToUserIds?: string;

  // Contact Hierarchy fields
  primaryContactId?: string;
  secondaryContactId?: string;
  hrContactId?: string;

  // Resource Pool fields
  resourcePoolId?: string;
  resourcePoolAssignmentMethod?: ResourcePoolAssignmentMethod;

  sendNotification?: boolean;
  emailTemplateId?: string;
  notifyPreviousHandler?: boolean;
  escalationMessage?: string;
}

export interface UpdateEscalationMatrixRequest {
  name: string;
  description?: string;
  categoryId?: string;
  branchId?: string;
  departmentId?: string;
  isActive: boolean;
  priority: number;
  enableAutoEscalation: boolean;
  sendEmailNotifications: boolean;
  escalationLevels: CreateEscalationLevelRequest[];
}

export interface UpdateEscalationLevelRequest {
  name: string;
  description?: string;
  triggerAfterHours: number;
  triggerAfterValue: number;
  triggerTimeUnit: TimeUnit;
  assignmentStrategy: AssignmentStrategy;
  assignToUserId?: string;
  assignToRole?: string;
  assignToUserIds?: string;

  // Contact Hierarchy fields
  primaryContactId?: string;
  secondaryContactId?: string;
  hrContactId?: string;

  // Resource Pool fields
  resourcePoolId?: string;
  resourcePoolAssignmentMethod?: ResourcePoolAssignmentMethod;

  isActive: boolean;
  sendNotification: boolean;
  emailTemplateId?: string;
  notifyPreviousHandler: boolean;
  escalationMessage?: string;
}

export interface EscalationPolicy {
  id: string;
  companyId: string;
  companyName: string;
  name: string;
  description?: string;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  categoryId?: string;
  categoryName?: string;
  enableAutoEscalation: boolean;
  requireManualApproval: boolean;
  defaultEscalationMatrixId?: string;
  defaultEscalationMatrixName?: string;
  minimumSeverityForAutoEscalation?: number;
  maxAutoEscalationLevels?: number;
  priority: number;
  specificityScore: number;
  isActive: boolean;
  effectiveFrom?: Date;
  effectiveTo?: Date;
  scopeDescription: string;
  isCurrentlyEffective: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateEscalationPolicyRequest {
  companyId: string;
  name: string;
  description?: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  categoryId?: string;
  enableAutoEscalation: boolean;
  requireManualApproval: boolean;
  defaultEscalationMatrixId?: string;
  minimumSeverityForAutoEscalation?: number;
  maxAutoEscalationLevels?: number;
  priority?: number;
  effectiveFrom?: Date;
  effectiveTo?: Date;
}

export interface UpdateEscalationPolicyRequest {
  name: string;
  description?: string;
  enableAutoEscalation: boolean;
  requireManualApproval: boolean;
  defaultEscalationMatrixId?: string;
  minimumSeverityForAutoEscalation?: number;
  maxAutoEscalationLevels?: number;
  priority: number;
  isActive: boolean;
  effectiveFrom?: Date;
  effectiveTo?: Date;
}

export interface PolicyResolutionTestRequest {
  companyId: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  categoryId?: string;
}

export interface PolicyResolutionResult {
  resolvedPolicy?: EscalationPolicy;
  allMatchingPolicies: EscalationPolicy[];
  resolutionExplanation: string;
}

export interface EscalationHistory {
  id: string;
  complaintId: string;
  escalationMatrixId: string;
  escalationMatrixName: string;
  level: number;
  levelName: string;
  escalatedFromUserId?: string;
  escalatedFromUserName?: string;
  escalatedToUserId?: string;
  escalatedToUserName?: string;
  reason: string;
  escalatedAt: Date;
  acknowledgedAt?: Date;
  acknowledgedByUserId?: string;
  acknowledgedByUserName?: string;
  acknowledgmentNotes?: string;
  isAutoEscalation: boolean;
  status: string;
}

// Resource Pool Types - numeric values to match C# backend enum
export enum ResourcePoolType {
  Branch = 0,
  Department = 1,
  Section = 2,
  Custom = 3
}

export interface ResourcePoolMember {
  id: string;
  resourcePoolId: string;
  userId: string;
  userName: string;
  userEmail: string;
  addedAt: Date;
}

export interface ResourcePool {
  id: string;
  companyId: string;
  companyName?: string;
  name: string;
  description?: string;
  poolType: ResourcePoolType;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  members: ResourcePoolMember[];
  memberCount: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateResourcePoolRequest {
  name: string;
  description?: string;
  poolType: ResourcePoolType;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  memberUserIds: string[];
}

export interface UpdateResourcePoolRequest {
  name: string;
  description?: string;
  poolType: ResourcePoolType;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  isActive: boolean;
}

export interface AddResourcePoolMemberRequest {
  userId: string;
}

export interface RemoveResourcePoolMemberRequest {
  userId: string;
}
