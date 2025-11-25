export interface EmployeeType {
  id: string;
  companyId: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateEmployeeTypeRequest {
  companyId: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
}

export interface UpdateEmployeeTypeRequest {
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
}
