import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StockService } from '../../../services/stock.service';
import { ProductService } from '../../../services/product.service';
import { StockCategoryService, StockCategoryLookup } from '../../../services/stock-category.service';
import {
  StockItem,
  CreateStockItemRequest,
  UpdateStockItemRequest,
  AdjustStockRequest,
  TransferStockRequest,
  StockLocation,
  StockItemStatus,
  StockCondition,
  CostingMethod,
  StockItemStatusLabels,
  StockConditionLabels,
  CostingMethodLabels,
  StockMovement
} from '../../../models/stock.model';
import { ProductLookup } from '../../../models/product.model';

@Component({
  selector: 'app-stock-item-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './stock-item-management.component.html',
  styleUrls: ['./stock-item-management.component.scss']
})
export class StockItemManagementComponent implements OnInit {
  Math = Math;

  // Data
  stockItems = signal<StockItem[]>([]);
  products = signal<ProductLookup[]>([]);
  stockCategories = signal<StockCategoryLookup[]>([]);
  stockLocations = signal<StockLocation[]>([]);
  selectedItem = signal<StockItem | null>(null);
  movementHistory = signal<StockMovement[]>([]);

  // UI State
  isLoading = signal(false);
  showModal = signal(false);
  showAdjustModal = signal(false);
  showTransferModal = signal(false);
  showHistoryModal = signal(false);
  isEditMode = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Pagination
  currentPage = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));

  // Filters
  searchTerm = signal('');
  locationFilter = signal<string | null>(null);
  categoryFilter = signal<string | null>(null);
  lowStockFilter = signal(false);

  // Forms
  stockItemForm: FormGroup;
  adjustForm: FormGroup;
  transferForm: FormGroup;

  // Enum options
  statusOptions = Object.entries(StockItemStatusLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  conditionOptions = Object.entries(StockConditionLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  costingMethodOptions = Object.entries(CostingMethodLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  constructor(
    private stockService: StockService,
    private productService: ProductService,
    private stockCategoryService: StockCategoryService,
    private fb: FormBuilder
  ) {
    this.stockItemForm = this.fb.group({
      productId: ['', Validators.required],
      stockCategoryId: ['', Validators.required],
      locationId: [''],
      quantityOnHand: [0, [Validators.required, Validators.min(0)]],
      quantityReserved: [0, Validators.min(0)],
      minimumQuantity: [null],
      maximumQuantity: [null],
      reorderQuantity: [null],
      safetyStock: [null],
      unitOfMeasure: ['Each'],
      unitCost: [null],
      currency: ['INR'],
      costingMethod: [CostingMethod.FIFO],
      status: [StockItemStatus.Active],
      condition: [StockCondition.New],
      expiryDate: [''],
      batchNumber: [''],
      notes: ['']
    });

    this.adjustForm = this.fb.group({
      adjustmentQuantity: [0, [Validators.required, Validators.pattern(/^-?\d+(\.\d+)?$/)]],
      reason: ['', Validators.required],
      notes: ['']
    });

    this.transferForm = this.fb.group({
      toLocationId: ['', Validators.required],
      toStockCategoryId: [''],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      reason: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadStockItems();
    this.loadProducts();
    this.loadStockCategories();
    this.loadStockLocations();
  }

  loadStockItems(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.stockService.getStockItems({
      page: this.currentPage(),
      pageSize: this.pageSize(),
      searchTerm: this.searchTerm() || undefined,
      locationId: this.locationFilter() || undefined,
      stockCategoryId: this.categoryFilter() || undefined,
      lowStock: this.lowStockFilter() || undefined
    }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stockItems.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        } else {
          this.error.set(response.message || 'Failed to load stock items');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load stock items');
        this.isLoading.set(false);
        console.error('Error loading stock items:', err);
      }
    });
  }

  loadProducts(): void {
    this.productService.getProductLookup().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.products.set(response.data);
        }
      },
      error: (err) => console.error('Error loading products:', err)
    });
  }

  loadStockCategories(): void {
    this.stockCategoryService.getLookup(true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stockCategories.set(response.data);
        }
      },
      error: (err) => console.error('Error loading stock categories:', err)
    });
  }

  loadStockLocations(): void {
    this.stockService.getStockLocations({ pageSize: 100, isActive: true }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stockLocations.set(response.data.items);
        }
      },
      error: (err) => console.error('Error loading stock locations:', err)
    });
  }

  // Search & Filters
  onSearch(): void {
    this.currentPage.set(1);
    this.loadStockItems();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.locationFilter.set(null);
    this.categoryFilter.set(null);
    this.lowStockFilter.set(false);
    this.currentPage.set(1);
    this.loadStockItems();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadStockItems();
    }
  }

  // Add/Edit Modal
  openAddModal(): void {
    this.isEditMode.set(false);
    this.selectedItem.set(null);
    this.stockItemForm.reset({
      quantityOnHand: 0,
      quantityReserved: 0,
      unitOfMeasure: 'Each',
      currency: 'INR',
      costingMethod: CostingMethod.FIFO,
      status: StockItemStatus.Active,
      condition: StockCondition.New
    });
    this.showModal.set(true);
  }

  openEditModal(item: StockItem): void {
    this.isEditMode.set(true);
    this.selectedItem.set(item);
    this.stockItemForm.patchValue({
      productId: item.productId,
      stockCategoryId: item.stockCategoryId,
      locationId: item.locationId || '',
      quantityOnHand: item.quantityOnHand,
      quantityReserved: item.quantityReserved,
      minimumQuantity: item.minimumQuantity,
      maximumQuantity: item.maximumQuantity,
      reorderQuantity: item.reorderQuantity,
      safetyStock: item.safetyStock,
      unitOfMeasure: item.unitOfMeasure,
      unitCost: item.unitCost,
      currency: item.currency,
      costingMethod: item.costingMethod,
      status: item.status,
      condition: item.condition,
      expiryDate: item.expiryDate ? new Date(item.expiryDate).toISOString().split('T')[0] : '',
      batchNumber: item.batchNumber || '',
      notes: item.notes || ''
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedItem.set(null);
  }

  saveStockItem(): void {
    if (this.stockItemForm.invalid) {
      this.stockItemForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.stockItemForm.value;

    if (this.isEditMode()) {
      const request: UpdateStockItemRequest = {
        minimumQuantity: formValue.minimumQuantity,
        maximumQuantity: formValue.maximumQuantity,
        reorderQuantity: formValue.reorderQuantity,
        safetyStock: formValue.safetyStock,
        unitOfMeasure: formValue.unitOfMeasure,
        unitCost: formValue.unitCost,
        currency: formValue.currency,
        costingMethod: formValue.costingMethod,
        status: formValue.status,
        condition: formValue.condition,
        expiryDate: formValue.expiryDate || undefined,
        batchNumber: formValue.batchNumber || undefined,
        notes: formValue.notes || undefined
      };

      this.stockService.updateStockItem(this.selectedItem()!.id, request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage.set('Stock item updated successfully');
            this.closeModal();
            this.loadStockItems();
            setTimeout(() => this.successMessage.set(null), 3000);
          } else {
            this.error.set(response.message || 'Failed to update stock item');
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to update stock item');
          this.isLoading.set(false);
        }
      });
    } else {
      const request: CreateStockItemRequest = {
        productId: formValue.productId,
        stockCategoryId: formValue.stockCategoryId,
        locationId: formValue.locationId || undefined,
        quantityOnHand: formValue.quantityOnHand,
        quantityReserved: formValue.quantityReserved,
        minimumQuantity: formValue.minimumQuantity,
        maximumQuantity: formValue.maximumQuantity,
        reorderQuantity: formValue.reorderQuantity,
        safetyStock: formValue.safetyStock,
        unitOfMeasure: formValue.unitOfMeasure,
        unitCost: formValue.unitCost,
        currency: formValue.currency,
        costingMethod: formValue.costingMethod,
        status: formValue.status,
        condition: formValue.condition,
        expiryDate: formValue.expiryDate || undefined,
        batchNumber: formValue.batchNumber || undefined,
        notes: formValue.notes || undefined
      };

      this.stockService.createStockItem(request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage.set('Stock item created successfully');
            this.closeModal();
            this.loadStockItems();
            setTimeout(() => this.successMessage.set(null), 3000);
          } else {
            this.error.set(response.message || 'Failed to create stock item');
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to create stock item');
          this.isLoading.set(false);
        }
      });
    }
  }

  // Adjust Stock
  openAdjustModal(item: StockItem): void {
    this.selectedItem.set(item);
    this.adjustForm.reset({
      adjustmentQuantity: 0,
      reason: '',
      notes: ''
    });
    this.showAdjustModal.set(true);
  }

  closeAdjustModal(): void {
    this.showAdjustModal.set(false);
    this.selectedItem.set(null);
  }

  saveAdjustment(): void {
    if (this.adjustForm.invalid || !this.selectedItem()) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.adjustForm.value;

    const request: AdjustStockRequest = {
      stockItemId: this.selectedItem()!.id,
      adjustmentQuantity: parseFloat(formValue.adjustmentQuantity),
      reason: formValue.reason,
      notes: formValue.notes || undefined
    };

    this.stockService.adjustStock(this.selectedItem()!.id, request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Stock adjusted successfully');
          this.closeAdjustModal();
          this.loadStockItems();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to adjust stock');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to adjust stock');
        this.isLoading.set(false);
      }
    });
  }

  // Transfer Stock
  openTransferModal(item: StockItem): void {
    this.selectedItem.set(item);
    this.transferForm.reset({
      toLocationId: '',
      toStockCategoryId: '',
      quantity: 1,
      reason: '',
      notes: ''
    });
    this.showTransferModal.set(true);
  }

  closeTransferModal(): void {
    this.showTransferModal.set(false);
    this.selectedItem.set(null);
  }

  saveTransfer(): void {
    if (this.transferForm.invalid || !this.selectedItem()) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.transferForm.value;

    const request: TransferStockRequest = {
      stockItemId: this.selectedItem()!.id,
      toLocationId: formValue.toLocationId,
      toStockCategoryId: formValue.toStockCategoryId || undefined,
      quantity: parseFloat(formValue.quantity),
      reason: formValue.reason || undefined,
      notes: formValue.notes || undefined
    };

    this.stockService.transferStock(this.selectedItem()!.id, request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Stock transferred successfully');
          this.closeTransferModal();
          this.loadStockItems();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to transfer stock');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to transfer stock');
        this.isLoading.set(false);
      }
    });
  }

  // Movement History
  openHistoryModal(item: StockItem): void {
    this.selectedItem.set(item);
    this.movementHistory.set([]);
    this.showHistoryModal.set(true);

    this.stockService.getMovementsByStockItem(item.id, 50).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.movementHistory.set(response.data);
        }
      },
      error: (err) => console.error('Error loading movement history:', err)
    });
  }

  closeHistoryModal(): void {
    this.showHistoryModal.set(false);
    this.selectedItem.set(null);
    this.movementHistory.set([]);
  }

  // Delete
  deleteStockItem(item: StockItem): void {
    if (!confirm(`Are you sure you want to delete stock item "${item.productName}"?`)) {
      return;
    }

    this.isLoading.set(true);
    this.stockService.deleteStockItem(item.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Stock item deleted successfully');
          this.loadStockItems();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to delete stock item');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to delete stock item');
        this.isLoading.set(false);
      }
    });
  }

  // Helpers
  getStatusClass(status: number): string {
    const classes: { [key: number]: string } = {
      [StockItemStatus.Active]: 'status-active',
      [StockItemStatus.Inactive]: 'status-inactive',
      [StockItemStatus.OnHold]: 'status-on-hold',
      [StockItemStatus.Discontinued]: 'status-discontinued',
      [StockItemStatus.Reserved]: 'status-reserved'
    };
    return classes[status] || 'status-default';
  }

  getConditionClass(condition: number): string {
    if (condition <= 1) return 'condition-excellent';
    if (condition <= 2) return 'condition-good';
    return 'condition-poor';
  }

  isLowStock(item: StockItem): boolean {
    if (!item.minimumQuantity) return false;
    return item.quantityAvailable <= item.minimumQuantity;
  }

  formatCurrency(value: number | undefined, currency: string = 'INR'): string {
    if (value === undefined || value === null) return '-';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: currency
    }).format(value);
  }

  formatNumber(value: number | undefined): string {
    if (value === undefined || value === null) return '-';
    return new Intl.NumberFormat('en-IN').format(value);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDateTime(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getMovementTypeLabel(type: number): string {
    const labels: { [key: number]: string } = {
      0: 'Receipt', 1: 'Return', 2: 'Transfer In', 3: 'Production', 4: 'Adjustment (+)',
      10: 'Issue', 11: 'Sale', 12: 'Transfer Out', 13: 'Consumption', 14: 'Adjustment (-)',
      15: 'Scrap', 16: 'Loss', 20: 'Category Change', 21: 'Reserve', 22: 'Unreserve',
      30: 'Stock Count', 31: 'Revaluation'
    };
    return labels[type] || 'Unknown';
  }

  getMovementTypeClass(type: number): string {
    if (type <= 4) return 'movement-in';
    if (type >= 10 && type <= 16) return 'movement-out';
    return 'movement-other';
  }
}
