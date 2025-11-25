import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ComplaintInfoSettingsService } from '../../../services/complaint-info-settings.service';
import { AuthService } from '../../../services/auth.service';
import { ComplaintInformationSettings } from '../../../models/complaint-info-settings.model';

@Component({
  selector: 'app-complaint-info-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './complaint-info-settings.component.html',
  styleUrls: ['./complaint-info-settings.component.css']
})
export class ComplaintInfoSettingsComponent implements OnInit {
  settingsForm: FormGroup;
  loading = false;
  saving = false;
  error: string | null = null;
  successMessage: string | null = null;
  companyId: string = '';
  originalSettings: any = null;

  constructor(
    private fb: FormBuilder,
    private settingsService: ComplaintInfoSettingsService,
    private authService: AuthService
  ) {
    this.settingsForm = this.fb.group({
      // Handler visibility settings
      showEmployeeCodeToHandlers: [true],
      showEmailToHandlers: [true],
      showPhoneToHandlers: [true],
      showBranchToHandlers: [true],
      showDepartmentToHandlers: [true],
      showSectionToHandlers: [true],
      showJobTitleToHandlers: [true],
      showManagerDetailsToHandlers: [true],
      showPreviousComplaintsToHandlers: [true],

      // Management visibility settings
      showEmployeeAddressToManagement: [false],
      showEmergencyContactToManagement: [false],
      showPerformanceMetricsToManagement: [false],

      // Privacy settings
      maskPersonalInfoInLogs: [true],
      redactInfoAfterClosure: [false],
      dataRetentionDays: [0],

      // Report settings
      includeEmployeeCodeInReports: [true],
      includeEmailInReports: [true],
      includePhoneInReports: [true],
      maskEmailInReports: [false],
      maskPhoneInReports: [false]
    });
  }

  ngOnInit(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.companyId) {
      this.companyId = currentUser.companyId;
      this.loadSettings();
    } else {
      this.error = 'Unable to determine company ID. Please ensure you are logged in.';
    }
  }

  loadSettings(): void {
    this.loading = true;
    this.error = null;

    this.settingsService.getSettings(this.companyId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.originalSettings = { ...response.data };
          this.settingsForm.patchValue(response.data);
        } else {
          this.error = response.message || 'Failed to load settings';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load settings. Please try again.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSave(): void {
    if (this.settingsForm.valid) {
      this.saving = true;
      this.error = null;
      this.successMessage = null;

      this.settingsService.updateSettings(this.companyId, this.settingsForm.value).subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.successMessage = 'Settings updated successfully';
            this.originalSettings = { ...response.data };
            this.settingsForm.patchValue(response.data);
            setTimeout(() => this.successMessage = null, 5000);
          } else {
            this.error = response.message || 'Failed to save settings';
          }
          this.saving = false;
        },
        error: (err) => {
          this.error = 'Failed to save settings. Please try again.';
          this.saving = false;
          console.error(err);
        }
      });
    }
  }

  onReset(): void {
    if (this.originalSettings) {
      this.settingsForm.patchValue(this.originalSettings);
      this.successMessage = 'Form reset to saved values';
      setTimeout(() => this.successMessage = null, 3000);
    } else {
      this.loadSettings();
    }
  }

  hasChanges(): boolean {
    if (!this.originalSettings) return false;

    const currentValues = this.settingsForm.value;
    return Object.keys(currentValues).some(
      key => currentValues[key] !== this.originalSettings[key]
    );
  }

  toggleAllHandlerSettings(enabled: boolean): void {
    this.settingsForm.patchValue({
      showEmployeeCodeToHandlers: enabled,
      showEmailToHandlers: enabled,
      showPhoneToHandlers: enabled,
      showBranchToHandlers: enabled,
      showDepartmentToHandlers: enabled,
      showSectionToHandlers: enabled,
      showJobTitleToHandlers: enabled,
      showManagerDetailsToHandlers: enabled,
      showPreviousComplaintsToHandlers: enabled
    });
  }

  toggleAllManagementSettings(enabled: boolean): void {
    this.settingsForm.patchValue({
      showEmployeeAddressToManagement: enabled,
      showEmergencyContactToManagement: enabled,
      showPerformanceMetricsToManagement: enabled
    });
  }

  toggleAllReportSettings(enabled: boolean): void {
    this.settingsForm.patchValue({
      includeEmployeeCodeInReports: enabled,
      includeEmailInReports: enabled,
      includePhoneInReports: enabled
    });
  }
}
