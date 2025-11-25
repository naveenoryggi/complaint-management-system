import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  SmsGatewayService,
  SmsGatewaySettings,
  CreateSmsGatewayRequest,
  UpdateSmsGatewayRequest
} from '../../../services/sms-gateway.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-sms-gateway-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './sms-gateway-management.component.html',
  styleUrls: ['./sms-gateway-management.component.scss']
})
export class SmsGatewayManagementComponent
  extends BaseMasterManagementComponent<SmsGatewaySettings, CreateSmsGatewayRequest, UpdateSmsGatewayRequest>
  implements OnInit {

  // Form model
  form!: (CreateSmsGatewayRequest & { id?: string }) | (UpdateSmsGatewayRequest & { code?: string });

  // Test SMS state
  isTestingSms = false;
  testPhoneNumber = '';
  testMessage = 'This is a test message from Complaint Management System.';
  testResult: { success: boolean; message: string } | null = null;

  // Popular providers
  providers = [
    { value: 'Twilio', label: 'Twilio' },
    { value: 'AWS_SNS', label: 'AWS SNS' },
    { value: 'MessageBird', label: 'MessageBird' },
    { value: 'Nexmo', label: 'Nexmo (Vonage)' },
    { value: 'Plivo', label: 'Plivo' },
    { value: 'Custom', label: 'Custom Provider' }
  ];

  protected override get entityName(): string {
    return 'SMS Gateway Settings';
  }

  // Getters/setters for SMS-specific fields
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

  get accountSid(): string {
    return (this.form as any).accountSid || '';
  }

  set accountSid(value: string) {
    (this.form as any).accountSid = value;
  }

  get authToken(): string {
    return (this.form as any).authToken || '';
  }

  set authToken(value: string) {
    (this.form as any).authToken = value;
  }

  get fromNumber(): string {
    return (this.form as any).fromNumber || '';
  }

  set fromNumber(value: string) {
    (this.form as any).fromNumber = value;
  }

  get senderName(): string {
    return (this.form as any).senderName || '';
  }

  set senderName(value: string) {
    (this.form as any).senderName = value;
  }

  get isDefault(): boolean {
    return (this.form as any).isDefault !== undefined ? (this.form as any).isDefault : false;
  }

  set isDefault(value: boolean) {
    (this.form as any).isDefault = value;
  }

  get maxSmsPerHour(): number | undefined {
    return (this.form as any).maxSmsPerHour;
  }

  set maxSmsPerHour(value: number | undefined) {
    (this.form as any).maxSmsPerHour = value;
  }

  get costPerSms(): number | undefined {
    return (this.form as any).costPerSms;
  }

  set costPerSms(value: number | undefined) {
    (this.form as any).costPerSms = value;
  }

  get timeoutSeconds(): number {
    return (this.form as any).timeoutSeconds || 30;
  }

  set timeoutSeconds(value: number) {
    (this.form as any).timeoutSeconds = value;
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
    private smsGatewayService: SmsGatewayService,
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

  protected override getEmptyForm(): CreateSmsGatewayRequest {
    return {
      name: '',
      provider: '',
      apiUrl: '',
      accountSid: '',
      authToken: '',
      fromNumber: '',
      senderName: '',
      isActive: true,
      isDefault: false,
      timeoutSeconds: 30,
      code: ''
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.smsGatewayService.getAll(true).subscribe({
      next: (response) => {
        this.items = response.data || [];
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load SMS gateway settings. Please try again.';
        this.loading = false;
        this.logger.error('Error loading SMS gateway settings', error, 'SmsGatewayManagementComponent');
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
        s.provider.toLowerCase().includes(search) ||
        s.fromNumber.toLowerCase().includes(search) ||
        s.senderName.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateSmsGatewayRequest): Observable<ApiResponse<SmsGatewaySettings>> {
    return this.smsGatewayService.create(request);
  }

  protected override updateItem(id: string, request: UpdateSmsGatewayRequest): Observable<ApiResponse<SmsGatewaySettings>> {
    return this.smsGatewayService.update(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.smsGatewayService.delete(id);
  }

  override openEditModal(item: SmsGatewaySettings): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit SMS gateway settings';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit SMS Gateway Settings';

    this.form = {
      id: item.id,
      name: item.name,
      code: item.code ||  '',
      provider: item.provider,
      apiUrl: item.apiUrl,
      accountSid: item.accountSid,
      authToken: '', // Don't show existing token
      fromNumber: item.fromNumber,
      senderName: item.senderName,
      isActive: item.isActive,
      isDefault: item.isDefault,
      maxSmsPerHour: item.maxSmsPerHour,
      costPerSms: item.costPerSms,
      timeoutSeconds: item.timeoutSeconds,
      additionalConfig: item.additionalConfig,
      testNotes: item.testNotes
    };

    this.showModal = true;
    this.errorMessage = '';
    this.testResult = null;
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate provider
    if (!this.provider || this.provider.trim() === '') {
      this.errorMessage = 'Please select or enter a provider';
      return false;
    }

    // Validate API URL
    if (!this.apiUrl || this.apiUrl.trim() === '') {
      this.errorMessage = 'API URL is required';
      return false;
    }

    // Validate URL format
    try {
      new URL(this.apiUrl);
    } catch {
      this.errorMessage = 'Please enter a valid API URL';
      return false;
    }

    // Validate phone number format (basic international format check)
    const phoneRegex = /^\+?[1-9]\d{1,14}$/;
    if (!phoneRegex.test(this.fromNumber.replace(/[\s-]/g, ''))) {
      this.errorMessage = 'Please enter a valid phone number (international format recommended, e.g., +1234567890)';
      return false;
    }

    // Validate timeout
    if (this.timeoutSeconds < 10 || this.timeoutSeconds > 300) {
      this.errorMessage = 'Timeout must be between 10 and 300 seconds';
      return false;
    }

    // Validate auth token for new entries
    if (this.modalMode === 'create' && !this.authToken) {
      this.errorMessage = 'Auth Token is required';
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
      this.createItem(this.form as CreateSmsGatewayRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'SMS gateway settings created successfully';
            this.logger.info('SMS gateway created', undefined, 'SmsGatewayManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create SMS gateway settings';
            this.logger.error('Failed to create SMS gateway', { message: response.message }, 'SmsGatewayManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create SMS gateway settings. Please try again.';
          this.logger.error('Error creating SMS gateway', error, 'SmsGatewayManagementComponent');
          this.loading = false;
        }
      });
    } else {
      const updateRequest: UpdateSmsGatewayRequest = {
        name: this.formName,
        code: this.formCode || '',
        provider: this.provider,
        apiUrl: this.apiUrl,
        accountSid: this.accountSid,
        authToken: this.authToken || '',
        fromNumber: this.fromNumber,
        senderName: this.senderName,
        isActive: this.formIsActive,
        isDefault: this.isDefault,
        maxSmsPerHour: this.maxSmsPerHour,
        costPerSms: this.costPerSms,
        timeoutSeconds: this.timeoutSeconds,
        additionalConfig: this.additionalConfig,
        testNotes: this.testNotes
      };

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'SMS gateway settings updated successfully';
            this.logger.info('SMS gateway updated', undefined, 'SmsGatewayManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update SMS gateway settings';
            this.logger.error('Failed to update SMS gateway', { message: response.message }, 'SmsGatewayManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update SMS gateway settings. Please try again.';
          this.logger.error('Error updating SMS gateway', error, 'SmsGatewayManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create SMS gateway settings';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New SMS Gateway Settings';
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

  toggleSettingsStatus(settings: SmsGatewaySettings): void {
    const updateRequest: UpdateSmsGatewayRequest = {
      name: settings.name,
      code: settings.code,
      provider: settings.provider,
      apiUrl: settings.apiUrl,
      accountSid: settings.accountSid,
      authToken: settings.authToken,
      fromNumber: settings.fromNumber,
      senderName: settings.senderName,
      isActive: !settings.isActive,
      isDefault: settings.isDefault,
      maxSmsPerHour: settings.maxSmsPerHour,
      costPerSms: settings.costPerSms,
      timeoutSeconds: settings.timeoutSeconds,
      additionalConfig: settings.additionalConfig,
      testNotes: settings.testNotes
    };

    this.smsGatewayService.update(settings.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          settings.isActive = !settings.isActive;
          this.successMessage = `SMS gateway ${settings.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('SMS gateway status toggled', { gatewayId: settings.id, isActive: settings.isActive }, 'SmsGatewayManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update gateway status';
        this.logger.error('Error toggling gateway status', error, 'SmsGatewayManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  setAsDefault(settings: SmsGatewaySettings): void {
    this.smsGatewayService.setAsDefault(settings.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'SMS gateway set as default successfully';
          this.logger.info('SMS gateway set as default', { gatewayId: settings.id }, 'SmsGatewayManagementComponent');
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to set gateway as default';
        this.logger.error('Error setting gateway as default', error, 'SmsGatewayManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  testSmsConnection(settings: SmsGatewaySettings): void {
    if (!this.testPhoneNumber) {
      this.errorMessage = 'Please enter a phone number to send test SMS';
      return;
    }

    const phoneRegex = /^\+?[1-9]\d{1,14}$/;
    if (!phoneRegex.test(this.testPhoneNumber.replace(/[\s-]/g, ''))) {
      this.errorMessage = 'Please enter a valid phone number';
      return;
    }

    this.isTestingSms = true;
    this.testResult = null;
    this.errorMessage = '';

    this.smsGatewayService.testSms(settings.id, this.testPhoneNumber, this.testMessage).subscribe({
      next: (response) => {
        this.isTestingSms = false;
        if (response.isSuccess) {
          this.testResult = { success: true, message: 'Test SMS sent successfully!' };
          this.logger.info('Test SMS sent', { gatewayId: settings.id, testPhone: this.testPhoneNumber }, 'SmsGatewayManagementComponent');
          this.saveTestPhoneToLocalStorage();
        } else {
          this.testResult = { success: false, message: response.message || 'Failed to send test SMS' };
          this.logger.error('Failed to send test SMS', { message: response.message }, 'SmsGatewayManagementComponent');
        }
      },
      error: (error) => {
        this.isTestingSms = false;
        this.testResult = { success: false, message: error.error?.message || 'Failed to send test SMS' };
        this.logger.error('Error sending test SMS', error, 'SmsGatewayManagementComponent');
      }
    });
  }

  testSmsConnectionFromModal(): void {
    if (this.modalMode === 'edit' && this.formId) {
      const settingsToTest: SmsGatewaySettings = {
        id: this.formId,
        name: this.formName,
        code: this.formCode || '',
        provider: this.provider,
        apiUrl: this.apiUrl,
        accountSid: this.accountSid,
        authToken: this.authToken,
        fromNumber: this.fromNumber,
        senderName: this.senderName,
        isActive: this.formIsActive,
        isDefault: this.isDefault,
        timeoutSeconds: this.timeoutSeconds,
        maxSmsPerHour: this.maxSmsPerHour,
        costPerSms: this.costPerSms,
        companyId: '',
        createdAt: new Date(),
        createdBy: '',
        isDeleted: false
      };
      this.testSmsConnection(settingsToTest);
    }
  }

  private loadTestPhoneFromLocalStorage(): void {
    const savedPhone = localStorage.getItem('test-phone-number');
    if (savedPhone) {
      this.testPhoneNumber = savedPhone;
    }
  }

  private saveTestPhoneToLocalStorage(): void {
    localStorage.setItem('test-phone-number', this.testPhoneNumber);
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
    // Auto-fill API URL based on provider selection
    switch (this.provider) {
      case 'Twilio':
        this.apiUrl = 'https://api.twilio.com/2010-04-01';
        break;
      case 'AWS_SNS':
        this.apiUrl = 'https://sns.amazonaws.com';
        break;
      case 'MessageBird':
        this.apiUrl = 'https://rest.messagebird.com';
        break;
      case 'Nexmo':
        this.apiUrl = 'https://rest.nexmo.com';
        break;
      case 'Plivo':
        this.apiUrl = 'https://api.plivo.com/v1';
        break;
      default:
        // Keep existing URL or empty
        break;
    }
  }
}
