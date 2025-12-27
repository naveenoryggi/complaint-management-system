// Notification Types
export enum NotificationType {
  COMPLAINT_ASSIGNED = 'COMPLAINT_ASSIGNED',
  COMPLAINT_REASSIGNED = 'COMPLAINT_REASSIGNED',
  STATUS_CHANGED = 'STATUS_CHANGED',
  COMMENT_ADDED = 'COMMENT_ADDED',
  SLA_WARNING = 'SLA_WARNING',
  SLA_BREACH = 'SLA_BREACH',
  ESCALATION = 'ESCALATION',
  OVERDUE = 'OVERDUE',
  PRIORITY_CHANGED = 'PRIORITY_CHANGED',
  BULK_IMPORT_COMPLETE = 'BULK_IMPORT_COMPLETE',
  SYSTEM_ALERT = 'SYSTEM_ALERT',
  EMAIL_RECEIVED = 'EMAIL_RECEIVED'
}

// Reference Types (what entity the notification links to)
export enum NotificationReferenceType {
  COMPLAINT = 'COMPLAINT',
  COMMENT = 'COMMENT',
  EMAIL_THREAD = 'EMAIL_THREAD',
  USER = 'USER',
  SYSTEM = 'SYSTEM'
}

// Notification Priority
export enum NotificationPriority {
  LOW = 0,
  NORMAL = 1,
  HIGH = 2,
  URGENT = 3
}

// Main Notification interface (named AppNotification to avoid conflict with browser's Notification API)
export interface AppNotification {
  id: string;
  userId: string;
  companyId: string;
  type: NotificationType;
  title: string;
  message: string;
  referenceType?: NotificationReferenceType;
  referenceId?: string;
  isRead: boolean;
  readAt?: Date;
  createdAt: Date;
  expiresAt?: Date;
  priority: NotificationPriority;
  metadata?: NotificationMetadata;
}

// Metadata for additional context
export interface NotificationMetadata {
  complaintNumber?: string;
  complaintSubject?: string;
  oldStatus?: string;
  newStatus?: string;
  assignedBy?: string;
  assignedTo?: string;
  commentPreview?: string;
  slaTimeRemaining?: number;
  escalationLevel?: number;
  [key: string]: any;
}

// API Response for notifications
export interface NotificationResponse {
  isSuccess: boolean;
  data?: AppNotification[];
  message?: string;
  errors?: string[];
}

export interface SingleNotificationResponse {
  isSuccess: boolean;
  data?: AppNotification;
  message?: string;
  errors?: string[];
}

// Notification count response
export interface NotificationCountResponse {
  isSuccess: boolean;
  data?: {
    total: number;
    unread: number;
    byType: { [key in NotificationType]?: number };
  };
  message?: string;
}

// Paginated notifications request
export interface NotificationQueryParams {
  page?: number;
  pageSize?: number;
  isRead?: boolean;
  type?: NotificationType;
  referenceType?: NotificationReferenceType;
  startDate?: string;
  endDate?: string;
  priority?: NotificationPriority;
}

// Paginated response
export interface PaginatedNotificationResponse {
  isSuccess: boolean;
  data?: {
    items: AppNotification[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
  message?: string;
}

// Create notification request (for backend)
export interface CreateNotificationRequest {
  userId?: string;
  roleCode?: string;  // For role-based notifications
  type: NotificationType;
  title: string;
  message: string;
  referenceType?: NotificationReferenceType;
  referenceId?: string;
  priority?: NotificationPriority;
  metadata?: NotificationMetadata;
}

// Mark notifications request
export interface MarkNotificationsRequest {
  notificationIds?: string[];
  markAll?: boolean;
  isRead: boolean;
}

// SignalR notification event
export interface NotificationEvent {
  action: 'new' | 'read' | 'deleted' | 'updated';
  notification?: AppNotification;
  notificationId?: string;
  unreadCount?: number;
}

// Notification preferences (user settings)
export interface NotificationPreferences {
  userId: string;
  emailNotifications: boolean;
  pushNotifications: boolean;
  inAppNotifications: boolean;
  mutedTypes: NotificationType[];
  quietHoursStart?: string;  // HH:mm format
  quietHoursEnd?: string;
}

// Helper functions
export function getNotificationIcon(type: NotificationType): string {
  const icons: { [key in NotificationType]: string } = {
    [NotificationType.COMPLAINT_ASSIGNED]: 'fa-user-plus',
    [NotificationType.COMPLAINT_REASSIGNED]: 'fa-user-gear',
    [NotificationType.STATUS_CHANGED]: 'fa-circle-check',
    [NotificationType.COMMENT_ADDED]: 'fa-comment',
    [NotificationType.SLA_WARNING]: 'fa-clock',
    [NotificationType.SLA_BREACH]: 'fa-triangle-exclamation',
    [NotificationType.ESCALATION]: 'fa-arrow-up',
    [NotificationType.OVERDUE]: 'fa-calendar-xmark',
    [NotificationType.PRIORITY_CHANGED]: 'fa-flag',
    [NotificationType.BULK_IMPORT_COMPLETE]: 'fa-file-import',
    [NotificationType.SYSTEM_ALERT]: 'fa-bell',
    [NotificationType.EMAIL_RECEIVED]: 'fa-envelope'
  };
  return icons[type] || 'fa-bell';
}

export function getNotificationColor(type: NotificationType): string {
  const colors: { [key in NotificationType]: string } = {
    [NotificationType.COMPLAINT_ASSIGNED]: '#3b82f6',    // Blue
    [NotificationType.COMPLAINT_REASSIGNED]: '#8b5cf6',  // Purple
    [NotificationType.STATUS_CHANGED]: '#10b981',        // Green
    [NotificationType.COMMENT_ADDED]: '#6366f1',         // Indigo
    [NotificationType.SLA_WARNING]: '#f59e0b',           // Amber
    [NotificationType.SLA_BREACH]: '#ef4444',            // Red
    [NotificationType.ESCALATION]: '#f97316',            // Orange
    [NotificationType.OVERDUE]: '#dc2626',               // Red
    [NotificationType.PRIORITY_CHANGED]: '#eab308',      // Yellow
    [NotificationType.BULK_IMPORT_COMPLETE]: '#22c55e',  // Green
    [NotificationType.SYSTEM_ALERT]: '#64748b',          // Slate
    [NotificationType.EMAIL_RECEIVED]: '#0ea5e9'         // Sky
  };
  return colors[type] || '#6b7280';
}

export function getPriorityLabel(priority: NotificationPriority): string {
  const labels: { [key in NotificationPriority]: string } = {
    [NotificationPriority.LOW]: 'Low',
    [NotificationPriority.NORMAL]: 'Normal',
    [NotificationPriority.HIGH]: 'High',
    [NotificationPriority.URGENT]: 'Urgent'
  };
  return labels[priority] || 'Normal';
}

export function getTypeLabel(type: NotificationType): string {
  const labels: { [key in NotificationType]: string } = {
    [NotificationType.COMPLAINT_ASSIGNED]: 'Complaint Assigned',
    [NotificationType.COMPLAINT_REASSIGNED]: 'Complaint Reassigned',
    [NotificationType.STATUS_CHANGED]: 'Status Changed',
    [NotificationType.COMMENT_ADDED]: 'New Comment',
    [NotificationType.SLA_WARNING]: 'SLA Warning',
    [NotificationType.SLA_BREACH]: 'SLA Breach',
    [NotificationType.ESCALATION]: 'Escalation',
    [NotificationType.OVERDUE]: 'Overdue',
    [NotificationType.PRIORITY_CHANGED]: 'Priority Changed',
    [NotificationType.BULK_IMPORT_COMPLETE]: 'Import Complete',
    [NotificationType.SYSTEM_ALERT]: 'System Alert',
    [NotificationType.EMAIL_RECEIVED]: 'Email Received'
  };
  return labels[type] || type;
}
