import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StockCategoryService, StockCategory, CreateStockCategoryRequest } from '../../../services/stock-category.service';

@Component({
  selector: 'app-stock-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './stock-category-management.component.html',
  styleUrls: ['./stock-category-management.component.scss']
})
export class StockCategoryManagementComponent implements OnInit {
  categories = signal<StockCategory[]>([]);
  selectedCategory = signal<StockCategory | null>(null);

  isLoading = signal(false);
  showModal = signal(false);
  isEditMode = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  searchTerm = signal('');
  currentPage = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);

  categoryForm: FormGroup;

  // Predefined colors for selection
  colorOptions = [
    { value: '#22c55e', label: 'Green' },
    { value: '#3b82f6', label: 'Blue' },
    { value: '#f59e0b', label: 'Amber' },
    { value: '#ef4444', label: 'Red' },
    { value: '#8b5cf6', label: 'Purple' },
    { value: '#06b6d4', label: 'Cyan' },
    { value: '#ec4899', label: 'Pink' },
    { value: '#84cc16', label: 'Lime' },
    { value: '#f97316', label: 'Orange' },
    { value: '#6366f1', label: 'Indigo' }
  ];

  constructor(
    private stockCategoryService: StockCategoryService,
    private fb: FormBuilder
  ) {
    this.categoryForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      colorCode: ['#3b82f6'],
      icon: [''],
      isForSale: [false],
      isFixedAsset: [false],
      isServiceStock: [false],
      isFaulty: [false],
      isDemo: [false],
      requiresSpecialHandling: [false],
      allowsCustomerAssignment: [true],
      allowsEmployeeAssignment: [true],
      sortOrder: [0],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.stockCategoryService.getAll(
      this.currentPage(),
      this.pageSize(),
      this.searchTerm() || undefined
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        } else {
          this.error.set(response.message || 'Failed to load stock categories');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load stock categories');
        this.isLoading.set(false);
        console.error('Error loading categories:', err);
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadCategories();
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.selectedCategory.set(null);
    this.categoryForm.reset({
      colorCode: '#3b82f6',
      allowsCustomerAssignment: true,
      allowsEmployeeAssignment: true,
      sortOrder: 0,
      isActive: true
    });
    this.showModal.set(true);
  }

  openEditModal(category: StockCategory): void {
    this.isEditMode.set(true);
    this.selectedCategory.set(category);
    this.categoryForm.patchValue({
      code: category.code,
      name: category.name,
      description: category.description || '',
      colorCode: category.colorCode || '#3b82f6',
      icon: category.icon || '',
      isForSale: category.isForSale,
      isFixedAsset: category.isFixedAsset,
      isServiceStock: category.isServiceStock,
      isFaulty: category.isFaulty,
      isDemo: category.isDemo,
      requiresSpecialHandling: category.requiresSpecialHandling,
      allowsCustomerAssignment: category.allowsCustomerAssignment,
      allowsEmployeeAssignment: category.allowsEmployeeAssignment,
      sortOrder: category.sortOrder,
      isActive: category.isActive
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedCategory.set(null);
  }

  saveCategory(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.categoryForm.value;

    const request: CreateStockCategoryRequest = {
      code: formValue.code,
      name: formValue.name,
      description: formValue.description || undefined,
      colorCode: formValue.colorCode,
      icon: formValue.icon || undefined,
      isForSale: formValue.isForSale,
      isFixedAsset: formValue.isFixedAsset,
      isServiceStock: formValue.isServiceStock,
      isFaulty: formValue.isFaulty,
      isDemo: formValue.isDemo,
      requiresSpecialHandling: formValue.requiresSpecialHandling,
      allowsCustomerAssignment: formValue.allowsCustomerAssignment,
      allowsEmployeeAssignment: formValue.allowsEmployeeAssignment,
      sortOrder: formValue.sortOrder,
      isActive: formValue.isActive
    };

    const operation = this.isEditMode()
      ? this.stockCategoryService.update(this.selectedCategory()!.id, request)
      : this.stockCategoryService.create(request);

    operation.subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set(this.isEditMode() ? 'Category updated successfully' : 'Category created successfully');
          this.closeModal();
          this.loadCategories();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to save category');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to save category');
        this.isLoading.set(false);
      }
    });
  }

  deleteCategory(category: StockCategory): void {
    if (category.isSystem) {
      this.error.set('Cannot delete system stock category');
      setTimeout(() => this.error.set(null), 3000);
      return;
    }

    if (category.assetCount > 0) {
      this.error.set(`Cannot delete category "${category.name}" - it has ${category.assetCount} assets assigned`);
      setTimeout(() => this.error.set(null), 3000);
      return;
    }

    if (!confirm(`Are you sure you want to delete "${category.name}"?`)) {
      return;
    }

    this.isLoading.set(true);
    this.stockCategoryService.delete(category.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Category deleted successfully');
          this.loadCategories();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to delete category');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to delete category');
        this.isLoading.set(false);
      }
    });
  }

  seedDefaults(): void {
    if (!confirm('This will create default stock categories. Continue?')) {
      return;
    }

    this.isLoading.set(true);
    this.stockCategoryService.seedDefaults().subscribe({
      next: () => {
        this.successMessage.set('Default categories created successfully');
        this.loadCategories();
        setTimeout(() => this.successMessage.set(null), 3000);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to seed default categories');
        this.isLoading.set(false);
      }
    });
  }

  getCategoryFlags(category: StockCategory): string[] {
    const flags: string[] = [];
    if (category.isForSale) flags.push('For Sale');
    if (category.isFixedAsset) flags.push('Fixed Asset');
    if (category.isServiceStock) flags.push('Service Stock');
    if (category.isFaulty) flags.push('Faulty');
    if (category.isDemo) flags.push('Demo');
    if (category.requiresSpecialHandling) flags.push('Special Handling');
    return flags;
  }
}
