import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError, timer, switchMap, of } from 'rxjs';
import { Router } from '@angular/router';
import { runtimeConfig } from '../../environments/environment';
import { LoginRequest, LoginResponse, User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private tokenKey = 'complaint_system_token';
  private refreshTokenKey = 'complaint_system_refresh_token';
  private userKey = 'complaint_system_user';
  private tokenExpiryKey = 'complaint_system_token_expiry';
  private sessionTimeout: number = 30 * 60 * 1000; // 30 minutes
  private sessionTimer: any;
  private refreshTimer: any;
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Check for existing session and validate token expiry
    this.validateExistingSession();
    const storedUser = sessionStorage.getItem(this.userKey);
    let parsedUser = null;
    if (storedUser) {
      try {
        parsedUser = JSON.parse(storedUser);
      } catch {
        console.warn('Invalid user data in sessionStorage, clearing...');
        this.clearSessionData();
      }
    }
    this.currentUserSubject = new BehaviorSubject<User | null>(parsedUser);
    this.currentUser = this.currentUserSubject.asObservable();

    // Set up session timeout monitoring
    this.setupSessionTimeout();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get token(): string | null {
    // Check if token is expired
    if (this.isTokenExpired()) {
      this.clearSession();
      return null;
    }
    return sessionStorage.getItem(this.tokenKey);
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${runtimeConfig.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.handleAuthenticationSuccess(response.data);
          }
        })
      );
  }

  /**
   * Handle successful authentication by storing tokens and setting up refresh
   */
  private handleAuthenticationSuccess(data: { token: string; refreshToken: string; expiresAt: string; user: User }): void {
    // Store tokens in sessionStorage
    sessionStorage.setItem(this.tokenKey, data.token);
    sessionStorage.setItem(this.refreshTokenKey, data.refreshToken);
    sessionStorage.setItem(this.userKey, JSON.stringify(data.user));

    // Calculate and store token expiry time
    const expiryTime = new Date(data.expiresAt).getTime();
    sessionStorage.setItem(this.tokenExpiryKey, expiryTime.toString());

    this.currentUserSubject.next(data.user);

    // Setup proactive token refresh (refresh 5 minutes before expiry)
    this.setupTokenRefresh(expiryTime);

    // Keep the session timeout as a fallback
    this.setupSessionTimeout();
  }

  logout(): void {
    // Call backend logout to revoke refresh tokens
    const token = this.token;
    if (token) {
      this.http.post(`${runtimeConfig.apiUrl}/auth/logout`, {}).subscribe({
        next: () => {
          console.log('Logout successful on server');
        },
        error: (err) => {
          console.warn('Logout failed on server:', err);
        }
      });
    }

    this.clearSession();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem(this.tokenKey);
    if (!token) return false;

    // Check if token is expired with buffer time to prevent race conditions
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      const bufferTime = 5000; // 5 seconds buffer to handle timing edge cases
      const isValid = (Date.now() + bufferTime) < exp;

      if (!isValid) {
        // Token is expired or about to expire, clear session
        this.clearSession();
      }

      return isValid;
    } catch {
      return false;
    }
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUserValue;
    return user?.permissions.includes(permission) ?? false;
  }

  hasAnyPermission(permissions: string[]): boolean {
    const user = this.currentUserValue;
    return permissions.some(p => user?.permissions.includes(p)) ?? false;
  }

  private clearSession(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.refreshTokenKey);
    sessionStorage.removeItem(this.userKey);
    sessionStorage.removeItem(this.tokenExpiryKey);
    this.currentUserSubject.next(null);
    this.clearSessionTimer();
    this.clearRefreshTimer();
    this.isRefreshing = false;
  }

  private clearSessionData(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.refreshTokenKey);
    sessionStorage.removeItem(this.userKey);
    sessionStorage.removeItem(this.tokenExpiryKey);
  }

  private isTokenExpired(): boolean {
    const expiryTime = sessionStorage.getItem(this.tokenExpiryKey);
    if (!expiryTime) return true;
    return Date.now() > parseInt(expiryTime);
  }

  private validateExistingSession(): void {
    if (this.isTokenExpired()) {
      this.clearSessionData();
    }
  }

  private setupSessionTimeout(): void {
    this.clearSessionTimer();
    this.sessionTimer = setTimeout(() => {
      console.warn('Session expired due to inactivity');
      this.clearSession();
      this.router.navigate(['/login']);
    }, this.sessionTimeout);
  }

  private clearSessionTimer(): void {
    if (this.sessionTimer) {
      clearTimeout(this.sessionTimer);
      this.sessionTimer = null;
    }
  }

  public extendSession(): void {
    if (this.token) {
      const expiryTime = Date.now() + this.sessionTimeout;
      sessionStorage.setItem(this.tokenExpiryKey, expiryTime.toString());
      this.setupSessionTimeout();
    }
  }

  /**
   * Setup proactive token refresh timer
   * Refreshes the token 5 minutes before it expires
   */
  private setupTokenRefresh(expiryTime: number): void {
    this.clearRefreshTimer();

    // Calculate time until refresh (5 minutes before expiry)
    const refreshBuffer = 5 * 60 * 1000; // 5 minutes
    const timeUntilRefresh = expiryTime - Date.now() - refreshBuffer;

    if (timeUntilRefresh > 0) {
      this.refreshTimer = setTimeout(() => {
        this.refreshAccessToken().subscribe({
          next: () => {
            console.log('Token refreshed successfully');
          },
          error: (err) => {
            console.error('Token refresh failed:', err);
            // If refresh fails, log the user out
            this.logout();
          }
        });
      }, timeUntilRefresh);
    } else {
      // Token is about to expire or already expired, refresh immediately
      this.refreshAccessToken().subscribe({
        error: (err) => {
          console.error('Token refresh failed:', err);
          this.logout();
        }
      });
    }
  }

  /**
   * Clear the token refresh timer
   */
  private clearRefreshTimer(): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }

  /**
   * Refresh the access token using the refresh token
   * Implements token rotation for security
   */
  public refreshAccessToken(): Observable<LoginResponse> {
    const refreshToken = sessionStorage.getItem(this.refreshTokenKey);

    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    // Prevent multiple simultaneous refresh requests
    if (this.isRefreshing) {
      // Wait for the current refresh to complete
      return this.refreshTokenSubject.pipe(
        switchMap(token => {
          if (token) {
            // Refresh completed successfully, return mock response
            return of({
              isSuccess: true,
              message: 'Token refreshed',
              data: {
                token: token,
                refreshToken: sessionStorage.getItem(this.refreshTokenKey) || '',
                expiresAt: sessionStorage.getItem(this.tokenExpiryKey) || '',
                user: this.currentUserValue!
              }
            });
          }
          return throwError(() => new Error('Token refresh failed'));
        })
      );
    }

    this.isRefreshing = true;
    this.refreshTokenSubject.next(null);

    return this.http.post<LoginResponse>(`${runtimeConfig.apiUrl}/auth/refresh`, {
      refreshToken: refreshToken
    }).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          // Store new tokens
          this.handleAuthenticationSuccess(response.data);
          this.refreshTokenSubject.next(response.data.token);
        }
        this.isRefreshing = false;
      }),
      catchError(err => {
        this.isRefreshing = false;
        this.refreshTokenSubject.next(null);
        // Clear session and redirect to login
        this.clearSession();
        this.router.navigate(['/login']);
        return throwError(() => err);
      })
    );
  }

  /**
   * Get the refresh token
   */
  public get refreshToken(): string | null {
    return sessionStorage.getItem(this.refreshTokenKey);
  }

  /**
   * Check if refresh is in progress
   */
  public get isRefreshingToken(): boolean {
    return this.isRefreshing;
  }
}