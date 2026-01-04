import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { SystemConfiguration } from '../models/system-configuration.model';
import { LoggerService } from '../core/services/logger.service';
import { runtimeConfig } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SystemConfigurationService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/SystemConfiguration`;
  }
  private configSubject = new BehaviorSubject<SystemConfiguration | null>(null);
  public config$ = this.configSubject.asObservable();

  constructor(
    private http: HttpClient,
    private logger: LoggerService
  ) {}

  /**
   * Get system configuration for the current user's company
   */
  getConfiguration(): Observable<SystemConfiguration> {
    this.logger.info('Fetching system configuration');
    return this.http.get<SystemConfiguration>(this.apiUrl).pipe(
      tap(config => {
        this.logger.info('System configuration loaded', config);
        this.configSubject.next(config);
      })
    );
  }

  /**
   * Update system configuration (Admin only)
   */
  updateConfiguration(config: SystemConfiguration): Observable<SystemConfiguration> {
    this.logger.info('Updating system configuration', config);
    return this.http.put<SystemConfiguration>(this.apiUrl, config).pipe(
      tap(updatedConfig => {
        this.logger.info('System configuration updated successfully', updatedConfig);
        this.configSubject.next(updatedConfig);
      })
    );
  }

  /**
   * Reset system configuration to defaults (Admin only)
   */
  resetConfiguration(): Observable<SystemConfiguration> {
    this.logger.info('Resetting system configuration to defaults');
    return this.http.post<SystemConfiguration>(`${this.apiUrl}/reset`, {}).pipe(
      tap(config => {
        this.logger.info('System configuration reset to defaults', config);
        this.configSubject.next(config);
      })
    );
  }

  /**
   * Get current configuration value (synchronous)
   */
  getCurrentConfig(): SystemConfiguration | null {
    return this.configSubject.value;
  }

  /**
   * Get a specific OAuth-related setting by providing the token expiry in hours
   * Returns the recommended refresh interval in minutes
   */
  getRecommendedRefreshInterval(tokenExpiryHours: number): number {
    // For 1-hour tokens, refresh every 30 minutes (half the expiry time)
    // For longer tokens, refresh more frequently but with diminishing returns
    if (tokenExpiryHours <= 1) {
      return 30; // 30 minutes for 1-hour tokens
    } else if (tokenExpiryHours <= 2) {
      return 45; // 45 minutes for 2-hour tokens
    } else if (tokenExpiryHours <= 6) {
      return 60; // 1 hour for 6-hour tokens
    } else {
      return 90; // 90 minutes for longer tokens
    }
  }

  /**
   * Format polling interval for display
   */
  formatPollingInterval(seconds: number): string {
    if (seconds < 60) {
      return `${seconds} seconds`;
    }
    const minutes = Math.floor(seconds / 60);
    return `${minutes} minute${minutes !== 1 ? 's' : ''}`;
  }

  /**
   * Format refresh interval for display
   */
  formatRefreshInterval(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} minute${minutes !== 1 ? 's' : ''}`;
    }
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    if (remainingMinutes === 0) {
      return `${hours} hour${hours !== 1 ? 's' : ''}`;
    }
    return `${hours} hour${hours !== 1 ? 's' : ''} ${remainingMinutes} minute${remainingMinutes !== 1 ? 's' : ''}`;
  }
}
