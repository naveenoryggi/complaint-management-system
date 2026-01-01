import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PortalLoginRequest,
  PortalLoginResponse,
  PortalUser
} from '../models/portal.model';

@Injectable({
  providedIn: 'root'
})
export class PortalAuthService {
  private readonly API_URL = `${environment.apiUrl}/portal`;
  private readonly PORTAL_TOKEN_KEY = 'portal_token';
  private readonly PORTAL_REFRESH_TOKEN_KEY = 'portal_refresh_token';
  private readonly PORTAL_USER_KEY = 'portal_user';

  private currentUserSubject = new BehaviorSubject<PortalUser | null>(this.getStoredUser());
  currentUser$ = this.currentUserSubject.asObservable();

  // Signals for reactive state
  private _isAuthenticated = signal<boolean>(this.hasValidToken());
  private _user = signal<PortalUser | null>(this.getStoredUser());

  isAuthenticated = computed(() => this._isAuthenticated());
  user = computed(() => this._user());

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  login(credentials: PortalLoginRequest): Observable<PortalLoginResponse> {
    return this.http.post<PortalLoginResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.setSession(response);
          }
        }),
        catchError(error => {
          console.error('Portal login error:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.API_URL}/auth/logout`, { refreshToken }).subscribe({
        error: () => {} // Ignore logout errors
      });
    }
    this.clearSession();
    this.router.navigate(['/portal/login']);
  }

  refreshToken(): Observable<PortalLoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<PortalLoginResponse>(`${this.API_URL}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.setSession(response);
          }
        }),
        catchError(error => {
          this.clearSession();
          return throwError(() => error);
        })
      );
  }

  getToken(): string | null {
    return localStorage.getItem(this.PORTAL_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.PORTAL_REFRESH_TOKEN_KEY);
  }

  getUser(): PortalUser | null {
    return this._user();
  }

  hasPermission(permission: string): boolean {
    const user = this._user();
    return user?.permissions?.includes(permission) ?? false;
  }

  private setSession(response: PortalLoginResponse): void {
    localStorage.setItem(this.PORTAL_TOKEN_KEY, response.token);
    localStorage.setItem(this.PORTAL_REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.PORTAL_USER_KEY, JSON.stringify(response.user));

    this._isAuthenticated.set(true);
    this._user.set(response.user);
    this.currentUserSubject.next(response.user);
  }

  private clearSession(): void {
    localStorage.removeItem(this.PORTAL_TOKEN_KEY);
    localStorage.removeItem(this.PORTAL_REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.PORTAL_USER_KEY);

    this._isAuthenticated.set(false);
    this._user.set(null);
    this.currentUserSubject.next(null);
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      return Date.now() < expiry;
    } catch {
      return false;
    }
  }

  private getStoredUser(): PortalUser | null {
    const userStr = localStorage.getItem(this.PORTAL_USER_KEY);
    if (!userStr) return null;

    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }
}
