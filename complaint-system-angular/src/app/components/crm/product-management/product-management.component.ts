import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ProductService } from '../../../services/product.service';
import { BrandService } from '../../../services/brand.service';
import { ProductSubTypeService } from '../../../services/product-subtype.service';
import { BrandLookup } from '../../../models/brand.model';
import { ProductSubTypeLookup } from '../../../models/product-subtype.model';
import {
  Product,
  ProductSummary,
  CreateProductRequest,
  ProductType,
  ProductStatus,
  UnitOfMeasure,
  TaxType,
  CategoryLookup,
  CategoryTreeNode,
  ProductTypeLabels,
  ProductStatusLabels,
  UnitOfMeasureLabels,
  TaxTypeLabels
} from '../../../models/product.model';
import { MultiSelectDropdownComponent, MultiSelectOption } from '../../shared/multi-select-dropdown/multi-select-dropdown.component';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule, MultiSelectDropdownComponent],
  templateUrl: './product-management.component.html',
  styleUrls: ['./product-management.component.scss']
})
export class ProductManagementComponent implements OnInit, OnDestroy {
  products: ProductSummary[] = [];
  selectedProduct: Product | null = null;
  categories: CategoryLookup[] = [];
  categoryMultiSelectOptions: MultiSelectOption[] = [];
  selectedCategoryIds: string[] = [];
  subTypes: ProductSubTypeLookup[] = [];
  brands: BrandLookup[] = [];
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
  totalProducts = 0;
  totalPages = 1;

  // Filters
  filterCategory: string | null = null;
  filterType: number | null = null;
  filterStatus: number | null = null;
  filterInStock: boolean | null = null;

  // Modal state
  showModal = false;
  isEditMode = false;
  activeTab = 'basic';

  // Quick Add Modal state
  showQuickAddCategory = false;
  showQuickAddSubType = false;
  showQuickAddBrand = false;
  quickAddLoading = false;
  quickCategoryForm = { code: '', name: '', description: '' };
  quickSubTypeForm = { categoryId: '', code: '', name: '', description: '' };
  quickBrandForm = { code: '', name: '', country: '' };

  // Form data
  productForm: CreateProductRequest = this.getEmptyForm();

  // Enums for template
  productTypes = Object.entries(ProductTypeLabels).map(([key, value]) => ({ value: +key, label: value }));
  productStatuses = Object.entries(ProductStatusLabels).map(([key, value]) => ({ value: +key, label: value }));
  unitsOfMeasure = Object.entries(UnitOfMeasureLabels).map(([key, value]) => ({ value: +key, label: value }));
  taxTypes = Object.entries(TaxTypeLabels).map(([key, value]) => ({ value: +key, label: value }));

  constructor(
    private productService: ProductService,
    private brandService: BrandService,
    private subTypeService: ProductSubTypeService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
    this.loadBrands();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.currentPage = 1;
      this.loadProducts();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getProducts({
      search: this.searchTerm || undefined,
      categoryId: this.filterCategory || undefined,
      type: this.filterType ?? undefined,
      status: this.filterStatus ?? undefined,
      inStock: this.filterInStock ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.products = response.data.items;
          this.totalProducts = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load products';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading products';
        this.loading = false;
      }
    });
  }

  loadCategories(): void {
    // Load category tree for hierarchical display
    this.productService.getCategoryTree().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Flatten tree and create multi-select options
          this.categoryMultiSelectOptions = this.flattenCategoryTree(response.data);
          // Also keep flat list for backward compatibility
          this.categories = response.data.map(c => ({
            id: c.id,
            code: c.code,
            name: c.name,
            fullPath: c.name,
            level: c.level
          }));
        }
      },
      error: () => {
        // Fallback to flat lookup if tree fails
        this.productService.getCategoryLookup().subscribe({
          next: (fallbackResponse) => {
            if (fallbackResponse.isSuccess) {
              this.categories = fallbackResponse.data;
              this.categoryMultiSelectOptions = fallbackResponse.data.map(c => ({
                id: c.id,
                label: c.name,
                code: c.code,
                level: c.level,
                hasChildren: false
              }));
            }
          }
        });
      }
    });
  }

  // Flatten category tree for multi-select options
  private flattenCategoryTree(nodes: CategoryTreeNode[], result: MultiSelectOption[] = []): MultiSelectOption[] {
    for (const node of nodes) {
      result.push({
        id: node.id,
        label: node.name,
        code: node.code,
        level: node.level,
        hasChildren: node.hasChildren
      });
      if (node.children && node.children.length > 0) {
        this.flattenCategoryTree(node.children, result);
      }
    }
    return result;
  }

  loadBrands(): void {
    this.brandService.getBrandLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.brands = response.data;
        }
      }
    });
  }

  loadSubTypes(categoryId?: string): void {
    if (!categoryId) {
      this.subTypes = [];
      return;
    }
    this.subTypeService.getSubTypeLookup(categoryId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.subTypes = response.data;
        }
      }
    });
  }

  onCategoryChange(categoryId: string | null): void {
    // Reset sub-type when category changes
    this.productForm.subTypeId = undefined;

    // Load sub-types for the selected category
    if (categoryId) {
      this.loadSubTypes(categoryId);
    } else {
      this.subTypes = [];
    }
  }

  // Handler for multi-select category selection
  onCategorySelectionChange(selectedIds: string[]): void {
    this.selectedCategoryIds = selectedIds;
    // Reset sub-type when categories change
    this.productForm.subTypeId = undefined;
    this.subTypes = [];

    // Set the first selected category as the primary categoryId
    if (selectedIds.length > 0) {
      this.productForm.categoryId = selectedIds[0];
      // Load sub-types for the first selected category
      this.loadSubTypes(selectedIds[0]);
    } else {
      this.productForm.categoryId = undefined;
    }
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.filterCategory = null;
    this.filterType = null;
    this.filterStatus = null;
    this.filterInStock = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadProducts();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProducts();
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

  // Product Detail
  viewProduct(product: ProductSummary): void {
    this.detailLoading = true;
    this.productService.getProductById(product.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedProduct = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  closeDetail(): void {
    this.selectedProduct = null;
  }

  // Modal Operations
  openAddModal(): void {
    this.productForm = this.getEmptyForm();
    this.subTypes = []; // Reset sub-types for new product
    this.isEditMode = false;
    this.activeTab = 'basic';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(product: ProductSummary): void {
    this.detailLoading = true;
    this.productService.getProductById(product.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const p = response.data;
          // Load sub-types for the product's category
          if (p.categoryId) {
            this.loadSubTypes(p.categoryId);
          } else {
            this.subTypes = [];
          }
          this.productForm = {
            categoryId: p.categoryId,
            subTypeId: p.subTypeId,
            parentProductId: p.parentProductId,
            productCode: p.productCode,
            name: p.name,
            shortDescription: p.shortDescription,
            description: p.description,
            type: p.type,
            sku: p.sku,
            partNumber: p.partNumber,
            hsnCode: p.hsnCode,
            sacCode: p.sacCode,
            unitPrice: p.unitPrice,
            costPrice: p.costPrice,
            msrp: p.msrp,
            minimumPrice: p.minimumPrice,
            currency: p.currency,
            taxRate: p.taxRate,
            isTaxable: p.isTaxable,
            taxType: p.taxType,
            unitOfMeasure: p.unitOfMeasure,
            minOrderQuantity: p.minOrderQuantity,
            maxOrderQuantity: p.maxOrderQuantity,
            trackInventory: p.trackInventory,
            quantityInStock: p.quantityInStock,
            reorderLevel: p.reorderLevel,
            reorderQuantity: p.reorderQuantity,
            leadTimeDays: p.leadTimeDays,
            allowBackorder: p.allowBackorder,
            brandId: p.brandId,
            manufacturer: p.manufacturer,
            model: p.model,
            version: p.version,
            weight: p.weight,
            countryOfOrigin: p.countryOfOrigin,
            imageUrl: p.imageUrl,
            defaultWarrantyMonths: p.defaultWarrantyMonths,
            extendedWarrantyMonths: p.extendedWarrantyMonths,
            warrantyTerms: p.warrantyTerms,
            isServiceable: p.isServiceable,
            requiresInstallation: p.requiresInstallation,
            isFeatured: p.isFeatured,
            isPublic: p.isPublic,
            requireSerialNumber: p.requireSerialNumber,
            notes: p.notes
          };
          this.selectedProduct = p;
          this.isEditMode = true;
          this.activeTab = 'basic';
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load product details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.productForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  saveProduct(): void {
    // Validate required fields
    if (!this.productForm.productCode?.trim()) {
      this.errorMessage = 'Product code is required';
      return;
    }
    if (!this.productForm.name?.trim()) {
      this.errorMessage = 'Product name is required';
      return;
    }
    if (this.productForm.unitPrice === undefined || this.productForm.unitPrice < 0) {
      this.errorMessage = 'Valid unit price is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedProduct) {
      this.productService.updateProduct(this.selectedProduct.id, {
        ...this.productForm,
        status: this.selectedProduct.status,
        statusReason: this.selectedProduct.statusReason
      }).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Product updated successfully';
            this.closeModal();
            this.loadProducts();
          } else {
            this.errorMessage = response.message || 'Failed to update product';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.productService.createProduct(this.productForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Product created successfully';
            this.closeModal();
            this.loadProducts();
          } else {
            this.errorMessage = response.message || 'Failed to create product';
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

  deleteProduct(product: ProductSummary): void {
    if (confirm(`Are you sure you want to delete product "${product.name}"?`)) {
      this.loading = true;
      this.productService.deleteProduct(product.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Product deleted successfully';
            this.loadProducts();
          } else {
            this.errorMessage = response.message || 'Failed to delete product';
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

  cloneProduct(product: ProductSummary): void {
    const newCode = prompt('Enter code for cloned product:', `${product.productCode}-COPY`);
    if (newCode) {
      this.loading = true;
      this.productService.cloneProduct(product.id, newCode).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Product cloned successfully';
            this.loadProducts();
          } else {
            this.errorMessage = response.message || 'Failed to clone product';
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

  // Quick Add Category
  openQuickAddCategory(): void {
    this.quickCategoryForm = { code: '', name: '', description: '' };
    this.showQuickAddCategory = true;
  }

  closeQuickAddCategory(): void {
    this.showQuickAddCategory = false;
  }

  saveQuickCategory(): void {
    if (!this.quickCategoryForm.code?.trim() || !this.quickCategoryForm.name?.trim()) {
      this.errorMessage = 'Category code and name are required';
      return;
    }

    this.quickAddLoading = true;
    this.productService.createCategory({
      code: this.quickCategoryForm.code,
      name: this.quickCategoryForm.name,
      description: this.quickCategoryForm.description,
      isActive: true,
      isPublic: true,
      allowProducts: true,
      allowSubCategories: true
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Category created successfully';
          this.loadCategories();
          // Auto-select the newly created category
          setTimeout(() => {
            const newCategory = this.categories.find(c => c.code === this.quickCategoryForm.code);
            if (newCategory) {
              this.productForm.categoryId = newCategory.id;
            }
          }, 500);
          this.closeQuickAddCategory();
        } else {
          this.errorMessage = response.message || 'Failed to create category';
        }
        this.quickAddLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.quickAddLoading = false;
      }
    });
  }

  // Quick Add SubType
  openQuickAddSubType(): void {
    this.quickSubTypeForm = {
      categoryId: this.productForm.categoryId || '',
      code: '',
      name: '',
      description: ''
    };
    this.showQuickAddSubType = true;
  }

  closeQuickAddSubType(): void {
    this.showQuickAddSubType = false;
  }

  saveQuickSubType(): void {
    if (!this.quickSubTypeForm.categoryId) {
      this.errorMessage = 'Please select a category first';
      return;
    }
    if (!this.quickSubTypeForm.code?.trim() || !this.quickSubTypeForm.name?.trim()) {
      this.errorMessage = 'Sub-type code and name are required';
      return;
    }

    this.quickAddLoading = true;
    this.subTypeService.createSubType({
      categoryId: this.quickSubTypeForm.categoryId,
      code: this.quickSubTypeForm.code,
      name: this.quickSubTypeForm.name,
      description: this.quickSubTypeForm.description,
      displayOrder: 0
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Sub-type created successfully';
          // Reload sub-types for the current category
          this.loadSubTypes(this.productForm.categoryId!);
          // Auto-select the newly created sub-type
          setTimeout(() => {
            const newSubType = this.subTypes.find(st => st.code === this.quickSubTypeForm.code);
            if (newSubType) {
              this.productForm.subTypeId = newSubType.id;
            }
          }, 500);
          this.closeQuickAddSubType();
        } else {
          this.errorMessage = response.message || 'Failed to create sub-type';
        }
        this.quickAddLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.quickAddLoading = false;
      }
    });
  }

  // Quick Add Brand
  openQuickAddBrand(): void {
    this.quickBrandForm = { code: '', name: '', country: '' };
    this.showQuickAddBrand = true;
  }

  closeQuickAddBrand(): void {
    this.showQuickAddBrand = false;
  }

  saveQuickBrand(): void {
    if (!this.quickBrandForm.code?.trim() || !this.quickBrandForm.name?.trim()) {
      this.errorMessage = 'Brand code and name are required';
      return;
    }

    this.quickAddLoading = true;
    this.brandService.createBrand({
      code: this.quickBrandForm.code,
      name: this.quickBrandForm.name,
      country: this.quickBrandForm.country,
      displayOrder: 0
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Brand created successfully';
          this.loadBrands();
          // Auto-select the newly created brand
          setTimeout(() => {
            const newBrand = this.brands.find(b => b.code === this.quickBrandForm.code);
            if (newBrand) {
              this.productForm.brandId = newBrand.id;
            }
          }, 500);
          this.closeQuickAddBrand();
        } else {
          this.errorMessage = response.message || 'Failed to create brand';
        }
        this.quickAddLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.quickAddLoading = false;
      }
    });
  }

  private getEmptyForm(): CreateProductRequest {
    return {
      productCode: '',
      name: '',
      type: ProductType.Physical,
      unitPrice: 0,
      currency: 'INR',
      unitOfMeasure: UnitOfMeasure.Each,
      isTaxable: true,
      taxType: TaxType.GST,
      trackInventory: false,
      allowBackorder: false,
      isServiceable: false,
      requiresInstallation: false,
      isFeatured: false,
      isPublic: true,
      requireSerialNumber: false
    };
  }

  getStatusClass(status: ProductStatus): string {
    switch (status) {
      case ProductStatus.Active: return 'status-active';
      case ProductStatus.Inactive: return 'status-inactive';
      case ProductStatus.Draft: return 'status-draft';
      case ProductStatus.Discontinued: return 'status-discontinued';
      case ProductStatus.EndOfLife: return 'status-eol';
      case ProductStatus.Archived: return 'status-archived';
      default: return '';
    }
  }

  getTypeClass(type: ProductType): string {
    switch (type) {
      case ProductType.Physical: return 'type-physical';
      case ProductType.Digital: return 'type-digital';
      case ProductType.Service: return 'type-service';
      case ProductType.Subscription: return 'type-subscription';
      case ProductType.Bundle: return 'type-bundle';
      case ProductType.SparePart: return 'type-sparepart';
      case ProductType.Maintenance: return 'type-maintenance';
      default: return '';
    }
  }

  formatCurrency(value: number, currency: string = 'INR'): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: currency
    }).format(value);
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }
}
