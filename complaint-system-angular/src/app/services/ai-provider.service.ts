import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface FeatureMapping {
  [key: string]: boolean | undefined;
  extraction?: boolean;
  declarations?: boolean;
  prebid?: boolean;
  document_generation?: boolean;
  alignment?: boolean;
}

export interface AIProvider {
  id: string;
  tenant_id: string;
  provider_name: string;
  display_name: string;
  api_key_masked: string;
  endpoint_url?: string;
  deployment_name?: string;
  api_version?: string;
  default_model: string;
  feature_mapping: FeatureMapping;
  is_active: boolean;
  is_default: boolean;
  total_tokens_used: number;
  total_requests: number;
  last_used_at?: string;
  last_test_at?: string;
  last_test_success?: boolean;
  created_at?: string;
  updated_at?: string;
}

export interface AIProviderCreate {
  provider_name: string;
  display_name: string;
  api_key: string;
  endpoint_url?: string;
  deployment_name?: string;
  api_version?: string;
  default_model: string;
  feature_mapping?: FeatureMapping;
  is_active?: boolean;
  is_default?: boolean;
}

export interface AIProviderUpdate {
  display_name?: string;
  api_key?: string;
  endpoint_url?: string;
  deployment_name?: string;
  api_version?: string;
  default_model?: string;
  feature_mapping?: FeatureMapping;
  is_active?: boolean;
  is_default?: boolean;
}

export interface TestConnectionResult {
  success: boolean;
  latency_ms: number;
  response?: string;
  error?: string;
}

export interface AIProviderListResponse {
  providers: AIProvider[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class AIProviderService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/ai-providers`;

  listProviders(): Observable<AIProviderListResponse> {
    return this.http.get<AIProviderListResponse>(this.baseUrl);
  }

  getProvider(id: string): Observable<AIProvider> {
    return this.http.get<AIProvider>(`${this.baseUrl}/${id}`);
  }

  createProvider(data: AIProviderCreate): Observable<AIProvider> {
    return this.http.post<AIProvider>(this.baseUrl, data);
  }

  updateProvider(id: string, data: AIProviderUpdate): Observable<AIProvider> {
    return this.http.put<AIProvider>(`${this.baseUrl}/${id}`, data);
  }

  deleteProvider(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  testConnection(id: string): Observable<TestConnectionResult> {
    return this.http.post<TestConnectionResult>(`${this.baseUrl}/${id}/test`, {});
  }

  setDefault(id: string): Observable<AIProvider> {
    return this.http.put<AIProvider>(`${this.baseUrl}/${id}/set-default`, {});
  }

  updateFeatures(id: string, featureMapping: FeatureMapping): Observable<AIProvider> {
    return this.http.put<AIProvider>(`${this.baseUrl}/${id}/features`, { feature_mapping: featureMapping });
  }
}
