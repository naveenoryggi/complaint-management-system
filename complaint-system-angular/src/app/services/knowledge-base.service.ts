import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

// ---------------------------------------------------------------------------
// Interfaces
// ---------------------------------------------------------------------------

export interface KBEntry {
  id: string;
  tenant_id: string;
  title: string;
  content: string;
  content_format: string;
  category: string;
  subcategory: string | null;
  tags: string[] | null;
  keywords: string[] | null;
  source_type: string;
  source_tender_id: string | null;
  source_description: string | null;
  valid_from: string | null;
  valid_until: string | null;
  fiscal_year: string | null;
  is_current: boolean;
  usage_count: number;
  last_used_at: string | null;
  last_used_tender_id: string | null;
  confidence_score: number | null;
  is_verified: boolean;
  is_archived: boolean;
  created_by: string | null;
  created_at: string | null;
  updated_at: string | null;
  // Enriched by suggestions endpoint
  is_stale?: boolean;
  already_used_in_tender?: boolean;
}

export interface KBVersion {
  id: string;
  version_number: number;
  content: string;
  change_reason: string | null;
  changed_by: string | null;
  created_at: string | null;
}

export interface KBTenderLink {
  id: string;
  tender_id: string;
  usage_context: string | null;
  used_at: string | null;
  used_by: string | null;
}

export interface KBEntryDetail extends KBEntry {
  versions: KBVersion[];
  tender_links: KBTenderLink[];
}

export interface KBListResponse {
  items: KBEntry[];
  total: number;
  page: number;
  page_size: number;
  total_pages: number;
}

export interface KBCategoryStat {
  category: string;
  count: number;
  total_usage: number;
  stale_count: number;
}

export interface KBEntryCreate {
  title: string;
  content: string;
  content_format?: string;
  category: string;
  subcategory?: string;
  tags?: string[];
  keywords?: string[];
  source_type?: string;
  source_tender_id?: string;
  source_description?: string;
  valid_from?: string;
  valid_until?: string;
  fiscal_year?: string;
  is_current?: boolean;
  confidence_score?: number;
  is_verified?: boolean;
}

export interface KBEntryUpdate {
  title?: string;
  content?: string;
  content_format?: string;
  category?: string;
  subcategory?: string;
  tags?: string[];
  keywords?: string[];
  source_type?: string;
  source_tender_id?: string;
  source_description?: string;
  valid_from?: string;
  valid_until?: string;
  fiscal_year?: string;
  is_current?: boolean;
  confidence_score?: number;
  is_verified?: boolean;
  is_archived?: boolean;
  change_reason?: string;
}

export interface HarvestResult {
  created: number;
  skipped: number;
  source: string;
  tender_id?: string;
}

export interface KBDashboardStats {
  total_entries: number;
  verified_count: number;
  stale_count: number;
  harvested_this_month: number;
  category_distribution: { category: string; count: number }[];
}

// ---------------------------------------------------------------------------
// Service
// ---------------------------------------------------------------------------

@Injectable({ providedIn: 'root' })
export class KnowledgeBaseService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/knowledge-base`;

  listEntries(params?: {
    category?: string;
    subcategory?: string;
    search?: string;
    is_current?: boolean;
    page?: number;
    page_size?: number;
  }): Observable<KBListResponse> {
    let httpParams = new HttpParams();
    if (params?.category) httpParams = httpParams.set('category', params.category);
    if (params?.subcategory) httpParams = httpParams.set('subcategory', params.subcategory);
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.is_current !== undefined) httpParams = httpParams.set('is_current', String(params.is_current));
    if (params?.page) httpParams = httpParams.set('page', String(params.page));
    if (params?.page_size) httpParams = httpParams.set('page_size', String(params.page_size));
    return this.http.get<KBListResponse>(this.baseUrl, { params: httpParams });
  }

  getEntry(entryId: string): Observable<KBEntryDetail> {
    return this.http.get<KBEntryDetail>(`${this.baseUrl}/${entryId}`);
  }

  createEntry(data: KBEntryCreate): Observable<KBEntry> {
    return this.http.post<KBEntry>(this.baseUrl, data);
  }

  updateEntry(entryId: string, data: KBEntryUpdate): Observable<KBEntry> {
    return this.http.put<KBEntry>(`${this.baseUrl}/${entryId}`, data);
  }

  deleteEntry(entryId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${entryId}`);
  }

  getCategoryStats(): Observable<KBCategoryStat[]> {
    return this.http.get<KBCategoryStat[]>(`${this.baseUrl}/categories`);
  }

  getSuggestions(params?: {
    category?: string;
    subcategory?: string;
    keywords?: string;
    tender_id?: string;
    limit?: number;
  }): Observable<KBEntry[]> {
    let httpParams = new HttpParams();
    if (params?.category) httpParams = httpParams.set('category', params.category);
    if (params?.subcategory) httpParams = httpParams.set('subcategory', params.subcategory);
    if (params?.keywords) httpParams = httpParams.set('keywords', params.keywords);
    if (params?.tender_id) httpParams = httpParams.set('tender_id', params.tender_id);
    if (params?.limit) httpParams = httpParams.set('limit', String(params.limit));
    return this.http.get<KBEntry[]>(`${this.baseUrl}/suggest`, { params: httpParams });
  }

  recordUsage(entryId: string, tenderId: string, usageContext?: string): Observable<KBEntry> {
    return this.http.post<KBEntry>(`${this.baseUrl}/record-usage`, {
      entry_id: entryId,
      tender_id: tenderId,
      usage_context: usageContext,
    });
  }

  harvestFromTender(tenderId: string): Observable<HarvestResult> {
    return this.http.post<HarvestResult>(`${this.baseUrl}/harvest/tender/${tenderId}`, {});
  }

  harvestCompanyData(): Observable<HarvestResult> {
    return this.http.post<HarvestResult>(`${this.baseUrl}/harvest/company`, {});
  }

  getDashboardStats(): Observable<KBDashboardStats> {
    return this.http.get<KBDashboardStats>(`${this.baseUrl}/dashboard-stats`);
  }

  generateSummary(tenderId: string, entryIds: string[]): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate-summary`, {
      tender_id: tenderId,
      entry_ids: entryIds,
    }, { responseType: 'blob' });
  }
}
