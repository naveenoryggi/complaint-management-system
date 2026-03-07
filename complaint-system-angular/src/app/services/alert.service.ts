import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface TenderAlert {
  id: string;
  tender_id: string;
  tender_title: string;
  alert_type: 'tender_deadline' | 'emd_expiry' | 'fee_due';
  severity: 'critical' | 'warning' | 'info';
  message: string;
  due_date: string | null;
  days_remaining: number;
}

export interface AlertSummary {
  critical: number;
  warning: number;
  info: number;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class AlertService {
  private http = inject(HttpClient);
  private baseUrl = `${runtimeConfig.pythonApiUrl}/api/v1/alerts`;

  getAlerts(): Observable<TenderAlert[]> {
    return this.http.get<TenderAlert[]>(this.baseUrl);
  }

  dismissAlert(alertId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${alertId}/dismiss`, {});
  }

  getAlertSummary(): Observable<AlertSummary> {
    return this.http.get<AlertSummary>(`${this.baseUrl}/summary`);
  }
}
