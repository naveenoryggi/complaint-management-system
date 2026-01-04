import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';

export interface WhatsAppSettings {
  id: string;
  name: string;
  code: string;
  provider: string;
  apiUrl: string;
  businessAccountId: string;
  phoneNumberId: string;
  accessToken: string;
  webhookToken?: string;
  fromNumber: string;
  businessName: string;
  isActive: boolean;
  isDefault: boolean;
  maxMessagesPerHour?: number;
  timeoutSeconds?: number;
  companyId: string;
  additionalConfig?: string;
  testNotes?: string;
  lastTestedAt?: Date;
  mediaStorageType?: string;
  mediaStoragePath?: string;
  mediaStorageConfig?: string;
  mediaPublicBaseUrl?: string;
  mediaRetentionDays?: number;
  maxMediaSizeMB?: number;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
  isDeleted: boolean;
}

export interface CreateWhatsAppRequest {
  name: string;
  code: string;
  provider: string;
  apiUrl: string;
  businessAccountId: string;
  phoneNumberId: string;
  accessToken: string;
  webhookToken?: string;
  fromNumber: string;
  businessName: string;
  isActive: boolean;
  isDefault: boolean;
  maxMessagesPerHour?: number;
  timeoutSeconds?: number;
  additionalConfig?: string;
  testNotes?: string;
  mediaStorageType?: string;
  mediaStoragePath?: string;
  mediaStorageConfig?: string;
  mediaPublicBaseUrl?: string;
  mediaRetentionDays?: number;
  maxMediaSizeMB?: number;
}

export interface UpdateWhatsAppRequest extends CreateWhatsAppRequest {}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class WhatsAppSettingsService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/communication/whatsapp-settings`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all WhatsApp settings
   * @param includeInactive Include inactive settings
   */
  getAll(includeInactive: boolean = false): Observable<ApiResponse<WhatsAppSettings[]>> {
    const params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    return this.http.get<WhatsAppSettings[]>(this.apiUrl, { params })
      .pipe(map(data => ({ isSuccess: true, message: 'Success', data })));
  }

  /**
   * Get a single WhatsApp setting by ID
   * @param id The WhatsApp setting ID
   */
  getById(id: string): Observable<ApiResponse<WhatsAppSettings>> {
    return this.http.get<WhatsAppSettings>(`${this.apiUrl}/${id}`)
      .pipe(map(data => ({ isSuccess: true, message: 'Success', data })));
  }

  /**
   * Create a new WhatsApp setting
   * @param request The WhatsApp setting creation request
   */
  create(request: CreateWhatsAppRequest): Observable<ApiResponse<WhatsAppSettings>> {
    return this.http.post<WhatsAppSettings>(this.apiUrl, request)
      .pipe(map(data => ({ isSuccess: true, message: 'WhatsApp settings created successfully', data })));
  }

  /**
   * Update an existing WhatsApp setting
   * @param id The WhatsApp setting ID
   * @param request The WhatsApp setting update request
   */
  update(id: string, request: UpdateWhatsAppRequest): Observable<ApiResponse<WhatsAppSettings>> {
    return this.http.put<WhatsAppSettings>(`${this.apiUrl}/${id}`, request)
      .pipe(map(data => ({ isSuccess: true, message: 'WhatsApp settings updated successfully', data })));
  }

  /**
   * Delete a WhatsApp setting
   * @param id The WhatsApp setting ID
   */
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(map(() => ({ isSuccess: true, message: 'WhatsApp settings deleted successfully', data: true })));
  }

  /**
   * Test a WhatsApp setting
   * @param id The WhatsApp setting ID
   * @param phoneNumber Phone number to send test message to
   * @param message Test message content
   */
  testWhatsApp(id: string, phoneNumber: string, message: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.apiUrl}/${id}/test`, { phoneNumber, message })
      .pipe(map(data => ({ isSuccess: true, message: 'Test WhatsApp message sent successfully', data })));
  }

  /**
   * Set a WhatsApp setting as default
   * @param id The WhatsApp setting ID
   */
  setAsDefault(id: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.apiUrl}/${id}/set-default`, {})
      .pipe(map(() => ({ isSuccess: true, message: 'WhatsApp settings set as default' })));
  }

  /**
   * Get WhatsApp templates (if supported by provider)
   * @param id The WhatsApp setting ID
   */
  getTemplates(id: string): Observable<ApiResponse<any[]>> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/templates`)
      .pipe(map(data => ({ isSuccess: true, message: 'Templates retrieved', data })));
  }
}
