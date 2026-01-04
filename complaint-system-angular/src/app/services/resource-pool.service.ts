import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import {
  ResourcePool,
  CreateResourcePoolRequest,
  UpdateResourcePoolRequest,
  AddResourcePoolMemberRequest,
  RemoveResourcePoolMemberRequest
} from '../models/escalation.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ResourcePoolService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/resource-pools`;
  }

  constructor(private http: HttpClient) {}

  // Resource Pool CRUD Operations
  getAllPools(companyId?: string): Observable<ResourcePool[]> {
    let params = new HttpParams();
    if (companyId) {
      params = params.set('companyId', companyId);
    }
    return this.http.get<ApiResponse<ResourcePool[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data));
  }

  getPoolById(id: string): Observable<ResourcePool> {
    return this.http.get<ApiResponse<ResourcePool>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data));
  }

  createPool(request: CreateResourcePoolRequest): Observable<ResourcePool> {
    return this.http.post<ApiResponse<ResourcePool>>(this.apiUrl, request)
      .pipe(map(response => response.data));
  }

  updatePool(id: string, request: UpdateResourcePoolRequest): Observable<ResourcePool> {
    return this.http.put<ApiResponse<ResourcePool>>(`${this.apiUrl}/${id}`, request)
      .pipe(map(response => response.data));
  }

  deletePool(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(map(() => undefined));
  }

  // Member Management
  addMember(poolId: string, request: AddResourcePoolMemberRequest): Observable<ResourcePool> {
    return this.http.post<ApiResponse<ResourcePool>>(`${this.apiUrl}/${poolId}/members`, request)
      .pipe(map(response => response.data));
  }

  removeMember(poolId: string, userId: string): Observable<ResourcePool> {
    return this.http.delete<ApiResponse<ResourcePool>>(`${this.apiUrl}/${poolId}/members/${userId}`)
      .pipe(map(response => response.data));
  }

  // Helper methods for filtering
  getPoolsByBranch(branchId: string): Observable<ResourcePool[]> {
    const params = new HttpParams().set('branchId', branchId);
    return this.http.get<ApiResponse<ResourcePool[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data));
  }

  getPoolsByDepartment(departmentId: string): Observable<ResourcePool[]> {
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http.get<ApiResponse<ResourcePool[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data));
  }

  getPoolsBySection(sectionId: string): Observable<ResourcePool[]> {
    const params = new HttpParams().set('sectionId', sectionId);
    return this.http.get<ApiResponse<ResourcePool[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data));
  }
}
