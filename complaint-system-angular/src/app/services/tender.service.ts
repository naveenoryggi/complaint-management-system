import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface Tender {
  id: string;
  tenant_id: string;
  created_by: string;
  title: string;
  reference_number?: string;
  issuing_authority?: string;
  portal_name?: string;
  portal_url?: string;
  deadline?: string;
  estimated_value?: number;
  requirements?: any;
  notes?: string;
  status: string;
  created_at: string;
  updated_at: string;
  document_count?: number;
}

export interface TenderCreate {
  title: string;
  reference_number?: string;
  issuing_authority?: string;
  portal_name?: string;
  portal_url?: string;
  deadline?: string;
  estimated_value?: number;
  requirements?: any;
  notes?: string;
  status?: string;
}

export interface TenderUpdate {
  title?: string;
  reference_number?: string;
  issuing_authority?: string;
  portal_name?: string;
  portal_url?: string;
  deadline?: string;
  estimated_value?: number;
  requirements?: any;
  notes?: string;
  status?: string;
}

export interface TenderListResponse {
  items: Tender[];
  total: number;
  page: number;
  page_size: number;
  total_pages: number;
}

export interface TenderDocument {
  id: string;
  tender_id: string;
  document_id: string;
  document_order: number;
  is_generated: boolean;
  generation_prompt?: string;
  created_at: string;
  document?: any;
}

export interface TenderDocumentAssociation {
  document_id: string;
  document_order?: number;
  is_generated?: boolean;
  generation_prompt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TenderService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/tenders`;

  /**
   * Create a new tender
   */
  createTender(tender: TenderCreate): Observable<Tender> {
    return this.http.post<Tender>(this.baseUrl, tender);
  }

  /**
   * Get list of tenders with pagination and filtering
   */
  listTenders(
    page: number = 1,
    pageSize: number = 50,
    status?: string,
    search?: string
  ): Observable<TenderListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('page_size', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<TenderListResponse>(this.baseUrl, { params });
  }

  /**
   * Get tenders with upcoming deadlines
   */
  getUpcomingTenders(days: number = 7): Observable<Tender[]> {
    return this.http.get<Tender[]>(`${this.baseUrl}/upcoming`, {
      params: { days: days.toString() }
    });
  }

  /**
   * Get tender by ID
   */
  getTender(tenderId: string): Observable<Tender> {
    return this.http.get<Tender>(`${this.baseUrl}/${tenderId}`);
  }

  /**
   * Update tender
   */
  updateTender(tenderId: string, update: TenderUpdate): Observable<Tender> {
    return this.http.put<Tender>(`${this.baseUrl}/${tenderId}`, update);
  }

  /**
   * Delete tender
   */
  deleteTender(tenderId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tenderId}`);
  }

  /**
   * Associate document with tender
   */
  associateDocument(
    tenderId: string,
    association: TenderDocumentAssociation
  ): Observable<TenderDocument> {
    return this.http.post<TenderDocument>(
      `${this.baseUrl}/${tenderId}/documents`,
      association
    );
  }

  /**
   * Get tender documents
   */
  getTenderDocuments(tenderId: string): Observable<TenderDocument[]> {
    return this.http.get<TenderDocument[]>(`${this.baseUrl}/${tenderId}/documents`);
  }

  /**
   * Remove document association
   */
  removeDocumentAssociation(tenderId: string, documentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tenderId}/documents/${documentId}`);
  }
}
