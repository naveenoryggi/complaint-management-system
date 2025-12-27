import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  PriorityMasterService,
  ComplaintPriorityMaster,
  CreatePriorityMasterRequest,
  UpdatePriorityMasterRequest
} from '../../../services/priority-master.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-priority-master-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './priority-master-management.component.html',
  styleUrls: ['./priority-master-management.component.scss']
})
export class PriorityMasterManagementComponent
  extends BaseMasterManagementComponent<ComplaintPriorityMaster, CreatePriorityMasterRequest, UpdatePriorityMasterRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreatePriorityMasterRequest & { id?: string }) | (UpdatePriorityMasterRequest & { code?: string });

  protected override get entityName(): string {
    return 'Priority';
  }

  // Getter/setter for code property to handle union type in templates (for backward compatibility)
  get priorityCode(): string {
    return this.formCode;
  }

  set priorityCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for priority-specific fields
  get level(): number {
    return (this.form as any).level || 1;
  }

  set level(value: number) {
    (this.form as any).level = value;
  }

  get colorCode(): string {
    return (this.form as any).colorCode || '#ef4444';
  }

  set colorCode(value: string) {
    (this.form as any).colorCode = value;
  }

  get iconClass(): string {
    return (this.form as any).iconClass || 'fa-flag';
  }

  set iconClass(value: string) {
    (this.form as any).iconClass = value;
  }

  get slaResponseHours(): number {
    return (this.form as any).slaResponseHours || 4;
  }

  set slaResponseHours(value: number) {
    (this.form as any).slaResponseHours = value;
  }

  get slaResolutionHours(): number {
    return (this.form as any).slaResolutionHours || 24;
  }

  set slaResolutionHours(value: number) {
    (this.form as any).slaResolutionHours = value;
  }

  constructor(
    private priorityService: PriorityMasterService,
    authService: AuthService,
    logger: LoggerService
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }

  protected override getEmptyForm(): CreatePriorityMasterRequest {
    const currentUser = this.authService.currentUserValue;
    return {
      name: '',
      code: '',
      description: '',
      displayOrder: 0,
      level: 1,
      colorCode: '#ef4444',
      iconClass: 'fa-flag',
      slaResponseHours: 4,
      slaResolutionHours: 24,
      isActive: true,
      companyId: currentUser?.companyId
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    const companyId = currentUser?.companyId;

    this.priorityService.getPriorities(companyId, undefined, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.items = response.data;
          this.filterItems();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load priorities. Please try again.';
        this.loading = false;
        this.logger.error('Error loading priorities', error, 'PriorityMasterManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.showActiveOnly) {
      filtered = filtered.filter(p => p.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(search) ||
        p.code.toLowerCase().includes(search) ||
        (p.description && p.description.toLowerCase().includes(search))
      );
    }

    // Sort by display order first, then by level
    this.filteredItems = filtered.sort((a, b) => {
      if (a.displayOrder !== b.displayOrder) {
        return a.displayOrder - b.displayOrder;
      }
      return b.level - a.level; // Higher level first if display order is same
    });
  }

  protected override createItem(request: CreatePriorityMasterRequest): Observable<ApiResponse<ComplaintPriorityMaster>> {
    return this.priorityService.createPriority(request);
  }

  protected override updateItem(id: string, request: UpdatePriorityMasterRequest): Observable<ApiResponse<ComplaintPriorityMaster>> {
    return this.priorityService.updatePriority(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.priorityService.deletePriority(id);
  }

  override openEditModal(item: ComplaintPriorityMaster): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit priorities';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Priority';

    // Create edit form with all priority-specific fields
    this.form = {
      id: item.id,
      name: item.name,
      description: item.description || '',
      displayOrder: item.displayOrder,
      level: item.level,
      colorCode: item.colorCode || '#ef4444',
      iconClass: item.iconClass || 'fa-flag',
      slaResponseHours: item.slaResponseHours,
      slaResolutionHours: item.slaResolutionHours,
      isActive: item.isActive,
      code: item.code
    } as (CreatePriorityMasterRequest & { id?: string }) | (UpdatePriorityMasterRequest & { code?: string });

    this.showModal = true;
    this.errorMessage = '';
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate priority level
    const level = (this.form as any).level;
    if (level < 1 || level > 5) {
      this.errorMessage = 'Priority level must be between 1 and 5';
      return false;
    }

    // Validate SLA hours
    const slaResponse = (this.form as any).slaResponseHours;
    const slaResolution = (this.form as any).slaResolutionHours;

    if (slaResponse < 1 || slaResponse > 9999) {
      this.errorMessage = 'SLA Response hours must be between 1 and 9999';
      return false;
    }

    if (slaResolution < 1 || slaResolution > 9999) {
      this.errorMessage = 'SLA Resolution hours must be between 1 and 9999';
      return false;
    }

    if (slaResponse > slaResolution) {
      this.errorMessage = 'SLA Response hours cannot be greater than SLA Resolution hours';
      return false;
    }

    // Validate color code format if provided
    const colorCode = (this.form as any).colorCode;
    if (colorCode) {
      const colorRegex = /^#[0-9A-Fa-f]{6}$/;
      if (!colorRegex.test(colorCode)) {
        this.errorMessage = 'Color code must be in hex format (e.g., #ef4444)';
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
      this.createItem(this.form as CreatePriorityMasterRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Priority created successfully';
            this.logger.info('Priority created', undefined, 'PriorityMasterManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create priority';
            this.logger.error('Failed to create priority', { message: response.message }, 'PriorityMasterManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create priority. Please try again.';
          this.logger.error('Error creating priority', error, 'PriorityMasterManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including priority-specific ones
      const updateRequest = {
        id: this.formId,
        name: this.formName,
        description: this.formDescription,
        displayOrder: this.formDisplayOrder,
        level: this.level,
        colorCode: this.colorCode,
        iconClass: this.iconClass,
        slaResponseHours: this.slaResponseHours,
        slaResolutionHours: this.slaResolutionHours,
        isActive: this.formIsActive
      } as UpdatePriorityMasterRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Priority updated successfully';
            this.logger.info('Priority updated', undefined, 'PriorityMasterManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update priority';
            this.logger.error('Failed to update priority', { message: response.message }, 'PriorityMasterManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update priority. Please try again.';
          this.logger.error('Error updating priority', error, 'PriorityMasterManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get priorities(): ComplaintPriorityMaster[] {
    return this.items;
  }

  get filteredPriorities(): ComplaintPriorityMaster[] {
    return this.filteredItems;
  }

  get priorityForm(): (CreatePriorityMasterRequest & { id?: string }) | (UpdatePriorityMasterRequest & { code?: string }) {
    return this.form;
  }

  set priorityForm(value: (CreatePriorityMasterRequest & { id?: string }) | (UpdatePriorityMasterRequest & { code?: string })) {
    this.form = value;
  }

  get priorityToDelete(): ComplaintPriorityMaster | null {
    return this.itemToDelete;
  }

  set priorityToDelete(value: ComplaintPriorityMaster | null) {
    this.itemToDelete = value;
  }

  isEditingSystemPriority(): boolean {
    return this.isEditingSystemEntity();
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create priorities';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Priority';
  }

  override closeModal(): void {
    super.closeModal();
  }

  savePriority(): void {
    this.save();
  }

  openDeleteConfirm(priority: ComplaintPriorityMaster): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete priorities';
      return;
    }
    super.confirmDelete(priority);
  }

  closeDeleteConfirm(): void {
    this.cancelDelete();
  }

  override confirmDelete(): void {
    this.executeDelete();
  }

  onFilterChange(): void {
    this.onActiveFilterChange();
  }

  // TrackBy function for *ngFor optimization
  override trackById(index: number, item: ComplaintPriorityMaster): string {
    return item.id;
  }

  // Get Material Icon based on priority level/name
  getPriorityIcon(priority: ComplaintPriorityMaster): string {
    const level = priority.level;
    const code = priority.code?.toLowerCase() || '';
    const name = priority.name?.toLowerCase() || '';

    // Map priority levels/codes to Material Icons
    if (level === 5 || code.includes('critical') || code.includes('urgent')) {
      return 'priority_high';
    } else if (level === 4 || code.includes('high')) {
      return 'arrow_upward';
    } else if (level === 3 || code.includes('medium') || code.includes('normal')) {
      return 'remove';
    } else if (level === 2 || code.includes('low')) {
      return 'arrow_downward';
    } else if (level === 1 || code.includes('lowest') || code.includes('minor')) {
      return 'keyboard_double_arrow_down';
    }

    // Check by name as fallback
    if (name.includes('urgent') || name.includes('critical')) {
      return 'priority_high';
    } else if (name.includes('high')) {
      return 'arrow_upward';
    } else if (name.includes('medium') || name.includes('normal')) {
      return 'remove';
    } else if (name.includes('low')) {
      return 'arrow_downward';
    }

    // Default icon
    return 'flag';
  }
}
