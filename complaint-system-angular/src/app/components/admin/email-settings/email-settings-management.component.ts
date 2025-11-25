import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EmailSettingsService } from '../../../services/email-settings.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import {
  EmailServerSettings,
  CreateEmailServerSettingsRequest,
  UpdateEmailServerSettingsRequest
} from '../../../models/communication.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-email-settings-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './email-settings-management.component.html',
  styleUrls: ['./email-settings-management.component.scss']
})
export class EmailSettingsManagementComponent
  extends BaseMasterManagementComponent<EmailServerSettings, CreateEmailServerSettingsRequest, UpdateEmailServerSettingsRequest>
  implements OnInit {

  // Form model
  form!: (CreateEmailServerSettingsRequest & { id?: string }) | (UpdateEmailServerSettingsRequest & { code?: string });

  // Test email state
  isTestingEmail = false;
  testEmail = '';
  testResult: { success: boolean; message: string } | null = null;
  showTestDialog = false;
  testingSettings: EmailServerSettings | null = null;

  // Email provider presets
  emailProviders = [
    {
      name: 'Gmail',
      icon: 'fab fa-google',
      host: 'smtp.gmail.com',
      port: 587,
      useSsl: true,
      description: 'Google Gmail SMTP',
      notes: 'Use App Password for 2FA accounts'
    },
    {
      name: 'Outlook/Office 365',
      icon: 'fab fa-microsoft',
      host: 'smtp.office365.com',
      port: 587,
      useSsl: true,
      description: 'Microsoft Outlook/Office 365',
      notes: 'Standard Microsoft email service'
    },
    {
      name: 'Yahoo Mail',
      icon: 'fab fa-yahoo',
      host: 'smtp.mail.yahoo.com',
      port: 587,
      useSsl: true,
      description: 'Yahoo Mail SMTP',
      notes: 'Use App Password for 2FA accounts'
    },
    {
      name: 'Custom SMTP',
      icon: 'fas fa-server',
      host: '',
      port: 587,
      useSsl: true,
      description: 'Custom SMTP Server',
      notes: 'Configure your own SMTP server'
    }
  ];

  selectedProvider = this.emailProviders[3]; // Default to Custom
  showProviderDropdown = false;

  protected override get entityName(): string {
    return 'Email Server Settings';
  }

  // Getters/setters for email-specific fields
  get host(): string {
    return (this.form as any).host || '';
  }

  set host(value: string) {
    (this.form as any).host = value;
  }

  get port(): number {
    return (this.form as any).port || 587;
  }

  set port(value: number) {
    (this.form as any).port = value;
  }

  get useSsl(): boolean {
    return (this.form as any).useSsl !== undefined ? (this.form as any).useSsl : true;
  }

  set useSsl(value: boolean) {
    (this.form as any).useSsl = value;
  }

  get username(): string {
    return (this.form as any).username || '';
  }

  set username(value: string) {
    (this.form as any).username = value;
  }

  get password(): string {
    return (this.form as any).password || '';
  }

  set password(value: string) {
    (this.form as any).password = value;
  }

  get fromEmail(): string {
    return (this.form as any).fromEmail || '';
  }

  set fromEmail(value: string) {
    (this.form as any).fromEmail = value;
  }

  get fromName(): string {
    return (this.form as any).fromName || '';
  }

  set fromName(value: string) {
    (this.form as any).fromName = value;
  }

  get isDefault(): boolean {
    return (this.form as any).isDefault !== undefined ? (this.form as any).isDefault : false;
  }

  set isDefault(value: boolean) {
    (this.form as any).isDefault = value;
  }

  get maxEmailsPerHour(): number | undefined {
    return (this.form as any).maxEmailsPerHour;
  }

  set maxEmailsPerHour(value: number | undefined) {
    (this.form as any).maxEmailsPerHour = value;
  }

  get timeoutSeconds(): number {
    return (this.form as any).timeoutSeconds || 30;
  }

  set timeoutSeconds(value: number) {
    (this.form as any).timeoutSeconds = value;
  }

  get testNotes(): string {
    return (this.form as any).testNotes || '';
  }

  set testNotes(value: string) {
    (this.form as any).testNotes = value;
  }

  // OAuth fields
  get authenticationType(): number {
    return (this.form as any).authenticationType !== undefined ? Number((this.form as any).authenticationType) : 1;
  }

  set authenticationType(value: number | string) {
    (this.form as any).authenticationType = Number(value);
  }

  get oauthClientId(): string {
    return (this.form as any).oauthClientId || '';
  }

  set oauthClientId(value: string) {
    (this.form as any).oauthClientId = value;
  }

  get oauthClientSecret(): string {
    return (this.form as any).oauthClientSecret || '';
  }

  set oauthClientSecret(value: string) {
    (this.form as any).oauthClientSecret = value;
  }

  get oauthTenantId(): string {
    return (this.form as any).oauthTenantId || '';
  }

  set oauthTenantId(value: string) {
    (this.form as any).oauthTenantId = value;
  }

  get oauthTokenRefreshIntervalMinutes(): number | undefined {
    return (this.form as any).oauthTokenRefreshIntervalMinutes;
  }

  set oauthTokenRefreshIntervalMinutes(value: number | undefined) {
    (this.form as any).oauthTokenRefreshIntervalMinutes = value;
  }

  // Status filter
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  // Section collapse management for UI organization
  collapsedSections = {
    basic: false,      // Start expanded
    auth: false,       // Start expanded
    sender: true,      // Start collapsed
    advanced: true     // Start collapsed
  };

  // OAuth help panel
  showOAuthHelp = false;
  oauthHelpTab: 'office365' | 'gmail' = 'office365';

  constructor(
    private emailSettingsService: EmailSettingsService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.loadTestEmailFromLocalStorage();
    this.handleOAuthCallback(); // Check for OAuth callback params in URL
  }

  protected override getEmptyForm(): CreateEmailServerSettingsRequest {
    return {
      name: '',
      code: '', // Will be generated from host:port before save
      authenticationType: 0, // Default to Basic
      host: '',
      port: 587,
      useSsl: true,
      username: '',
      password: '',
      fromEmail: '',
      fromName: '',
      isActive: true,
      isDefault: false,
      timeoutSeconds: 30,
      // OAuth fields
      oauthClientId: '',
      oauthClientSecret: '',
      oauthTenantId: '',
      oauthTokenRefreshIntervalMinutes: undefined
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.emailSettingsService.getEmailSettings(true).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load email settings. Please try again.';
        this.loading = false;
        this.logger.error('Error loading email settings', error, 'EmailSettingsManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(s => s.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(s => !s.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(s =>
        s.name.toLowerCase().includes(search) ||
        s.host.toLowerCase().includes(search) ||
        s.fromEmail.toLowerCase().includes(search) ||
        s.username.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateEmailServerSettingsRequest): Observable<ApiResponse<EmailServerSettings>> {
    return this.emailSettingsService.createEmailSetting(request);
  }

  protected override updateItem(id: string, request: UpdateEmailServerSettingsRequest): Observable<ApiResponse<EmailServerSettings>> {
    return this.emailSettingsService.updateEmailSetting(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.emailSettingsService.deleteEmailSetting(id);
  }

  override openEditModal(item: EmailServerSettings): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit email settings';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Email Server Settings';

    this.form = {
      id: item.id,
      name: item.name,
      code: item.code || `${item.host}:${item.port}`, // Use code or generate from host:port
      authenticationType: item.authenticationType || 1, // Default to Basic Auth
      host: item.host,
      port: item.port,
      useSsl: item.useSsl,
      username: item.username,
      password: '', // Don't show existing password
      fromEmail: item.fromEmail,
      fromName: item.fromName,
      isActive: item.isActive,
      isDefault: item.isDefault,
      maxEmailsPerHour: item.maxEmailsPerHour,
      timeoutSeconds: item.timeoutSeconds,
      testNotes: item.testNotes,
      // OAuth fields
      oauthClientId: item.oauthClientId,
      oauthClientSecret: '',
      oauthTenantId: item.oauthTenantId,
      oauthTokenRefreshIntervalMinutes: item.oauthTokenRefreshIntervalMinutes
    };

    this.showModal = true;
    this.errorMessage = '';
    this.testResult = null;
  }

  protected override validateForm(): boolean {
    // Auto-generate code from host:port before validation
    if (!this.formCode || this.formCode.trim() === '') {
      this.formCode = `${this.host}:${this.port}`;
    }

    // Call base validation
    if (!super.validateForm()) {
      return false;
    }

    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.fromEmail)) {
      this.errorMessage = 'Please enter a valid From Email address';
      return false;
    }

    // Validate port range
    if (this.port < 1 || this.port > 65535) {
      this.errorMessage = 'Port must be between 1 and 65535';
      return false;
    }

    // Validate timeout
    if (this.timeoutSeconds < 10 || this.timeoutSeconds > 300) {
      this.errorMessage = 'Timeout must be between 10 and 300 seconds';
      return false;
    }

    // Validate password for new entries (only for Basic Auth, not OAuth2)
    if (this.modalMode === 'create' && this.authenticationType == 1 && !this.password) {
      this.errorMessage = 'Password is required for Basic Authentication';
      return false;
    }

    // Validate OAuth fields when using OAuth2
    if (this.authenticationType == 2) {
      if (!this.oauthClientId || this.oauthClientId.trim() === '') {
        this.errorMessage = 'OAuth Client ID is required for OAuth 2.0 authentication';
        return false;
      }
      if (this.modalMode === 'create' && (!this.oauthClientSecret || this.oauthClientSecret.trim() === '')) {
        this.errorMessage = 'OAuth Client Secret is required for OAuth 2.0 authentication';
        return false;
      }
      if (!this.oauthTenantId || this.oauthTenantId.trim() === '') {
        this.errorMessage = 'OAuth Tenant ID is required for OAuth 2.0 authentication';
        return false;
      }
    }

    return true;
  }

  override save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.createItem(this.form as CreateEmailServerSettingsRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Email server settings created successfully';
            this.logger.info('Email settings created', undefined, 'EmailSettingsManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create email settings';
            this.logger.error('Failed to create email settings', { message: response.message }, 'EmailSettingsManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          if (error.status === 401) {
            this.errorMessage = 'Your session has expired. Please log in again.';
          } else {
            this.errorMessage = error.error?.message || 'Failed to create email settings. Please try again.';
          }
          this.logger.error('Error creating email settings', error, 'EmailSettingsManagementComponent');
          this.loading = false;
        }
      });
    } else {
      const updateRequest: UpdateEmailServerSettingsRequest = {
        id: this.formId,
        name: this.formName,
        code: this.formCode || `${this.host}:${this.port}`,
        authenticationType: this.authenticationType,
        host: this.host,
        port: this.port,
        useSsl: this.useSsl,
        username: this.username,
        password: this.password || undefined,
        fromEmail: this.fromEmail,
        fromName: this.fromName,
        isActive: this.formIsActive,
        isDefault: this.isDefault,
        maxEmailsPerHour: this.maxEmailsPerHour,
        timeoutSeconds: this.timeoutSeconds,
        testNotes: this.testNotes,
        // OAuth fields
        oauthClientId: this.oauthClientId,
        oauthClientSecret: this.oauthClientSecret || undefined,
        oauthTenantId: this.oauthTenantId,
        oauthTokenRefreshIntervalMinutes: this.oauthTokenRefreshIntervalMinutes
      };

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Email server settings updated successfully';
            this.logger.info('Email settings updated', undefined, 'EmailSettingsManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update email settings';
            this.logger.error('Failed to update email settings', { message: response.message }, 'EmailSettingsManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update email settings. Please try again.';
          this.logger.error('Error updating email settings', error, 'EmailSettingsManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create email settings';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Email Server Settings';
    this.testResult = null;
  }

  override closeModal(): void {
    super.closeModal();
    this.testResult = null;
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.statusFilter = filter;
    this.filterItems();
  }

  toggleSettingsStatus(settings: EmailServerSettings): void {
    const updateRequest: UpdateEmailServerSettingsRequest = {
      id: settings.id,
      name: settings.name,
      code: settings.code,
      authenticationType: settings.authenticationType || 0,
      host: settings.host,
      port: settings.port,
      useSsl: settings.useSsl,
      username: settings.username,
      fromEmail: settings.fromEmail,
      fromName: settings.fromName,
      isActive: !settings.isActive,
      isDefault: settings.isDefault,
      maxEmailsPerHour: settings.maxEmailsPerHour,
      timeoutSeconds: settings.timeoutSeconds,
      testNotes: settings.testNotes,
      // OAuth fields
      oauthClientId: settings.oauthClientId,
      oauthTenantId: settings.oauthTenantId,
      oauthTokenRefreshIntervalMinutes: settings.oauthTokenRefreshIntervalMinutes
    };

    this.emailSettingsService.updateEmailSetting(settings.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          settings.isActive = !settings.isActive;
          this.successMessage = `Email settings ${settings.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Email settings status toggled', { settingsId: settings.id, isActive: settings.isActive }, 'EmailSettingsManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update settings status';
        this.logger.error('Error toggling settings status', error, 'EmailSettingsManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  setAsDefault(settings: EmailServerSettings): void {
    this.emailSettingsService.setAsDefault(settings.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Email settings set as default successfully';
          this.logger.info('Email settings set as default', { settingsId: settings.id }, 'EmailSettingsManagementComponent');
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to set settings as default';
        this.logger.error('Error setting settings as default', error, 'EmailSettingsManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  /**
   * Select email provider and populate form fields
   */
  selectEmailProvider(provider: any): void {
    this.selectedProvider = provider;
    this.host = provider.host;
    this.port = provider.port;
    this.useSsl = provider.useSsl;
    this.showProviderDropdown = false;

    // Update form name if empty
    if (!this.formName && provider.name !== 'Custom SMTP') {
      this.formName = `${provider.name} SMTP`;
    }

    this.logger.info('Email provider selected', { provider: provider.name }, 'EmailSettingsManagementComponent');
  }

  openTestDialog(settings: EmailServerSettings): void {
    this.testingSettings = settings;
    this.showTestDialog = true;
    this.testResult = null;
    this.errorMessage = '';
    this.loadTestEmailFromLocalStorage();
  }

  closeTestDialog(): void {
    this.showTestDialog = false;
    this.testingSettings = null;
    this.testResult = null;
    this.errorMessage = '';
  }

  testEmailConnection(settings?: EmailServerSettings): void {
    // If called from grid without testEmail, open dialog
    if (settings && !this.testEmail) {
      this.openTestDialog(settings);
      return;
    }

    if (!this.testEmail) {
      this.errorMessage = 'Please enter an email address to send test email';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.testEmail)) {
      this.errorMessage = 'Please enter a valid email address';
      return;
    }

    let settingsToTest: EmailServerSettings;

    if (settings) {
      // Test existing settings from grid
      settingsToTest = settings;
    } else if (this.testingSettings) {
      // Test from dialog
      settingsToTest = this.testingSettings;
    } else if (this.modalMode === 'edit' && this.formId) {
      // Test settings from modal form
      settingsToTest = {
        id: this.formId,
        name: this.formName,
        code: this.formCode || `${this.host}:${this.port}`,
        authenticationType: this.authenticationType,
        host: this.host,
        port: this.port,
        useSsl: this.useSsl,
        username: this.username,
        fromEmail: this.fromEmail,
        fromName: this.formName,
        isActive: this.formIsActive,
        isDefault: this.isDefault,
        timeoutSeconds: this.timeoutSeconds,
        maxEmailsPerHour: this.maxEmailsPerHour,
        createdAt: new Date().toISOString(),
        isDeleted: false,
        companyId: this.authService.currentUserValue?.companyId,
        updatedAt: new Date().toISOString(),
        // OAuth fields
        oauthClientId: this.oauthClientId,
        oauthTenantId: this.oauthTenantId,
        oauthTokenRefreshIntervalMinutes: this.oauthTokenRefreshIntervalMinutes
      };
    } else {
      this.errorMessage = 'No email settings available to test';
      return;
    }

    this.isTestingEmail = true;
    this.testResult = null;
    this.errorMessage = '';

    this.emailSettingsService.testEmailSetting(settingsToTest.id, this.testEmail).subscribe({
      next: (response) => {
        this.isTestingEmail = false;
        if (response.isSuccess) {
          this.testResult = { success: true, message: 'Test email sent successfully!' };
          this.logger.info('Test email sent', { settingsId: settingsToTest.id, testEmail: this.testEmail }, 'EmailSettingsManagementComponent');
          this.saveTestEmailToLocalStorage();
        } else {
          this.testResult = { success: false, message: response.message || 'Failed to send test email' };
          this.logger.error('Failed to send test email', { message: response.message }, 'EmailSettingsManagementComponent');
        }
      },
      error: (error) => {
        this.isTestingEmail = false;
        this.testResult = { success: false, message: error.error?.message || 'Failed to send test email' };
        this.logger.error('Error sending test email', error, 'EmailSettingsManagementComponent');
      }
    });
  }

  
  private loadTestEmailFromLocalStorage(): void {
    const savedEmail = localStorage.getItem('test-email');
    if (savedEmail) {
      this.testEmail = savedEmail;
    } else {
      // Default to current user's email
      const currentUser = this.authService.currentUserValue;
      if (currentUser?.email) {
        this.testEmail = currentUser.email;
      }
    }
  }

  private saveTestEmailToLocalStorage(): void {
    localStorage.setItem('test-email', this.testEmail);
  }

  getActiveSettingsCount(): number {
    return this.items.filter(s => s.isActive).length;
  }

  getInactiveSettingsCount(): number {
    return this.items.filter(s => !s.isActive).length;
  }

  navigateBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // TrackBy functions for *ngFor optimization
  trackBySettingsId(index: number, settings: EmailServerSettings): string {
    return settings.id;
  }

  trackByProviderName(index: number, provider: any): string {
    return provider.name;
  }

  /**
   * Override delete to immediately update UI
   */
  override executeDelete(): void {
    if (!this.itemToDelete) return;

    this.deleteLoading = true;
    this.errorMessage = '';

    const deletedItemId = this.itemToDelete.id;

    this.deleteItem(deletedItemId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          // Immediately remove the item from local arrays
          this.items = this.items.filter(item => item.id !== deletedItemId);
          this.filterItems(); // Update filtered items

          this.successMessage = `${this.entityName} deleted successfully`;
          this.logger.info(`${this.entityName} deleted`, { id: deletedItemId }, 'EmailSettingsManagementComponent');

          // Reload from server in background to ensure consistency
          this.loadItems();

          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || `Failed to delete ${this.entityName.toLowerCase()}`;
          this.logger.error(`Failed to delete ${this.entityName}`, { message: response.message }, 'EmailSettingsManagementComponent');
        }
        this.deleteLoading = false;
        this.cancelDelete();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || `Failed to delete ${this.entityName.toLowerCase()}. Please try again.`;
        this.logger.error(`Error deleting ${this.entityName}`, error, 'EmailSettingsManagementComponent');
        this.deleteLoading = false;
        this.cancelDelete();
      }
    });
  }

  // Section management methods
  toggleSection(section: 'basic' | 'auth' | 'sender' | 'advanced'): void {
    this.collapsedSections[section] = !this.collapsedSections[section];
  }

  // Check if basic info section is complete
  isBasicInfoComplete(): boolean {
    return !!(this.formName && this.host && this.port);
  }

  // Check if auth section is complete
  isAuthComplete(): boolean {
    if (this.authenticationType === 1) {
      return !!(this.username && (this.password || this.modalMode === 'edit'));
    } else {
      return !!(this.oauthClientId && this.oauthTenantId && (this.oauthClientSecret || this.modalMode === 'edit'));
    }
  }

  // Check if sender info section is complete
  isSenderInfoComplete(): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return !!(this.fromEmail && emailRegex.test(this.fromEmail) && this.fromName);
  }

  // Check if advanced section has content
  isAdvancedConfigured(): boolean {
    return this.isDefault || (this.maxEmailsPerHour !== undefined && this.maxEmailsPerHour > 0) || this.timeoutSeconds !== 30;
  }

  // Calculate overall form completion percentage
  getFormCompletionPercentage(): number {
    const sections = [
      this.isBasicInfoComplete(),
      this.isAuthComplete(),
      this.isSenderInfoComplete()
    ];
    const completed = sections.filter(s => s).length;
    return Math.round((completed / sections.length) * 100);
  }

  // Get OAuth callback URL for instructions
  getOAuthCallbackUrl(): string {
    return `${window.location.origin}/api/oauth/callback`;
  }

  // Copy text to clipboard
  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.successMessage = 'Copied to clipboard!';
      setTimeout(() => this.successMessage = '', 2000);
    }).catch(err => {
      console.error('Failed to copy text: ', err);
    });
  }

  /**
   * Initiates OAuth 2.0 authorization flow for SMTP
   * Opens authorization URL in a new window
   */
  initiateOAuthFlow(): void {
    if (!this.formId) {
      this.errorMessage = 'Please save the email settings first before authorizing OAuth';
      return;
    }

    if (!this.oauthClientId || !this.oauthTenantId) {
      this.errorMessage = 'Please fill in OAuth Client ID and Tenant ID before authorizing';
      return;
    }

    // Save the current settings first to ensure OAuth credentials are persisted
    if (this.modalMode === 'edit') {
      this.logger.info('Initiating OAuth authorization flow for SMTP settings', { settingsId: this.formId }, 'EmailSettingsManagementComponent');

      // Construct OAuth authorization URL
      const authUrl = `http://localhost:5000/api/oauth/authorize-smtp/${this.formId}`;

      // Open in a popup window
      const popup = window.open(
        authUrl,
        'oauth-authorize',
        'width=600,height=700,scrollbars=yes,resizable=yes'
      );

      if (!popup) {
        this.errorMessage = 'Popup blocked! Please allow popups for this site and try again.';
        return;
      }

      // Close modal to see the popup better
      this.showModal = false;

      this.logger.info('OAuth authorization popup opened', { settingsId: this.formId }, 'EmailSettingsManagementComponent');
    }
  }

  /**
   * Handle OAuth callback (called when user returns from Microsoft OAuth)
   */
  handleOAuthCallback(): void {
    // Check URL for oauth success/error params
    const urlParams = new URLSearchParams(window.location.search);
    const oauthStatus = urlParams.get('oauth');
    const settingsId = urlParams.get('settingsId');
    const message = urlParams.get('message');

    if (oauthStatus === 'success' && settingsId) {
      this.successMessage = 'OAuth authorization successful! You can now send emails using this account.';
      this.logger.info('OAuth authorization successful', { settingsId }, 'EmailSettingsManagementComponent');

      // Reload items to get updated token status
      this.loadItems();

      // Clear URL params
      window.history.replaceState({}, document.title, window.location.pathname);

      setTimeout(() => this.successMessage = '', 5000);
    } else if (oauthStatus === 'error') {
      this.errorMessage = `OAuth authorization failed: ${message || 'Unknown error'}`;
      this.logger.error('OAuth authorization failed', { message }, 'EmailSettingsManagementComponent');

      // Clear URL params
      window.history.replaceState({}, document.title, window.location.pathname);

      setTimeout(() => this.errorMessage = '', 5000);
    }
  }
}
