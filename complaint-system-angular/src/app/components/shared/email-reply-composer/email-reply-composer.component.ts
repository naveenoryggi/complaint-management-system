import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormControl, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  SendEmailReplyRequest,
  ReplyType,
  EmailRecipient
} from '../../../models/communication.model';
import { EmailThreadService, EmailThreadItemDto, CannedResponseDto } from '../../../services/email-thread.service';
import { LoggerService } from '../../../core/services/logger.service';

/**
 * EmailReplyComposerComponent
 *
 * A professional email composition component with rich text editing capabilities.
 * Features:
 * - Rich text editor with Quill.js (formatting, links, lists, etc.)
 * - Reply, Reply All, Forward, and Private Note modes
 * - Recipient management (To, CC, BCC) with email validation
 * - Canned responses for quick replies
 * - Email signature support
 * - Draft auto-save (optional)
 * - Attachment support indication
 * - Full keyboard shortcuts support
 *
 * @example
 * <app-email-reply-composer
 *   [complaintId]="complaint.id"
 *   [replyTo]="selectedEmail"
 *   [replyType]="ReplyType.Reply"
 *   (emailSent)="handleEmailSent($event)"
 *   (cancelled)="handleCancel()">
 * </app-email-reply-composer>
 */
@Component({
  selector: 'app-email-reply-composer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, QuillModule],
  templateUrl: './email-reply-composer.component.html',
  styleUrls: ['./email-reply-composer.component.scss']
})
export class EmailReplyComposerComponent implements OnInit, OnDestroy {
  // ==================== INPUTS ====================

  @Input() complaintId!: string;
  @Input() replyTo?: EmailThreadItemDto; // Original email to reply to
  @Input() emailThread?: EmailThreadItemDto[]; // Full email thread (for forwarding entire conversation)
  @Input() replyType: ReplyType = ReplyType.Reply;
  @Input() showCannedResponses: boolean = true;
  @Input() enableAutoSave: boolean = false;
  @Input() autoSaveInterval: number = 30000; // 30 seconds

  // ==================== OUTPUTS ====================

  @Output() emailSent = new EventEmitter<EmailThreadItemDto>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() draftSaved = new EventEmitter<SendEmailReplyRequest>();

  // ==================== SERVICES ====================

  private readonly fb = inject(FormBuilder);
  private readonly emailThreadService = inject(EmailThreadService);
  private readonly logger = inject(LoggerService);

  // ==================== STATE ====================

  private destroy$ = new Subject<void>();
  private autoSaveTimer?: ReturnType<typeof setTimeout>;

  emailForm!: FormGroup;
  isSending = false;
  hasError = false;
  errorMessage = '';
  successMessage = '';

  // Recipient chips management
  toRecipients: EmailRecipient[] = [];
  ccRecipients: EmailRecipient[] = [];
  bccRecipients: EmailRecipient[] = [];

  toInputValue = '';
  ccInputValue = '';
  bccInputValue = '';

  showCc = false;
  showBcc = false;

  // Canned responses
  cannedResponses: CannedResponseDto[] = [];
  loadingCannedResponses = false;
  selectedCannedResponse: CannedResponseDto | null = null;

  // Quill editor configuration - Enhanced like Outlook/Zoho
  quillConfig = {
    toolbar: [
      // Font family and size - First row
      [{ 'font': [] }],
      [{ 'size': ['small', false, 'large', 'huge'] }],

      // Text formatting - Second row
      ['bold', 'italic', 'underline', 'strike'],
      [{ 'color': [] }, { 'background': [] }],

      // Paragraph formatting - Third row
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'align': [] }],

      // Lists and indentation - Fourth row
      [{ 'list': 'ordered'}, { 'list': 'bullet' }, { 'list': 'check' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],

      // Advanced formatting - Fifth row
      ['blockquote', 'code-block'],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'direction': 'rtl' }],

      // Insert elements - Sixth row
      ['link', 'image', 'video', 'formula'],

      // Clear formatting
      ['clean']
    ],
    keyboard: {
      bindings: {
        // Ctrl+Enter to send
        sendEmail: {
          key: 13,
          ctrlKey: true,
          handler: () => this.onSubmit()
        }
      }
    }
  };

  // Enum reference for template
  ReplyType = ReplyType;

  // ==================== LIFECYCLE HOOKS ====================

  ngOnInit(): void {
    this.initializeForm();
    this.prefillRecipientsBasedOnReplyType();
    this.prefillSubject();
    this.prefillBody(); // Include original email content for forwards/replies

    if (this.showCannedResponses) {
      this.loadCannedResponses();
    }

    if (this.enableAutoSave) {
      this.setupAutoSave();
    }
  }

  ngOnDestroy(): void {
    this.clearAutoSave();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==================== FORM INITIALIZATION ====================

  private initializeForm(): void {
    this.emailForm = this.fb.group({
      subject: ['', Validators.required],
      body: ['', Validators.required],
      isPrivateNote: [this.replyType === ReplyType.PrivateNote]
    });

    // Watch for form changes for auto-save
    if (this.enableAutoSave) {
      this.emailForm.valueChanges
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          // Trigger auto-save
        });
    }
  }

  private prefillRecipientsBasedOnReplyType(): void {
    if (!this.replyTo) return;

    switch (this.replyType) {
      case ReplyType.Reply:
        // Reply to sender only
        this.toRecipients = [{
          emailAddress: this.replyTo.fromEmail,
          displayName: this.replyTo.fromName
        }];
        break;

      case ReplyType.ReplyAll:
        // Reply to sender + all To + all CC
        this.toRecipients = [
          {
            emailAddress: this.replyTo.fromEmail,
            displayName: this.replyTo.fromName
          },
          ...this.replyTo.toRecipients,
          ...this.replyTo.ccRecipients
        ];

        // Remove duplicates
        this.toRecipients = this.removeDuplicateRecipients(this.toRecipients);
        break;

      case ReplyType.Forward:
        // Forward - no recipients pre-filled
        this.toRecipients = [];
        break;

      case ReplyType.PrivateNote:
        // Private note - no recipients
        this.toRecipients = [];
        break;
    }
  }

  private prefillSubject(): void {
    if (!this.replyTo) {
      this.emailForm.patchValue({ subject: '' });
      return;
    }

    let subject = this.replyTo.subject || '(No Subject)';

    switch (this.replyType) {
      case ReplyType.Reply:
      case ReplyType.ReplyAll:
        if (!subject.toLowerCase().startsWith('re:')) {
          subject = `Re: ${subject}`;
        }
        break;

      case ReplyType.Forward:
        if (!subject.toLowerCase().startsWith('fwd:')) {
          subject = `Fwd: ${subject}`;
        }
        break;

      case ReplyType.PrivateNote:
        subject = `[Internal Note] ${subject}`;
        break;
    }

    this.emailForm.patchValue({ subject });
  }

  private prefillBody(): void {
    // Only include original email content for Reply, ReplyAll, and Forward
    if (!this.replyTo || this.replyType === ReplyType.PrivateNote) {
      return;
    }

    let quotedContent: string;

    // For Forward, include the ENTIRE email thread if available
    if (this.replyType === ReplyType.Forward && this.emailThread && this.emailThread.length > 0) {
      quotedContent = this.formatEntireThreadAsQuoted(this.emailThread);
    } else {
      // For Reply/ReplyAll or single message forward
      quotedContent = this.formatOriginalEmailAsQuoted(this.replyTo);
    }

    // For Forward, include the full quoted message
    // For Reply/ReplyAll, user can choose to include it or not
    if (this.replyType === ReplyType.Forward) {
      this.emailForm.patchValue({ body: quotedContent });
    } else {
      // For Reply/ReplyAll, append quoted text after cursor
      // This allows user to type above the quoted text
      this.emailForm.patchValue({ body: `<br><br>${quotedContent}` });
    }
  }

  private formatOriginalEmailAsQuoted(email: EmailThreadItemDto): string {
    const fromName = email.fromName || email.fromEmail || 'Unknown Sender';
    const fromEmail = email.fromEmail || '';
    const sentDate = email.sentAt ? new Date(email.sentAt).toLocaleString() : 'Unknown Date';
    const subject = email.subject || '(No Subject)';
    const body = email.htmlBody || email.textBody || '(No content)';

    // Format as professional Outlook-style email quote - clean and simple
    const quotedHeader = `
      <div style="margin-top: 20px; font-family: Calibri, Arial, sans-serif; font-size: 11pt;">
        <hr style="border: none; border-top: 1px solid #e1e1e1; margin: 15px 0;">
        <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>From:</strong> ${fromName} ${fromEmail ? `&lt;${fromEmail}&gt;` : ''}</p>
        <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>Sent:</strong> ${sentDate}</p>
        <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>Subject:</strong> ${subject}</p>
        <br>
        <div style="font-family: Calibri, Arial, sans-serif; font-size: 11pt; color: #000000;">
          ${body}
        </div>
      </div>
    `;

    return quotedHeader;
  }

  private formatEntireThreadAsQuoted(emails: EmailThreadItemDto[]): string {
    // Sort emails by date (oldest first) to show conversation chronologically
    const sortedEmails = [...emails].sort((a, b) => {
      const dateA = new Date(a.sentAt || 0).getTime();
      const dateB = new Date(b.sentAt || 0).getTime();
      return dateA - dateB;
    });

    // Format each email in the thread - Outlook style, clean and professional
    const formattedEmails = sortedEmails.map((email, index) => {
      const fromName = email.fromName || email.fromEmail || 'Unknown Sender';
      const fromEmail = email.fromEmail || '';
      const sentDate = email.sentAt ? new Date(email.sentAt).toLocaleString() : 'Unknown Date';
      const subject = email.subject || '(No Subject)';
      const body = email.htmlBody || email.textBody || '(No content)';
      const direction = email.isOutbound ? 'Sent' : 'Received';

      return `
        <div style="margin: 25px 0; font-family: Calibri, Arial, sans-serif; font-size: 11pt;">
          ${index > 0 ? '<hr style="border: none; border-top: 1px solid #e1e1e1; margin: 20px 0;">' : ''}
          <p style="margin: 0 0 10px 0; padding: 0; color: #0066cc; font-weight: bold;">
            ${direction} - Message ${index + 1} of ${sortedEmails.length}
          </p>
          <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>From:</strong> ${fromName} ${fromEmail ? `&lt;${fromEmail}&gt;` : ''}</p>
          <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>Sent:</strong> ${sentDate}</p>
          <p style="margin: 0; padding: 0; line-height: 1.5;"><strong>Subject:</strong> ${subject}</p>
          <br>
          <div style="font-family: Calibri, Arial, sans-serif; font-size: 11pt; color: #000000;">
            ${body}
          </div>
        </div>
      `;
    }).join('');

    return `
      <div style="margin-top: 20px; font-family: Calibri, Arial, sans-serif; font-size: 11pt;">
        <hr style="border: none; border-top: 2px solid #0066cc; margin: 15px 0;">
        <p style="margin: 10px 0; padding: 0; color: #0066cc; font-weight: bold; font-size: 12pt;">
          Forwarded Conversation (${sortedEmails.length} message${sortedEmails.length > 1 ? 's' : ''})
        </p>
        ${formattedEmails}
      </div>
    `;
  }

  // ==================== RECIPIENT MANAGEMENT ====================

  addToRecipient(): void {
    this.addRecipient(this.toInputValue, this.toRecipients);
    this.toInputValue = '';
  }

  addCcRecipient(): void {
    this.addRecipient(this.ccInputValue, this.ccRecipients);
    this.ccInputValue = '';
  }

  addBccRecipient(): void {
    this.addRecipient(this.bccInputValue, this.bccRecipients);
    this.bccInputValue = '';
  }

  private addRecipient(email: string, recipientList: EmailRecipient[]): void {
    const trimmedEmail = email.trim();

    if (!trimmedEmail) return;

    // Validate email format
    if (!this.isValidEmail(trimmedEmail)) {
      this.showError('Invalid email address format');
      return;
    }

    // Check for duplicates
    if (recipientList.some(r => r.emailAddress.toLowerCase() === trimmedEmail.toLowerCase())) {
      this.showError('Email address already added');
      return;
    }

    recipientList.push({
      emailAddress: trimmedEmail,
      displayName: undefined
    });
  }

  removeToRecipient(index: number): void {
    this.toRecipients.splice(index, 1);
  }

  removeCcRecipient(index: number): void {
    this.ccRecipients.splice(index, 1);
  }

  removeBccRecipient(index: number): void {
    this.bccRecipients.splice(index, 1);
  }

  private removeDuplicateRecipients(recipients: EmailRecipient[]): EmailRecipient[] {
    const seen = new Set<string>();
    return recipients.filter(r => {
      const email = r.emailAddress.toLowerCase();
      if (seen.has(email)) {
        return false;
      }
      seen.add(email);
      return true;
    });
  }

  onToEmailKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addToRecipient();
    }
  }

  onCcEmailKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addCcRecipient();
    }
  }

  onBccEmailKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addBccRecipient();
    }
  }

  toggleCc(): void {
    this.showCc = !this.showCc;
  }

  toggleBcc(): void {
    this.showBcc = !this.showBcc;
  }

  // ==================== CANNED RESPONSES ====================

  private loadCannedResponses(): void {
    this.loadingCannedResponses = true;

    this.emailThreadService.getCannedResponses()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.cannedResponses = response.data;
            this.logger.info('Loaded canned responses', { count: this.cannedResponses.length });
          }
          this.loadingCannedResponses = false;
        },
        error: (error: Error) => {
          this.logger.error('Failed to load canned responses', error);
          this.loadingCannedResponses = false;
        }
      });
  }

  onCannedResponseSelected(cannedResponseId: string): void {
    if (!cannedResponseId) return;

    const cannedResponse = this.cannedResponses.find(r => r.id === cannedResponseId);
    if (!cannedResponse) return;

    this.selectedCannedResponse = cannedResponse;

    // Populate form with canned response
    this.emailForm.patchValue({
      subject: cannedResponse.subject || this.emailForm.value.subject,
      body: cannedResponse.body
    });

    this.logger.info('Applied canned response', { cannedResponseId: cannedResponse.id });
  }

  // ==================== FORM SUBMISSION ====================

  async onSubmit(): Promise<void> {
    if (this.isSending) return;

    // Validate form
    if (!this.validateForm()) {
      return;
    }

    this.isSending = true;
    this.hasError = false;
    this.errorMessage = '';
    this.successMessage = '';

    const request: SendEmailReplyRequest = {
      complaintId: this.complaintId,
      inReplyToEmailMessageId: this.replyTo?.id || null,
      replyType: this.replyType,
      toRecipients: this.toRecipients,
      ccRecipients: this.ccRecipients,
      bccRecipients: this.bccRecipients,
      subject: this.emailForm.value.subject,
      htmlBody: this.emailForm.value.body,
      plainTextBody: this.stripHtml(this.emailForm.value.body),
      isPrivateNote: this.emailForm.value.isPrivateNote || false
    };

    this.emailThreadService.sendReply(this.complaintId, request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isSending = false;

          if (response.isSuccess && response.data) {
            this.successMessage = 'Email sent successfully!';
            this.logger.info('Email sent successfully', { emailId: response.data.id });

            // Emit event
            this.emailSent.emit(response.data);

            // Reset form
            setTimeout(() => this.resetForm(), 1000);
          } else {
            this.showError(response.message || 'Failed to send email');
          }
        },
        error: (error: Error) => {
          this.isSending = false;
          this.logger.error('Failed to send email', error);
          this.showError('An error occurred while sending the email');
        }
      });
  }

  private validateForm(): boolean {
    // Check if form is valid
    if (!this.emailForm.valid) {
      this.showError('Please fill in all required fields');
      return false;
    }

    // For non-private notes, check recipients
    if (!this.emailForm.value.isPrivateNote && this.replyType !== ReplyType.PrivateNote) {
      if (this.toRecipients.length === 0) {
        this.showError('Please add at least one recipient');
        return false;
      }

      // Validate all email addresses
      const allRecipients = [...this.toRecipients, ...this.ccRecipients, ...this.bccRecipients];
      for (const recipient of allRecipients) {
        if (!this.isValidEmail(recipient.emailAddress)) {
          this.showError(`Invalid email address: ${recipient.emailAddress}`);
          return false;
        }
      }
    }

    return true;
  }

  onCancel(): void {
    // Only confirm if there's actual content
    const hasContent = this.emailForm.dirty && (
      this.emailForm.value.subject?.trim() ||
      this.emailForm.value.body?.trim() ||
      this.toRecipients.length > 0 ||
      this.ccRecipients.length > 0 ||
      this.bccRecipients.length > 0
    );

    if (hasContent) {
      if (confirm('Are you sure you want to discard your changes?')) {
        this.resetForm();
        this.cancelled.emit();
      }
    } else {
      this.resetForm();
      this.cancelled.emit();
    }
  }

  private resetForm(): void {
    this.emailForm.reset();
    this.toRecipients = [];
    this.ccRecipients = [];
    this.bccRecipients = [];
    this.toInputValue = '';
    this.ccInputValue = '';
    this.bccInputValue = '';
    this.showCc = false;
    this.showBcc = false;
    this.hasError = false;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // ==================== AUTO-SAVE ====================

  private setupAutoSave(): void {
    // Auto-save disabled - not currently implemented in parent components
    // TODO: Implement draft storage if needed
    // this.autoSaveTimer = setInterval(() => {
    //   this.saveDraft();
    // }, this.autoSaveInterval);
  }

  private clearAutoSave(): void {
    if (this.autoSaveTimer) {
      clearInterval(this.autoSaveTimer);
      this.autoSaveTimer = undefined;
    }
  }

  private saveDraft(): void {
    if (!this.emailForm.dirty) return;

    const draft: SendEmailReplyRequest = {
      complaintId: this.complaintId,
      inReplyToEmailMessageId: this.replyTo?.id || null,
      replyType: this.replyType,
      toRecipients: this.toRecipients,
      ccRecipients: this.ccRecipients,
      bccRecipients: this.bccRecipients,
      subject: this.emailForm.value.subject || '',
      htmlBody: this.emailForm.value.body || '',
      isPrivateNote: this.emailForm.value.isPrivateNote || false
    };

    this.draftSaved.emit(draft);
    this.logger.info('Draft saved', { complaintId: this.complaintId });
  }

  // ==================== UTILITY METHODS ====================

  private showError(message: string): void {
    this.hasError = true;
    this.errorMessage = message;
    this.successMessage = '';

    // Auto-hide error after 5 seconds
    setTimeout(() => {
      this.hasError = false;
      this.errorMessage = '';
    }, 5000);
  }

  private isValidEmail(email: string): boolean {
    if (!email) return false;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private stripHtml(html: string): string {
    if (!html) return '';
    const tmp = document.createElement('DIV');
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || '';
  }

  getReplyTypeLabel(): string {
    switch (this.replyType) {
      case ReplyType.Reply:
        return 'Reply';
      case ReplyType.ReplyAll:
        return 'Reply All';
      case ReplyType.Forward:
        return 'Forward';
      case ReplyType.NewEmail:
        return 'New Email';
      case ReplyType.PrivateNote:
        return 'Private Note';
      default:
        return 'Compose';
    }
  }
}
