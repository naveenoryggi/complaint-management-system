export interface Section {
  id: string;
  departmentId: string;
  name: string;
  code: string;
  description?: string;
  headId?: string;
  headName?: string;
  secondaryHeadId?: string;
  secondaryHeadName?: string;
  hrResponsibleId?: string;
  hrResponsibleName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateSectionRequest {
  departmentId: string;
  name: string;
  code: string;
  description?: string;
  headId?: string;
  secondaryHeadId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}

export interface UpdateSectionRequest {
  name: string;
  code: string;
  description?: string;
  headId?: string;
  secondaryHeadId?: string;
  hrResponsibleId?: string;
  isActive: boolean;
}
