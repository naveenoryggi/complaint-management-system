import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Company, CompanyResponse, UpdateCompanyRequest, UploadLogoResponse } from '../models/company.model';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private apiUrl = `${environment.apiUrl}/company`;

  constructor(private http: HttpClient) {}

  /**
   * Get company details by ID
   */
  getCompanyById(id: string): Observable<CompanyResponse> {
    return this.http.get<CompanyResponse>(`${this.apiUrl}/${id}`);
  }

  /**
   * Update company details
   */
  updateCompany(id: string, request: UpdateCompanyRequest): Observable<CompanyResponse> {
    return this.http.put<CompanyResponse>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Upload company logo
   */
  uploadLogo(companyId: string, file: File): Observable<UploadLogoResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<UploadLogoResponse>(`${this.apiUrl}/${companyId}/logo`, formData);
  }

  /**
   * Delete company logo
   */
  deleteLogo(companyId: string): Observable<{ isSuccess: boolean; message?: string }> {
    return this.http.delete<{ isSuccess: boolean; message?: string }>(`${this.apiUrl}/${companyId}/logo`);
  }

  /**
   * Get full logo URL
   */
  getLogoUrl(logoPath?: string): string | null {
    if (!logoPath) return null;
    // If it's already a full URL, return as is
    if (logoPath.startsWith('http')) {
      return logoPath;
    }
    // Otherwise, append to API base URL
    return `${environment.apiUrl.replace('/api', '')}${logoPath}`;
  }
}
