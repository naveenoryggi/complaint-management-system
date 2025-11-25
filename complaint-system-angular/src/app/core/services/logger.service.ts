import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export enum LogLevel {
  Debug = 0,
  Info = 1,
  Warn = 2,
  Error = 3,
  None = 4
}

export interface LogEntry {
  timestamp: Date;
  level: LogLevel;
  message: string;
  data?: any;
  source?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LoggerService {
  private readonly LOG_KEY = 'app_logs';
  private readonly MAX_LOGS = 500;
  private currentLogLevel: LogLevel = environment.production ? LogLevel.Warn : LogLevel.Debug;

  constructor() {}

  /**
   * Set the current log level
   */
  setLogLevel(level: LogLevel): void {
    this.currentLogLevel = level;
  }

  /**
   * Log debug message
   */
  debug(message: string, data?: any, source?: string): void {
    this.log(LogLevel.Debug, message, data, source);
  }

  /**
   * Log info message
   */
  info(message: string, data?: any, source?: string): void {
    this.log(LogLevel.Info, message, data, source);
  }

  /**
   * Log warning message
   */
  warn(message: string, data?: any, source?: string): void {
    this.log(LogLevel.Warn, message, data, source);
  }

  /**
   * Log error message
   */
  error(message: string, data?: any, source?: string): void {
    this.log(LogLevel.Error, message, data, source);
  }

  /**
   * Core logging method
   */
  private log(level: LogLevel, message: string, data?: any, source?: string): void {
    // Don't log if level is below current threshold
    if (level < this.currentLogLevel) {
      return;
    }

    const logEntry: LogEntry = {
      timestamp: new Date(),
      level,
      message,
      data,
      source
    };

    // Log to console with appropriate styling
    this.logToConsole(logEntry);

    // Store in localStorage for non-production or error/warn levels
    if (!environment.production || level >= LogLevel.Warn) {
      this.storeLog(logEntry);
    }
  }

  /**
   * Log to console with styling
   */
  private logToConsole(entry: LogEntry): void {
    const timestamp = entry.timestamp.toISOString();
    const sourceStr = entry.source ? `[${entry.source}]` : '';
    const prefix = `${timestamp} ${sourceStr}`;

    switch (entry.level) {
      case LogLevel.Debug:
        console.debug(`%c${prefix} DEBUG:`, 'color: gray', entry.message, entry.data || '');
        break;
      case LogLevel.Info:
        console.info(`%c${prefix} INFO:`, 'color: blue', entry.message, entry.data || '');
        break;
      case LogLevel.Warn:
        console.warn(`%c${prefix} WARN:`, 'color: orange; font-weight: bold', entry.message, entry.data || '');
        break;
      case LogLevel.Error:
        console.error(`%c${prefix} ERROR:`, 'color: red; font-weight: bold', entry.message, entry.data || '');
        break;
    }
  }

  /**
   * Store log in localStorage
   */
  private storeLog(entry: LogEntry): void {
    try {
      const logs = this.getStoredLogs();
      logs.unshift(entry);

      // Keep only MAX_LOGS entries
      if (logs.length > this.MAX_LOGS) {
        logs.splice(this.MAX_LOGS);
      }

      localStorage.setItem(this.LOG_KEY, JSON.stringify(logs));
    } catch (e) {
      // Silently fail if localStorage is full or unavailable
    }
  }

  /**
   * Get all stored logs
   */
  getStoredLogs(): LogEntry[] {
    try {
      const logsJson = localStorage.getItem(this.LOG_KEY);
      return logsJson ? JSON.parse(logsJson) : [];
    } catch (e) {
      return [];
    }
  }

  /**
   * Clear all stored logs
   */
  clearLogs(): void {
    try {
      localStorage.removeItem(this.LOG_KEY);
      console.log('✅ All logs cleared');
    } catch (e) {
      console.error('Failed to clear logs:', e);
    }
  }

  /**
   * Export logs as JSON
   */
  exportLogs(): void {
    try {
      const logs = this.getStoredLogs();
      const dataStr = JSON.stringify(logs, null, 2);
      const dataBlob = new Blob([dataStr], { type: 'application/json' });

      const url = window.URL.createObjectURL(dataBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `app-logs-${new Date().toISOString().slice(0, 10)}.json`;
      link.click();

      window.URL.revokeObjectURL(url);
      console.log('✅ Logs exported successfully');
    } catch (e) {
      console.error('Failed to export logs:', e);
    }
  }

  /**
   * Get log statistics
   */
  getLogStats(): { total: number; byLevel: Record<string, number>; last24Hours: number } {
    const logs = this.getStoredLogs();
    const now = new Date().getTime();
    const twentyFourHoursAgo = now - (24 * 60 * 60 * 1000);

    const byLevel: Record<string, number> = {};
    let last24Hours = 0;

    logs.forEach(log => {
      const logTime = new Date(log.timestamp).getTime();

      // Count by level
      const levelName = LogLevel[log.level];
      byLevel[levelName] = (byLevel[levelName] || 0) + 1;

      // Count last 24 hours
      if (logTime >= twentyFourHoursAgo) {
        last24Hours++;
      }
    });

    return {
      total: logs.length,
      byLevel,
      last24Hours
    };
  }

  /**
   * Print log summary to console
   */
  printLogSummary(): void {
    const stats = this.getLogStats();
    const logs = this.getStoredLogs();

    console.group('%c📊 LOG SUMMARY', 'color: blue; font-weight: bold; font-size: 16px;');
    console.log('%cTotal Logs:', 'font-weight: bold', stats.total);
    console.log('%cLast 24 Hours:', 'font-weight: bold', stats.last24Hours);
    console.log('%cBy Level:', 'font-weight: bold');
    console.table(stats.byLevel);

    if (logs.length > 0) {
      console.log('%cMost Recent Logs:', 'font-weight: bold');
      console.table(logs.slice(0, 10).map(log => ({
        Time: new Date(log.timestamp).toLocaleString(),
        Level: LogLevel[log.level],
        Source: log.source || 'N/A',
        Message: log.message
      })));
    }

    console.log('\n%cCommands:', 'font-weight: bold; color: green');
    console.log('  • window.showLogs()   - Show this summary');
    console.log('  • window.clearLogs()  - Clear all stored logs');
    console.log('  • window.exportLogs() - Download logs as JSON');

    console.groupEnd();
  }
}

// Make functions available globally for console access
if (typeof window !== 'undefined') {
  (window as any).showLogs = () => {
    const service = new LoggerService();
    service.printLogSummary();
  };

  (window as any).clearLogs = () => {
    const service = new LoggerService();
    service.clearLogs();
  };

  (window as any).exportLogs = () => {
    const service = new LoggerService();
    service.exportLogs();
  };
}
