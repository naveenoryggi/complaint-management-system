// Email Server Settings
export interface EmailServerSettings {
  id: string;
  name: string;
  code: string; // Use host:port as code for base class compatibility
  authenticationType: number; // 0 = Basic, 1 = OAuth2
  host: string;
  port: number;
  useSsl: boolean;
  username: string;
  password?: string;
  fromEmail: string;
  fromName: string;
  isActive: boolean;
  isDefault: boolean;
  maxEmailsPerHour?: number;
  timeoutSeconds: number;
  companyId?: string;
  testNotes?: string;
  lastTestedAt?: string;
  // OAuth 2.0 fields
  oauthClientId?: string;
  oauthClientSecret?: string;
  oauthTenantId?: string;
  oAuthAccessToken?: string;
  oAuthRefreshToken?: string;
  oAuthTokenExpiresAt?: string;
  oauthScopes?: string;
  oauthTokenRefreshIntervalMinutes?: number;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface CreateEmailServerSettingsRequest {
  name: string;
  code: string; // Generated from host:port
  authenticationType: number; // 0 = Basic, 1 = OAuth2
  host: string;
  port: number;
  useSsl: boolean;
  username: string;
  password: string;
  fromEmail: string;
  fromName: string;
  isActive: boolean;
  isDefault: boolean;
  maxEmailsPerHour?: number;
  timeoutSeconds: number;
  companyId?: string;
  // OAuth 2.0 fields
  oauthClientId?: string;
  oauthClientSecret?: string;
  oauthTenantId?: string;
  oauthScopes?: string;
  oauthTokenRefreshIntervalMinutes?: number;
}

export interface UpdateEmailServerSettingsRequest {
  id: string;
  name: string;
  code: string; // Generated from host:port
  authenticationType: number; // 0 = Basic, 1 = OAuth2
  host: string;
  port: number;
  useSsl: boolean;
  username: string;
  password?: string;
  fromEmail: string;
  fromName: string;
  isActive: boolean;
  isDefault: boolean;
  maxEmailsPerHour?: number;
  timeoutSeconds: number;
  testNotes?: string;
  // OAuth 2.0 fields
  oauthClientId?: string;
  oauthClientSecret?: string;
  oauthTenantId?: string;
  oauthScopes?: string;
  oauthTokenRefreshIntervalMinutes?: number;
}

// Communication Template
export interface CommunicationTemplate {
  id: string;
  name: string;
  code: string;
  description?: string;
  channel: CommunicationChannel;
  subject?: string;
  body: string;
  htmlBody?: string;
  isActive: boolean;
  isSystem: boolean;
  companyId?: string;
  category?: string;
  language?: string;
  availablePlaceholders?: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface CreateCommunicationTemplateRequest {
  name: string;
  code: string;
  description?: string;
  channel: CommunicationChannel;
  subject?: string;
  body: string;
  htmlBody?: string;
  isActive: boolean;
  companyId?: string;
}

export interface UpdateCommunicationTemplateRequest {
  id: string;
  name: string;
  code: string;
  description?: string;
  channel: CommunicationChannel;
  subject?: string;
  body: string;
  htmlBody?: string;
  isActive: boolean;
  category?: string;
  language?: string;
  availablePlaceholders?: string;
}

// Notification Rule
export interface NotificationRule {
  id: string;
  name: string;
  description?: string;
  eventTypeId: string;
  channel: CommunicationChannel;
  templateId: string;
  recipientType: RecipientType;
  specificUserIds?: string[];
  specificRoleIds?: string[];
  specificEmails?: string[];
  conditions?: string;
  isActive: boolean;
  priority: number;
  delayMinutes: number;
  sendOnlyOnce: boolean;
  companyId?: string;
  additionalData?: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface CreateNotificationRuleRequest {
  name: string;
  description?: string;
  eventTypeId: string;
  channel: CommunicationChannel;
  templateId: string;
  recipientType: RecipientType;
  specificUserIds?: string[];
  specificRoleIds?: string[];
  specificEmails?: string[];
  conditions?: string;
  isActive: boolean;
  priority: number;
  delayMinutes: number;
  sendOnlyOnce: boolean;
  companyId?: string;
  additionalData?: string;
}

export interface UpdateNotificationRuleRequest {
  id: string;
  name: string;
  description?: string;
  eventTypeId: string;
  channel: CommunicationChannel;
  templateId: string;
  recipientType: RecipientType;
  specificUserIds?: string[];
  specificRoleIds?: string[];
  specificEmails?: string[];
  conditions?: string;
  isActive: boolean;
  priority: number;
  delayMinutes: number;
  sendOnlyOnce: boolean;
  additionalData?: string;
}

// Event Type
export interface EventType {
  id: string;
  code: string;
  name: string;
  entityType: string;
  description?: string;
  category?: string;
  availableFields?: string[];
  isSystem: boolean;
  isActive: boolean;
  companyId?: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

// Enums
export enum CommunicationChannel {
  Email = 0,
  SMS = 1,
  WhatsApp = 2,
  InApp = 3
}

export enum RecipientType {
  Complainant = 0,
  AssignedHandler = 1,
  ComplainantManager = 2,
  ComplainantDepartmentManager = 3,
  EscalationHandler = 4,
  SpecificUsers = 5,
  SpecificRoles = 6,
  SpecificEmails = 7,
  CompanyPrimaryManager = 11,
  CompanySecondaryManager = 12,
  BranchPrimaryManager = 16,
  BranchSecondaryManager = 17,
  DepartmentPrimaryManager = 21,
  DepartmentSecondaryManager = 22,
  SectionHead = 25
}

// Helper functions
export function getCommunicationChannelLabel(channel: CommunicationChannel): string {
  switch (channel) {
    case CommunicationChannel.Email:
      return 'Email';
    case CommunicationChannel.SMS:
      return 'SMS';
    case CommunicationChannel.WhatsApp:
      return 'WhatsApp';
    case CommunicationChannel.InApp:
      return 'In-App';
    default:
      return 'Unknown';
  }
}

export function getRecipientTypeLabel(recipientType: RecipientType): string {
  switch (recipientType) {
    case RecipientType.Complainant:
      return 'Complainant';
    case RecipientType.AssignedHandler:
      return 'Assigned Handler';
    case RecipientType.ComplainantManager:
      return 'Complainant Manager';
    case RecipientType.ComplainantDepartmentManager:
      return 'Complainant Department Manager';
    case RecipientType.EscalationHandler:
      return 'Escalation Handler';
    case RecipientType.SpecificUsers:
      return 'Specific Users';
    case RecipientType.SpecificRoles:
      return 'Specific Roles';
    case RecipientType.SpecificEmails:
      return 'Specific Emails';
    case RecipientType.CompanyPrimaryManager:
      return 'Company Primary Manager';
    case RecipientType.CompanySecondaryManager:
      return 'Company Secondary Manager';
    case RecipientType.BranchPrimaryManager:
      return 'Branch Primary Manager';
    case RecipientType.BranchSecondaryManager:
      return 'Branch Secondary Manager';
    case RecipientType.DepartmentPrimaryManager:
      return 'Department Primary Manager';
    case RecipientType.DepartmentSecondaryManager:
      return 'Department Secondary Manager';
    case RecipientType.SectionHead:
      return 'Section Head';
    default:
      return 'Unknown';
  }
}

// ================================
// EMAIL TICKETING SYSTEM (NEW)
// ================================

// Email Configuration - IMAP/SMTP settings for email ticketing
export interface EmailConfiguration {
  id: string;
  companyId: string;
  authenticationType: number; // 0 = Basic, 1 = OAuth
  imapHost: string;
  imapPort: number;
  imapUseSsl: boolean;
  imapUsername: string;
  imapPassword: string;
  imapFolder: string;
  smtpHost: string;
  smtpPort: number;
  smtpUseSsl: boolean;
  smtpUsername: string;
  smtpPassword: string;
  fromEmail: string;
  fromName: string;
  pollingIntervalMinutes: number;
  pollingIntervalSeconds?: number; // Takes precedence over minutes if set
  isEnabled: boolean;
  lastPolledAt?: string;
  sendAutoAcknowledgement: boolean;
  autoAcknowledgementTemplateId?: string;
  enableThreading: boolean;
  threadTimeoutDays: number;
  maxAttachmentSizeBytes: number;
  allowedAttachmentExtensions: string;
  oauthClientId?: string;
  oauthTenantId?: string;
  oauthClientSecret?: string;
  oAuthAccessToken?: string;
  oAuthTokenExpiresAt?: string;
  oauthTokenRefreshIntervalMinutes?: number; // How often to refresh OAuth tokens for this account (5-120 min). If null, uses system default
  // Separate SMTP Account Support
  useSeparateSmtpAccount: boolean; // Use different account for sending vs receiving
  smtpAuthenticationType?: number; // Separate SMTP auth type (0 = Basic, 1 = OAuth)
  smtpSeparateUsername?: string;
  smtpSeparatePassword?: string;
  smtpSeparateFromEmail?: string;
  smtpSeparateFromName?: string;
  smtpSeparateOAuthClientId?: string;
  smtpSeparateOAuthClientSecret?: string;
  smtpSeparateOAuthTenantId?: string;
  smtpSeparateOAuthAccessToken?: string;
  smtpSeparateOAuthRefreshToken?: string;
  smtpSeparateOAuthTokenExpiresAt?: string;
  smtpSeparateOAuthScopes?: string;
  createdAt: string;
  updatedAt?: string;
  isDeleted: boolean;
}

export interface CreateEmailConfigurationRequest {
  authenticationType: number; // 0 = Basic, 1 = OAuth
  imapHost: string;
  imapPort: number;
  imapUseSsl: boolean;
  imapUsername: string;
  imapPassword: string;
  imapFolder: string;
  smtpHost: string;
  smtpPort: number;
  smtpUseSsl: boolean;
  smtpUsername: string;
  smtpPassword: string;
  fromEmail: string;
  fromName: string;
  pollingIntervalMinutes: number;
  pollingIntervalSeconds?: number; // Takes precedence over minutes if set
  isEnabled: boolean;
  sendAutoAcknowledgement: boolean;
  autoAcknowledgementTemplateId?: string;
  enableThreading?: boolean;
  threadTimeoutDays?: number;
  maxAttachmentSizeBytes?: number;
  allowedAttachmentExtensions?: string;
  oauthClientId?: string;
  oauthTenantId?: string;
  oauthClientSecret?: string;
  oauthTokenRefreshIntervalMinutes?: number; // How often to refresh OAuth tokens for this account (5-120 min). If null, uses system default
  // Separate SMTP Account Support
  useSeparateSmtpAccount?: boolean;
  smtpAuthenticationType?: number;
  smtpSeparateUsername?: string;
  smtpSeparatePassword?: string;
  smtpSeparateFromEmail?: string;
  smtpSeparateFromName?: string;
  smtpSeparateOAuthClientId?: string;
  smtpSeparateOAuthClientSecret?: string;
  smtpSeparateOAuthTenantId?: string;
  smtpSeparateOAuthScopes?: string;
}

export interface UpdateEmailConfigurationRequest {
  id: string;
  authenticationType: number; // 0 = Basic, 1 = OAuth
  imapHost: string;
  imapPort: number;
  imapUseSsl: boolean;
  imapUsername: string;
  imapPassword?: string;
  imapFolder: string;
  smtpHost: string;
  smtpPort: number;
  smtpUseSsl: boolean;
  smtpUsername: string;
  smtpPassword?: string;
  fromEmail: string;
  fromName: string;
  pollingIntervalMinutes: number;
  pollingIntervalSeconds?: number; // Takes precedence over minutes if set
  isEnabled: boolean;
  sendAutoAcknowledgement: boolean;
  autoAcknowledgementTemplateId?: string;
  enableThreading?: boolean;
  threadTimeoutDays?: number;
  maxAttachmentSizeBytes?: number;
  allowedAttachmentExtensions?: string;
  oauthClientId?: string;
  oauthTenantId?: string;
  oauthClientSecret?: string;
  oauthTokenRefreshIntervalMinutes?: number; // How often to refresh OAuth tokens for this account (5-120 min). If null, uses system default
  // Separate SMTP Account Support
  useSeparateSmtpAccount?: boolean;
  smtpAuthenticationType?: number;
  smtpSeparateUsername?: string;
  smtpSeparatePassword?: string;
  smtpSeparateFromEmail?: string;
  smtpSeparateFromName?: string;
  smtpSeparateOAuthClientId?: string;
  smtpSeparateOAuthClientSecret?: string;
  smtpSeparateOAuthTenantId?: string;
  smtpSeparateOAuthScopes?: string;
}

// Email Message - Stores inbound/outbound emails linked to complaints
export interface EmailMessage {
  id: string;
  companyId: string;
  complaintId?: string;
  messageId: string;
  inReplyTo?: string;
  references?: string;
  subject: string;
  fromEmail: string;
  fromName: string;
  toEmail: string;
  toName?: string;
  ccEmails?: string;
  bccEmails?: string;
  textBody: string;
  htmlBody?: string;
  isHtml: boolean;
  direction: EmailDirection;
  status: EmailStatus;
  receivedAt: string;
  processedAt: string;
  sentAt?: string;
  sentByUserId?: string;
  threadId?: string;
  threadPosition: number;
  isAutoAcknowledgement: boolean;
  isInternal: boolean;
  isRead: boolean;
  isSpam: boolean;
  failed: boolean;
  failureReason?: string;
  rawHeaders?: string;
}

// Email Attachment - Stores email attachment metadata
export interface EmailAttachment {
  id: string;
  emailMessageId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  fileExtension: string;
  storagePath: string;
  isInline: boolean;
  contentId?: string;
  isScanned: boolean;
  isSafe: boolean;
  scanResult?: string;
  isDeleted: boolean;
}

// Reply Type Enum
export enum ReplyType {
  Reply = 1,           // Reply to sender only
  ReplyAll = 2,        // Reply to all participants (To + CC)
  Forward = 3,         // Forward to new recipients
  NewEmail = 4,        // New email in thread
  PrivateNote = 5      // Internal note only (not sent via email)
}

// Email Recipient
export interface EmailRecipient {
  emailAddress: string;
  displayName?: string;
}

// Send Email Reply Request
export interface SendEmailReplyRequest {
  complaintId: string;
  inReplyToEmailMessageId?: string | null;
  replyType: ReplyType;
  toRecipients: EmailRecipient[];
  ccRecipients: EmailRecipient[];
  bccRecipients: EmailRecipient[];
  subject: string;
  htmlBody: string;
  plainTextBody?: string;
  isPrivateNote: boolean;
}

// Email Statistics
export interface EmailStatistics {
  totalEmails: number;
  inboundEmails: number;
  outboundEmails: number;
  processedEmails: number;
  failedEmails: number;
  spamEmails: number;
  internalNotes: number;
  emailsLast24Hours: number;
  emailsLast7Days: number;
  emailsLast30Days: number;
}

// Email Ticketing Enums
export enum EmailDirection {
  Inbound = 0,
  Outbound = 1
}

export enum EmailStatus {
  Pending = 0,
  Processed = 1,
  Failed = 2,
  Sent = 3,
  Ignored = 4
}

// Email Provider Presets
export interface EmailProviderPreset {
  name: string;
  icon: string;
  imapHost: string;
  imapPort: number;
  imapUseSsl: boolean;
  smtpHost: string;
  smtpPort: number;
  smtpUseSsl: boolean;
  notes?: string;
}

export const EMAIL_PROVIDER_PRESETS: EmailProviderPreset[] = [
  {
    name: 'Gmail',
    icon: 'fab fa-google',
    imapHost: 'imap.gmail.com',
    imapPort: 993,
    imapUseSsl: true,
    smtpHost: 'smtp.gmail.com',
    smtpPort: 587,
    smtpUseSsl: true,
    notes: 'Requires App Password if 2FA is enabled'
  },
  {
    name: 'Outlook/Office 365',
    icon: 'fab fa-microsoft',
    imapHost: 'outlook.office365.com',
    imapPort: 993,
    imapUseSsl: true,
    smtpHost: 'smtp.office365.com',
    smtpPort: 587,
    smtpUseSsl: true,
    notes: 'Modern Auth (OAuth2) supported'
  },
  {
    name: 'Yahoo Mail',
    icon: 'fab fa-yahoo',
    imapHost: 'imap.mail.yahoo.com',
    imapPort: 993,
    imapUseSsl: true,
    smtpHost: 'smtp.mail.yahoo.com',
    smtpPort: 587,
    smtpUseSsl: true,
    notes: 'Requires App Password'
  },
  {
    name: 'GoDaddy',
    icon: 'fas fa-server',
    imapHost: 'imap.secureserver.net',
    imapPort: 993,
    imapUseSsl: true,
    smtpHost: 'smtpout.secureserver.net',
    smtpPort: 465,
    smtpUseSsl: true,
    notes: 'Use Workspace email account'
  },
  {
    name: 'Custom IMAP/SMTP',
    icon: 'fas fa-cog',
    imapHost: '',
    imapPort: 993,
    imapUseSsl: true,
    smtpHost: '',
    smtpPort: 587,
    smtpUseSsl: true,
    notes: 'Configure custom IMAP/SMTP server'
  }
];

// Helper functions for Email Ticketing
export function getEmailDirectionLabel(direction: EmailDirection): string {
  switch (direction) {
    case EmailDirection.Inbound:
      return 'Inbound';
    case EmailDirection.Outbound:
      return 'Outbound';
    default:
      return 'Unknown';
  }
}

export function getEmailStatusLabel(status: EmailStatus): string {
  switch (status) {
    case EmailStatus.Pending:
      return 'Pending';
    case EmailStatus.Processed:
      return 'Processed';
    case EmailStatus.Failed:
      return 'Failed';
    case EmailStatus.Sent:
      return 'Sent';
    case EmailStatus.Ignored:
      return 'Ignored';
    default:
      return 'Unknown';
  }
}

export function getEmailStatusColor(status: EmailStatus): string {
  switch (status) {
    case EmailStatus.Pending:
      return 'warning';
    case EmailStatus.Processed:
      return 'success';
    case EmailStatus.Failed:
      return 'danger';
    case EmailStatus.Sent:
      return 'success';
    case EmailStatus.Ignored:
      return 'secondary';
    default:
      return 'secondary';
  }
}

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
}
