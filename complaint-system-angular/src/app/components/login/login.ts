import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

/**
 * LoginComponent - Modern, professional login page
 *
 * Features:
 * - Password visibility toggle
 * - Remember me functionality
 * - Form validation with visual feedback
 * - Loading states with spinner
 * - Accessible form controls (ARIA labels, keyboard navigation)
 * - Responsive design (mobile-first approach)
 * - Animated background with gradient orbs
 * - Professional error handling
 *
 * @author Angular Frontend Excellence Specialist
 * @version 2.0
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent implements OnInit, AfterViewInit {
  /** Reactive form for login credentials */
  loginForm!: FormGroup;

  /** Loading state indicator */
  loading = false;

  /** Error message to display to user */
  errorMessage = '';

  /** URL to redirect after successful login */
  returnUrl = '/dashboard';

  /** Password visibility toggle state */
  showPassword = false;

  /** Remember me checkbox state */
  rememberMe = false;

  /** Local storage key for remember me */
  private readonly REMEMBER_ME_KEY = 'rememberMe';
  private readonly REMEMBERED_EMAIL_KEY = 'rememberedEmail';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    // Initialize form in constructor to prevent undefined errors during authentication check
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required]], // Supports Employee ID, Phone Number, or Email
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Reset loading state to prevent stuck disabled button
    this.loading = false;
    this.errorMessage = '';

    // Clean up expired session data to prevent infinite redirect loops
    this.validateAndCleanupSession();

    // Redirect to dashboard if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    // Get return url from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    // Load remembered credentials if available
    this.loadRememberedCredentials();

    // Trigger change detection to ensure form state is current
    this.cdr.detectChanges();
  }

  /**
   * AfterViewInit lifecycle hook - sets up form value listeners for change detection
   * Critical for E2E tests where forms fill rapidly
   */
  ngAfterViewInit(): void {
    // Listen to form value changes and trigger change detection
    this.loginForm.valueChanges.subscribe(() => {
      this.cdr.detectChanges();
    });

    // Listen to form status changes and trigger change detection
    this.loginForm.statusChanges.subscribe((status) => {
      console.log('[LoginForm] Status changed:', status);
      this.cdr.detectChanges();
    });
  }

  /**
   * Toggles password visibility between text and password input types
   * Provides better UX for users who want to verify their password entry
   */
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  /**
   * Handles forgot password functionality
   * Navigates to the forgot password page
   */
  onForgotPassword(): void {
    this.router.navigate(['/forgot-password']);
  }

  /**
   * Loads remembered credentials from local storage
   * If remember me was previously checked, pre-fills the email field
   */
  private loadRememberedCredentials(): void {
    const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
    const rememberedEmail = localStorage.getItem(this.REMEMBERED_EMAIL_KEY);

    if (rememberMe && rememberedEmail) {
      this.rememberMe = true;
      this.loginForm.patchValue({
        email: rememberedEmail
      });
    }
  }

  /**
   * Saves or clears remembered credentials based on remember me checkbox
   */
  private handleRememberMe(): void {
    if (this.rememberMe) {
      localStorage.setItem(this.REMEMBER_ME_KEY, 'true');
      localStorage.setItem(this.REMEMBERED_EMAIL_KEY, this.loginForm.value.email);
    } else {
      localStorage.removeItem(this.REMEMBER_ME_KEY);
      localStorage.removeItem(this.REMEMBERED_EMAIL_KEY);
    }
  }

  /**
   * Validates and cleans up expired session data
   * Prevents infinite redirect loops by clearing stale tokens
   */
  private validateAndCleanupSession(): void {
    try {
      const token = sessionStorage.getItem('complaint_system_token');
      const expiryTime = sessionStorage.getItem('complaint_system_token_expiry');

      if (token && expiryTime) {
        const expiry = parseInt(expiryTime, 10);
        const now = Date.now();

        // If token is expired, clear all session data
        if (now > expiry) {
          console.log('[Login] Clearing expired session data');
          sessionStorage.removeItem('complaint_system_token');
          sessionStorage.removeItem('complaint_system_refresh_token');
          sessionStorage.removeItem('complaint_system_user');
          sessionStorage.removeItem('complaint_system_token_expiry');
        }
      }
    } catch (error) {
      // If any error occurs during cleanup, clear everything to be safe
      console.warn('[Login] Error during session cleanup, clearing all session data:', error);
      sessionStorage.clear();
    }
  }

  /**
   * Handles form submission
   * Validates form, shows loading state, calls auth service, and handles response
   */
  onSubmit(): void {
    // Prevent submission if form is invalid
    if (this.loginForm.invalid) {
      // Mark all fields as touched to trigger validation messages
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    // Set loading state and clear previous errors
    this.loading = true;
    this.errorMessage = '';
    this.cdr.detectChanges(); // Trigger change detection for loading state

    // Prepare credentials
    const credentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    // Call authentication service
    this.authService.login(credentials).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          // Handle remember me functionality
          this.handleRememberMe();

          // Navigate to return URL
          this.router.navigate([this.returnUrl]);
        } else {
          // Display error message from server
          this.errorMessage = response.message || 'Login failed. Please check your credentials and try again.';
          this.loading = false;
          this.cdr.detectChanges(); // Trigger change detection for error state
        }
      },
      error: (error) => {
        // Log error for debugging (remove in production or use proper logging service)
        console.error('Login error:', error);

        // Display user-friendly error message
        this.errorMessage = error.error?.message || 'An error occurred during login. Please try again.';
        this.loading = false;
        this.cdr.detectChanges(); // Trigger change detection for error state
      }
    });
  }
}
