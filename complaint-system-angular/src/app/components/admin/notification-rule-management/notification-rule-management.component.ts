import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationRuleService } from '../../../services/notification-rule.service';
import { TemplateService } from '../../../services/template.service';
import { RoleService } from '../../../services/role.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import {
  NotificationRule,
  CreateNotificationRuleRequest,
  UpdateNotificationRuleRequest,
  EventType,
  CommunicationTemplate,
  CommunicationChannel,
  RecipientType,
  getCommunicationChannelLabel,
  getRecipientTypeLabel
} from '../../../models/communication.model';
import { Role } from '../../../models/role.model';

interface RuleForm {
  id?: string;
  name: string;
  description: string;
  eventTypeId: string;
  channel: CommunicationChannel;
  templateId: string;
  recipientType: RecipientType;
  specificUserIds: string[];
  specificRoleIds: string[];
  specificEmails: string[];
  conditions: string;
  isActive: boolean;
  priority: number;
  delayMinutes: number;
  sendOnlyOnce: boolean;
}

@Component({
  selector: 'app-notification-rule-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-rule-management.component.html',
  styleUrls: ['./notification-rule-management.component.scss']
})
export class NotificationRuleManagementComponent implements OnInit {
  // Data lists
  rules: NotificationRule[] = [];
  filteredRules: NotificationRule[] = [];
  eventTypes: EventType[] = [];
  templates: CommunicationTemplate[] = [];
  availableTemplates: CommunicationTemplate[] = [];
  roles: Role[] = [];

  // UI state
  loading = false;
  showModal = false;
  showDeleteModal = false;
  modalMode: 'create' | 'edit' = 'create';
  modalTitle = '';
  errorMessage = '';
  successMessage = '';

  // Form
  form: RuleForm = this.getEmptyForm();

  // Filters
  searchTerm = '';
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  eventFilter = 'all';
  channelFilter: 'all' | CommunicationChannel = 'all';
  recipientFilter: 'all' | RecipientType = 'all';

  // Item to delete
  itemToDelete: NotificationRule | null = null;

  // Enums for template
  CommunicationChannel = CommunicationChannel;
  RecipientType = RecipientType;

  // Permissions
  canManageSettings = false;

  constructor(
    private notificationRuleService: NotificationRuleService,
    private templateService: TemplateService,
    private roleService: RoleService,
    private authService: AuthService,
    private logger: LoggerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.checkPermissions();
    this.loadData();
  }

  private checkPermissions(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.permissions) {
      this.canManageSettings = currentUser.permissions.includes('ManageSettings');
    }
  }

  private loadData(): void {
    this.loading = true;
    this.errorMessage = '';

    // Load all required data in parallel
    Promise.all([
      this.notificationRuleService.getNotificationRules(true).toPromise(),
      this.notificationRuleService.getEventTypes(true).toPromise(),
      this.templateService.getTemplates(true).toPromise(),
      this.roleService.getAllRoles().toPromise()
    ]).then(([rules, eventTypes, templates, rolesResponse]) => {
      this.rules = rules || [];
      this.eventTypes = eventTypes || [];
      this.templates = templates || [];
      this.roles = rolesResponse?.data || [];
      this.filterRules();
      this.loading = false;
    }).catch(error => {
      this.errorMessage = 'Failed to load notification rules. Please try again.';
      this.loading = false;
      this.logger.error('Error loading notification rules data', error, 'NotificationRuleManagementComponent');
    });
  }

  private getEmptyForm(): RuleForm {
    return {
      name: '',
      description: '',
      eventTypeId: '',
      channel: CommunicationChannel.Email,
      templateId: '',
      recipientType: RecipientType.Complainant,
      specificUserIds: [],
      specificRoleIds: [],
      specificEmails: [],
      conditions: '',
      isActive: true,
      priority: 1,
      delayMinutes: 0,
      sendOnlyOnce: false
    };
  }

  filterRules(): void {
    let filtered = this.rules;

    // Filter by status
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(r => r.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(r => !r.isActive);
    }

    // Filter by event type
    if (this.eventFilter !== 'all') {
      filtered = filtered.filter(r => r.eventTypeId === this.eventFilter);
    }

    // Filter by channel
    if (this.channelFilter !== 'all') {
      filtered = filtered.filter(r => r.channel === this.channelFilter);
    }

    // Filter by recipient type
    if (this.recipientFilter !== 'all') {
      filtered = filtered.filter(r => r.recipientType === this.recipientFilter);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(r =>
        r.name.toLowerCase().includes(search) ||
        (r.description && r.description.toLowerCase().includes(search))
      );
    }

    this.filteredRules = filtered;
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.statusFilter = filter;
    this.filterRules();
  }

  setEventFilter(eventTypeId: string): void {
    this.eventFilter = eventTypeId;
    this.filterRules();
  }

  setChannelFilter(channel: 'all' | CommunicationChannel): void {
    this.channelFilter = channel;
    this.filterRules();
  }

  setRecipientFilter(recipientType: 'all' | RecipientType): void {
    this.recipientFilter = recipientType;
    this.filterRules();
  }

  onChannelChange(): void {
    // Filter templates by selected channel
    this.availableTemplates = this.templates.filter(t => t.channel === this.form.channel && t.isActive);

    // Reset template selection if current template doesn't match channel
    const currentTemplate = this.templates.find(t => t.id === this.form.templateId);
    if (!currentTemplate || currentTemplate.channel !== this.form.channel) {
      this.form.templateId = '';
    }
  }

  onRecipientTypeChange(): void {
    // Reset specific fields when recipient type changes
    this.form.specificUserIds = [];
    this.form.specificRoleIds = [];
    this.form.specificEmails = [];
  }

  openCreateModal(): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create notification rules';
      return;
    }

    this.modalMode = 'create';
    this.modalTitle = 'Create New Notification Rule';
    this.form = this.getEmptyForm();
    this.onChannelChange();
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(rule: NotificationRule): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit notification rules';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Notification Rule';

    this.form = {
      id: rule.id,
      name: rule.name,
      description: rule.description || '',
      eventTypeId: rule.eventTypeId,
      channel: rule.channel,
      templateId: rule.templateId,
      recipientType: rule.recipientType,
      specificUserIds: rule.specificUserIds || [],
      specificRoleIds: rule.specificRoleIds || [],
      specificEmails: rule.specificEmails || [],
      conditions: rule.conditions || '',
      isActive: rule.isActive,
      priority: rule.priority,
      delayMinutes: rule.delayMinutes,
      sendOnlyOnce: rule.sendOnlyOnce
    };

    this.onChannelChange();
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.form = this.getEmptyForm();
    this.errorMessage = '';
  }

  validateForm(): boolean {
    if (!this.form.name || this.form.name.trim() === '') {
      this.errorMessage = 'Rule name is required';
      return false;
    }

    if (!this.form.eventTypeId) {
      this.errorMessage = 'Event type is required';
      return false;
    }

    if (!this.form.templateId) {
      this.errorMessage = 'Template is required';
      return false;
    }

    if (this.form.priority < 1 || this.form.priority > 100) {
      this.errorMessage = 'Priority must be between 1 and 100';
      return false;
    }

    if (this.form.delayMinutes < 0 || this.form.delayMinutes > 1440) {
      this.errorMessage = 'Delay must be between 0 and 1440 minutes (24 hours)';
      return false;
    }

    // Validate specific recipient fields
    if (this.form.recipientType === RecipientType.SpecificUsers && this.form.specificUserIds.length === 0) {
      this.errorMessage = 'At least one user must be selected for Specific Users recipient type';
      return false;
    }

    if (this.form.recipientType === RecipientType.SpecificRoles && this.form.specificRoleIds.length === 0) {
      this.errorMessage = 'At least one role must be selected for Specific Roles recipient type';
      return false;
    }

    if (this.form.recipientType === RecipientType.SpecificEmails && this.form.specificEmails.length === 0) {
      this.errorMessage = 'At least one email must be provided for Specific Emails recipient type';
      return false;
    }

    // Validate conditions JSON if provided
    if (this.form.conditions && this.form.conditions.trim() !== '') {
      try {
        JSON.parse(this.form.conditions);
      } catch (e) {
        this.errorMessage = 'Conditions must be valid JSON';
        return false;
      }
    }

    return true;
  }

  save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      const request: CreateNotificationRuleRequest = {
        name: this.form.name,
        description: this.form.description || undefined,
        eventTypeId: this.form.eventTypeId,
        channel: this.form.channel,
        templateId: this.form.templateId,
        recipientType: this.form.recipientType,
        specificUserIds: this.form.specificUserIds.length > 0 ? this.form.specificUserIds : undefined,
        specificRoleIds: this.form.specificRoleIds.length > 0 ? this.form.specificRoleIds : undefined,
        specificEmails: this.form.specificEmails.length > 0 ? this.form.specificEmails : undefined,
        conditions: this.form.conditions || undefined,
        isActive: this.form.isActive,
        priority: this.form.priority,
        delayMinutes: this.form.delayMinutes,
        sendOnlyOnce: this.form.sendOnlyOnce
      };

      this.notificationRuleService.createNotificationRule(request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Notification rule created successfully';
            this.logger.info('Notification rule created', undefined, 'NotificationRuleManagementComponent');
            this.closeModal();
            this.loadData();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create notification rule';
            this.logger.error('Failed to create notification rule', { message: response.message }, 'NotificationRuleManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create notification rule. Please try again.';
          this.logger.error('Error creating notification rule', error, 'NotificationRuleManagementComponent');
          this.loading = false;
        }
      });
    } else {
      const request: UpdateNotificationRuleRequest = {
        id: this.form.id!,
        name: this.form.name,
        description: this.form.description || undefined,
        eventTypeId: this.form.eventTypeId,
        channel: this.form.channel,
        templateId: this.form.templateId,
        recipientType: this.form.recipientType,
        specificUserIds: this.form.specificUserIds.length > 0 ? this.form.specificUserIds : undefined,
        specificRoleIds: this.form.specificRoleIds.length > 0 ? this.form.specificRoleIds : undefined,
        specificEmails: this.form.specificEmails.length > 0 ? this.form.specificEmails : undefined,
        conditions: this.form.conditions || undefined,
        isActive: this.form.isActive,
        priority: this.form.priority,
        delayMinutes: this.form.delayMinutes,
        sendOnlyOnce: this.form.sendOnlyOnce
      };

      this.notificationRuleService.updateNotificationRule(this.form.id!, request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Notification rule updated successfully';
            this.logger.info('Notification rule updated', undefined, 'NotificationRuleManagementComponent');
            this.closeModal();
            this.loadData();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update notification rule';
            this.logger.error('Failed to update notification rule', { message: response.message }, 'NotificationRuleManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update notification rule. Please try again.';
          this.logger.error('Error updating notification rule', error, 'NotificationRuleManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  confirmDelete(rule: NotificationRule): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete notification rules';
      return;
    }

    this.itemToDelete = rule;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.itemToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.itemToDelete) return;

    this.loading = true;
    this.notificationRuleService.deleteNotificationRule(this.itemToDelete.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Notification rule deleted successfully';
          this.logger.info('Notification rule deleted', { ruleId: this.itemToDelete?.id }, 'NotificationRuleManagementComponent');
          this.cancelDelete();
          this.loadData();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || 'Failed to delete notification rule';
          this.logger.error('Failed to delete notification rule', { message: response.message }, 'NotificationRuleManagementComponent');
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete notification rule. Please try again.';
        this.logger.error('Error deleting notification rule', error, 'NotificationRuleManagementComponent');
        this.loading = false;
      }
    });
  }

  toggleRuleStatus(rule: NotificationRule): void {
    const request: UpdateNotificationRuleRequest = {
      id: rule.id,
      name: rule.name,
      description: rule.description,
      eventTypeId: rule.eventTypeId,
      channel: rule.channel,
      templateId: rule.templateId,
      recipientType: rule.recipientType,
      specificUserIds: rule.specificUserIds,
      specificRoleIds: rule.specificRoleIds,
      specificEmails: rule.specificEmails,
      conditions: rule.conditions,
      isActive: !rule.isActive,
      priority: rule.priority,
      delayMinutes: rule.delayMinutes,
      sendOnlyOnce: rule.sendOnlyOnce
    };

    this.notificationRuleService.updateNotificationRule(rule.id, request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          rule.isActive = !rule.isActive;
          this.successMessage = `Notification rule ${rule.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Notification rule status toggled', { ruleId: rule.id, isActive: rule.isActive }, 'NotificationRuleManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update rule status';
        this.logger.error('Error toggling rule status', error, 'NotificationRuleManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  // Helper methods
  getEventTypeName(eventTypeId: string): string {
    const eventType = this.eventTypes.find(e => e.id === eventTypeId);
    return eventType ? eventType.name : 'Unknown';
  }

  getTemplateName(templateId: string): string {
    const template = this.templates.find(t => t.id === templateId);
    return template ? template.name : 'Unknown';
  }

  getChannelLabel(channel: CommunicationChannel): string {
    return getCommunicationChannelLabel(channel);
  }

  getRecipientTypeLabel(recipientType: RecipientType): string {
    return getRecipientTypeLabel(recipientType);
  }

  getRoleName(roleId: string): string {
    const role = this.roles.find(r => r.id === roleId);
    return role ? role.name : roleId;
  }

  getRoleNames(roleIds: string[]): string {
    return roleIds.map(id => this.getRoleName(id)).join(', ');
  }

  // Helper method to safely format specificEmails (handles both string and array)
  formatSpecificEmails(emails: string[] | string | undefined | null): string {
    if (!emails) return '';
    if (Array.isArray(emails)) {
      return emails.join(', ');
    }
    // If it's a string, return as-is
    return String(emails);
  }

  // Helper method to check if specificEmails has content
  hasSpecificEmails(emails: string[] | string | undefined | null): boolean {
    if (!emails) return false;
    if (Array.isArray(emails)) {
      return emails.length > 0;
    }
    // If it's a string, check if it's not empty
    return String(emails).trim().length > 0;
  }

  getActiveRulesCount(): number {
    return this.rules.filter(r => r.isActive).length;
  }

  getInactiveRulesCount(): number {
    return this.rules.filter(r => !r.isActive).length;
  }

  // TrackBy function for *ngFor optimization
  trackByRuleId(index: number, rule: NotificationRule): string {
    return rule.id;
  }

  // Get priority badge class
  getPriorityClass(priority: number): string {
    if (priority <= 10) return 'priority-high';
    if (priority <= 50) return 'priority-medium';
    return 'priority-low';
  }

  // Get channel icon class
  getChannelIcon(channel: CommunicationChannel): string {
    switch (channel) {
      case CommunicationChannel.Email:
        return 'fas fa-envelope channel-email';
      case CommunicationChannel.SMS:
        return 'fas fa-sms channel-sms';
      case CommunicationChannel.WhatsApp:
        return 'fab fa-whatsapp channel-whatsapp';
      default:
        return 'fas fa-bell';
    }
  }

  addSpecificEmail(): void {
    const email = prompt('Enter email address:');
    if (email && email.trim() !== '') {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (emailRegex.test(email)) {
        if (!this.form.specificEmails.includes(email)) {
          this.form.specificEmails.push(email);
        }
      } else {
        alert('Please enter a valid email address');
      }
    }
  }

  removeSpecificEmail(email: string): void {
    const index = this.form.specificEmails.indexOf(email);
    if (index > -1) {
      this.form.specificEmails.splice(index, 1);
    }
  }

  navigateBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
