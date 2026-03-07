import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TrackingService } from '../../services/tracking.service';

@Component({
  selector: 'app-portal-status-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './portal-status-widget.component.html',
  styleUrls: ['./portal-status-widget.component.css']
})
export class PortalStatusWidgetComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private router = inject(Router);

  activeCount = signal(0);
  expiringCount = signal(0);
  expiredCount = signal(0);
  loading = signal(false);

  ngOnInit(): void {
    this.loadPortals();
  }

  private loadPortals(): void {
    this.loading.set(true);
    this.trackingService.listPortals().subscribe({
      next: (portals: any[]) => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const thirtyDaysFromNow = new Date(today);
        thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);

        let active = 0;
        let expiring = 0;
        let expired = 0;

        (portals || []).forEach((portal: any) => {
          const status = (portal.registration_status || '').toLowerCase();
          const dscExpiry = portal.dsc_expiry_date ? new Date(portal.dsc_expiry_date) : null;

          if (dscExpiry) {
            dscExpiry.setHours(0, 0, 0, 0);
            if (dscExpiry < today) {
              expired++;
            } else if (dscExpiry <= thirtyDaysFromNow) {
              expiring++;
            } else if (status === 'active' || status === 'registered') {
              active++;
            } else {
              active++;
            }
          } else {
            if (status === 'expired' || status === 'inactive') {
              expired++;
            } else if (status === 'expiring_soon') {
              expiring++;
            } else {
              active++;
            }
          }
        });

        this.activeCount.set(active);
        this.expiringCount.set(expiring);
        this.expiredCount.set(expired);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Error loading portals for widget', err);
        this.loading.set(false);
      }
    });
  }

  get totalCount(): number {
    return this.activeCount() + this.expiringCount() + this.expiredCount();
  }

  get healthScore(): number {
    const total = this.totalCount;
    if (total === 0) return 100;
    return Math.round((this.activeCount() / total) * 100);
  }

  get healthLabel(): string {
    const score = this.healthScore;
    if (score >= 80) return 'Good';
    if (score >= 50) return 'Fair';
    return 'Needs Attention';
  }

  get healthClass(): string {
    const score = this.healthScore;
    if (score >= 80) return 'health-good';
    if (score >= 50) return 'health-fair';
    return 'health-poor';
  }

  navigateToPortals(): void {
    this.router.navigate(['/portal-registrations']);
  }
}
