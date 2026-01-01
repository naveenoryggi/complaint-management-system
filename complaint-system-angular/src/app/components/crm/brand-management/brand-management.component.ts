import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { BrandService } from '../../../services/brand.service';
import {
  Brand,
  BrandSummary,
  CreateBrandRequest,
  UpdateBrandRequest
} from '../../../models/brand.model';

@Component({
  selector: 'app-brand-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './brand-management.component.html',
  styleUrls: ['./brand-management.component.scss']
})
export class BrandManagementComponent implements OnInit, OnDestroy {
  brands: BrandSummary[] = [];
  selectedBrand: Brand | null = null;
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
  totalBrands = 0;
  totalPages = 1;

  // Filters
  filterActive: boolean | null = null;

  // Modal state
  showModal = false;
  isEditMode = false;

  // Form data
  brandForm: CreateBrandRequest = this.getEmptyForm();

  constructor(private brandService: BrandService) {}

  ngOnInit(): void {
    this.loadBrands();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.currentPage = 1;
      this.loadBrands();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getEmptyForm(): CreateBrandRequest {
    return {
      code: '',
      name: '',
      description: '',
      logoUrl: '',
      websiteUrl: '',
      country: '',
      displayOrder: 0,
      notes: ''
    };
  }

  loadBrands(): void {
    this.loading = true;
    this.errorMessage = '';

    this.brandService.getBrands({
      search: this.searchTerm || undefined,
      isActive: this.filterActive ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.brands = response.data.items;
          this.totalBrands = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load brands';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading brands';
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadBrands();
  }

  clearFilters(): void {
    this.filterActive = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadBrands();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadBrands();
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
    this.brandForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(brand: BrandSummary): void {
    this.detailLoading = true;
    this.brandService.getBrandById(brand.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const b = response.data;
          this.brandForm = {
            code: b.code,
            name: b.name,
            description: b.description || '',
            logoUrl: b.logoUrl || '',
            websiteUrl: b.websiteUrl || '',
            country: b.country || '',
            displayOrder: b.displayOrder,
            notes: b.notes || ''
          };
          this.selectedBrand = b;
          this.isEditMode = true;
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load brand details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.brandForm = this.getEmptyForm();
    this.selectedBrand = null;
    this.errorMessage = '';
  }

  saveBrand(): void {
    // Validate required fields
    if (!this.brandForm.code?.trim()) {
      this.errorMessage = 'Brand code is required';
      return;
    }
    if (!this.brandForm.name?.trim()) {
      this.errorMessage = 'Brand name is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedBrand) {
      const updateRequest: UpdateBrandRequest = {
        name: this.brandForm.name,
        description: this.brandForm.description,
        logoUrl: this.brandForm.logoUrl,
        websiteUrl: this.brandForm.websiteUrl,
        country: this.brandForm.country,
        isActive: this.selectedBrand.isActive,
        displayOrder: this.brandForm.displayOrder,
        notes: this.brandForm.notes
      };

      this.brandService.updateBrand(this.selectedBrand.id, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Brand updated successfully';
            this.closeModal();
            this.loadBrands();
          } else {
            this.errorMessage = response.message || 'Failed to update brand';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.brandService.createBrand(this.brandForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Brand created successfully';
            this.closeModal();
            this.loadBrands();
          } else {
            this.errorMessage = response.message || 'Failed to create brand';
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

  deleteBrand(brand: BrandSummary): void {
    if (confirm(`Are you sure you want to delete brand "${brand.name}"?`)) {
      this.loading = true;
      this.brandService.deleteBrand(brand.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Brand deleted successfully';
            this.loadBrands();
          } else {
            this.errorMessage = response.message || 'Failed to delete brand';
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

  toggleActive(brand: BrandSummary): void {
    this.brandService.getBrandById(brand.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const b = response.data;
          const updateRequest: UpdateBrandRequest = {
            name: b.name,
            description: b.description,
            logoUrl: b.logoUrl,
            websiteUrl: b.websiteUrl,
            country: b.country,
            isActive: !b.isActive,
            displayOrder: b.displayOrder,
            notes: b.notes
          };

          this.brandService.updateBrand(brand.id, updateRequest).subscribe({
            next: (updateResponse) => {
              if (updateResponse.isSuccess) {
                this.successMessage = `Brand ${updateResponse.data.isActive ? 'activated' : 'deactivated'} successfully`;
                this.loadBrands();
              }
            }
          });
        }
      }
    });
  }
}
