import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AssetService, AssetSummary, Asset, CreateAssetRequest, AssetStatus, AssetType, AssetCondition, AssetOwnershipType, AssetAssignmentPurpose, AssetStatistics } from '../../../services/asset.service';
import { CustomerService } from '../../../services/customer.service';
import { Customer } from '../../../models/customer.model';
import { StockCategoryService, StockCategoryLookup } from '../../../services/stock-category.service';

@Component({
  selector: 'app-asset-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './asset-management.component.html',
  styleUrls: ['./asset-management.component.scss']
})
export class AssetManagementComponent implements OnInit {
  Math = Math;  // Expose Math for template

  // Data
  assets = signal<AssetSummary[]>([]);
  customers = signal<Customer[]>([]);
  stockCategories = signal<StockCategoryLookup[]>([]);
  statistics = signal<AssetStatistics | null>(null);
  selectedAsset = signal<Asset | null>(null);

  // UI State
  isLoading = signal(false);
  showModal = signal(false);
  showAssignModal = signal(false);
  showTransferModal = signal(false);
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
  statusFilter = signal<number | null>(null);
  typeFilter = signal<number | null>(null);
  customerFilter = signal<string | null>(null);

  // Forms
  assetForm: FormGroup;
  assignForm: FormGroup;
  transferForm: FormGroup;

  // Enum options for dropdowns
  statusOptions = [
    { value: 0, label: 'Ordered' },
    { value: 1, label: 'Received' },
    { value: 2, label: 'In Stock' },
    { value: 3, label: 'Deployed' },
    { value: 4, label: 'In Service' },
    { value: 5, label: 'Under Repair' },
    { value: 6, label: 'Out of Service' },
    { value: 7, label: 'Retired' },
    { value: 8, label: 'Disposed' },
    { value: 9, label: 'Lost' },
    { value: 10, label: 'In Transit' },
    { value: 11, label: 'On Loan' },
    { value: 12, label: 'Returned' },
    { value: 13, label: 'Reserved' }
  ];

  typeOptions = [
    { value: 1, label: 'Hardware' },
    { value: 2, label: 'Software' },
    { value: 3, label: 'Network' },
    { value: 4, label: 'Peripheral' },
    { value: 5, label: 'Mobile' },
    { value: 6, label: 'Furniture' },
    { value: 7, label: 'Vehicle' },
    { value: 8, label: 'Machinery' },
    { value: 9, label: 'Tool' },
    { value: 10, label: 'Office Equipment' },
    { value: 11, label: 'Medical Equipment' },
    { value: 12, label: 'Lab Equipment' },
    { value: 99, label: 'Other' }
  ];

  conditionOptions = [
    { value: 1, label: 'New' },
    { value: 2, label: 'Excellent' },
    { value: 3, label: 'Good' },
    { value: 4, label: 'Fair' },
    { value: 5, label: 'Poor' },
    { value: 6, label: 'Non-Functional' },
    { value: 7, label: 'Beyond Repair' },
    { value: 8, label: 'Refurbished' }
  ];

  ownershipOptions = [
    { value: 1, label: 'Owned' },
    { value: 2, label: 'Leased' },
    { value: 3, label: 'Rented' },
    { value: 4, label: 'Loaned' },
    { value: 5, label: 'Financed' },
    { value: 6, label: 'Customer Owned' },
    { value: 7, label: 'Consignment' },
    { value: 8, label: 'Demo' },
    { value: 9, label: 'Development' }
  ];

  assignmentPurposeOptions = [
    { value: 1, label: 'Permanent' },
    { value: 2, label: 'Demo' },
    { value: 3, label: 'Development' },
    { value: 4, label: 'Temporary Loan' },
    { value: 5, label: 'Training' },
    { value: 6, label: 'POC' },
    { value: 7, label: 'Replacement' }
  ];

  constructor(
    private assetService: AssetService,
    private customerService: CustomerService,
    private stockCategoryService: StockCategoryService,
    private fb: FormBuilder
  ) {
    this.assetForm = this.fb.group({
      customerId: ['', Validators.required],
      locationId: [''],
      productId: [''],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      type: [1, Validators.required],
      serialNumber: [''],
      manufacturer: [''],
      model: [''],
      status: [2],
      condition: [1],
      stockCategoryId: [''],  // Dynamic stock category (GUID)
      ownershipType: [1],
      purchaseDate: [''],
      purchasePrice: [''],
      warrantyStartDate: [''],
      warrantyEndDate: [''],
      notes: [''],
      // Assignment
      assignedUserId: [''],
      assignmentPurpose: [1],
      assignmentDate: [''],
      expectedReturnDate: [''],
      assignmentNotes: ['']
    });

    this.assignForm = this.fb.group({
      assignedUserId: [''],
      customerId: [''],
      departmentId: [''],
      assignmentPurpose: [1, Validators.required],
      assignmentDate: [new Date().toISOString().split('T')[0]],
      expectedReturnDate: [''],
      assignmentNotes: ['']
    });

    this.transferForm = this.fb.group({
      newCustomerId: ['', Validators.required],
      newLocationId: [''],
      transferDate: [new Date().toISOString().split('T')[0], Validators.required],
      transferReason: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadAssets();
    this.loadCustomers();
    this.loadStockCategories();
    this.loadStatistics();
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

  loadAssets(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.assetService.getAssets(
      this.currentPage(),
      this.pageSize(),
      this.searchTerm() || undefined,
      this.statusFilter() ?? undefined,
      this.typeFilter() ?? undefined,
      this.customerFilter() ?? undefined
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.assets.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        } else {
          this.error.set(response.message || 'Failed to load assets');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load assets');
        this.isLoading.set(false);
        console.error('Error loading assets:', err);
      }
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomerLookup().subscribe({
      next: (response: any) => {
        if (response.isSuccess && response.data) {
          this.customers.set(response.data);
        }
      },
      error: (err: any) => console.error('Error loading customers:', err)
    });
  }

  loadStatistics(): void {
    this.assetService.getStatistics().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.statistics.set(response.data);
        }
      },
      error: (err) => console.error('Error loading statistics:', err)
    });
  }

  // Search
  onSearch(): void {
    this.currentPage.set(1);
    this.loadAssets();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set(null);
    this.typeFilter.set(null);
    this.customerFilter.set(null);
    this.currentPage.set(1);
    this.loadAssets();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadAssets();
    }
  }

  // Modal actions
  openAddModal(): void {
    this.isEditMode.set(false);
    this.selectedAsset.set(null);
    this.assetForm.reset({
      type: 1,
      status: 2,
      condition: 1,
      stockCategoryId: '',
      ownershipType: 1,
      assignmentPurpose: 1
    });
    this.showModal.set(true);
  }

  openEditModal(asset: AssetSummary): void {
    this.isEditMode.set(true);
    this.isLoading.set(true);

    this.assetService.getAsset(asset.id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.selectedAsset.set(response.data);
          this.assetForm.patchValue({
            customerId: response.data.customerId,
            locationId: response.data.locationId || '',
            productId: response.data.productId || '',
            name: response.data.name,
            description: response.data.description || '',
            type: response.data.type,
            serialNumber: response.data.serialNumber || '',
            manufacturer: response.data.manufacturer || '',
            model: response.data.model || '',
            status: response.data.status,
            condition: response.data.condition,
            stockCategoryId: response.data.stockCategoryId || '',
            ownershipType: response.data.ownershipType,
            purchaseDate: response.data.purchaseDate ? new Date(response.data.purchaseDate).toISOString().split('T')[0] : '',
            purchasePrice: response.data.purchasePrice || '',
            warrantyStartDate: response.data.warrantyStartDate ? new Date(response.data.warrantyStartDate).toISOString().split('T')[0] : '',
            warrantyEndDate: response.data.warrantyEndDate ? new Date(response.data.warrantyEndDate).toISOString().split('T')[0] : '',
            notes: response.data.notes || '',
            assignedUserId: response.data.assignedUserId || '',
            assignmentPurpose: response.data.assignmentPurpose || 1,
            assignmentDate: response.data.assignmentDate ? new Date(response.data.assignmentDate).toISOString().split('T')[0] : '',
            expectedReturnDate: response.data.expectedReturnDate ? new Date(response.data.expectedReturnDate).toISOString().split('T')[0] : '',
            assignmentNotes: response.data.assignmentNotes || ''
          });
          this.showModal.set(true);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load asset details');
        this.isLoading.set(false);
      }
    });
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedAsset.set(null);
  }

  saveAsset(): void {
    if (this.assetForm.invalid) {
      this.assetForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.assetForm.value;

    const request: CreateAssetRequest = {
      customerId: formValue.customerId,
      locationId: formValue.locationId || undefined,
      productId: formValue.productId || undefined,
      name: formValue.name,
      description: formValue.description || undefined,
      type: formValue.type,
      serialNumber: formValue.serialNumber || undefined,
      manufacturer: formValue.manufacturer || undefined,
      model: formValue.model || undefined,
      status: formValue.status,
      condition: formValue.condition,
      stockCategoryId: formValue.stockCategoryId || undefined,
      ownershipType: formValue.ownershipType,
      purchaseDate: formValue.purchaseDate || undefined,
      purchasePrice: formValue.purchasePrice ? parseFloat(formValue.purchasePrice) : undefined,
      warrantyStartDate: formValue.warrantyStartDate || undefined,
      warrantyEndDate: formValue.warrantyEndDate || undefined,
      notes: formValue.notes || undefined,
      assignedUserId: formValue.assignedUserId || undefined,
      assignmentPurpose: formValue.assignmentPurpose,
      assignmentDate: formValue.assignmentDate || undefined,
      expectedReturnDate: formValue.expectedReturnDate || undefined,
      assignmentNotes: formValue.assignmentNotes || undefined
    };

    const operation = this.isEditMode()
      ? this.assetService.updateAsset(this.selectedAsset()!.id, request)
      : this.assetService.createAsset(request);

    operation.subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set(this.isEditMode() ? 'Asset updated successfully' : 'Asset created successfully');
          this.closeModal();
          this.loadAssets();
          this.loadStatistics();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to save asset');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to save asset');
        this.isLoading.set(false);
      }
    });
  }

  deleteAsset(asset: AssetSummary): void {
    if (!confirm(`Are you sure you want to delete asset "${asset.name}" (${asset.assetTag})?`)) {
      return;
    }

    this.isLoading.set(true);
    this.assetService.deleteAsset(asset.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Asset deleted successfully');
          this.loadAssets();
          this.loadStatistics();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to delete asset');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to delete asset');
        this.isLoading.set(false);
      }
    });
  }

  // Assignment
  openAssignModal(asset: AssetSummary): void {
    this.assetService.getAsset(asset.id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.selectedAsset.set(response.data);
          this.assignForm.reset({
            assignmentPurpose: 1,
            assignmentDate: new Date().toISOString().split('T')[0]
          });
          this.showAssignModal.set(true);
        }
      }
    });
  }

  closeAssignModal(): void {
    this.showAssignModal.set(false);
    this.selectedAsset.set(null);
  }

  saveAssignment(): void {
    if (this.assignForm.invalid || !this.selectedAsset()) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.assignForm.value;

    this.assetService.assignAsset(this.selectedAsset()!.id, {
      assignedUserId: formValue.assignedUserId || undefined,
      customerId: formValue.customerId || undefined,
      departmentId: formValue.departmentId || undefined,
      assignmentPurpose: formValue.assignmentPurpose,
      assignmentDate: formValue.assignmentDate || undefined,
      expectedReturnDate: formValue.expectedReturnDate || undefined,
      assignmentNotes: formValue.assignmentNotes || undefined
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Asset assigned successfully');
          this.closeAssignModal();
          this.loadAssets();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to assign asset');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to assign asset');
        this.isLoading.set(false);
      }
    });
  }

  // Transfer
  openTransferModal(asset: AssetSummary): void {
    this.assetService.getAsset(asset.id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.selectedAsset.set(response.data);
          this.transferForm.reset({
            transferDate: new Date().toISOString().split('T')[0]
          });
          this.showTransferModal.set(true);
        }
      }
    });
  }

  closeTransferModal(): void {
    this.showTransferModal.set(false);
    this.selectedAsset.set(null);
  }

  saveTransfer(): void {
    if (this.transferForm.invalid || !this.selectedAsset()) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.transferForm.value;

    this.assetService.transferAsset(this.selectedAsset()!.id, {
      newCustomerId: formValue.newCustomerId,
      newLocationId: formValue.newLocationId || undefined,
      transferDate: new Date(formValue.transferDate),
      transferReason: formValue.transferReason || undefined,
      notes: formValue.notes || undefined
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage.set('Asset transferred successfully');
          this.closeTransferModal();
          this.loadAssets();
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.error.set(response.message || 'Failed to transfer asset');
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to transfer asset');
        this.isLoading.set(false);
      }
    });
  }

  // Helpers
  getStatusClass(status: number): string {
    const statusClasses: { [key: number]: string } = {
      2: 'status-in-stock',
      3: 'status-deployed',
      4: 'status-in-service',
      5: 'status-under-repair',
      6: 'status-out-of-service',
      7: 'status-retired',
      11: 'status-on-loan'
    };
    return statusClasses[status] || 'status-default';
  }

  getConditionClass(condition: number): string {
    if (condition <= 2) return 'condition-excellent';
    if (condition <= 4) return 'condition-good';
    return 'condition-poor';
  }

  formatCurrency(value: number | undefined, currency: string = 'INR'): string {
    if (value === undefined || value === null) return '-';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: currency
    }).format(value);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
