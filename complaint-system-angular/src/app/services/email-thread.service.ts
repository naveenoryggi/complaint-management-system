import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  EmailMessage,
  EmailAttachment,
  SendEmailReplyRequest,
  EmailStatistics,
  EmailDirection,
  EmailStatus,
  ReplyType,
  EmailRecipient
} from '../models/communication.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  error?: string;
}

export interface EmailThread {
  complaintId: string;
  emails: EmailMessage[];
  totalCount: number;
  inboundCount: number;
  outboundCount: number;
  unreadCount: number;
}

export interface EmailThreadItemDto {
  id: string;
  messageId: string;
  fromEmail: string;
  fromName: string;
  toRecipients: EmailRecipient[];
  ccRecipients: EmailRecipient[];
  subject: string;
  htmlBody: string;
  textBody: string;
  receivedAt: Date;
  sentAt?: Date;
  isOutbound: boolean;
  isPrivateNote: boolean;
  isRead: boolean;
  sentByUserId?: string;
  attachmentCount: number;
}

export interface ComplaintEmailParticipant {
  id: string;
  complaintId: string;
  emailAddress: string;
  displayName?: string;
  participantType: string; // To, CC, BCC
  addedBy: string;
  addedAt: Date;
  isActive: boolean;
}

export interface CannedResponseDto {
  id: string;
  title: string;
  shortCode?: string;
  subject?: string;
  body: string;
  usageCount: number;
  categoryId?: string;
  categoryName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailThreadService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/complaints`;

  // State management for email threads per complaint
  private emailThreadsMap = new Map<string, BehaviorSubject<EmailThreadItemDto[]>>();
  private unreadCountsMap = new Map<string, BehaviorSubject<number>>();

  // ==========================
  // Email Thread Operations
  // ==========================

  /**
   * Get email thread for a complaint
   * NEW: Uses /api/complaints/{complaintId}/emails endpoint
   */
  getComplaintEmails(complaintId: string, includePrivateNotes: boolean = false): Observable<ApiResponse<EmailThreadItemDto[]>> {
    const params: any = {};
    if (includePrivateNotes) {
      params.includePrivateNotes = 'true';
    }

    return this.http.get<ApiResponse<EmailThreadItemDto[]>>(
      `${this.baseUrl}/${complaintId}/emails`,
      { params }
    ).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.setEmailThread(complaintId, response.data);
          // Update unread count
          const unreadCount = response.data.filter(e => !e.isRead && !e.isOutbound).length;
          this.setUnreadCount(complaintId, unreadCount);
        }
      })
    );
  }

  /**
   * Get email thread as observable for a complaint
   */
  getEmailThread$(complaintId: string): Observable<EmailThreadItemDto[]> {
    if (!this.emailThreadsMap.has(complaintId)) {
      this.emailThreadsMap.set(complaintId, new BehaviorSubject<EmailThreadItemDto[]>([]));
      // Load emails for this complaint
      this.getComplaintEmails(complaintId).subscribe();
    }
    return this.emailThreadsMap.get(complaintId)!.asObservable();
  }

  /**
   * Get participants in email thread
   * NEW: Uses /api/complaints/{complaintId}/emails/participants endpoint
   */
  getParticipants(complaintId: string): Observable<ApiResponse<ComplaintEmailParticipant[]>> {
    return this.http.get<ApiResponse<ComplaintEmailParticipant[]>>(
      `${this.baseUrl}/${complaintId}/emails/participants`
    );
  }

  /**
   * Get unread email count for a complaint
   * NEW: Uses /api/complaints/{complaintId}/emails/unread-count endpoint
   */
  getUnreadCount(complaintId: string): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(
      `${this.baseUrl}/${complaintId}/emails/unread-count`
    ).pipe(
      tap(response => {
        if (response.isSuccess && response.data !== undefined) {
          this.setUnreadCount(complaintId, response.data);
        }
      })
    );
  }

  /**
   * Get unread count as observable
   */
  getUnreadCount$(complaintId: string): Observable<number> {
    if (!this.unreadCountsMap.has(complaintId)) {
      this.unreadCountsMap.set(complaintId, new BehaviorSubject<number>(0));
      // Load unread count for this complaint
      this.getUnreadCount(complaintId).subscribe();
    }
    return this.unreadCountsMap.get(complaintId)!.asObservable();
  }

  /**
   * Set email thread for a complaint (used internally)
   */
  private setEmailThread(complaintId: string, emails: EmailThreadItemDto[]): void {
    if (!this.emailThreadsMap.has(complaintId)) {
      this.emailThreadsMap.set(complaintId, new BehaviorSubject<EmailThreadItemDto[]>(emails));
    } else {
      this.emailThreadsMap.get(complaintId)!.next(emails);
    }
  }

  /**
   * Set unread count for a complaint (used internally)
   */
  private setUnreadCount(complaintId: string, count: number): void {
    if (!this.unreadCountsMap.has(complaintId)) {
      this.unreadCountsMap.set(complaintId, new BehaviorSubject<number>(count));
    } else {
      this.unreadCountsMap.get(complaintId)!.next(count);
    }
  }

  /**
   * Clear email thread cache for a complaint
   */
  clearEmailThread(complaintId: string): void {
    this.emailThreadsMap.delete(complaintId);
    this.unreadCountsMap.delete(complaintId);
  }

  /**
   * Clear all email thread caches
   */
  clearAllEmailThreads(): void {
    this.emailThreadsMap.clear();
    this.unreadCountsMap.clear();
  }

  // ==========================
  // Send Email Operations
  // ==========================

  /**
   * Send email reply (Reply/Reply All/Forward/Private Note)
   * NEW: Uses /api/complaints/{complaintId}/emails/reply endpoint
   */
  sendReply(complaintId: string, request: SendEmailReplyRequest): Observable<ApiResponse<EmailThreadItemDto>> {
    return this.http.post<ApiResponse<EmailThreadItemDto>>(
      `${this.baseUrl}/${complaintId}/emails/reply`,
      request
    ).pipe(
      tap(response => {
        if (response.isSuccess) {
          // Refresh the email thread for this complaint
          this.getComplaintEmails(complaintId).subscribe();
        }
      })
    );
  }

  /**
   * Send a simple reply
   */
  sendSimpleReply(
    complaintId: string,
    inReplyToEmailId: string | null,
    subject: string,
    htmlBody: string,
    toRecipients: EmailRecipient[],
    ccRecipients: EmailRecipient[] = [],
    bccRecipients: EmailRecipient[] = []
  ): Observable<ApiResponse<EmailThreadItemDto>> {
    const request: SendEmailReplyRequest = {
      complaintId,
      inReplyToEmailMessageId: inReplyToEmailId,
      replyType: ReplyType.Reply,
      subject,
      htmlBody,
      toRecipients,
      ccRecipients,
      bccRecipients,
      isPrivateNote: false
    };
    return this.sendReply(complaintId, request);
  }

  /**
   * Send private note (internal-only, not sent via email)
   */
  sendPrivateNote(
    complaintId: string,
    subject: string,
    htmlBody: string
  ): Observable<ApiResponse<EmailThreadItemDto>> {
    const request: SendEmailReplyRequest = {
      complaintId,
      inReplyToEmailMessageId: null,
      replyType: ReplyType.PrivateNote,
      subject,
      htmlBody,
      toRecipients: [],
      ccRecipients: [],
      bccRecipients: [],
      isPrivateNote: true
    };
    return this.sendReply(complaintId, request);
  }

  // ==========================
  // Mark as Read Operations
  // ==========================

  /**
   * Mark email as read
   * NEW: Uses /api/complaints/{complaintId}/emails/{emailId}/mark-read endpoint
   */
  markAsRead(complaintId: string, emailId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.baseUrl}/${complaintId}/emails/${emailId}/mark-read`,
      {}
    ).pipe(
      tap(response => {
        if (response.isSuccess) {
          // Refresh the email thread to update read status
          this.getComplaintEmails(complaintId).subscribe();
        }
      })
    );
  }

  /**
   * Mark all emails in complaint as read
   * NEW: Uses /api/complaints/{complaintId}/emails/mark-all-read endpoint
   */
  markAllAsRead(complaintId: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.baseUrl}/${complaintId}/emails/mark-all-read`,
      {}
    ).pipe(
      tap(response => {
        if (response.isSuccess) {
          // Refresh the email thread to update read status
          this.getComplaintEmails(complaintId).subscribe();
        }
      })
    );
  }

  // ==========================
  // Canned Responses
  // ==========================

  /**
   * Get canned responses (quick reply templates)
   * NEW: Uses /api/canned-responses endpoint
   */
  getCannedResponses(categoryId?: string): Observable<ApiResponse<CannedResponseDto[]>> {
    const params: any = {};
    if (categoryId) {
      params.categoryId = categoryId;
    }

    return this.http.get<ApiResponse<CannedResponseDto[]>>(
      `${environment.apiUrl}/canned-responses`,
      { params }
    );
  }

  // ==========================
  // Email Thread Analysis
  // ==========================

  /**
   * Analyze email thread for a complaint
   */
  analyzeEmailThread(complaintId: string): Observable<EmailThread> {
    return new Observable(observer => {
      this.getComplaintEmails(complaintId).subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            const emails = response.data;
            const thread: EmailThread = {
              complaintId,
              emails: emails.sort((a, b) =>
                new Date(a.receivedAt).getTime() - new Date(b.receivedAt).getTime()
              ) as any,
              totalCount: emails.length,
              inboundCount: emails.filter(e => !e.isOutbound).length,
              outboundCount: emails.filter(e => e.isOutbound).length,
              unreadCount: emails.filter(e => !e.isRead && !e.isOutbound).length
            };
            observer.next(thread);
            observer.complete();
          } else {
            observer.error(new Error(response.message || 'Failed to load emails'));
          }
        },
        error: (err) => observer.error(err)
      });
    });
  }

  /**
   * Get latest email for a complaint
   */
  getLatestEmail(complaintId: string): Observable<EmailThreadItemDto | null> {
    return new Observable(observer => {
      this.getComplaintEmails(complaintId).subscribe({
        next: (response) => {
          if (response.isSuccess && response.data && response.data.length > 0) {
            const sorted = response.data.sort((a, b) =>
              new Date(b.receivedAt).getTime() - new Date(a.receivedAt).getTime()
            );
            observer.next(sorted[0]);
            observer.complete();
          } else {
            observer.next(null);
            observer.complete();
          }
        },
        error: () => {
          observer.next(null);
          observer.complete();
        }
      });
    });
  }

  // ==========================
  // Helper Methods
  // ==========================

  /**
   * Check if complaint has email thread
   */
  hasEmailThread(complaintId: string): Observable<boolean> {
    return new Observable(observer => {
      this.getComplaintEmails(complaintId).subscribe({
        next: (response) => {
          observer.next(response.isSuccess && response.data && response.data.length > 0);
          observer.complete();
        },
        error: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }

  /**
   * Format email for display
   * Removes quoted content from email replies to show only the new message
   */
  formatEmailPreview(email: EmailThreadItemDto, maxLength: number = 100): string {
    if (!email) return '';

    let body = email.htmlBody
      ? this.stripHtmlWithoutQuotes(email.htmlBody)
      : this.removeQuotedText(email.textBody || '');

    if (!body) return '';

    // Clean up whitespace
    body = body.replace(/\s+/g, ' ').trim();

    if (body.length <= maxLength) {
      return body;
    }

    return body.substring(0, maxLength) + '...';
  }

  /**
   * Strip HTML tags and remove quoted content from email
   */
  private stripHtmlWithoutQuotes(html: string): string {
    if (!html) return '';

    // First remove quoted content patterns (before stripping HTML)
    let processed = html
      // Remove Gmail quoted content
      .replace(/<div class="gmail_quote"[\s\S]*$/gi, '')
      .replace(/<div class="gmail_extra"[\s\S]*$/gi, '')
      // Remove Outlook quoted content
      .replace(/<hr[^>]*>[\s\S]*?<p[^>]*>\s*<b>\s*From:\s*<\/b>[\s\S]*$/gi, '')
      .replace(/<p[^>]*>\s*<strong>\s*From:\s*<\/strong>[\s\S]*$/gi, '')
      .replace(/<p[^>]*>\s*<b>\s*From:\s*<\/b>[\s\S]*$/gi, '')
      // Remove blockquotes
      .replace(/<blockquote[\s\S]*?<\/blockquote>/gi, '')
      // Remove other quote patterns
      .replace(/<div class="yahoo_quoted"[\s\S]*$/gi, '')
      .replace(/<div class="moz-cite-prefix"[\s\S]*$/gi, '');

    // Then strip HTML tags
    return this.stripHtml(processed);
  }

  /**
   * Remove quoted text from plain text emails
   */
  private removeQuotedText(text: string): string {
    if (!text) return '';

    // Split by common quote indicators and take only the first part
    const lines = text.split('\n');
    const result: string[] = [];

    for (const line of lines) {
      // Stop at common quote indicators
      if (line.trim().startsWith('>') ||
          line.trim().startsWith('From:') ||
          line.trim().startsWith('On ') && line.includes(' wrote:') ||
          line.trim().startsWith('-----Original Message-----')) {
        break;
      }
      result.push(line);
    }

    return result.join('\n');
  }

  /**
   * Strip HTML tags from string
   * Pre-processes to remove style/script tags completely before extracting text
   */
  stripHtml(html: string): string {
    if (!html) return '';

    // Pre-process: Remove <style>, <script>, <head>, and other non-visible content tags
    // This prevents CSS/JS content from appearing in the text output
    let preprocessed = html
      .replace(/<style[^>]*>[\s\S]*?<\/style>/gi, '') // Remove style tags and content
      .replace(/<script[^>]*>[\s\S]*?<\/script>/gi, '') // Remove script tags and content
      .replace(/<head[^>]*>[\s\S]*?<\/head>/gi, '') // Remove head tags and content
      .replace(/<meta[^>]*\/?>/gi, '') // Remove meta tags
      .replace(/<link[^>]*\/?>/gi, '') // Remove link tags
      .replace(/<!--[\s\S]*?-->/g, ''); // Remove HTML comments

    const tmp = document.createElement('DIV');
    tmp.innerHTML = preprocessed;
    return tmp.textContent || tmp.innerText || '';
  }

  /**
   * Get email direction label
   */
  getDirectionLabel(isOutbound: boolean): string {
    return isOutbound ? 'Sent' : 'Received';
  }

  /**
   * Validate email reply before sending
   */
  validateEmailReply(request: SendEmailReplyRequest): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    if (!request.complaintId || request.complaintId.trim() === '') {
      errors.push('Complaint ID is required');
    }

    if (!request.isPrivateNote) {
      if (!request.toRecipients || request.toRecipients.length === 0) {
        errors.push('At least one recipient is required');
      } else {
        // Validate all To recipients
        for (const recipient of request.toRecipients) {
          if (!this.isValidEmail(recipient.emailAddress)) {
            errors.push(`Invalid email address: ${recipient.emailAddress}`);
          }
        }
      }

      // Validate CC recipients if provided
      if (request.ccRecipients && request.ccRecipients.length > 0) {
        for (const recipient of request.ccRecipients) {
          if (!this.isValidEmail(recipient.emailAddress)) {
            errors.push(`Invalid CC email: ${recipient.emailAddress}`);
          }
        }
      }

      // Validate BCC recipients if provided
      if (request.bccRecipients && request.bccRecipients.length > 0) {
        for (const recipient of request.bccRecipients) {
          if (!this.isValidEmail(recipient.emailAddress)) {
            errors.push(`Invalid BCC email: ${recipient.emailAddress}`);
          }
        }
      }
    }

    if (!request.subject || request.subject.trim() === '') {
      errors.push('Subject is required');
    }

    if (!request.htmlBody || request.htmlBody.trim() === '') {
      errors.push('Email body is required');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  /**
   * Simple email validation
   */
  private isValidEmail(email: string): boolean {
    if (!email) return false;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  /**
   * Format recipients for display
   */
  formatRecipients(recipients: EmailRecipient[]): string {
    if (!recipients || recipients.length === 0) return '';

    return recipients.map(r => r.displayName || r.emailAddress).join(', ');
  }

  /**
   * Get time ago string (e.g., "2 hours ago")
   */
  getTimeAgo(date: Date): string {
    const now = new Date();
    const emailDate = new Date(date);
    const diffMs = now.getTime() - emailDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

    return emailDate.toLocaleDateString();
  }
}
