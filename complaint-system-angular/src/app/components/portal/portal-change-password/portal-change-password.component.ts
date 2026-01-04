import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { PortalAuthService } from '../../../services/portal-auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-portal-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './portal-change-password.component.html',
  styleUrls: ['./portal-change-password.component.scss']
})
export class PortalChangePasswordComponent {
  private readonly API_URL = `${environment.apiUrl}/portal`;

  passwordForm: FormGroup;
  isLoading = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  showCurrentPassword = signal(false);
  showNewPassword = signal(false);
  showConfirmPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: PortalAuthService,
    private http: HttpClient,
    private router: Router
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        this.passwordStrengthValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(value);

    const valid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecial;
    return valid ? null : { passwordStrength: true };
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm'): void {
    switch (field) {
      case 'current':
        this.showCurrentPassword.update(v => !v);
        break;
      case 'new':
        this.showNewPassword.update(v => !v);
        break;
      case 'confirm':
        this.showConfirmPassword.update(v => !v);
        break;
    }
  }

  onSubmit(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const { currentPassword, newPassword } = this.passwordForm.value;

    this.http.post<any>(`${this.API_URL}/auth/change-password`, {
      currentPassword,
      newPassword
    }, {
      headers: this.getHeaders()
    }).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.isSuccess) {
          this.successMessage.set('Password changed successfully! Redirecting to dashboard...');
          this.passwordForm.reset();
          setTimeout(() => {
            this.router.navigate(['/portal/dashboard']);
          }, 2000);
        } else {
          this.errorMessage.set(response.message || 'Failed to change password.');
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        if (error.status === 400) {
          this.errorMessage.set('Current password is incorrect.');
        } else {
          this.errorMessage.set('Failed to change password. Please try again.');
        }
        console.error('Error changing password:', error);
      }
    });
  }

  get currentPassword() { return this.passwordForm.get('currentPassword'); }
  get newPassword() { return this.passwordForm.get('newPassword'); }
  get confirmPassword() { return this.passwordForm.get('confirmPassword'); }

  hasMinLength(value: string | null | undefined): boolean {
    return !!value && value.length >= 8;
  }

  hasUpperCase(value: string | null | undefined): boolean {
    return !!value && /[A-Z]/.test(value);
  }

  hasLowerCase(value: string | null | undefined): boolean {
    return !!value && /[a-z]/.test(value);
  }

  hasNumber(value: string | null | undefined): boolean {
    return !!value && /[0-9]/.test(value);
  }

  hasSpecialCharacter(value: string | null | undefined): boolean {
    if (!value) return false;
    return /[!@#$%^&*(),.?":{}|<>\-_=+\[\];'\/\\]/.test(value);
  }
}
