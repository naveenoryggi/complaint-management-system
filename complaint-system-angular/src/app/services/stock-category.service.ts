import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface StockCategory {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description?: string;
  colorCode?: string;
  icon?: string;
  isForSale: boolean;
  isFixedAsset: boolean;
  isServiceStock: boolean;
  isFaulty: boolean;
  isDemo: boolean;
  requiresSpecialHandling: boolean;
  allowsCustomerAssignment: boolean;
  allowsEmployeeAssignment: boolean;
  sortOrder: number;
  isActive: boolean;
  isSystem: boolean;
  assetCount: number;
  createdAt: Date;
  updatedAt?: Date;
}

export interface StockCategoryLookup {
  id: string;
  code: string;
  name: string;
  colorCode?: string;
  isActive: boolean;
}

export interface CreateStockCategoryRequest {
  code: string;
  name: string;
  description?: string;
  colorCode?: string;
  icon?: string;
  isForSale?: boolean;
  isFixedAsset?: boolean;
  isServiceStock?: boolean;
  isFaulty?: boolean;
  isDemo?: boolean;
  requiresSpecialHandling?: boolean;
  allowsCustomerAssignment?: boolean;
  allowsEmployeeAssignment?: boolean;
  sortOrder?: number;
  isActive?: boolean;
}

export interface UpdateStockCategoryRequest extends CreateStockCategoryRequest {}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class StockCategoryService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/StockCategories`;
  }

  constructor(private http: HttpClient) {}

  getAll(
    page: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    isActive?: boolean
  ): Observable<ApiResponse<PagedResult<StockCategory>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<ApiResponse<PagedResult<StockCategory>>>(this.apiUrl, { params });
  }

  getLookup(activeOnly: boolean = true): Observable<ApiResponse<StockCategoryLookup[]>> {
    const params = new HttpParams().set('activeOnly', activeOnly.toString());
    return this.http.get<ApiResponse<StockCategoryLookup[]>>(`${this.apiUrl}/lookup`, { params });
  }

  getById(id: string): Observable<ApiResponse<StockCategory>> {
    return this.http.get<ApiResponse<StockCategory>>(`${this.apiUrl}/${id}`);
  }

  getByCode(code: string): Observable<ApiResponse<StockCategory>> {
    return this.http.get<ApiResponse<StockCategory>>(`${this.apiUrl}/code/${code}`);
  }

  create(request: CreateStockCategoryRequest): Observable<ApiResponse<StockCategory>> {
    return this.http.post<ApiResponse<StockCategory>>(this.apiUrl, request);
  }

  update(id: string, request: UpdateStockCategoryRequest): Observable<ApiResponse<StockCategory>> {
    return this.http.put<ApiResponse<StockCategory>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  seedDefaults(): Observable<any> {
    return this.http.post(`${this.apiUrl}/seed`, {});
  }
}
