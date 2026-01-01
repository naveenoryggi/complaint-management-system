import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Brand,
  BrandSummary,
  BrandLookup,
  CreateBrandRequest,
  UpdateBrandRequest,
  PagedBrandResult
} from '../models/brand.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private readonly baseUrl = `${environment.apiUrl}/brands`;

  constructor(private http: HttpClient) {}

  /**
   * Get all brands with pagination and filtering
   */
  getBrands(params?: {
    search?: string;
    isActive?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<ApiResponse<PagedBrandResult>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params?.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<ApiResponse<PagedBrandResult>>(this.baseUrl, { params: httpParams });
  }

  /**
   * Get a brand by ID
   */
  getBrandById(id: string): Observable<ApiResponse<Brand>> {
    return this.http.get<ApiResponse<Brand>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get a brand by code
   */
  getBrandByCode(code: string): Observable<ApiResponse<Brand>> {
    return this.http.get<ApiResponse<Brand>>(`${this.baseUrl}/by-code/${code}`);
  }

  /**
   * Create a new brand
   */
  createBrand(request: CreateBrandRequest): Observable<ApiResponse<Brand>> {
    return this.http.post<ApiResponse<Brand>>(this.baseUrl, request);
  }

  /**
   * Update an existing brand
   */
  updateBrand(id: string, request: UpdateBrandRequest): Observable<ApiResponse<Brand>> {
    return this.http.put<ApiResponse<Brand>>(`${this.baseUrl}/${id}`, request);
  }

  /**
   * Delete a brand
   */
  deleteBrand(id: string): Observable<ApiResponse<{ message: string }>> {
    return this.http.delete<ApiResponse<{ message: string }>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get brand lookup for dropdowns
   */
  getBrandLookup(): Observable<ApiResponse<BrandLookup[]>> {
    return this.http.get<ApiResponse<BrandLookup[]>>(`${this.baseUrl}/lookup`);
  }

  /**
   * Check if brand code exists
   */
  checkCodeExists(code: string, excludeId?: string): Observable<ApiResponse<boolean>> {
    let httpParams = new HttpParams();
    if (excludeId) httpParams = httpParams.set('excludeId', excludeId);

    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/code-exists/${code}`, { params: httpParams });
  }
}
