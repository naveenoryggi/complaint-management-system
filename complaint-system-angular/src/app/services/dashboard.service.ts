import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { CacheService } from './cache.service';
import {
  DashboardPreferences,
  DashboardStatistics,
  SaveDashboardPreferencesRequest
} from '../models/dashboard.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(
    private http: HttpClient,
    private cacheService: CacheService
  ) {}

  /**
   * Get dashboard preferences for the current user (cached)
   */
  getPreferences(): Observable<ApiResponse<DashboardPreferences>> {
    return this.cacheService.get(
      'user-preferences',
      () => this.http.get<ApiResponse<DashboardPreferences>>(`${this.apiUrl}/preferences`),
      'userPreferences'
    );
  }

  /**
   * Save or update dashboard preferences for the current user
   */
  savePreferences(request: SaveDashboardPreferencesRequest): Observable<ApiResponse<DashboardPreferences>> {
    return this.http.put<ApiResponse<DashboardPreferences>>(`${this.apiUrl}/preferences`, request).pipe(
      map(response => {
        // Invalidate cache when preferences are updated
        this.cacheService.invalidate('user-preferences', 'userPreferences');
        return response;
      })
    );
  }

  /**
   * Get dashboard statistics with status widgets (cached)
   */
  getStatistics(dateRangeDays?: number, statusIds?: string[]): Observable<ApiResponse<DashboardStatistics>> {
    // Create cache key based on parameters
    const paramsHash = this.createParamsHash({ dateRangeDays, statusIds });
    const cacheKey = `statistics-${paramsHash}`;

    return this.cacheService.get(
      cacheKey,
      () => {
        let params = new HttpParams();

        if (dateRangeDays !== undefined) {
          params = params.set('dateRangeDays', dateRangeDays.toString());
        }

        if (statusIds && statusIds.length > 0) {
          statusIds.forEach(id => {
            params = params.append('statusIds', id);
          });
        }

        return this.http.get<ApiResponse<DashboardStatistics>>(`${this.apiUrl}/statistics`, { params });
      },
      'dashboard'
    );
  }

  /**
   * Reset dashboard preferences to default for the current user
   */
  resetPreferences(): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/preferences`).pipe(
      map(response => {
        // Invalidate cache when preferences are reset
        this.cacheService.invalidate('user-preferences', 'userPreferences');
        return response;
      })
    );
  }

  /**
   * Force refresh of cached dashboard statistics
   */
  refreshStatistics(dateRangeDays?: number, statusIds?: string[]): Observable<ApiResponse<DashboardStatistics>> {
    const paramsHash = this.createParamsHash({ dateRangeDays, statusIds });
    const cacheKey = `statistics-${paramsHash}`;

    // Invalidate cache to force refresh
    this.cacheService.invalidate(cacheKey, 'dashboard');

    // Get fresh data
    return this.getStatistics(dateRangeDays, statusIds);
  }

  /**
   * Preload dashboard data for better performance
   */
  preloadDashboardData(): Observable<{ preferences: DashboardPreferences; statistics: DashboardStatistics }> {
    const preferences$ = this.cacheService.preload(
      'user-preferences',
      () => this.http.get<ApiResponse<DashboardPreferences>>(`${this.apiUrl}/preferences`).pipe(
        map(response => response.data!)
      ),
      'userPreferences'
    );

    const statistics$ = this.cacheService.preload(
      'statistics-default',
      () => {
        const params = new HttpParams().set('dateRangeDays', '30');
        return this.http.get<ApiResponse<DashboardStatistics>>(`${this.apiUrl}/statistics`, { params }).pipe(
          map(response => response.data!)
        );
      },
      'dashboard'
    );

    return this.cacheService.createCachedObservable(
      'dashboard-preload',
      () => preferences$,
      'dashboard'
    ).pipe(
      map(preferences => ({ preferences, statistics: null as any }))
    );
  }

  /**
   * Clear all dashboard-related cache entries
   */
  clearCache(): void {
    this.cacheService.invalidateGroup('dashboard');
    this.cacheService.invalidateGroup('userPreferences');
  }

  /**
   * Get cache statistics for dashboard operations
   */
  getCacheStats(): any {
    return this.cacheService.getStats();
  }

  /**
   * Create a hash from parameters for cache key generation
   */
  private createParamsHash(params: any): string {
    return btoa(JSON.stringify(params)).replace(/[+/=]/g, '').substring(0, 8);
  }
}
