import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { CustomerService } from '../../../services/customer.service';
import {
  Customer,
  CustomerSummary,
  CreateCustomerRequest,
  CustomerType,
  CustomerStatus,
  CustomerSegment,
  CustomerTypeLabels,
  CustomerStatusLabels,
  CustomerSegmentLabels
} from '../../../models/customer.model';

@Component({
  selector: 'app-customer-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-management.component.html',
  styleUrls: ['./customer-management.component.scss']
})
export class CustomerManagementComponent implements OnInit, OnDestroy {
  customers: CustomerSummary[] = [];
  selectedCustomer: Customer | null = null;
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
  totalCustomers = 0;
  totalPages = 1;

  // Filter
  filterStatus: number | null = null;
  filterType: number | null = null;
  filterSegment: number | null = null;

  // Modal state
  showModal = false;
  isEditMode = false;

  // Form data
  customerForm: CreateCustomerRequest = this.getEmptyForm();

  // Enums for template
  customerTypes = Object.entries(CustomerTypeLabels).map(([key, value]) => ({ value: +key, label: value }));
  customerStatuses = Object.entries(CustomerStatusLabels).map(([key, value]) => ({ value: +key, label: value }));
  customerSegments = Object.entries(CustomerSegmentLabels).map(([key, value]) => ({ value: +key, label: value }));

  constructor(private customerService: CustomerService) {}

  ngOnInit(): void {
    this.loadCustomers();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.currentPage = 1;
      this.loadCustomers();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCustomers(): void {
    this.loading = true;
    this.errorMessage = '';

    this.customerService.getCustomers({
      search: this.searchTerm || undefined,
      status: this.filterStatus ?? undefined,
      type: this.filterType ?? undefined,
      segment: this.filterSegment ?? undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.customers = response.data.items;
          this.totalCustomers = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load customers';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading customers';
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadCustomers();
  }

  clearFilters(): void {
    this.filterStatus = null;
    this.filterType = null;
    this.filterSegment = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadCustomers();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCustomers();
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

  // Customer Detail
  viewCustomer(customer: CustomerSummary): void {
    this.detailLoading = true;
    this.customerService.getCustomerById(customer.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedCustomer = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  closeDetail(): void {
    this.selectedCustomer = null;
  }

  // Modal Operations
  openAddModal(): void {
    this.customerForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(customer: CustomerSummary): void {
    this.detailLoading = true;
    this.customerService.getCustomerById(customer.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.customerForm = {
            code: response.data.code,
            name: response.data.name,
            legalName: response.data.legalName,
            type: response.data.type,
            primaryEmail: response.data.primaryEmail,
            primaryPhone: response.data.primaryPhone,
            website: response.data.website,
            billingAddressLine1: response.data.billingAddressLine1,
            billingAddressLine2: response.data.billingAddressLine2,
            billingCity: response.data.billingCity,
            billingState: response.data.billingState,
            billingCountry: response.data.billingCountry,
            billingPostalCode: response.data.billingPostalCode,
            taxId: response.data.taxId,
            panNumber: response.data.panNumber,
            industry: response.data.industry,
            segment: response.data.segment,
            employeeCount: response.data.employeeCount,
            annualRevenue: response.data.annualRevenue,
            currency: response.data.currency,
            creditTerms: response.data.creditTerms,
            creditLimit: response.data.creditLimit,
            portalEnabled: response.data.portalEnabled,
            notes: response.data.notes,
            tags: response.data.tags
          };
          this.selectedCustomer = response.data;
          this.isEditMode = true;
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load customer details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.customerForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  saveCustomer(): void {
    // Validate required fields
    if (!this.customerForm.code?.trim()) {
      this.errorMessage = 'Customer code is required';
      return;
    }
    if (!this.customerForm.name?.trim()) {
      this.errorMessage = 'Customer name is required';
      return;
    }
    if (!this.customerForm.primaryEmail?.trim()) {
      this.errorMessage = 'Primary email is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedCustomer) {
      this.customerService.updateCustomer(this.selectedCustomer.id, {
        ...this.customerForm,
        status: this.selectedCustomer.status,
        customerRating: this.selectedCustomer.customerRating
      }).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Customer updated successfully';
            this.closeModal();
            this.loadCustomers();
          } else {
            this.errorMessage = response.message || 'Failed to update customer';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.customerService.createCustomer(this.customerForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Customer created successfully';
            this.closeModal();
            this.loadCustomers();
          } else {
            this.errorMessage = response.message || 'Failed to create customer';
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

  deleteCustomer(customer: CustomerSummary): void {
    if (confirm(`Are you sure you want to delete customer "${customer.name}"?`)) {
      this.loading = true;
      this.customerService.deleteCustomer(customer.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Customer deleted successfully';
            this.loadCustomers();
          } else {
            this.errorMessage = response.message || 'Failed to delete customer';
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

  private getEmptyForm(): CreateCustomerRequest {
    return {
      code: '',
      name: '',
      type: CustomerType.Direct,
      primaryEmail: '',
      currency: 'INR',
      portalEnabled: true
    };
  }

  getStatusClass(status: CustomerStatus): string {
    switch (status) {
      case CustomerStatus.Active: return 'status-active';
      case CustomerStatus.Inactive: return 'status-inactive';
      case CustomerStatus.Suspended: return 'status-suspended';
      case CustomerStatus.Churned: return 'status-churned';
      case CustomerStatus.Prospect: return 'status-prospect';
      default: return '';
    }
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }
}
