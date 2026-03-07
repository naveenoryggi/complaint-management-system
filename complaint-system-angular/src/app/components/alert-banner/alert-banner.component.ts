import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AlertService, TenderAlert } from '../../services/alert.service';

@Component({
  selector: 'app-alert-banner',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatBadgeModule
  ],
  templateUrl: './alert-banner.component.html',
  styleUrls: ['./alert-banner.component.css']
})
export class AlertBannerComponent implements OnInit {
  private alertService = inject(AlertService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  alerts = signal<TenderAlert[]>([]);
  loading = signal(false);
  collapsed = signal(false);

  ngOnInit(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.loading.set(true);
    this.alertService.getAlerts().subscribe({
      next: (data) => {
        this.alerts.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open('Failed to load alerts', 'Close', { duration: 3000 });
      }
    });
  }

  dismissAlert(alertId: string, event: MouseEvent): void {
    event.stopPropagation();
    this.alertService.dismissAlert(alertId).subscribe({
      next: () => {
        this.alerts.update(list => list.filter(a => a.id !== alertId));
        this.snackBar.open('Alert dismissed', 'Close', { duration: 2000 });
      },
      error: () => {
        this.snackBar.open('Failed to dismiss alert', 'Close', { duration: 3000 });
      }
    });
  }

  toggleCollapsed(): void {
    this.collapsed.update(v => !v);
  }

  navigateToTender(tenderId: string): void {
    this.router.navigate(['/tenders', tenderId]);
  }

  getSeverityColor(severity: string): string {
    switch (severity) {
      case 'critical': return 'critical';
      case 'warning':  return 'warning';
      case 'info':     return 'info';
      default:         return 'info';
    }
  }

  getSeverityIcon(severity: string): string {
    switch (severity) {
      case 'critical': return 'error';
      case 'warning':  return 'warning';
      case 'info':     return 'info';
      default:         return 'notifications';
    }
  }

  getAlertTypeLabel(type: string): string {
    switch (type) {
      case 'tender_deadline': return 'Tender Deadline';
      case 'emd_expiry':      return 'EMD Expiry';
      case 'fee_due':         return 'Fee Due';
      default:                return type;
    }
  }

  get criticalCount(): number {
    return this.alerts().filter(a => a.severity === 'critical').length;
  }

  get warningCount(): number {
    return this.alerts().filter(a => a.severity === 'warning').length;
  }

  get infoCount(): number {
    return this.alerts().filter(a => a.severity === 'info').length;
  }
}
