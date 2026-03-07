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
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TenderService, Tender, ComplianceScoreSummary } from '../../services/tender.service';
import { KnowledgeBaseService, KBDashboardStats } from '../../services/knowledge-base.service';
import { AlertBannerComponent } from '../alert-banner/alert-banner.component';
import { PortalStatusWidgetComponent } from '../portal-status-widget/portal-status-widget.component';

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
    MatSnackBarModule,
    MatProgressBarModule,
    AlertBannerComponent,
    PortalStatusWidgetComponent
  ],
  templateUrl: './tender-dashboard.component.html',
  styleUrls: ['./tender-dashboard.component.css']
})
export class TenderDashboardComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private tenderService = inject(TenderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private kbService = inject(KnowledgeBaseService);

  summary = signal<DashboardSummary | null>(null);
  upcomingTenders = signal<Tender[]>([]);
  // Legacy alert signals kept for backward compat — alerts now handled by AlertBannerComponent
  expiringDSCs = signal<PortalRegistration[]>([]);
  expiringEMDs = signal<EMDRecord[]>([]);
  statusDistribution = signal<{ status: string; label: string; count: number; color: string; icon: string }[]>([]);
  totalTenders = signal(0);
  loading = signal(false);
  complianceScores = signal<Map<string, ComplianceScoreSummary>>(new Map());
  averageReadiness = signal<number | null>(null);
  kbStats = signal<KBDashboardStats | null>(null);

  statusConfig: { [key: string]: { label: string; color: string; icon: string } } = {
    'draft': { label: 'Draft', color: '#9e9e9e', icon: 'edit' },
    'in_progress': { label: 'In Progress', color: '#2196f3', icon: 'schedule' },
    'submitted': { label: 'Submitted', color: '#ff9800', icon: 'send' },
    'won': { label: 'Won', color: '#4caf50', icon: 'check_circle' },
    'lost': { label: 'Lost', color: '#f44336', icon: 'cancel' },
    'cancelled': { label: 'Cancelled', color: '#757575', icon: 'block' }
  };

  ngOnInit() {
    this.loading.set(true);

    // Fire all data loads in parallel
    this.loadDashboard();
    this.loadUpcomingTenders();
    this.loadStatusDistribution();
    this.loadKBStats();
    // Note: loadAlerts() removed — AlertBannerComponent and PortalStatusWidget
    // handle alerts independently, avoiding duplicate listPortals()/listEMDs() calls

    // Safety: ensure spinner never shows longer than 5s even if an API hangs
    setTimeout(() => {
      if (this.loading()) {
        this.loading.set(false);
      }
    }, 5000);
  }

  loadDashboard() {
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
      next: (data) => {
        this.upcomingTenders.set(data);
        this.loadComplianceScores(data);
      },
      error: (error) => {
        console.error('Error loading upcoming tenders:', error);
      }
    });
  }

  loadComplianceScores(tenders: Tender[]) {
    const ids = tenders.map(t => t.id);
    if (ids.length === 0) return;

    this.tenderService.getComplianceScores(ids).subscribe({
      next: (scores) => {
        const map = new Map<string, ComplianceScoreSummary>();
        scores.forEach(s => map.set(s.tender_id, s));
        this.complianceScores.set(map);

        if (scores.length > 0) {
          const avg = scores.reduce((sum, s) => sum + s.overall_score, 0) / scores.length;
          this.averageReadiness.set(Math.round(avg));
        }
      },
      error: () => {}
    });
  }

  getReadinessScore(tenderId: string): number | null {
    const s = this.complianceScores().get(tenderId);
    return s ? s.overall_score : null;
  }

  getReadinessColor(score: number | null): string {
    if (score === null) return '#9e9e9e';
    if (score >= 80) return '#4caf50';
    if (score >= 50) return '#ff9800';
    return '#f44336';
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
    this.tenderService.listTenders(1, 500).subscribe({
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

  loadKBStats() {
    this.kbService.getDashboardStats().subscribe({
      next: (stats) => this.kbStats.set(stats),
      error: () => {}
    });
  }

  getCategoryBarWidth(count: number): string {
    const stats = this.kbStats();
    if (!stats) return '0%';
    const max = Math.max(...stats.category_distribution.map(c => c.count), 1);
    return `${Math.round((count / max) * 100)}%`;
  }

  getCategoryLabel(category: string): string {
    const labels: { [key: string]: string } = {
      company_snippet: 'Company', financial_data: 'Financial', experience_summary: 'Experience',
      declaration_text: 'Declarations', technical_response: 'Technical', personnel_bio: 'Personnel',
      pricing_data: 'Pricing', standard_clause: 'Clauses', submission_note: 'Notes'
    };
    return labels[category] || category;
  }
}
