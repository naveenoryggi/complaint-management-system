import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface ComplaintPriorityMaster {
  id: string;
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
  level: number;
  colorCode?: string;
  iconClass?: string;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  isActive: boolean;
  isSystem: boolean;
  companyId?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreatePriorityMasterRequest {
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
  level: number;
  colorCode?: string;
  iconClass?: string;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  isActive: boolean;
  companyId?: string;
}

export interface UpdatePriorityMasterRequest {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  level: number;
  colorCode?: string;
  iconClass?: string;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PriorityMasterService {
  private apiUrl = `${environment.apiUrl}/ComplaintPriorityMaster`;

  constructor(private http: HttpClient) {}

  /**
   * Get all complaint priority masters
   * @param companyId Optional company filter
   * @param isActive Optional active status filter
   * @param includeSystem Whether to include system-level priorities (default: true)
   */
  getPriorities(companyId?: string, isActive?: boolean, includeSystem: boolean = true): Observable<ApiResponse<ComplaintPriorityMaster[]>> {
    let params = new HttpParams();

    if (companyId) {
      params = params.set('companyId', companyId);
    }
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    params = params.set('includeSystem', includeSystem.toString());

    return this.http.get<ApiResponse<ComplaintPriorityMaster[]>>(this.apiUrl, { params });
  }

  /**
   * Create a new complaint priority
   */
  createPriority(request: CreatePriorityMasterRequest): Observable<ApiResponse<ComplaintPriorityMaster>> {
    return this.http.post<ApiResponse<ComplaintPriorityMaster>>(this.apiUrl, request);
  }

  /**
   * Update an existing complaint priority
   */
  updatePriority(id: string, request: UpdatePriorityMasterRequest): Observable<ApiResponse<ComplaintPriorityMaster>> {
    return this.http.put<ApiResponse<ComplaintPriorityMaster>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a complaint priority (soft delete)
   */
  deletePriority(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
