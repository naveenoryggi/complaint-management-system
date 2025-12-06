import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import DOMPurify from 'dompurify';
import { EmailThreadService, EmailThreadItemDto } from '../../../services/email-thread.service';
import { LoggerService } from '../../../core/services/logger.service';
import { DateService } from '../../../services/date.service';
import {
  EmailAttachment,
  EmailRecipient,
  formatFileSize
} from '../../../models/communication.model';

/**
 * EmailThreadViewerComponent
 *
 * A comprehensive Angular component for displaying email conversations in complaints.
 * Features:
 * - Display email thread with sender, receiver, CC, BCC, timestamp
 * - Safe HTML rendering with DOMPurify sanitization
 * - Attachment display with download links
 * - Expand/collapse individual messages
 * - Direction highlighting (inbound vs outbound)
 * - Reply and Forward buttons with EventEmitters
 * - Chronological ordering (newest/oldest first toggle)
 * - OnPush change detection for optimal performance
 * - Strict TypeScript typing (no 'any' types)
 * - Proper memory management with takeUntil
 *
 * @example
 * <app-email-thread-viewer
 *   [complaintId]="complaint.id"
 *   [showActions]="true"
 *   [sortOrder]="'newest-first'"
 *   (replyClicked)="handleReply($event)"
 *   (forwardClicked)="handleForward($event)">
 * </app-email-thread-viewer>
 */
@Component({
  selector: 'app-email-thread-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './email-thread-viewer.component.html',
  styleUrls: ['./email-thread-viewer.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailThreadViewerComponent implements OnInit, OnDestroy {
  // ==================== INPUTS ====================

  @Input() complaintId!: string;
  @Input() showActions: boolean = true; // Show reply/forward buttons
  @Input() sortOrder: 'newest-first' | 'oldest-first' = 'newest-first'; // Chronological ordering
  @Input() autoRefresh: boolean = false; // Auto-refresh email thread
  @Input() refreshInterval: number = 30000; // Auto-refresh interval in ms (30 seconds)
  @Input() maxHeight: string = '600px'; // Maximum height for scrollable area

  // ==================== OUTPUTS ====================

  @Output() replyClicked = new EventEmitter<EmailThreadItemDto>();
  @Output() replyAllClicked = new EventEmitter<EmailThreadItemDto>();
  @Output() forwardClicked = new EventEmitter<EmailThreadItemDto>();
  @Output() privateNoteClicked = new EventEmitter<EmailThreadItemDto>();
  @Output() composeNewClicked = new EventEmitter<void>();
  @Output() attachmentClicked = new EventEmitter<EmailAttachment>();
  @Output() emailExpanded = new EventEmitter<EmailThreadItemDto>();
  @Output() emailCollapsed = new EventEmitter<EmailThreadItemDto>();

  // ==================== STATE ====================

  private destroy$ = new Subject<void>();
  private refreshTimer: ReturnType<typeof setTimeout> | null = null;

  emails: EmailThreadItemDto[] = [];
  isLoading = false;
  hasError = false;
  errorMessage = '';
  expandedEmailIds = new Set<string>(); // Track multiple expanded emails

  // Loading states for individual actions
  loadingStates = new Map<string, Set<string>>(); // emailId -> Set of action types

  // Statistics
  totalCount = 0;
  inboundCount = 0;
  outboundCount = 0;

  // ==================== CONSTRUCTOR ====================

  constructor(
    private readonly emailThreadService: EmailThreadService,
    private readonly logger: LoggerService,
    private readonly sanitizer: DomSanitizer,
    private readonly cdr: ChangeDetectorRef,
    private readonly dateService: DateService
  ) {}

  // ==================== LIFECYCLE HOOKS ====================

  ngOnInit(): void {
    if (!this.complaintId) {
      this.logger.warn('EmailThreadViewerComponent: complaintId is required');
      return;
    }

    this.loadEmails();

    // Setup auto-refresh if enabled
    if (this.autoRefresh && this.refreshInterval > 0) {
      this.setupAutoRefresh();
    }
  }

  ngOnDestroy(): void {
    this.clearAutoRefresh();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== EMAIL LOADING ====================

  /**
   * Load email thread for the complaint
   */
  loadEmails(): void {
    this.isLoading = true;
    this.hasError = false;
    this.errorMessage = '';

    this.emailThreadService.getComplaintEmails(this.complaintId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.emails = this.sortEmails(response.data);
            this.updateStatistics();
            this.logger.info('Emails loaded for complaint', {
              complaintId: this.complaintId,
              count: this.emails.length
            });
          } else {
            this.hasError = true;
            this.errorMessage = response.message || 'Failed to load emails';
            this.logger.error('Failed to load emails', { message: response.message });
          }
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: (error: Error) => {
          this.logger.error('Error loading emails:', error);
          this.hasError = true;
          this.errorMessage = 'An error occurred while loading emails';
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Refresh email thread
   */
  refresh(): void {
    this.loadEmails();
  }

  /**
   * Sort emails based on sortOrder
   */
  private sortEmails(emails: EmailThreadItemDto[]): EmailThreadItemDto[] {
    const sorted = [...emails];

    if (this.sortOrder === 'newest-first') {
      return sorted.sort((a, b) =>
        new Date(b.receivedAt).getTime() - new Date(a.receivedAt).getTime()
      );
    } else {
      return sorted.sort((a, b) =>
        new Date(a.receivedAt).getTime() - new Date(b.receivedAt).getTime()
      );
    }
  }

  /**
   * Toggle sort order
   */
  toggleSortOrder(): void {
    this.sortOrder = this.sortOrder === 'newest-first' ? 'oldest-first' : 'newest-first';
    this.emails = this.sortEmails(this.emails);
    this.cdr.markForCheck();
  }

  /**
   * Update statistics
   */
  private updateStatistics(): void {
    this.totalCount = this.emails.length;
    this.inboundCount = this.emails.filter(e => !e.isOutbound).length;
    this.outboundCount = this.emails.filter(e => e.isOutbound).length;
  }

  // ==================== AUTO-REFRESH ====================

  /**
   * Setup auto-refresh timer
   */
  private setupAutoRefresh(): void {
    this.clearAutoRefresh();
    this.refreshTimer = setTimeout(() => {
      this.loadEmails();
      this.setupAutoRefresh(); // Re-schedule
    }, this.refreshInterval);
  }

  /**
   * Clear auto-refresh timer
   */
  private clearAutoRefresh(): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }

  // ==================== EMAIL EXPANSION ====================

  /**
   * Toggle email expansion
   */
  toggleEmail(email: EmailThreadItemDto): void {
    const emailId = email.id;

    if (this.isExpanded(emailId)) {
      this.expandedEmailIds.delete(emailId);
      this.emailCollapsed.emit(email);
    } else {
      this.expandedEmailIds.add(emailId);
      this.emailExpanded.emit(email);
    }

    this.cdr.markForCheck();
  }

  /**
   * Check if email is expanded
   */
  isExpanded(emailId: string): boolean {
    return this.expandedEmailIds.has(emailId);
  }

  /**
   * Expand all emails
   */
  expandAll(): void {
    this.emails.forEach(email => {
      this.expandedEmailIds.add(email.id);
    });
    this.cdr.markForCheck();
  }

  /**
   * Collapse all emails
   */
  collapseAll(): void {
    this.expandedEmailIds.clear();
    this.cdr.markForCheck();
  }

  // ==================== ATTACHMENTS ====================

  /**
   * Check if email has attachments
   */
  hasAttachments(email: EmailThreadItemDto): boolean {
    return email.attachmentCount > 0;
  }

  // ==================== ACTIONS ====================

  /**
   * Handle reply click
   */
  onReplyClick(email: EmailThreadItemDto): void {
    this.replyClicked.emit(email);
    this.logger.info('Reply clicked', { emailId: email.id });
  }

  /**
   * Handle reply all click
   */
  onReplyAllClick(email: EmailThreadItemDto): void {
    this.replyAllClicked.emit(email);
    this.logger.info('Reply All clicked', { emailId: email.id });
  }

  /**
   * Handle forward click
   */
  onForwardClick(email: EmailThreadItemDto): void {
    this.forwardClicked.emit(email);
    this.logger.info('Forward clicked', { emailId: email.id });
  }

  /**
   * Handle private note click
   */
  onPrivateNoteClick(email: EmailThreadItemDto): void {
    this.privateNoteClicked.emit(email);
    this.logger.info('Private Note clicked', { emailId: email.id });
  }

  /**
   * Handle compose new email click
   */
  onComposeNewClick(): void {
    this.composeNewClicked.emit();
    this.logger.info('Compose New Email clicked');
  }

  /**
   * Check if action is loading
   */
  isActionLoading(emailId: string, action: string): boolean {
    return this.loadingStates.get(emailId)?.has(action) || false;
  }

  /**
   * Set action loading state
   */
  setActionLoading(emailId: string, action: string, loading: boolean): void {
    if (!this.loadingStates.has(emailId)) {
      this.loadingStates.set(emailId, new Set());
    }

    const actions = this.loadingStates.get(emailId)!;
    if (loading) {
      actions.add(action);
    } else {
      actions.delete(action);
    }

    this.cdr.markForCheck();
  }

  // ==================== HTML SANITIZATION ====================

  /**
   * Sanitize HTML content using DOMPurify
   * Removes potentially dangerous tags and attributes
   * Pre-processes to remove style/script tags completely (including their content)
   */
  sanitizeHtml(html: string | undefined): SafeHtml {
    if (!html) {
      return '';
    }

    // Pre-process: Remove <style>, <script>, <head>, and <meta> tags completely (including content)
    // This prevents CSS/JS content from appearing as raw text after DOMPurify strips the tags
    let preprocessed = html
      .replace(/<style[^>]*>[\s\S]*?<\/style>/gi, '') // Remove style tags and content
      .replace(/<script[^>]*>[\s\S]*?<\/script>/gi, '') // Remove script tags and content
      .replace(/<head[^>]*>[\s\S]*?<\/head>/gi, '') // Remove head tags and content
      .replace(/<meta[^>]*\/?>/gi, '') // Remove meta tags
      .replace(/<link[^>]*\/?>/gi, '') // Remove link tags (external stylesheets)
      .replace(/<!--[\s\S]*?-->/g, ''); // Remove HTML comments

    // Configure DOMPurify to allow safe HTML while removing dangerous content
    const clean = DOMPurify.sanitize(preprocessed, {
      ALLOWED_TAGS: [
        'p', 'br', 'strong', 'em', 'u', 's', 'a', 'span', 'div',
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'ul', 'ol', 'li', 'blockquote', 'pre', 'code',
        'table', 'thead', 'tbody', 'tr', 'th', 'td', 'caption', 'colgroup', 'col',
        'img', 'figure', 'figcaption',
        'b', 'i', 'small', 'mark', 'sub', 'sup',
        'hr', 'center', 'font'
      ],
      ALLOWED_ATTR: [
        'href', 'title', 'target', 'rel', 'class', 'style',
        'src', 'alt', 'width', 'height', 'border', 'cellpadding', 'cellspacing',
        'align', 'valign', 'bgcolor', 'color', 'size', 'face'
      ],
      ALLOW_DATA_ATTR: false,
      FORCE_BODY: true,
      RETURN_DOM: false,
      RETURN_DOM_FRAGMENT: false,
      SANITIZE_DOM: true
    });

    return this.sanitizer.sanitize(1, clean) || ''; // SecurityContext.HTML = 1
  }

  /**
   * Get email body (sanitized HTML or plain text)
   */
  getEmailBody(email: EmailThreadItemDto): SafeHtml | string {
    if (email.htmlBody) {
      return this.sanitizeHtml(email.htmlBody);
    }
    return email.textBody || '';
  }

  // ==================== FORMATTING ====================

  /**
   * Format date for display (using user's timezone from DateService)
   * Shows relative time for recent emails, formatted date for older ones
   */
  formatDate(dateString: string): string {
    // Use DateService which respects user's timezone settings
    return this.dateService.getRelativeTime(dateString);
  }

  /**
   * Format full date for tooltip (using user's timezone from DateService)
   * Returns full formatted date with timezone consideration
   */
  formatFullDate(dateString: string): string {
    // Use DateService.formatDate which respects user's timezone
    // This will show: "DD/MM/YYYY, hh:mm AM/PM" in user's configured timezone
    return this.dateService.formatDate(dateString, true); // true = include seconds
  }

  /**
   * Get email preview (collapsed state)
   */
  getEmailPreview(email: EmailThreadItemDto): string {
    return this.emailThreadService.formatEmailPreview(email, 150);
  }

  /**
   * Get direction label
   */
  getDirectionLabel(isOutbound: boolean): string {
    return this.emailThreadService.getDirectionLabel(isOutbound);
  }

  /**
   * Get user initials for avatar
   */
  getUserInitials(name: string | undefined, email: string): string {
    if (name) {
      const parts = name.trim().split(' ');
      if (parts.length >= 2) {
        return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
      }
      return name.substring(0, 2).toUpperCase();
    }

    return email.substring(0, 2).toUpperCase();
  }

  /**
   * Get email recipients display (To, CC combined)
   */
  getRecipients(email: EmailThreadItemDto): string {
    return this.emailThreadService.formatRecipients([
      ...email.toRecipients,
      ...email.ccRecipients
    ]);
  }

  /**
   * Format recipients list for display
   */
  formatRecipientsList(recipients: EmailRecipient[]): string {
    return this.emailThreadService.formatRecipients(recipients);
  }

  // ==================== UTILITY ====================

  /**
   * Track by function for ngFor optimization
   */
  trackByEmailId(index: number, email: EmailThreadItemDto): string {
    return email.id;
  }
}
