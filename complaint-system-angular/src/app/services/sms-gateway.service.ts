import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';

export interface SmsGatewaySettings {
  id: string;
  name: string;
  code: string;
  provider: string;
  apiUrl: string;
  accountSid: string;
  authToken: string;
  fromNumber: string;
  senderName: string;
  isActive: boolean;
  isDefault: boolean;
  maxSmsPerHour?: number;
  costPerSms?: number;
  timeoutSeconds?: number;
  companyId: string;
  additionalConfig?: string;
  testNotes?: string;
  lastTestedAt?: Date;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
  isDeleted: boolean;
}

export interface CreateSmsGatewayRequest {
  name: string;
  code: string;
  provider: string;
  apiUrl: string;
  accountSid: string;
  authToken: string;
  fromNumber: string;
  senderName: string;
  isActive: boolean;
  isDefault: boolean;
  maxSmsPerHour?: number;
  costPerSms?: number;
  timeoutSeconds?: number;
  additionalConfig?: string;
  testNotes?: string;
}

export interface UpdateSmsGatewayRequest extends CreateSmsGatewayRequest {}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SmsGatewayService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/communication/sms-settings`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all SMS gateway settings
   * @param includeInactive Include inactive settings
   */
  getAll(includeInactive: boolean = false): Observable<ApiResponse<SmsGatewaySettings[]>> {
    const params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    return this.http.get<SmsGatewaySettings[]>(this.apiUrl, { params })
      .pipe(map(data => ({ isSuccess: true, message: 'Success', data })));
  }

  /**
   * Get a single SMS gateway setting by ID
   * @param id The SMS setting ID
   */
  getById(id: string): Observable<ApiResponse<SmsGatewaySettings>> {
    return this.http.get<SmsGatewaySettings>(`${this.apiUrl}/${id}`)
      .pipe(map(data => ({ isSuccess: true, message: 'Success', data })));
  }

  /**
   * Create a new SMS gateway setting
   * @param request The SMS setting creation request
   */
  create(request: CreateSmsGatewayRequest): Observable<ApiResponse<SmsGatewaySettings>> {
    return this.http.post<SmsGatewaySettings>(this.apiUrl, request)
      .pipe(map(data => ({ isSuccess: true, message: 'SMS gateway created successfully', data })));
  }

  /**
   * Update an existing SMS gateway setting
   * @param id The SMS setting ID
   * @param request The SMS setting update request
   */
  update(id: string, request: UpdateSmsGatewayRequest): Observable<ApiResponse<SmsGatewaySettings>> {
    return this.http.put<SmsGatewaySettings>(`${this.apiUrl}/${id}`, request)
      .pipe(map(data => ({ isSuccess: true, message: 'SMS gateway updated successfully', data })));
  }

  /**
   * Delete an SMS gateway setting
   * @param id The SMS setting ID
   */
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(map(() => ({ isSuccess: true, message: 'SMS gateway deleted successfully', data: true })));
  }

  /**
   * Test an SMS gateway setting
   * @param id The SMS setting ID
   * @param phoneNumber Phone number to send test SMS to
   * @param message Test message content
   */
  testSms(id: string, phoneNumber: string, message: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.apiUrl}/${id}/test`, { phoneNumber, message })
      .pipe(map(data => ({ isSuccess: true, message: 'Test SMS sent successfully', data })));
  }

  /**
   * Set an SMS gateway setting as default
   * @param id The SMS setting ID
   */
  setAsDefault(id: string): Observable<ApiResponse<any>> {
    return this.http.post<any>(`${this.apiUrl}/${id}/set-default`, {})
      .pipe(map(() => ({ isSuccess: true, message: 'SMS gateway set as default' })));
  }
}
