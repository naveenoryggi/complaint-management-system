import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DepartmentService } from '../../../services/department.service';
import { BranchService } from '../../../services/branch.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { Department, CreateDepartmentRequest, UpdateDepartmentRequest } from '../../../models/department.model';
import { Branch } from '../../../models/branch.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-department-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UserAutocompleteComponent, UtcToLocalPipe],
  templateUrl: './department-management.component.html',
  styleUrls: ['./department-management.component.scss']
})
export class DepartmentManagementComponent
  extends BaseMasterManagementComponent<Department, CreateDepartmentRequest, UpdateDepartmentRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateDepartmentRequest & { id?: string }) | (UpdateDepartmentRequest & { code?: string });

  // User display names for autocomplete
  managerName?: string;
  secondaryManagerName?: string;
  hrResponsibleName?: string;

  // Department-specific properties
  branches: Branch[] = [];
  selectedBranchId = '';

  protected override get entityName(): string {
    return 'Department';
  }

  // Getter/setter for code property (for backward compatibility)
  get departmentCode(): string {
    return this.formCode;
  }

  set departmentCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for department-specific fields
  get branchId(): string {
    return (this.form as any).branchId || '';
  }

  set branchId(value: string) {
    (this.form as any).branchId = value;
  }

  get managerId(): string | undefined {
    return (this.form as any).managerId;
  }

  set managerId(value: string | undefined) {
    (this.form as any).managerId = value;
  }

  get secondaryManagerId(): string | undefined {
    return (this.form as any).secondaryManagerId;
  }

  set secondaryManagerId(value: string | undefined) {
    (this.form as any).secondaryManagerId = value;
  }

  get hrResponsibleId(): string | undefined {
    return (this.form as any).hrResponsibleId;
  }

  set hrResponsibleId(value: string | undefined) {
    (this.form as any).hrResponsibleId = value;
  }

  // Status filter for departments (3-way filter like Branch)
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  constructor(
    private departmentService: DepartmentService,
    private branchService: BranchService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    this.canManageSettings = this.authService.hasPermission('ManageSettings');
    this.loadBranches();
    this.logger.info('Department Management initialized', undefined, 'DepartmentManagementComponent');
  }

  protected override getEmptyForm(): CreateDepartmentRequest {
    return {
      branchId: this.selectedBranchId || '',
      name: '',
      code: '',
      description: '',
      managerId: undefined,
      secondaryManagerId: undefined,
      hrResponsibleId: undefined,
      isActive: true
    };
  }

  loadBranches(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated';
      this.logger.error('User not authenticated', undefined, 'DepartmentManagementComponent');
      return;
    }

    this.branchService.getBranches(currentUser.companyId, false).subscribe({
      next: (data) => {
        this.branches = data;
        // Auto-select first branch if available
        if (this.branches.length > 0 && !this.selectedBranchId) {
          this.selectedBranchId = this.branches[0].id;
          this.onBranchChange();
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load branches';
        this.logger.error('Error loading branches', error, 'DepartmentManagementComponent');
      }
    });
  }

  onBranchChange(): void {
    if (this.selectedBranchId) {
      this.loadItems();
    } else {
      this.items = [];
      this.filteredItems = [];
    }
  }

  protected override loadItems(): void {
    if (!this.selectedBranchId) {
      this.items = [];
      this.filteredItems = [];
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.departmentService.getDepartments(this.selectedBranchId, false).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load departments. Please try again.';
        this.loading = false;
        this.logger.error('Error loading departments', error, 'DepartmentManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status (3-way filter)
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(d => d.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(d => !d.isActive);
    }
    // 'all' shows both active and inactive

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(d =>
        d.name.toLowerCase().includes(search) ||
        d.code.toLowerCase().includes(search) ||
        (d.description && d.description.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateDepartmentRequest): Observable<ApiResponse<Department>> {
    return this.departmentService.createDepartment(request);
  }

  protected override updateItem(id: string, request: UpdateDepartmentRequest): Observable<ApiResponse<Department>> {
    return this.departmentService.updateDepartment(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.departmentService.deleteDepartment(id) as Observable<ApiResponse<boolean>>;
  }

  override openEditModal(item: Department): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit departments';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Department';

    // Create edit form with all department-specific fields
    this.form = {
      id: item.id,
      branchId: item.branchId,
      name: item.name,
      code: item.code,
      description: item.description || '',
      managerId: item.managerId,
      secondaryManagerId: item.secondaryManagerId,
      hrResponsibleId: item.hrResponsibleId,
      isActive: item.isActive
    } as (CreateDepartmentRequest & { id?: string }) | (UpdateDepartmentRequest & { code?: string });

    // Set user display names
    this.managerName = item.managerName;
    this.secondaryManagerName = item.secondaryManagerName;
    this.hrResponsibleName = item.hrResponsibleName;

    this.showModal = true;
    this.errorMessage = '';
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate branch ID is set
    if (!this.branchId) {
      this.errorMessage = 'Branch is required';
      return false;
    }

    // Validate department code length
    if (this.formCode && this.formCode.length > 20) {
      this.errorMessage = 'Department code must be 20 characters or less';
      return false;
    }

    return true;
  }

  override save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.createItem(this.form as CreateDepartmentRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Department created successfully';
            this.logger.info('Department created', undefined, 'DepartmentManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create department';
            this.logger.error('Failed to create department', { message: response.message }, 'DepartmentManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create department. Please try again.';
          this.logger.error('Error creating department', error, 'DepartmentManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including department-specific ones
      const updateRequest = {
        name: this.formName,
        code: this.formCode,
        description: this.formDescription,
        managerId: this.managerId,
        secondaryManagerId: this.secondaryManagerId,
        hrResponsibleId: this.hrResponsibleId,
        isActive: this.formIsActive
      } as UpdateDepartmentRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Department updated successfully';
            this.logger.info('Department updated', undefined, 'DepartmentManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update department';
            this.logger.error('Failed to update department', { message: response.message }, 'DepartmentManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update department. Please try again.';
          this.logger.error('Error updating department', error, 'DepartmentManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get departments(): Department[] {
    return this.items;
  }

  get filteredDepartments(): Department[] {
    return this.filteredItems;
  }

  get departmentForm(): (CreateDepartmentRequest & { id?: string }) | (UpdateDepartmentRequest & { code?: string }) {
    return this.form;
  }

  set departmentForm(value: (CreateDepartmentRequest & { id?: string }) | (UpdateDepartmentRequest & { code?: string })) {
    this.form = value;
  }

  get departmentToDelete(): Department | null {
    return this.itemToDelete;
  }

  set departmentToDelete(value: Department | null) {
    this.itemToDelete = value;
  }

  // Backward compatibility for old naming conventions
  get isEditMode(): boolean {
    return this.modalMode === 'edit';
  }

  get showDeleteModal(): boolean {
    return this.showDeleteConfirm;
  }

  override openCreateModal(): void {
    if (!this.selectedBranchId) {
      this.errorMessage = 'Please select a branch first';
      return;
    }

    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create departments';
      return;
    }

    super.openCreateModal();
    this.modalTitle = 'Create New Department';
    // Set branchId after calling super which resets the form
    this.branchId = this.selectedBranchId;
  }

  override closeModal(): void {
    super.closeModal();
  }

  saveDepartment(): void {
    this.save();
  }

  openDeleteModal(department: Department): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete departments';
      return;
    }
    super.confirmDelete(department);
  }

  closeDeleteModal(): void {
    this.cancelDelete();
  }

  override confirmDelete(): void {
    this.executeDelete();
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.statusFilter = filter;
    this.filterItems();
  }

  loadDepartments(): void {
    this.loadItems();
  }

  // Department-specific utility methods
  toggleDepartmentStatus(department: Department): void {
    const updateRequest: UpdateDepartmentRequest = {
      name: department.name,
      code: department.code,
      description: department.description,
      managerId: department.managerId,
      secondaryManagerId: department.secondaryManagerId,
      hrResponsibleId: department.hrResponsibleId,
      isActive: !department.isActive
    };

    this.departmentService.updateDepartment(department.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          department.isActive = !department.isActive;
          this.successMessage = `Department ${department.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Department status toggled', { departmentId: department.id, isActive: department.isActive }, 'DepartmentManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update department status';
        this.logger.error('Error toggling department status', error, 'DepartmentManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  getSelectedBranchName(): string {
    const branch = this.branches.find(b => b.id === this.selectedBranchId);
    return branch ? branch.name : '';
  }

  getActiveDepartmentsCount(): number {
    return this.departments.filter(d => d.isActive).length;
  }

  getInactiveDepartmentsCount(): number {
    return this.departments.filter(d => !d.isActive).length;
  }

  // User selection handlers
  onManagerSelected(user: UserSearchResult | null): void {
    this.managerId = user?.id;
    this.managerName = user?.fullName;
  }

  onSecondaryManagerSelected(user: UserSearchResult | null): void {
    this.secondaryManagerId = user?.id;
    this.secondaryManagerName = user?.fullName;
  }

  onHrResponsibleSelected(user: UserSearchResult | null): void {
    this.hrResponsibleId = user?.id;
    this.hrResponsibleName = user?.fullName;
  }

  navigateBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // TrackBy functions for *ngFor optimization
  trackByBranchId(index: number, branch: Branch): string {
    return branch.id;
  }

  trackByDepartmentId(index: number, department: Department): string {
    return department.id;
  }
}
