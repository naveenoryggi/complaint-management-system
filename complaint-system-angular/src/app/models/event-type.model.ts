export interface EventType {
  id: string;
  name: string;
  code: string;
  description?: string;
  entityType: string;
  category?: string;
  isActive: boolean;
  isSystem: boolean;
  availableFields?: string;
  iconClass?: string;
  companyId?: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface CreateEventTypeRequest {
  name: string;
  code: string;
  description?: string;
  entityType: string;
  category?: string;
  isActive: boolean;
  availableFields?: string;
  iconClass?: string;
  companyId?: string;
}

export interface UpdateEventTypeRequest {
  id: string;
  name: string;
  code: string;
  description?: string;
  entityType: string;
  category?: string;
  isActive: boolean;
  availableFields?: string;
  iconClass?: string;
}

export interface EventTypeFilters {
  includeInactive?: boolean;
  entityType?: string;
  category?: string;
  companyId?: string;
}
