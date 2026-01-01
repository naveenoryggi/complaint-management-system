import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import {
  ProductCategory,
  CategoryTreeNode,
  CreateCategoryRequest
} from '../../../models/product.model';

@Component({
  selector: 'app-product-category',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-category.component.html',
  styleUrls: ['./product-category.component.scss']
})
export class ProductCategoryComponent implements OnInit {
  categoryTree: CategoryTreeNode[] = [];
  selectedCategory: ProductCategory | null = null;
  expandedNodes: Set<string> = new Set();
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';

  // Modal state
  showModal = false;
  isEditMode = false;
  parentCategoryId: string | null = null;

  // Form data
  categoryForm: CreateCategoryRequest = this.getEmptyForm();

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadCategoryTree();
  }

  loadCategoryTree(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getCategoryTree().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.categoryTree = response.data;
        } else {
          this.errorMessage = response.message || 'Failed to load categories';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading categories';
        this.loading = false;
      }
    });
  }

  toggleNode(nodeId: string): void {
    if (this.expandedNodes.has(nodeId)) {
      this.expandedNodes.delete(nodeId);
    } else {
      this.expandedNodes.add(nodeId);
    }
  }

  isExpanded(nodeId: string): boolean {
    return this.expandedNodes.has(nodeId);
  }

  selectCategory(node: CategoryTreeNode): void {
    this.detailLoading = true;
    this.productService.getCategoryById(node.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedCategory = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  closeDetail(): void {
    this.selectedCategory = null;
  }

  // Modal Operations
  openAddRootModal(): void {
    this.categoryForm = this.getEmptyForm();
    this.parentCategoryId = null;
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
  }

  openAddChildModal(parentId: string): void {
    this.categoryForm = this.getEmptyForm();
    this.parentCategoryId = parentId;
    this.categoryForm.parentCategoryId = parentId;
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(node: CategoryTreeNode): void {
    this.detailLoading = true;
    this.productService.getCategoryById(node.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const c = response.data;
          this.categoryForm = {
            parentCategoryId: c.parentCategoryId,
            code: c.code,
            name: c.name,
            description: c.description,
            displayOrder: c.displayOrder,
            icon: c.icon,
            color: c.color,
            imageUrl: c.imageUrl,
            defaultWarrantyMonths: c.defaultWarrantyMonths,
            defaultResponseTimeHours: c.defaultResponseTimeHours,
            defaultResolutionTimeHours: c.defaultResolutionTimeHours,
            defaultTaxRate: c.defaultTaxRate,
            defaultHSNCode: c.defaultHSNCode,
            defaultSACCode: c.defaultSACCode,
            requireSerialNumber: c.requireSerialNumber,
            trackInventory: c.trackInventory,
            isPublic: c.isPublic,
            allowProducts: c.allowProducts,
            allowSubCategories: c.allowSubCategories,
            isActive: c.isActive,
            slug: c.slug,
            metaTitle: c.metaTitle,
            metaDescription: c.metaDescription,
            keywords: c.keywords,
            notes: c.notes
          };
          this.selectedCategory = c;
          this.isEditMode = true;
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load category details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.categoryForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  saveCategory(): void {
    // Validate required fields
    if (!this.categoryForm.code?.trim()) {
      this.errorMessage = 'Category code is required';
      return;
    }
    if (!this.categoryForm.name?.trim()) {
      this.errorMessage = 'Category name is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedCategory) {
      this.productService.updateCategory(this.selectedCategory.id, this.categoryForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Category updated successfully';
            this.closeModal();
            this.loadCategoryTree();
          } else {
            this.errorMessage = response.message || 'Failed to update category';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.productService.createCategory(this.categoryForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Category created successfully';
            this.closeModal();
            this.loadCategoryTree();
            // Auto-expand parent if adding child
            if (this.parentCategoryId) {
              this.expandedNodes.add(this.parentCategoryId);
            }
          } else {
            this.errorMessage = response.message || 'Failed to create category';
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

  deleteCategory(node: CategoryTreeNode): void {
    if (node.children && node.children.length > 0) {
      this.errorMessage = 'Cannot delete category with subcategories. Delete subcategories first.';
      return;
    }

    if (confirm(`Are you sure you want to delete category "${node.name}"?`)) {
      this.loading = true;
      this.productService.deleteCategory(node.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Category deleted successfully';
            this.loadCategoryTree();
            if (this.selectedCategory?.id === node.id) {
              this.selectedCategory = null;
            }
          } else {
            this.errorMessage = response.message || 'Failed to delete category';
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

  private getEmptyForm(): CreateCategoryRequest {
    return {
      code: '',
      name: '',
      description: '',
      displayOrder: 0,
      isActive: true,
      isPublic: true,
      allowProducts: true,
      allowSubCategories: true,
      trackInventory: false,
      requireSerialNumber: false
    };
  }

  expandAll(): void {
    this.addAllNodeIds(this.categoryTree);
  }

  collapseAll(): void {
    this.expandedNodes.clear();
  }

  private addAllNodeIds(nodes: CategoryTreeNode[]): void {
    for (const node of nodes) {
      if (node.children && node.children.length > 0) {
        this.expandedNodes.add(node.id);
        this.addAllNodeIds(node.children);
      }
    }
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }
}
