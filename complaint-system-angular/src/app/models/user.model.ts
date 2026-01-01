export interface User {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  phoneNumber?: string; // Alias for phone property
  jobTitle?: string;
  companyId: string;
  companyName: string;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  employeeTypeId?: string;
  employeeTypeName?: string;
  managerId?: string;
  managerName?: string;
  roles: UserRole[];
  permissions: string[];
  isActive?: boolean;
  isPortalUser?: boolean;  // True if user is a customer portal user

  // Timezone & Localization
  timeZone: string; // Resolved timezone (fallback chain applied)
  preferredTimeZone?: string; // User's personal preference
  preferredLocale?: string; // e.g., "en-US", "fr-FR"
  preferredDateFormat?: string; // e.g., "MM/dd/yyyy", "dd/MM/yyyy"
  preferredTimeFormat?: string; // e.g., "hh:mm a", "HH:mm"
}

export interface UserRole {
  roleId: string;
  roleName: string;
  roleCode: string;
  roleType: string;
  escalationLevel: number;
  isPrimary: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  isSuccess: boolean;
  message: string;
  data: {
    token: string;
    refreshToken: string;
    expiresAt: string;
    user: User;
  };
}

export interface CreateUserRequest {
  companyId: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  jobTitle?: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  employeeTypeId?: string;
  managerId?: string;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  jobTitle?: string;
  branchId?: string;
  departmentId?: string;
  sectionId?: string;
  employeeTypeId?: string;
  managerId?: string;
  isActive: boolean;
}
