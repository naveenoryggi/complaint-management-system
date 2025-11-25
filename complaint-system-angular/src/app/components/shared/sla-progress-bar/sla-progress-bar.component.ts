import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UrgencyLevel } from '../../../services/sla.service';

/**
 * SLA Progress Bar Component
 * Displays progress toward SLA deadline with color-coded urgency
 * Shows elapsed time, remaining time, and percentage complete
 */
@Component({
  selector: 'app-sla-progress-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sla-progress-bar.component.html',
  styleUrls: ['./sla-progress-bar.component.scss']
})
export class SLAProgressBarComponent {
  /** Label for the progress bar (e.g., "Response Time", "Resolution Progress") */
  @Input() label: string = 'Resolution Progress';

  /** Percentage complete (0-100) */
  @Input() percentComplete: number = 0;

  /** Elapsed time display string */
  @Input() elapsedTime: string = '';

  /** Remaining time display string */
  @Input() remainingTime: string = '';

  /** Urgency level determines color */
  @Input() urgency: UrgencyLevel = 'green';

  /** Show header with label and percentage */
  @Input() showHeader: boolean = true;

  /** Show footer with time details */
  @Input() showFooter: boolean = true;

  /** Compact mode (smaller size) */
  @Input() compact: boolean = false;

  /** Animated progress bar */
  @Input() animated: boolean = true;

  /** Striped pattern */
  @Input() striped: boolean = false;

  /**
   * Get CSS class for progress bar based on urgency
   */
  get progressClass(): string {
    const classes = [`progress-${this.urgency}`];
    if (this.animated) classes.push('progress-bar-animated');
    if (this.striped) classes.push('progress-bar-striped');
    return classes.join(' ');
  }

  /**
   * Get container CSS classes
   */
  get containerClass(): string {
    const classes = ['sla-progress-container'];
    if (this.compact) classes.push('sla-progress-compact');
    return classes.join(' ');
  }

  /**
   * Get clamped percentage (0-100)
   */
  get safePercentComplete(): number {
    return Math.min(100, Math.max(0, this.percentComplete));
  }

  /**
   * Get urgency label
   */
  get urgencyLabel(): string {
    const labels: Record<UrgencyLevel, string> = {
      'green': 'On Track',
      'yellow': 'Warning',
      'orange': 'Urgent',
      'red': 'Breached'
    };
    return labels[this.urgency];
  }

  /**
   * Get percentage display class
   */
  get percentageClass(): string {
    return `sla-percentage sla-percentage-${this.urgency}`;
  }
}
