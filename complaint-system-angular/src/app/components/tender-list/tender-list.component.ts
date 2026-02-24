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
import { TenderService, Tender, TenderListResponse } from '../../services/tender.service';

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
    MatProgressSpinnerModule
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
    'title',
    'reference_number',
    'issuing_authority',
    'deadline',
    'status',
    'document_count',
    'actions'
  ];

  tenders = signal<Tender[]>([]);
  loading = signal(false);

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
      },
      error: (error) => {
        console.error('Error loading tenders:', error);
        this.snackBar.open('Failed to load tenders', 'Close', { duration: 3000 });
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
    this.router.navigate(['/tenders/create']);
  }

  onViewTender(tender: Tender) {
    this.router.navigate(['/tenders', tender.id]);
  }

  onEditTender(tender: Tender) {
    this.router.navigate(['/tenders', tender.id, 'edit']);
  }

  onDeleteTender(tender: Tender) {
    if (confirm(`Are you sure you want to delete tender "${tender.title}"?`)) {
      this.tenderService.deleteTender(tender.id).subscribe({
        next: () => {
          this.snackBar.open('Tender deleted successfully', 'Close', { duration: 3000 });
          this.loadTenders();
        },
        error: (error) => {
          console.error('Error deleting tender:', error);
          this.snackBar.open('Failed to delete tender', 'Close', { duration: 3000 });
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
}
