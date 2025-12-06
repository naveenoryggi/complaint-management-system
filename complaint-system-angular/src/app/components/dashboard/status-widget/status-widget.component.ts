import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatusWidget } from '../../../models/dashboard.model';

@Component({
  selector: 'app-status-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-widget.component.html',
  styleUrl: './status-widget.component.scss'
})
export class StatusWidgetComponent {
  @Input() widget!: StatusWidget;
  @Input() showTrends: boolean = true;
  @Input() showPercentages: boolean = true;

  // Default Font Awesome icons for common status names
  private readonly statusIconMap: Record<string, string> = {
    'ticket received': 'fa-ticket',
    'submitted': 'fa-paper-plane',
    'under review': 'fa-magnifying-glass',
    'in progress': 'fa-spinner',
    'escalated': 'fa-arrow-up-right-from-square',
    'pending info': 'fa-hourglass-half',
    'resolved': 'fa-circle-check',
    'closed': 'fa-lock',
    'rejected': 'fa-circle-xmark',
    'reopened': 'fa-rotate-left',
    'open': 'fa-folder-open',
    'new': 'fa-plus-circle',
    'pending': 'fa-clock',
    'completed': 'fa-check-circle',
    'cancelled': 'fa-ban',
    'on hold': 'fa-pause-circle'
  };

  // Bootstrap Icons to Font Awesome mapping
  private readonly biToFaMap: Record<string, string> = {
    'bi-ticket-perforated': 'fa-ticket',
    'bi-send': 'fa-paper-plane',
    'bi-search': 'fa-magnifying-glass',
    'bi-gear': 'fa-spinner',
    'bi-arrow-up-right-square': 'fa-arrow-up-right-from-square',
    'bi-hourglass-split': 'fa-hourglass-half',
    'bi-check-circle': 'fa-circle-check',
    'bi-lock': 'fa-lock',
    'bi-x-circle': 'fa-circle-xmark',
    'bi-arrow-counterclockwise': 'fa-rotate-left',
    'bi-folder2-open': 'fa-folder-open',
    'bi-plus-circle': 'fa-plus-circle',
    'bi-clock': 'fa-clock',
    'bi-check2-circle': 'fa-check-circle',
    'bi-slash-circle': 'fa-ban',
    'bi-pause-circle': 'fa-pause-circle'
  };

  /**
   * Get the Font Awesome icon class for the widget
   * - Converts Bootstrap Icons to Font Awesome
   * - Provides default icons based on status name if none exists
   */
  getWidgetIcon(): string {
    const iconClass = this.widget.iconClass?.trim();

    // If iconClass exists, check if it's a Bootstrap Icon and convert
    if (iconClass) {
      // Check if it's a Bootstrap Icon (bi-*)
      if (iconClass.startsWith('bi-')) {
        return this.biToFaMap[iconClass] || this.getDefaultIconByName();
      }
      // Check if it already has fa- prefix
      if (iconClass.startsWith('fa-')) {
        return iconClass;
      }
      // Otherwise return as-is
      return iconClass;
    }

    // No iconClass provided, get default based on status name
    return this.getDefaultIconByName();
  }

  private getDefaultIconByName(): string {
    const statusName = this.widget.name?.toLowerCase().trim();
    return this.statusIconMap[statusName] || 'fa-circle';
  }

  getTrendIcon(): string {
    switch (this.widget.trend) {
      case 'up':
        return 'fa-arrow-up';
      case 'down':
        return 'fa-arrow-down';
      default:
        return 'fa-minus';
    }
  }

  getTrendClass(): string {
    switch (this.widget.trend) {
      case 'up':
        return 'trend-up';
      case 'down':
        return 'trend-down';
      default:
        return 'trend-stable';
    }
  }

  getFormattedPercentage(): string {
    const sign = this.widget.percentageChange > 0 ? '+' : '';
    return `${sign}${this.widget.percentageChange.toFixed(1)}%`;
  }

  getFormattedAverageTime(): string {
    if (!this.widget.averageTimeInStatus) {
      return 'N/A';
    }

    const hours = this.widget.averageTimeInStatus;
    if (hours < 1) {
      return `${Math.round(hours * 60)}m`;
    } else if (hours < 24) {
      return `${hours.toFixed(1)}h`;
    } else {
      const days = Math.floor(hours / 24);
      const remainingHours = Math.round(hours % 24);
      return remainingHours > 0 ? `${days}d ${remainingHours}h` : `${days}d`;
    }
  }
}
