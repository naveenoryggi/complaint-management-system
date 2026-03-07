import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface CompanyProfile {
  id: string;
  tenant_id: string;
  company_name: string;
  cin_number?: string;
  pan_number?: string;
  gstin?: string;
  msme_registration?: string;
  registered_address?: string;
  corporate_address?: string;
  website?: string;
  phone?: string;
  email?: string;
  annual_turnover?: Record<string, number>;
  year_established?: number;
  employee_count?: number;
  logo_path?: string;
  letterhead_path?: string;
  signature_path?: string;
  stamp_path?: string;
  pf_registration_number?: string;
  pf_registration_date?: string;
  esi_registration_number?: string;
  esi_registration_date?: string;
  bank_name?: string;
  account_number?: string;
  ifsc_code?: string;
  branch_name?: string;
  created_at: string;
  updated_at: string;
}

export interface Certification {
  id: string;
  company_id: string;
  name: string;
  cert_type?: string;
  issuing_body?: string;
  certificate_number?: string;
  issue_date?: string;
  expiry_date?: string;
  document_path?: string;
  is_valid: boolean;
  created_at: string;
  updated_at: string;
}

export interface Personnel {
  id: string;
  company_id: string;
  name: string;
  designation?: string;
  role_in_tender?: string;
  qualification?: string;
  experience_years?: number;
  specialization?: string[];
  cv_path?: string;
  experience_cert_path?: string;
  qualification_cert_path?: string;
  created_at: string;
  updated_at: string;
}

@Injectable({ providedIn: 'root' })
export class CompanyMasterService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/company`;

  getProfile(): Observable<CompanyProfile> {
    return this.http.get<CompanyProfile>(`${this.baseUrl}/profile`);
  }
  createProfile(data: Partial<CompanyProfile>): Observable<CompanyProfile> {
    return this.http.post<CompanyProfile>(`${this.baseUrl}/profile`, data);
  }
  updateProfile(data: Partial<CompanyProfile>): Observable<CompanyProfile> {
    return this.http.put<CompanyProfile>(`${this.baseUrl}/profile`, data);
  }
  uploadBrandAsset(assetType: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/profile/brand-asset/${assetType}`, formData);
  }
  deleteBrandAsset(assetType: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/profile/brand-asset/${assetType}`);
  }
  listCertifications(): Observable<Certification[]> {
    return this.http.get<Certification[]>(`${this.baseUrl}/certifications`);
  }
  createCertification(data: Partial<Certification>): Observable<Certification> {
    return this.http.post<Certification>(`${this.baseUrl}/certifications`, data);
  }
  updateCertification(id: string, data: Partial<Certification>): Observable<Certification> {
    return this.http.put<Certification>(`${this.baseUrl}/certifications/${id}`, data);
  }
  deleteCertification(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/certifications/${id}`);
  }
  listPersonnel(): Observable<Personnel[]> {
    return this.http.get<Personnel[]>(`${this.baseUrl}/personnel`);
  }
  createPersonnel(data: Partial<Personnel>): Observable<Personnel> {
    return this.http.post<Personnel>(`${this.baseUrl}/personnel`, data);
  }
  updatePersonnel(id: string, data: Partial<Personnel>): Observable<Personnel> {
    return this.http.put<Personnel>(`${this.baseUrl}/personnel/${id}`, data);
  }
  deletePersonnel(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/personnel/${id}`);
  }
}
