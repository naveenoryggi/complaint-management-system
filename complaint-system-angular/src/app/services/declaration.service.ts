import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface DeclarationType {
  key: string;
  name: string;
  category: string;
  description: string;
  is_required: boolean;
}

export interface DeclarationAnalysisItem {
  key: string;
  ai_recommended: boolean;
  reason: string | null;
  tender_specific_points: string[];
}

export interface DeclarationAnalysisResult {
  tender_id: string;
  total_types: number;
  ai_recommended_count: number;
  analysis: DeclarationAnalysisItem[];
}

export interface BulkDeclarationRequest {
  declaration_types: string[];
  tender_id?: string;
  signatory_name?: string;
  designation?: string;
  ai_types?: string[];
  analysis_results?: DeclarationAnalysisItem[];
  bidder_role?: string;
}

export interface ValidationResult {
  total_points: number;
  found: number;
  missing: string[];
  status: 'pass' | 'warning' | 'none';
}

export interface DeclarationPreview {
  key: string;
  name: string;
  content: string;
  mode: 'standard' | 'ai';
  category: string;
  validation?: ValidationResult;
  error?: string;
}

export interface PreviewBulkResponse {
  previews: DeclarationPreview[];
}

export interface SaveAndLinkRequest {
  tender_id: string;
  declaration_types: string[];
  signatory_name?: string;
  designation?: string;
  ai_types?: string[];
  analysis_results?: DeclarationAnalysisItem[];
  bidder_role?: string;
}

export interface SaveAndLinkResultItem {
  key: string;
  document_id: string | null;
  linked_checklist_item_id: string | null;
  filename: string | null;
  status: string;
  error?: string;
}

export interface SaveAndLinkResult {
  results: SaveAndLinkResultItem[];
  saved: number;
  linked: number;
}

@Injectable({ providedIn: 'root' })
export class DeclarationService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/declarations`;

  getDeclarationTypes(): Observable<DeclarationType[]> {
    return this.http.get<DeclarationType[]>(`${this.baseUrl}/types`);
  }

  analyzeTender(tenderId: string): Observable<DeclarationAnalysisResult> {
    return this.http.post<DeclarationAnalysisResult>(
      `${this.baseUrl}/analyze/${tenderId}`,
      {}
    );
  }

  generateSingle(
    type: string,
    tenderId?: string,
    signatoryName?: string,
    designation?: string,
    mode: string = 'standard',
    tenderSpecificPoints?: string[],
    bidderRole: string = 'bidder'
  ): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate`, {
      declaration_type: type,
      tender_id: tenderId || null,
      signatory_name: signatoryName || null,
      designation: designation || null,
      mode,
      tender_specific_points: tenderSpecificPoints || null,
      bidder_role: bidderRole,
    }, { responseType: 'blob' });
  }

  generateBulk(request: BulkDeclarationRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate-bulk`, request, { responseType: 'blob' });
  }

  generateBulkForTender(tenderId: string, request: BulkDeclarationRequest): Observable<Blob> {
    return this.http.post(
      `${this.baseUrl}/tenders/${tenderId}/generate-bulk`,
      request,
      { responseType: 'blob' }
    );
  }

  previewBulk(request: BulkDeclarationRequest): Observable<PreviewBulkResponse> {
    return this.http.post<PreviewBulkResponse>(`${this.baseUrl}/preview`, request);
  }

  previewSingle(
    type: string,
    tenderId?: string,
    signatoryName?: string,
    designation?: string,
    mode: string = 'standard',
    tenderSpecificPoints?: string[],
    bidderRole: string = 'bidder'
  ): Observable<DeclarationPreview> {
    return this.http.post<DeclarationPreview>(`${this.baseUrl}/preview-single`, {
      declaration_type: type,
      tender_id: tenderId || null,
      signatory_name: signatoryName || null,
      designation: designation || null,
      mode,
      tender_specific_points: tenderSpecificPoints || null,
      bidder_role: bidderRole,
    });
  }

  saveAndLink(request: SaveAndLinkRequest): Observable<SaveAndLinkResult> {
    return this.http.post<SaveAndLinkResult>(`${this.baseUrl}/save-and-link`, request);
  }
}
