import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Role,
  CreateRoleRequest,
  UpdateRoleRequest,
  UpdateRolePermissionsRequest,
  AssignRoleToUserRequest,
  UserRoleAssignment,
  ApiResponse,
  PermissionType
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = `${environment.apiUrl}/roles`;

  constructor(private http: HttpClient) {}

  /**
   * Get all roles
   * @param includeInactive Whether to include inactive roles
   */
  getAllRoles(includeInactive: boolean = false): Observable<ApiResponse<Role[]>> {
    const params = new HttpParams().set('includeInactive', includeInactive.toString());
    return this.http.get<ApiResponse<Role[]>>(this.apiUrl, { params });
  }

  /**
   * Get role by ID
   * @param id Role ID
   */
  getRoleById(id: string): Observable<ApiResponse<Role>> {
    return this.http.get<ApiResponse<Role>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create new role
   * @param request Create role request
   */
  createRole(request: CreateRoleRequest): Observable<ApiResponse<Role>> {
    return this.http.post<ApiResponse<Role>>(this.apiUrl, request);
  }

  /**
   * Update role
   * @param id Role ID
   * @param request Update role request
   */
  updateRole(id: string, request: UpdateRoleRequest): Observable<ApiResponse<Role>> {
    return this.http.put<ApiResponse<Role>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete role (soft delete)
   * @param id Role ID
   */
  deleteRole(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Update role permissions
   * @param id Role ID
   * @param request Update permissions request
   */
  updateRolePermissions(id: string, request: UpdateRolePermissionsRequest): Observable<ApiResponse<Role>> {
    return this.http.put<ApiResponse<Role>>(`${this.apiUrl}/${id}/permissions`, request);
  }

  /**
   * Get all available permissions
   */
  getAvailablePermissions(): Observable<ApiResponse<{ [key: string]: number }>> {
    return this.http.get<ApiResponse<{ [key: string]: number }>>(`${this.apiUrl}/permissions`);
  }

  /**
   * Assign role to user
   * @param request Assign role request
   */
  assignRoleToUser(request: AssignRoleToUserRequest): Observable<ApiResponse<UserRoleAssignment>> {
    return this.http.post<ApiResponse<UserRoleAssignment>>(`${this.apiUrl}/assign`, request);
  }

  /**
   * Remove role from user
   * @param userRoleId User role assignment ID
   */
  removeRoleFromUser(userRoleId: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/assign/${userRoleId}`);
  }

  /**
   * Get user's role assignments
   * @param userId User ID
   */
  getUserRoles(userId: string): Observable<ApiResponse<UserRoleAssignment[]>> {
    return this.http.get<ApiResponse<UserRoleAssignment[]>>(`${this.apiUrl}/user/${userId}/roles`);
  }

  /**
   * Helper method to get permission type name
   * @param permissionType Permission type enum value
   */
  getPermissionName(permissionType: PermissionType): string {
    return PermissionType[permissionType];
  }

  /**
   * Helper method to get all permission types as array
   */
  getAllPermissionTypes(): { value: PermissionType; label: string }[] {
    return Object.keys(PermissionType)
      .filter(key => isNaN(Number(key)))
      .map(key => ({
        value: PermissionType[key as keyof typeof PermissionType],
        label: this.formatPermissionLabel(key)
      }));
  }

  /**
   * Format permission label for display
   * @param key Permission key
   */
  private formatPermissionLabel(key: string): string {
    return key.replace(/([A-Z])/g, ' $1').trim();
  }
}
