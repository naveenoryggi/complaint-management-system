import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import { Branch, CreateBranchRequest, UpdateBranchRequest } from '../models/branch.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class BranchService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/branches`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all branches for a company
   * @param companyId The company ID
   * @param activeOnly Filter to only active branches
   */
  getBranches(companyId: string, activeOnly: boolean = false): Observable<Branch[]> {
    const params = new HttpParams()
      .set('companyId', companyId)
      .set('activeOnly', activeOnly.toString());

    return this.http.get<ApiResponse<Branch[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single branch by ID
   * @param id The branch ID
   */
  getBranchById(id: string): Observable<Branch> {
    return this.http.get<ApiResponse<Branch>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Get branch with its departments (hierarchy)
   * @param id The branch ID
   */
  getBranchHierarchy(id: string): Observable<any> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/hierarchy`)
      .pipe(map(response => response.data));
  }

  /**
   * Create a new branch
   * @param request The branch creation request
   */
  createBranch(request: CreateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(this.apiUrl, request);
  }

  /**
   * Update an existing branch
   * @param id The branch ID
   * @param request The branch update request
   */
  updateBranch(id: string, request: UpdateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.put<ApiResponse<Branch>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a branch
   * @param id The branch ID
   */
  deleteBranch(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
