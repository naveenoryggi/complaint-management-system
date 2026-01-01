// Product Sub-Type Models

export interface ProductSubType {
  id: string;
  companyId: string;
  categoryId: string;
  categoryName: string;
  code: string;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  displayOrder: number;
  isActive: boolean;
  notes?: string;

  // Statistics
  productCount: number;

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface ProductSubTypeSummary {
  id: string;
  categoryId: string;
  categoryName: string;
  code: string;
  name: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  productCount: number;
}

export interface ProductSubTypeLookup {
  id: string;
  categoryId: string;
  code: string;
  name: string;
}

export interface CreateProductSubTypeRequest {
  categoryId: string;
  code: string;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  displayOrder: number;
  notes?: string;
}

export interface UpdateProductSubTypeRequest {
  categoryId: string;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  displayOrder: number;
  notes?: string;
}

export interface PagedProductSubTypeResult {
  items: ProductSubTypeSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
