import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContractService } from '../../../services/contract.service';
import { CustomerService } from '../../../services/customer.service';
import { InlineSelectComponent, SelectOption } from '../../shared/inline-select/inline-select.component';
import {
  Contract,
  ContractSummary,
  CreateContractRequest,
  UpdateContractRequest,
  ContractType,
  ContractTypeLabels,
  ContractStatus,
  ContractStatusLabels,
  CoverageType,
  CoverageTypeLabels,
  BillingFrequency,
  BillingFrequencyLabels,
  SupportHoursType,
  SupportHoursTypeLabels
} from '../../../models/contract.model';

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule, FormsModule, InlineSelectComponent],
  templateUrl: './contract-management.component.html',
  styleUrls: ['./contract-management.component.scss']
})
export class ContractManagementComponent implements OnInit {
  contracts: ContractSummary[] = [];
  selectedContract: Contract | null = null;
  customers: { id: string; name: string }[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Filters
  searchTerm = '';
  filterType: ContractType | '' = '';
  filterStatus: ContractStatus | '' = '';
  filterCoverage: CoverageType | '' = '';
  filterExpiring = false;

  // State
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';

  // Modal
  showModal = false;
  isEditMode = false;
  contractForm: CreateContractRequest = this.getEmptyForm();
  activeTab = 'basic';

  // Enums for templates
  contractTypes = Object.entries(ContractTypeLabels).map(([key, value]) => ({ key: +key, value }));
  contractStatuses = Object.entries(ContractStatusLabels).map(([key, value]) => ({ key: +key, value }));
  coverageTypes = Object.entries(CoverageTypeLabels).map(([key, value]) => ({ key: +key, value }));
  billingFrequencies = Object.entries(BillingFrequencyLabels).map(([key, value]) => ({ key: +key, value }));
  supportHoursTypes = Object.entries(SupportHoursTypeLabels).map(([key, value]) => ({ key: +key, value }));

  ContractTypeLabels = ContractTypeLabels;
  ContractStatusLabels = ContractStatusLabels;
  CoverageTypeLabels = CoverageTypeLabels;

  // SelectOption arrays for inline-select component
  contractTypeOptions: SelectOption[] = [];
  coverageTypeOptions: SelectOption[] = [];
  billingFrequencyOptions: SelectOption[] = [];
  supportHoursOptions: SelectOption[] = [];
  customerOptions: SelectOption[] = [];

  // Custom options added by user
  customContractTypes: SelectOption[] = [];
  customCoverageTypes: SelectOption[] = [];

  constructor(
    private contractService: ContractService,
    private customerService: CustomerService
  ) {}

  ngOnInit(): void {
    this.initSelectOptions();
    this.loadContracts();
    this.loadCustomers();
  }

  initSelectOptions(): void {
    // Initialize contract type options from enum
    this.contractTypeOptions = this.contractTypes.map(t => ({
      value: t.key,
      label: t.value
    }));

    // Initialize coverage type options from enum
    this.coverageTypeOptions = this.coverageTypes.map(c => ({
      value: c.key,
      label: c.value
    }));

    // Initialize billing frequency options from enum
    this.billingFrequencyOptions = [
      { value: undefined, label: 'Select' },
      ...this.billingFrequencies.map(b => ({
        value: b.key,
        label: b.value
      }))
    ];

    // Initialize support hours options from enum
    this.supportHoursOptions = [
      { value: undefined, label: 'Select' },
      ...this.supportHoursTypes.map(s => ({
        value: s.key,
        label: s.value
      }))
    ];

    // Load any custom types from localStorage
    this.loadCustomOptions();
  }

  loadCustomOptions(): void {
    const customTypes = localStorage.getItem('custom_contract_types');
    if (customTypes) {
      this.customContractTypes = JSON.parse(customTypes);
      this.contractTypeOptions = [
        ...this.contractTypes.map(t => ({ value: t.key, label: t.value })),
        ...this.customContractTypes
      ];
    }

    const customCoverage = localStorage.getItem('custom_contract_coverage_types');
    if (customCoverage) {
      this.customCoverageTypes = JSON.parse(customCoverage);
      this.coverageTypeOptions = [
        ...this.coverageTypes.map(c => ({ value: c.key, label: c.value })),
        ...this.customCoverageTypes
      ];
    }
  }

  saveCustomOptions(): void {
    localStorage.setItem('custom_contract_types', JSON.stringify(this.customContractTypes));
    localStorage.setItem('custom_contract_coverage_types', JSON.stringify(this.customCoverageTypes));
  }

  loadContracts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.contractService.getContracts({
      search: this.searchTerm || undefined,
      type: this.filterType !== '' ? this.filterType : undefined,
      status: this.filterStatus !== '' ? this.filterStatus : undefined,
      coverageType: this.filterCoverage !== '' ? this.filterCoverage : undefined,
      expiringWithinDays: this.filterExpiring ? 30 : undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.contracts = response.data.items || [];
          this.totalItems = response.data.totalCount || 0;
          this.totalPages = response.data.totalPages || 0;
        } else {
          this.contracts = [];
          this.errorMessage = response.message || 'Failed to load contracts';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading contracts';
        this.contracts = [];
        this.loading = false;
      }
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomerLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.customers = response.data.map(c => ({ id: c.id, name: c.name }));
          this.customerOptions = [
            { value: '', label: 'Select Customer' },
            ...this.customers.map(c => ({ value: c.id, label: c.name }))
          ];
        }
      },
      error: () => {}
    });
  }

  // Inline option handlers
  onContractTypeAdded(label: string): void {
    const newOption: SelectOption = {
      value: 'custom_' + Date.now(),
      label: label,
      isCustom: true
    };
    this.customContractTypes.push(newOption);
    this.contractTypeOptions = [
      ...this.contractTypes.map(t => ({ value: t.key, label: t.value })),
      ...this.customContractTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Contract type "${label}" added`;
    setTimeout(() => this.dismissSuccess(), 3000);
  }

  onContractTypeDeleted(option: SelectOption): void {
    this.customContractTypes = this.customContractTypes.filter(t => t.value !== option.value);
    this.contractTypeOptions = [
      ...this.contractTypes.map(t => ({ value: t.key, label: t.value })),
      ...this.customContractTypes
    ];
    this.saveCustomOptions();
    this.successMessage = `Contract type "${option.label}" deleted`;
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

  onCustomerAdded(name: string): void {
    // Call customer service to create customer
    this.customerService.createCustomer({
      code: 'CUST-' + Date.now(),
      name: name,
      type: 0, // Corporate
      primaryEmail: `${name.toLowerCase().replace(/\s+/g, '.')}@example.com`
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadCustomers();
          this.successMessage = `Customer "${name}" created`;
          setTimeout(() => this.dismissSuccess(), 3000);
        } else {
          this.errorMessage = response.message || 'Failed to create customer';
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create customer';
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadContracts();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadContracts();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.filterType = '';
    this.filterStatus = '';
    this.filterCoverage = '';
    this.filterExpiring = false;
    this.currentPage = 1;
    this.loadContracts();
  }

  selectContract(contract: ContractSummary): void {
    this.detailLoading = true;
    this.contractService.getContractById(contract.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedContract = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  closeDetail(): void {
    this.selectedContract = null;
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadContracts();
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
    this.contractForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.activeTab = 'basic';
    this.errorMessage = '';
  }

  openEditModal(contract: ContractSummary): void {
    this.detailLoading = true;
    this.contractService.getContractById(contract.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const c = response.data;
          this.contractForm = {
            customerId: c.customerId,
            partnerId: c.partnerId,
            name: c.name,
            type: c.type,
            description: c.description,
            externalContractId: c.externalContractId,
            poNumber: c.poNumber,
            invoiceNumber: c.invoiceNumber,
            startDate: c.startDate?.split('T')[0] || '',
            endDate: c.endDate?.split('T')[0] || '',
            signedDate: c.signedDate?.split('T')[0],
            autoRenew: c.autoRenew,
            renewalPeriodMonths: c.renewalPeriodMonths,
            renewalNoticeDays: c.renewalNoticeDays,
            contractValue: c.contractValue,
            annualValue: c.annualValue,
            monthlyValue: c.monthlyValue,
            currency: c.currency,
            billingFrequency: c.billingFrequency,
            paymentTerms: c.paymentTerms,
            coverageType: c.coverageType,
            includesOnsite: c.includesOnsite,
            includesRemote: c.includesRemote,
            includesParts: c.includesParts,
            includesLabor: c.includesLabor,
            includedHours: c.includedHours,
            includedIncidents: c.includedIncidents,
            slaSettingsId: c.slaSettingsId,
            responseTimeHours: c.responseTimeHours,
            resolutionTimeHours: c.resolutionTimeHours,
            supportHoursType: c.supportHoursType
          };
          this.selectedContract = c;
          this.isEditMode = true;
          this.showModal = true;
          this.activeTab = 'basic';
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load contract details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.contractForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  saveContract(): void {
    if (!this.contractForm.customerId) {
      this.errorMessage = 'Customer is required';
      return;
    }
    if (!this.contractForm.name?.trim()) {
      this.errorMessage = 'Contract name is required';
      return;
    }
    if (!this.contractForm.startDate) {
      this.errorMessage = 'Start date is required';
      return;
    }
    if (!this.contractForm.endDate) {
      this.errorMessage = 'End date is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedContract) {
      const updateRequest: UpdateContractRequest = {
        name: this.contractForm.name,
        description: this.contractForm.description,
        externalContractId: this.contractForm.externalContractId,
        poNumber: this.contractForm.poNumber,
        invoiceNumber: this.contractForm.invoiceNumber,
        startDate: this.contractForm.startDate,
        endDate: this.contractForm.endDate,
        signedDate: this.contractForm.signedDate,
        autoRenew: this.contractForm.autoRenew,
        renewalPeriodMonths: this.contractForm.renewalPeriodMonths,
        renewalNoticeDays: this.contractForm.renewalNoticeDays,
        contractValue: this.contractForm.contractValue,
        annualValue: this.contractForm.annualValue,
        monthlyValue: this.contractForm.monthlyValue,
        billingFrequency: this.contractForm.billingFrequency,
        paymentTerms: this.contractForm.paymentTerms,
        coverageType: this.contractForm.coverageType,
        includesOnsite: this.contractForm.includesOnsite,
        includesRemote: this.contractForm.includesRemote,
        includesParts: this.contractForm.includesParts,
        includesLabor: this.contractForm.includesLabor,
        includedHours: this.contractForm.includedHours,
        includedIncidents: this.contractForm.includedIncidents,
        slaSettingsId: this.contractForm.slaSettingsId,
        responseTimeHours: this.contractForm.responseTimeHours,
        resolutionTimeHours: this.contractForm.resolutionTimeHours,
        supportHoursType: this.contractForm.supportHoursType
      };

      this.contractService.updateContract(this.selectedContract.id, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Contract updated successfully';
            this.closeModal();
            this.loadContracts();
          } else {
            this.errorMessage = response.message || 'Failed to update contract';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.contractService.createContract(this.contractForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Contract created successfully';
            this.closeModal();
            this.loadContracts();
          } else {
            this.errorMessage = response.message || 'Failed to create contract';
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

  deleteContract(contract: ContractSummary): void {
    if (contract.status !== ContractStatus.Draft) {
      this.errorMessage = 'Only draft contracts can be deleted';
      return;
    }

    if (confirm(`Are you sure you want to delete contract "${contract.name}"?`)) {
      this.loading = true;
      this.contractService.deleteContract(contract.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Contract deleted successfully';
            this.loadContracts();
            if (this.selectedContract?.id === contract.id) {
              this.selectedContract = null;
            }
          } else {
            this.errorMessage = response.message || 'Failed to delete contract';
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

  activateContract(contract: Contract): void {
    if (confirm(`Activate contract "${contract.name}"?`)) {
      this.loading = true;
      this.contractService.activateContract(contract.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Contract activated successfully';
            this.loadContracts();
            this.selectedContract = response.data;
          } else {
            this.errorMessage = response.message || 'Failed to activate contract';
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

  terminateContract(contract: Contract): void {
    const reason = prompt('Enter termination reason:');
    if (reason) {
      this.loading = true;
      this.contractService.terminateContract(contract.id, {
        terminatedDate: new Date().toISOString().split('T')[0],
        reason
      }).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Contract terminated successfully';
            this.loadContracts();
            this.selectedContract = response.data;
          } else {
            this.errorMessage = response.message || 'Failed to terminate contract';
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

  private getEmptyForm(): CreateContractRequest {
    return {
      customerId: '',
      name: '',
      type: ContractType.AMC,
      startDate: '',
      endDate: '',
      coverageType: CoverageType.FullCoverage,
      autoRenew: false,
      includesOnsite: true,
      includesRemote: true,
      includesParts: true,
      includesLabor: true,
      currency: 'INR'
    };
  }

  getStatusClass(status: ContractStatus): string {
    switch (status) {
      case ContractStatus.Active: return 'status-active';
      case ContractStatus.Draft: return 'status-draft';
      case ContractStatus.Pending: return 'status-pending';
      case ContractStatus.Expired: return 'status-expired';
      case ContractStatus.Terminated: return 'status-terminated';
      case ContractStatus.Suspended: return 'status-suspended';
      case ContractStatus.PendingRenewal: return 'status-renewal';
      default: return 'status-default';
    }
  }

  getTypeClass(type: ContractType): string {
    switch (type) {
      case ContractType.AMC:
      case ContractType.ComprehensiveAMC: return 'type-amc';
      case ContractType.Warranty:
      case ContractType.ExtendedWarranty: return 'type-warranty';
      case ContractType.SLA: return 'type-sla';
      case ContractType.Subscription: return 'type-subscription';
      default: return 'type-default';
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

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }
}
