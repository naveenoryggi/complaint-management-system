import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, tap, catchError, shareReplay } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import {
  LicenseInfo,
  ModuleConfig,
  ModuleMetadata,
  LicenseValidationResult,
  LicenseLimitCheckResult,
  ActivateLicenseRequest,
  ApiResponse,
  LicenseModule
} from '../models/license.model';

@Injectable({
  providedIn: 'root'
})
export class LicenseService {
  private get apiUrl() { return `${runtimeConfig.apiUrl}/licenses`; }

  // Cache for enabled modules
  private modulesCache$: Observable<ModuleConfig[]> | null = null;
  private modulesCacheTime: number = 0;
  private readonly CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

  // BehaviorSubject for reactive module updates
  private enabledModulesSubject = new BehaviorSubject<ModuleConfig[]>([]);
  public enabledModules$ = this.enabledModulesSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get license information for the current company
   */
  getLicenseInfo(): Observable<ApiResponse<LicenseInfo>> {
    return this.http.get<ApiResponse<LicenseInfo>>(`${this.apiUrl}/info`);
  }

  /**
   * Get all modules with their enabled status (cached)
   */
  getModules(forceRefresh: boolean = false): Observable<ModuleConfig[]> {
    const now = Date.now();

    // Return cached result if still valid
    if (!forceRefresh && this.modulesCache$ && (now - this.modulesCacheTime) < this.CACHE_DURATION) {
      return this.modulesCache$;
    }

    // Fetch fresh data
    this.modulesCache$ = this.http.get<ApiResponse<ModuleConfig[]>>(`${this.apiUrl}/modules`).pipe(
      map(response => {
        if (response.isSuccess && response.data) {
          return response.data.sort((a, b) => a.displayOrder - b.displayOrder);
        }
        return [];
      }),
      tap(modules => {
        this.modulesCacheTime = now;
        this.enabledModulesSubject.next(modules.filter(m => m.isEnabled));
      }),
      catchError(error => {
        console.error('Error loading modules:', error);
        return of([]);
      }),
      shareReplay(1)
    );

    return this.modulesCache$;
  }

  /**
   * Get only enabled modules for navigation
   */
  getEnabledModules(forceRefresh: boolean = false): Observable<ModuleConfig[]> {
    return this.getModules(forceRefresh).pipe(
      map(modules => modules.filter(m => m.isEnabled))
    );
  }

  /**
   * Get all module metadata (static information)
   */
  getModulesMetadata(): Observable<ApiResponse<ModuleMetadata[]>> {
    return this.http.get<ApiResponse<ModuleMetadata[]>>(`${this.apiUrl}/modules/metadata`);
  }

  /**
   * Check if a specific module is enabled
   */
  isModuleEnabled(moduleCode: string): Observable<boolean> {
    return this.http.get<ApiResponse<{ isEnabled: boolean }>>(`${this.apiUrl}/modules/${moduleCode}/enabled`).pipe(
      map(response => response.isSuccess && response.data?.isEnabled === true),
      catchError(() => of(false))
    );
  }

  /**
   * Validate the current license
   */
  validateLicense(): Observable<ApiResponse<LicenseValidationResult>> {
    return this.http.get<ApiResponse<LicenseValidationResult>>(`${this.apiUrl}/validate`);
  }

  /**
   * Activate a license using a license key
   */
  activateLicense(request: ActivateLicenseRequest): Observable<ApiResponse<LicenseInfo>> {
    return this.http.post<ApiResponse<LicenseInfo>>(`${this.apiUrl}/activate`, request).pipe(
      tap(() => {
        // Invalidate cache after activation
        this.modulesCache$ = null;
        this.getModules(true).subscribe();
      })
    );
  }

  /**
   * Get license limits and current usage
   */
  checkLicenseLimits(): Observable<ApiResponse<LicenseLimitCheckResult>> {
    return this.http.get<ApiResponse<LicenseLimitCheckResult>>(`${this.apiUrl}/limits`);
  }

  /**
   * Create a trial license
   */
  createTrialLicense(days: number = 30): Observable<ApiResponse<LicenseInfo>> {
    return this.http.post<ApiResponse<LicenseInfo>>(`${this.apiUrl}/trial?days=${days}`, {}).pipe(
      tap(() => {
        // Invalidate cache after creating trial
        this.modulesCache$ = null;
        this.getModules(true).subscribe();
      })
    );
  }

  /**
   * Get module by code
   */
  getModuleByCode(moduleCode: string): Observable<ModuleConfig | undefined> {
    return this.getModules().pipe(
      map(modules => modules.find(m => m.moduleCode === moduleCode))
    );
  }

  /**
   * Check if user can access a route based on module licensing
   */
  canAccessRoute(route: string): Observable<boolean> {
    return this.getEnabledModules().pipe(
      map(modules => {
        // Core module routes are always accessible
        const coreRoutes = ['/dashboard', '/complaints', '/team-performance', '/reports', '/notifications', '/admin'];
        if (coreRoutes.some(r => route.startsWith(r))) {
          return true;
        }

        // Check if any enabled module has this route
        return modules.some(m => m.route && route.startsWith(m.route));
      })
    );
  }

  /**
   * Clear module cache (useful after license changes)
   */
  clearCache(): void {
    this.modulesCache$ = null;
    this.modulesCacheTime = 0;
  }

  /**
   * Get module icon based on module code
   */
  getModuleIcon(moduleCode: string): string {
    const iconMap: { [key: string]: string } = {
      'CORE': 'fa-ticket-alt',
      'CRM_CUSTOMER': 'fa-users',
      'CRM_PRODUCT': 'fa-boxes-stacked',
      'CRM_SALES': 'fa-chart-line',
      'SERVICE_CONTRACT': 'fa-file-contract',
      'ASSET_MANAGEMENT': 'fa-server',
      'CUSTOMER_PORTAL': 'fa-globe',
      'ADVANCED_REPORTING': 'fa-chart-pie',
      'API_ACCESS': 'fa-code',
      'WHITE_LABELING': 'fa-palette',
      'MULTI_TENANT': 'fa-building-user',
      'SSO': 'fa-key',
      'AUDIT_TRAIL': 'fa-history',
      'DATA_EXPORT': 'fa-file-export'
    };
    return iconMap[moduleCode] || 'fa-cube';
  }

  /**
   * Get module color based on module code
   */
  getModuleColor(moduleCode: string): string {
    const colorMap: { [key: string]: string } = {
      'CORE': '#0057FF',
      'CRM_CUSTOMER': '#10B981',
      'CRM_PRODUCT': '#8B5CF6',
      'CRM_SALES': '#F59E0B',
      'SERVICE_CONTRACT': '#EF4444',
      'ASSET_MANAGEMENT': '#06B6D4',
      'CUSTOMER_PORTAL': '#EC4899',
      'ADVANCED_REPORTING': '#6366F1',
      'API_ACCESS': '#14B8A6',
      'WHITE_LABELING': '#F97316',
      'MULTI_TENANT': '#84CC16',
      'SSO': '#A855F7',
      'AUDIT_TRAIL': '#64748B',
      'DATA_EXPORT': '#0EA5E9'
    };
    return colorMap[moduleCode] || '#6B7280';
  }
}
