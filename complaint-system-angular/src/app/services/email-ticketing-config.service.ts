import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  EmailConfiguration,
  CreateEmailConfigurationRequest,
  UpdateEmailConfigurationRequest
} from '../models/communication.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  error?: string;
}

export interface TestConnectionResult {
  success: boolean;
  message: string;
  details?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailTicketingConfigService {
  private readonly http = inject(HttpClient);
  private get baseUrl(): string {
    return `${runtimeConfig.apiUrl}/email-configuration`;
  }

  // State management
  private configurationsSubject = new BehaviorSubject<EmailConfiguration[]>([]);
  public configurations$ = this.configurationsSubject.asObservable();

  private activeConfigSubject = new BehaviorSubject<EmailConfiguration | null>(null);
  public activeConfig$ = this.activeConfigSubject.asObservable();

  // ==========================
  // CRUD Operations
  // ==========================

  /**
   * Get all email configurations for the company
   */
  getConfigurations(includeDeleted: boolean = false): Observable<ApiResponse<EmailConfiguration[]>> {
    const url = includeDeleted
      ? `${this.baseUrl}?includeDeleted=true`
      : this.baseUrl;

    return this.http.get<ApiResponse<EmailConfiguration[]>>(url).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.configurationsSubject.next(response.data);

          // Set active config if one is enabled
          const activeConfig = response.data.find(c => c.isEnabled && !c.isDeleted);
          if (activeConfig) {
            this.activeConfigSubject.next(activeConfig);
          }
        }
      })
    );
  }

  /**
   * Get specific email configuration by ID
   */
  getConfiguration(id: string): Observable<ApiResponse<EmailConfiguration>> {
    return this.http.get<ApiResponse<EmailConfiguration>>(`${this.baseUrl}/${id}`);
  }

  /**
   * Create new email configuration
   */
  createConfiguration(request: CreateEmailConfigurationRequest): Observable<ApiResponse<EmailConfiguration>> {
    return this.http.post<ApiResponse<EmailConfiguration>>(this.baseUrl, request).pipe(
      tap(() => this.getConfigurations().subscribe()) // Refresh list
    );
  }

  /**
   * Update existing email configuration
   */
  updateConfiguration(id: string, request: UpdateEmailConfigurationRequest): Observable<ApiResponse<EmailConfiguration>> {
    return this.http.put<ApiResponse<EmailConfiguration>>(`${this.baseUrl}/${id}`, request).pipe(
      tap(() => this.getConfigurations().subscribe()) // Refresh list
    );
  }

  /**
   * Delete email configuration (soft delete)
   */
  deleteConfiguration(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.getConfigurations().subscribe()) // Refresh list
    );
  }

  // ==========================
  // Connection Testing
  // ==========================

  /**
   * Test IMAP connection
   */
  testImapConnection(id: string): Observable<ApiResponse<TestConnectionResult>> {
    return this.http.post<ApiResponse<TestConnectionResult>>(
      `${this.baseUrl}/${id}/test-imap`,
      {}
    );
  }

  /**
   * Test SMTP connection
   */
  testSmtpConnection(id: string): Observable<ApiResponse<TestConnectionResult>> {
    return this.http.post<ApiResponse<TestConnectionResult>>(
      `${this.baseUrl}/${id}/test-smtp`,
      {}
    );
  }

  /**
   * Test both IMAP and SMTP connections
   */
  testAllConnections(id: string): Observable<{
    imap: ApiResponse<TestConnectionResult>;
    smtp: ApiResponse<TestConnectionResult>;
  }> {
    return new Observable(observer => {
      let imapResult: ApiResponse<TestConnectionResult>;
      let smtpResult: ApiResponse<TestConnectionResult>;
      let completed = 0;

      this.testImapConnection(id).subscribe({
        next: (result) => {
          imapResult = result;
          completed++;
          if (completed === 2) {
            observer.next({ imap: imapResult, smtp: smtpResult });
            observer.complete();
          }
        },
        error: (err) => observer.error(err)
      });

      this.testSmtpConnection(id).subscribe({
        next: (result) => {
          smtpResult = result;
          completed++;
          if (completed === 2) {
            observer.next({ imap: imapResult, smtp: smtpResult });
            observer.complete();
          }
        },
        error: (err) => observer.error(err)
      });
    });
  }

  // ==========================
  // Email Polling
  // ==========================

  /**
   * Manually trigger email polling for a configuration
   */
  pollEmailsNow(id: string): Observable<ApiResponse<{
    totalEmailsFetched: number;
    newTicketsCreated: number;
    existingTicketsUpdated: number;
    errors: string[];
  }>> {
    return this.http.post<ApiResponse<{
      totalEmailsFetched: number;
      newTicketsCreated: number;
      existingTicketsUpdated: number;
      errors: string[];
    }>>(`${this.baseUrl}/${id}/poll-now`, {});
  }

  /**
   * Enable/disable email configuration
   */
  toggleConfiguration(id: string, enabled: boolean): Observable<ApiResponse<EmailConfiguration>> {
    return this.http.patch<ApiResponse<EmailConfiguration>>(
      `${this.baseUrl}/${id}/toggle`,
      { isEnabled: enabled }
    ).pipe(
      tap(() => this.getConfigurations().subscribe()) // Refresh list
    );
  }

  // ==========================
  // Helper Methods
  // ==========================

  /**
   * Get the currently active (enabled) configuration
   */
  getActiveConfiguration(): EmailConfiguration | null {
    return this.activeConfigSubject.value;
  }

  /**
   * Check if email ticketing is configured and enabled
   */
  isEmailTicketingEnabled(): boolean {
    const config = this.getActiveConfiguration();
    return config !== null && config.isEnabled && !config.isDeleted;
  }

  /**
   * Clear cached configurations
   */
  clearCache(): void {
    this.configurationsSubject.next([]);
    this.activeConfigSubject.next(null);
  }

  /**
   * Validate email configuration before saving
   */
  validateConfiguration(config: CreateEmailConfigurationRequest | UpdateEmailConfigurationRequest): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    const isOAuth = config.authenticationType === 1 || config.authenticationType === 2; // OAuth or OAuth2

    // IMAP validation
    if (!config.imapHost || config.imapHost.trim() === '') {
      errors.push('IMAP host is required');
    }
    if (!config.imapPort || config.imapPort < 1 || config.imapPort > 65535) {
      errors.push('IMAP port must be between 1 and 65535');
    }
    if (!config.imapUsername || config.imapUsername.trim() === '') {
      errors.push('IMAP username is required');
    }
    // Password only required for Basic auth
    if (!isOAuth && (!config.imapPassword || config.imapPassword.trim() === '')) {
      errors.push('IMAP password is required');
    }
    if (!config.imapFolder || config.imapFolder.trim() === '') {
      errors.push('IMAP folder is required');
    }

    // SMTP validation
    if (!config.smtpHost || config.smtpHost.trim() === '') {
      errors.push('SMTP host is required');
    }
    if (!config.smtpPort || config.smtpPort < 1 || config.smtpPort > 65535) {
      errors.push('SMTP port must be between 1 and 65535');
    }
    if (!config.smtpUsername || config.smtpUsername.trim() === '') {
      errors.push('SMTP username is required');
    }
    // Password only required for Basic auth
    if (!isOAuth && (!config.smtpPassword || config.smtpPassword.trim() === '')) {
      errors.push('SMTP password is required');
    }

    // Email validation
    if (!config.fromEmail || config.fromEmail.trim() === '') {
      errors.push('From email is required');
    } else if (!this.isValidEmail(config.fromEmail)) {
      errors.push('From email is not valid');
    }
    if (!config.fromName || config.fromName.trim() === '') {
      errors.push('From name is required');
    }

    // Polling interval validation
    if (!config.pollingIntervalMinutes || config.pollingIntervalMinutes < 1) {
      errors.push('Polling interval must be at least 1 minute');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  /**
   * Simple email validation
   */
  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
}
