import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PasswordStrengthMeterComponent } from '../../shared/password-strength-meter/password-strength-meter.component';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, PasswordStrengthMeterComponent],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  loading = false;
  submitted = false;
  message = '';
  isError = false;
  token: string | null = null;
  tokenValid = false;
  validatingToken = true;
  showPassword = false;
  showConfirmPassword = false;
  userEmail = '';

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.resetPasswordForm = this.formBuilder.group(
      {
        newPassword: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]]
      },
      {
        validators: this.passwordMatchValidator
      }
    );
  }

  ngOnInit(): void {
    // Get token from query parameters
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.validatingToken = false;
      this.isError = true;
      this.message = 'Invalid password reset link. Please request a new one.';
      return;
    }

    // Validate token
    this.validateToken();
  }

  passwordMatchValidator(group: FormGroup) {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (confirmPassword && newPassword !== confirmPassword) {
      group.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  get f() {
    return this.resetPasswordForm.controls;
  }

  validateToken(): void {
    if (!this.token) {
      return;
    }

    this.http
      .post<any>(`${environment.apiUrl}/password-reset/validate`, {
        token: this.token
      })
      .subscribe({
        next: (response) => {
          this.validatingToken = false;
          if (response.isValid) {
            this.tokenValid = true;
            this.userEmail = response.email || '';
          } else {
            this.isError = true;
            this.message = response.errorMessage || 'Invalid or expired token. Please request a new password reset.';
          }
        },
        error: () => {
          this.validatingToken = false;
          this.isError = true;
          this.message = 'An error occurred while validating your reset link. Please try again.';
        }
      });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    this.submitted = true;
    this.message = '';
    this.isError = false;

    // Validate form
    if (this.resetPasswordForm.invalid) {
      return;
    }

    this.loading = true;

    const requestBody = {
      token: this.token,
      newPassword: this.resetPasswordForm.value.newPassword
    };

    this.http
      .post<any>(`${environment.apiUrl}/password-reset/reset`, requestBody)
      .subscribe({
        next: (response) => {
          this.loading = false;
          if (response.success) {
            this.isError = false;
            this.message = response.message || 'Your password has been reset successfully.';

            // Redirect to login after 3 seconds
            setTimeout(() => {
              this.router.navigate(['/login'], {
                queryParams: { message: 'Password reset successful. Please login with your new password.' }
              });
            }, 3000);
          } else {
            this.isError = true;
            if (response.validationErrors && response.validationErrors.length > 0) {
              this.message = response.validationErrors.join(', ');
            } else {
              this.message = response.message || 'An error occurred. Please try again.';
            }
          }
        },
        error: (error) => {
          this.loading = false;
          this.isError = true;

          if (error.error?.validationErrors && error.error.validationErrors.length > 0) {
            this.message = error.error.validationErrors.join(', ');
          } else if (error.error?.message) {
            this.message = error.error.message;
          } else {
            this.message = 'An error occurred. Please try again.';
          }
        }
      });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  requestNewResetLink(): void {
    this.router.navigate(['/forgot-password']);
  }
}
