import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface CompanyDocument {
  id: string;
  name: string;
  description: string | null;
  file_path: string;
  file_size: number;
  mime_type: string;
  document_type: string;
  company_doc_category: string;
  expiry_date: string | null;
  is_company_document: boolean;
  tags: string[];
  version: number;
  created_at: string | null;
  updated_at: string | null;
}

export interface CategorySummary {
  category: string;
  label: string;
  count: number;
  has_expiring: boolean;
  has_expired: boolean;
  earliest_expiry: string | null;
}

export interface AutoLinkResult {
  linked: number;
  details: { checklist_item: string; linked_document: string; category: string }[];
}

@Injectable({
  providedIn: 'root'
})
export class CompanyDocumentService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/company-documents`;

  list(category?: string): Observable<CompanyDocument[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    return this.http.get<CompanyDocument[]>(this.baseUrl, { params });
  }

  upload(file: File, name: string, category: string, description?: string, expiryDate?: string): Observable<CompanyDocument> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('name', name);
    formData.append('company_doc_category', category);
    if (description) formData.append('description', description);
    if (expiryDate) formData.append('expiry_date', expiryDate);
    return this.http.post<CompanyDocument>(`${this.baseUrl}/upload`, formData);
  }

  update(id: string, data: Partial<{ name: string; description: string; company_doc_category: string; expiry_date: string }>): Observable<CompanyDocument> {
    return this.http.put<CompanyDocument>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  getCategories(): Observable<CategorySummary[]> {
    return this.http.get<CategorySummary[]>(`${this.baseUrl}/categories`);
  }

  autoLink(tenderId: string): Observable<AutoLinkResult> {
    return this.http.post<AutoLinkResult>(`${this.baseUrl}/auto-link/${tenderId}`, {});
  }
}
