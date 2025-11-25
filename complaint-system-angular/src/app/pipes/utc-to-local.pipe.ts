import { Pipe, PipeTransform } from '@angular/core';
import { DateService } from '../services/date.service';

/**
 * Converts UTC dates from the API to user's configured timezone for display
 * Uses DateService which respects user's timezone preferences
 *
 * Usage: {{ utcDate | utcToLocal }}
 * Usage: {{ utcDate | utcToLocal:'relative' }}
 *
 * Formats:
 * - 'default' or omitted: Full date format (DD/MM/YYYY, hh:mm AM/PM)
 * - 'relative': Relative time (e.g., "2 hours ago", "just now")
 * - 'short': Short date format
 * - 'medium': Medium date format with seconds
 *
 * @example
 * <p>Created: {{ complaint.createdAt | utcToLocal }}</p>
 * <p>Updated: {{ complaint.updatedAt | utcToLocal:'relative' }}</p>
 */
@Pipe({
  name: 'utcToLocal',
  standalone: true
})
export class UtcToLocalPipe implements PipeTransform {
  constructor(private dateService: DateService) {}

  transform(value: string | Date | null | undefined, format: string = 'default'): string | null {
    if (!value) {
      return null;
    }

    try {
      // Convert to string if it's a Date object
      const dateString = typeof value === 'string' ? value : value.toISOString();

      // Use DateService for timezone-aware formatting
      switch (format) {
        case 'relative':
          return this.dateService.getRelativeTime(dateString);

        case 'short':
          return this.dateService.formatDate(dateString, false);

        case 'medium':
          return this.dateService.formatDate(dateString, true);

        case 'default':
        default:
          return this.dateService.formatDate(dateString);
      }
    } catch (error) {
      console.error('Error converting UTC date to local time:', error);
      return null;
    }
  }
}
