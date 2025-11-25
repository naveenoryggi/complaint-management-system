import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * Centralized date formatting service for dynamic timezone handling
 * Displays dates in user's timezone (from their preferences or company default)
 * Supports international deployment with per-user timezone resolution
 */
@Injectable({
  providedIn: 'root'
})
export class DateService {
  private readonly locale = 'en-IN';

  /**
   * Get the current user's timezone dynamically
   * Fallback chain: User timezone > Browser timezone > Environment default > Asia/Kolkata
   */
  private getUserTimeZone(): string {
    // Try to get from sessionStorage (AuthService stores it here)
    try {
      const userStr = sessionStorage.getItem('complaint_system_user');
      if (userStr) {
        const user = JSON.parse(userStr);
        if (user?.timeZone) {
          return user.timeZone;
        }
      }
    } catch (error) {
      console.warn('Error reading user timezone from storage:', error);
    }

    // Fallback to browser-detected timezone
    try {
      const browserTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
      if (browserTimeZone) {
        return browserTimeZone;
      }
    } catch (error) {
      console.warn('Error detecting browser timezone:', error);
    }

    // Final fallback to environment or default
    return environment.timezone || 'Asia/Kolkata';
  }

  /**
   * Format a UTC date string to India Standard Time
   * @param dateString - UTC date string from API
   * @param includeSeconds - Whether to include seconds in time display
   * @returns Formatted date string in IST
   */
  formatDate(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    if (!dateString) return '-';

    try {
      // Parse the UTC date string
      // If the date doesn't end with 'Z', it's assumed to be UTC from our API
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      // Check if valid date
      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      // Format using user's timezone
      const options: Intl.DateTimeFormatOptions = {
        timeZone: this.getUserTimeZone(),
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      };

      if (includeSeconds) {
        options.second = '2-digit';
      }

      return utcDate.toLocaleString(this.locale, options);
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Format a UTC date string to short format (e.g., "28 Oct 2025, 3:30 PM")
   * @param dateString - UTC date string from API
   * @returns Short formatted date string in IST
   */
  formatDateShort(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      return utcDate.toLocaleString(this.locale, {
        timeZone: this.getUserTimeZone(),
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Format a UTC date string to time only (e.g., "3:30 PM")
   * @param dateString - UTC date string from API
   * @param includeSeconds - Whether to include seconds
   * @returns Time string in IST
   */
  formatTime(dateString: string | null | undefined, includeSeconds: boolean = false): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      const options: Intl.DateTimeFormatOptions = {
        timeZone: this.getUserTimeZone(),
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      };

      if (includeSeconds) {
        options.second = '2-digit';
      }

      return utcDate.toLocaleTimeString(this.locale, options);
    } catch (error) {
      console.error('Error formatting time:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Format a UTC date string to date only (e.g., "28/10/2025")
   * @param dateString - UTC date string from API
   * @returns Date string in IST
   */
  formatDateOnly(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      return utcDate.toLocaleDateString(this.locale, {
        timeZone: this.getUserTimeZone(),
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Get relative time (e.g., "2 hours ago", "Yesterday")
   * @param dateString - UTC date string from API
   * @returns Relative time string
   */
  getRelativeTime(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      const now = new Date();
      const diffMs = now.getTime() - utcDate.getTime();
      const diffMins = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

      if (diffMins < 1) {
        return 'Just now';
      } else if (diffMins < 60) {
        return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
      } else if (diffHours < 24) {
        return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
      } else if (diffDays === 1) {
        return 'Yesterday';
      } else if (diffDays < 7) {
        return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
      } else {
        // Return formatted date for older dates
        return this.formatDateShort(dateString);
      }
    } catch (error) {
      console.error('Error calculating relative time:', error);
      return 'Invalid Date';
    }
  }

  /**
   * Check if a date is today (in IST timezone)
   * @param dateString - UTC date string from API
   * @returns True if the date is today
   */
  isToday(dateString: string | null | undefined): boolean {
    if (!dateString) return false;

    try {
      const dateStr = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const utcDate = new Date(dateStr);

      if (isNaN(utcDate.getTime())) {
        return false;
      }

      const today = new Date();
      const userTz = this.getUserTimeZone();
      const istDate = new Date(utcDate.toLocaleDateString('en-US', { timeZone: userTz }));
      const istToday = new Date(today.toLocaleDateString('en-US', { timeZone: userTz }));

      return istDate.toDateString() === istToday.toDateString();
    } catch (error) {
      console.error('Error checking if date is today:', error);
      return false;
    }
  }

  /**
   * Get current timestamp in IST format
   * @returns Current date/time string in IST
   */
  getCurrentISTTimestamp(): string {
    return this.formatDate(new Date().toISOString());
  }
}