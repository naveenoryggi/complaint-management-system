import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { EmailTicketingConfigService } from '../../../services/email-ticketing-config.service';
import { SystemConfigurationService } from '../../../services/system-configuration.service';
import { LoggerService } from '../../../core/services/logger.service';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';
import {
  EmailConfiguration,
  CreateEmailConfigurationRequest,
  UpdateEmailConfigurationRequest,
  EMAIL_PROVIDER_PRESETS,
  EmailProviderPreset
} from '../../../models/communication.model';
import { SystemConfiguration, validateSystemConfiguration } from '../../../models/system-configuration.model';

@Component({
  selector: 'app-email-ticketing-config',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './email-ticketing-config.component.html',
  styleUrls: ['./email-ticketing-config.component.scss']
})
export class EmailTicketingConfigComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // State
  configurations: EmailConfiguration[] = [];
  selectedConfig: EmailConfiguration | null = null;
  isLoading = false;
  isSaving = false;
  showForm = false;
  isEditMode = false;

  // OAuth Wizard State
  wizardStep: number = 1;
  selectedInstructionsTab: 'office365' | 'gmail' = 'office365';

  // Form model
  form: any = this.getEmptyForm();

  // Provider presets
  emailProviders = EMAIL_PROVIDER_PRESETS;
  selectedProvider: EmailProviderPreset = EMAIL_PROVIDER_PRESETS[4]; // Custom by default
  showProviderDropdown = false;

  // OAuth provider configurations
  oauthProviders: EmailProviderPreset[] = [
    {
      name: 'Office 365',
      icon: 'fab fa-microsoft',
      imapHost: 'outlook.office365.com',
      imapPort: 993,
      imapUseSsl: true,
      smtpHost: 'smtp.office365.com',
      smtpPort: 587,
      smtpUseSsl: true,
      notes: 'Microsoft 365 Business & Enterprise'
    },
    {
      name: 'Gmail',
      icon: 'fab fa-google',
      imapHost: 'imap.gmail.com',
      imapPort: 993,
      imapUseSsl: true,
      smtpHost: 'smtp.gmail.com',
      smtpPort: 587,
      smtpUseSsl: true,
      notes: 'Google Workspace & Gmail'
    },
    {
      name: 'Outlook.com',
      icon: 'fas fa-envelope',
      imapHost: 'outlook.office365.com',
      imapPort: 993,
      imapUseSsl: true,
      smtpHost: 'smtp-mail.outlook.com',
      smtpPort: 587,
      smtpUseSsl: true,
      notes: 'Personal Outlook accounts'
    }
  ];

  // System Configuration
  systemConfig: SystemConfiguration | null = null;
  showSystemSettings = false;
  isSavingSystemConfig = false;

  // Testing state
  isTestingImap = false;
  isTestingSmtp = false;
  isPolling = false;
  testImapResult: { success: boolean; message: string } | null = null;
  testSmtpResult: { success: boolean; message: string } | null = null;
  pollResult: any = null;

  // Validation
  validationErrors: string[] = [];

  constructor(
    private configService: EmailTicketingConfigService,
    private systemConfigService: SystemConfigurationService,
    private logger: LoggerService,
    private router: Router
  ) {}


  ngOnInit(): void {
    // Load both email configurations and system configuration in parallel
    forkJoin({
      emailConfigs: this.configService.getConfigurations(false),
      systemConfig: this.systemConfigService.getConfiguration()
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          // Extract data from API response wrapper
          if (result.emailConfigs.isSuccess && result.emailConfigs.data) {
            this.configurations = result.emailConfigs.data;
          }
          this.systemConfig = result.systemConfig;
          this.logger.info('Loaded email configurations and system configuration', {
            emailConfigCount: this.configurations.length,
            systemConfigLoaded: !!this.systemConfig
          });
        },
        error: (error) => {
          this.logger.error('Error loading configurations', error);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==========================
  // Data Loading
  // ==========================

  loadConfigurations(): void {
    this.isLoading = true;
    this.configService.getConfigurations(false)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.configurations = response.data;
            this.logger.info('Email configurations loaded', { count: this.configurations.length });
          } else {
            this.logger.error('Failed to load configurations:', response.message);
            this.showError(response.message || 'Failed to load configurations');
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.logger.error('Error loading configurations:', error);
          this.showError('An error occurred while loading configurations');
          this.isLoading = false;
        }
      });
  }

  // ==========================
  // CRUD Operations
  // ==========================

  showCreateForm(): void {
    this.form = this.getEmptyForm();
    this.isEditMode = false;
    this.showForm = true;
    this.validationErrors = [];
    this.testImapResult = null;
    this.testSmtpResult = null;
    this.wizardStep = 1;
    this.selectedInstructionsTab = 'office365';
  }

  showEditForm(config: EmailConfiguration): void {
    this.form = { ...config };
    this.selectedConfig = config;
    this.isEditMode = true;
    this.showForm = true;
    this.validationErrors = [];
    this.testImapResult = null;
    this.testSmtpResult = null;

    // Set selected provider
    const provider = this.emailProviders.find(
      p => p.imapHost === config.imapHost && p.smtpHost === config.smtpHost
    );
    this.selectedProvider = provider || this.emailProviders[4]; // Custom if no match
  }

  cancelForm(): void {
    this.showForm = false;
    this.form = this.getEmptyForm();
    this.selectedConfig = null;
    this.validationErrors = [];
    this.testImapResult = null;
    this.testSmtpResult = null;
    this.wizardStep = 1;
  }

  saveConfiguration(): void {
    // Validate
    const validation = this.configService.validateConfiguration(this.form);
    if (!validation.isValid) {
      this.validationErrors = validation.errors;
      this.showError('Please fix validation errors');
      return;
    }

    this.isSaving = true;
    this.validationErrors = [];

    if (this.isEditMode && this.selectedConfig) {
      // Update
      this.configService.updateConfiguration(this.selectedConfig.id, this.form)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.showSuccess('Configuration updated successfully');
              this.cancelForm();
              this.loadConfigurations();
            } else {
              this.showError(response.message || 'Failed to update configuration');
            }
            this.isSaving = false;
          },
          error: (error) => {
            this.logger.error('Error updating configuration:', error);
            this.showError('An error occurred while updating configuration');
            this.isSaving = false;
          }
        });
    } else {
      // Create
      this.configService.createConfiguration(this.form)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.isSuccess && response.data) {
              // Check if OAuth authentication is selected
              if (this.form.authenticationType === 1) {
                // OAuth flow - redirect to authorization endpoint
                this.logger.info('OAuth configuration created, redirecting to authorization', { configId: response.data.id });
                window.location.href = `/api/oauth/authorize/${response.data.id}`;
              } else {
                // Basic auth - show success and reload
                this.showSuccess('Configuration created successfully');
                this.cancelForm();
                this.loadConfigurations();
              }
            } else {
              this.showError(response.message || 'Failed to create configuration');
            }
            this.isSaving = false;
          },
          error: (error) => {
            this.logger.error('Error creating configuration:', error);
            this.showError('An error occurred while creating configuration');
            this.isSaving = false;
          }
        });
    }
  }

  deleteConfiguration(config: EmailConfiguration): void {
    if (!confirm(`Are you sure you want to delete the configuration for ${config.fromEmail}?`)) {
      return;
    }

    this.configService.deleteConfiguration(config.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.showSuccess('Configuration deleted successfully');
            this.loadConfigurations();
          } else {
            this.showError(response.message || 'Failed to delete configuration');
          }
        },
        error: (error) => {
          this.logger.error('Error deleting configuration:', error);
          this.showError('An error occurred while deleting configuration');
        }
      });
  }

  toggleConfiguration(config: EmailConfiguration): void {
    const action = config.isEnabled ? 'disable' : 'enable';
    if (!confirm(`Are you sure you want to ${action} this configuration?`)) {
      return;
    }

    this.configService.toggleConfiguration(config.id, !config.isEnabled)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.showSuccess(`Configuration ${action}d successfully`);
            this.loadConfigurations();
          } else {
            this.showError(response.message || `Failed to ${action} configuration`);
          }
        },
        error: (error) => {
          this.logger.error(`Error ${action}ing configuration:`, error);
          this.showError(`An error occurred while ${action}ing configuration`);
        }
      });
  }

  // ==========================
  // Testing
  // ==========================

  testImapConnection(): void {
    if (!this.selectedConfig) {
      this.showError('Please save the configuration first');
      return;
    }

    this.isTestingImap = true;
    this.testImapResult = null;

    this.configService.testImapConnection(this.selectedConfig.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.testImapResult = response.data;
            if (response.data.success) {
              this.showSuccess('IMAP connection successful');
            } else {
              this.showError('IMAP connection failed: ' + response.data.message);
            }
          } else {
            this.testImapResult = { success: false, message: response.message || 'Test failed' };
            this.showError('IMAP test failed');
          }
          this.isTestingImap = false;
        },
        error: (error) => {
          this.logger.error('Error testing IMAP:', error);
          this.testImapResult = { success: false, message: 'Connection error' };
          this.showError('An error occurred while testing IMAP connection');
          this.isTestingImap = false;
        }
      });
  }

  testSmtpConnection(): void {
    if (!this.selectedConfig) {
      this.showError('Please save the configuration first');
      return;
    }

    this.isTestingSmtp = true;
    this.testSmtpResult = null;

    this.configService.testSmtpConnection(this.selectedConfig.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.testSmtpResult = response.data;
            if (response.data.success) {
              this.showSuccess('SMTP connection successful');
            } else {
              this.showError('SMTP connection failed: ' + response.data.message);
            }
          } else {
            this.testSmtpResult = { success: false, message: response.message || 'Test failed' };
            this.showError('SMTP test failed');
          }
          this.isTestingSmtp = false;
        },
        error: (error) => {
          this.logger.error('Error testing SMTP:', error);
          this.testSmtpResult = { success: false, message: 'Connection error' };
          this.showError('An error occurred while testing SMTP connection');
          this.isTestingSmtp = false;
        }
      });
  }

  pollEmailsNow(config: EmailConfiguration): void {
    this.isPolling = true;
    this.pollResult = null;

    this.configService.pollEmailsNow(config.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.pollResult = response.data;
            this.showSuccess(`Polling complete: ${response.data.totalEmailsFetched} emails fetched, ${response.data.newTicketsCreated} complaints created`);
            this.loadConfigurations();
          } else {
            this.showError(response.message || 'Polling failed');
          }
          this.isPolling = false;
        },
        error: (error) => {
          this.logger.error('Error polling emails:', error);
          this.showError('An error occurred while polling emails');
          this.isPolling = false;
        }
      });
  }

  // ==========================
  // OAuth Wizard Methods
  // ==========================

  selectAuthType(type: number): void {
    this.form.authenticationType = type;
    this.wizardStep = 1;
    this.logger.info('Authentication type selected', { type: type === 2 ? 'OAuth2' : 'Basic' });
  }

  nextWizardStep(): void {
    if (this.wizardStep < 6) { // Now we have 6 steps total (with step 2.5 being step 3)
      this.wizardStep++;
      this.logger.info('Wizard step advanced', { step: this.wizardStep });
    }
  }

  prevWizardStep(): void {
    if (this.wizardStep > 1) {
      this.wizardStep--;
      this.logger.info('Wizard step back', { step: this.wizardStep });
    } else if (this.wizardStep === 1) {
      // If on first step, close the dialog
      this.cancelForm();
      this.logger.info('Closing dialog from step 1');
    }
  }

  selectInstructionsTab(tab: 'office365' | 'gmail'): void {
    this.selectedInstructionsTab = tab;
  }

  getOAuthCallbackUrl(): string {
    return `${window.location.origin}/api/oauth/callback`;
  }

  copyToClipboard(text: string): void {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        this.showSuccess('Copied to clipboard!');
      }).catch(err => {
        this.logger.error('Failed to copy to clipboard', err);
        this.fallbackCopyToClipboard(text);
      });
    } else {
      this.fallbackCopyToClipboard(text);
    }
  }

  private fallbackCopyToClipboard(text: string): void {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    document.body.appendChild(textArea);
    textArea.select();
    try {
      document.execCommand('copy');
      this.showSuccess('Copied to clipboard!');
    } catch (err) {
      this.logger.error('Fallback copy failed', err);
      this.showError('Failed to copy to clipboard');
    }
    document.body.removeChild(textArea);
  }

  isTokenExpiringSoon(expiresAt: string | undefined): boolean {
    if (!expiresAt) return false;
    const expiryDate = new Date(expiresAt);
    const now = new Date();
    const daysUntilExpiry = (expiryDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
    return daysUntilExpiry < 7; // Warning if less than 7 days
  }

  refreshOAuth(config: EmailConfiguration): void {
    this.logger.info('Refreshing OAuth token', { configId: config.id });
    // Use absolute URL to backend server (not relative URL which goes to Angular dev server)
    window.location.href = `http://localhost:5000/api/oauth/authorize/${config.id}`;
  }

  getTokenExpiryText(expiresAt: string | undefined): string {
    if (!expiresAt) return 'Unknown';
    // Ensure we treat the date as UTC by appending 'Z' if not present
    const dateString = expiresAt.endsWith('Z') ? expiresAt : expiresAt + 'Z';
    const expiryDate = new Date(dateString);
    const now = new Date();
    const daysUntilExpiry = Math.floor((expiryDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

    if (daysUntilExpiry < 0) return 'Expired';
    if (daysUntilExpiry === 0) return 'Expires today';
    if (daysUntilExpiry === 1) return 'Expires tomorrow';
    return `Expires in ${daysUntilExpiry} days`;
  }

  // Check if OAuth configuration is complete but not authorized
  isOAuthPendingAuthorization(config: EmailConfiguration): boolean {
    if (config.authenticationType !== 2) return false; // Not OAuth (2 = OAuth2)

    // Has OAuth credentials configured
    const hasCredentials = !!(config.oauthClientId && config.oauthClientSecret);

    // But no access token yet
    const hasToken = !!config.oAuthAccessToken;

    return hasCredentials && !hasToken;
  }

  // Check if OAuth is authorized and active
  isOAuthAuthorized(config: EmailConfiguration): boolean {
    if (config.authenticationType !== 2) return false; // 2 = OAuth2
    return !!(config.oAuthAccessToken && config.oAuthTokenExpiresAt);
  }

  // Check if OAuth token is expired
  isOAuthTokenExpired(config: EmailConfiguration): boolean {
    if (!config.oAuthTokenExpiresAt) return false;
    // Ensure we treat the date as UTC by appending 'Z' if not present
    const dateString = config.oAuthTokenExpiresAt.endsWith('Z')
      ? config.oAuthTokenExpiresAt
      : config.oAuthTokenExpiresAt + 'Z';
    const expiryDate = new Date(dateString);
    return expiryDate.getTime() < Date.now();
  }

  // Get OAuth status text for display
  getOAuthStatusText(config: EmailConfiguration): string {
    if (config.authenticationType === 1) return 'Basic Auth';

    if (this.isOAuthTokenExpired(config)) return 'OAuth 2.0 - Expired';
    if (this.isOAuthAuthorized(config)) return 'OAuth 2.0 - Authorized';
    if (this.isOAuthPendingAuthorization(config)) return 'OAuth 2.0 - Pending';
    return 'OAuth 2.0 - Not Configured';
  }

  // Get OAuth status CSS class
  getOAuthStatusClass(config: EmailConfiguration): string {
    if (config.authenticationType === 1) return 'basic';

    if (this.isOAuthTokenExpired(config)) return 'oauth-expired';
    if (this.isOAuthAuthorized(config)) return 'oauth-authorized';
    if (this.isOAuthPendingAuthorization(config)) return 'oauth-pending';
    return 'oauth-not-configured';
  }

  // ==========================
  // Provider Presets
  // ==========================

  selectProvider(provider: EmailProviderPreset): void {
    this.selectedProvider = provider;
    this.form.imapHost = provider.imapHost;
    this.form.imapPort = provider.imapPort;
    this.form.imapUseSsl = provider.imapUseSsl;
    this.form.smtpHost = provider.smtpHost;
    this.form.smtpPort = provider.smtpPort;
    this.form.smtpUseSsl = provider.smtpUseSsl;
    this.showProviderDropdown = false;

    // Clear OAuth fields when switching providers (they need to enter new credentials)
    if (this.form.authenticationType === 1) {
      this.form.oauthClientId = '';
      this.form.oauthClientSecret = '';
      this.form.oauthTenantId = '';
      this.logger.info('OAuth fields cleared for new provider', { provider: provider.name });
    }
  }

  toggleProviderDropdown(): void {
    this.showProviderDropdown = !this.showProviderDropdown;
  }

  /**
   * Get human-readable polling interval display
   */
  getPollingIntervalDisplay(config: EmailConfiguration): string {
    const seconds = config.pollingIntervalSeconds || (config.pollingIntervalMinutes * 60);

    if (seconds < 60) {
      return `${seconds} seconds`;
    } else if (seconds < 3600) {
      const minutes = Math.floor(seconds / 60);
      return minutes === 1 ? '1 minute' : `${minutes} minutes`;
    } else {
      const hours = Math.floor(seconds / 3600);
      return hours === 1 ? '1 hour' : `${hours} hours`;
    }
  }

  // ==========================
  // Helper Methods
  // ==========================

  private getEmptyForm(): CreateEmailConfigurationRequest {
    return {
      authenticationType: 1, // OAuth by default
      imapHost: '',
      imapPort: 993,
      imapUseSsl: true,
      imapUsername: '',
      imapPassword: '',
      imapFolder: 'INBOX',
      smtpHost: '',
      smtpPort: 587,
      smtpUseSsl: true,
      smtpUsername: '',
      smtpPassword: '',
      fromEmail: '',
      fromName: '',
      pollingIntervalMinutes: 5,
      pollingIntervalSeconds: 120, // Default to 2 minutes
      isEnabled: true,
      sendAutoAcknowledgement: true,
      enableThreading: true,
      threadTimeoutDays: 7,
      maxAttachmentSizeBytes: 10485760, // 10 MB
      allowedAttachmentExtensions: '.pdf,.doc,.docx,.txt,.jpg,.jpeg,.png,.gif',
      oauthClientId: '',
      oauthTenantId: '',
      oauthClientSecret: '',
      oauthTokenRefreshIntervalMinutes: undefined, // null = use system default
      // Separate SMTP Account fields
      useSeparateSmtpAccount: false,
      smtpAuthenticationType: 1, // OAuth by default
      smtpSeparateUsername: '',
      smtpSeparatePassword: '',
      smtpSeparateFromEmail: '',
      smtpSeparateFromName: '',
      smtpSeparateOAuthClientId: '',
      smtpSeparateOAuthClientSecret: '',
      smtpSeparateOAuthTenantId: '',
      smtpSeparateOAuthScopes: ''
    };
  }

  private showSuccess(message: string): void {
    this.logger.info('Success', { message });
    alert(message); // Replace with toast notification
  }

  private showError(message: string): void {
    this.logger.error('Error:', message);
    alert(message); // Replace with toast notification
  }

  formatDate(date: string | undefined): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleString();
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  // TrackBy functions for *ngFor optimization
  trackByConfigId(index: number, config: EmailConfiguration): string {
    return config.id;
  }

  trackByValidationError(index: number, error: string): number {
    return index;
  }

  trackByProviderName(index: number, provider: EmailProviderPreset): string {
    return provider.name;
  }

  // ==========================
  // System Configuration Management
  // ==========================

  toggleSystemSettings(): void {
    this.showSystemSettings = !this.showSystemSettings;
    if (this.showSystemSettings && !this.systemConfig) {
      // Load system config if not already loaded
      this.systemConfigService.getConfiguration()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (config) => {
            this.systemConfig = config;
            this.logger.info('System configuration loaded', config);
          },
          error: (error) => {
            this.logger.error('Error loading system configuration', error);
            this.showError('Failed to load system configuration');
          }
        });
    }
  }

  saveSystemConfiguration(): void {
    if (!this.systemConfig) {
      this.showError('No system configuration to save');
      return;
    }

    // Validate before saving
    const validation = validateSystemConfiguration(this.systemConfig);
    if (!validation.isValid) {
      this.showError('Validation failed:\n' + validation.errors.join('\n'));
      return;
    }

    this.isSavingSystemConfig = true;
    this.systemConfigService.updateConfiguration(this.systemConfig)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedConfig) => {
          this.systemConfig = updatedConfig;
          this.isSavingSystemConfig = false;
          this.showSuccess('System configuration saved successfully! OAuth token refresh will use new settings on next cycle.');
          this.logger.info('System configuration saved', updatedConfig);
        },
        error: (error) => {
          this.isSavingSystemConfig = false;
          this.logger.error('Error saving system configuration', error);
          this.showError('Failed to save system configuration: ' + (error.error?.message || error.message));
        }
      });
  }

  resetSystemConfiguration(): void {
    if (!confirm('Are you sure you want to reset all system settings to defaults? This will reset OAuth refresh intervals, email polling settings, and other system-wide configurations.')) {
      return;
    }

    this.isSavingSystemConfig = true;
    this.systemConfigService.resetConfiguration()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (config) => {
          this.systemConfig = config;
          this.isSavingSystemConfig = false;
          this.showSuccess('System configuration reset to defaults successfully!');
          this.logger.info('System configuration reset', config);
        },
        error: (error) => {
          this.isSavingSystemConfig = false;
          this.logger.error('Error resetting system configuration', error);
          this.showError('Failed to reset system configuration');
        }
      });
  }

  getRefreshIntervalDisplay(): string {
    if (!this.systemConfig) return '';
    return this.systemConfigService.formatRefreshInterval(this.systemConfig.oAuthTokenRefreshIntervalMinutes);
  }

  getSystemPollingIntervalDisplay(): string {
    if (!this.systemConfig) return '';
    return this.systemConfigService.formatPollingInterval(this.systemConfig.defaultEmailPollingIntervalSeconds);
  }
}
