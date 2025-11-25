import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SyncResult {
  syncLogId: string;
  duration: number;
  companies: {
    created: number;
    updated: number;
    total: number;
  };
  branches: {
    created: number;
    updated: number;
    total: number;
  };
  departments: {
    created: number;
    updated: number;
    total: number;
  };
  sections: {
    created: number;
    updated: number;
    total: number;
  };
  employees: {
    created: number;
    updated: number;
    total: number;
  };
}

export interface SyncResponse {
  success: boolean;
  message: string;
  data?: SyncResult;
  error?: string;
  details?: string;
}

export interface SyncHistoryItem {
  syncLogId: string;
  syncType: string;
  status: string;
  startedAt: string;
  completedAt: string;
  duration: number;
  companies: {
    created: number;
    updated: number;
    total: number;
  };
  branches: {
    created: number;
    updated: number;
    total: number;
  };
  departments: {
    created: number;
    updated: number;
    total: number;
  };
  sections: {
    created: number;
    updated: number;
    total: number;
  };
  employees: {
    created: number;
    updated: number;
    total: number;
  };
  users?: {
    created: number;
    updated: number;
    total: number;
  };
  error?: string;
}

export interface SyncHistoryResponse {
  success: boolean;
  data: SyncHistoryItem[];
}

export interface SyncStatusResponse {
  success: boolean;
  message?: string;
  data?: SyncHistoryItem;
}

export interface SyncSchedule {
  id: string;
  tenantId: string;
  scheduleType: string; // "Daily", "Weekly", "Monthly"
  timeOfDay: string; // HH:mm format
  dayValue?: number; // For Weekly (0-6) or Monthly (1-31)
  isEnabled: boolean;
  lastRunAt?: string;
  nextRunAt?: string;
  description?: string;
  createdAt: string;
}

export interface SyncScheduleRequest {
  scheduleType: string;
  timeOfDay: string;
  dayValue?: number;
  isEnabled?: boolean;
  description?: string;
}

export interface ScheduleResponse {
  success: boolean;
  message?: string;
  data?: SyncSchedule;
}

export interface ScheduleListResponse {
  success: boolean;
  data: SyncSchedule[];
}

export interface SqlHealthCheck {
  blockedProcessCount: number;
  longRunningQueryCount: number;
  openTransactionCount: number;
  stuckSyncCount: number;
  checkTime: string;
  overallStatus: string; // "Healthy", "Warning", "Critical"
}

export interface BlockedProcess {
  blockingSessionId: number;
  blockedSessionId: number;
  blockingQuery: string;
  blockedQuery: string;
  blockingUser: string;
  blockedUser: string;
  waitType: string;
  waitTimeSeconds: number;
  blockedStatus: string;
  blockingStatus: string;
}

export interface LongRunningQuery {
  sessionId: number;
  loginName: string;
  programName: string;
  status: string;
  command: string;
  elapsedTimeSeconds: number;
  queryText: string;
  blockingSessionId?: number;
  waitType: string;
  reads: number;
  writes: number;
}

export interface OpenTransaction {
  sessionId: number;
  loginName: string;
  programName: string;
  transactionId: number;
  transactionName: string;
  beginTime: string;
  durationSeconds: number;
  transactionState: string;
  queryText: string;
}

export interface SqlDiagnostics {
  healthCheck: SqlHealthCheck;
  blockedProcesses: BlockedProcess[];
  longRunningQueries: LongRunningQuery[];
  openTransactions: OpenTransaction[];
}

export interface SqlDiagnosticsResponse {
  success: boolean;
  data: SqlDiagnostics;
}

export interface SqlHealthCheckResponse {
  success: boolean;
  data: SqlHealthCheck;
}

export interface KillSessionRequest {
  sessionId: number;
  reason: string;
}

export interface KillSessionResult {
  success: boolean;
  message: string;
  sessionId: number;
}

export interface KillSessionResponse {
  success: boolean;
  message?: string;
  data?: KillSessionResult;
}

export interface CleanupResponse {
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class OryggiSyncService {
  private apiUrl = `${environment.apiUrl}/OryggiSync`;

  constructor(private http: HttpClient) {}

  /**
   * Trigger a manual sync from Oryggi database
   * TenantId is optional - backend will use default tenant if not provided
   * Timeout set to 10 minutes for large sync operations
   */
  triggerSync(tenantId?: string | null): Observable<SyncResponse> {
    const body = tenantId ? { tenantId } : {};
    return this.http.post<SyncResponse>(`${this.apiUrl}/trigger`, body, {
      // 10 minute timeout for large sync operations
      // Angular HttpClient uses rxjs timeout operator
      // We'll set this in the component instead using pipe(timeout(600000))
    });
  }

  /**
   * Get sync history for a tenant
   */
  getSyncHistory(tenantId: string, count: number = 10): Observable<SyncHistoryResponse> {
    return this.http.get<SyncHistoryResponse>(`${this.apiUrl}/history/${tenantId}?count=${count}`);
  }

  /**
   * Get latest sync status for a tenant
   */
  getSyncStatus(tenantId: string): Observable<SyncStatusResponse> {
    return this.http.get<SyncStatusResponse>(`${this.apiUrl}/status/${tenantId}`);
  }

  /**
   * Get all sync schedules
   */
  getSchedules(): Observable<ScheduleListResponse> {
    return this.http.get<ScheduleListResponse>(`${this.apiUrl}/schedules`);
  }

  /**
   * Get a specific schedule by ID
   */
  getSchedule(id: string): Observable<ScheduleResponse> {
    return this.http.get<ScheduleResponse>(`${this.apiUrl}/schedules/${id}`);
  }

  /**
   * Create a new sync schedule
   */
  createSchedule(schedule: SyncScheduleRequest): Observable<ScheduleResponse> {
    return this.http.post<ScheduleResponse>(`${this.apiUrl}/schedules`, schedule);
  }

  /**
   * Update an existing sync schedule
   */
  updateSchedule(id: string, schedule: SyncScheduleRequest): Observable<ScheduleResponse> {
    return this.http.put<ScheduleResponse>(`${this.apiUrl}/schedules/${id}`, schedule);
  }

  /**
   * Delete a sync schedule
   */
  deleteSchedule(id: string): Observable<ScheduleResponse> {
    return this.http.delete<ScheduleResponse>(`${this.apiUrl}/schedules/${id}`);
  }

  /**
   * Get comprehensive SQL diagnostics
   */
  getDiagnostics(): Observable<SqlDiagnosticsResponse> {
    return this.http.get<SqlDiagnosticsResponse>(`${this.apiUrl}/diagnostics`);
  }

  /**
   * Get SQL health check summary
   */
  getHealthCheck(): Observable<SqlHealthCheckResponse> {
    return this.http.get<SqlHealthCheckResponse>(`${this.apiUrl}/diagnostics/health`);
  }

  /**
   * Kill a specific SQL session
   */
  killSession(request: KillSessionRequest): Observable<KillSessionResponse> {
    return this.http.post<KillSessionResponse>(`${this.apiUrl}/diagnostics/kill-session`, request);
  }

  /**
   * Clean up stuck syncs by marking them as failed
   */
  cleanupStuckSyncs(): Observable<CleanupResponse> {
    return this.http.post<CleanupResponse>(`${this.apiUrl}/diagnostics/cleanup-stuck-syncs`, {});
  }

  /**
   * Clean up a single stuck sync by marking it as failed
   */
  cleanupSingleSync(syncLogId: string): Observable<CleanupResponse> {
    return this.http.post<CleanupResponse>(`${this.apiUrl}/diagnostics/cleanup-sync/${syncLogId}`, {});
  }

  /**
   * Sync a single employee from Oryggi by their employee code
   */
  syncSingleEmployee(corpEmpCode: string, tenantId?: string): Observable<any> {
    const params = tenantId ? `?tenantId=${tenantId}` : '';
    return this.http.post<any>(`${this.apiUrl}/employee/${corpEmpCode}${params}`, {});
  }
}
