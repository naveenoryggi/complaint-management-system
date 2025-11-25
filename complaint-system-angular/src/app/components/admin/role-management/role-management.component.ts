import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoleService } from '../../../services/role.service';
import {
  Role,
  CreateRoleRequest,
  UpdateRoleRequest,
  RoleType,
  PermissionType,
  Permission,
  RolePermissionUpdate
} from '../../../models/role.model';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './role-management.component.html',
  styleUrls: ['./role-management.component.scss']
})
export class RoleManagementComponent implements OnInit {
  roles: Role[] = [];
  filteredRoles: Role[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';
  showActiveOnly = true;

  // Modal state
  showModal = false;
  modalMode: 'create' | 'edit' | 'permissions' = 'create';
  modalTitle = '';

  // Form models
  roleForm: any = this.getEmptyRoleForm();
  selectedRole: Role | null = null;

  // Permission management
  availablePermissions: { value: PermissionType; label: string }[] = [];
  selectedPermissions: Set<PermissionType> = new Set();

  // Enums for template
  RoleType = RoleType;
  PermissionType = PermissionType;

  // Role types for dropdown
  roleTypes = [
    { value: RoleType.SystemAdmin, label: 'System Admin' },
    { value: RoleType.TenantAdmin, label: 'Tenant Admin' },
    { value: RoleType.CompanyAdmin, label: 'Company Admin' },
    { value: RoleType.DepartmentManager, label: 'Department Manager' },
    { value: RoleType.Level1Handler, label: 'Level 1 Handler' },
    { value: RoleType.Level2Handler, label: 'Level 2 Handler' },
    { value: RoleType.Level3Handler, label: 'Level 3 Handler' },
    { value: RoleType.Level4Handler, label: 'Level 4 Handler' },
    { value: RoleType.Level5Handler, label: 'Level 5 Handler' },
    { value: RoleType.HRRepresentative, label: 'HR Representative' },
    { value: RoleType.Complainant, label: 'Complainant' },
    { value: RoleType.Viewer, label: 'Viewer' },
    { value: RoleType.ReportingManager, label: 'Reporting Manager' },
    { value: RoleType.PrimaryContact, label: 'Primary Contact' },
    { value: RoleType.SecondaryContact, label: 'Secondary Contact' }
  ];

  // Delete confirmation
  showDeleteConfirm = false;
  roleToDelete: Role | null = null;
  deleteLoading = false;

  constructor(private roleService: RoleService) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadAvailablePermissions();
  }

  getEmptyRoleForm(): CreateRoleRequest {
    return {
      name: '',
      code: '',
      description: '',
      roleType: RoleType.Viewer,
      escalationLevel: 0,
      displayOrder: 0,
      permissions: []
    };
  }

  loadRoles(): void {
    this.loading = true;
    this.errorMessage = '';

    this.roleService.getAllRoles(!this.showActiveOnly).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.roles = response.data;
          this.filterRoles();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load roles. Please try again.';
        this.loading = false;
        console.error('Error loading roles:', error);
      }
    });
  }

  loadAvailablePermissions(): void {
    this.availablePermissions = this.roleService.getAllPermissionTypes();
  }

  filterRoles(): void {
    let filtered = this.roles;

    // Filter by active status
    if (this.showActiveOnly) {
      filtered = filtered.filter(r => r.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(r =>
        r.name.toLowerCase().includes(search) ||
        r.code.toLowerCase().includes(search) ||
        (r.description && r.description.toLowerCase().includes(search))
      );
    }

    this.filteredRoles = filtered.sort((a, b) => a.displayOrder - b.displayOrder);
  }

  onSearchChange(): void {
    this.filterRoles();
  }

  onFilterChange(): void {
    this.loadRoles();
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.modalTitle = 'Create New Role';
    this.roleForm = this.getEmptyRoleForm();
    this.selectedPermissions = new Set();
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(role: Role): void {
    this.modalMode = 'edit';
    this.modalTitle = 'Edit Role';
    this.selectedRole = role;
    this.roleForm = {
      id: role.id,
      name: role.name,
      description: role.description || '',
      escalationLevel: role.escalationLevel,
      displayOrder: role.displayOrder,
      isActive: role.isActive
    };
    this.showModal = true;
    this.errorMessage = '';
  }

  openPermissionsModal(role: Role): void {
    this.modalMode = 'permissions';
    this.modalTitle = `Manage Permissions: ${role.name}`;
    this.selectedRole = role;
    this.selectedPermissions = new Set(
      role.permissions.filter(p => p.isGranted).map(p => p.permissionType)
    );
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.roleForm = this.getEmptyRoleForm();
    this.selectedRole = null;
    this.selectedPermissions = new Set();
    this.errorMessage = '';
  }

  togglePermission(permissionType: PermissionType): void {
    if (this.selectedPermissions.has(permissionType)) {
      this.selectedPermissions.delete(permissionType);
    } else {
      this.selectedPermissions.add(permissionType);
    }
  }

  isPermissionSelected(permissionType: PermissionType): boolean {
    return this.selectedPermissions.has(permissionType);
  }

  saveRole(): void {
    // Validation
    if (this.modalMode === 'create') {
      if (!this.roleForm.name || !this.roleForm.code) {
        this.errorMessage = 'Role name and code are required';
        return;
      }
    } else if (this.modalMode === 'edit') {
      if (!this.roleForm.name) {
        this.errorMessage = 'Role name is required';
        return;
      }
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.roleForm.permissions = Array.from(this.selectedPermissions);

      this.roleService.createRole(this.roleForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Role created successfully';
            this.closeModal();
            this.loadRoles();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create role';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create role. Please try again.';
          this.loading = false;
          console.error('Error creating role:', error);
        }
      });
    } else if (this.modalMode === 'edit') {
      if (!this.roleForm.id) return;

      this.roleService.updateRole(this.roleForm.id, this.roleForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Role updated successfully';
            this.closeModal();
            this.loadRoles();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update role';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update role. Please try again.';
          this.loading = false;
          console.error('Error updating role:', error);
        }
      });
    } else if (this.modalMode === 'permissions') {
      this.savePermissions();
    }
  }

  savePermissions(): void {
    if (!this.selectedRole) return;

    this.loading = true;
    this.errorMessage = '';

    const permissionUpdates: RolePermissionUpdate[] = this.availablePermissions.map(p => ({
      permissionType: p.value,
      isGranted: this.selectedPermissions.has(p.value)
    }));

    this.roleService.updateRolePermissions(this.selectedRole.id, {
      permissions: permissionUpdates
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Permissions updated successfully';
          this.closeModal();
          this.loadRoles();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || 'Failed to update permissions';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update permissions. Please try again.';
        this.loading = false;
        console.error('Error updating permissions:', error);
      }
    });
  }

  openDeleteConfirm(role: Role): void {
    this.roleToDelete = role;
    this.showDeleteConfirm = true;
    this.errorMessage = '';
  }

  closeDeleteConfirm(): void {
    this.showDeleteConfirm = false;
    this.roleToDelete = null;
  }

  confirmDelete(): void {
    if (!this.roleToDelete) return;

    this.deleteLoading = true;
    this.errorMessage = '';

    this.roleService.deleteRole(this.roleToDelete.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Role deleted successfully';
          this.closeDeleteConfirm();
          this.loadRoles();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || 'Failed to delete role';
        }
        this.deleteLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete role. Please try again.';
        this.deleteLoading = false;
        console.error('Error deleting role:', error);
      }
    });
  }

  getRoleTypeName(roleType: RoleType): string {
    const type = this.roleTypes.find(t => t.value === roleType);
    return type ? type.label : 'Unknown';
  }

  getPermissionCount(role: Role): number {
    return role.permissions.filter(p => p.isGranted).length;
  }
}

