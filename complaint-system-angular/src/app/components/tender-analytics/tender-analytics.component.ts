import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TenderService, TenderAnalytics, WinLossAnalytics } from '../../services/tender.service';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-tender-analytics',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatSnackBarModule,
    MatProgressBarModule,
  ],
  templateUrl: './tender-analytics.component.html',
  styleUrls: ['./tender-analytics.component.css'],
})
export class TenderAnalyticsComponent implements OnInit {
  private tenderService = inject(TenderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  analytics = signal<TenderAnalytics | null>(null);
  winLoss = signal<WinLossAnalytics | null>(null);
  loading = signal(false);

  statusConfig: { [key: string]: { label: string; color: string; icon: string } } = {
    draft: { label: 'Draft', color: '#9e9e9e', icon: 'edit' },
    in_progress: { label: 'In Progress', color: '#2196f3', icon: 'schedule' },
    submitted: { label: 'Submitted', color: '#ff9800', icon: 'send' },
    won: { label: 'Won', color: '#4caf50', icon: 'check_circle' },
    lost: { label: 'Lost', color: '#f44336', icon: 'cancel' },
    cancelled: { label: 'Cancelled', color: '#757575', icon: 'block' },
  };

  monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  ngOnInit() {
    this.loadAnalytics();
    this.loadWinLossAnalytics();
  }

  loadAnalytics() {
    this.loading.set(true);
    this.tenderService.getAnalytics().subscribe({
      next: (data) => {
        this.analytics.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to load analytics', 'Close', { duration: 5000 });
        this.loading.set(false);
      },
    });
  }

  loadWinLossAnalytics() {
    this.tenderService.getWinLossAnalytics().subscribe({
      next: (data) => this.winLoss.set(data),
      error: (err) => { console.error('Error loading win/loss analytics:', err); }
    });
  }

  getLossReasonsList(): { reason: string; count: number }[] {
    const wl = this.winLoss();
    if (!wl) return [];
    return Object.entries(wl.loss_reasons)
      .map(([reason, count]) => ({ reason, count }))
      .sort((a, b) => b.count - a.count);
  }

  getLossReasonBarWidth(count: number): string {
    const reasons = this.getLossReasonsList();
    if (reasons.length === 0) return '0%';
    const max = Math.max(...reasons.map(r => r.count), 1);
    return Math.round((count / max) * 100) + '%';
  }

  getStatusLabel(status: string): string {
    return this.statusConfig[status]?.label || status;
  }

  getStatusColor(status: string): string {
    return this.statusConfig[status]?.color || '#9e9e9e';
  }

  getStatusIcon(status: string): string {
    return this.statusConfig[status]?.icon || 'help';
  }

  getStatusBarWidth(count: number): string {
    const a = this.analytics();
    if (!a || !a.total_tenders) return '0%';
    return Math.round((count / a.total_tenders) * 100) + '%';
  }

  getMonthBarWidth(count: number): string {
    const a = this.analytics();
    if (!a) return '0%';
    const max = Math.max(...a.monthly_trend.map(m => m.count), 1);
    return Math.round((count / max) * 100) + '%';
  }

  getMonthLabel(year: number, month: number): string {
    return this.monthNames[month - 1] + ' ' + year;
  }

  getTotalValue(): number {
    const a = this.analytics();
    if (!a) return 0;
    return a.value_analysis.reduce((sum, v) => sum + (v.total_value || 0), 0);
  }

  formatCurrency(value: number | null | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', maximumFractionDigits: 0
    }).format(value);
  }

  navigateBack() {
    this.router.navigate(['/tender-dashboard']);
  }
}
