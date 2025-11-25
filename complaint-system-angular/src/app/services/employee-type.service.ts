import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { EmployeeType, CreateEmployeeTypeRequest, UpdateEmployeeTypeRequest } from '../models/employee-type.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class EmployeeTypeService {
  private apiUrl = `${environment.apiUrl}/employeetypes`;

  constructor(private http: HttpClient) {}

  /**
   * Get all employee types for a company
   * @param companyId The company ID
   * @param activeOnly Filter to only active employee types
   */
  getEmployeeTypes(companyId: string, activeOnly: boolean = false): Observable<EmployeeType[]> {
    const params = new HttpParams()
      .set('companyId', companyId)
      .set('activeOnly', activeOnly.toString());

    return this.http.get<ApiResponse<EmployeeType[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single employee type by ID
   * @param id The employee type ID
   */
  getEmployeeTypeById(id: string): Observable<EmployeeType> {
    return this.http.get<ApiResponse<EmployeeType>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Get employee type with its users (hierarchy)
   * @param id The employee type ID
   */
  getEmployeeTypeHierarchy(id: string): Observable<any> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/hierarchy`)
      .pipe(map(response => response.data));
  }

  /**
   * Create a new employee type
   * @param request The employee type creation request
   */
  createEmployeeType(request: CreateEmployeeTypeRequest): Observable<ApiResponse<EmployeeType>> {
    return this.http.post<ApiResponse<EmployeeType>>(this.apiUrl, request);
  }

  /**
   * Update an existing employee type
   * @param id The employee type ID
   * @param request The employee type update request
   */
  updateEmployeeType(id: string, request: UpdateEmployeeTypeRequest): Observable<ApiResponse<EmployeeType>> {
    return this.http.put<ApiResponse<EmployeeType>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete an employee type
   * @param id The employee type ID
   */
  deleteEmployeeType(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
