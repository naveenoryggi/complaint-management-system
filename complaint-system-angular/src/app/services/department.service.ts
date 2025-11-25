import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Department, CreateDepartmentRequest, UpdateDepartmentRequest } from '../models/department.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private apiUrl = `${environment.apiUrl}/departments`;

  constructor(private http: HttpClient) {}

  /**
   * Get all departments for a branch
   * @param branchId The branch ID
   * @param activeOnly Filter to only active departments
   */
  getDepartments(branchId: string, activeOnly: boolean = false): Observable<Department[]> {
    const params = new HttpParams()
      .set('branchId', branchId)
      .set('activeOnly', activeOnly.toString());

    return this.http.get<ApiResponse<Department[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single department by ID
   * @param id The department ID
   */
  getDepartmentById(id: string): Observable<Department> {
    return this.http.get<ApiResponse<Department>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Get department with its sections (hierarchy)
   * @param id The department ID
   */
  getDepartmentHierarchy(id: string): Observable<any> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/hierarchy`)
      .pipe(map(response => response.data));
  }

  /**
   * Create a new department
   * @param request The department creation request
   */
  createDepartment(request: CreateDepartmentRequest): Observable<ApiResponse<Department>> {
    return this.http.post<ApiResponse<Department>>(this.apiUrl, request);
  }

  /**
   * Update an existing department
   * @param id The department ID
   * @param request The department update request
   */
  updateDepartment(id: string, request: UpdateDepartmentRequest): Observable<ApiResponse<Department>> {
    return this.http.put<ApiResponse<Department>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a department
   * @param id The department ID
   */
  deleteDepartment(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
