import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UrgencyLevel } from '../../../services/sla.service';

/**
 * SLA Badge Component
 * Displays a compact SLA indicator with urgency color-coding
 * Used in complaint cards, lists, and dashboards
 */
@Component({
  selector: 'app-sla-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sla-badge.component.html',
  styleUrls: ['./sla-badge.component.scss']
})
export class SLABadgeComponent {
  /** SLA level name (e.g., "Gold", "Silver", "Bronze") */
  @Input() slaLevel: string = '';

  /** Urgency level determines color scheme */
  @Input() urgency: UrgencyLevel = 'green';

  /** Time remaining display string (e.g., "4h 30m", "Overdue") */
  @Input() remainingTime: string = '';

  /** Whether to show time remaining */
  @Input() showTime: boolean = true;

  /** Show icon */
  @Input() showIcon: boolean = true;

  /** Compact mode (smaller size) */
  @Input() compact: boolean = false;

  /** Custom tooltip */
  @Input() customTooltip?: string;

  /**
   * Get CSS class for badge based on urgency
   */
  get badgeClass(): string {
    const classes = ['sla-badge', `sla-${this.urgency}`];
    if (this.compact) classes.push('sla-badge-compact');
    return classes.join(' ');
  }

  /**
   * Get time display string
   */
  get timeDisplay(): string {
    if (!this.showTime || !this.remainingTime) return '';
    return this.remainingTime;
  }

  /**
   * Get tooltip text
   */
  get tooltip(): string {
    if (this.customTooltip) return this.customTooltip;

    const urgencyLabels: Record<UrgencyLevel, string> = {
      'green': 'On Track',
      'yellow': 'Warning',
      'orange': 'Urgent',
      'red': 'Breached'
    };

    const urgencyLabel = urgencyLabels[this.urgency];
    const slaText = this.slaLevel ? `${this.slaLevel} SLA` : 'SLA';
    const timeText = this.remainingTime ? ` - ${this.remainingTime} remaining` : '';

    return `${slaText}: ${urgencyLabel}${timeText}`;
  }

  /**
   * Get icon class based on urgency
   */
  get iconClass(): string {
    const icons: Record<UrgencyLevel, string> = {
      'green': 'bi-check-circle-fill',
      'yellow': 'bi-exclamation-triangle-fill',
      'orange': 'bi-hourglass-split',
      'red': 'bi-x-circle-fill'
    };
    return icons[this.urgency];
  }
}
