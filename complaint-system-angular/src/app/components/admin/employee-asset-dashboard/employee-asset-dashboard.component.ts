import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AssetAssignmentService } from '../../../services/asset-assignment.service';
import {
  AssetAssignment,
  EmployeeAssetDashboard,
  AssetAssignmentSummary,
  ReportIssueRequest,
  AssetAssignmentPurposeLabels,
  AssetConditionLabels,
  AssetIssueType,
  AssetIssueTypeLabels,
  AssetIssueSeverity,
  AssetIssueSeverityLabels,
  getPurposeColorClass,
  getConditionColorClass
} from '../../../models/asset-assignment.model';

@Component({
  selector: 'app-employee-asset-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './employee-asset-dashboard.component.html',
  styleUrls: ['./employee-asset-dashboard.component.scss']
})
export class EmployeeAssetDashboardComponent implements OnInit {
  // Data
  dashboard = signal<EmployeeAssetDashboard | null>(null);
  selectedAssignment = signal<AssetAssignment | null>(null);

  // UI State
  isLoading = signal(false);
  showReportIssueModal = signal(false);
  showDetailsModal = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Active tab
  activeTab = signal<'current' | 'pending' | 'overdue' | 'upcoming' | 'history'>('current');

  // Forms
  reportIssueForm: FormGroup;

  // Enum options
  issueTypeOptions = Object.entries(AssetIssueTypeLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  severityOptions = Object.entries(AssetIssueSeverityLabels).map(([value, label]) => ({
    value: parseInt(value),
    label
  }));

  // Helpers
  getPurposeLabel = (purpose: number) => AssetAssignmentPurposeLabels[purpose] || 'Unknown';
  getConditionLabel = (condition: number) => AssetConditionLabels[condition] || 'Unknown';
  getPurposeColorClass = getPurposeColorClass;
  getConditionColorClass = getConditionColorClass;

  // Computed values
  currentAssignments = computed(() => this.dashboard()?.currentAssignments || []);
  responsibleForAssignments = computed(() => this.dashboard()?.responsibleForAssignments || []);
  pendingAcknowledgement = computed(() => this.dashboard()?.pendingAcknowledgement || []);
  overdueReturns = computed(() => this.dashboard()?.overdueReturns || []);
  upcomingReturns = computed(() => this.dashboard()?.upcomingReturns || []);
  recentHistory = computed(() => this.dashboard()?.recentHistory || []);
  statistics = computed(() => this.dashboard()?.statistics);

  constructor(
    private assignmentService: AssetAssignmentService,
    private fb: FormBuilder
  ) {
    this.reportIssueForm = this.fb.group({
      issueType: [AssetIssueType.Malfunction, Validators.required],
      severity: [AssetIssueSeverity.Medium, Validators.required],
      description: ['', [Validators.required, Validators.minLength(10)]],
      isAssetUsable: [true]
    });
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.assignmentService.getMyDashboard().subscribe({
      next: (data) => {
        this.dashboard.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load dashboard');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  // Tab navigation
  setActiveTab(tab: 'current' | 'pending' | 'overdue' | 'upcoming' | 'history'): void {
    this.activeTab.set(tab);
  }

  // Modal operations
  openReportIssueModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.reportIssueForm.reset({
      issueType: AssetIssueType.Malfunction,
      severity: AssetIssueSeverity.Medium,
      isAssetUsable: true
    });
    this.showReportIssueModal.set(true);
  }

  openDetailsModal(assignment: AssetAssignmentSummary): void {
    this.loadAssignmentDetails(assignment.id);
    this.showDetailsModal.set(true);
  }

  loadAssignmentDetails(id: string): void {
    this.assignmentService.getById(id).subscribe({
      next: (data) => {
        this.selectedAssignment.set(data);
      },
      error: (err) => console.error('Failed to load assignment details', err)
    });
  }

  closeModals(): void {
    this.showReportIssueModal.set(false);
    this.showDetailsModal.set(false);
    this.selectedAssignment.set(null);
  }

  // Actions
  acknowledgeAssignment(assignment: AssetAssignmentSummary): void {
    if (!confirm('Acknowledge receipt of this asset?')) return;

    this.isLoading.set(true);
    this.assignmentService.acknowledgeAsset(assignment.id, {}).subscribe({
      next: () => {
        this.showSuccess('Asset acknowledged successfully');
        this.loadDashboard();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to acknowledge asset');
        this.isLoading.set(false);
      }
    });
  }

  submitReportIssue(): void {
    if (this.reportIssueForm.invalid || !this.selectedAssignment()) return;

    this.isLoading.set(true);
    const request: ReportIssueRequest = this.reportIssueForm.value;

    this.assignmentService.reportIssue(this.selectedAssignment()!.id, request).subscribe({
      next: () => {
        this.showSuccess('Issue reported successfully');
        this.closeModals();
        this.loadDashboard();
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to report issue');
        this.isLoading.set(false);
      }
    });
  }

  // Utility methods
  showSuccess(message: string): void {
    this.successMessage.set(message);
    this.isLoading.set(false);
    setTimeout(() => this.successMessage.set(null), 3000);
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  formatDateTime(date: Date | string | undefined): string {
    if (!date) return '-';
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

  getDaysOverdue(expectedReturnDate?: Date): number {
    if (!expectedReturnDate) return 0;
    const now = new Date();
    const due = new Date(expectedReturnDate);
    const diff = Math.ceil((now.getTime() - due.getTime()) / (1000 * 60 * 60 * 24));
    return Math.max(0, diff);
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
      return 'Pending';
    }
    return 'Active';
  }

  getSeverityClass(severity: AssetIssueSeverity): string {
    switch (severity) {
      case AssetIssueSeverity.Low: return 'bg-blue-100 text-blue-800';
      case AssetIssueSeverity.Medium: return 'bg-yellow-100 text-yellow-800';
      case AssetIssueSeverity.High: return 'bg-orange-100 text-orange-800';
      case AssetIssueSeverity.Critical: return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
