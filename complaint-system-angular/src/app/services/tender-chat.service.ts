import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface ChatMessage {
  id: string;
  tender_id: string;
  role: 'user' | 'assistant';
  content: string;
  tokens_used: number | null;
  model_used: string | null;
  user_name: string | null;
  created_at: string | null;
}

export interface ChatHistory {
  messages: ChatMessage[];
  total: number;
  page: number;
  page_size: number;
}

export interface ChatResponse {
  user_message: ChatMessage;
  assistant_message: ChatMessage;
}

@Injectable({
  providedIn: 'root'
})
export class TenderChatService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/tenders`;

  getHistory(tenderId: string, page = 1, pageSize = 50): Observable<ChatHistory> {
    const params = new HttpParams().set('page', page).set('page_size', pageSize);
    return this.http.get<ChatHistory>(`${this.baseUrl}/${tenderId}/chat`, { params });
  }

  sendMessage(tenderId: string, message: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.baseUrl}/${tenderId}/chat`, { message });
  }

  clearHistory(tenderId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${tenderId}/chat`);
  }
}
