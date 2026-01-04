import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  Store,
  CreateStoreDto,
  UpdateStoreDto,
  StoreLookup,
  AssignStoreManagersDto,
  StoreStaff,
  StoreUserRole,
  AssignStoreUserRoleDto
} from '../models/store.model';

export interface ApiResult<T> {
  isSuccess: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/stores`;
  }

  constructor(private http: HttpClient) {}

  // Store CRUD Operations

  getStores(includeInactive: boolean = false): Observable<ApiResult<Store[]>> {
    return this.http.get<ApiResult<Store[]>>(`${this.apiUrl}?includeInactive=${includeInactive}`);
  }

  getStoreLookup(): Observable<ApiResult<StoreLookup[]>> {
    return this.http.get<ApiResult<StoreLookup[]>>(`${this.apiUrl}/lookup`);
  }

  getStore(id: string): Observable<ApiResult<Store>> {
    return this.http.get<ApiResult<Store>>(`${this.apiUrl}/${id}`);
  }

  createStore(dto: CreateStoreDto): Observable<ApiResult<Store>> {
    return this.http.post<ApiResult<Store>>(this.apiUrl, dto);
  }

  updateStore(id: string, dto: UpdateStoreDto): Observable<ApiResult<Store>> {
    return this.http.put<ApiResult<Store>>(`${this.apiUrl}/${id}`, dto);
  }

  deleteStore(id: string): Observable<ApiResult<void>> {
    return this.http.delete<ApiResult<void>>(`${this.apiUrl}/${id}`);
  }

  // Store Manager Operations

  assignManagers(id: string, dto: AssignStoreManagersDto): Observable<ApiResult<Store>> {
    return this.http.put<ApiResult<Store>>(`${this.apiUrl}/${id}/managers`, dto);
  }

  getManagedStores(): Observable<ApiResult<StoreLookup[]>> {
    return this.http.get<ApiResult<StoreLookup[]>>(`${this.apiUrl}/managed`);
  }

  // Store Staff Operations

  getStoreStaff(storeId: string): Observable<ApiResult<StoreStaff[]>> {
    return this.http.get<ApiResult<StoreStaff[]>>(`${this.apiUrl}/${storeId}/staff`);
  }

  assignStoreRole(storeId: string, dto: AssignStoreUserRoleDto): Observable<ApiResult<StoreUserRole>> {
    return this.http.post<ApiResult<StoreUserRole>>(`${this.apiUrl}/${storeId}/staff`, dto);
  }

  revokeStoreRole(roleId: string): Observable<ApiResult<void>> {
    return this.http.delete<ApiResult<void>>(`${this.apiUrl}/staff/${roleId}`);
  }

  getMyStoreRoles(): Observable<ApiResult<StoreUserRole[]>> {
    return this.http.get<ApiResult<StoreUserRole[]>>(`${this.apiUrl}/my-roles`);
  }
}
