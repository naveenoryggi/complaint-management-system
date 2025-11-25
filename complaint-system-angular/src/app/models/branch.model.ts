export interface Branch {
  id: string;
  companyId: string;
  name: string;
  code: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  city?: string;
  country?: string;
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

export interface CreateBranchRequest {
  companyId: string;
  name: string;
  code: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  city?: string;
  country?: string;
  managerId?: string;
  secondaryManagerId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}

export interface UpdateBranchRequest {
  name: string;
  code: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  city?: string;
  country?: string;
  managerId?: string;
  secondaryManagerId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}
