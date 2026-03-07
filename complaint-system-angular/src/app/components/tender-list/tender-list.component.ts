import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime } from 'rxjs/operators';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { TenderService, Tender, TenderListResponse, ComplianceScoreSummary } from '../../services/tender.service';

@Component({
  selector: 'app-tender-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatCheckboxModule
  ],
  templateUrl: './tender-list.component.html',
  styleUrls: ['./tender-list.component.css']
})
export class TenderListComponent implements OnInit {
  private tenderService = inject(TenderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  displayedColumns: string[] = [
    'select',
    'title',
    'reference_number',
    'issuing_authority',
    'deadline',
    'status',
    'document_count',
    'readiness',
    'actions'
  ];

  tenders = signal<Tender[]>([]);
  loading = signal(false);
  complianceScores = signal<Map<string, ComplianceScoreSummary>>(new Map());
  selectedTenderIds = signal<Set<string>>(new Set());

  // Pagination
  totalTenders = signal(0);
  pageSize = signal(50);
  pageIndex = signal(0);

  // Filters
  searchControl = new FormControl('');
  statusControl = new FormControl('all');

  statusOptions = [
    { value: 'all', label: 'All Status' },
    { value: 'draft', label: 'Draft' },
    { value: 'in_progress', label: 'In Progress' },
    { value: 'submitted', label: 'Submitted' },
    { value: 'won', label: 'Won' },
    { value: 'lost', label: 'Lost' },
    { value: 'cancelled', label: 'Cancelled' }
  ];

  ngOnInit() {
    this.loadTenders();

    // Search with debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300)
    ).subscribe(() => {
      this.pageIndex.set(0);
      this.loadTenders();
    });

    // Status filter
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadTenders();
    });
  }

  loadTenders() {
    this.loading.set(true);

    const status = this.statusControl.value === 'all' ? undefined : this.statusControl.value || undefined;
    const search = this.searchControl.value || undefined;

    this.tenderService.listTenders(
      this.pageIndex() + 1,
      this.pageSize(),
      status,
      search
    ).subscribe({
      next: (response: TenderListResponse) => {
        this.tenders.set(response.items);
        this.totalTenders.set(response.total);
        this.loading.set(false);
        this.loadComplianceScores(response.items);
      },
      error: (err) => {
        console.error('Error loading tenders:', err);
        this.snackBar.open(err.error?.detail || 'Failed to load tenders', 'Close', { duration: 5000 });
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadTenders();
  }

  onCreateTender() {
    this.router.navigate(['/tenders/new']);
  }

  onViewTender(tender: Tender) {
    this.router.navigate(['/tenders', tender.id]);
  }

  onEditTender(tender: Tender) {
    this.router.navigate(['/tenders', tender.id, 'edit']);
  }

  onCloneTender(tender: Tender, event: Event) {
    event.stopPropagation();
    const cloneData: any = {
      title: `${tender.title} (Copy)`,
      reference_number: tender.reference_number || undefined,
      issuing_authority: tender.issuing_authority || undefined,
      portal_name: (tender as any).portal_name || undefined,
      portal_url: (tender as any).portal_url || undefined,
      deadline: tender.deadline || undefined,
      estimated_value: (tender as any).estimated_value || undefined,
      notes: (tender as any).notes || undefined,
      status: 'draft'
    };

    this.tenderService.createTender(cloneData).subscribe({
      next: (newTender) => {
        this.snackBar.open('Tender cloned successfully', 'View', { duration: 5000 })
          .onAction().subscribe(() => {
            this.router.navigate(['/tenders', newTender.id]);
          });
        this.loadTenders();
      },
      error: (err) => {
        console.error('Error cloning tender:', err);
        this.snackBar.open(err.error?.detail || 'Failed to clone tender', 'Close', { duration: 5000 });
      }
    });
  }

  onDeleteTender(tender: Tender) {
    if (confirm(`Are you sure you want to delete tender "${tender.title}"?`)) {
      this.tenderService.deleteTender(tender.id).subscribe({
        next: () => {
          this.snackBar.open('Tender deleted successfully', 'Close', { duration: 3000 });
          this.loadTenders();
        },
        error: (err) => {
          console.error('Error deleting tender:', err);
          this.snackBar.open(err.error?.detail || 'Failed to delete tender', 'Close', { duration: 5000 });
        }
      });
    }
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'draft': 'default',
      'in_progress': 'primary',
      'submitted': 'accent',
      'won': 'success',
      'lost': 'warn',
      'cancelled': 'default'
    };
    return colors[status] || 'default';
  }

  onExportCSV() {
    const tenders = this.tenders();
    if (tenders.length === 0) {
      this.snackBar.open('No tenders to export', 'Close', { duration: 3000 });
      return;
    }

    const headers = ['Title', 'Reference Number', 'Issuing Authority', 'Deadline', 'Status', 'Documents'];
    const rows = tenders.map(t => [
      `"${(t.title || '').replace(/"/g, '""')}"`,
      `"${(t.reference_number || '').replace(/"/g, '""')}"`,
      `"${(t.issuing_authority || '').replace(/"/g, '""')}"`,
      t.deadline ? new Date(t.deadline).toLocaleDateString('en-IN') : '',
      t.status,
      (t.document_count || 0).toString()
    ]);

    const csv = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `tenders_export_${new Date().toISOString().split('T')[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
    this.snackBar.open(`Exported ${tenders.length} tenders to CSV`, 'Close', { duration: 3000 });
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'draft': 'edit',
      'in_progress': 'schedule',
      'submitted': 'send',
      'won': 'check_circle',
      'lost': 'cancel',
      'cancelled': 'block'
    };
    return icons[status] || 'help';
  }

  formatDeadline(deadline: string | undefined): string {
    if (!deadline) return 'No deadline';

    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffTime = deadlineDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) {
      return `Overdue by ${Math.abs(diffDays)} days`;
    } else if (diffDays === 0) {
      return 'Due today';
    } else if (diffDays === 1) {
      return 'Due tomorrow';
    } else if (diffDays <= 7) {
      return `${diffDays} days left`;
    } else {
      return deadlineDate.toLocaleDateString();
    }
  }

  isDeadlineNear(deadline: string | undefined): boolean {
    if (!deadline) return false;

    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffTime = deadlineDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays >= 0 && diffDays <= 7;
  }

  isDeadlineOverdue(deadline: string | undefined): boolean {
    if (!deadline) return false;

    const deadlineDate = new Date(deadline);
    const now = new Date();

    return deadlineDate.getTime() < now.getTime();
  }

  loadComplianceScores(tenders: Tender[]) {
    const ids = tenders.map(t => t.id);
    if (ids.length === 0) return;

    this.tenderService.getComplianceScores(ids).subscribe({
      next: (scores) => {
        const map = new Map<string, ComplianceScoreSummary>();
        scores.forEach(s => map.set(s.tender_id, s));
        this.complianceScores.set(map);
      },
      error: (err) => { console.error('Error loading compliance scores:', err); }
    });
  }

  getReadinessScore(tenderId: string): number | null {
    const score = this.complianceScores().get(tenderId);
    return score ? score.overall_score : null;
  }

  getReadinessLabel(tenderId: string): string {
    const score = this.complianceScores().get(tenderId);
    return score ? score.overall_label : '';
  }

  getReadinessColor(score: number | null): string {
    if (score === null) return '#9e9e9e';
    if (score >= 80) return '#4caf50';
    if (score >= 50) return '#ff9800';
    return '#f44336';
  }

  // --- Feature 7: Compare ---
  toggleSelect(tenderId: string) {
    const selected = new Set(this.selectedTenderIds());
    if (selected.has(tenderId)) {
      selected.delete(tenderId);
    } else {
      if (selected.size >= 4) {
        this.snackBar.open('Maximum 4 tenders can be compared', 'Close', { duration: 3000 });
        return;
      }
      selected.add(tenderId);
    }
    this.selectedTenderIds.set(selected);
  }

  isSelected(tenderId: string): boolean {
    return this.selectedTenderIds().has(tenderId);
  }

  getSelectedCount(): number {
    return this.selectedTenderIds().size;
  }

  onCompareSelected() {
    const ids = Array.from(this.selectedTenderIds());
    if (ids.length < 2) {
      this.snackBar.open('Select at least 2 tenders to compare', 'Close', { duration: 3000 });
      return;
    }
    this.router.navigate(['/tender-compare'], { queryParams: { ids: ids.join(',') } });
  }

  clearSelection() {
    this.selectedTenderIds.set(new Set());
  }
}
