import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TemplateService } from '../../../services/template.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import {
  CommunicationTemplate,
  CreateCommunicationTemplateRequest,
  UpdateCommunicationTemplateRequest,
  CommunicationChannel,
  getCommunicationChannelLabel
} from '../../../models/communication.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-template-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './template-management.component.html',
  styleUrls: ['./template-management.component.scss']
})
export class TemplateManagementComponent
  extends BaseMasterManagementComponent<CommunicationTemplate, CreateCommunicationTemplateRequest, UpdateCommunicationTemplateRequest>
  implements OnInit {

  // Form model
  form!: (CreateCommunicationTemplateRequest & { id?: string }) | (UpdateCommunicationTemplateRequest & { description?: string });

  // Communication channels enum for template
  CommunicationChannel = CommunicationChannel;

  // Available placeholders
  availablePlaceholders: string[] = [];

  // Preview state
  showPreview = false;
  previewContent = '';

  // Filter state
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  channelFilter: 'all' | CommunicationChannel = 'all';
  systemFilter: 'all' | 'system' | 'custom' = 'all';

  protected override get entityName(): string {
    return 'Communication Template';
  }

  // Getters/setters for template-specific fields
  get channel(): CommunicationChannel {
    return (this.form as any).channel ?? CommunicationChannel.Email;
  }

  set channel(value: CommunicationChannel) {
    (this.form as any).channel = value;
  }

  get subject(): string {
    return (this.form as any).subject || '';
  }

  set subject(value: string) {
    (this.form as any).subject = value;
  }

  get body(): string {
    return (this.form as any).body || '';
  }

  set body(value: string) {
    (this.form as any).body = value;
  }

  get htmlBody(): string {
    return (this.form as any).htmlBody || '';
  }

  set htmlBody(value: string) {
    (this.form as any).htmlBody = value;
  }

  get isSystem(): boolean {
    return (this.form as any).isSystem || false;
  }

  constructor(
    private templateService: TemplateService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.availablePlaceholders = this.templateService.getAvailablePlaceholders();
  }

  protected override getEmptyForm(): CreateCommunicationTemplateRequest {
    return {
      name: '',
      code: '',
      description: '',
      channel: CommunicationChannel.Email,
      subject: '',
      body: '',
      htmlBody: '',
      isActive: true
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.templateService.getTemplates(true).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load templates. Please try again.';
        this.loading = false;
        this.logger.error('Error loading templates', error, 'TemplateManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(t => t.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(t => !t.isActive);
    }

    // Filter by channel
    if (this.channelFilter !== 'all') {
      filtered = filtered.filter(t => t.channel === this.channelFilter);
    }

    // Filter by system/custom
    if (this.systemFilter === 'system') {
      filtered = filtered.filter(t => t.isSystem);
    } else if (this.systemFilter === 'custom') {
      filtered = filtered.filter(t => !t.isSystem);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(t =>
        t.name.toLowerCase().includes(search) ||
        t.code.toLowerCase().includes(search) ||
        (t.description && t.description.toLowerCase().includes(search)) ||
        t.body.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateCommunicationTemplateRequest): Observable<ApiResponse<CommunicationTemplate>> {
    return this.templateService.createTemplate(request);
  }

  protected override updateItem(id: string, request: UpdateCommunicationTemplateRequest): Observable<ApiResponse<CommunicationTemplate>> {
    return this.templateService.updateTemplate(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.templateService.deleteTemplate(id);
  }

  override openEditModal(item: CommunicationTemplate): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit templates';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Communication Template';

    this.form = {
      id: item.id,
      name: item.name,
      code: item.code,
      description: item.description,
      channel: item.channel,
      subject: item.subject,
      body: item.body,
      htmlBody: item.htmlBody,
      isActive: item.isActive,
      category: item.category,
      language: item.language,
      availablePlaceholders: item.availablePlaceholders
    };

    this.showModal = true;
    this.errorMessage = '';
    this.showPreview = false;
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate code format (uppercase with underscores)
    const codeRegex = /^[A-Z_]+$/;
    if (!codeRegex.test(this.formCode)) {
      this.errorMessage = 'Template code must contain only uppercase letters and underscores (e.g., COMPLAINT_CREATED)';
      return false;
    }

    // Validate subject for email templates
    if (this.channel === CommunicationChannel.Email && !this.subject) {
      this.errorMessage = 'Subject is required for email templates';
      return false;
    }

    // Validate body is not empty
    if (!this.body || this.body.trim() === '') {
      this.errorMessage = 'Template body is required';
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
      this.createItem(this.form as CreateCommunicationTemplateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Template created successfully';
            this.logger.info('Template created', undefined, 'TemplateManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create template';
            this.logger.error('Failed to create template', { message: response.message }, 'TemplateManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create template. Please try again.';
          this.logger.error('Error creating template', error, 'TemplateManagementComponent');
          this.loading = false;
        }
      });
    } else {
      const updateRequest: UpdateCommunicationTemplateRequest = {
        id: this.formId,
        name: this.formName,
        code: this.formCode,
        description: this.formDescription,
        channel: this.channel,
        subject: this.subject,
        body: this.body,
        htmlBody: this.htmlBody,
        isActive: this.formIsActive,
        category: (this.form as UpdateCommunicationTemplateRequest).category,
        language: (this.form as UpdateCommunicationTemplateRequest).language,
        availablePlaceholders: (this.form as UpdateCommunicationTemplateRequest).availablePlaceholders
      };

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Template updated successfully';
            this.logger.info('Template updated', undefined, 'TemplateManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update template';
            this.logger.error('Failed to update template', { message: response.message }, 'TemplateManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update template. Please try again.';
          this.logger.error('Error updating template', error, 'TemplateManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  override openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create templates';
      return;
    }
    super.openCreateModal();
    this.modalTitle = 'Create New Communication Template';
    this.showPreview = false;
  }

  override closeModal(): void {
    super.closeModal();
    this.showPreview = false;
    this.previewContent = '';
  }

  override confirmDelete(item: CommunicationTemplate): void {
    // Check if system template
    if (item.isSystem) {
      this.errorMessage = 'System templates cannot be deleted';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete templates';
      return;
    }

    super.confirmDelete(item);
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.statusFilter = filter;
    this.filterItems();
  }

  setChannelFilter(channel: 'all' | CommunicationChannel): void {
    this.channelFilter = channel;
    this.filterItems();
  }

  setSystemFilter(filter: 'all' | 'system' | 'custom'): void {
    this.systemFilter = filter;
    this.filterItems();
  }

  toggleTemplateStatus(template: CommunicationTemplate): void {
    const updateRequest: UpdateCommunicationTemplateRequest = {
      id: template.id,
      name: template.name,
      code: template.code,
      description: template.description,
      channel: template.channel,
      subject: template.subject,
      body: template.body,
      htmlBody: template.htmlBody,
      isActive: !template.isActive
    };

    this.templateService.updateTemplate(template.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          template.isActive = !template.isActive;
          this.successMessage = `Template ${template.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Template status toggled', { templateId: template.id, isActive: template.isActive }, 'TemplateManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update template status';
        this.logger.error('Error toggling template status', error, 'TemplateManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  insertPlaceholder(placeholder: string): void {
    // Insert placeholder at cursor position in body field
    const bodyTextarea = document.getElementById('templateBody') as HTMLTextAreaElement;
    if (bodyTextarea) {
      const startPos = bodyTextarea.selectionStart;
      const endPos = bodyTextarea.selectionEnd;
      this.body = this.body.substring(0, startPos) + placeholder + this.body.substring(endPos);

      // Set cursor position after inserted placeholder
      setTimeout(() => {
        bodyTextarea.focus();
        bodyTextarea.setSelectionRange(startPos + placeholder.length, startPos + placeholder.length);
      }, 0);
    } else {
      // Append to end if textarea not found
      this.body += placeholder;
    }
  }

  togglePreview(): void {
    if (!this.showPreview) {
      // Generate preview with sample data
      const sampleData: Record<string, string> = {
        complaintId: '550e8400-e29b-41d4-a716-446655440000',
        complaintNumber: 'CMP-2025-0001',
        title: 'Sample Complaint Title',
        description: 'This is a sample complaint description',
        categoryName: 'IT Support',
        priorityName: 'High',
        statusName: 'In Progress',
        complainantName: 'John Doe',
        complainantEmail: 'john.doe@example.com',
        complainantEmployeeCode: 'EMP001',
        assignedToName: 'Jane Smith',
        assignedToEmail: 'jane.smith@example.com',
        assignedToPhone: '+971-50-123-4567',
        escalationLevel: '1',
        escalationReason: 'SLA breach',
        closedBy: 'Admin User',
        resolution: 'Issue resolved successfully',
        createdDate: new Date().toLocaleDateString(),
        dueDate: new Date(Date.now() + 86400000).toLocaleDateString(),
        closedDate: new Date().toLocaleDateString(),
        companyName: 'Acme Corporation',
        branchName: 'Dubai Office',
        departmentName: 'IT Department',
        sectionName: 'Helpdesk'
      };

      this.previewContent = this.templateService.previewTemplate(this.body, sampleData);
      this.showPreview = true;
    } else {
      this.showPreview = false;
    }
  }

  getChannelLabel(channel: CommunicationChannel): string {
    return getCommunicationChannelLabel(channel);
  }

  getActiveTemplatesCount(): number {
    return this.items.filter(t => t.isActive).length;
  }

  getInactiveTemplatesCount(): number {
    return this.items.filter(t => !t.isActive).length;
  }

  getSystemTemplatesCount(): number {
    return this.items.filter(t => t.isSystem).length;
  }

  getCustomTemplatesCount(): number {
    return this.items.filter(t => !t.isSystem).length;
  }

  navigateBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
