import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  ProductSubType,
  ProductSubTypeSummary,
  ProductSubTypeLookup,
  CreateProductSubTypeRequest,
  UpdateProductSubTypeRequest,
  PagedProductSubTypeResult
} from '../models/product-subtype.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ProductSubTypeService {
  private get baseUrl(): string {
    return `${runtimeConfig.apiUrl}/productsubtypes`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all sub-types with pagination and filtering
   */
  getSubTypes(params?: {
    search?: string;
    categoryId?: string;
    isActive?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<ApiResponse<PagedProductSubTypeResult>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params?.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<ApiResponse<PagedProductSubTypeResult>>(this.baseUrl, { params: httpParams });
  }

  /**
   * Get a sub-type by ID
   */
  getSubTypeById(id: string): Observable<ApiResponse<ProductSubType>> {
    return this.http.get<ApiResponse<ProductSubType>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get a sub-type by code within a category
   */
  getSubTypeByCode(categoryId: string, code: string): Observable<ApiResponse<ProductSubType>> {
    return this.http.get<ApiResponse<ProductSubType>>(`${this.baseUrl}/by-code/${categoryId}/${code}`);
  }

  /**
   * Create a new sub-type
   */
  createSubType(request: CreateProductSubTypeRequest): Observable<ApiResponse<ProductSubType>> {
    return this.http.post<ApiResponse<ProductSubType>>(this.baseUrl, request);
  }

  /**
   * Update an existing sub-type
   */
  updateSubType(id: string, request: UpdateProductSubTypeRequest): Observable<ApiResponse<ProductSubType>> {
    return this.http.put<ApiResponse<ProductSubType>>(`${this.baseUrl}/${id}`, request);
  }

  /**
   * Delete a sub-type
   */
  deleteSubType(id: string): Observable<ApiResponse<{ message: string }>> {
    return this.http.delete<ApiResponse<{ message: string }>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get sub-type lookup for dropdowns (optionally filtered by category)
   */
  getSubTypeLookup(categoryId?: string): Observable<ApiResponse<ProductSubTypeLookup[]>> {
    let httpParams = new HttpParams();
    if (categoryId) httpParams = httpParams.set('categoryId', categoryId);

    return this.http.get<ApiResponse<ProductSubTypeLookup[]>>(`${this.baseUrl}/lookup`, { params: httpParams });
  }

  /**
   * Check if sub-type code exists within a category
   */
  checkCodeExists(categoryId: string, code: string, excludeId?: string): Observable<ApiResponse<boolean>> {
    let httpParams = new HttpParams();
    if (excludeId) httpParams = httpParams.set('excludeId', excludeId);

    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/code-exists/${categoryId}/${code}`, { params: httpParams });
  }
}
