import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TrackingService, DashboardSummary, PortalRegistration, EMDRecord } from '../../services/tracking.service';
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
    MatChipsModule,
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
  expiringDSCs = signal<PortalRegistration[]>([]);
  expiringEMDs = signal<EMDRecord[]>([]);
  statusDistribution = signal<{ status: string; label: string; count: number; color: string; icon: string }[]>([]);
  totalTenders = signal(0);
  loading = signal(false);

  statusConfig: { [key: string]: { label: string; color: string; icon: string } } = {
    'draft': { label: 'Draft', color: '#9e9e9e', icon: 'edit' },
    'in_progress': { label: 'In Progress', color: '#2196f3', icon: 'schedule' },
    'submitted': { label: 'Submitted', color: '#ff9800', icon: 'send' },
    'won': { label: 'Won', color: '#4caf50', icon: 'check_circle' },
    'lost': { label: 'Lost', color: '#f44336', icon: 'cancel' },
    'cancelled': { label: 'Cancelled', color: '#757575', icon: 'block' }
  };

  ngOnInit() {
    this.loadDashboard();
    this.loadUpcomingTenders();
    this.loadAlerts();
    this.loadStatusDistribution();
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

  loadAlerts() {
    // Load DSC expiry alerts
    this.trackingService.listPortals().subscribe({
      next: (portals) => {
        const now = new Date();
        const thirtyDaysFromNow = new Date();
        thirtyDaysFromNow.setDate(now.getDate() + 30);

        const expiring = portals.filter(p => {
          if (!p.dsc_expiry_date) return false;
          const expiry = new Date(p.dsc_expiry_date);
          return expiry <= thirtyDaysFromNow;
        }).sort((a, b) => {
          return new Date(a.dsc_expiry_date!).getTime() - new Date(b.dsc_expiry_date!).getTime();
        });
        this.expiringDSCs.set(expiring);
      },
      error: () => {}
    });

    // Load EMD validity alerts
    this.trackingService.listEMDs().subscribe({
      next: (emds) => {
        const now = new Date();
        const thirtyDaysFromNow = new Date();
        thirtyDaysFromNow.setDate(now.getDate() + 30);

        const expiring = emds.filter(e => {
          if (!e.validity_end_date || e.status === 'released') return false;
          const endDate = new Date(e.validity_end_date);
          return endDate <= thirtyDaysFromNow;
        }).sort((a, b) => {
          return new Date(a.validity_end_date!).getTime() - new Date(b.validity_end_date!).getTime();
        });
        this.expiringEMDs.set(expiring);
      },
      error: () => {}
    });
  }

  getDscExpiryDays(date: string): number {
    const expiry = new Date(date);
    const now = new Date();
    return Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
  }

  getDscExpiryLabel(date: string): string {
    const days = this.getDscExpiryDays(date);
    if (days < 0) return `Expired ${Math.abs(days)} days ago`;
    if (days === 0) return 'Expires today';
    if (days === 1) return 'Expires tomorrow';
    return `Expires in ${days} days`;
  }

  isDscExpired(date: string): boolean {
    return this.getDscExpiryDays(date) < 0;
  }

  isDscCritical(date: string): boolean {
    const days = this.getDscExpiryDays(date);
    return days <= 7;
  }

  loadStatusDistribution() {
    this.tenderService.listTenders(1, 1000).subscribe({
      next: (response) => {
        const counts: { [key: string]: number } = {};
        response.items.forEach(t => {
          counts[t.status] = (counts[t.status] || 0) + 1;
        });
        this.totalTenders.set(response.total);

        const distribution = Object.keys(this.statusConfig).map(status => ({
          status,
          label: this.statusConfig[status].label,
          count: counts[status] || 0,
          color: this.statusConfig[status].color,
          icon: this.statusConfig[status].icon
        })).filter(d => d.count > 0);

        this.statusDistribution.set(distribution);
      },
      error: () => {}
    });
  }

  getBarWidth(count: number): string {
    const total = this.totalTenders();
    if (!total) return '0%';
    return `${Math.round((count / total) * 100)}%`;
  }

  formatEmdAmount(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', maximumFractionDigits: 0
    }).format(amount);
  }
}
