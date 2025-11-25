import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  loading = false;
  submitted = false;
  message = '';
  isError = false;

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get f() {
    return this.forgotPasswordForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.message = '';
    this.isError = false;

    // Validate form
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.loading = true;

    const requestBody = {
      email: this.forgotPasswordForm.value.email
    };

    this.http
      .post<any>(`${environment.apiUrl}/password-reset/request`, requestBody)
      .subscribe({
        next: (response) => {
          this.loading = false;
          if (response.success) {
            this.isError = false;
            this.message = response.message || 'If an account with that email exists, a password reset link has been sent.';
            this.forgotPasswordForm.reset();
            this.submitted = false;
          } else {
            this.isError = true;
            this.message = response.message || 'An error occurred. Please try again.';
          }
        },
        error: (error) => {
          this.loading = false;
          this.isError = true;

          if (error.status === 429) {
            this.message = error.error?.message || 'Too many requests. Please try again later.';
          } else {
            this.message = 'An error occurred. Please try again.';
          }
        }
      });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
