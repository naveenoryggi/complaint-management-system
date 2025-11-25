import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  WhatsAppSettingsService,
  WhatsAppSettings,
  CreateWhatsAppRequest,
  UpdateWhatsAppRequest
} from '../../../services/whatsapp-settings.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-whatsapp-settings-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './whatsapp-settings-management.component.html',
  styleUrls: ['./whatsapp-settings-management.component.scss']
})
export class WhatsAppSettingsManagementComponent
  extends BaseMasterManagementComponent<WhatsAppSettings, CreateWhatsAppRequest, UpdateWhatsAppRequest>
  implements OnInit {

  // Form model
  form!: (CreateWhatsAppRequest & { id?: string }) | (UpdateWhatsAppRequest & { code?: string });

  // Test WhatsApp state
  isTestingWhatsApp = false;
  testPhoneNumber = '';
  testMessage = 'This is a test message from Complaint Management System.';
  testResult: { success: boolean; message: string } | null = null;

  // Popular providers
  providers = [
    { value: 'WhatsApp_Business_API', label: 'WhatsApp Business API (Official)' },
    { value: 'Twilio_WhatsApp', label: 'Twilio WhatsApp' },
    { value: 'MessageBird_WhatsApp', label: 'MessageBird WhatsApp' },
    { value: 'Custom', label: 'Custom Provider' }
  ];

  // Media storage types
  mediaStorageTypes = [
    { value: 'Local', label: 'Local File System' },
    { value: 'S3', label: 'Amazon S3' },
    { value: 'Azure_Blob', label: 'Azure Blob Storage' },
    { value: 'Google_Cloud', label: 'Google Cloud Storage' }
  ];

  protected override get entityName(): string {
    return 'WhatsApp Settings';
  }

  // Getters/setters for WhatsApp-specific fields
  get provider(): string {
    return (this.form as any).provider || '';
  }

  set provider(value: string) {
    (this.form as any).provider = value;
  }

  get apiUrl(): string {
    return (this.form as any).apiUrl || '';
  }

  set apiUrl(value: string) {
    (this.form as any).apiUrl = value;
  }

  get businessAccountId(): string {
    return (this.form as any).businessAccountId || '';
  }

  set businessAccountId(value: string) {
    (this.form as any).businessAccountId = value;
  }

  get phoneNumberId(): string {
    return (this.form as any).phoneNumberId || '';
  }

  set phoneNumberId(value: string) {
    (this.form as any).phoneNumberId = value;
  }

  get accessToken(): string {
    return (this.form as any).accessToken || '';
  }

  set accessToken(value: string) {
    (this.form as any).accessToken = value;
  }

  get webhookToken(): string {
    return (this.form as any).webhookToken || '';
  }

  set webhookToken(value: string) {
    (this.form as any).webhookToken = value;
  }

  get fromNumber(): string {
    return (this.form as any).fromNumber || '';
  }

  set fromNumber(value: string) {
    (this.form as any).fromNumber = value;
  }

  get businessName(): string {
    return (this.form as any).businessName || '';
  }

  set businessName(value: string) {
    (this.form as any).businessName = value;
  }

  get isDefault(): boolean {
    return (this.form as any).isDefault !== undefined ? (this.form as any).isDefault : false;
  }

  set isDefault(value: boolean) {
    (this.form as any).isDefault = value;
  }

  get maxMessagesPerHour(): number | undefined {
    return (this.form as any).maxMessagesPerHour;
  }

  set maxMessagesPerHour(value: number | undefined) {
    (this.form as any).maxMessagesPerHour = value;
  }

  get timeoutSeconds(): number {
    return (this.form as any).timeoutSeconds || 30;
  }

  set timeoutSeconds(value: number) {
    (this.form as any).timeoutSeconds = value;
  }

  get mediaStorageType(): string {
    return (this.form as any).mediaStorageType || '';
  }

  set mediaStorageType(value: string) {
    (this.form as any).mediaStorageType = value;
  }

  get mediaStoragePath(): string {
    return (this.form as any).mediaStoragePath || '';
  }

  set mediaStoragePath(value: string) {
    (this.form as any).mediaStoragePath = value;
  }

  get mediaStorageConfig(): string {
    return (this.form as any).mediaStorageConfig || '';
  }

  set mediaStorageConfig(value: string) {
    (this.form as any).mediaStorageConfig = value;
  }

  get mediaPublicBaseUrl(): string {
    return (this.form as any).mediaPublicBaseUrl || '';
  }

  set mediaPublicBaseUrl(value: string) {
    (this.form as any).mediaPublicBaseUrl = value;
  }

  get mediaRetentionDays(): number | undefined {
    return (this.form as any).mediaRetentionDays;
  }

  set mediaRetentionDays(value: number | undefined) {
    (this.form as any).mediaRetentionDays = value;
  }

  get maxMediaSizeMB(): number | undefined {
    return (this.form as any).maxMediaSizeMB;
  }

  set maxMediaSizeMB(value: number | undefined) {
    (this.form as any).maxMediaSizeMB = value;
  }

  get additionalConfig(): string {
    return (this.form as any).additionalConfig || '';
  }

  set additionalConfig(value: string) {
    (this.form as any).additionalConfig = value;
  }

  get testNotes(): string {
    return (this.form as any).testNotes || '';
  }

  set testNotes(value: string) {
    (this.form as any).testNotes = value;
  }

  // Status filter
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  constructor(
    private whatsappService: WhatsAppSettingsService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.loadTestPhoneFromLocalStorage();
  }

  protected override getEmptyForm(): CreateWhatsAppRequest {
    return {
      name: '',
      code: '',
      provider: '',
      apiUrl: '',
      businessAccountId: '',
      phoneNumberId: '',
      accessToken: '',
      fromNumber: '',
      businessName: '',
      isActive: true,
      isDefault: false,
      timeoutSeconds: 30
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.whatsappService.getAll(true).subscribe({
      next: (response) => {
        this.items = response.data || [];
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load WhatsApp settings. Please try again.';
        this.loading = false;
        this.logger.error('Error loading WhatsApp settings', error, 'WhatsAppSettingsManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    if (this.statusFilter === 'active') {
      filtered = filtered.filter(s => s.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(s => !s.isActive);
    }

    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(s =>
        s.name.toLowerCase().includes(search) ||
        s.provider.toLowerCase().includes(search) ||
        s.fromNumber.toLowerCase().includes(search) ||
        s.businessName.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateWhatsAppRequest): Observable<ApiResponse<WhatsAppSettings>> {
    return this.whatsappService.create(request);
  }

  protected override updateItem(id: string, request: UpdateWhatsAppRequest): Observable<ApiResponse<WhatsAppSettings>> {
    return this.whatsappService.update(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.whatsappService.delete(id);
  }

  override openEditModal(item: WhatsAppSettings): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit WhatsApp settings';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit WhatsApp Settings';

    this.form = {
      id: item.id,
      name: item.name,
      code: item.code || '',
      provider: item.provider,
      apiUrl: item.apiUrl,
      businessAccountId: item.businessAccountId,
      phoneNumberId: item.phoneNumberId,
      accessToken: '',
      webhookToken: item.webhookToken,
      fromNumber: item.fromNumber,
      businessName: item.businessName,
      isActive: item.isActive,
      isDefault: item.isDefault,
      maxMessagesPerHour: item.maxMessagesPerHour,
      timeoutSeconds: item.timeoutSeconds,
      mediaStorageType: item.mediaStorageType,
      mediaStoragePath: item.mediaStoragePath,
      mediaStorageConfig: item.mediaStorageConfig,
      mediaPublicBaseUrl: item.mediaPublicBaseUrl,
      mediaRetentionDays: item.mediaRetentionDays,
      maxMediaSizeMB: item.maxMediaSizeMB,
      additionalConfig: item.additionalConfig,
      testNotes: item.testNotes
    };

    this.showModal = true;
    this.errorMessage = '';
    this.testResult = null;
  }

  protected override validateForm(): boolean {
    if (!super.validateForm()) {
      return false;
    }

    if (!this.provider || this.provider.trim() === '') {
      this.errorMessage = 'Please select or enter a provider';
      return false;
    }

    if (!this.apiUrl || this.apiUrl.trim() === '') {
      this.errorMessage = 'API URL is required';
      return false;
    }

    try {
      new URL(this.apiUrl);
    } catch {
      this.errorMessage = 'Please enter a valid API URL';
      return false;
    }

    const phoneRegex = /^\+?[1-9]\d{1,14}$/;
    if (!phoneRegex.test(this.fromNumber.replace(/[\s-]/g, ''))) {
      this.errorMessage = 'Please enter a valid phone number (international format recommended, e.g., +1234567890)';
      return false;
    }

    if (this.timeoutSeconds < 10 || this.timeoutSeconds > 300) {
      this.errorMessage = 'Timeout must be between 10 and 300 seconds';
      return false;
    }

    if (this.modalMode === 'create' && !this.accessToken) {
      this.errorMessage = 'Access Token is required';
      return false;
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
      this.createItem(this.form as CreateWhatsAppRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'WhatsApp settings created successfully';
            this.logger.info('WhatsApp settings created', undefined, 'WhatsAppSettingsManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create WhatsApp settings';
            this.logger.error('Failed to create WhatsApp settings', { message: response.message }, 'WhatsAppSettingsManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create WhatsApp settings. Please try again.';
          this.logger.error('Error creating WhatsApp settings', error, 'WhatsAppSettingsManagementComponent');
          this.loading = false;
        }
      });
    } else {
      const updateRequest: UpdateWhatsAppRequest = {
        name: this.formName,
        code: this.formCode || '',
        provider: this.provider,
        apiUrl: this.apiUrl,
        businessAccountId: this.businessAccountId,
        phoneNumberId: this.phoneNumberId,
        accessToken: this.accessToken || '',
        webhookToken: this.webhookToken,
        fromNumber: this.fromNumber,
        businessName: this.businessName,
        isActive: this.formIsActive,
        isDefault: this.isDefault,
        maxMessagesPerHour: this.maxMessagesPerHour,
        timeoutSeconds: this.timeoutSeconds,
        mediaStorageType: this.mediaStorageType,
        mediaStoragePath: this.mediaStoragePath,
        mediaStorageConfig: this.mediaStorageConfig,
        mediaPublicBaseUrl: this.mediaPublicBaseUrl,
        mediaRetentionDays: this.mediaRetentionDays,
        maxMediaSizeMB: this.maxMediaSizeMB,
        additionalConfig: this.additionalConfig,
        testNotes: this.testNotes
      };

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'WhatsApp settings updated successfully';
            this.logger.info('WhatsApp settings updated', undefined, 'WhatsAppSettingsManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update WhatsApp settings';
            this.logger.error('Failed to update WhatsApp settings', { message: response.message }, 'WhatsAppSettingsManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update WhatsApp settings. Please try again.';
          this.logger.error('Error updating WhatsApp settings', error, 'WhatsAppSettingsManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create WhatsApp settings';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New WhatsApp Settings';
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

  toggleSettingsStatus(settings: WhatsAppSettings): void {
    const updateRequest: UpdateWhatsAppRequest = {
      name: settings.name,
      code: settings.code,
      provider: settings.provider,
      apiUrl: settings.apiUrl,
      businessAccountId: settings.businessAccountId,
      phoneNumberId: settings.phoneNumberId,
      accessToken: settings.accessToken,
      fromNumber: settings.fromNumber,
      businessName: settings.businessName,
      isActive: !settings.isActive,
      isDefault: settings.isDefault,
      maxMessagesPerHour: settings.maxMessagesPerHour,
      timeoutSeconds: settings.timeoutSeconds
    };

    this.whatsappService.update(settings.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          settings.isActive = !settings.isActive;
          this.successMessage = `WhatsApp settings ${settings.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('WhatsApp settings status toggled', { settingsId: settings.id, isActive: settings.isActive }, 'WhatsAppSettingsManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update settings status';
        this.logger.error('Error toggling settings status', error, 'WhatsAppSettingsManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  setAsDefault(settings: WhatsAppSettings): void {
    this.whatsappService.setAsDefault(settings.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'WhatsApp settings set as default successfully';
          this.logger.info('WhatsApp settings set as default', { settingsId: settings.id }, 'WhatsAppSettingsManagementComponent');
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to set settings as default';
        this.logger.error('Error setting settings as default', error, 'WhatsAppSettingsManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  testWhatsAppConnection(settings: WhatsAppSettings): void {
    if (!this.testPhoneNumber) {
      this.errorMessage = 'Please enter a phone number to send test WhatsApp message';
      return;
    }

    const phoneRegex = /^\+?[1-9]\d{1,14}$/;
    if (!phoneRegex.test(this.testPhoneNumber.replace(/[\s-]/g, ''))) {
      this.errorMessage = 'Please enter a valid phone number';
      return;
    }

    this.isTestingWhatsApp = true;
    this.testResult = null;
    this.errorMessage = '';

    this.whatsappService.testWhatsApp(settings.id, this.testPhoneNumber, this.testMessage).subscribe({
      next: (response) => {
        this.isTestingWhatsApp = false;
        if (response.isSuccess) {
          this.testResult = { success: true, message: 'Test WhatsApp message sent successfully!' };
          this.logger.info('Test WhatsApp sent', { settingsId: settings.id, testPhone: this.testPhoneNumber }, 'WhatsAppSettingsManagementComponent');
          this.saveTestPhoneToLocalStorage();
        } else {
          this.testResult = { success: false, message: response.message || 'Failed to send test WhatsApp message' };
          this.logger.error('Failed to send test WhatsApp', { message: response.message }, 'WhatsAppSettingsManagementComponent');
        }
      },
      error: (error) => {
        this.isTestingWhatsApp = false;
        this.testResult = { success: false, message: error.error?.message || 'Failed to send test WhatsApp message' };
        this.logger.error('Error sending test WhatsApp', error, 'WhatsAppSettingsManagementComponent');
      }
    });
  }

  private loadTestPhoneFromLocalStorage(): void {
    const savedPhone = localStorage.getItem('test-whatsapp-number');
    if (savedPhone) {
      this.testPhoneNumber = savedPhone;
    }
  }

  private saveTestPhoneToLocalStorage(): void {
    localStorage.setItem('test-whatsapp-number', this.testPhoneNumber);
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

  onProviderChange(): void {
    switch (this.provider) {
      case 'WhatsApp_Business_API':
        this.apiUrl = 'https://graph.facebook.com/v18.0';
        break;
      case 'Twilio_WhatsApp':
        this.apiUrl = 'https://api.twilio.com/2010-04-01';
        break;
      case 'MessageBird_WhatsApp':
        this.apiUrl = 'https://conversations.messagebird.com/v1';
        break;
      default:
        break;
    }
  }
}
