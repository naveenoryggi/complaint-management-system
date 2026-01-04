import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssetRequestService } from '../../../services/asset-request.service';
import { StoreService } from '../../../services/store.service';
import { LoggerService } from '../../../core/services/logger.service';
import {
  AssetRequest,
  AssetRequestListItem,
  AssetRequestFilterDto,
  AssetRequestType,
  AssetRequestStatus,
  AssetRequestTypeLabels,
  AssetRequestStatusLabels,
  AssetRequestStatusColors,
  ApprovalActionLabels,
  AssignmentPurposeLabels
} from '../../../models/asset-request.model';
import { StoreLookup } from '../../../models/store.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-asset-request-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './asset-request-management.component.html',
  styleUrls: ['./asset-request-management.component.scss']
})
export class AssetRequestManagementComponent implements OnInit {
  requests: AssetRequestListItem[] = [];
  stores: StoreLookup[] = [];
  selectedRequest: AssetRequest | null = null;

  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  showDetailModal = false;
  showApproveModal = false;
  showRejectModal = false;

  // Filter
  filter: AssetRequestFilterDto = {
    page: 1,
    pageSize: 20
  };
  totalCount = 0;
  totalPages = 0;

  // Action forms
  approveComments = '';
  rejectReason = '';
  rejectComments = '';

  // Enums and labels for template
  AssetRequestType = AssetRequestType;
  AssetRequestStatus = AssetRequestStatus;
  AssetRequestTypeLabels = AssetRequestTypeLabels;
  AssetRequestStatusLabels = AssetRequestStatusLabels;
  AssetRequestStatusColors = AssetRequestStatusColors;
  ApprovalActionLabels = ApprovalActionLabels;
  AssignmentPurposeLabels = AssignmentPurposeLabels;

  requestTypes = [
    { value: undefined, label: 'All Types' },
    { value: AssetRequestType.Return, label: 'Return' },
    { value: AssetRequestType.Assignment, label: 'Assignment' },
    { value: AssetRequestType.Transfer, label: 'Transfer' }
  ];

  requestStatuses = [
    { value: undefined, label: 'All Statuses' },
    { value: AssetRequestStatus.Draft, label: 'Draft' },
    { value: AssetRequestStatus.Pending, label: 'Pending' },
    { value: AssetRequestStatus.Escalated, label: 'Escalated' },
    { value: AssetRequestStatus.Approved, label: 'Approved' },
    { value: AssetRequestStatus.Rejected, label: 'Rejected' },
    { value: AssetRequestStatus.Cancelled, label: 'Cancelled' },
    { value: AssetRequestStatus.Completed, label: 'Completed' }
  ];

  constructor(
    private requestService: AssetRequestService,
    private storeService: StoreService,
    private logger: LoggerService
  ) {}

  ngOnInit(): void {
    this.loadRequests();
    this.loadStores();
  }

  loadRequests(): void {
    this.isLoading = true;
    this.requestService.getRequests(this.filter).subscribe({
      next: (result) => {
        this.requests = result.items || [];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        this.logger.error('Error loading requests', error);
        this.errorMessage = 'Failed to load requests';
        this.isLoading = false;
      }
    });
  }

  loadStores(): void {
    this.storeService.getStoreLookup().subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.stores = result.data;
        }
      },
      error: (error) => {
        this.logger.error('Error loading stores', error);
      }
    });
  }

  applyFilter(): void {
    this.filter.page = 1;
    this.loadRequests();
  }

  resetFilter(): void {
    this.filter = {
      page: 1,
      pageSize: 20
    };
    this.loadRequests();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.filter.page = page;
      this.loadRequests();
    }
  }

  viewDetails(request: AssetRequestListItem): void {
    this.isLoading = true;
    this.requestService.getRequest(request.id).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.selectedRequest = result.data;
          this.showDetailModal = true;
        } else {
          this.errorMessage = result.message;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.logger.error('Error loading request details', error);
        this.errorMessage = 'Failed to load request details';
        this.isLoading = false;
      }
    });
  }

  closeDetailModal(): void {
    this.showDetailModal = false;
    this.selectedRequest = null;
  }

  openApproveModal(): void {
    this.approveComments = '';
    this.showApproveModal = true;
  }

  closeApproveModal(): void {
    this.showApproveModal = false;
    this.approveComments = '';
  }

  openRejectModal(): void {
    this.rejectReason = '';
    this.rejectComments = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.rejectReason = '';
    this.rejectComments = '';
  }

  approveRequest(): void {
    if (!this.selectedRequest) return;

    this.isSubmitting = true;
    this.requestService.approveRequest(this.selectedRequest.id, { comments: this.approveComments }).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Request approved successfully';
          this.loadRequests();
          this.closeApproveModal();
          this.closeDetailModal();
        } else {
          this.errorMessage = result.message;
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error approving request', error);
        this.errorMessage = 'Failed to approve request';
        this.isSubmitting = false;
      }
    });
  }

  rejectRequest(): void {
    if (!this.selectedRequest || !this.rejectReason) {
      this.errorMessage = 'Rejection reason is required';
      return;
    }

    this.isSubmitting = true;
    this.requestService.rejectRequest(this.selectedRequest.id, {
      reason: this.rejectReason,
      comments: this.rejectComments
    }).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Request rejected';
          this.loadRequests();
          this.closeRejectModal();
          this.closeDetailModal();
        } else {
          this.errorMessage = result.message;
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error rejecting request', error);
        this.errorMessage = 'Failed to reject request';
        this.isSubmitting = false;
      }
    });
  }

  completeRequest(): void {
    if (!this.selectedRequest) return;

    if (!confirm('Mark this request as completed?')) return;

    this.isSubmitting = true;
    this.requestService.completeRequest(this.selectedRequest.id).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Request marked as completed';
          this.loadRequests();
          this.closeDetailModal();
        } else {
          this.errorMessage = result.message;
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error completing request', error);
        this.errorMessage = 'Failed to complete request';
        this.isSubmitting = false;
      }
    });
  }

  cancelRequest(): void {
    if (!this.selectedRequest) return;

    const reason = prompt('Enter cancellation reason (optional):');

    this.isSubmitting = true;
    this.requestService.cancelRequest(this.selectedRequest.id, reason || undefined).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Request cancelled';
          this.loadRequests();
          this.closeDetailModal();
        } else {
          this.errorMessage = result.message;
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error cancelling request', error);
        this.errorMessage = 'Failed to cancel request';
        this.isSubmitting = false;
      }
    });
  }

  getStatusBadgeClass(status: AssetRequestStatus): string {
    return `badge bg-${AssetRequestStatusColors[status] || 'secondary'}`;
  }

  getStoreName(storeId: string | undefined): string {
    if (!storeId) return '-';
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.name : '-';
  }

  canApprove(request: AssetRequest): boolean {
    return request.status === AssetRequestStatus.Pending || request.status === AssetRequestStatus.Escalated;
  }

  canComplete(request: AssetRequest): boolean {
    return request.status === AssetRequestStatus.Approved;
  }

  canCancel(request: AssetRequest): boolean {
    return request.status === AssetRequestStatus.Draft ||
           request.status === AssetRequestStatus.Pending ||
           request.status === AssetRequestStatus.Escalated;
  }
}
