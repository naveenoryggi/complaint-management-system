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

export interface ComplaintStatusMaster {
  id: string;
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
  colorCode?: string;
  iconClass?: string;
  isActive: boolean;
  isSystem: boolean;
  isFinal: boolean;
  companyId?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateStatusMasterRequest {
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
  colorCode?: string;
  iconClass?: string;
  isActive: boolean;
  isFinal: boolean;
  companyId?: string;
}

export interface UpdateStatusMasterRequest {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  colorCode?: string;
  iconClass?: string;
  isActive: boolean;
  isFinal: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class StatusMasterService {
  private apiUrl = `${environment.apiUrl}/complaintstatusmaster`;

  constructor(private http: HttpClient) {}

  /**
   * Get all complaint status masters
   * @param companyId Optional company filter
   * @param isActive Optional active status filter
   * @param includeSystem Whether to include system-level statuses (default: true)
   */
  getStatuses(companyId?: string, isActive?: boolean, includeSystem: boolean = true): Observable<ApiResponse<ComplaintStatusMaster[]>> {
    let params = new HttpParams();

    if (companyId) {
      params = params.set('companyId', companyId);
    }
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    params = params.set('includeSystem', includeSystem.toString());

    return this.http.get<ApiResponse<ComplaintStatusMaster[]>>(this.apiUrl, { params });
  }

  /**
   * Create a new complaint status
   */
  createStatus(request: CreateStatusMasterRequest): Observable<ApiResponse<ComplaintStatusMaster>> {
    return this.http.post<ApiResponse<ComplaintStatusMaster>>(this.apiUrl, request);
  }

  /**
   * Update an existing complaint status
   */
  updateStatus(id: string, request: UpdateStatusMasterRequest): Observable<ApiResponse<ComplaintStatusMaster>> {
    return this.http.put<ApiResponse<ComplaintStatusMaster>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a complaint status (soft delete)
   */
  deleteStatus(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
