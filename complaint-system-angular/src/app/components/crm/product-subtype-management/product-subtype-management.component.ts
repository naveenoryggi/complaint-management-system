import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ProductSubTypeService } from '../../../services/product-subtype.service';
import { ProductService } from '../../../services/product.service';
import {
  ProductSubType,
  ProductSubTypeSummary,
  CreateProductSubTypeRequest,
  UpdateProductSubTypeRequest
} from '../../../models/product-subtype.model';
import { CategoryLookup } from '../../../models/product.model';

@Component({
  selector: 'app-product-subtype-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-subtype-management.component.html',
  styleUrls: ['./product-subtype-management.component.scss']
})
export class ProductSubTypeManagementComponent implements OnInit, OnDestroy {
  subTypes: ProductSubTypeSummary[] = [];
  categories: CategoryLookup[] = [];
  selectedSubType: ProductSubType | null = null;
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';

  Math = Math;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalSubTypes = 0;
  totalPages = 1;

  // Filters
  filterCategory: string | null = null;
  filterActive: boolean | null = null;

  // Modal state
  showModal = false;
  isEditMode = false;

  // Form data
  subTypeForm: CreateProductSubTypeRequest = this.getEmptyForm();

  constructor(
    private subTypeService: ProductSubTypeService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadSubTypes();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.currentPage = 1;
      this.loadSubTypes();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getEmptyForm(): CreateProductSubTypeRequest {
    return {
      categoryId: '',
      code: '',
      name: '',
      description: '',
      icon: '',
      color: '',
      displayOrder: 0,
      notes: ''
    };
  }

  loadCategories(): void {
    this.productService.getCategoryLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.categories = response.data;
        }
      }
    });
  }

  loadSubTypes(): void {
    this.loading = true;
    this.errorMessage = '';

    this.subTypeService.getSubTypes({
      search: this.searchTerm || undefined,
      categoryId: this.filterCategory || undefined,
      isActive: this.filterActive ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.subTypes = response.data.items;
          this.totalSubTypes = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load sub-types';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading sub-types';
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadSubTypes();
  }

  clearFilters(): void {
    this.filterCategory = null;
    this.filterActive = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadSubTypes();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadSubTypes();
    }
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // Modal Operations
  openAddModal(): void {
    this.subTypeForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(subType: ProductSubTypeSummary): void {
    this.detailLoading = true;
    this.subTypeService.getSubTypeById(subType.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const st = response.data;
          this.subTypeForm = {
            categoryId: st.categoryId,
            code: st.code,
            name: st.name,
            description: st.description || '',
            icon: st.icon || '',
            color: st.color || '',
            displayOrder: st.displayOrder,
            notes: st.notes || ''
          };
          this.selectedSubType = st;
          this.isEditMode = true;
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load sub-type details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.subTypeForm = this.getEmptyForm();
    this.selectedSubType = null;
    this.errorMessage = '';
  }

  saveSubType(): void {
    // Validate required fields
    if (!this.subTypeForm.categoryId) {
      this.errorMessage = 'Category is required';
      return;
    }
    if (!this.subTypeForm.code?.trim()) {
      this.errorMessage = 'Sub-type code is required';
      return;
    }
    if (!this.subTypeForm.name?.trim()) {
      this.errorMessage = 'Sub-type name is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedSubType) {
      const updateRequest: UpdateProductSubTypeRequest = {
        categoryId: this.subTypeForm.categoryId,
        name: this.subTypeForm.name,
        description: this.subTypeForm.description,
        icon: this.subTypeForm.icon,
        color: this.subTypeForm.color,
        isActive: this.selectedSubType.isActive,
        displayOrder: this.subTypeForm.displayOrder,
        notes: this.subTypeForm.notes
      };

      this.subTypeService.updateSubType(this.selectedSubType.id, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Sub-type updated successfully';
            this.closeModal();
            this.loadSubTypes();
          } else {
            this.errorMessage = response.message || 'Failed to update sub-type';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.subTypeService.createSubType(this.subTypeForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Sub-type created successfully';
            this.closeModal();
            this.loadSubTypes();
          } else {
            this.errorMessage = response.message || 'Failed to create sub-type';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  deleteSubType(subType: ProductSubTypeSummary): void {
    if (confirm(`Are you sure you want to delete sub-type "${subType.name}"?`)) {
      this.loading = true;
      this.subTypeService.deleteSubType(subType.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Sub-type deleted successfully';
            this.loadSubTypes();
          } else {
            this.errorMessage = response.message || 'Failed to delete sub-type';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  toggleActive(subType: ProductSubTypeSummary): void {
    this.subTypeService.getSubTypeById(subType.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const st = response.data;
          const updateRequest: UpdateProductSubTypeRequest = {
            categoryId: st.categoryId,
            name: st.name,
            description: st.description,
            icon: st.icon,
            color: st.color,
            isActive: !st.isActive,
            displayOrder: st.displayOrder,
            notes: st.notes
          };

          this.subTypeService.updateSubType(subType.id, updateRequest).subscribe({
            next: (updateResponse) => {
              if (updateResponse.isSuccess) {
                this.successMessage = `Sub-type ${updateResponse.data.isActive ? 'activated' : 'deactivated'} successfully`;
                this.loadSubTypes();
              }
            }
          });
        }
      }
    });
  }

  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown';
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }
}
