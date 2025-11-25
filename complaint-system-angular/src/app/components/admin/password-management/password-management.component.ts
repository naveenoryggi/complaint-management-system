import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { PasswordService, GeneratePasswordRequest } from '../../../services/password.service';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user.model';
import { PasswordStrengthMeterComponent } from '../../shared/password-strength-meter/password-strength-meter.component';

interface UserPasswordStatus {
  userId: string;
  daysUntilExpiration: number | null;
  isExpired: boolean;
  isLocked: boolean;
}

@Component({
  selector: 'app-password-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PasswordStrengthMeterComponent],
  templateUrl: './password-management.component.html',
  styleUrls: ['./password-management.component.scss']
})
export class PasswordManagementComponent implements OnInit, OnDestroy {
  // User search
  searchForm!: FormGroup;
  searchResults: User[] = [];
  isSearching = false;
  selectedUser: User | null = null;
  userPasswordStatus: UserPasswordStatus | null = null;

  // Password operations
  activeTab: 'set' | 'reset' | 'generate' | 'unlock' = 'set';
  setPasswordForm!: FormGroup;
  resetPasswordForm!: FormGroup;
  generatePasswordForm!: FormGroup;
  showPassword = false;
  generatedPassword: string | null = null;
  generatedPasswordStrength: any = null;

  // UI state
  isSubmitting = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private passwordService: PasswordService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.setupUserSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForms(): void {
    this.searchForm = this.fb.group({
      searchTerm: ['', [Validators.required, Validators.minLength(2)]]
    });

    this.setPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      mustChangeOnNextLogin: [true],
      sendEmail: [false]
    }, { validators: this.passwordMatchValidator });

    this.resetPasswordForm = this.fb.group({
      sendEmail: [true]
    });

    this.generatePasswordForm = this.fb.group({
      length: [16, [Validators.required, Validators.min(8), Validators.max(32)]],
      includeUppercase: [true],
      includeLowercase: [true],
      includeDigits: [true],
      includeSpecialChars: [true]
    });
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

  private setupUserSearch(): void {
    this.searchSubject$
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        switchMap(searchTerm => {
          if (!searchTerm || searchTerm.trim().length < 2) {
            this.isSearching = false;
            return of([]);
          }

          this.isSearching = true;
          return this.userService.searchUsers(searchTerm.trim());
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (response) => {
          this.searchResults = Array.isArray(response) ? response : (response.data || []);
          this.isSearching = false;
        },
        error: (error) => {
          console.error('Error searching users:', error);
          this.searchResults = [];
          this.isSearching = false;
        }
      });
  }

  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchSubject$.next(target.value);
  }

  selectUser(user: User): void {
    this.selectedUser = user;
    this.searchResults = [];
    this.searchForm.patchValue({ searchTerm: `${user.fullName} (${user.employeeCode})` });
    this.loadUserPasswordStatus();
    this.clearMessages();
  }

  clearUserSelection(): void {
    this.selectedUser = null;
    this.userPasswordStatus = null;
    this.searchForm.reset();
    this.searchResults = [];
    this.clearMessages();
  }

  private loadUserPasswordStatus(): void {
    if (!this.selectedUser) return;

    this.passwordService.getUserPasswordStatus(this.selectedUser.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status) => {
          this.userPasswordStatus = status;
        },
        error: (error) => {
          console.error('Error loading password status:', error);
        }
      });
  }

  switchTab(tab: 'set' | 'reset' | 'generate' | 'unlock'): void {
    this.activeTab = tab;
    this.clearMessages();
    this.generatedPassword = null;
    this.generatedPasswordStrength = null;
  }

  // Set Password
  getSetPasswordValue(): string {
    return this.setPasswordForm.get('password')?.value || '';
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSetPassword(): void {
    if (this.setPasswordForm.invalid || !this.selectedUser || this.isSubmitting) {
      this.markFormGroupTouched(this.setPasswordForm);
      return;
    }

    this.clearMessages();
    this.isSubmitting = true;

    const { password, mustChangeOnNextLogin, sendEmail } = this.setPasswordForm.value;

    this.passwordService.setUserPassword(
      this.selectedUser.id,
      password,
      mustChangeOnNextLogin,
      sendEmail
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.successMessage = response.message || 'Password set successfully';
          this.setPasswordForm.reset({ mustChangeOnNextLogin: true, sendEmail: false });
          this.loadUserPasswordStatus();
          this.isSubmitting = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.error || error.error?.message ||
                              'Failed to set password. Please try again.';
          this.isSubmitting = false;
        }
      });
  }

  // Reset Password
  onResetPassword(): void {
    if (!this.selectedUser || this.isSubmitting) return;

    this.clearMessages();
    this.isSubmitting = true;

    const { sendEmail } = this.resetPasswordForm.value;

    this.passwordService.resetUserPassword(this.selectedUser.id, sendEmail)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.newPassword && !sendEmail) {
            this.successMessage = `Password reset successfully. New password: ${response.newPassword}`;
          } else {
            this.successMessage = response.message || 'Password reset successfully';
          }
          this.loadUserPasswordStatus();
          this.isSubmitting = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.error || error.error?.message ||
                              'Failed to reset password. Please try again.';
          this.isSubmitting = false;
        }
      });
  }

  // Generate Password
  onGeneratePassword(): void {
    if (this.generatePasswordForm.invalid) {
      this.markFormGroupTouched(this.generatePasswordForm);
      return;
    }

    this.clearMessages();

    const request: GeneratePasswordRequest = this.generatePasswordForm.value;

    this.passwordService.generatePassword(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.generatedPassword = response.password;
          this.generatedPasswordStrength = response.strength;
          this.successMessage = 'Password generated successfully';
        },
        error: (error) => {
          this.errorMessage = error.error?.error || error.error?.message ||
                              'Failed to generate password. Please try again.';
        }
      });
  }

  copyToClipboard(): void {
    if (this.generatedPassword) {
      navigator.clipboard.writeText(this.generatedPassword).then(() => {
        this.successMessage = 'Password copied to clipboard';
      }).catch(() => {
        this.errorMessage = 'Failed to copy password';
      });
    }
  }

  // Unlock Account
  onUnlockAccount(): void {
    if (!this.selectedUser || this.isSubmitting) return;

    this.clearMessages();
    this.isSubmitting = true;

    this.passwordService.unlockAccount(this.selectedUser.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.successMessage = response.message || 'Account unlocked successfully';
          this.loadUserPasswordStatus();
          this.isSubmitting = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.error || error.error?.message ||
                              'Failed to unlock account. Please try again.';
          this.isSubmitting = false;
        }
      });
  }

  // Helper methods
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  private clearMessages(): void {
    this.successMessage = null;
    this.errorMessage = null;
  }

  isFieldInvalid(formName: 'set' | 'generate', fieldName: string): boolean {
    const form = formName === 'set' ? this.setPasswordForm : this.generatePasswordForm;
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(formName: 'set' | 'generate', fieldName: string): string | null {
    const form = formName === 'set' ? this.setPasswordForm : this.generatePasswordForm;
    const field = form.get(fieldName);

    if (!field || !field.errors || !(field.dirty || field.touched)) {
      return null;
    }

    if (field.errors['required']) {
      return 'This field is required';
    }

    if (field.errors['minlength']) {
      return `Minimum length is ${field.errors['minlength'].requiredLength} characters`;
    }

    if (field.errors['min']) {
      return `Minimum value is ${field.errors['min'].min}`;
    }

    if (field.errors['max']) {
      return `Maximum value is ${field.errors['max'].max}`;
    }

    return null;
  }

  getFormError(): string | null {
    if (this.setPasswordForm.errors?.['passwordMismatch'] &&
        this.setPasswordForm.get('confirmPassword')?.touched) {
      return 'Passwords do not match';
    }
    return null;
  }

  getPasswordStatusBadgeClass(): string {
    if (!this.userPasswordStatus) return '';

    if (this.userPasswordStatus.isLocked) return 'badge-locked';
    if (this.userPasswordStatus.isExpired) return 'badge-expired';
    if (this.userPasswordStatus.daysUntilExpiration !== null &&
        this.userPasswordStatus.daysUntilExpiration <= 7) return 'badge-warning';

    return 'badge-success';
  }

  getPasswordStatusText(): string {
    if (!this.userPasswordStatus) return '';

    if (this.userPasswordStatus.isLocked) return 'Locked';
    if (this.userPasswordStatus.isExpired) return 'Expired';
    if (this.userPasswordStatus.daysUntilExpiration === null) return 'Never Expires';
    if (this.userPasswordStatus.daysUntilExpiration <= 7)
      return `Expires in ${this.userPasswordStatus.daysUntilExpiration} days`;

    return 'Active';
  }
}
