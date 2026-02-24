import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface GenerationContext {
  tender_title: string;
  tender_reference?: string;
  issuing_authority: string;
  company_name: string;
  company_profile?: string;
  experience?: string;
  requirements: string[] | string;
  scope_of_work: string;
  company_address?: string;
}

export interface AIGenerateRequest {
  generation_type: 'technical_solution' | 'compliance_declaration' | 'covering_letter' | 'executive_summary' | 'methodology' | 'proposal';
  context: GenerationContext;
  model?: string;
  max_tokens?: number;
  temperature?: number;
  save_as_document?: boolean;
  document_name?: string;
}

export interface AIGenerationResponse {
  id: string;
  generation_type: string;
  content: string;
  tokens_used: number;
  model_used: string;
  document_id?: string;
  created_at: string;
}

export interface AIUsageStats {
  total_generations: number;
  total_tokens_used: number;
  estimated_cost_usd: number;
  generations_by_type: { [key: string]: number };
  current_month_tokens: number;
  current_month_cost_usd: number;
}

export interface GenerationType {
  value: string;
  label: string;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
export class AIService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/ai`;

  /**
   * Generate a document using AI
   */
  generateDocument(request: AIGenerateRequest): Observable<AIGenerationResponse> {
    return this.http.post<AIGenerationResponse>(`${this.baseUrl}/generate`, request);
  }

  /**
   * Get generation history
   */
  getGenerationHistory(limit: number = 50): Observable<AIGenerationResponse[]> {
    return this.http.get<AIGenerationResponse[]>(`${this.baseUrl}/history`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Get usage statistics
   */
  getUsageStats(): Observable<AIUsageStats> {
    return this.http.get<AIUsageStats>(`${this.baseUrl}/usage`);
  }

  /**
   * Get available templates
   */
  getTemplates(): Observable<{ [key: string]: string }> {
    return this.http.get<{ [key: string]: string }>(`${this.baseUrl}/templates`);
  }

  /**
   * Get generation types
   */
  getGenerationTypes(): Observable<GenerationType[]> {
    return this.http.get<GenerationType[]>(`${this.baseUrl}/generation-types`);
  }

  /**
   * Get template details
   */
  getTemplateDetails(generationType: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/template/${generationType}`);
  }

  /**
   * Preview prompt
   */
  previewPrompt(generationType: string, context: GenerationContext): Observable<any> {
    return this.http.post(`${this.baseUrl}/preview`, context, {
      params: { generation_type: generationType }
    });
  }
}
