import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface PasswordStrengthResult {
  score: number;
  category: string;
  colorCode: string;
}

export interface PasswordValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface PasswordStatusResult {
  daysUntilExpiration: number | null;
  isExpired: boolean;
  isLocked: boolean;
}

export interface GeneratePasswordRequest {
  length?: number;
  includeUppercase?: boolean;
  includeLowercase?: boolean;
  includeDigits?: boolean;
  includeSpecialChars?: boolean;
}

export interface GeneratePasswordResult {
  password: string;
  strength: PasswordStrengthResult;
}

@Injectable({
  providedIn: 'root'
})
export class PasswordService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/password`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Check password strength (anonymous - no auth required)
   */
  checkPasswordStrength(password: string): Observable<PasswordStrengthResult> {
    return this.http.post<PasswordStrengthResult>(`${this.apiUrl}/strength`, { password });
  }

  /**
   * Validate password against company policy
   */
  validatePassword(password: string, companyId: string): Observable<PasswordValidationResult> {
    return this.http.post<PasswordValidationResult>(`${this.apiUrl}/validate`, {
      password,
      companyId
    });
  }

  /**
   * Get current user's password status
   */
  getPasswordStatus(): Observable<PasswordStatusResult> {
    return this.http.get<PasswordStatusResult>(`${this.apiUrl}/status`);
  }

  /**
   * Change current user's password
   */
  changePassword(currentPassword: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/change`, {
      currentPassword,
      newPassword
    });
  }

  // ========== Admin Operations ==========

  /**
   * Generate secure random password (admin only)
   */
  generatePassword(request?: GeneratePasswordRequest): Observable<GeneratePasswordResult> {
    const body = request || {
      length: 16,
      includeUppercase: true,
      includeLowercase: true,
      includeDigits: true,
      includeSpecialChars: true
    };
    return this.http.post<GeneratePasswordResult>(`${this.apiUrl}/generate`, body);
  }

  /**
   * Set user password (admin only)
   */
  setUserPassword(
    userId: string,
    password: string,
    mustChangeOnNextLogin: boolean = true,
    sendEmail: boolean = false
  ): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/set`, {
      userId,
      password,
      mustChangeOnNextLogin,
      sendEmail
    });
  }

  /**
   * Reset user password with auto-generated password (admin only)
   */
  resetUserPassword(
    userId: string,
    sendEmail: boolean = true
  ): Observable<{ message: string; newPassword?: string }> {
    return this.http.post<{ message: string; newPassword?: string }>(`${this.apiUrl}/reset`, {
      userId,
      sendEmail
    });
  }

  /**
   * Unlock locked user account (admin only)
   */
  unlockAccount(userId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/unlock`, { userId });
  }

  /**
   * Get user password status by ID (admin only)
   */
  getUserPasswordStatus(userId: string): Observable<PasswordStatusResult & { userId: string }> {
    return this.http.get<PasswordStatusResult & { userId: string }>(
      `${this.apiUrl}/status/${userId}`
    );
  }

  /**
   * Get password strength category label
   */
  getStrengthLabel(score: number): string {
    if (score <= 20) return 'Very Weak';
    if (score <= 35) return 'Weak';
    if (score <= 50) return 'Fair';
    if (score <= 65) return 'Good';
    if (score <= 80) return 'Strong';
    return 'Very Strong';
  }

  /**
   * Get password strength color
   */
  getStrengthColor(score: number): string {
    if (score <= 20) return '#dc3545'; // Red
    if (score <= 35) return '#fd7e14'; // Orange
    if (score <= 50) return '#ffc107'; // Yellow
    if (score <= 65) return '#20c997'; // Teal
    if (score <= 80) return '#28a745'; // Green
    return '#007bff'; // Blue
  }
}
