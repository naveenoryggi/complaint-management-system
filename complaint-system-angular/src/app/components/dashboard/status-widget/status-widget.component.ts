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

  getTrendIcon(): string {
    switch (this.widget.trend) {
      case 'up':
        return 'bi-arrow-up';
      case 'down':
        return 'bi-arrow-down';
      default:
        return 'bi-dash';
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
