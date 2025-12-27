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
 * Interface for reply events that include the quick reply content
 */
export interface EmailReplyEvent {
  email: EmailThreadItemDto;
  quickReplyContent: string;
}

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

  @Output() replyClicked = new EventEmitter<EmailReplyEvent>();
  @Output() replyAllClicked = new EventEmitter<EmailReplyEvent>();
  @Output() forwardClicked = new EventEmitter<EmailReplyEvent>();
  @Output() privateNoteClicked = new EventEmitter<EmailReplyEvent>();
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

  // Reply content for the text editor
  replyContent = '';

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
   * Priority: Unread inbound emails first, then by date
   * This ensures support agents see emails requiring attention at the top
   */
  private sortEmails(emails: EmailThreadItemDto[]): EmailThreadItemDto[] {
    const sorted = [...emails];

    return sorted.sort((a, b) => {
      // First priority: Unread inbound emails come first
      // (Outbound emails are always "read" since we sent them)
      const aUnread = !a.isRead && !a.isOutbound;
      const bUnread = !b.isRead && !b.isOutbound;

      if (aUnread && !bUnread) return -1; // a is unread, b is read -> a first
      if (!aUnread && bUnread) return 1;  // b is unread, a is read -> b first

      // Second priority: Sort by date based on sortOrder
      const aTime = new Date(a.receivedAt).getTime();
      const bTime = new Date(b.receivedAt).getTime();

      if (this.sortOrder === 'newest-first') {
        return bTime - aTime; // Newest first
      } else {
        return aTime - bTime; // Oldest first
      }
    });
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
   * When expanding an unread inbound email, mark it as read
   */
  toggleEmail(email: EmailThreadItemDto): void {
    const emailId = email.id;

    if (this.isExpanded(emailId)) {
      this.expandedEmailIds.delete(emailId);
      this.emailCollapsed.emit(email);
    } else {
      this.expandedEmailIds.add(emailId);
      this.emailExpanded.emit(email);

      // Mark as read when expanding an unread inbound email
      // This clears the HasCustomerResponse flag on the complaint
      if (!email.isRead && !email.isOutbound) {
        this.markEmailAsRead(email);
      }
    }

    this.cdr.markForCheck();
  }

  /**
   * Mark email as read via API
   * This also clears the HasCustomerResponse flag on the complaint
   */
  private markEmailAsRead(email: EmailThreadItemDto): void {
    this.emailThreadService.markAsRead(this.complaintId, email.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess) {
            // Update local state
            email.isRead = true;
            this.logger.info('Email marked as read', { emailId: email.id, complaintId: this.complaintId });
            this.cdr.markForCheck();
          }
        },
        error: (error: Error) => {
          this.logger.error('Failed to mark email as read', { emailId: email.id, error });
        }
      });
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
    const event: EmailReplyEvent = { email, quickReplyContent: this.replyContent };
    this.replyClicked.emit(event);
    this.logger.info('Reply clicked', { emailId: email.id, hasContent: !!this.replyContent });
    this.clearReplyContent();
  }

  /**
   * Handle reply all click
   */
  onReplyAllClick(email: EmailThreadItemDto): void {
    const event: EmailReplyEvent = { email, quickReplyContent: this.replyContent };
    this.replyAllClicked.emit(event);
    this.logger.info('Reply All clicked', { emailId: email.id, hasContent: !!this.replyContent });
    this.clearReplyContent();
  }

  /**
   * Handle forward click
   */
  onForwardClick(email: EmailThreadItemDto): void {
    const event: EmailReplyEvent = { email, quickReplyContent: this.replyContent };
    this.forwardClicked.emit(event);
    this.logger.info('Forward clicked', { emailId: email.id, hasContent: !!this.replyContent });
    this.clearReplyContent();
  }

  /**
   * Handle private note click
   */
  onPrivateNoteClick(email: EmailThreadItemDto): void {
    const event: EmailReplyEvent = { email, quickReplyContent: this.replyContent };
    this.privateNoteClicked.emit(event);
    this.logger.info('Private Note clicked', { emailId: email.id, hasContent: !!this.replyContent });
    this.clearReplyContent();
  }

  /**
   * Clear the quick reply content after emitting
   */
  private clearReplyContent(): void {
    this.replyContent = '';
    this.cdr.markForCheck();
  }

  /**
   * Handle compose new email click
   */
  onComposeNewClick(): void {
    this.composeNewClicked.emit();
    this.logger.info('Compose New Email clicked');
  }

  /**
   * Check if there are any inbound emails to reply to
   */
  hasInboundEmails(): boolean {
    return this.emails.some(e => !e.isOutbound);
  }

  /**
   * Get the latest inbound email for reply
   */
  private getLatestInboundEmail(): EmailThreadItemDto | undefined {
    return this.emails.find(e => !e.isOutbound);
  }

  /**
   * Handle reply to latest email
   */
  onReplyToLatest(): void {
    const latestInbound = this.getLatestInboundEmail();
    if (latestInbound) {
      const event: EmailReplyEvent = { email: latestInbound, quickReplyContent: this.replyContent };
      this.replyClicked.emit(event);
      this.logger.info('Reply to latest clicked', { emailId: latestInbound.id, hasContent: !!this.replyContent });
      this.clearReplyContent();
    }
  }

  /**
   * Handle reply all to latest email
   */
  onReplyAllToLatest(): void {
    const latestInbound = this.getLatestInboundEmail();
    if (latestInbound) {
      const event: EmailReplyEvent = { email: latestInbound, quickReplyContent: this.replyContent };
      this.replyAllClicked.emit(event);
      this.logger.info('Reply All to latest clicked', { emailId: latestInbound.id, hasContent: !!this.replyContent });
      this.clearReplyContent();
    }
  }

  /**
   * Handle forward latest email
   */
  onForwardLatest(): void {
    const latest = this.emails[0];
    if (latest) {
      const event: EmailReplyEvent = { email: latest, quickReplyContent: this.replyContent };
      this.forwardClicked.emit(event);
      this.logger.info('Forward latest clicked', { emailId: latest.id, hasContent: !!this.replyContent });
      this.clearReplyContent();
    }
  }

  /**
   * Handle add internal note
   */
  onAddInternalNote(): void {
    const latest = this.emails[0];
    if (latest) {
      const event: EmailReplyEvent = { email: latest, quickReplyContent: this.replyContent };
      this.privateNoteClicked.emit(event);
      this.logger.info('Add Internal Note clicked', { hasContent: !!this.replyContent });
      this.clearReplyContent();
    }
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
   * Also removes quoted email content (from Outlook, Gmail, etc.) to avoid redundancy in thread view
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

    // Remove quoted email content to avoid redundancy in thread view
    preprocessed = this.removeQuotedContent(preprocessed);

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
   * Remove quoted email content from HTML to avoid redundancy
   * Handles various email client patterns (Outlook, Gmail, Yahoo, etc.)
   */
  private removeQuotedContent(html: string): string {
    let result = html;

    // Remove Gmail quoted content
    result = result.replace(/<div class="gmail_quote"[\s\S]*$/gi, '');
    result = result.replace(/<div class="gmail_extra"[\s\S]*$/gi, '');

    // Remove Outlook quoted content (starts with divider line or "From:" pattern)
    // Pattern: <hr> followed by From: email info
    result = result.replace(/<hr[^>]*>[\s\S]*?<p[^>]*>\s*<b>\s*From:\s*<\/b>[\s\S]*$/gi, '');

    // Outlook mobile pattern: <p><strong>From:</strong>...</p> and everything after
    result = result.replace(/<p[^>]*>\s*<strong>\s*From:\s*<\/strong>[\s\S]*$/gi, '');
    result = result.replace(/<p[^>]*>\s*<b>\s*From:\s*<\/b>[\s\S]*$/gi, '');

    // Remove blockquote elements (common for quoted content)
    result = result.replace(/<blockquote[\s\S]*?<\/blockquote>/gi, '');

    // Remove Yahoo quoted content
    result = result.replace(/<div class="yahoo_quoted"[\s\S]*$/gi, '');

    // Remove Apple Mail signatures
    result = result.replace(/<div class="AppleMailSignature"[\s\S]*?<\/div>/gi, '');

    // Remove Mozilla cite prefix
    result = result.replace(/<div class="moz-cite-prefix"[\s\S]*$/gi, '');

    // Trim trailing empty paragraphs
    result = result.replace(/(<p>\s*(&nbsp;)*\s*<\/p>\s*)+$/gi, '');
    result = result.replace(/(<br\s*\/?>\s*)+$/gi, '');

    return result;
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
