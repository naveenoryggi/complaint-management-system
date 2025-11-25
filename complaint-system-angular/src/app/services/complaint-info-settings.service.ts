import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ComplaintInformationSettings,
  UpdateComplaintInfoSettingsRequest
} from '../models/complaint-info-settings.model';
import { ApiResponse } from '../models/complaint.model';

@Injectable({
  providedIn: 'root'
})
export class ComplaintInfoSettingsService {
  private apiUrl = `${environment.apiUrl}/ComplaintInfoSettings`;

  constructor(private http: HttpClient) {}

  /**
   * Get complaint information settings for a company
   */
  getSettings(companyId: string): Observable<ApiResponse<ComplaintInformationSettings>> {
    return this.http.get<ApiResponse<ComplaintInformationSettings>>(`${this.apiUrl}/${companyId}`);
  }

  /**
   * Update complaint information settings for a company
   */
  updateSettings(
    companyId: string,
    request: UpdateComplaintInfoSettingsRequest
  ): Observable<ApiResponse<ComplaintInformationSettings>> {
    return this.http.put<ApiResponse<ComplaintInformationSettings>>(
      `${this.apiUrl}/${companyId}`,
      request
    );
  }
}
