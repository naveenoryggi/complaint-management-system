import { Component, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, interval, takeUntil, switchMap, startWith } from 'rxjs';
import { SLAService, SLAStatusDisplay, UrgencyLevel } from '../../../services/sla.service';
import { SLABadgeComponent } from '../sla-badge/sla-badge.component';
import { SLAProgressBarComponent } from '../sla-progress-bar/sla-progress-bar.component';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

/**
 * SLA Info Panel Component
 * Comprehensive SLA information display with real-time updates
 * Shows response and resolution progress, deadlines, and explanations
 */
@Component({
  selector: 'app-sla-info-panel',
  standalone: true,
  imports: [CommonModule, SLABadgeComponent, SLAProgressBarComponent, UtcToLocalPipe],
  templateUrl: './sla-info-panel.component.html',
  styleUrls: ['./sla-info-panel.component.scss']
})
export class SLAInfoPanelComponent implements OnInit, OnDestroy {
  private slaService = inject(SLAService);
  private destroy$ = new Subject<void>();

  /** Complaint ID to fetch SLA status for */
  @Input() complaintId: string = '';

  /** Show detailed explanation text */
  @Input() showExplanation: boolean = true;

  /** View mode determines explanation content */
  @Input() viewMode: 'user' | 'handler' = 'user';

  /** Auto-refresh interval in seconds (0 to disable) */
  @Input() autoRefreshInterval: number = 60;

  /** Show response SLA section */
  @Input() showResponse: boolean = true;

  /** Show resolution SLA section */
  @Input() showResolution: boolean = true;

  /** Compact mode */
  @Input() compact: boolean = false;

  /** SLA status data */
  slaStatus: SLAStatusDisplay | null = null;

  /** Loading state */
  loading: boolean = false;

  /** Error state */
  error: string | null = null;

  ngOnInit(): void {
    if (this.complaintId) {
      this.loadSLAStatus();

      // Set up auto-refresh if enabled
      if (this.autoRefreshInterval > 0) {
        interval(this.autoRefreshInterval * 1000)
          .pipe(
            startWith(0),
            switchMap(() => this.slaService.getSLAStatus(this.complaintId)),
            takeUntil(this.destroy$)
          )
          .subscribe({
            next: (response) => {
              if (response.isSuccess && response.data) {
                this.slaStatus = response.data;
                this.error = null;
              }
            },
            error: (err) => {
              console.error('Failed to fetch SLA status:', err);
              this.error = 'Failed to load SLA information';
            }
          });
      }
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load SLA status from API
   */
  loadSLAStatus(): void {
    this.loading = true;
    this.error = null;

    this.slaService.getSLAStatus(this.complaintId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.loading = false;
          if (response.isSuccess && response.data) {
            this.slaStatus = response.data;
          } else {
            this.error = response.message || 'Failed to load SLA information';
          }
        },
        error: (err) => {
          this.loading = false;
          this.error = 'Failed to load SLA information';
          console.error('Error loading SLA status:', err);
        }
      });
  }

  /**
   * Format minutes to human-readable time
   */
  formatTime(minutes: number): string {
    return this.slaService.formatMinutes(minutes);
  }

  /**
   * Get response urgency level
   */
  getResponseUrgency(): UrgencyLevel {
    if (!this.slaStatus?.response) return 'green';
    return this.slaService.getUrgencyLevel(this.slaStatus.response.percentComplete);
  }

  /**
   * Get explanation text based on view mode and SLA status
   * CRITICAL FIX: Added null safety checks for nested properties
   */
  getExplanationText(): string {
    if (!this.slaStatus || !this.slaStatus.slaLevel || !this.slaStatus.resolution) {
      return '';
    }

    const urgency = this.slaStatus.urgencyLevel || 'green';
    const slaName = this.slaStatus.slaLevel.name || 'Standard';
    const remainingTime = this.formatTime(this.slaStatus.resolution.remainingMinutes || 0);

    if (this.viewMode === 'handler') {
      // Handler view - action-oriented
      switch (urgency) {
        case 'green':
          return `This complaint is on track to meet the ${slaName} SLA deadline. Continue normal processing.`;
        case 'yellow':
          return `⚠️ This complaint is approaching its SLA deadline. ${remainingTime} remaining. Consider prioritizing this ticket.`;
        case 'orange':
          return `🔥 URGENT: This complaint is nearing SLA breach! ${remainingTime} remaining. Immediate attention required.`;
        case 'red':
          return `🚨 CRITICAL: This complaint has breached its SLA deadline! Escalation may be triggered. Resolve immediately.`;
      }
    } else {
      // User view - informational
      switch (urgency) {
        case 'green':
          return `Your complaint is being processed according to our ${slaName} service level agreement. We aim to resolve it within the committed timeframe.`;
        case 'yellow':
          return `Your complaint is progressing. Our team is working to resolve it within the ${slaName} SLA timeframe (${remainingTime} remaining).`;
        case 'orange':
          return `We understand your complaint is urgent. Our team is prioritizing it to resolve within the committed ${slaName} SLA timeframe.`;
        case 'red':
          return `We apologize for the delay. Your complaint has exceeded our standard ${slaName} resolution timeframe. A manager has been notified and will ensure prompt resolution.`;
      }
    }

    return '';
  }

  /**
   * Get container CSS classes
   */
  get containerClass(): string {
    const classes = ['sla-info-panel'];
    if (this.compact) classes.push('sla-info-panel-compact');
    if (this.slaStatus) classes.push(`sla-panel-${this.slaStatus.urgencyLevel}`);
    return classes.join(' ');
  }

  /**
   * Check if response SLA is met
   */
  get isResponseMet(): boolean {
    return this.slaStatus?.response?.status === 'met';
  }

  /**
   * Check if resolution is complete
   */
  get isResolutionComplete(): boolean {
    return !!this.slaStatus?.resolution?.resolvedDate;
  }

  /**
   * Get pause indicator text
   */
  get pauseText(): string {
    if (!this.slaStatus?.isPaused) return '';
    const reason = this.slaStatus.pauseReason || 'SLA tracking paused';
    return `⏸️ ${reason}`;
  }
}
