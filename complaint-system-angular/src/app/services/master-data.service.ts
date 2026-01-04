import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import { CacheService } from './cache.service';

export interface StatusOption {
  value: string;  // Changed from number to string (GUID) for master-based system
  label: string;
  description: string;
}

export interface PriorityOption {
  id?: string;
  value: string;  // Changed from number to string (GUID) for master-based system
  label: string;
  code?: string;
  description?: string;
  level?: number;
  colorCode?: string;
  iconClass?: string;
  displayOrder?: number;
  isSystem?: boolean;
  slaResponseHours?: number;
  slaResolutionHours?: number;
  isActive?: boolean;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  private get apiUrl(): string {
    return runtimeConfig.apiUrl;
  }
  private priorityOptions: PriorityOption[] = [];
  private statusOptions: StatusOption[] = [];

  constructor(
    private http: HttpClient,
    private cacheService: CacheService
  ) {}

  /**
   * Get all status master records for filters (cached)
   */
  getStatusOptions(): Observable<StatusOption[]> {
    return this.cacheService.get(
      'status-options',
      () => this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/complaintstatusmaster`).pipe(
        map(response => {
          if (response.isSuccess && response.data) {
            // Map status master records to dropdown options using GUID
            this.statusOptions = response.data
              .filter((status: any) => status.isActive)
              .sort((a: any, b: any) => a.displayOrder - b.displayOrder)
              .map((status: any) => ({
                value: status.id,  // Use GUID as value
                label: status.name,
                description: status.description || ''
              }));
            return this.statusOptions;
          }
          throw new Error(response.message || 'Failed to load status options');
        }),
        catchError(this.handleError)
      ),
      'referenceData'
    );
  }

  /**
   * Get all priority master records for filters (cached)
   */
  getPriorityOptions(): Observable<PriorityOption[]> {
    return this.cacheService.get(
      'priority-options',
      () => this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/complaintprioritymaster`).pipe(
        map(response => {
          if (response.isSuccess && response.data) {
            // Map priority master records to dropdown options using GUID
            const filteredPriorities = response.data
              .filter((priority: any) => {
                // Check if label is valid (not empty, null, or whitespace)
                const hasValidLabel = priority.name && priority.name.trim().length > 0;
                // Check if it's active
                const isActive = priority.isActive !== false;
                return hasValidLabel && isActive;
              })
              .map((priority: any) => ({
                id: priority.id,
                value: priority.id,  // Use GUID as value (changed from numeric level)
                label: priority.name.trim(),
                code: priority.code,
                description: priority.description,
                level: priority.level,
                colorCode: priority.colorCode,
                iconClass: priority.iconClass,
                displayOrder: priority.displayOrder,
                isSystem: priority.isSystem,
                slaResponseHours: priority.slaResponseHours,
                slaResolutionHours: priority.slaResolutionHours,
                isActive: priority.isActive
              }));

            // Sort by display order, then by level
            filteredPriorities.sort((a, b) => {
              if (a.displayOrder && b.displayOrder) {
                return a.displayOrder - b.displayOrder;
              }
              return (a.level || 0) - (b.level || 0);
            });

            this.priorityOptions = filteredPriorities;
            return filteredPriorities;
          }
          throw new Error(response.message || 'Failed to load priority options');
        }),
        catchError((error) => {
          // Fallback to system priorities if API fails
          console.warn('API failed, using fallback priority options:', error);
          return this.getFallbackPriorityOptions();
        })
      ),
      'referenceData'
    );
  }

  /**
   * Fallback method to provide system priorities if API fails (using placeholder GUIDs)
   */
  private getFallbackPriorityOptions(): Observable<PriorityOption[]> {
    const fallbackPriorities: PriorityOption[] = [
      { value: '20000000-0000-0000-0000-000000000001', label: 'Low', code: 'LOW', description: 'Low priority - No immediate action required', level: 1, colorCode: '#4CAF50', iconClass: 'bi-arrow-down-circle', displayOrder: 1, isSystem: true, slaResponseHours: 60, slaResolutionHours: 168, isActive: true },
      { value: '20000000-0000-0000-0000-000000000002', label: 'Normal', code: 'NORMAL', description: 'Normal priority - Standard processing time', level: 2, colorCode: '#2196F3', iconClass: 'bi-dash-circle', displayOrder: 2, isSystem: true, slaResponseHours: 48, slaResolutionHours: 120, isActive: true },
      { value: '20000000-0000-0000-0000-000000000003', label: 'High', code: 'HIGH', description: 'High priority - Requires expedited attention', level: 3, colorCode: '#FF9800', iconClass: 'bi-exclamation-circle', displayOrder: 3, isSystem: true, slaResponseHours: 24, slaResolutionHours: 72, isActive: true },
      { value: '20000000-0000-0000-0000-000000000004', label: 'Critical', code: 'CRITICAL', description: 'Critical priority - Requires immediate attention', level: 4, colorCode: '#F44336', iconClass: 'bi-exclamation-triangle', displayOrder: 4, isSystem: true, slaResponseHours: 4, slaResolutionHours: 24, isActive: true },
      { value: '20000000-0000-0000-0000-000000000005', label: 'Urgent', code: 'URGENT', description: 'Urgent priority - Highest priority level', level: 5, colorCode: '#9C27B0', iconClass: 'bi-lightning', displayOrder: 5, isSystem: true, slaResponseHours: 1, slaResolutionHours: 8, isActive: true }
    ];
    return new Observable(observer => {
      observer.next(fallbackPriorities);
      observer.complete();
    });
  }

  /**
   * Dynamic lookup methods for priorities (supports N number of user-defined priorities)
   */
  getPriorityName(priorityValue: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (priorityValue === null || priorityValue === undefined) {
      return 'Unknown';
    }

    // Handle string values directly from API (could be display name or GUID)
    if (typeof priorityValue === 'string') {
      // Check if it's a GUID (priorityId) - try to find in options
      const priority = this.priorityOptions.find(p => p.value === priorityValue);
      if (priority) {
        return priority.label;
      }
      // Otherwise it's already a display name
      return priorityValue;
    }

    // Handle numeric values (legacy enum values - shouldn't happen after migration)
    const priorityStr = String(priorityValue);
    const priority = this.priorityOptions.find(p => p.value === priorityStr);
    return priority?.label || 'Unknown';
  }

  getPriorityClass(priorityValue: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (priorityValue === null || priorityValue === undefined) {
      return 'priority-normal';
    }

    // Handle string values directly from API
    if (typeof priorityValue === 'string') {
      switch(priorityValue.toLowerCase()) {
        case 'critical':
        case 'urgent':
        case 'emergency':
        case 'severe':
          return 'priority-emergency';
        case 'high':
        case 'elevated':
          return 'priority-high';
        case 'normal':
        case 'medium':
          return 'priority-normal';
        case 'low':
          return 'priority-low';
        default:
          return 'priority-normal';
      }
    }

    // Handle numeric values (legacy - shouldn't happen after migration)
    const priorityStr = String(priorityValue);
    const priority = this.priorityOptions.find(p => p.value === priorityStr);
    if (priority?.colorCode) {
      // Use custom color if provided
      return `priority-custom`; // CSS will use the colorCode
    }

    // Fallback to level-based styling
    if (priority?.level) {
      if (priority.level >= 10) return 'priority-emergency';
      if (priority.level >= 8) return 'priority-critical';
      if (priority.level >= 6) return 'priority-urgent';
      if (priority.level >= 5) return 'priority-severe';
      if (priority.level >= 4) return 'priority-elevated';
      if (priority.level >= 3) return 'priority-high';
      if (priority.level >= 2) return 'priority-medium';
      if (priority.level >= 1) return 'priority-normal';
    }

    return 'priority-low';
  }

  getPriorityColor(priorityValue: number | string): string {
    // Handle string values (could be GUID or display name)
    if (typeof priorityValue === 'string') {
      const priority = this.priorityOptions.find(p => p.value === priorityValue);
      return priority?.colorCode || '#6c757d'; // Default gray color
    }

    // Handle numeric values (legacy)
    const priorityStr = String(priorityValue);
    const priority = this.priorityOptions.find(p => p.value === priorityStr);
    return priority?.colorCode || '#6c757d'; // Default gray color
  }

  getPriorityIcon(priorityValue: number | string): string {
    // Handle string values (could be GUID or display name)
    if (typeof priorityValue === 'string') {
      const priority = this.priorityOptions.find(p => p.value === priorityValue);
      return priority?.iconClass || 'bi-flag';
    }

    // Handle numeric values (legacy)
    const priorityStr = String(priorityValue);
    const priority = this.priorityOptions.find(p => p.value === priorityStr);
    return priority?.iconClass || 'bi-flag';
  }

  /**
   * Dynamic lookup methods for statuses
   */
  getStatusName(statusValue: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (statusValue === null || statusValue === undefined) {
      return 'Unknown';
    }

    // Handle string values directly from API (could be display name or GUID)
    if (typeof statusValue === 'string') {
      // Check if it's a GUID (statusId) - try to find in options
      const status = this.statusOptions.find(s => s.value === statusValue);
      if (status) {
        return status.label;
      }
      // Otherwise it's already a display name
      return statusValue;
    }

    // Handle numeric values (legacy enum values - shouldn't happen after migration)
    const statusStr = String(statusValue);
    const status = this.statusOptions.find(s => s.value === statusStr);
    if (status?.label) {
      // Convert camelCase to readable format
      return status.label
        .replace(/([A-Z])/g, ' $1')
        .replace(/^./, str => str.toUpperCase())
        .trim();
    }
    return 'Unknown';
  }

  getStatusClass(statusValue: number | string | undefined | null): string {
    // Add null/undefined safety check
    if (statusValue === null || statusValue === undefined) {
      return 'status-default';
    }

    // Handle string values directly from API (could be GUID or display name)
    if (typeof statusValue === 'string') {
      // Try to find in options first (in case it's a GUID)
      const status = this.statusOptions.find(s => s.value === statusValue);
      if (status) {
        // Use the status label for class determination
        switch(status.label.toLowerCase().replace(/\s+/g, '')) {
          case 'resolved':
          case 'closed':
            return 'status-resolved';
          case 'inprogress':
            return 'status-in-progress';
          case 'escalated':
            return 'status-escalated';
          case 'submitted':
          case 'underreview':
            return 'status-submitted';
          case 'pendinginfo':
            return 'status-pendinginfo';
          case 'rejected':
            return 'status-rejected';
          case 'reopened':
            return 'status-reopened';
          default:
            return 'status-default';
        }
      }

      // Fallback: assume it's already a display name
      switch(statusValue.toLowerCase().replace(/\s+/g, '')) {
        case 'resolved':
        case 'closed':
          return 'status-resolved';
        case 'inprogress':
          return 'status-in-progress';
        case 'escalated':
          return 'status-escalated';
        case 'submitted':
        case 'underreview':
          return 'status-submitted';
        case 'pendinginfo':
          return 'status-pendinginfo';
        case 'rejected':
          return 'status-rejected';
        case 'reopened':
          return 'status-reopened';
        default:
          return 'status-default';
      }
    }

    // Handle numeric values (legacy)
    const statusStr = String(statusValue);
    const status = this.statusOptions.find(s => s.value === statusStr);
    // You can enhance this with color codes from the API if needed
    switch(status?.label?.toLowerCase().replace(/\s+/g, '')) {
      case 'resolved':
      case 'closed':
        return 'status-resolved';
      case 'inprogress':
        return 'status-inprogress';
      case 'escalated':
        return 'status-escalated';
      case 'submitted':
      case 'underreview':
        return 'status-submitted';
      case 'pendinginfo':
        return 'status-pendinginfo';
      case 'rejected':
        return 'status-rejected';
      case 'reopened':
        return 'status-reopened';
      default:
        return 'status-default';
    }
  }

  
  /**
   * Clear master data cache
   */
  clearCache(): void {
    this.cacheService.invalidate('status-options', 'referenceData');
    this.cacheService.invalidate('priority-options', 'referenceData');

    // Clear local data
    this.statusOptions = [];
    this.priorityOptions = [];
  }

  /**
   * Force refresh of all master data
   */
  refreshMasterData(): Observable<{ statusOptions: StatusOption[]; priorityOptions: PriorityOption[] }> {
    // Invalidate cache entries
    this.cacheService.invalidate('status-options', 'referenceData');
    this.cacheService.invalidate('priority-options', 'referenceData');

    // Return combined observable for both data types
    const status$ = this.getStatusOptions();
    const priority$ = this.getPriorityOptions();

    return new Observable(observer => {
      let statusOptions: StatusOption[] = [];
      let priorityOptions: PriorityOption[] = [];
      let completed = 0;

      status$.subscribe({
        next: (data) => {
          statusOptions = data;
          completed++;
          if (completed === 2) {
            observer.next({ statusOptions, priorityOptions });
            observer.complete();
          }
        },
        error: (error) => observer.error(error)
      });

      priority$.subscribe({
        next: (data) => {
          priorityOptions = data;
          completed++;
          if (completed === 2) {
            observer.next({ statusOptions, priorityOptions });
            observer.complete();
          }
        },
        error: (error) => observer.error(error)
      });
    });
  }

  /**
   * Preload master data for better performance
   */
  preloadMasterData(): Observable<void> {
    return this.cacheService.preload(
      'master-data-preload',
      () => this.refreshMasterData().pipe(
        map(() => void 0)
      ),
      'referenceData'
    );
  }

  /**
   * Get cache statistics for master data operations
   */
  getCacheStats(): any {
    return this.cacheService.getStats();
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = error.error?.message || `Error Code: ${error.status}\nMessage: ${error.message}`;
    }

    console.error('MasterDataService Error:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }
}