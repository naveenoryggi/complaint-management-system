export enum RoleType {
  SystemAdmin = 0,
  TenantAdmin = 1,
  CompanyAdmin = 2,
  DepartmentManager = 3,
  Level1Handler = 4,
  Level2Handler = 5,
  Level3Handler = 6,
  Level4Handler = 7,
  Level5Handler = 8,
  HRRepresentative = 9,
  Complainant = 10,
  Viewer = 11,
  ReportingManager = 12,
  PrimaryContact = 13,
  SecondaryContact = 14
}

export enum PermissionType {
  ViewComplaints = 0,
  CreateComplaint = 1,
  EditComplaint = 2,
  DeleteComplaint = 3,
  AssignComplaint = 4,
  EscalateComplaint = 5,
  CloseComplaint = 6,
  ReopenComplaint = 7,
  AddComment = 8,
  ViewComments = 9,
  AddAttachment = 10,
  ViewAttachments = 11,
  ManageCategories = 12,
  ManageRoles = 13,
  ViewReports = 14,
  ManageSettings = 15,
  ManageUsers = 16,
  ViewAuditLogs = 17,
  ViewEscalation = 18,
  ManageEscalation = 19
}

export interface Role {
  id: string;
  name: string;
  code: string;
  description: string;
  roleType: RoleType;
  escalationLevel: number;
  isSystemRole: boolean;
  isActive: boolean;
  displayOrder: number;
  permissions: Permission[];
  createdAt: string;
  updatedAt?: string;
}

export interface Permission {
  id: string;
  permissionType: PermissionType;
  permissionName: string;
  isGranted: boolean;
}

export interface CreateRoleRequest {
  name: string;
  code: string;
  description: string;
  roleType: RoleType;
  escalationLevel: number;
  displayOrder: number;
  permissions: PermissionType[];
}

export interface UpdateRoleRequest {
  name?: string;
  description?: string;
  escalationLevel?: number;
  displayOrder?: number;
  isActive?: boolean;
}

export interface UpdateRolePermissionsRequest {
  permissions: RolePermissionUpdate[];
}

export interface RolePermissionUpdate {
  permissionType: PermissionType;
  isGranted: boolean;
}

export interface AssignRoleToUserRequest {
  userId: string;
  roleId: string;
  effectiveFrom?: string;
  effectiveTo?: string;
  isPrimary: boolean;
  notes?: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
}

export interface UserRoleAssignment {
  id: string;
  userId: string;
  userName: string;
  roleId: string;
  roleName: string;
  effectiveFrom: string;
  effectiveTo?: string;
  isPrimary: boolean;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}
