import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import {
  EmailServerSettings,
  CreateEmailServerSettingsRequest,
  UpdateEmailServerSettingsRequest
} from '../models/communication.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class EmailSettingsService {
  // Use getter to always get the current runtimeConfig value (loaded from config.json)
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/email-settings`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all email server settings
   * @param includeInactive Include inactive settings
   */
  getEmailSettings(includeInactive: boolean = false): Observable<EmailServerSettings[]> {
    const params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    return this.http.get<ApiResponse<EmailServerSettings[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single email server setting by ID
   * @param id The email setting ID
   */
  getEmailSettingById(id: string): Observable<EmailServerSettings> {
    return this.http.get<ApiResponse<EmailServerSettings>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Get the default email server setting
   */
  getDefaultEmailSetting(): Observable<EmailServerSettings | null> {
    return this.http.get<ApiResponse<EmailServerSettings>>(`${this.apiUrl}/default`)
      .pipe(map(response => response.data || null));
  }

  /**
   * Create a new email server setting
   * @param request The email setting creation request
   */
  createEmailSetting(request: CreateEmailServerSettingsRequest): Observable<ApiResponse<EmailServerSettings>> {
    return this.http.post<ApiResponse<EmailServerSettings>>(this.apiUrl, request);
  }

  /**
   * Update an existing email server setting
   * @param id The email setting ID
   * @param request The email setting update request
   */
  updateEmailSetting(id: string, request: UpdateEmailServerSettingsRequest): Observable<ApiResponse<EmailServerSettings>> {
    return this.http.put<ApiResponse<EmailServerSettings>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete an email server setting
   * @param id The email setting ID
   */
  deleteEmailSetting(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Test an email server setting
   * @param id The email setting ID
   * @param testEmail Email address to send test email to
   */
  testEmailSetting(id: string, testEmail: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/test`, { testRecipient: testEmail });
  }

  /**
   * Set an email server setting as default
   * @param id The email setting ID
   */
  setAsDefault(id: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/set-default`, {});
  }
}
