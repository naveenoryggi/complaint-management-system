import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService, CreateCategoryRequest, UpdateCategoryRequest } from '../../../services/category.service';
import { Category } from '../../../models/category.model';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.component.html',
  styleUrls: ['./category-management.component.scss']
})
export class CategoryManagementComponent
  extends BaseMasterManagementComponent<Category, CreateCategoryRequest, UpdateCategoryRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateCategoryRequest & { id?: string }) | (UpdateCategoryRequest & { code?: string });

  protected override get entityName(): string {
    return 'Category';
  }

  // Getter/setter for code property (for backward compatibility)
  get categoryCode(): string {
    return this.formCode;
  }

  set categoryCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for category-specific fields
  get parentCategoryId(): string | undefined {
    return (this.form as any).parentCategoryId;
  }

  set parentCategoryId(value: string | undefined) {
    (this.form as any).parentCategoryId = value;
  }

  get defaultPriority(): number {
    return (this.form as any).defaultPriority || 2;
  }

  set defaultPriority(value: number) {
    (this.form as any).defaultPriority = value;
  }

  get defaultSlaHours(): number {
    return (this.form as any).defaultSlaHours || 48;
  }

  set defaultSlaHours(value: number) {
    (this.form as any).defaultSlaHours = value;
  }

  constructor(
    private categoryService: CategoryService,
    authService: AuthService,
    logger: LoggerService
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }

  protected override getEmptyForm(): CreateCategoryRequest {
    return {
      name: '',
      code: '',
      description: '',
      parentCategoryId: undefined,
      defaultPriority: 2,
      defaultSlaHours: 48,
      isActive: true,
      displayOrder: 0
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.categoryService.getCategories(false).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.items = response.data;
          this.filterItems();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load categories. Please try again.';
        this.loading = false;
        this.logger.error('Error loading categories', error, 'CategoryManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    // Note: showActiveOnly = true means checkbox is CHECKED, so show ONLY active
    if (this.showActiveOnly) {
      filtered = filtered.filter(c => c.isActive);
    }
    // When showActiveOnly = false (unchecked), show all categories including inactive

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(c =>
        c.name.toLowerCase().includes(search) ||
        c.code.toLowerCase().includes(search) ||
        (c.description && c.description.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered.sort((a, b) => a.displayOrder - b.displayOrder);
  }

  protected override createItem(request: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.categoryService.createCategory(request);
  }

  protected override updateItem(id: string, request: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.categoryService.updateCategory(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.categoryService.deleteCategory(id);
  }

  override openEditModal(item: Category): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit categories';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Category';

    // Create edit form with all category-specific fields
    this.form = {
      id: item.id,
      name: item.name,
      code: item.code,
      description: item.description || '',
      parentCategoryId: item.parentCategoryId,
      defaultPriority: item.defaultPriority,
      defaultSlaHours: item.defaultSlaHours,
      isActive: item.isActive,
      displayOrder: item.displayOrder
    } as (CreateCategoryRequest & { id?: string }) | (UpdateCategoryRequest & { code?: string });

    this.showModal = true;
    this.errorMessage = '';
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate default priority
    const priority = (this.form as any).defaultPriority;
    if (priority < 1 || priority > 4) {
      this.errorMessage = 'Default priority must be between 1 and 4';
      return false;
    }

    // Validate default SLA hours
    const slaHours = (this.form as any).defaultSlaHours;
    if (slaHours < 1 || slaHours > 720) {
      this.errorMessage = 'Default SLA hours must be between 1 and 720';
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
      this.createItem(this.form as CreateCategoryRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Category created successfully';
            this.logger.info('Category created', undefined, 'CategoryManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create category';
            this.logger.error('Failed to create category', { message: response.message }, 'CategoryManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create category. Please try again.';
          this.logger.error('Error creating category', error, 'CategoryManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including category-specific ones
      const updateRequest = {
        id: this.formId,
        name: this.formName,
        code: this.formCode,
        description: this.formDescription,
        parentCategoryId: this.parentCategoryId,
        defaultPriority: this.defaultPriority,
        defaultSlaHours: this.defaultSlaHours,
        isActive: this.formIsActive,
        displayOrder: this.formDisplayOrder
      } as UpdateCategoryRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Category updated successfully';
            this.logger.info('Category updated', undefined, 'CategoryManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update category';
            this.logger.error('Failed to update category', { message: response.message }, 'CategoryManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update category. Please try again.';
          this.logger.error('Error updating category', error, 'CategoryManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get categories(): Category[] {
    return this.items;
  }

  get filteredCategories(): Category[] {
    return this.filteredItems;
  }

  get categoryForm(): (CreateCategoryRequest & { id?: string }) | (UpdateCategoryRequest & { code?: string }) {
    return this.form;
  }

  set categoryForm(value: (CreateCategoryRequest & { id?: string }) | (UpdateCategoryRequest & { code?: string })) {
    this.form = value;
  }

  get categoryToDelete(): Category | null {
    return this.itemToDelete;
  }

  set categoryToDelete(value: Category | null) {
    this.itemToDelete = value;
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create categories';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Category';
  }

  override closeModal(): void {
    super.closeModal();
  }

  saveCategory(): void {
    this.save();
  }

  openDeleteConfirm(category: Category): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete categories';
      return;
    }
    super.confirmDelete(category);
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

  loadCategories(): void {
    this.loadItems();
  }

  filterCategories(): void {
    this.filterItems();
  }

  // Category-specific utility methods
  getPriorityBadgeClass(priority: number): string {
    switch (priority) {
      case 1: return 'priority-low';
      case 2: return 'priority-normal';
      case 3: return 'priority-high';
      case 4: return 'priority-critical';
      default: return 'priority-normal';
    }
  }

  getPriorityName(priority: number): string {
    switch (priority) {
      case 1: return 'Low';
      case 2: return 'Normal';
      case 3: return 'High';
      case 4: return 'Critical';
      default: return 'Normal';
    }
  }

  getParentCategories(): Category[] {
    // Return only top-level categories (no parent) for parent category selection
    return this.categories.filter(c => !c.parentCategoryId && c.isActive);
  }

  // TrackBy functions for *ngFor optimization
  trackByCategoryId(index: number, category: Category): string {
    return category.id;
  }
}
