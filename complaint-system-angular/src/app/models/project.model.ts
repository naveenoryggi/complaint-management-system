// Project Management Models

// ============ Enums ============

export enum ProjectStatus {
  Draft = 0,
  Planning = 1,
  InProgress = 2,
  OnHold = 3,
  Completed = 4,
  Cancelled = 5
}

export enum MilestoneStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Delayed = 3
}

export enum ProjectTaskStatus {
  ToDo = 0,
  InProgress = 1,
  InReview = 2,
  Completed = 3,
  Blocked = 4
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum DocumentCategory {
  Proposal = 0,
  Contract = 1,
  Specification = 2,
  Design = 3,
  Report = 4,
  Other = 5
}

// ============ Label Maps ============

export const ProjectStatusLabels: Record<ProjectStatus, string> = {
  [ProjectStatus.Draft]: 'Draft',
  [ProjectStatus.Planning]: 'Planning',
  [ProjectStatus.InProgress]: 'In Progress',
  [ProjectStatus.OnHold]: 'On Hold',
  [ProjectStatus.Completed]: 'Completed',
  [ProjectStatus.Cancelled]: 'Cancelled'
};

export const MilestoneStatusLabels: Record<MilestoneStatus, string> = {
  [MilestoneStatus.Pending]: 'Pending',
  [MilestoneStatus.InProgress]: 'In Progress',
  [MilestoneStatus.Completed]: 'Completed',
  [MilestoneStatus.Delayed]: 'Delayed'
};

export const ProjectTaskStatusLabels: Record<ProjectTaskStatus, string> = {
  [ProjectTaskStatus.ToDo]: 'To Do',
  [ProjectTaskStatus.InProgress]: 'In Progress',
  [ProjectTaskStatus.InReview]: 'In Review',
  [ProjectTaskStatus.Completed]: 'Completed',
  [ProjectTaskStatus.Blocked]: 'Blocked'
};

export const TaskPriorityLabels: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Low',
  [TaskPriority.Medium]: 'Medium',
  [TaskPriority.High]: 'High',
  [TaskPriority.Critical]: 'Critical'
};

export const DocumentCategoryLabels: Record<DocumentCategory, string> = {
  [DocumentCategory.Proposal]: 'Proposal',
  [DocumentCategory.Contract]: 'Contract',
  [DocumentCategory.Specification]: 'Specification',
  [DocumentCategory.Design]: 'Design',
  [DocumentCategory.Report]: 'Report',
  [DocumentCategory.Other]: 'Other'
};

// ============ Project DTOs ============

export interface Project {
  id: string;
  companyId: string;
  projectCode: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  statusName: string;

  // Customer
  customerId: string;
  customerCode: string;
  customerName: string;

  // Timeline
  plannedStartDate?: Date;
  plannedEndDate?: Date;
  actualStartDate?: Date;
  actualEndDate?: Date;
  progressPercentage: number;

  // Budget
  budget?: number;
  actualCost?: number;
  currency: string;

  // Team
  projectManagerId?: string;
  projectManagerName?: string;

  // Additional
  priority?: number;
  tags?: string;
  notes?: string;
  customFields?: string;

  // Statistics
  milestoneCount: number;
  completedMilestoneCount: number;
  taskCount: number;
  completedTaskCount: number;
  documentCount: number;
  teamMemberCount: number;
  ticketCount: number;

  // Nested data
  milestones: ProjectMilestone[];
  tasks: ProjectTask[];
  documents: ProjectDocument[];
  teamMembers: ProjectTeamMember[];

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface ProjectSummary {
  id: string;
  projectCode: string;
  name: string;
  status: ProjectStatus;
  statusName: string;

  // Customer
  customerId: string;
  customerName: string;

  // Timeline
  plannedStartDate?: Date;
  plannedEndDate?: Date;
  progressPercentage: number;

  // Budget
  budget?: number;
  actualCost?: number;

  // Team
  projectManagerName?: string;

  // Statistics
  milestoneCount: number;
  completedMilestoneCount?: number;
  taskCount: number;
  completedTaskCount?: number;
  teamMemberCount: number;
}

export interface ProjectLookup {
  id: string;
  projectCode: string;
  name: string;
  status: ProjectStatus;
  customerId: string;
  customerName: string;
}

export interface CreateProjectRequest {
  projectCode: string;
  name: string;
  description?: string;
  status?: ProjectStatus;
  customerId: string;
  plannedStartDate?: Date;
  plannedEndDate?: Date;
  budget?: number;
  currency?: string;
  projectManagerId?: string;
  priority?: number;
  tags?: string;
  notes?: string;
  customFields?: string;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string;
  status: ProjectStatus;
  plannedStartDate?: Date;
  plannedEndDate?: Date;
  actualStartDate?: Date;
  actualEndDate?: Date;
  progressPercentage: number;
  budget?: number;
  actualCost?: number;
  currency?: string;
  projectManagerId?: string;
  priority?: number;
  tags?: string;
  notes?: string;
  customFields?: string;
}

// ============ Milestone DTOs ============

export interface ProjectMilestone {
  id: string;
  projectId: string;
  name: string;
  description?: string;
  status: MilestoneStatus;
  statusName: string;
  dueDate: Date;
  completedDate?: Date;
  sortOrder: number;
  notes?: string;
  taskCount: number;
  completedTaskCount: number;
  createdAt: Date;
}

export interface CreateMilestoneRequest {
  name: string;
  description?: string;
  dueDate: Date;
  sortOrder?: number;
  notes?: string;
}

export interface UpdateMilestoneRequest {
  name: string;
  description?: string;
  status: MilestoneStatus;
  dueDate: Date;
  completedDate?: Date;
  sortOrder: number;
  notes?: string;
}

// ============ Task DTOs ============

export interface ProjectTask {
  id: string;
  projectId: string;
  milestoneId?: string;
  milestoneName?: string;
  title: string;
  description?: string;
  status: ProjectTaskStatus;
  statusName: string;
  priority: TaskPriority;
  priorityName: string;

  // Assignment
  assigneeId?: string;
  assigneeName?: string;

  // Timeline
  dueDate?: Date;
  completedDate?: Date;

  // Time Tracking
  estimatedHours?: number;
  actualHours?: number;

  tags?: string;
  notes?: string;
  createdAt: Date;
}

export interface CreateTaskRequest {
  milestoneId?: string;
  title: string;
  description?: string;
  priority?: TaskPriority;
  assigneeId?: string;
  dueDate?: Date;
  estimatedHours?: number;
  tags?: string;
  notes?: string;
}

export interface UpdateTaskRequest {
  milestoneId?: string;
  title: string;
  description?: string;
  status: ProjectTaskStatus;
  priority: TaskPriority;
  assigneeId?: string;
  dueDate?: Date;
  completedDate?: Date;
  estimatedHours?: number;
  actualHours?: number;
  tags?: string;
  notes?: string;
}

// ============ Document DTOs ============

export interface ProjectDocument {
  id: string;
  projectId: string;
  fileName: string;
  filePath: string;
  fileType?: string;
  fileSize: number;
  category: DocumentCategory;
  categoryName: string;
  description?: string;
  uploadedById: string;
  uploadedByName?: string;
  uploadedAt: Date;
}

export interface UploadDocumentRequest {
  fileName: string;
  filePath: string;
  fileType?: string;
  fileSize: number;
  category?: DocumentCategory;
  description?: string;
}

// ============ Team Member DTOs ============

export interface ProjectTeamMember {
  id: string;
  projectId: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  role?: string;
  allocationPercentage?: number;
  joinedDate: Date;
  leftDate?: Date;
  isActive: boolean;
  notes?: string;
}

export interface AddTeamMemberRequest {
  employeeId: string;
  role?: string;
  allocationPercentage?: number;
  joinedDate?: Date;
  notes?: string;
}

export interface UpdateTeamMemberRequest {
  role?: string;
  allocationPercentage?: number;
  isActive: boolean;
  leftDate?: Date;
  notes?: string;
}

// ============ Statistics DTO ============

export interface ProjectStatistics {
  totalMilestones: number;
  completedMilestones: number;
  delayedMilestones: number;
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  blockedTasks: number;
  totalTeamMembers: number;
  activeTeamMembers: number;
  totalDocuments: number;
  totalEstimatedHours: number;
  totalActualHours: number;
  relatedTickets: number;
}

// ============ Paged Result ============

export interface PagedProjectResult {
  items: ProjectSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ============ Enum Arrays for Dropdowns ============

export const ProjectStatusOptions = [
  { value: ProjectStatus.Draft, label: 'Draft' },
  { value: ProjectStatus.Planning, label: 'Planning' },
  { value: ProjectStatus.InProgress, label: 'In Progress' },
  { value: ProjectStatus.OnHold, label: 'On Hold' },
  { value: ProjectStatus.Completed, label: 'Completed' },
  { value: ProjectStatus.Cancelled, label: 'Cancelled' }
];

export const MilestoneStatusOptions = [
  { value: MilestoneStatus.Pending, label: 'Pending' },
  { value: MilestoneStatus.InProgress, label: 'In Progress' },
  { value: MilestoneStatus.Completed, label: 'Completed' },
  { value: MilestoneStatus.Delayed, label: 'Delayed' }
];

export const ProjectTaskStatusOptions = [
  { value: ProjectTaskStatus.ToDo, label: 'To Do' },
  { value: ProjectTaskStatus.InProgress, label: 'In Progress' },
  { value: ProjectTaskStatus.InReview, label: 'In Review' },
  { value: ProjectTaskStatus.Completed, label: 'Completed' },
  { value: ProjectTaskStatus.Blocked, label: 'Blocked' }
];

export const TaskPriorityOptions = [
  { value: TaskPriority.Low, label: 'Low' },
  { value: TaskPriority.Medium, label: 'Medium' },
  { value: TaskPriority.High, label: 'High' },
  { value: TaskPriority.Critical, label: 'Critical' }
];

export const DocumentCategoryOptions = [
  { value: DocumentCategory.Proposal, label: 'Proposal' },
  { value: DocumentCategory.Contract, label: 'Contract' },
  { value: DocumentCategory.Specification, label: 'Specification' },
  { value: DocumentCategory.Design, label: 'Design' },
  { value: DocumentCategory.Report, label: 'Report' },
  { value: DocumentCategory.Other, label: 'Other' }
];
