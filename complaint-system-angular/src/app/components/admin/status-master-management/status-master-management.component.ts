import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  StatusMasterService,
  ComplaintStatusMaster,
  CreateStatusMasterRequest,
  UpdateStatusMasterRequest
} from '../../../services/status-master.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-status-master-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './status-master-management.component.html',
  styleUrls: ['./status-master-management.component.scss']
})
export class StatusMasterManagementComponent
  extends BaseMasterManagementComponent<ComplaintStatusMaster, CreateStatusMasterRequest, UpdateStatusMasterRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateStatusMasterRequest & { id?: string }) | (UpdateStatusMasterRequest & { code?: string });

  protected override get entityName(): string {
    return 'Status';
  }

  // Getter/setter for code property to handle union type in templates (for backward compatibility)
  get statusCode(): string {
    return this.formCode;
  }

  set statusCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for status-specific fields
  get colorCode(): string {
    return (this.form as any).colorCode || '#6c757d';
  }

  set colorCode(value: string) {
    (this.form as any).colorCode = value;
  }

  get iconClass(): string {
    return (this.form as any).iconClass || 'fa-circle-notch';
  }

  set iconClass(value: string) {
    (this.form as any).iconClass = value;
  }

  get isFinal(): boolean {
    return (this.form as any).isFinal || false;
  }

  set isFinal(value: boolean) {
    (this.form as any).isFinal = value;
  }

  constructor(
    private statusService: StatusMasterService,
    authService: AuthService,
    logger: LoggerService
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }

  protected override getEmptyForm(): CreateStatusMasterRequest {
    const currentUser = this.authService.currentUserValue;
    return {
      name: '',
      code: '',
      description: '',
      displayOrder: 0,
      colorCode: '#6c757d',
      iconClass: 'fa-circle-notch',
      isActive: true,
      isFinal: false,
      companyId: currentUser?.companyId
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    const companyId = currentUser?.companyId;

    this.statusService.getStatuses(companyId, undefined, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.items = response.data;
          this.filterItems();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load statuses. Please try again.';
        this.loading = false;
        this.logger.error('Error loading statuses', error, 'StatusMasterManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.showActiveOnly) {
      filtered = filtered.filter(s => s.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(s =>
        s.name.toLowerCase().includes(search) ||
        s.code.toLowerCase().includes(search) ||
        (s.description && s.description.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered.sort((a, b) => a.displayOrder - b.displayOrder);
  }

  protected override createItem(request: CreateStatusMasterRequest): Observable<ApiResponse<ComplaintStatusMaster>> {
    return this.statusService.createStatus(request);
  }

  protected override updateItem(id: string, request: UpdateStatusMasterRequest): Observable<ApiResponse<ComplaintStatusMaster>> {
    return this.statusService.updateStatus(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.statusService.deleteStatus(id);
  }

  override openEditModal(item: ComplaintStatusMaster): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit statuses';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Status';

    // Create edit form with all status-specific fields
    this.form = {
      id: item.id,
      name: item.name,
      description: item.description || '',
      displayOrder: item.displayOrder,
      colorCode: item.colorCode || '#6c757d',
      iconClass: item.iconClass || 'fa-circle-notch',
      isActive: item.isActive,
      isFinal: item.isFinal,
      code: item.code
    } as (CreateStatusMasterRequest & { id?: string }) | (UpdateStatusMasterRequest & { code?: string });

    this.showModal = true;
    this.errorMessage = '';
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate color code format if provided
    const colorCode = (this.form as any).colorCode;
    if (colorCode) {
      const colorRegex = /^#[0-9A-Fa-f]{6}$/;
      if (!colorRegex.test(colorCode)) {
        this.errorMessage = 'Color code must be in hex format (e.g., #007bff)';
        return false;
      }
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
      this.createItem(this.form as CreateStatusMasterRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Status created successfully';
            this.logger.info('Status created', undefined, 'StatusMasterManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create status';
            this.logger.error('Failed to create status', { message: response.message }, 'StatusMasterManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create status. Please try again.';
          this.logger.error('Error creating status', error, 'StatusMasterManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including status-specific ones
      const updateRequest = {
        id: this.formId,
        name: this.formName,
        description: this.formDescription,
        displayOrder: this.formDisplayOrder,
        colorCode: this.colorCode,
        iconClass: this.iconClass,
        isActive: this.formIsActive,
        isFinal: this.isFinal
      } as UpdateStatusMasterRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Status updated successfully';
            this.logger.info('Status updated', undefined, 'StatusMasterManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update status';
            this.logger.error('Failed to update status', { message: response.message }, 'StatusMasterManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update status. Please try again.';
          this.logger.error('Error updating status', error, 'StatusMasterManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get statuses(): ComplaintStatusMaster[] {
    return this.items;
  }

  get filteredStatuses(): ComplaintStatusMaster[] {
    return this.filteredItems;
  }

  get statusForm(): (CreateStatusMasterRequest & { id?: string }) | (UpdateStatusMasterRequest & { code?: string }) {
    return this.form;
  }

  set statusForm(value: (CreateStatusMasterRequest & { id?: string }) | (UpdateStatusMasterRequest & { code?: string })) {
    this.form = value;
  }

  get statusToDelete(): ComplaintStatusMaster | null {
    return this.itemToDelete;
  }

  set statusToDelete(value: ComplaintStatusMaster | null) {
    this.itemToDelete = value;
  }

  isEditingSystemStatus(): boolean {
    return this.isEditingSystemEntity();
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create statuses';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Status';
  }

  override closeModal(): void {
    super.closeModal();
  }

  saveStatus(): void {
    this.save();
  }

  openDeleteConfirm(status: ComplaintStatusMaster): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete statuses';
      return;
    }
    super.confirmDelete(status);
  }

  closeDeleteConfirm(): void {
    this.cancelDelete();
  }

  executeDeleteConfirm(): void {
    this.executeDelete();
  }

  onFilterChange(): void {
    this.onActiveFilterChange();
  }
}
