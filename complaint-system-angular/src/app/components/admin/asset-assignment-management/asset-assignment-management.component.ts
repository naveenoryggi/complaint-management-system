import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AssetAssignmentService } from '../../../services/asset-assignment.service';
import { AssetService, AssetLookup } from '../../../services/asset.service';
import { UserService, UserLookup } from '../../../services/user.service';
import { DepartmentService, DepartmentLookup } from '../../../services/department.service';
import { CustomerService } from '../../../services/customer.service';
import { CustomerSummary } from '../../../models/customer.model';
import {
  AssetAssignment,
  AssetAssignmentSummary,
  AssignAssetRequest,
  ReturnAssetRequest,
  TransferAssetRequest,
  ExtendAssignmentRequest,
  AssetAssignmentPurpose,
  AssetCondition,
  AssetAssignmentPurposeLabels,
  AssetConditionLabels,
  getPurposeColorClass,
  getConditionColorClass
} from '../../../models/asset-assignment.model';

@Component({
  selector: 'app-asset-assignment-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './asset-assignment-management.component.html',
  styleUrls: ['./asset-assignment-management.component.scss']
})
export class AssetAssignmentManagementComponent implements OnInit {
  // Data
  assignments = signal<AssetAssignmentSummary[]>([]);
  selectedAssignment = signal<AssetAssignment | null>(null);
  assets = signal<AssetLookup[]>([]);
  users = signal<UserLookup[]>([]);
  customers = signal<CustomerSummary[]>([]);
  departments = signal<DepartmentLookup[]>([]);

  // Assignment type toggle
  isExternalAssignment = signal(false);

  // UI State
  isLoading = signal(false);
  showAssignModal = signal(false);
  showReturnModal = signal(false);
  showTransferModal = signal(false);
  showExtendModal = signal(false);
  showDetailsModal = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Filters
  searchTerm = signal('');
  activeFilter = signal<boolean | null>(true);
  purposeFilter = signal<AssetAssignmentPurpose | null>(null);
  overdueFilter = signal(false);
  externalAssignmentFilter = signal<boolean | null>(null);

  // Forms
  assignForm: FormGroup;
  returnForm: FormGroup;
  transferForm: FormGroup;
  extendForm: FormGroup;

  // Enum options
  purposeOptions = Object.entries(AssetAssignmentPurposeLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  conditionOptions = Object.entries(AssetConditionLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  // Helpers
  getPurposeColorClass = getPurposeColorClass;
  getConditionColorClass = getConditionColorClass;

  // Filtered assignments
  filteredAssignments = computed(() => {
    let result = this.assignments();

    // Search filter
    const search = this.searchTerm().toLowerCase();
    if (search) {
      result = result.filter(a =>
        a.assetName.toLowerCase().includes(search) ||
        a.assetTag.toLowerCase().includes(search) ||
        (a.assigneeName?.toLowerCase().includes(search)) ||
        (a.assignedToUserName?.toLowerCase().includes(search)) ||
        (a.assignedToCustomerName?.toLowerCase().includes(search)) ||
        (a.assignedToEmployeeCode?.toLowerCase().includes(search)) ||
        (a.assignedToCustomerCode?.toLowerCase().includes(search)) ||
        (a.responsibleUserName?.toLowerCase().includes(search)) ||
        (a.assignmentNumber?.toLowerCase().includes(search))
      );
    }

    // Active filter
    if (this.activeFilter() !== null) {
      result = result.filter(a => a.isActive === this.activeFilter());
    }

    // Purpose filter
    if (this.purposeFilter() !== null) {
      result = result.filter(a => a.purpose === this.purposeFilter());
    }

    // External assignment filter
    if (this.externalAssignmentFilter() !== null) {
      result = result.filter(a => a.isExternalAssignment === this.externalAssignmentFilter());
    }

    // Overdue filter
    if (this.overdueFilter()) {
      result = result.filter(a => a.isOverdue);
    }

    return result;
  });

  constructor(
    private assignmentService: AssetAssignmentService,
    private assetService: AssetService,
    private userService: UserService,
    private customerService: CustomerService,
    private departmentService: DepartmentService,
    private fb: FormBuilder
  ) {
    this.assignForm = this.fb.group({
      assetId: ['', Validators.required],
      isExternalAssignment: [false],
      // For internal assignments
      assignedToUserId: [''],
      // For external assignments (customer/SI)
      assignedToCustomerId: [''],
      responsibleUserId: [''],
      purpose: [AssetAssignmentPurpose.Permanent, Validators.required],
      assignmentDate: [this.formatDate(new Date())],
      expectedReturnDate: [''],
      conditionAtAssignment: [AssetCondition.Good],
      conditionNotesAtAssignment: [''],
      location: [''],
      departmentId: [''],
      costCenter: [''],
      notes: ['']
    });

    this.returnForm = this.fb.group({
      conditionAtReturn: [AssetCondition.Good, Validators.required],
      conditionNotesAtReturn: [''],
      returnDate: [this.formatDate(new Date())],
      notes: ['']
    });

    this.transferForm = this.fb.group({
      newAssignedToUserId: ['', Validators.required],
      purpose: [AssetAssignmentPurpose.Permanent],
      transferDate: [this.formatDate(new Date())],
      expectedReturnDate: [''],
      conditionAtTransfer: [AssetCondition.Good],
      conditionNotes: [''],
      location: [''],
      departmentId: [''],
      costCenter: [''],
      notes: ['']
    });

    this.extendForm = this.fb.group({
      newExpectedReturnDate: ['', Validators.required],
      reason: ['']
    });
  }

  ngOnInit(): void {
    this.loadAssignments();
    this.loadAssets();
    this.loadUsers();
    this.loadCustomers();
    this.loadDepartments();
  }

  // Data loading methods
  loadAssignments(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.assignmentService.getAssignments({
      isActive: this.activeFilter() ?? undefined,
      purpose: this.purposeFilter() ?? undefined
    }).subscribe({
      next: (data) => {
        this.assignments.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load assignments');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  loadAssets(): void {
    this.assetService.getLookups(undefined, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.assets.set(response.data);
        }
      },
      error: (err) => console.error('Failed to load assets', err)
    });
  }

  loadUsers(): void {
    this.userService.getUserLookups().subscribe({
      next: (data) => {
        this.users.set(data);
      },
      error: (err) => console.error('Failed to load users', err)
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomers({ pageSize: 500 }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.customers.set(response.data.items);
        }
      },
      error: (err) => console.error('Failed to load customers', err)
    });
  }

  loadDepartments(): void {
    this.departmentService.getDepartmentLookups().subscribe({
      next: (data) => {
        this.departments.set(data);
      },
      error: (err) => console.error('Failed to load departments', err)
    });
  }

  // Modal operations
  openAssignModal(): void {
    this.assignForm.reset({
      isExternalAssignment: false,
      purpose: AssetAssignmentPurpose.Permanent,
      assignmentDate: this.formatDate(new Date()),
      conditionAtAssignment: AssetCondition.Good
    });
    this.isExternalAssignment.set(false);
    this.showAssignModal.set(true);
  }

  // Toggle between internal and external assignment
  toggleAssignmentType(): void {
    const isExternal = this.assignForm.get('isExternalAssignment')?.value;
    this.isExternalAssignment.set(isExternal);

    // Clear the opposite field when toggling
    if (isExternal) {
      this.assignForm.patchValue({ assignedToUserId: '' });
    } else {
      this.assignForm.patchValue({ assignedToCustomerId: '', responsibleUserId: '' });
    }
  }

  openReturnModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.returnForm.reset({
      conditionAtReturn: AssetCondition.Good,
      returnDate: this.formatDate(new Date())
    });
    this.showReturnModal.set(true);
  }

  openTransferModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.transferForm.reset({
      purpose: AssetAssignmentPurpose.Permanent,
      transferDate: this.formatDate(new Date()),
      conditionAtTransfer: AssetCondition.Good
    });
    this.showTransferModal.set(true);
  }

  openExtendModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.extendForm.reset();
    this.showExtendModal.set(true);
  }

  openDetailsModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.showDetailsModal.set(true);
  }

  closeModals(): void {
    this.showAssignModal.set(false);
    this.showReturnModal.set(false);
    this.showTransferModal.set(false);
    this.showExtendModal.set(false);
    this.showDetailsModal.set(false);
    this.selectedAssignment.set(null);
  }

  loadAssignmentDetails(id: string): void {
    this.assignmentService.getById(id).subscribe({
      next: (data) => {
        this.selectedAssignment.set(data);
      },
      error: (err) => console.error('Failed to load assignment details', err)
    });
  }

  // Form submissions
  submitAssignment(): void {
    const formValue = this.assignForm.value;
    const isExternal = formValue.isExternalAssignment;

    // Validate based on assignment type
    if (!formValue.assetId) {
      this.error.set('Please select an asset');
      return;
    }

    if (isExternal) {
      if (!formValue.assignedToCustomerId) {
        this.error.set('Please select a customer/SI');
        return;
      }
      if (!formValue.responsibleUserId) {
        this.error.set('Please select a responsible user for tracking');
        return;
      }
    } else {
      if (!formValue.assignedToUserId) {
        this.error.set('Please select an employee');
        return;
      }
    }

    this.isLoading.set(true);
    const request: AssignAssetRequest = {
      assetId: formValue.assetId,
      assignedToUserId: isExternal ? undefined : formValue.assignedToUserId || undefined,
      assignedToCustomerId: isExternal ? formValue.assignedToCustomerId || undefined : undefined,
      responsibleUserId: isExternal ? formValue.responsibleUserId || undefined : undefined,
      purpose: formValue.purpose,
      assignmentDate: formValue.assignmentDate
        ? new Date(formValue.assignmentDate)
        : undefined,
      expectedReturnDate: formValue.expectedReturnDate
        ? new Date(formValue.expectedReturnDate)
        : undefined,
      conditionAtAssignment: formValue.conditionAtAssignment,
      conditionNotesAtAssignment: formValue.conditionNotesAtAssignment,
      location: formValue.location,
      departmentId: formValue.departmentId || undefined,
      costCenter: formValue.costCenter,
      notes: formValue.notes
    };

    this.assignmentService.assignAsset(request).subscribe({
      next: () => {
        this.showSuccess('Asset assigned successfully');
        this.closeModals();
        this.loadAssignments();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to assign asset');
        this.isLoading.set(false);
      }
    });
  }

  submitReturn(): void {
    if (this.returnForm.invalid || !this.selectedAssignment()) return;

    this.isLoading.set(true);
    const request: ReturnAssetRequest = {
      ...this.returnForm.value,
      returnDate: this.returnForm.value.returnDate
        ? new Date(this.returnForm.value.returnDate)
        : undefined
    };

    this.assignmentService.returnAsset(this.selectedAssignment()!.id, request).subscribe({
      next: () => {
        this.showSuccess('Asset returned successfully');
        this.closeModals();
        this.loadAssignments();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to return asset');
        this.isLoading.set(false);
      }
    });
  }

  submitTransfer(): void {
    if (this.transferForm.invalid || !this.selectedAssignment()) return;

    this.isLoading.set(true);
    const request: TransferAssetRequest = {
      ...this.transferForm.value,
      transferDate: this.transferForm.value.transferDate
        ? new Date(this.transferForm.value.transferDate)
        : undefined,
      expectedReturnDate: this.transferForm.value.expectedReturnDate
        ? new Date(this.transferForm.value.expectedReturnDate)
        : undefined
    };

    this.assignmentService.transferAsset(this.selectedAssignment()!.id, request).subscribe({
      next: () => {
        this.showSuccess('Asset transferred successfully');
        this.closeModals();
        this.loadAssignments();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to transfer asset');
        this.isLoading.set(false);
      }
    });
  }

  submitExtend(): void {
    if (this.extendForm.invalid || !this.selectedAssignment()) return;

    this.isLoading.set(true);
    const request: ExtendAssignmentRequest = {
      newExpectedReturnDate: new Date(this.extendForm.value.newExpectedReturnDate),
      reason: this.extendForm.value.reason
    };

    this.assignmentService.extendAssignment(this.selectedAssignment()!.id, request).subscribe({
      next: () => {
        this.showSuccess('Assignment extended successfully');
        this.closeModals();
        this.loadAssignments();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to extend assignment');
        this.isLoading.set(false);
      }
    });
  }

  acknowledgeAssignment(assignment: AssetAssignmentSummary): void {
    if (!confirm('Acknowledge receipt of this asset?')) return;

    this.isLoading.set(true);
    this.assignmentService.acknowledgeAsset(assignment.id, {}).subscribe({
      next: () => {
        this.showSuccess('Asset acknowledged');
        this.loadAssignments();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to acknowledge asset');
        this.isLoading.set(false);
      }
    });
  }

  // Filter methods
  setActiveFilter(value: boolean | null): void {
    this.activeFilter.set(value);
    this.loadAssignments();
  }

  setPurposeFilter(value: AssetAssignmentPurpose | null): void {
    this.purposeFilter.set(value);
    this.loadAssignments();
  }

  toggleOverdueFilter(): void {
    this.overdueFilter.set(!this.overdueFilter());
  }

  setExternalAssignmentFilter(value: boolean | null): void {
    this.externalAssignmentFilter.set(value);
    this.loadAssignments();
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.activeFilter.set(true);
    this.purposeFilter.set(null);
    this.externalAssignmentFilter.set(null);
    this.overdueFilter.set(false);
    this.loadAssignments();
  }

  // Utility methods
  showSuccess(message: string): void {
    this.successMessage.set(message);
    this.isLoading.set(false);
    setTimeout(() => this.successMessage.set(null), 3000);
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  formatDateTime(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  getDaysUntilDue(expectedReturnDate?: Date): number | null {
    if (!expectedReturnDate) return null;
    const now = new Date();
    const due = new Date(expectedReturnDate);
    const diff = Math.ceil((due.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    return diff;
  }

  getStatusBadgeClass(assignment: AssetAssignmentSummary): string {
    if (!assignment.isActive) {
      return 'bg-gray-100 text-gray-800';
    }
    if (assignment.isOverdue) {
      return 'bg-red-100 text-red-800';
    }
    if (!assignment.isAcknowledged) {
      return 'bg-yellow-100 text-yellow-800';
    }
    return 'bg-green-100 text-green-800';
  }

  getStatusText(assignment: AssetAssignmentSummary): string {
    if (!assignment.isActive) {
      return 'Returned';
    }
    if (assignment.isOverdue) {
      return 'Overdue';
    }
    if (!assignment.isAcknowledged) {
      return 'Pending Ack';
    }
    return 'Active';
  }
}
