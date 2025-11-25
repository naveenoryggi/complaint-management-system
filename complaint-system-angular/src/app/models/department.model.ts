export interface Department {
  id: string;
  branchId: string;
  name: string;
  code: string;
  description?: string;
  managerId?: string;
  managerName?: string;
  secondaryManagerId?: string;
  secondaryManagerName?: string;
  hrResponsibleId?: string;
  hrResponsibleName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDepartmentRequest {
  branchId: string;
  name: string;
  code: string;
  description?: string;
  managerId?: string;
  secondaryManagerId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}

export interface UpdateDepartmentRequest {
  name: string;
  code: string;
  description?: string;
  managerId?: string;
  secondaryManagerId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}
