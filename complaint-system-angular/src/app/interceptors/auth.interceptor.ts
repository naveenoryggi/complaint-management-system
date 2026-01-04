import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, throwError, switchMap, filter, take } from 'rxjs';
import { Router } from '@angular/router';

/**
 * HTTP Interceptor for authentication with automatic token refresh on 401 errors
 *
 * Features:
 * - Automatically adds Authorization header to all requests
 * - Intercepts 401 errors and attempts token refresh
 * - Retries the original request with new token after successful refresh
 * - Queues subsequent requests while refreshing to avoid multiple refresh calls
 * - Redirects to login if refresh fails
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.token;

  // Skip interceptor for auth endpoints and portal endpoints
  // Note: Do NOT skip /api/assets/ - only skip static assets in /assets/ folder
  if (
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/refresh') ||
    req.url.includes('/portal/')
  ) {
    return next(req);
  }

  // Clone the request and add authorization header if token exists
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && !req.url.includes('/auth/')) {
        // Check if we have a refresh token
        const refreshToken = authService.refreshToken;

        if (!refreshToken) {
          // No refresh token available, redirect to login
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => error);
        }

        // If already refreshing, wait for the current refresh to complete
        if (authService.isRefreshingToken) {
          // Wait for the refresh to complete and retry the request
          return authService['refreshTokenSubject'].pipe(
            filter(token => token !== null),
            take(1),
            switchMap(newToken => {
              // Clone the request with the new token
              const clonedReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(clonedReq);
            })
          );
        }

        // Not currently refreshing, attempt to refresh the token
        return authService.refreshAccessToken().pipe(
          switchMap(response => {
            if (response.isSuccess && response.data) {
              // Retry the original request with the new token
              const clonedReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.data.token}`
                }
              });
              return next(clonedReq);
            }
            // Refresh failed, redirect to login
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => error);
          }),
          catchError(refreshError => {
            // Token refresh failed, logout and redirect to login
            console.error('Token refresh failed in interceptor:', refreshError);
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => error);
          })
        );
      }

      // For other errors, just pass them through
      return throwError(() => error);
    })
  );
};
