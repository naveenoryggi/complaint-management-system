import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LicenseService } from '../../../services/license.service';
import { AppHeaderComponent } from '../../shared/app-header/app-header.component';
import {
  LicenseInfo,
  ModuleConfig,
  LicenseValidationResult,
  LicenseLimitCheckResult
} from '../../../models/license.model';

@Component({
  selector: 'app-license-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AppHeaderComponent],
  templateUrl: './license-management.component.html',
  styleUrls: ['./license-management.component.scss']
})
export class LicenseManagementComponent implements OnInit, OnDestroy {
  licenseInfo: LicenseInfo | null = null;
  modules: ModuleConfig[] = [];
  validationResult: LicenseValidationResult | null = null;
  limitsResult: LicenseLimitCheckResult | null = null;

  isLoading = true;
  isActivating = false;
  showActivateModal = false;
  activationError: string | null = null;
  activationSuccess: string | null = null;

  activateForm: FormGroup;

  private destroy$ = new Subject<void>();

  constructor(
    private licenseService: LicenseService,
    private fb: FormBuilder
  ) {
    this.activateForm = this.fb.group({
      licenseKey: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  ngOnInit(): void {
    this.loadLicenseData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLicenseData(): void {
    this.isLoading = true;

    // Load license info
    this.licenseService.getLicenseInfo().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.licenseInfo = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading license info:', error);
      }
    });

    // Load modules
    this.licenseService.getModules(true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (modules) => {
        this.modules = modules;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading modules:', error);
        this.isLoading = false;
      }
    });

    // Load validation
    this.licenseService.validateLicense().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.validationResult = response.data;
        }
      }
    });

    // Load limits
    this.licenseService.checkLicenseLimits().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.limitsResult = response.data;
        }
      }
    });
  }

  openActivateModal(): void {
    this.showActivateModal = true;
    this.activationError = null;
    this.activationSuccess = null;
    this.activateForm.reset();
  }

  closeActivateModal(): void {
    this.showActivateModal = false;
  }

  activateLicense(): void {
    if (this.activateForm.invalid) return;

    this.isActivating = true;
    this.activationError = null;
    this.activationSuccess = null;

    this.licenseService.activateLicense({ licenseKey: this.activateForm.value.licenseKey }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        this.isActivating = false;
        if (response.isSuccess) {
          this.activationSuccess = 'License activated successfully!';
          this.licenseInfo = response.data;
          setTimeout(() => {
            this.closeActivateModal();
            this.loadLicenseData();
          }, 2000);
        } else {
          this.activationError = response.message || 'Failed to activate license';
        }
      },
      error: (error) => {
        this.isActivating = false;
        this.activationError = error.error?.message || 'Failed to activate license. Please check the key and try again.';
      }
    });
  }

  createTrialLicense(): void {
    this.isLoading = true;
    this.licenseService.createTrialLicense(30).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.licenseInfo = response.data;
          this.loadLicenseData();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error creating trial license:', error);
        this.isLoading = false;
      }
    });
  }

  getModuleIcon(module: ModuleConfig): string {
    return module.icon || this.licenseService.getModuleIcon(module.moduleCode);
  }

  getModuleColor(module: ModuleConfig): string {
    return module.color || this.licenseService.getModuleColor(module.moduleCode);
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'status-active';
      case 'expired':
        return 'status-expired';
      case 'graceperiod':
        return 'status-warning';
      case 'pending':
        return 'status-pending';
      default:
        return 'status-default';
    }
  }

  getLicenseTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'Trial': 'Trial License',
      'Basic': 'Basic License',
      'Standard': 'Standard License',
      'Professional': 'Professional License',
      'Enterprise': 'Enterprise License',
      'Custom': 'Custom License'
    };
    return labels[type] || type;
  }

  getEnabledModulesCount(): number {
    return this.modules.filter(m => m.isEnabled).length;
  }

  getTotalModulesCount(): number {
    return this.modules.length;
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getUsagePercentage(current: number, max: number | null | undefined): number {
    if (!max || max === 0) return 0;
    return Math.round((current / max) * 100);
  }

  getUsageClass(current: number, max: number | null | undefined): string {
    const percentage = this.getUsagePercentage(current, max);
    if (percentage >= 90) return 'usage-critical';
    if (percentage >= 75) return 'usage-warning';
    return 'usage-normal';
  }
}
