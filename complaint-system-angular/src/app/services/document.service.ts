import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface Document {
  id: string;
  tenant_id: string;
  created_by: string;
  name: string;
  description?: string;
  document_type?: string;
  tags: string[];
  is_template: boolean;
  file_path: string;
  file_size: number;
  mime_type: string;
  version: number;
  metadata: Record<string, any>;
  created_at: string;
  updated_at: string;
}

export interface DocumentListResponse {
  items: Document[];
  total: number;
  page: number;
  page_size: number;
  total_pages: number;
}

export interface DocumentUploadResponse {
  id: string;
  name: string;
  file_path: string;
  file_size: number;
  mime_type: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/documents`;

  listDocuments(page: number = 1, pageSize: number = 50): Observable<DocumentListResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('page_size', pageSize.toString());
    return this.http.get<DocumentListResponse>(this.baseUrl, { params });
  }

  searchDocuments(
    query?: string,
    documentType?: string,
    tags?: string[],
    isTemplate?: boolean,
    page: number = 1,
    pageSize: number = 50
  ): Observable<DocumentListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('page_size', pageSize.toString());

    if (query) params = params.set('query', query);
    if (documentType) params = params.set('document_type', documentType);
    if (isTemplate !== undefined) params = params.set('is_template', isTemplate.toString());
    if (tags?.length) {
      tags.forEach(tag => { params = params.append('tags', tag); });
    }

    return this.http.get<DocumentListResponse>(`${this.baseUrl}/search`, { params });
  }

  uploadDocument(file: File, name: string, documentType?: string, tags?: string[], isTemplate: boolean = false): Observable<DocumentUploadResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('name', name);
    if (documentType) formData.append('document_type', documentType);
    if (tags?.length) formData.append('tags', tags.join(','));
    formData.append('is_template', isTemplate.toString());

    return this.http.post<DocumentUploadResponse>(`${this.baseUrl}/upload`, formData);
  }

  getDocument(id: string): Observable<Document> {
    return this.http.get<Document>(`${this.baseUrl}/${id}`);
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  downloadDocument(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/download`, { responseType: 'blob' });
  }
}
