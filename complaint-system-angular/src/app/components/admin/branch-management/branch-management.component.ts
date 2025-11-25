import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BranchService } from '../../../services/branch.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { Branch, CreateBranchRequest, UpdateBranchRequest } from '../../../models/branch.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-branch-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UserAutocompleteComponent, UtcToLocalPipe],
  templateUrl: './branch-management.component.html',
  styleUrls: ['./branch-management.component.scss']
})
export class BranchManagementComponent
  extends BaseMasterManagementComponent<Branch, CreateBranchRequest, UpdateBranchRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateBranchRequest & { id?: string }) | (UpdateBranchRequest & { code?: string });

  // User display names for autocomplete
  managerName?: string;
  secondaryManagerName?: string;
  hrResponsibleName?: string;

  protected override get entityName(): string {
    return 'Branch';
  }

  // Getter/setter for code property (for backward compatibility)
  get branchCode(): string {
    return this.formCode;
  }

  set branchCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for branch-specific fields
  get companyId(): string {
    return (this.form as any).companyId || '';
  }

  set companyId(value: string) {
    (this.form as any).companyId = value;
  }

  get contactEmail(): string {
    return (this.form as any).contactEmail || '';
  }

  set contactEmail(value: string) {
    (this.form as any).contactEmail = value;
  }

  get contactPhone(): string {
    return (this.form as any).contactPhone || '';
  }

  set contactPhone(value: string) {
    (this.form as any).contactPhone = value;
  }

  get address(): string {
    return (this.form as any).address || '';
  }

  set address(value: string) {
    (this.form as any).address = value;
  }

  get city(): string {
    return (this.form as any).city || '';
  }

  set city(value: string) {
    (this.form as any).city = value;
  }

  get country(): string {
    return (this.form as any).country || '';
  }

  set country(value: string) {
    (this.form as any).country = value;
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

  // Status filter for branches
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  constructor(
    private branchService: BranchService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }

  protected override getEmptyForm(): CreateBranchRequest {
    const currentUser = this.authService.currentUserValue;
    return {
      companyId: currentUser?.companyId || '',
      name: '',
      code: '',
      description: '',
      contactEmail: '',
      contactPhone: '',
      address: '',
      city: '',
      country: '',
      managerId: undefined,
      secondaryManagerId: undefined,
      hrResponsibleId: undefined,
      isActive: true
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated';
      this.loading = false;
      this.logger.error('User not authenticated', undefined, 'BranchManagementComponent');
      return;
    }

    this.branchService.getBranches(currentUser.companyId, false).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load branches. Please try again.';
        this.loading = false;
        this.logger.error('Error loading branches', error, 'BranchManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(b => b.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(b => !b.isActive);
    }
    // 'all' shows both active and inactive

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(b =>
        b.name.toLowerCase().includes(search) ||
        b.code.toLowerCase().includes(search) ||
        (b.city && b.city.toLowerCase().includes(search)) ||
        (b.country && b.country.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.branchService.createBranch(request);
  }

  protected override updateItem(id: string, request: UpdateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.branchService.updateBranch(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.branchService.deleteBranch(id);
  }

  override openEditModal(item: Branch): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit branches';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Branch';

    // Create edit form with all branch-specific fields
    this.form = {
      id: item.id,
      companyId: item.companyId,
      name: item.name,
      code: item.code,
      description: item.description || '',
      contactEmail: item.contactEmail || '',
      contactPhone: item.contactPhone || '',
      address: item.address || '',
      city: item.city || '',
      country: item.country || '',
      managerId: item.managerId,
      secondaryManagerId: item.secondaryManagerId,
      hrResponsibleId: item.hrResponsibleId,
      isActive: item.isActive
    } as (CreateBranchRequest & { id?: string }) | (UpdateBranchRequest & { code?: string });

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

    // Validate branch code length
    if (this.formCode && this.formCode.length > 20) {
      this.errorMessage = 'Branch code must be 20 characters or less';
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
      this.createItem(this.form as CreateBranchRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Branch created successfully';
            this.logger.info('Branch created', undefined, 'BranchManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create branch';
            this.logger.error('Failed to create branch', { message: response.message }, 'BranchManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create branch. Please try again.';
          this.logger.error('Error creating branch', error, 'BranchManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including branch-specific ones
      const updateRequest = {
        name: this.formName,
        code: this.formCode,
        description: this.formDescription,
        contactEmail: this.contactEmail,
        contactPhone: this.contactPhone,
        address: this.address,
        city: this.city,
        country: this.country,
        managerId: this.managerId,
        secondaryManagerId: this.secondaryManagerId,
        hrResponsibleId: this.hrResponsibleId,
        isActive: this.formIsActive
      } as UpdateBranchRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Branch updated successfully';
            this.logger.info('Branch updated', undefined, 'BranchManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update branch';
            this.logger.error('Failed to update branch', { message: response.message }, 'BranchManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update branch. Please try again.';
          this.logger.error('Error updating branch', error, 'BranchManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get branches(): Branch[] {
    return this.items;
  }

  get filteredBranches(): Branch[] {
    return this.filteredItems;
  }

  get branchForm(): (CreateBranchRequest & { id?: string }) | (UpdateBranchRequest & { code?: string }) {
    return this.form;
  }

  set branchForm(value: (CreateBranchRequest & { id?: string }) | (UpdateBranchRequest & { code?: string })) {
    this.form = value;
  }

  get branchToDelete(): Branch | null {
    return this.itemToDelete;
  }

  set branchToDelete(value: Branch | null) {
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
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create branches';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Branch';
  }

  override closeModal(): void {
    super.closeModal();
  }

  saveBranch(): void {
    this.save();
  }

  openDeleteModal(branch: Branch): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete branches';
      return;
    }
    super.confirmDelete(branch);
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

  loadBranches(): void {
    this.loadItems();
  }

  // Branch-specific utility methods
  toggleBranchStatus(branch: Branch): void {
    const updateRequest: UpdateBranchRequest = {
      name: branch.name,
      code: branch.code,
      description: branch.description,
      contactEmail: branch.contactEmail,
      contactPhone: branch.contactPhone,
      address: branch.address,
      city: branch.city,
      country: branch.country,
      managerId: branch.managerId,
      secondaryManagerId: branch.secondaryManagerId,
      hrResponsibleId: branch.hrResponsibleId,
      isActive: !branch.isActive
    };

    this.branchService.updateBranch(branch.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          branch.isActive = !branch.isActive;
          this.successMessage = `Branch ${branch.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Branch status toggled', { branchId: branch.id, isActive: branch.isActive }, 'BranchManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update branch status';
        this.logger.error('Error toggling branch status', error, 'BranchManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  getActiveBranchesCount(): number {
    return this.branches.filter(b => b.isActive).length;
  }

  getInactiveBranchesCount(): number {
    return this.branches.filter(b => !b.isActive).length;
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

  // TrackBy function for *ngFor optimization
  trackByBranchId(index: number, branch: Branch): string {
    return branch.id;
  }
}
