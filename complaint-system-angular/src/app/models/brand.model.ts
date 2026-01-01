// Brand Models

export interface Brand {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  country?: string;
  isActive: boolean;
  displayOrder: number;
  notes?: string;

  // Statistics
  productCount: number;

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface BrandSummary {
  id: string;
  code: string;
  name: string;
  logoUrl?: string;
  country?: string;
  isActive: boolean;
  productCount: number;
}

export interface BrandLookup {
  id: string;
  code: string;
  name: string;
}

export interface CreateBrandRequest {
  code: string;
  name: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  country?: string;
  displayOrder: number;
  notes?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  country?: string;
  isActive: boolean;
  displayOrder: number;
  notes?: string;
}

export interface PagedBrandResult {
  items: BrandSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
