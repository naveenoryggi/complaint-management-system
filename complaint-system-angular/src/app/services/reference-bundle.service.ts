import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface ReferenceBundle {
  id: string;
  tenant_id: string;
  bundle_name: string;
  client_name: string;
  client_short_name?: string;
  client_type?: string;
  project_name?: string;
  work_order_number?: string;
  contract_value?: number;
  status: string;
  scope_description?: string;
  value_bands?: string[];
  completeness_score: number;
  missing_documents?: string[];
  bundle_documents?: BundleDocument[];
  created_at: string;
  updated_at: string;
}

export interface BundleDocument {
  id: string;
  bundle_id: string;
  tier: string;
  doc_subtype: string;
  document_path?: string;
  file_name?: string;
  invoice_number?: string;
  invoice_amount?: number;
  is_verified: boolean;
  created_at: string;
}

export interface BundleListResponse {
  items: ReferenceBundle[];
  total: number;
  page: number;
  page_size: number;
}

export interface TenderCriteria {
  id: string;
  tender_id: string;
  criteria_code: string;
  description?: string;
  stage: string;
  max_marks: number;
  qualifying_marks?: number;
  scoring_rules?: any;
  evidence_required?: string[];
  can_reuse_across_tenders: boolean;
  created_at: string;
}

export interface BundleCriteriaAssignment {
  id: string;
  bundle_id: string;
  criteria_id: string;
  predicted_marks?: number;
  actual_marks?: number;
  submitted_doc_types?: string[];
  notes?: string;
}

export interface CriteriaScore {
  criteria_code: string;
  max_marks: number;
  predicted_marks: number;
  gap_analysis: string;
}

export interface ScoringSimulation {
  tender_id: string;
  total_max_marks: number;
  total_predicted_marks: number;
  criteria_scores: CriteriaScore[];
}

@Injectable({ providedIn: 'root' })
export class ReferenceBundleService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/bundles`;

  listBundles(page = 1, pageSize = 20, clientType?: string, status?: string, search?: string): Observable<BundleListResponse> {
    let params = new HttpParams().set('page', page).set('page_size', pageSize);
    if (clientType) params = params.set('client_type', clientType);
    if (status) params = params.set('status', status);
    if (search) params = params.set('search', search);
    return this.http.get<BundleListResponse>(`${this.baseUrl}/`, { params });
  }
  getBundle(id: string): Observable<ReferenceBundle> {
    return this.http.get<ReferenceBundle>(`${this.baseUrl}/${id}`);
  }
  createBundle(data: Partial<ReferenceBundle>): Observable<ReferenceBundle> {
    return this.http.post<ReferenceBundle>(`${this.baseUrl}/`, data);
  }
  updateBundle(id: string, data: Partial<ReferenceBundle>): Observable<ReferenceBundle> {
    return this.http.put<ReferenceBundle>(`${this.baseUrl}/${id}`, data);
  }
  deleteBundle(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
  addBundleDocument(bundleId: string, data: Partial<BundleDocument>): Observable<BundleDocument> {
    return this.http.post<BundleDocument>(`${this.baseUrl}/${bundleId}/documents`, data);
  }
  removeBundleDocument(bundleId: string, docId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${bundleId}/documents/${docId}`);
  }
  listTenderCriteria(tenderId: string): Observable<TenderCriteria[]> {
    return this.http.get<TenderCriteria[]>(`${this.baseUrl}/criteria/tender/${tenderId}`);
  }
  createTenderCriteria(tenderId: string, data: Partial<TenderCriteria>): Observable<TenderCriteria> {
    return this.http.post<TenderCriteria>(`${this.baseUrl}/criteria/tender/${tenderId}`, data);
  }
  deleteTenderCriteria(criteriaId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/criteria/${criteriaId}`);
  }
  assignBundleToCriteria(data: any): Observable<BundleCriteriaAssignment> {
    return this.http.post<BundleCriteriaAssignment>(`${this.baseUrl}/assignments`, data);
  }
  listTenderAssignments(tenderId: string): Observable<BundleCriteriaAssignment[]> {
    return this.http.get<BundleCriteriaAssignment[]>(`${this.baseUrl}/assignments/tender/${tenderId}`);
  }
  simulateScoring(tenderId: string): Observable<ScoringSimulation> {
    return this.http.get<ScoringSimulation>(`${this.baseUrl}/scoring/tender/${tenderId}`);
  }
}
