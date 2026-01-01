import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContractService } from '../../../services/contract.service';
import { ProductService } from '../../../services/product.service';
import { InlineSelectComponent, SelectOption } from '../../shared/inline-select/inline-select.component';
import {
  Warranty,
  WarrantySummary,
  CreateWarrantyRequest,
  UpdateWarrantyRequest,
  WarrantyType,
  WarrantyTypeLabels,
  CoverageType,
  CoverageTypeLabels,
  SupportHoursType,
  SupportHoursTypeLabels
} from '../../../models/contract.model';

@Component({
  selector: 'app-warranty-management',
  standalone: true,
  imports: [CommonModule, FormsModule, InlineSelectComponent],
  templateUrl: './warranty-management.component.html',
  styleUrls: ['./warranty-management.component.scss']
})
export class WarrantyManagementComponent implements OnInit {
  warranties: WarrantySummary[] = [];
  selectedWarranty: Warranty | null = null;
  products: { id: string; name: string }[] = [];
  categories: { id: string; name: string }[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Filters
  searchTerm = '';
  filterType: WarrantyType | '' = '';
  filterCoverage: CoverageType | '' = '';
  filterActive: boolean | '' = '';

  // State
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';

  // Modal
  showModal = false;
  isEditMode = false;
  warrantyForm: CreateWarrantyRequest = this.getEmptyForm();
  activeTab = 'basic';

  // Enums for templates
  warrantyTypes = Object.entries(WarrantyTypeLabels).map(([key, value]) => ({ key: +key, value }));
  coverageTypes = Object.entries(CoverageTypeLabels).map(([key, value]) => ({ key: +key, value }));
  supportHoursTypes = Object.entries(SupportHoursTypeLabels).map(([key, value]) => ({ key: +key, value }));

  WarrantyTypeLabels = WarrantyTypeLabels;
  CoverageTypeLabels = CoverageTypeLabels;

  // SelectOption arrays for inline-select component
  warrantyTypeOptions: SelectOption[] = [];
  coverageTypeOptions: SelectOption[] = [];
  supportHoursOptions: SelectOption[] = [];
  productOptions: SelectOption[] = [];
  categoryOptions: SelectOption[] = [];

  // Custom options added by user (stored separately for persistence)
  customWarrantyTypes: SelectOption[] = [];
  customCoverageTypes: SelectOption[] = [];

  constructor(
    private contractService: ContractService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.initSelectOptions();
    this.loadWarranties();
    this.loadProducts();
    this.loadCategories();
  }

  initSelectOptions(): void {
    // Initialize warranty type options from enum
    this.warrantyTypeOptions = this.warrantyTypes.map(t => ({
      value: t.key,
      label: t.value
    }));

    // Initialize coverage type options from enum
    this.coverageTypeOptions = this.coverageTypes.map(c => ({
      value: c.key,
      label: c.value
    }));

    // Initialize support hours options from enum
    this.supportHoursOptions = [
      { value: undefined, label: '-- Select --' },
      ...this.supportHoursTypes.map(s => ({
        value: s.key,
        label: s.value
      }))
    ];

    // Load any custom types from localStorage
    this.loadCustomOptions();
  }

  loadCustomOptions(): void {
    const customTypes = localStorage.getItem('custom_warranty_types');
    if (customTypes) {
      this.customWarrantyTypes = JSON.parse(customTypes);
      this.warrantyTypeOptions = [
        ...this.warrantyTypes.map(t => ({ value: t.key, label: t.value })),
        ...this.customWarrantyTypes
      ];
    }

    const customCoverage = localStorage.getItem('custom_coverage_types');
    if (customCoverage) {
      this.customCoverageTypes = JSON.parse(customCoverage);
      this.coverageTypeOptions = [
        ...this.coverageTypes.map(c => ({ value: c.key, label: c.value })),
        ...this.customCoverageTypes
      ];
    }
  }

  saveCustomOptions(): void {
    localStorage.setItem('custom_warranty_types', JSON.stringify(this.customWarrantyTypes));
    localStorage.setItem('custom_coverage_types', JSON.stringify(this.customCoverageTypes));
  }

  loadWarranties(): void {
    this.loading = true;
    this.errorMessage = '';

    this.contractService.getWarranties({
      search: this.searchTerm || undefined,
      type: this.filterType !== '' ? this.filterType : undefined,
      coverageType: this.filterCoverage !== '' ? this.filterCoverage : undefined,
      isActive: this.filterActive !== '' ? this.filterActive : undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.warranties = response.data.items || [];
          this.totalItems = response.data.totalCount || 0;
          this.totalPages = response.data.totalPages || 0;
        } else {
          this.warranties = [];
          this.errorMessage = response.message || 'Failed to load warranties';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading warranties';
        this.warranties = [];
        this.loading = false;
      }
    });
  }

  loadProducts(): void {
    this.productService.getProductLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.products = response.data.map(p => ({ id: p.id, name: p.name }));
          this.productOptions = [
            { value: '', label: '-- All Products --' },
            ...this.products.map(p => ({ value: p.id, label: p.name }))
          ];
        }
      },
      error: () => {}
    });
  }

  loadCategories(): void {
    this.productService.getCategoryLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.categories = response.data.map(c => ({ id: c.id, name: c.name }));
          this.categoryOptions = [
            { value: '', label: '-- All Categories --' },
            ...this.categories.map(c => ({ value: c.id, label: c.name }))
          ];
        }
      },
      error: () => {}
    });
  }

  // Inline option handlers
  onWarrantyTypeAdded(label: string): void {
    const newOption: SelectOption = {
      value: 'custom_' + Date.now(),
      label: label,
      isCustom: true
    };
    this.customWarrantyTypes.push(newOption);
    this.warrantyTypeOptions = [
      ...this.warrantyTypes.map(t => ({ value: t.key, label: t.value })),
      ...this.customWarrantyTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Warranty type "${label}" added`;
    setTimeout(() => this.dismissSuccess(), 3000);
  }

  onWarrantyTypeDeleted(option: SelectOption): void {
    this.customWarrantyTypes = this.customWarrantyTypes.filter(t => t.value !== option.value);
    this.warrantyTypeOptions = [
      ...this.warrantyTypes.map(t => ({ value: t.key, label: t.value })),
      ...this.customWarrantyTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Warranty type "${option.label}" deleted`;
    setTimeout(() => this.dismissSuccess(), 3000);
  }

  onCoverageTypeAdded(label: string): void {
    const newOption: SelectOption = {
      value: 'custom_' + Date.now(),
      label: label,
      isCustom: true
    };
    this.customCoverageTypes.push(newOption);
    this.coverageTypeOptions = [
      ...this.coverageTypes.map(c => ({ value: c.key, label: c.value })),
      ...this.customCoverageTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Coverage type "${label}" added`;
    setTimeout(() => this.dismissSuccess(), 3000);
  }

  onCoverageTypeDeleted(option: SelectOption): void {
    this.customCoverageTypes = this.customCoverageTypes.filter(c => c.value !== option.value);
    this.coverageTypeOptions = [
      ...this.coverageTypes.map(c => ({ value: c.key, label: c.value })),
      ...this.customCoverageTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Coverage type "${option.label}" deleted`;
    setTimeout(() => this.dismissSuccess(), 3000);
  }

  onProductAdded(name: string): void {
    // Call product service to create product
    this.productService.createProduct({
      productCode: 'PRD-' + Date.now(),
      name: name,
      type: 0, // Physical
      unitPrice: 0
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadProducts();
          this.successMessage = `Product "${name}" created`;
          setTimeout(() => this.dismissSuccess(), 3000);
        } else {
          this.errorMessage = response.message || 'Failed to create product';
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create product';
      }
    });
  }

  onCategoryAdded(name: string): void {
    // Call product service to create category
    this.productService.createCategory({
      code: 'CAT-' + Date.now(),
      name: name,
      isActive: true
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadCategories();
          this.successMessage = `Category "${name}" created`;
          setTimeout(() => this.dismissSuccess(), 3000);
        } else {
          this.errorMessage = response.message || 'Failed to create category';
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create category';
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadWarranties();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadWarranties();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.filterType = '';
    this.filterCoverage = '';
    this.filterActive = '';
    this.currentPage = 1;
    this.loadWarranties();
  }

  selectWarranty(warranty: WarrantySummary): void {
    this.detailLoading = true;
    this.contractService.getWarrantyById(warranty.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedWarranty = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  closeDetail(): void {
    this.selectedWarranty = null;
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadWarranties();
    }
  }

  getVisiblePages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // Modal Operations
  openAddModal(): void {
    this.warrantyForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.activeTab = 'basic';
    this.errorMessage = '';
  }

  openEditModal(warranty: WarrantySummary): void {
    this.detailLoading = true;
    this.contractService.getWarrantyById(warranty.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const w = response.data;
          this.warrantyForm = {
            productId: w.productId,
            categoryId: w.categoryId,
            code: w.code,
            name: w.name,
            description: w.description,
            type: w.type,
            durationMonths: w.durationMonths,
            extendedDurationMonths: w.extendedDurationMonths,
            gracePeriodDays: w.gracePeriodDays,
            maxDurationMonths: w.maxDurationMonths,
            coverageType: w.coverageType,
            includesParts: w.includesParts,
            includesLabor: w.includesLabor,
            includesOnsite: w.includesOnsite,
            includesShipping: w.includesShipping,
            coveredItems: w.coveredItems,
            excludedItems: w.excludedItems,
            terms: w.terms,
            isTransferable: w.isTransferable,
            requiresRegistration: w.requiresRegistration,
            registrationDeadlineDays: w.registrationDeadlineDays,
            requiresProofOfPurchase: w.requiresProofOfPurchase,
            extendedWarrantyPrice: w.extendedWarrantyPrice,
            extendedWarrantyPricePercent: w.extendedWarrantyPricePercent,
            currency: w.currency,
            responseTimeHours: w.responseTimeHours,
            resolutionTimeHours: w.resolutionTimeHours,
            supportHoursType: w.supportHoursType,
            maxClaims: w.maxClaims,
            maxClaimValue: w.maxClaimValue,
            maxClaimValuePercent: w.maxClaimValuePercent,
            deductibleAmount: w.deductibleAmount,
            isActive: w.isActive,
            isDefault: w.isDefault,
            displayOrder: w.displayOrder
          };
          this.selectedWarranty = w;
          this.isEditMode = true;
          this.showModal = true;
          this.activeTab = 'basic';
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load warranty details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.warrantyForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  saveWarranty(): void {
    if (!this.warrantyForm.code?.trim()) {
      this.errorMessage = 'Warranty code is required';
      return;
    }
    if (!this.warrantyForm.name?.trim()) {
      this.errorMessage = 'Warranty name is required';
      return;
    }
    if (!this.warrantyForm.durationMonths || this.warrantyForm.durationMonths < 1) {
      this.errorMessage = 'Duration is required and must be at least 1 month';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedWarranty) {
      const updateRequest: UpdateWarrantyRequest = {
        name: this.warrantyForm.name,
        description: this.warrantyForm.description,
        type: this.warrantyForm.type,
        durationMonths: this.warrantyForm.durationMonths,
        extendedDurationMonths: this.warrantyForm.extendedDurationMonths,
        gracePeriodDays: this.warrantyForm.gracePeriodDays,
        maxDurationMonths: this.warrantyForm.maxDurationMonths,
        coverageType: this.warrantyForm.coverageType,
        includesParts: this.warrantyForm.includesParts,
        includesLabor: this.warrantyForm.includesLabor,
        includesOnsite: this.warrantyForm.includesOnsite,
        includesShipping: this.warrantyForm.includesShipping,
        coveredItems: this.warrantyForm.coveredItems,
        excludedItems: this.warrantyForm.excludedItems,
        terms: this.warrantyForm.terms,
        isTransferable: this.warrantyForm.isTransferable,
        requiresRegistration: this.warrantyForm.requiresRegistration,
        registrationDeadlineDays: this.warrantyForm.registrationDeadlineDays,
        requiresProofOfPurchase: this.warrantyForm.requiresProofOfPurchase,
        extendedWarrantyPrice: this.warrantyForm.extendedWarrantyPrice,
        extendedWarrantyPricePercent: this.warrantyForm.extendedWarrantyPricePercent,
        responseTimeHours: this.warrantyForm.responseTimeHours,
        resolutionTimeHours: this.warrantyForm.resolutionTimeHours,
        supportHoursType: this.warrantyForm.supportHoursType,
        maxClaims: this.warrantyForm.maxClaims,
        maxClaimValue: this.warrantyForm.maxClaimValue,
        maxClaimValuePercent: this.warrantyForm.maxClaimValuePercent,
        deductibleAmount: this.warrantyForm.deductibleAmount,
        isActive: this.warrantyForm.isActive,
        isDefault: this.warrantyForm.isDefault,
        displayOrder: this.warrantyForm.displayOrder
      };

      this.contractService.updateWarranty(this.selectedWarranty.id, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Warranty updated successfully';
            this.closeModal();
            this.loadWarranties();
          } else {
            this.errorMessage = response.message || 'Failed to update warranty';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.contractService.createWarranty(this.warrantyForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Warranty created successfully';
            this.closeModal();
            this.loadWarranties();
          } else {
            this.errorMessage = response.message || 'Failed to create warranty';
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

  deleteWarranty(warranty: WarrantySummary): void {
    if (confirm(`Are you sure you want to delete warranty "${warranty.name}"?`)) {
      this.loading = true;
      this.contractService.deleteWarranty(warranty.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Warranty deleted successfully';
            this.loadWarranties();
            if (this.selectedWarranty?.id === warranty.id) {
              this.selectedWarranty = null;
            }
          } else {
            this.errorMessage = response.message || 'Failed to delete warranty';
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

  private getEmptyForm(): CreateWarrantyRequest {
    return {
      code: '',
      name: '',
      type: WarrantyType.Standard,
      durationMonths: 12,
      coverageType: CoverageType.FullCoverage,
      includesParts: true,
      includesLabor: true,
      includesOnsite: false,
      includesShipping: false,
      isTransferable: false,
      requiresRegistration: false,
      requiresProofOfPurchase: true,
      isActive: true,
      isDefault: false,
      currency: 'INR'
    };
  }

  getTypeClass(type: WarrantyType): string {
    switch (type) {
      case WarrantyType.Standard: return 'type-standard';
      case WarrantyType.Extended: return 'type-extended';
      case WarrantyType.Limited: return 'type-limited';
      case WarrantyType.Lifetime: return 'type-lifetime';
      case WarrantyType.Prorated: return 'type-prorated';
      case WarrantyType.Express: return 'type-express';
      case WarrantyType.Implied: return 'type-implied';
      default: return 'type-default';
    }
  }

  getCoverageClass(coverage: CoverageType): string {
    switch (coverage) {
      case CoverageType.FullCoverage:
      case CoverageType.Comprehensive: return 'coverage-full';
      case CoverageType.PartsOnly: return 'coverage-parts';
      case CoverageType.LaborOnly: return 'coverage-labor';
      default: return 'coverage-default';
    }
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }

  formatCurrency(value: number | undefined, currency: string = 'INR'): string {
    if (value === undefined || value === null) return '-';
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency }).format(value);
  }
}
