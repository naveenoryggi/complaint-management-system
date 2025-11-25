import { Directive, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data?: T;
}

export interface BaseMasterEntity {
  id: string;
  name: string;
  code: string;
  description?: string;
  displayOrder?: number; // Optional - not all entities need ordering
  isActive: boolean;
  isSystem?: boolean; // Optional - only for Status and Priority masters
  companyId?: string;
}

export interface BaseCreateRequest {
  name: string;
  code: string;
  description?: string;
  displayOrder?: number; // Optional - not all entities need ordering
  isActive: boolean;
  companyId?: string;
}

export interface BaseUpdateRequest {
  name: string;
  description?: string;
  displayOrder?: number; // Optional - not all entities need ordering
  isActive: boolean;
}

@Directive()
export abstract class BaseMasterManagementComponent<
  TEntity extends BaseMasterEntity,
  TCreateRequest extends BaseCreateRequest,
  TUpdateRequest extends BaseUpdateRequest
> implements OnInit {

  items: TEntity[] = [];
  filteredItems: TEntity[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';
  showActiveOnly = true;
  canManageSettings = false;
  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  modalTitle = '';

  // Form model - union type to handle both Create and Update
  abstract form: (TCreateRequest & { id?: string }) | (TUpdateRequest & { code?: string });

  showDeleteConfirm = false;
  itemToDelete: TEntity | null = null;
  deleteLoading = false;

  // Getter/setter for id property to handle union type
  get formId(): string {
    return 'id' in this.form ? this.form.id || '' : '';
  }

  set formId(value: string) {
    if ('id' in this.form) {
      this.form.id = value;
    }
  }

  // Getter/setter for code property to handle union type
  get formCode(): string {
    return 'code' in this.form ? this.form.code || '' : '';
  }

  set formCode(value: string) {
    if ('code' in this.form) {
      this.form.code = value;
    }
  }

  // Getter/setter for name property
  get formName(): string {
    return (this.form as any).name || '';
  }

  set formName(value: string) {
    (this.form as any).name = value;
  }

  // Getter/setter for description property
  get formDescription(): string | undefined {
    return (this.form as any).description;
  }

  set formDescription(value: string | undefined) {
    (this.form as any).description = value;
  }

  // Getter/setter for displayOrder property
  get formDisplayOrder(): number {
    return (this.form as any).displayOrder || 0;
  }

  set formDisplayOrder(value: number) {
    (this.form as any).displayOrder = value;
  }

  // Getter/setter for isActive property
  get formIsActive(): boolean {
    return (this.form as any).isActive;
  }

  set formIsActive(value: boolean) {
    (this.form as any).isActive = value;
  }

  protected abstract get entityName(): string;
  protected abstract getEmptyForm(): TCreateRequest;
  protected abstract loadItems(): void;
  protected abstract filterItems(): void;
  protected abstract createItem(request: TCreateRequest): Observable<ApiResponse<TEntity>>;
  protected abstract updateItem(id: string, request: TUpdateRequest): Observable<ApiResponse<TEntity>>;
  protected abstract deleteItem(id: string): Observable<ApiResponse<boolean>>;

  constructor(
    protected authService: AuthService,
    protected logger: LoggerService
  ) {}

  ngOnInit(): void {
    this.canManageSettings = this.authService.hasPermission('ManageSettings');
    this.loadItems();
    this.logger.info(`${this.entityName} Management initialized`, undefined, this.constructor.name);
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.modalTitle = `Create New ${this.entityName}`;
    this.form = this.getEmptyForm();
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(item: TEntity): void {
    this.modalMode = 'edit';
    this.modalTitle = `Edit ${this.entityName}`;

    // Create edit form with all fields from the entity
    this.form = {
      name: item.name,
      description: item.description,
      displayOrder: item.displayOrder,
      isActive: item.isActive,
      id: item.id,
      code: item.code
    } as (TCreateRequest & { id?: string }) | (TUpdateRequest & { code?: string });

    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.form = this.getEmptyForm();
    this.errorMessage = '';
  }

  isEditingSystemEntity(): boolean {
    if (this.modalMode !== 'edit' || !this.formId) return false;
    const item = this.items.find(i => i.id === this.formId);
    return item?.isSystem === true; // Only true if isSystem exists and is true
  }

  save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.createItem(this.form as TCreateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = `${this.entityName} created successfully`;
            this.logger.info(`${this.entityName} created`, undefined, this.constructor.name);
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || `Failed to create ${this.entityName.toLowerCase()}`;
            this.logger.error(`Failed to create ${this.entityName}`, { message: response.message }, this.constructor.name);
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || `Failed to create ${this.entityName.toLowerCase()}. Please try again.`;
          this.logger.error(`Error creating ${this.entityName}`, error, this.constructor.name);
          this.loading = false;
        }
      });
    } else {
      const updateRequest = {
        name: this.formName,
        description: this.formDescription,
        displayOrder: this.formDisplayOrder,
        isActive: this.formIsActive
      } as TUpdateRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = `${this.entityName} updated successfully`;
            this.logger.info(`${this.entityName} updated`, undefined, this.constructor.name);
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || `Failed to update ${this.entityName.toLowerCase()}`;
            this.logger.error(`Failed to update ${this.entityName}`, { message: response.message }, this.constructor.name);
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || `Failed to update ${this.entityName.toLowerCase()}. Please try again.`;
          this.logger.error(`Error updating ${this.entityName}`, error, this.constructor.name);
          this.loading = false;
        }
      });
    }
  }

  protected validateForm(): boolean {
    if (!this.formName) {
      this.errorMessage = `${this.entityName} name is required`;
      return false;
    }

    if (this.modalMode === 'create' && !this.formCode) {
      this.errorMessage = `${this.entityName} code is required`;
      return false;
    }

    // Validate displayOrder only if it exists
    if (this.formDisplayOrder !== undefined && this.formDisplayOrder !== null) {
      if (this.formDisplayOrder < 0 || this.formDisplayOrder > 9999) {
        this.errorMessage = 'Display order must be between 0 and 9999';
        return false;
      }
    }

    return true;
  }

  confirmDelete(item: TEntity): void {
    this.itemToDelete = item;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.itemToDelete = null;
    this.showDeleteConfirm = false;
  }

  executeDelete(): void {
    if (!this.itemToDelete) return;

    this.deleteLoading = true;
    this.errorMessage = '';

    this.deleteItem(this.itemToDelete.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = `${this.entityName} deleted successfully`;
          this.logger.info(`${this.entityName} deleted`, { id: this.itemToDelete?.id }, this.constructor.name);
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || `Failed to delete ${this.entityName.toLowerCase()}`;
          this.logger.error(`Failed to delete ${this.entityName}`, { message: response.message }, this.constructor.name);
        }
        this.deleteLoading = false;
        this.cancelDelete();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || `Failed to delete ${this.entityName.toLowerCase()}. Please try again.`;
        this.logger.error(`Error deleting ${this.entityName}`, error, this.constructor.name);
        this.deleteLoading = false;
        this.cancelDelete();
      }
    });
  }

  onSearchChange(): void {
    this.filterItems();
  }

  onActiveFilterChange(): void {
    this.filterItems();
  }

  trackById(index: number, item: TEntity): string {
    return item.id;
  }
}
