import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TrackingService, DashboardSummary } from '../../services/tracking.service';
import { TenderService, Tender } from '../../services/tender.service';

@Component({
  selector: 'app-tender-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatListModule,
    MatSnackBarModule
  ],
  templateUrl: './tender-dashboard.component.html',
  styleUrls: ['./tender-dashboard.component.css']
})
export class TenderDashboardComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private tenderService = inject(TenderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  summary = signal<DashboardSummary | null>(null);
  upcomingTenders = signal<Tender[]>([]);
  loading = signal(false);

  ngOnInit() {
    this.loadDashboard();
    this.loadUpcomingTenders();
  }

  loadDashboard() {
    this.loading.set(true);
    this.trackingService.getDashboard().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.snackBar.open('Failed to load dashboard summary', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  loadUpcomingTenders() {
    this.tenderService.getUpcomingTenders(14).subscribe({
      next: (data) => this.upcomingTenders.set(data),
      error: (error) => {
        console.error('Error loading upcoming tenders:', error);
      }
    });
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(value);
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

  isDeadlineUrgent(deadline: string | undefined): boolean {
    if (!deadline) return false;
    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffTime = deadlineDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays <= 3;
  }
}
