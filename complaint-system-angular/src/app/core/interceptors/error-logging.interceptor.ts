import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';
import { throwError, timer } from 'rxjs';
import { catchError, tap, retry, timeout } from 'rxjs/operators';
import { ErrorLoggingService } from '../services/error-logging.service';

export const errorLoggingInterceptor: HttpInterceptorFn = (req, next) => {
  const errorLoggingService = inject(ErrorLoggingService);
  const startTime = Date.now();
  const requestId = generateRequestId();

  // Clone request to add request ID for tracking
  const clonedReq = req.clone({
    headers: req.headers.set('X-Request-ID', requestId),
    setParams: { ...req.params.keys().reduce((acc, key) => ({
      ...acc,
      [key]: req.params.get(key)
    }), {}), _requestId: requestId }
  });

  return next(clonedReq).pipe(
    // Add timeout for requests
    timeout(30000), // 30 seconds timeout

    // Retry on transient errors with exponential backoff
    retry({
      count: 2,
      delay: (error, retryCount) => {
        // Don't retry on client errors (4xx) or specific error types
        if (error instanceof HttpErrorResponse) {
          if (error.status >= 400 && error.status < 500) {
            throw error;
          }
          if (error.status === 429) { // Rate limiting
            return timer(1000 * retryCount); // Exponential backoff
          }
        }
        return timer(1000 * retryCount); // 1s, 2s delay
      },
      resetOnSuccess: true
    }),

    tap({
      next: (event) => {
        // Log successful responses for debugging
        if (event instanceof HttpResponse) {
          const duration = Date.now() - startTime;

          // Log slow requests
          if (duration > 5000) {
            console.warn(
              `🐌 Slow request detected: ${req.method} ${req.url} (${duration}ms)`,
              { requestId, status: event.status }
            );
          } else {
            console.log(
              `✅ HTTP ${event.status} - ${req.method} ${req.url} (${duration}ms)`,
              { requestId }
            );
          }
        }
      },
      error: (error) => {
        // Add retry tracking to error logging
        if (error instanceof HttpErrorResponse) {
          errorLoggingService.addRetryAttempt(requestId);
        }
      }
    }),

    catchError((error: HttpErrorResponse) => {
      const duration = Date.now() - startTime;

      // Log the error with enhanced context
      errorLoggingService.logHttpError(error, req.method, req.url);

      // Also log to console with additional context
      console.error(
        `❌ HTTP ${error.status} - ${req.method} ${req.url} (${duration}ms)`,
        {
          requestId,
          error: error.error,
          message: error.message,
          statusText: error.statusText,
          url: error.url,
          duration: `${duration}ms`
        }
      );

      // Handle specific error types with user-friendly responses
      if (error.status === 401) {
        return throwError(() => ({
          ...error,
          userMessage: 'Your session has expired. Please log in again.',
          requiresReauth: true
        }));
      }

      if (error.status === 403) {
        return throwError(() => ({
          ...error,
          userMessage: 'You do not have permission to perform this action.',
          requiresAuth: true
        }));
      }

      if (error.status === 429) {
        const retryAfter = error.headers?.get('Retry-After');
        return throwError(() => ({
          ...error,
          userMessage: `Too many requests. Please try again in ${retryAfter || '60'} seconds.`,
          retryAfter: retryAfter ? parseInt(retryAfter) * 1000 : 60000
        }));
      }

      if (error.status >= 500) {
        return throwError(() => ({
          ...error,
          userMessage: 'Server error occurred. Please try again later.',
          isServerError: true
        }));
      }

      if (error.status === 0) {
        return throwError(() => ({
          ...error,
          userMessage: 'Network error. Please check your internet connection.',
          isNetworkError: true
        }));
      }

      // Return enhanced error
      return throwError(() => ({
        ...error,
        requestId,
        duration,
        userMessage: error.error?.message || error.message || 'An error occurred'
      }));
    })
  );
};

/**
 * Generate unique request ID for tracking
 */
function generateRequestId(): string {
  return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}
