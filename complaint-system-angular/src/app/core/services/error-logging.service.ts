import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { Router } from '@angular/router';

export interface ErrorLog {
  id: string;
  timestamp: Date;
  type: 'HTTP_ERROR' | 'JAVASCRIPT_ERROR' | 'UNHANDLED_REJECTION' | 'VALIDATION_ERROR' | 'BUSINESS_ERROR';
  severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  message: string;
  stack?: string;
  url?: string;
  statusCode?: number;
  method?: string;
  user?: string;
  userAgent: string;
  errorDetails?: any;
  resolved: boolean;
  retryCount?: number;
  resolvedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorLoggingService {
  private readonly LOG_KEY = 'app_error_logs';
  private readonly MAX_LOGS = 100; // Keep last 100 errors in localStorage

  constructor(private http: HttpClient, private router: Router) {
    // Listen for unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      this.logError({
        id: this.generateErrorId(),
        timestamp: new Date(),
        type: 'UNHANDLED_REJECTION',
        severity: 'HIGH',
        message: event.reason?.message || 'Unhandled Promise Rejection',
        stack: event.reason?.stack,
        userAgent: navigator.userAgent,
        resolved: false
      });
    });

    // Listen for global JavaScript errors
    window.addEventListener('error', (event) => {
      this.logError({
        id: this.generateErrorId(),
        timestamp: new Date(),
        type: 'JAVASCRIPT_ERROR',
        severity: 'HIGH',
        message: event.message || 'JavaScript Error',
        stack: event.error?.stack,
        url: event.filename,
        userAgent: navigator.userAgent,
        resolved: false
      });
    });
  }

  /**
   * Log HTTP error to localStorage and console
   */
  logHttpError(error: HttpErrorResponse, method: string, url: string): void {
    const severity = this.determineSeverity(error.status);
    const errorLog: ErrorLog = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      type: 'HTTP_ERROR',
      severity,
      message: error.message || 'HTTP Error',
      url: url,
      statusCode: error.status,
      method: method,
      userAgent: navigator.userAgent,
      errorDetails: {
        statusText: error.statusText,
        error: error.error
      },
      resolved: false
    };

    // Handle specific HTTP error types
    this.handleHttpErrorTypes(error);

    this.logError(errorLog);
  }

  /**
   * Log validation error
   */
  logValidationError(message: string, context?: any): void {
    const errorLog: ErrorLog = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      type: 'VALIDATION_ERROR',
      severity: 'MEDIUM',
      message,
      url: this.router.url,
      userAgent: navigator.userAgent,
      errorDetails: context,
      resolved: false
    };

    this.logError(errorLog);
  }

  /**
   * Log business logic error
   */
  logBusinessError(message: string, severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL' = 'MEDIUM', context?: any): void {
    const errorLog: ErrorLog = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      type: 'BUSINESS_ERROR',
      severity,
      message,
      url: this.router.url,
      userAgent: navigator.userAgent,
      errorDetails: context,
      resolved: false
    };

    this.logError(errorLog);
  }

  /**
   * Handle specific HTTP error types with user-friendly actions
   */
  private handleHttpErrorTypes(error: HttpErrorResponse): void {
    switch (error.status) {
      case 401:
        // Session expired - redirect to login
        setTimeout(() => {
          this.router.navigate(['/login'], {
            queryParams: {
              error: 'session_expired',
              message: encodeURIComponent('Your session has expired. Please log in again.')
            }
          });
        }, 1000);
        break;

      case 403:
        // Access denied - log warning
        console.warn('Access denied for URL:', error.url);
        break;

      case 429:
        // Rate limited - log warning with retry info
        const retryAfter = error.headers?.get('Retry-After');
        console.warn(`Rate limited. Retry after ${retryAfter || '60'} seconds.`, error.url);
        break;

      case 500:
      case 502:
      case 503:
      case 504:
        // Server errors - could trigger retry logic
        console.error(`Server error (${error.status}):`, error.message);
        break;
    }
  }

  /**
   * Determine error severity based on HTTP status
   */
  private determineSeverity(status?: number): 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL' {
    if (!status) return 'MEDIUM';

    if (status >= 500) return 'CRITICAL';
    if (status === 401 || status === 403) return 'HIGH';
    if (status >= 400) return 'MEDIUM';

    return 'LOW';
  }

  /**
   * Generate unique error ID
   */
  private generateErrorId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Log JavaScript error to localStorage and console
   */
  logJavaScriptError(error: Error, additionalInfo?: any): void {
    const errorLog: ErrorLog = {
      id: this.generateErrorId(),
      timestamp: new Date(),
      type: 'JAVASCRIPT_ERROR',
      severity: 'HIGH',
      message: error.message || 'JavaScript Error',
      stack: error.stack,
      url: this.router.url,
      userAgent: navigator.userAgent,
      errorDetails: additionalInfo,
      resolved: false
    };

    this.logError(errorLog);
  }

  /**
   * Generic log error method
   */
  private logError(errorLog: ErrorLog): void {
    try {
      // Log to console with styling
      console.group(`%c🔴 ERROR - ${errorLog.type}`, 'color: red; font-weight: bold; font-size: 14px;');
      console.log('%cTimestamp:', 'font-weight: bold', errorLog.timestamp);
      console.log('%cMessage:', 'font-weight: bold', errorLog.message);

      if (errorLog.url) {
        console.log('%cURL:', 'font-weight: bold', `${errorLog.method} ${errorLog.url}`);
      }

      if (errorLog.statusCode) {
        console.log('%cStatus Code:', 'font-weight: bold', errorLog.statusCode);
      }

      if (errorLog.stack) {
        console.log('%cStack Trace:', 'font-weight: bold');
        console.log(errorLog.stack);
      }

      if (errorLog.errorDetails) {
        console.log('%cError Details:', 'font-weight: bold');
        console.log(errorLog.errorDetails);
      }

      console.groupEnd();

      // Store in localStorage
      this.storeErrorInLocalStorage(errorLog);

    } catch (e) {
      console.error('Failed to log error:', e);
    }
  }

  /**
   * Store error in localStorage with rotation
   */
  private storeErrorInLocalStorage(errorLog: ErrorLog): void {
    try {
      const logs = this.getStoredErrors();
      logs.unshift(errorLog);

      // Keep only MAX_LOGS entries
      if (logs.length > this.MAX_LOGS) {
        logs.splice(this.MAX_LOGS);
      }

      localStorage.setItem(this.LOG_KEY, JSON.stringify(logs));
    } catch (e) {
      console.error('Failed to store error in localStorage:', e);
    }
  }

  /**
   * Get all stored errors from localStorage
   */
  getStoredErrors(): ErrorLog[] {
    try {
      const logsJson = localStorage.getItem(this.LOG_KEY);
      return logsJson ? JSON.parse(logsJson) : [];
    } catch (e) {
      console.error('Failed to retrieve errors from localStorage:', e);
      return [];
    }
  }

  /**
   * Clear all stored errors
   */
  clearStoredErrors(): void {
    try {
      localStorage.removeItem(this.LOG_KEY);
      console.log('✅ All stored errors cleared');
    } catch (e) {
      console.error('Failed to clear stored errors:', e);
    }
  }

  /**
   * Export errors as JSON file for download
   */
  exportErrors(): void {
    try {
      const logs = this.getStoredErrors();
      const dataStr = JSON.stringify(logs, null, 2);
      const dataBlob = new Blob([dataStr], { type: 'application/json' });

      const url = window.URL.createObjectURL(dataBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `error-logs-${new Date().toISOString().slice(0, 10)}.json`;
      link.click();

      window.URL.revokeObjectURL(url);
      console.log('✅ Errors exported successfully');
    } catch (e) {
      console.error('Failed to export errors:', e);
    }
  }

  /**
   * Get error statistics
   */
  getErrorStats(): {
    total: number;
    byType: Record<string, number>;
    bySeverity: Record<string, number>;
    last24Hours: number;
    resolved: number;
    unresolved: number;
  } {
    const logs = this.getStoredErrors();
    const now = new Date().getTime();
    const twentyFourHoursAgo = now - (24 * 60 * 60 * 1000);

    const byType: Record<string, number> = {};
    const bySeverity: Record<string, number> = {};
    let last24Hours = 0;
    let resolved = 0;
    let unresolved = 0;

    logs.forEach(log => {
      const logTime = new Date(log.timestamp).getTime();

      // Count by type
      byType[log.type] = (byType[log.type] || 0) + 1;

      // Count by severity
      bySeverity[log.severity] = (bySeverity[log.severity] || 0) + 1;

      // Count last 24 hours
      if (logTime >= twentyFourHoursAgo) {
        last24Hours++;
      }

      // Count resolved/unresolved
      if (log.resolved) {
        resolved++;
      } else {
        unresolved++;
      }
    });

    return {
      total: logs.length,
      byType,
      bySeverity,
      last24Hours,
      resolved,
      unresolved
    };
  }

  /**
   * Mark error as resolved
   */
  resolveError(errorId: string): void {
    const logs = this.getStoredErrors();
    const errorIndex = logs.findIndex(log => log.id === errorId);

    if (errorIndex !== -1) {
      logs[errorIndex].resolved = true;
      logs[errorIndex].resolvedAt = new Date();
      this.updateStoredErrors(logs);
      console.log(`✅ Error ${errorId} marked as resolved`);
    }
  }

  /**
   * Resolve multiple errors by type
   */
  resolveErrorsByType(type: string): void {
    const logs = this.getStoredErrors();
    let resolvedCount = 0;

    logs.forEach(log => {
      if (log.type === type && !log.resolved) {
        log.resolved = true;
        log.resolvedAt = new Date();
        resolvedCount++;
      }
    });

    if (resolvedCount > 0) {
      this.updateStoredErrors(logs);
      console.log(`✅ Resolved ${resolvedCount} errors of type ${type}`);
    }
  }

  /**
   * Get critical errors
   */
  getCriticalErrors(): ErrorLog[] {
    return this.getStoredErrors().filter(log =>
      log.severity === 'CRITICAL' && !log.resolved
    );
  }

  /**
   * Get recent errors (last hour)
   */
  getRecentErrors(hours: number = 1): ErrorLog[] {
    const now = new Date().getTime();
    const hoursAgo = now - (hours * 60 * 60 * 1000);

    return this.getStoredErrors().filter(log =>
      new Date(log.timestamp).getTime() >= hoursAgo
    );
  }

  /**
   * Add retry attempt to error
   */
  addRetryAttempt(errorId: string): void {
    const logs = this.getStoredErrors();
    const errorIndex = logs.findIndex(log => log.id === errorId);

    if (errorIndex !== -1) {
      logs[errorIndex].retryCount = (logs[errorIndex].retryCount || 0) + 1;
      this.updateStoredErrors(logs);
    }
  }

  /**
   * Check if error should be retried
   */
  shouldRetryError(errorId: string, maxRetries: number = 3): boolean {
    const logs = this.getStoredErrors();
    const error = logs.find(log => log.id === errorId);

    if (!error) return false;
    if (error.resolved) return false;
    if ((error.retryCount || 0) >= maxRetries) return false;

    // Don't retry validation or business errors
    if (error.type === 'VALIDATION_ERROR' || error.type === 'BUSINESS_ERROR') {
      return false;
    }

    return true;
  }

  /**
   * Update stored errors (helper method)
   */
  private updateStoredErrors(logs: ErrorLog[]): void {
    try {
      localStorage.setItem(this.LOG_KEY, JSON.stringify(logs));
    } catch (e) {
      console.error('Failed to update stored errors:', e);
    }
  }

  /**
   * Print error summary to console
   */
  printErrorSummary(): void {
    const stats = this.getErrorStats();
    const logs = this.getStoredErrors();

    console.group('%c📊 ERROR SUMMARY', 'color: blue; font-weight: bold; font-size: 16px;');
    console.log('%cTotal Errors:', 'font-weight: bold', stats.total);
    console.log('%cLast 24 Hours:', 'font-weight: bold', stats.last24Hours);
    console.log('%cBy Type:', 'font-weight: bold');
    console.table(stats.byType);

    if (logs.length > 0) {
      console.log('%cMost Recent Errors:', 'font-weight: bold');
      console.table(logs.slice(0, 10).map(log => ({
        Time: new Date(log.timestamp).toLocaleString(),
        Type: log.type,
        Message: log.message,
        Status: log.statusCode || 'N/A',
        URL: log.url || 'N/A'
      })));
    }

    console.log('\n%cCommands:', 'font-weight: bold; color: green');
    console.log('  • window.clearErrors()  - Clear all stored errors');
    console.log('  • window.exportErrors() - Download errors as JSON');
    console.log('  • window.showErrors()   - Show this summary');

    console.groupEnd();
  }
}

// Make functions available globally for console access
if (typeof window !== 'undefined') {
  (window as any).showErrors = () => {
    // Create a mock router for global access
    const mockRouter = {
      url: '/unknown'
    };
    const service = new ErrorLoggingService({} as any, mockRouter as any);
    service.printErrorSummary();
  };

  (window as any).clearErrors = () => {
    const mockRouter = {
      url: '/unknown'
    };
    const service = new ErrorLoggingService({} as any, mockRouter as any);
    service.clearStoredErrors();
  };

  (window as any).exportErrors = () => {
    const mockRouter = {
      url: '/unknown'
    };
    const service = new ErrorLoggingService({} as any, mockRouter as any);
    service.exportErrors();
  };
}
