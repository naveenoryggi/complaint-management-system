import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StockService } from '../../../services/stock.service';
import {
  StockMovement,
  StockMovementType,
  StockMovementStatus,
  CreateStockMovementRequest
} from '../../../models/stock.model';

@Component({
  selector: 'app-stock-movement-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './stock-movement-management.component.html',
  styleUrls: ['./stock-movement-management.component.scss']
})
export class StockMovementManagementComponent implements OnInit {
  // Data
  movements = signal<StockMovement[]>([]);
  stockItems = signal<any[]>([]);
  movementTypes = signal<any[]>([]);
  movementStatuses = signal<any[]>([]);

  // State
  isLoading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Pagination
  currentPage = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));

  // Filters
  searchTerm = signal('');
  filterType = signal<string>('');
  filterStatus = signal<string>('');
  filterStockItemId = signal<string>('');
  dateFrom = signal<string>('');
  dateTo = signal<string>('');

  // Modal state
  showModal = signal(false);
  showDetailModal = signal(false);
  selectedMovement = signal<StockMovement | null>(null);

  // Form
  movementForm!: FormGroup;

  // Enums for template
  StockMovementType = StockMovementType;
  StockMovementStatus = StockMovementStatus;

  constructor(
    private stockService: StockService,
    private fb: FormBuilder
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadData();
  }

  private initForm(): void {
    this.movementForm = this.fb.group({
      stockItemId: [''],
      productId: [''],
      assetId: [''],
      customerId: [''],
      movementType: [StockMovementType.Adjustment_In, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitCost: [0],
      reason: ['', Validators.required],
      notes: [''],
      referenceNumber: ['']
    });
  }

  loadData(): void {
    this.loadMovements();
    this.loadStockItems();
    this.loadMovementTypes();
    this.loadMovementStatuses();
  }

  loadMovements(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const params: any = {
      pageNumber: this.currentPage(),
      pageSize: this.pageSize()
    };

    if (this.searchTerm()) params.searchTerm = this.searchTerm();
    if (this.filterType()) params.movementType = parseInt(this.filterType());
    if (this.filterStatus()) params.status = parseInt(this.filterStatus());
    if (this.filterStockItemId()) params.stockItemId = this.filterStockItemId();
    if (this.dateFrom()) params.dateFrom = this.dateFrom();
    if (this.dateTo()) params.dateTo = this.dateTo();

    this.stockService.getStockMovements(params).subscribe({
      next: (response) => {
        this.movements.set(response.data?.items || []);
        this.totalCount.set(response.data?.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (err: Error) => {
        console.error('Error loading movements:', err);
        this.error.set('Failed to load stock movements');
        this.isLoading.set(false);
      }
    });
  }

  loadStockItems(): void {
    this.stockService.getStockItemLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.stockItems.set(response.data || []);
        }
      },
      error: (err: Error) => console.error('Error loading stock items:', err)
    });
  }

  loadMovementTypes(): void {
    this.stockService.getMovementTypes().subscribe({
      next: (types) => {
        this.movementTypes.set(types || []);
      },
      error: (error) => console.error('Error loading movement types:', error)
    });
  }

  loadMovementStatuses(): void {
    this.stockService.getMovementStatuses().subscribe({
      next: (statuses) => {
        this.movementStatuses.set(statuses || []);
      },
      error: (error) => console.error('Error loading movement statuses:', error)
    });
  }

  // Filter handlers
  onSearch(): void {
    this.currentPage.set(1);
    this.loadMovements();
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.loadMovements();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.filterType.set('');
    this.filterStatus.set('');
    this.filterStockItemId.set('');
    this.dateFrom.set('');
    this.dateTo.set('');
    this.currentPage.set(1);
    this.loadMovements();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadMovements();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const total = this.totalPages();
    const current = this.currentPage();

    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);

    if (end - start < 4) {
      if (start === 1) {
        end = Math.min(total, start + 4);
      } else {
        start = Math.max(1, end - 4);
      }
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  // Modal operations
  openCreateModal(): void {
    this.movementForm.reset({
      movementType: StockMovementType.Adjustment_In,
      quantity: 1,
      unitCost: 0
    });
    this.selectedMovement.set(null);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedMovement.set(null);
    this.movementForm.reset();
  }

  viewMovement(movement: StockMovement): void {
    this.selectedMovement.set(movement);
    this.showDetailModal.set(true);
  }

  closeDetailModal(): void {
    this.showDetailModal.set(false);
    this.selectedMovement.set(null);
  }

  onSubmit(): void {
    if (this.movementForm.invalid) return;

    this.isLoading.set(true);
    const formValue = this.movementForm.value;

    const request: CreateStockMovementRequest = {
      stockItemId: formValue.stockItemId || undefined,
      productId: formValue.productId || '',
      assetId: formValue.assetId || undefined,
      customerId: formValue.customerId || undefined,
      movementType: formValue.movementType,
      quantity: formValue.quantity,
      unitCost: formValue.unitCost || undefined,
      reason: formValue.reason,
      notes: formValue.notes || undefined,
      referenceNumber: formValue.referenceNumber || undefined
    };

    this.stockService.createStockMovement(request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Stock movement created successfully');
          this.closeModal();
          this.loadMovements();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to create movement');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error creating movement:', error);
        this.error.set(error.error?.message || 'Failed to create movement');
        this.isLoading.set(false);
      }
    });
  }

  // Action operations
  approveMovement(movement: StockMovement): void {
    if (!confirm('Are you sure you want to approve this movement?')) return;

    this.isLoading.set(true);
    this.stockService.approveMovement(movement.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Movement approved successfully');
          this.loadMovements();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to approve movement');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error approving movement:', error);
        this.error.set(error.error?.message || 'Failed to approve movement');
        this.isLoading.set(false);
      }
    });
  }

  rejectMovement(movement: StockMovement): void {
    const reason = prompt('Please enter rejection reason:');
    if (!reason) return;

    this.isLoading.set(true);
    this.stockService.rejectMovement(movement.id, reason).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Movement rejected');
          this.loadMovements();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to reject movement');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error rejecting movement:', error);
        this.error.set(error.error?.message || 'Failed to reject movement');
        this.isLoading.set(false);
      }
    });
  }

  cancelMovement(movement: StockMovement): void {
    const reason = prompt('Please enter cancellation reason:');
    if (!reason) return;

    this.isLoading.set(true);
    this.stockService.cancelMovement(movement.id, reason).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Movement cancelled');
          this.loadMovements();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to cancel movement');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error cancelling movement:', error);
        this.error.set(error.error?.message || 'Failed to cancel movement');
        this.isLoading.set(false);
      }
    });
  }

  reverseMovement(movement: StockMovement): void {
    const reason = prompt('Please enter reversal reason:');
    if (!reason) return;

    this.isLoading.set(true);
    this.stockService.reverseMovement(movement.id, reason).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Movement reversed. A new reversal movement has been created.');
          this.loadMovements();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to reverse movement');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error reversing movement:', error);
        this.error.set(error.error?.message || 'Failed to reverse movement');
        this.isLoading.set(false);
      }
    });
  }

  // Helper methods
  getMovementTypeName(type: StockMovementType): string {
    const names: Record<number, string> = {
      [StockMovementType.Receipt]: 'Receipt',
      [StockMovementType.Return]: 'Return',
      [StockMovementType.TransferIn]: 'Transfer In',
      [StockMovementType.Production]: 'Production',
      [StockMovementType.Adjustment_In]: 'Adjustment In',
      [StockMovementType.Issue]: 'Issue',
      [StockMovementType.Sale]: 'Sale',
      [StockMovementType.TransferOut]: 'Transfer Out',
      [StockMovementType.Consumption]: 'Consumption',
      [StockMovementType.Adjustment_Out]: 'Adjustment Out',
      [StockMovementType.Scrap]: 'Scrap',
      [StockMovementType.Loss]: 'Loss',
      [StockMovementType.CategoryChange]: 'Category Change',
      [StockMovementType.Reservation]: 'Reservation',
      [StockMovementType.Unreservation]: 'Unreservation',
      [StockMovementType.StockCount]: 'Stock Count',
      [StockMovementType.Revaluation]: 'Revaluation'
    };
    return names[type] || 'Unknown';
  }

  getStatusName(status: StockMovementStatus): string {
    const names: Record<number, string> = {
      [StockMovementStatus.Draft]: 'Draft',
      [StockMovementStatus.Pending]: 'Pending',
      [StockMovementStatus.Approved]: 'Approved',
      [StockMovementStatus.Completed]: 'Completed',
      [StockMovementStatus.Cancelled]: 'Cancelled',
      [StockMovementStatus.Reversed]: 'Reversed'
    };
    return names[status] || 'Unknown';
  }

  isIncrease(type: StockMovementType): boolean {
    return [
      StockMovementType.Receipt,
      StockMovementType.Return,
      StockMovementType.TransferIn,
      StockMovementType.Production,
      StockMovementType.Adjustment_In
    ].includes(type);
  }

  canApprove(movement: StockMovement): boolean {
    return movement.status === StockMovementStatus.Pending;
  }

  canCancel(movement: StockMovement): boolean {
    return movement.status === StockMovementStatus.Draft || movement.status === StockMovementStatus.Pending;
  }

  canReverse(movement: StockMovement): boolean {
    return movement.status === StockMovementStatus.Completed;
  }

  dismissError(): void {
    this.error.set(null);
  }

  dismissSuccess(): void {
    this.successMessage.set(null);
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleString();
  }

  formatNumber(value: number | undefined): string {
    if (value === undefined || value === null) return '-';
    return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
}
