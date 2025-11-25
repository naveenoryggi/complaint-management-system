import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../environments/environment';
// Force recompile

// ============================
// Type Definitions
// ============================

export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  error?: string;
}

export type UrgencyLevel = 'green' | 'yellow' | 'orange' | 'red';
export type SLATimingStatus = 'met' | 'pending' | 'breached' | 'on-track' | 'warning' | 'urgent';

export interface ApplicableSLA {
  slaLevelId: string;
  slaLevelName: string;
  colorCode: string;
  responseTimeMinutes: number;
  resolutionTimeMinutes: number;
  responseTimeDisplay: string;
  resolutionTimeDisplay: string;
  source: 'category' | 'priority' | 'default';
}

export interface SLAStatusDisplay {
  complaintId: string;
  complaintNumber: string;
  slaLevel: {
    id: string;
    name: string;
    colorCode: string;
  };
  response: {
    targetHours: number;
    targetMinutes: number;
    elapsedHours: number;
    elapsedMinutes: number;
    remainingHours: number;
    remainingMinutes: number;
    percentComplete: number;
    status: SLATimingStatus;
    dueDate: Date | string;
    metDate?: Date | string;
  };
  resolution: {
    targetHours: number;
    targetMinutes: number;
    elapsedHours: number;
    elapsedMinutes: number;
    remainingHours: number;
    remainingMinutes: number;
    percentComplete: number;
    status: SLATimingStatus;
    dueDate: Date | string;
    resolvedDate?: Date | string;
  };
  urgencyLevel: UrgencyLevel;
  urgencyLabel: string;
  isPaused: boolean;
  pauseReason?: string;
}

export interface SLAStatusSummary {
  complaintId: string;
  urgencyLevel: UrgencyLevel;
  remainingMinutes: number;
  percentComplete: number;
  status: SLATimingStatus;
}

export interface SLATimelineEvent {
  timestamp: Date | string;
  eventType: 'submitted' | 'response_given' | 'paused' | 'resumed' | 'resolved' | 'breached';
  description: string;
  actor?: string;
  icon?: string;
  colorClass?: string;
}

export interface SLACoverageCell {
  categoryId: string;
  categoryName: string;
  priorityId: string;
  priorityName: string;
  slaLevelId?: string;
  slaLevelName?: string;
  responseTime?: string;
  resolutionTime?: string;
  isMapped: boolean;
  colorCode?: string;
}

export interface SLACoverageMatrix {
  categories: Array<{ id: string; name: string }>;
  priorities: Array<{ id: string; name: string }>;
  mappings: Record<string, SLACoverageCell>;
  unmappedCount: number;
  mappedCount: number;
}

export interface SLAWarning {
  complaintId: string;
  complaintNumber: string;
  title: string;
  assignedTo: string;
  urgencyLevel: UrgencyLevel;
  remainingMinutes: number;
  percentComplete: number;
  estimatedBreachTime: Date | string;
}

/**
 * SLA (Service Level Agreement) Service
 * Manages all SLA-related API calls and state
 */
@Injectable({
  providedIn: 'root'
})
export class SLAService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/sla`;

  // State management
  private settingsSubject = new BehaviorSubject<SLASettings | null>(null);
  private levelsSubject = new BehaviorSubject<SLALevel[]>([]);

  public settings$ = this.settingsSubject.asObservable();
  public levels$ = this.levelsSubject.asObservable();

  // ===================
  // Global SLA Settings
  // ===================

  /**
   * Get global SLA settings for the company
   */
  getSettings(): Observable<ApiResponse<SLASettings>> {
    return this.http.get<ApiResponse<SLASettings>>(`${this.baseUrl}/settings`).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.settingsSubject.next(response.data);
        }
      })
    );
  }

  /**
   * Update global SLA settings
   */
  updateSettings(settings: UpdateSLASettingsRequest): Observable<ApiResponse<SLASettings>> {
    return this.http.put<ApiResponse<SLASettings>>(`${this.baseUrl}/settings`, settings).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.settingsSubject.next(response.data);
        }
      })
    );
  }

  // ===========
  // SLA Levels
  // ===========

  /**
   * Get all SLA levels
   */
  getLevels(): Observable<ApiResponse<SLALevel[]>> {
    return this.http.get<ApiResponse<SLALevel[]>>(`${this.baseUrl}/levels`).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.levelsSubject.next(response.data);
        }
      })
    );
  }

  /**
   * Get specific SLA level by ID
   */
  getLevel(id: string): Observable<ApiResponse<SLALevel>> {
    return this.http.get<ApiResponse<SLALevel>>(`${this.baseUrl}/levels/${id}`);
  }

  /**
   * Create new SLA level
   */
  createLevel(level: CreateSLALevelRequest): Observable<ApiResponse<SLALevel>> {
    return this.http.post<ApiResponse<SLALevel>>(`${this.baseUrl}/levels`, level).pipe(
      tap(() => this.getLevels().subscribe()) // Refresh list
    );
  }

  /**
   * Update existing SLA level
   */
  updateLevel(id: string, level: UpdateSLALevelRequest): Observable<ApiResponse<SLALevel>> {
    return this.http.put<ApiResponse<SLALevel>>(`${this.baseUrl}/levels/${id}`, level).pipe(
      tap(() => this.getLevels().subscribe()) // Refresh list
    );
  }

  /**
   * Delete SLA level
   */
  deleteLevel(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/levels/${id}`).pipe(
      tap(() => this.getLevels().subscribe()) // Refresh list
    );
  }

  // =====================
  // Category SLA Mappings
  // =====================

  /**
   * Get all category SLA mappings
   */
  getCategoryMappings(): Observable<ApiResponse<CategorySLA[]>> {
    return this.http.get<ApiResponse<CategorySLA[]>>(`${this.baseUrl}/category-mappings`);
  }

  /**
   * Create or update category SLA mapping
   */
  saveCategoryMapping(mapping: CreateCategorySLARequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/category-mappings`, mapping);
  }

  /**
   * Bulk update category mappings
   */
  bulkUpdateCategoryMappings(mappings: CategorySLAMapping[]): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/category-mappings/bulk`, { mappings });
  }

  /**
   * Delete category SLA mapping
   */
  deleteCategoryMapping(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/category-mappings/${id}`);
  }

  // =====================
  // Priority SLA Mappings
  // =====================

  /**
   * Get all priority SLA mappings
   */
  getPriorityMappings(): Observable<ApiResponse<PrioritySLA[]>> {
    return this.http.get<ApiResponse<PrioritySLA[]>>(`${this.baseUrl}/priority-mappings`);
  }

  /**
   * Create or update priority SLA mapping
   */
  savePriorityMapping(mapping: CreatePrioritySLARequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/priority-mappings`, mapping);
  }

  /**
   * Bulk update priority mappings
   */
  bulkUpdatePriorityMappings(mappings: PrioritySLAMapping[]): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/priority-mappings/bulk`, { mappings });
  }

  /**
   * Delete priority SLA mapping
   */
  deletePriorityMapping(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/priority-mappings/${id}`);
  }

  // ==============
  // Helper Methods
  // ==============

  /**
   * Calculate SLA deadline from start time
   */
  calculateDeadline(startTime: Date, minutes: number, workingHoursOnly: boolean): Date {
    // Simple calculation - can be enhanced with working hours logic
    const deadline = new Date(startTime);
    if (workingHoursOnly) {
      // TODO: Implement working hours calculation
      deadline.setMinutes(deadline.getMinutes() + minutes);
    } else {
      deadline.setMinutes(deadline.getMinutes() + minutes);
    }
    return deadline;
  }

  /**
   * Format time remaining for display
   */
  formatTimeRemaining(deadline: Date): string {
    const now = new Date();
    const diff = deadline.getTime() - now.getTime();

    if (diff < 0) return 'Overdue';

    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days > 0) return `${days}d ${hours % 24}h`;
    if (hours > 0) return `${hours}h ${minutes % 60}m`;
    return `${minutes}m`;
  }

  /**
   * Get SLA status color based on percentage complete
   */
  getSLAStatusColor(percentComplete: number): string {
    if (percentComplete >= 100) return '#f44336'; // Red - breached
    if (percentComplete >= 80) return '#ff9800';  // Orange - warning
    return '#4caf50';  // Green - on track
  }

  // ========================
  // NEW: Phase 1 Visibility
  // ========================

  /**
   * Get applicable SLA for a specific category and priority combination
   */
  getApplicableSLA(categoryId: string, priorityId: string): Observable<ApiResponse<ApplicableSLA>> {
    return this.http.get<ApiResponse<ApplicableSLA>>(
      `${this.baseUrl}/applicable?categoryId=${categoryId}&priorityId=${priorityId}`
    );
  }

  /**
   * Get real-time SLA status for a specific complaint
   */
  getSLAStatus(complaintId: string): Observable<ApiResponse<SLAStatusDisplay>> {
    return this.http.get<ApiResponse<SLAStatusDisplay>>(`${this.baseUrl}/status/${complaintId}`);
  }

  /**
   * Get SLA status for multiple complaints (bulk operation)
   */
  getBulkSLAStatus(complaintIds: string[]): Observable<ApiResponse<Map<string, SLAStatusSummary>>> {
    return this.http.post<ApiResponse<Map<string, SLAStatusSummary>>>(
      `${this.baseUrl}/status/bulk`,
      { complaintIds }
    );
  }

  /**
   * Get SLA timeline/events for a specific complaint
   */
  getSLATimeline(complaintId: string): Observable<ApiResponse<SLATimelineEvent[]>> {
    return this.http.get<ApiResponse<SLATimelineEvent[]>>(`${this.baseUrl}/timeline/${complaintId}`);
  }

  /**
   * Get SLA coverage matrix (Admin only)
   */
  getCoverageMatrix(): Observable<ApiResponse<SLACoverageMatrix>> {
    return this.http.get<ApiResponse<SLACoverageMatrix>>(`${this.baseUrl}/coverage-matrix`);
  }

  /**
   * Get SLA breach warnings for user
   */
  getSLAWarnings(userId?: string, onlyMyTickets: boolean = true): Observable<ApiResponse<SLAWarning[]>> {
    const params = userId ? `?userId=${userId}&onlyMyTickets=${onlyMyTickets}` : '';
    return this.http.get<ApiResponse<SLAWarning[]>>(`${this.baseUrl}/warnings${params}`);
  }

  /**
   * Format minutes into human-readable time string
   */
  formatMinutes(minutes: number): string {
    if (minutes < 0) return 'Overdue';
    if (minutes === 0) return 'Due now';

    const days = Math.floor(minutes / 1440);
    const hours = Math.floor((minutes % 1440) / 60);
    const mins = minutes % 60;

    const parts: string[] = [];
    if (days > 0) parts.push(`${days}d`);
    if (hours > 0) parts.push(`${hours}h`);
    if (mins > 0 && days === 0) parts.push(`${mins}m`);

    return parts.join(' ') || '0m';
  }

  /**
   * Get urgency level from percentage complete
   */
  getUrgencyLevel(percentComplete: number): UrgencyLevel {
    if (percentComplete >= 100) return 'red';
    if (percentComplete >= 90) return 'red';
    if (percentComplete >= 75) return 'orange';
    if (percentComplete >= 60) return 'yellow';
    return 'green';
  }

  /**
   * Get urgency label from urgency level
   */
  getUrgencyLabel(urgency: UrgencyLevel): string {
    const labels: Record<UrgencyLevel, string> = {
      'green': 'On Track',
      'yellow': 'Warning',
      'orange': 'Urgent',
      'red': 'Breached'
    };
    return labels[urgency];
  }
}

// ================
// Existing Type Definitions
// ================

export interface SLASettings {
  id: string;
  isEnabled: boolean;
  workingHoursOnly: boolean;
  workingHoursStart?: string;
  workingHoursEnd?: string;
  workingDays: string;
  excludeHolidays: boolean;
  autoEscalateOnBreach: boolean;
  escalationThresholdPercent: number;
  notifyBeforeBreach: boolean;
  notifyBeforeBreachMinutes: number;
  pauseSLAOnPendingInfo: boolean;
  timezone: string;
  companyId?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateSLASettingsRequest {
  isEnabled: boolean;
  workingHoursOnly: boolean;
  workingHoursStart?: string;
  workingHoursEnd?: string;
  workingDays: string;
  excludeHolidays: boolean;
  autoEscalateOnBreach: boolean;
  escalationThresholdPercent: number;
  notifyBeforeBreach: boolean;
  notifyBeforeBreachMinutes: number;
  pauseSLAOnPendingInfo: boolean;
  timezone: string;
}

export interface SLALevel {
  id: string;
  name: string;
  description?: string;
  order: number;
  isActive: boolean;
  colorCode: string;
  defaultResponseTime: number;
  responseTimeUnit: string;
  defaultResolutionTime: number;
  resolutionTimeUnit: string;
  responseTimeInMinutes: number;
  resolutionTimeInMinutes: number;
  responseTimeDisplay: string;
  resolutionTimeDisplay: string;
  companyId?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateSLALevelRequest {
  name: string;
  description?: string;
  order: number;
  isActive: boolean;
  colorCode: string;
  defaultResponseTime: number;
  responseTimeUnit: string;
  defaultResolutionTime: number;
  resolutionTimeUnit: string;
}

export interface UpdateSLALevelRequest {
  name: string;
  description?: string;
  order: number;
  isActive: boolean;
  colorCode: string;
  defaultResponseTime: number;
  responseTimeUnit: string;
  defaultResolutionTime: number;
  resolutionTimeUnit: string;
}

export interface CategorySLA {
  id: string;
  categoryId: string;
  categoryName: string;
  slaLevelId: string;
  slaLevelName: string;
  slaLevelColorCode: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  effectiveResponseTimeMinutes: number;
  effectiveResolutionTimeMinutes: number;
  effectiveResponseTimeDisplay: string;
  effectiveResolutionTimeDisplay: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCategorySLARequest {
  categoryId: string;
  slaLevelId: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  isActive: boolean;
}

export interface CategorySLAMapping {
  categoryId: string;
  slaLevelId: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  isActive: boolean;
}

export interface PrioritySLA {
  id: string;
  priorityId: string;
  priorityName: string;
  slaLevelId: string;
  slaLevelName: string;
  slaLevelColorCode: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  effectiveResponseTimeMinutes: number;
  effectiveResolutionTimeMinutes: number;
  effectiveResponseTimeDisplay: string;
  effectiveResolutionTimeDisplay: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePrioritySLARequest {
  priorityId: string;
  slaLevelId: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  isActive: boolean;
}

export interface PrioritySLAMapping {
  priorityId: string;
  slaLevelId: string;
  overrideResponseTime?: number;
  overrideResolutionTime?: number;
  isActive: boolean;
}
