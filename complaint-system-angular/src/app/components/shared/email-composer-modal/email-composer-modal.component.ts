import { Component, EventEmitter, Input, Output, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { EmailThreadService } from '../../../services/email-thread.service';
import { SendEmailReplyRequest, ReplyType } from '../../../models/communication.model';

@Component({
  selector: 'app-email-composer-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './email-composer-modal.component.html',
  styleUrls: ['./email-composer-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailComposerModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly emailThreadService = inject(EmailThreadService);

  @Input() complaintId!: string;
  @Input() defaultTo: string = '';
  @Input() defaultSubject: string = '';
  @Input() replyToEmailId?: string;
  @Input() isVisible: boolean = false;

  @Output() closeModal = new EventEmitter<void>();
  @Output() emailSent = new EventEmitter<void>();

  emailForm!: FormGroup;
  isSending: boolean = false;
  sendError: string | null = null;
  sendSuccess: boolean = false;
  showCc: boolean = false;
  showBcc: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.emailForm = this.fb.group({
      to: [this.defaultTo, [Validators.required, Validators.email]],
      cc: [''],
      bcc: [''],
      subject: [this.defaultSubject, Validators.required],
      body: ['', Validators.required],
      isHtml: [false]
    });
  }

  toggleCc(): void {
    this.showCc = !this.showCc;
    if (!this.showCc) {
      this.emailForm.patchValue({ cc: '' });
    }
  }

  toggleBcc(): void {
    this.showBcc = !this.showBcc;
    if (!this.showBcc) {
      this.emailForm.patchValue({ bcc: '' });
    }
  }

  onSend(): void {
    if (this.emailForm.invalid) {
      Object.keys(this.emailForm.controls).forEach(key => {
        this.emailForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isSending = true;
    this.sendError = null;
    this.sendSuccess = false;

    const formValue = this.emailForm.value;

    const request: SendEmailReplyRequest = {
      complaintId: this.complaintId,
      inReplyToEmailMessageId: this.replyToEmailId || null,
      replyType: ReplyType.Reply,
      subject: formValue.subject,
      htmlBody: formValue.body,
      toRecipients: [{ emailAddress: formValue.to }],
      ccRecipients: formValue.cc ? [{ emailAddress: formValue.cc }] : [],
      bccRecipients: formValue.bcc ? [{ emailAddress: formValue.bcc }] : [],
      isPrivateNote: false
    };

    // Validate request
    const validation = this.emailThreadService.validateEmailReply(request);
    if (!validation.isValid) {
      this.sendError = validation.errors.join(', ');
      this.isSending = false;
      return;
    }

    this.emailThreadService.sendReply(this.complaintId, request).subscribe({
      next: (response: any) => {
        this.isSending = false;
        if (response.isSuccess) {
          this.sendSuccess = true;
          this.emailSent.emit();
          setTimeout(() => {
            this.onClose();
          }, 1500);
        } else {
          this.sendError = response.message || 'Failed to send email';
        }
      },
      error: (error: any) => {
        this.isSending = false;
        this.sendError = error.error?.message || error.message || 'An error occurred while sending email';
      }
    });
  }

  onClose(): void {
    this.emailForm.reset();
    this.showCc = false;
    this.showBcc = false;
    this.sendError = null;
    this.sendSuccess = false;
    this.closeModal.emit();
  }

  getErrorMessage(fieldName: string): string {
    const control = this.emailForm.get(fieldName);
    if (!control || !control.touched) return '';

    if (control.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (control.hasError('email')) {
      return 'Please enter a valid email address';
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      to: 'To',
      cc: 'CC',
      bcc: 'BCC',
      subject: 'Subject',
      body: 'Message'
    };
    return labels[fieldName] || fieldName;
  }

  hasError(fieldName: string): boolean {
    const control = this.emailForm.get(fieldName);
    return !!(control && control.invalid && control.touched);
  }
}
