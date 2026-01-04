// Store Management Models

export interface Store {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  primaryManagerId?: string;
  primaryManagerName?: string;
  secondaryManagerId?: string;
  secondaryManagerName?: string;
  branchId?: string;
  branchName?: string;
  isActive: boolean;
  approvalTimeoutHours: number;
  autoEscalateToSecondary: boolean;
  requireDeptApprovalForAssignment: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateStoreDto {
  code: string;
  name: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  primaryManagerId?: string;
  secondaryManagerId?: string;
  branchId?: string;
  approvalTimeoutHours?: number;
  autoEscalateToSecondary?: boolean;
  requireDeptApprovalForAssignment?: boolean;
}

export interface UpdateStoreDto {
  name?: string;
  description?: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  primaryManagerId?: string;
  secondaryManagerId?: string;
  branchId?: string;
  isActive?: boolean;
  approvalTimeoutHours?: number;
  autoEscalateToSecondary?: boolean;
  requireDeptApprovalForAssignment?: boolean;
}

export interface StoreLookup {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface AssignStoreManagersDto {
  primaryManagerId?: string;
  secondaryManagerId?: string;
}

// Store User Roles
export enum StoreRole {
  StoreKeeper = 1,
  StoreManager = 2,
  StoreAdmin = 3
}

export interface StoreUserRole {
  id: string;
  storeId: string;
  storeName: string;
  userId: string;
  userName: string;
  role: StoreRole;
  roleName: string;
  isActive: boolean;
  assignedAt: Date;
  assignedByName: string;
  revokedAt?: Date;
  revokedByName?: string;
  notes?: string;
}

export interface StoreStaff {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: StoreRole;
  roleName: string;
  isActive: boolean;
  assignedAt: Date;
}

export interface AssignStoreUserRoleDto {
  userId: string;
  role: StoreRole;
  notes?: string;
}

export const StoreRoleLabels: { [key: number]: string } = {
  [StoreRole.StoreKeeper]: 'Store Keeper',
  [StoreRole.StoreManager]: 'Store Manager',
  [StoreRole.StoreAdmin]: 'Store Admin'
};
