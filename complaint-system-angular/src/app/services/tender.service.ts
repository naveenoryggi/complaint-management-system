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

export interface EMDBankDetails {
  bank_name?: string;
  account_number?: string;
  ifsc_code?: string;
  branch?: string;
  account_holder?: string;
  dd_in_favour_of?: string;
}

export interface ExtractedEMD {
  amount?: number;
  mode?: string;
  validity_end_date?: string;
  msme_exempted?: boolean;
  exemption_details?: string;
  emd_format?: string;
  bank_details?: EMDBankDetails;
}

export interface ExtractedFee {
  fee_type: string;
  amount: number;
  payment_mode?: string;
  msme_exempted?: boolean;
  exemption_details?: string;
  bank_details?: EMDBankDetails;
  payment_deadline?: string;
  non_refundable?: boolean;
}

export interface ExtractedTenderData {
  title?: string;
  reference_number?: string;
  issuing_authority?: string;
  deadline?: string;
  estimated_value?: number;
  eligibility_criteria?: string[];
  technical_requirements?: string[];
  emd?: ExtractedEMD;
  tender_fees?: ExtractedFee[];
  evaluation_criteria?: { criteria_code: string; stage: string; max_marks: number; description?: string }[];
  document_checklist?: string[];
  important_dates?: { [key: string]: string };
  contact_info?: { [key: string]: string };
  special_conditions?: string[];
  oem_requirements?: { oem_name: string; product_category?: string; maf_required?: boolean }[];
}

export interface ExtractionResponse {
  tender_id: string;
  document_id?: string;
  extracted_data: ExtractedTenderData;
  model_used: string;
  tokens_used: number;
  extraction_mode: 'native_pdf' | 'text_extraction' | 'text_extraction_batched';
  files_processed: number;
  batches_used?: number;
  applied: boolean;
  applied_summary?: ApplyExtractionResponse;
  confidence_score?: number;
}

export interface ApplyExtractionResponse {
  tender_id: string;
  fields_updated: string[];
  criteria_created: number;
  emd_created: boolean;
  fees_created: number;
  oem_requirements_created: number;
  checklist_created: number;
}

export interface ComplianceCheckItem {
  key: string;
  label: string;
  status: 'ready' | 'missing' | 'partial' | 'na';
  detail?: string;
}

export interface ComplianceCategory {
  category: string;
  label: string;
  icon: string;
  weight: number;
  items: ComplianceCheckItem[];
  ready_count: number;
  total_count: number;
  score: number;
}

export interface ComplianceCheckResponse {
  tender_id: string;
  overall_score: number;
  overall_label: string;
  categories: ComplianceCategory[];
  checked_at: string;
}

export interface ComplianceScoreSummary {
  tender_id: string;
  overall_score: number;
  overall_label: string;
}

// Technical Compliance Matrix
export interface TechnicalComplianceItem {
  id: string;
  tender_id: string;
  clause_number: string;
  clause_title: string;
  clause_description?: string;
  section?: string;
  compliance_status: 'complied' | 'not_complied' | 'partially_complied' | 'pending' | 'na';
  deviation_remarks?: string;
  page_reference?: string;
  supporting_document_id?: string;
  is_mandatory: boolean;
  is_critical: boolean;
  ai_extracted: boolean;
  sort_order: number;
  risk_score?: number;
  risk_category?: 'low' | 'medium' | 'high' | 'critical';
  risk_reason?: string;
  interpretation?: string;
  cross_references?: { clause: string; relationship: string }[];
}

export interface ClauseInterpretationResult {
  interpreted: number;
  total_clauses: number;
  tokens_used: number;
  model_used: string;
  overall_risk_summary: string;
  red_flags: string[];
  interpretations: any[];
  risk_distribution: {
    distribution: { low: number; medium: number; high: number; critical: number };
    average_risk_score: number;
    max_risk_score: number;
    high_risk_count: number;
  };
}

export interface RiskSummary {
  total_interpreted: number;
  risk_distribution: { low: number; medium: number; high: number; critical: number };
  average_risk: number;
  max_risk: number;
  red_flags: { clause_number: string; clause_title: string; risk_score: number; risk_reason: string }[];
  top_risks: any[];
}

// Bill of Quantities
export interface BoQLineItem {
  id: string;
  tender_id: string;
  item_number: string;
  description: string;
  unit?: string;
  quantity: number;
  unit_rate?: number;
  total_before_tax?: number;
  gst_percentage?: number;
  gst_amount?: number;
  total_with_tax?: number;
  category?: string;
  is_optional: boolean;
  make_model?: string;
  oem_id?: string;
  sort_order: number;
  notes?: string;
}

export interface BoQSummary {
  total_items: number;
  mandatory_items: number;
  optional_items: number;
  total_before_tax: number;
  total_gst: number;
  total_with_tax: number;
  mandatory_total: number;
  optional_total: number;
  category_breakdown: { [key: string]: { count: number; total_before_tax: number; total_with_tax: number } };
}

// Milestones
export interface TenderMilestone {
  id: string;
  tender_id: string;
  milestone_type: string;
  label: string;
  event_date?: string;
  event_time?: string;
  location?: string;
  is_completed: boolean;
  notes?: string;
  alert_days_before?: number;
  sort_order: number;
}

// Corrigendum
export interface TenderCorrigendum {
  id: string;
  tender_id: string;
  corrigendum_number: number;
  issue_date?: string;
  title: string;
  summary?: string;
  changes?: any[];
  document_path?: string;
  is_acknowledged: boolean;
}

// Eligibility
export interface EligibilityCheck {
  check: string;
  label: string;
  requirement: string;
  actual: string;
  status: 'pass' | 'fail' | 'warn' | 'info';
  is_critical: boolean;
  qualifying_works?: any[];
  savings?: number;
  note?: string;
}

export interface EligibilityResult {
  tender_id: string;
  eligible: boolean;
  overall_status: string;
  checks: EligibilityCheck[];
  summary: { total_checks: number; passed: number; failed: number; warnings: number };
}

// Submission Checklist
export interface SubmissionChecklistItem {
  id: string;
  tender_id: string;
  document_name: string;
  document_category: string;
  submission_mode: 'online' | 'offline' | 'both';
  is_critical: boolean;
  envelope?: string;
  cover_name?: string;
  online_status: string;
  offline_status: string;
  notarization_required: boolean;
  notarization_type?: string;
  notarization_status: string;
  document_origin: 'tender_provided' | 'self_generated' | 'pre_existing';
  format_reference?: string;
  obtaining_source?: string;
  can_auto_generate: boolean;
  linked_document_id?: string;
  due_date?: string;
  notes?: string;
  sort_order: number;
  source: string;
  created_at?: string;
  updated_at?: string;
}

export interface ChecklistSummary {
  total: number;
  online: { total: number; ready: number; percentage: number };
  offline: { total: number; ready: number; percentage: number };
  critical_pending: number;
  notarization_pending: number;
  by_category: { [key: string]: { total: number; online_ready: number; offline_ready: number; notarization_pending: number } };
  by_envelope: { [key: string]: { total: number; ready: number } };
  by_cover: CoverPackage[];
}

export interface CoverPackage {
  cover_name: string;
  envelope: string;
  total: number;
  ready: number;
  critical: number;
  documents: {
    id: string;
    document_name: string;
    is_critical: boolean;
    online_status?: string;
    offline_status?: string;
    notarization_required: boolean;
    notarization_type?: string;
    notarization_status: string;
    document_origin?: string;
    can_auto_generate?: boolean;
  }[];
}

// Checklist Analysis
export interface ChecklistAnalysisItem {
  item_id: string;
  document_name: string;
  relevant: boolean;
  is_required: boolean;
  reason: string;
}

export interface ChecklistAnalysisResult {
  tender_id: string;
  total_items: number;
  relevant_count: number;
  required_count: number;
  irrelevant_count: number;
  updated_count: number;
  analysis: ChecklistAnalysisItem[];
  summary: string;
}

// Duplicate Detection
export interface DuplicateItem {
  item_id: string;
  item_name: string;
  matched_with: string;
  matched_with_id: string;
  match_type: 'exact' | 'core_name' | 'substring' | 'word_overlap';
  similarity: number;
}

export interface DuplicatePreviewResult {
  threshold: number;
  duplicate_count: number;
  duplicates: DuplicateItem[];
}

// Cover Assembly
export interface CoverAssemblyResult {
  success: boolean;
  file_path: string;
  document_count: number;
  cover_name: string;
  total_items: number;
  staged_items: number;
  message: string;
}

export interface AllCoversAssemblyResult {
  success: boolean;
  file_path: string;
  covers: CoverAssemblyResult[];
  message: string;
}

// Tender Analytics
export interface TenderAnalytics {
  total_tenders: number;
  status_distribution: { status: string; count: number }[];
  monthly_trend: { year: number; month: number; count: number }[];
  authority_breakdown: { authority: string; count: number }[];
  win_rate: number;
  value_analysis: { status: string; total_value: number; count: number }[];
}

// Pre-Bid Queries
export interface PrebidQuery {
  sno: number;
  tender_reference: string;
  existing_clause: string;
  clarification_sought: string;
  response: string;
  category: string;
  priority: string;
}

export interface PrebidQueriesResult {
  queries: PrebidQuery[];
  total: number;
  by_category: { [key: string]: number };
  tender_id: string;
  tender_title: string;
  tender_reference: string;
}

// QCBS
export interface QCBSResult {
  tender_id: string;
  technical_weight: number;
  financial_weight: number;
  our_technical_score: number;
  our_financial_bid: number;
  lowest_financial_bid: number;
  financial_score: number;
  combined_score: number;
  breakdown: { technical_contribution: number; financial_contribution: number };
  notes: string[];
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
    return this.http.post<Tender>(`${this.baseUrl}/`, tender);
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

    return this.http.get<TenderListResponse>(`${this.baseUrl}/`, { params });
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

  reorderDocuments(tenderId: string, documentIds: string[]): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.baseUrl}/${tenderId}/documents/reorder`,
      { document_ids: documentIds }
    );
  }

  extractPreview(files: File[], model?: string): Observable<ExtractionResponse> {
    const formData = new FormData();
    for (const file of files) {
      formData.append('files', file);
    }
    if (model) {
      formData.append('model', model);
    }
    return this.http.post<ExtractionResponse>(`${this.baseUrl}/extract-preview`, formData);
  }

  extractFromPDF(tenderId: string, files: File[], model?: string): Observable<ExtractionResponse> {
    const formData = new FormData();
    for (const file of files) {
      formData.append('files', file);
    }
    if (model) {
      formData.append('model', model);
    }
    return this.http.post<ExtractionResponse>(`${this.baseUrl}/${tenderId}/extract`, formData);
  }

  applyExtraction(tenderId: string, data: ExtractedTenderData): Observable<ApplyExtractionResponse> {
    return this.http.post<ApplyExtractionResponse>(`${this.baseUrl}/${tenderId}/apply-extraction`, data);
  }

  getComplianceCheck(tenderId: string): Observable<ComplianceCheckResponse> {
    return this.http.get<ComplianceCheckResponse>(`${this.baseUrl}/${tenderId}/compliance`);
  }

  getComplianceScores(tenderIds: string[]): Observable<ComplianceScoreSummary[]> {
    const ids = tenderIds.join(',');
    return this.http.get<ComplianceScoreSummary[]>(`${this.baseUrl}/compliance/scores`, {
      params: { tender_ids: ids }
    });
  }

  // --- Technical Compliance Matrix ---
  getComplianceMatrix(tenderId: string): Observable<TechnicalComplianceItem[]> {
    return this.http.get<TechnicalComplianceItem[]>(`${this.baseUrl}/${tenderId}/compliance-matrix`);
  }

  createComplianceItem(tenderId: string, item: Partial<TechnicalComplianceItem>): Observable<TechnicalComplianceItem> {
    return this.http.post<TechnicalComplianceItem>(`${this.baseUrl}/${tenderId}/compliance-matrix`, item);
  }

  bulkCreateComplianceItems(tenderId: string, items: Partial<TechnicalComplianceItem>[]): Observable<{ created: number }> {
    return this.http.post<{ created: number }>(`${this.baseUrl}/${tenderId}/compliance-matrix/bulk`, items);
  }

  updateComplianceItem(itemId: string, data: Partial<TechnicalComplianceItem>): Observable<TechnicalComplianceItem> {
    return this.http.put<TechnicalComplianceItem>(`${this.baseUrl}/compliance-matrix/${itemId}`, data);
  }

  deleteComplianceItem(itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/compliance-matrix/${itemId}`);
  }

  getComplianceMatrixSummary(tenderId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${tenderId}/compliance-matrix/summary`);
  }

  interpretClauses(tenderId: string, clauseIds?: string[]): Observable<ClauseInterpretationResult> {
    return this.http.post<ClauseInterpretationResult>(
      `${this.baseUrl}/${tenderId}/compliance-matrix/interpret`,
      clauseIds || null
    );
  }

  getRiskSummary(tenderId: string): Observable<RiskSummary> {
    return this.http.get<RiskSummary>(`${this.baseUrl}/${tenderId}/compliance-matrix/risk-summary`);
  }

  // --- Bill of Quantities ---
  getBoQItems(tenderId: string): Observable<BoQLineItem[]> {
    return this.http.get<BoQLineItem[]>(`${this.baseUrl}/${tenderId}/boq`);
  }

  createBoQItem(tenderId: string, item: Partial<BoQLineItem>): Observable<BoQLineItem> {
    return this.http.post<BoQLineItem>(`${this.baseUrl}/${tenderId}/boq`, item);
  }

  updateBoQItem(itemId: string, data: Partial<BoQLineItem>): Observable<BoQLineItem> {
    return this.http.put<BoQLineItem>(`${this.baseUrl}/boq/${itemId}`, data);
  }

  deleteBoQItem(itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/boq/${itemId}`);
  }

  getBoQSummary(tenderId: string): Observable<BoQSummary> {
    return this.http.get<BoQSummary>(`${this.baseUrl}/${tenderId}/boq/summary`);
  }

  // --- Milestones ---
  getMilestones(tenderId: string): Observable<TenderMilestone[]> {
    return this.http.get<TenderMilestone[]>(`${this.baseUrl}/${tenderId}/milestones`);
  }

  createMilestone(tenderId: string, data: Partial<TenderMilestone>): Observable<TenderMilestone> {
    return this.http.post<TenderMilestone>(`${this.baseUrl}/${tenderId}/milestones`, data);
  }

  updateMilestone(milestoneId: string, data: Partial<TenderMilestone>): Observable<TenderMilestone> {
    return this.http.put<TenderMilestone>(`${this.baseUrl}/milestones/${milestoneId}`, data);
  }

  deleteMilestone(milestoneId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/milestones/${milestoneId}`);
  }

  // --- Corrigendums ---
  getCorrigendums(tenderId: string): Observable<TenderCorrigendum[]> {
    return this.http.get<TenderCorrigendum[]>(`${this.baseUrl}/${tenderId}/corrigendums`);
  }

  createCorrigendum(tenderId: string, data: Partial<TenderCorrigendum>): Observable<TenderCorrigendum> {
    return this.http.post<TenderCorrigendum>(`${this.baseUrl}/${tenderId}/corrigendums`, data);
  }

  updateCorrigendum(corrigendumId: string, data: Partial<TenderCorrigendum>): Observable<TenderCorrigendum> {
    return this.http.put<TenderCorrigendum>(`${this.baseUrl}/corrigendums/${corrigendumId}`, data);
  }

  deleteCorrigendum(corrigendumId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/corrigendums/${corrigendumId}`);
  }

  // --- Eligibility Check ---
  checkEligibility(tenderId: string): Observable<EligibilityResult> {
    return this.http.get<EligibilityResult>(`${this.baseUrl}/${tenderId}/eligibility-check`);
  }

  // --- QCBS Calculator ---
  calculateQCBS(tenderId: string, params: {
    technical_weight: number;
    financial_weight: number;
    our_technical_score: number;
    our_financial_bid: number;
    lowest_financial_bid: number;
  }): Observable<QCBSResult> {
    return this.http.post<QCBSResult>(`${this.baseUrl}/${tenderId}/qcbs-calculate`, null, {
      params: params as any
    });
  }

  // --- Submission Checklist ---
  getChecklistItems(tenderId: string, mode?: string, category?: string, envelope?: string): Observable<SubmissionChecklistItem[]> {
    let params = new HttpParams();
    if (mode) params = params.set('mode', mode);
    if (category) params = params.set('category', category);
    if (envelope) params = params.set('envelope', envelope);
    return this.http.get<SubmissionChecklistItem[]>(`${this.baseUrl}/${tenderId}/submission-checklist`, { params });
  }

  createChecklistItem(tenderId: string, item: Partial<SubmissionChecklistItem>): Observable<SubmissionChecklistItem> {
    return this.http.post<SubmissionChecklistItem>(`${this.baseUrl}/${tenderId}/submission-checklist`, item);
  }

  bulkCreateChecklistItems(tenderId: string, items: Partial<SubmissionChecklistItem>[]): Observable<{ created: number }> {
    return this.http.post<{ created: number }>(`${this.baseUrl}/${tenderId}/submission-checklist/bulk`, items);
  }

  generateChecklist(tenderId: string, includeOffline: boolean = true): Observable<{ created: number; message: string }> {
    return this.http.post<{ created: number; message: string }>(
      `${this.baseUrl}/${tenderId}/submission-checklist/generate`,
      null,
      { params: { include_offline: includeOffline.toString() } }
    );
  }

  analyzeChecklist(tenderId: string): Observable<ChecklistAnalysisResult> {
    return this.http.post<ChecklistAnalysisResult>(
      `${this.baseUrl}/${tenderId}/submission-checklist/analyze`, {}
    );
  }

  removeIrrelevantItems(tenderId: string, itemIds: string[]): Observable<{ removed: number; message: string }> {
    return this.http.post<{ removed: number; message: string }>(
      `${this.baseUrl}/${tenderId}/submission-checklist/remove-irrelevant`,
      { item_ids: itemIds }
    );
  }

  previewDuplicates(tenderId: string, threshold: number = 0.6): Observable<DuplicatePreviewResult> {
    return this.http.get<DuplicatePreviewResult>(
      `${this.baseUrl}/${tenderId}/submission-checklist/duplicates`,
      { params: { threshold: threshold.toString() } }
    );
  }

  deduplicateChecklist(tenderId: string, threshold: number = 0.6): Observable<{ threshold: number; removed: number; message: string }> {
    return this.http.post<{ threshold: number; removed: number; message: string }>(
      `${this.baseUrl}/${tenderId}/submission-checklist/deduplicate`,
      null,
      { params: { threshold: threshold.toString() } }
    );
  }

  getChecklistSummary(tenderId: string): Observable<ChecklistSummary> {
    return this.http.get<ChecklistSummary>(`${this.baseUrl}/${tenderId}/submission-checklist/summary`);
  }

  getChecklistTemplates(tenderId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${tenderId}/submission-checklist/templates`);
  }

  getChecklistCovers(tenderId: string): Observable<CoverPackage[]> {
    return this.http.get<CoverPackage[]>(`${this.baseUrl}/${tenderId}/submission-checklist/covers`);
  }

  updateChecklistItem(itemId: string, data: Partial<SubmissionChecklistItem>): Observable<SubmissionChecklistItem> {
    return this.http.put<SubmissionChecklistItem>(`${this.baseUrl}/submission-checklist/${itemId}`, data);
  }

  deleteChecklistItem(itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/submission-checklist/${itemId}`);
  }

  // --- Document Staging (Link/Unlink) ---
  linkDocumentToChecklist(itemId: string, documentId: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/submission-checklist/${itemId}/link-document`, {
      document_id: documentId
    });
  }

  unlinkDocumentFromChecklist(itemId: string): Observable<SubmissionChecklistItem> {
    return this.http.delete<SubmissionChecklistItem>(`${this.baseUrl}/submission-checklist/${itemId}/link-document`);
  }

  autoLinkDocuments(itemId: string): Observable<{ linked: number; total_unlinked: number }> {
    return this.http.post<{ linked: number; total_unlinked: number }>(
      `${this.baseUrl}/submission-checklist/${itemId}/bulk-link`, null
    );
  }

  // --- Cover Assembly ---
  assembleCover(tenderId: string, coverName: string, includeTitlePage: boolean = true): Observable<CoverAssemblyResult> {
    return this.http.post<CoverAssemblyResult>(
      `${this.baseUrl}/${tenderId}/submission-checklist/covers/assemble`,
      null,
      { params: { cover_name: coverName, include_title_page: includeTitlePage.toString() } }
    );
  }

  assembleAllCovers(tenderId: string, includeTitlePages: boolean = true): Observable<AllCoversAssemblyResult> {
    return this.http.post<AllCoversAssemblyResult>(
      `${this.baseUrl}/${tenderId}/submission-checklist/covers/assemble-all`,
      null,
      { params: { include_title_pages: includeTitlePages.toString() } }
    );
  }

  // --- Analytics ---
  getAnalytics(): Observable<TenderAnalytics> {
    return this.http.get<TenderAnalytics>(`${this.baseUrl}/analytics/`);
  }

  // --- Pre-Bid Queries ---
  generatePrebidQueries(tenderId: string): Observable<PrebidQueriesResult> {
    return this.http.post<PrebidQueriesResult>(`${this.baseUrl}/${tenderId}/prebid-queries/generate`, {});
  }

  exportPrebidQueries(tenderId: string, queries: PrebidQuery[], tenderTitle: string, tenderReference: string): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${tenderId}/prebid-queries/export`, {
      queries,
      tender_title: tenderTitle,
      tender_reference: tenderReference,
    }, { responseType: 'blob' });
  }

  updatePrebidQuery(tenderId: string, index: number, data: Partial<PrebidQuery>): Observable<PrebidQuery> {
    return this.http.put<PrebidQuery>(`${this.baseUrl}/${tenderId}/prebid-queries/${index}`, data);
  }

  // --- AI Suggest Response ---
  suggestPrebidResponse(tenderId: string, queryIndex: number, query: PrebidQuery, allQueries: PrebidQuery[]): Observable<SuggestResponseResult> {
    return this.http.post<SuggestResponseResult>(`${this.baseUrl}/${tenderId}/prebid-queries/suggest-response`, {
      query_index: queryIndex,
      query,
      all_queries: allQueries,
    });
  }

  // --- AI Alignment Check ---
  runAlignmentCheck(tenderId: string): Observable<AlignmentCheckResponse> {
    return this.http.post<AlignmentCheckResponse>(`${this.baseUrl}/${tenderId}/alignment-check`, {});
  }

  // --- Feature 2: KB Auto-Fill ---
  getAutoFillSuggestions(tenderId: string, targetArea: string): Observable<AutoFillSuggestion[]> {
    const kbUrl = `${runtimeConfig.pythonApiUrl}/api/v1/knowledge-base`;
    return this.http.get<AutoFillSuggestion[]>(`${kbUrl}/auto-fill/${tenderId}`, {
      params: { target_area: targetArea }
    });
  }

  // --- Feature 3: Bid Cost Estimator ---
  estimateBidCost(tenderId: string): Observable<BidCostEstimate> {
    return this.http.post<BidCostEstimate>(`${this.baseUrl}/${tenderId}/estimate-bid-cost`, {});
  }

  // --- Feature 4: Workflow ---
  getAllowedTransitions(tenderId: string): Observable<AllowedTransitions> {
    return this.http.get<AllowedTransitions>(`${this.baseUrl}/${tenderId}/workflow/allowed-transitions`);
  }

  transitionStatus(tenderId: string, newStatus: string, reason?: string): Observable<TransitionResult> {
    return this.http.post<TransitionResult>(`${this.baseUrl}/${tenderId}/workflow/transition`, {
      new_status: newStatus,
      reason: reason || null
    });
  }

  getStatusHistory(tenderId: string): Observable<StatusTransition[]> {
    return this.http.get<StatusTransition[]>(`${this.baseUrl}/${tenderId}/workflow/history`);
  }

  // --- Feature 5: Collaboration ---
  getTeamAssignments(tenderId: string): Observable<TenderAssignment[]> {
    return this.http.get<TenderAssignment[]>(`${this.baseUrl}/${tenderId}/assignments`);
  }

  addTeamAssignment(tenderId: string, data: { user_id: string; user_name: string; role: string }): Observable<TenderAssignment> {
    return this.http.post<TenderAssignment>(`${this.baseUrl}/${tenderId}/assignments`, data);
  }

  removeTeamAssignment(tenderId: string, userId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${tenderId}/assignments/${userId}`);
  }

  getTenderComments(tenderId: string, page: number = 1): Observable<CommentsResponse> {
    return this.http.get<CommentsResponse>(`${this.baseUrl}/${tenderId}/comments`, {
      params: { page: page.toString() }
    });
  }

  addTenderComment(tenderId: string, data: { comment_text: string; parent_id?: string }): Observable<TenderComment> {
    return this.http.post<TenderComment>(`${this.baseUrl}/${tenderId}/comments`, data);
  }

  deleteTenderComment(tenderId: string, commentId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${tenderId}/comments/${commentId}`);
  }

  // --- Feature 6: Tender Result ---
  getTenderResult(tenderId: string): Observable<TenderResultData> {
    return this.http.get<TenderResultData>(`${this.baseUrl}/${tenderId}/result`);
  }

  createTenderResult(tenderId: string, data: Partial<TenderResultData>): Observable<TenderResultData> {
    return this.http.post<TenderResultData>(`${this.baseUrl}/${tenderId}/result`, data);
  }

  updateTenderResult(tenderId: string, data: Partial<TenderResultData>): Observable<TenderResultData> {
    return this.http.put<TenderResultData>(`${this.baseUrl}/${tenderId}/result`, data);
  }

  getWinLossAnalytics(): Observable<WinLossAnalytics> {
    return this.http.get<WinLossAnalytics>(`${this.baseUrl}/results/analytics`);
  }

  // --- Feature 7: Tender Compare ---
  compareTenders(tenderIds: string[]): Observable<TenderComparison[]> {
    return this.http.post<TenderComparison[]>(`${this.baseUrl}/compare`, {
      tender_ids: tenderIds
    });
  }

  // --- Feature 8: Bid/No-Bid ---
  analyzeBidDecision(tenderId: string): Observable<BidNoBidAnalysis> {
    return this.http.post<BidNoBidAnalysis>(`${this.baseUrl}/${tenderId}/bid-nobid-analysis`, {});
  }

  // --- Solution Document Generator ---
  generateSolution(tenderId: string, options?: { skip_optimization?: boolean; regenerate?: boolean }): Observable<SolutionGenerationResult> {
    return this.http.post<SolutionGenerationResult>(
      `${this.baseUrl}/${tenderId}/solution/generate`,
      options || {}
    );
  }

  getSolutionStatus(tenderId: string): Observable<SolutionGenerationStatus> {
    return this.http.get<SolutionGenerationStatus>(`${this.baseUrl}/${tenderId}/solution/status`);
  }

  getSolutionSections(tenderId: string): Observable<SolutionSectionsResponse> {
    return this.http.get<SolutionSectionsResponse>(`${this.baseUrl}/${tenderId}/solution/sections`);
  }

  regenerateSolutionSection(tenderId: string, sectionId: string, customInstructions?: string): Observable<RegeneratedSection> {
    return this.http.post<RegeneratedSection>(
      `${this.baseUrl}/${tenderId}/solution/sections/${sectionId}/regenerate`,
      { custom_instructions: customInstructions || null }
    );
  }

  exportSolutionDocx(tenderId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${tenderId}/solution/export`, { responseType: 'blob' });
  }
}

// AI Suggest Response
export interface SuggestResponseResult {
  suggested_response: string;
  tokens_used: number;
  model_used: string;
}

// AI Alignment Verification
export interface AlignmentIssue {
  area: string;
  severity: 'critical' | 'warning' | 'info';
  title: string;
  description: string;
  recommendation: string;
}

export interface AlignmentCheckResponse {
  tender_id: string;
  alignment_score: number;
  alignment_label: string;
  summary: string;
  issues: AlignmentIssue[];
  strengths: string[];
  risk_areas: string[];
  tokens_used: number;
  model_used: string;
}

// --- Feature 2: KB Auto-Fill ---
export interface AutoFillSuggestion {
  kb_entry_id: string;
  title: string;
  content: string;
  target_field: string;
  confidence_score: number;
  category: string;
  subcategory?: string;
  is_verified: boolean;
  usage_count: number;
}

// --- Feature 3: Bid Cost Estimator ---
export interface BidCostEstimate {
  tender_id: string;
  tender_title: string;
  direct_costs: { emd: number; tender_fee: number; document_cost: number; travel_cost: number };
  boq_summary: { subtotal: number; gst: number; total: number; item_count: number };
  overhead: number;
  ai_analysis: {
    recommended_margin_pct: number;
    risk_factors: string[];
    competitive_score: number;
    analysis_notes?: string;
    tokens_used?: number;
    model_used?: string;
  };
  total_bid_value: number;
  profit_estimate: number;
}

// --- Feature 4: Workflow ---
export interface AllowedTransitions {
  tender_id: string;
  current_status: string;
  allowed_transitions: string[];
}

export interface StatusTransition {
  id: string;
  from_status: string;
  to_status: string;
  changed_by: string;
  change_reason: string | null;
  changed_at: string;
}

export interface TransitionResult {
  tender_id: string;
  from_status: string;
  to_status: string;
  reason: string | null;
  changed_at: string;
}

// --- Feature 5: Collaboration ---
export interface TenderAssignment {
  id: string;
  tender_id: string;
  user_id: string;
  user_name: string;
  role: string;
  assigned_by: string;
  assigned_at: string;
}

export interface TenderComment {
  id: string;
  tender_id: string;
  user_id: string;
  user_name: string;
  comment_text: string;
  parent_id: string | null;
  created_at: string;
  updated_at: string;
}

export interface CommentsResponse {
  items: TenderComment[];
  total: number;
  page: number;
  page_size: number;
}

// --- Feature 6: Tender Result ---
export interface TenderResultData {
  id?: string;
  tender_id?: string;
  result: string;
  awarded_value?: number | null;
  award_date?: string | null;
  winning_bidder?: string | null;
  loss_reason?: string | null;
  lessons_learned?: string | null;
  contract_number?: string | null;
  contract_start?: string | null;
  contract_end?: string | null;
  created_at?: string;
  updated_at?: string;
}

export interface WinLossAnalytics {
  total_results: number;
  won_count: number;
  lost_count: number;
  cancelled_count: number;
  win_rate: number;
  loss_reasons: { [key: string]: number };
  average_awarded_value: number;
  total_awarded_value: number;
}

// --- Feature 7: Tender Compare ---
export interface TenderComparison {
  tender_id: string;
  title: string;
  reference_number: string | null;
  issuing_authority: string | null;
  status: string;
  deadline: string | null;
  estimated_value: number | null;
  document_count: number;
  emd_total: number;
  fee_total: number;
  oem_count: number;
  boq_items: number;
}

// --- Feature 8: Bid/No-Bid ---
export interface BidNoBidAnalysis {
  tender_id: string;
  recommendation: 'bid' | 'no_bid' | 'conditional';
  confidence: number;
  score_breakdown: {
    technical_fit: number;
    financial_capacity: number;
    timeline: number;
    competition: number;
    strategic: number;
  };
  rationale: string;
  conditions: string[];
  risks: string[];
  tokens_used: number;
  model_used: string;
}

// --- Solution Document Generator ---
export interface SolutionGenerationResult {
  generation_id: string;
  status: string;
  total_sections: number;
  total_requirements: number;
  coverage_percentage: number;
  estimated_score: number;
  gap_count: number;
  diagram_count: number;
}

export interface SolutionGenerationStatus {
  generation_id?: string;
  status: string;
  current_step?: string;
  total_steps: number;
  completed_steps: number;
  progress_percentage: number;
  total_sections?: number;
  total_requirements?: number;
  coverage_percentage?: number;
  estimated_score?: number;
  gap_analysis?: SolutionGap[];
  evaluation_results?: SolutionEvaluation[];
  diagram_suggestions?: DiagramSuggestion[];
  started_at?: string;
  completed_at?: string;
  error_message?: string;
  message?: string;
}

export interface SolutionGap {
  requirement_id: string;
  issue: string;
  risk_level: 'critical' | 'high' | 'medium' | 'low';
  recommendation: string;
  affected_section: string;
}

export interface SolutionEvaluation {
  criteria: string;
  max_marks: number;
  estimated_score: number;
  percentage: number;
  strengths: string[];
  weaknesses: string[];
  improvement_suggestions: string[];
}

export interface DiagramSuggestion {
  section_number: string;
  section_title: string;
  diagram_type: string;
  description: string;
  rationale: string;
}

export interface ProposalSection {
  id: string;
  section_number: string;
  section_title: string;
  parent_section_id?: string;
  mapped_requirement_ids: string[];
  status: string;
  sort_order: number;
  content?: string;
  content_version: number;
  content_format?: string;
  model_used?: string;
  diagram_suggestions?: DiagramSuggestion[];
  quality_score?: number;
}

export interface SolutionSectionsResponse {
  tender_id: string;
  sections: ProposalSection[];
  total: number;
}

export interface RegeneratedSection {
  section_id: string;
  section_number: string;
  section_title: string;
  content: string;
  version: number;
  model_used: string;
}
