import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ComplaintService } from '../../../services/complaint.service';
import { DateService } from '../../../services/date.service';
import { MasterDataService, StatusOption, PriorityOption } from '../../../services/master-data.service';
import { SLAService, SLAStatusSummary, UrgencyLevel } from '../../../services/sla.service';
import { Complaint, ComplaintStatus, ComplaintPriority, PreferredContactMethod, PagedResult } from '../../../models/complaint.model';
import { VirtualScrollTableComponent, TableColumn, SortEvent } from '../../../components/shared/virtual-scroll-table/virtual-scroll-table.component';

@Component({
  selector: 'app-complaint-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, VirtualScrollTableComponent],
  templateUrl: './complaint-list.component.html',
  styleUrls: ['./complaint-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ComplaintListComponent implements OnInit, OnDestroy {
  // Expose Math to template
  Math = Math;

  complaints: Complaint[] = [];
  loading = false;
  error: string | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Search term
  searchTerm = '';

  // SLA Status Map
  slaStatusMap = new Map<string, SLAStatusSummary>();
  loadingSLA = false;

  // Memory Leak Prevention
  private destroy$ = new Subject<void>();

  // Virtual Scroll Table Configuration
  // CRITICAL FIX: Pure arrow functions that capture 'this' via closure binding
  // These must be arrow functions assigned in constructor to maintain correct 'this' context
  private formatContactMethod!: (value: any, item: any) => string;
  private formatStatusValue!: (value: ComplaintStatus, item: any) => string;
  private formatPriorityValue!: (value: ComplaintPriority, item: any) => string;
  private formatDateValue!: (value: string, item: any) => string;
  private getStatusCellClass!: (value: ComplaintStatus, item: any) => string;
  private getPriorityCellClass!: (value: ComplaintPriority, item: any) => string;
  private formatSLAValue!: (complaintId: string, item: any) => string;
  private getSLACellClass!: (complaintId: string, item: any) => string;
  private getCodeClass!: (value: any, item: any) => string;
  private getDateClass!: (value: any, item: any) => string;

  tableColumns: TableColumn[] = [
    { key: 'complaintNumber', label: 'Complaint #', width: '120px', sortable: true, class: this.getCodeClass },
    { key: 'title', label: 'Title', sortable: true },
    { key: 'complainantName', label: 'Complainant', width: '150px', sortable: true },
    { key: 'employeeCode', label: 'Emp Code', width: '100px', sortable: true, class: this.getCodeClass },
    { key: 'branchName', label: 'Branch', width: '120px', sortable: true },
    { key: 'departmentName', label: 'Department', width: '120px', sortable: true },
    { key: 'sectionName', label: 'Section', width: '120px', sortable: true },
    { key: 'contactPhone', label: 'Contact Phone', width: '120px', sortable: true },
    {
      key: 'preferredContactMethod',
      label: 'Preferred Contact',
      width: '130px',
      sortable: true,
      format: this.formatContactMethod
    },
    { key: 'categoryName', label: 'Category', width: '120px', sortable: true },
    {
      key: 'status',
      label: 'Status',
      width: '100px',
      sortable: true,
      format: this.formatStatusValue,
      class: this.getStatusCellClass
    },
    {
      key: 'priority',
      label: 'Priority',
      width: '80px',
      sortable: true,
      format: this.formatPriorityValue,
      class: this.getPriorityCellClass
    },
    {
      key: 'id',
      label: 'SLA Status',
      width: '140px',
      sortable: false,
      format: this.formatSLAValue,
      class: this.getSLACellClass
    },
    {
      key: 'createdAt',
      label: 'Created',
      width: '120px',
      sortable: true,
      format: this.formatDateValue,
      class: this.getDateClass
    }
  ];

  // Filters (Changed from enums to Master IDs - GUID strings)
  statusFilter?: string;  // Status Master ID (GUID)
  priorityFilter?: string;  // Priority Master ID (GUID)

  // Enums for template
  ComplaintStatus = ComplaintStatus;
  ComplaintPriority = ComplaintPriority;

  // Dynamic loading arrays for dropdowns
  statusOptions: StatusOption[] = [];
  priorityOptions: PriorityOption[] = [];
  loadingMasterData = false;

  constructor(
    private complaintService: ComplaintService,
    private dateService: DateService,
    private masterDataService: MasterDataService,
    private slaService: SLAService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    // CRITICAL FIX: Initialize formatters in constructor to bind 'this' context
    // This prevents "Cannot read properties of undefined" errors in virtual scroll table

    this.formatContactMethod = (value: any, item: any): string => {
      switch (value) {
        case PreferredContactMethod.Email: return '📧 Email';
        case PreferredContactMethod.Phone: return '📞 Phone';
        case PreferredContactMethod.SMS: return '💬 SMS';
        default: return value || 'N/A';
      }
    };

    this.formatStatusValue = (value: ComplaintStatus, item: any): string => {
      return this.getStatusLabel(value);
    };

    this.formatPriorityValue = (value: ComplaintPriority, item: any): string => {
      return this.getPriorityLabel(value);
    };

    this.formatDateValue = (value: string, item: any): string => {
      return this.formatDate(value);
    };

    this.getStatusCellClass = (value: ComplaintStatus, item: any): string => {
      return `status-badge ${this.getStatusClass(value)}`;
    };

    this.getPriorityCellClass = (value: ComplaintPriority, item: any): string => {
      return `priority-${this.getPriorityClass(value)}`;
    };

    this.formatSLAValue = (complaintId: string, item: any): string => {
      const status = this.slaStatusMap.get(complaintId);
      if (!status) return '-';

      const urgency = status.urgencyLevel;
      const remaining = this.slaService.formatMinutes(status.remainingMinutes);
      const label = this.slaService.getUrgencyLabel(urgency);

      return `${label}: ${remaining}`;
    };

    this.getSLACellClass = (complaintId: string, item: any): string => {
      const status = this.slaStatusMap.get(complaintId);
      if (!status) return 'sla-badge sla-green';

      return `sla-badge sla-${status.urgencyLevel}`;
    };

    this.getCodeClass = (value: any, item: any): string => 'code';
    this.getDateClass = (value: any, item: any): string => 'date';
  }

  ngOnInit(): void {
    this.loadMasterData();
    this.loadComplaints();
  }

  loadMasterData(): void {
    this.loadingMasterData = true;
    this.cdr.markForCheck();

    // Load both status and priority options in parallel
    this.masterDataService.getStatusOptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (statusOptions) => {
          this.statusOptions = statusOptions;
          this.loadingMasterData = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          console.error('Failed to load status options:', error);
          // Fallback to enum values if API fails
          this.statusOptions = this.getFallbackStatusOptions();
          this.loadingMasterData = false;
          this.cdr.markForCheck();
        }
      });

    this.masterDataService.getPriorityOptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (priorityOptions) => {
          this.priorityOptions = priorityOptions;
          this.cdr.markForCheck();
        },
        error: (error) => {
          console.error('Failed to load priority options:', error);
          // Fallback to hardcoded values if API fails
          this.priorityOptions = this.getFallbackPriorityOptions();
          this.cdr.markForCheck();
        }
      });
  }

  private getFallbackStatusOptions(): StatusOption[] {
    // Fallback using placeholder GUIDs (should not be used in production - master data service has its own fallback)
    return [
      { value: '10000000-0000-0000-0000-000000000001', label: 'Submitted', description: 'Complaint has been submitted but not yet reviewed' },
      { value: '10000000-0000-0000-0000-000000000002', label: 'Under Review', description: 'Complaint is being reviewed by the assigned handler' },
      { value: '10000000-0000-0000-0000-000000000003', label: 'In Progress', description: 'Complaint is currently being investigated' },
      { value: '10000000-0000-0000-0000-000000000004', label: 'Escalated', description: 'Complaint has been escalated to a higher level' },
      { value: '10000000-0000-0000-0000-000000000005', label: 'Pending Info', description: 'Complaint is awaiting information from the complainant' },
      { value: '10000000-0000-0000-0000-000000000006', label: 'Resolved', description: 'Complaint has been resolved' },
      { value: '10000000-0000-0000-0000-000000000007', label: 'Closed', description: 'Complaint has been closed (final state)' },
      { value: '10000000-0000-0000-0000-000000000008', label: 'Rejected', description: 'Complaint has been rejected/dismissed' },
      { value: '10000000-0000-0000-0000-000000000009', label: 'Reopened', description: 'Complaint has been reopened after closure' }
    ];
  }

  private getFallbackPriorityOptions(): PriorityOption[] {
    // Fallback using placeholder GUIDs (should not be used in production - master data service has its own fallback)
    return [
      { value: '20000000-0000-0000-0000-000000000001', label: 'Low', description: 'Low priority' },
      { value: '20000000-0000-0000-0000-000000000002', label: 'Normal', description: 'Normal priority' },
      { value: '20000000-0000-0000-0000-000000000003', label: 'High', description: 'High priority' },
      { value: '20000000-0000-0000-0000-000000000004', label: 'Critical', description: 'Critical priority' },
      { value: '20000000-0000-0000-0000-000000000005', label: 'Urgent', description: 'Urgent priority' }
    ];
  }

  loadComplaints(): void {
    this.loading = true;
    this.error = null;
    this.cdr.markForCheck();

    this.complaintService.getComplaints(
      this.currentPage,
      this.pageSize,
      this.statusFilter,
      this.priorityFilter,
      this.searchTerm
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaints = response.data.items;
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;

          // Load SLA status for visible complaints
          this.loadSLAStatus();
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'Failed to load complaints';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  loadSLAStatus(): void {
    if (this.complaints.length === 0) return;

    this.loadingSLA = true;
    const complaintIds = this.complaints.map(c => c.id);

    this.slaService.getBulkSLAStatus(complaintIds)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            // CRITICAL FIX: Extract the 'statuses' object from the response data
            // The response.data is { statuses: { complaintId: SLAStatusSummary }, totalCount, successCount }
            const statusesData = (response.data as any).statuses || response.data;
            // Convert to Map for easy lookup
            this.slaStatusMap = new Map(Object.entries(statusesData));

            // CRITICAL FIX: Force table re-render by creating new array reference
            // This is needed because we use OnPush change detection and the table
            // needs to re-render cells when SLA data becomes available
            this.complaints = [...this.complaints];

            this.cdr.markForCheck();
          }
          this.loadingSLA = false;
        },
        error: (err) => {
          console.error('Failed to load SLA status:', err);
          this.loadingSLA = false;
          this.cdr.markForCheck();
        }
      });
  }

  getSLAStatus(complaintId: string): SLAStatusSummary | undefined {
    return this.slaStatusMap.get(complaintId);
  }

  getSLAUrgency(complaintId: string): UrgencyLevel {
    const status = this.slaStatusMap.get(complaintId);
    return status?.urgencyLevel || 'green';
  }

  getSLARemainingTime(complaintId: string): string {
    const status = this.slaStatusMap.get(complaintId);
    if (!status) return '';
    return this.slaService.formatMinutes(status.remainingMinutes);
  }

  viewComplaint(id: string): void {
    this.router.navigate(['/complaints', id]);
  }

  createComplaint(): void {
    this.router.navigate(['/complaints/new']);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadComplaints();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadComplaints();
  }

  clearFilters(): void {
    this.statusFilter = undefined;
    this.priorityFilter = undefined;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadComplaints();
  }

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getPriorityLabel(priority: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (priority === null || priority === undefined) {
      return 'Unknown';
    }

    // Handle string values from API
    if (typeof priority === 'string') {
      return priority; // Return the string value directly from API
    }

    // Handle numeric enum values
    switch(priority) {
      case ComplaintPriority.Low: return 'Low';
      case ComplaintPriority.Normal: return 'Normal';
      case ComplaintPriority.High: return 'High';
      case ComplaintPriority.Critical: return 'Critical';
      case ComplaintPriority.Urgent: return 'Urgent';
      default: return 'Unknown';
    }
  }

  getPriorityClass(priority: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (priority === null || priority === undefined) {
      return 'badge bg-secondary';
    }

    // Handle string values from API
    if (typeof priority === 'string') {
      switch(priority.toLowerCase()) {
        case 'critical':
        case 'urgent':
        case 'emergency':
        case 'severe':
          return 'badge bg-danger';
        case 'high':
        case 'elevated':
          return 'badge bg-warning';
        case 'normal':
        case 'medium':
          return 'badge bg-primary';
        case 'low':
          return 'badge bg-secondary';
        default:
          return 'badge bg-secondary';
      }
    }

    // Handle numeric enum values
    switch(priority) {
      case ComplaintPriority.Critical:
      case ComplaintPriority.Urgent:
        return 'badge bg-danger';
      case ComplaintPriority.High:
        return 'badge bg-warning';
      case ComplaintPriority.Normal:
        return 'badge bg-primary';
      case ComplaintPriority.Low:
        return 'badge bg-secondary';
      default:
        return 'badge bg-secondary';
    }
  }

  getStatusClass(status: ComplaintStatus | number | string | undefined | null): string {
    // Add null/undefined safety check
    if (status === null || status === undefined) {
      return 'badge bg-secondary';
    }

    // Handle string values from API
    if (typeof status === 'string') {
      switch(status.toLowerCase()) {
        case 'closed':
        case 'resolved':
          return 'badge bg-success';
        case 'inprogress':
        case 'in progress':
          return 'badge bg-primary';
        case 'escalated':
          return 'badge bg-danger';
        case 'submitted':
        case 'underreview':
        case 'under review':
          return 'badge bg-info';
        case 'pendinginfo':
        case 'pending info':
          return 'badge bg-warning';
        case 'rejected':
          return 'badge bg-dark';
        case 'reopened':
          return 'badge bg-warning';
        default:
          return 'badge bg-secondary';
      }
    }

    const statusValue = typeof status === 'number' ? status : Number(status as unknown);
    switch(statusValue) {
      case ComplaintStatus.Closed:
      case ComplaintStatus.Resolved:
        return 'badge bg-success';
      case ComplaintStatus.InProgress:
        return 'badge bg-primary';
      case ComplaintStatus.Escalated:
        return 'badge bg-danger';
      case ComplaintStatus.Submitted:
      case ComplaintStatus.UnderReview:
        return 'badge bg-info';
      case ComplaintStatus.PendingInfo:
        return 'badge bg-warning';
      case ComplaintStatus.Rejected:
        return 'badge bg-dark';
      case ComplaintStatus.Reopened:
        return 'badge bg-warning';
      default:
        return 'badge bg-secondary';
    }
  }

  getStatusLabel(status: ComplaintStatus | number | string | undefined | null): string {
    // Add null/undefined safety check
    if (status === null || status === undefined) {
      return 'Unknown';
    }

    // Handle string values from API
    if (typeof status === 'string') {
      return status; // Return the string value directly from API
    }

    // Safe numeric conversion with fallback
    const statusValue = typeof status === 'number' ? status : parseInt(status as string);
    if (isNaN(statusValue)) {
      return 'Unknown';
    }
    switch(statusValue) {
      case ComplaintStatus.Submitted: return 'Submitted';
      case ComplaintStatus.UnderReview: return 'Under Review';
      case ComplaintStatus.InProgress: return 'In Progress';
      case ComplaintStatus.Escalated: return 'Escalated';
      case ComplaintStatus.PendingInfo: return 'Pending Info';
      case ComplaintStatus.Resolved: return 'Resolved';
      case ComplaintStatus.Closed: return 'Closed';
      case ComplaintStatus.Rejected: return 'Rejected';
      case ComplaintStatus.Reopened: return 'Reopened';
      default: return 'Unknown';
    }
  }

  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    // Use DateService which respects user's configured timezone
    return this.dateService.formatDate(dateString);
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getContactMethodLabel(method: number): string {
    switch(method) {
      case PreferredContactMethod.Email: return 'Email';
      case PreferredContactMethod.Phone: return 'Phone';
      case PreferredContactMethod.Both: return 'Email & Phone';
      case PreferredContactMethod.SMS: return 'SMS';
      case PreferredContactMethod.InApp: return 'In-App';
      default: return 'Unknown';
    }
  }

  // Virtual Scroll Table Helper Methods - Using existing methods above

  trackComplaintBy = (index: number, complaint: Complaint): string => {
    return complaint.id;
  };

  onRowClick(complaint: Complaint): void {
    this.viewComplaint(complaint.id);
  }

  onSort(event: SortEvent): void {
    // Handle sorting if needed
    console.log('Sort event:', event);
  }

  // Badge class helpers for new UI design
  getStatusBadgeClass(status: ComplaintStatus | number | string | undefined | null): string {
    if (status === null || status === undefined) {
      return 'status-submitted';
    }

    // Handle string values from API
    if (typeof status === 'string') {
      switch(status.toLowerCase()) {
        case 'closed': return 'status-closed';
        case 'resolved': return 'status-resolved';
        case 'inprogress':
        case 'in progress': return 'status-in-progress';
        case 'escalated': return 'status-escalated';
        case 'submitted': return 'status-submitted';
        case 'underreview':
        case 'under review': return 'status-under-review';
        case 'pendinginfo':
        case 'pending info': return 'status-pending';
        case 'rejected': return 'status-rejected';
        case 'reopened': return 'status-reopened';
        default: return 'status-submitted';
      }
    }

    const statusValue = typeof status === 'number' ? status : Number(status as unknown);
    switch(statusValue) {
      case ComplaintStatus.Closed: return 'status-closed';
      case ComplaintStatus.Resolved: return 'status-resolved';
      case ComplaintStatus.InProgress: return 'status-in-progress';
      case ComplaintStatus.Escalated: return 'status-escalated';
      case ComplaintStatus.Submitted: return 'status-submitted';
      case ComplaintStatus.UnderReview: return 'status-under-review';
      case ComplaintStatus.PendingInfo: return 'status-pending';
      case ComplaintStatus.Rejected: return 'status-rejected';
      case ComplaintStatus.Reopened: return 'status-reopened';
      default: return 'status-submitted';
    }
  }

  getPriorityBadgeClass(priority: number | string | undefined | null): string {
    if (priority === null || priority === undefined) {
      return 'priority-normal';
    }

    // Handle string values from API
    if (typeof priority === 'string') {
      switch(priority.toLowerCase()) {
        case 'critical': return 'priority-critical';
        case 'urgent': return 'priority-urgent';
        case 'high': return 'priority-high';
        case 'normal':
        case 'medium': return 'priority-normal';
        case 'low': return 'priority-low';
        default: return 'priority-normal';
      }
    }

    switch(priority) {
      case ComplaintPriority.Critical: return 'priority-critical';
      case ComplaintPriority.Urgent: return 'priority-urgent';
      case ComplaintPriority.High: return 'priority-high';
      case ComplaintPriority.Normal: return 'priority-normal';
      case ComplaintPriority.Low: return 'priority-low';
      default: return 'priority-normal';
    }
  }

  // Memory Leak Prevention
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
