/**
 * System-wide configuration model
 * Matches the backend SystemConfiguration entity
 */
export interface SystemConfiguration {
  id: string;
  companyId: string;

  // OAuth Token Management Settings
  oAuthTokenRefreshIntervalMinutes: number; // 5-120 minutes
  oAuthTokenExpiryWarningDays: number; // 1-30 days

  // Email Polling Settings
  defaultEmailPollingIntervalSeconds: number; // 60-3600 seconds
  maxEmailsFetchPerPoll: number; // 10-500

  // Auto-Response Settings
  autoResponseEnabled: boolean;
  autoResponseMaxRetryAttempts: number; // 0-10
  autoResponseRetryDelaySeconds: number; // 30-600 seconds

  // Rate Limiting Settings
  emailRateLimitingEnabled: boolean;
  maxEmailsPerHour: number; // 10-1000

  // Notification Settings
  statusChangeNotificationsEnabled: boolean;
  assignmentNotificationsEnabled: boolean;
  escalationNotificationsEnabled: boolean;

  // Timezone Settings
  defaultTimezone: string; // IANA timezone
  dateFormat: string; // e.g., "dd/MM/yyyy"
  timeFormat: string; // e.g., "hh:mm tt"

  // Audit fields
  createdAt: string;
  updatedAt?: string | null;
  createdBy?: string | null;
  updatedBy?: string | null;
}

/**
 * Validation result for system configuration
 */
export interface SystemConfigurationValidation {
  isValid: boolean;
  errors: string[];
}

/**
 * Validate system configuration before saving
 */
export function validateSystemConfiguration(config: SystemConfiguration): SystemConfigurationValidation {
  const errors: string[] = [];

  // OAuth Token Refresh Interval
  if (config.oAuthTokenRefreshIntervalMinutes < 5 || config.oAuthTokenRefreshIntervalMinutes > 120) {
    errors.push('OAuth Token Refresh Interval must be between 5 and 120 minutes');
  }

  // OAuth Token Expiry Warning
  if (config.oAuthTokenExpiryWarningDays < 1 || config.oAuthTokenExpiryWarningDays > 30) {
    errors.push('OAuth Token Expiry Warning must be between 1 and 30 days');
  }

  // Email Polling Interval
  if (config.defaultEmailPollingIntervalSeconds < 60 || config.defaultEmailPollingIntervalSeconds > 3600) {
    errors.push('Default Email Polling Interval must be between 60 and 3600 seconds (1 minute - 1 hour)');
  }

  // Max Emails Fetch
  if (config.maxEmailsFetchPerPoll < 10 || config.maxEmailsFetchPerPoll > 500) {
    errors.push('Max Emails Fetch Per Poll must be between 10 and 500');
  }

  // Auto-Response Retry
  if (config.autoResponseMaxRetryAttempts < 0 || config.autoResponseMaxRetryAttempts > 10) {
    errors.push('Auto-Response Max Retry Attempts must be between 0 and 10');
  }

  // Auto-Response Retry Delay
  if (config.autoResponseRetryDelaySeconds < 30 || config.autoResponseRetryDelaySeconds > 600) {
    errors.push('Auto-Response Retry Delay must be between 30 and 600 seconds');
  }

  // Max Emails Per Hour
  if (config.maxEmailsPerHour < 10 || config.maxEmailsPerHour > 1000) {
    errors.push('Max Emails Per Hour must be between 10 and 1000');
  }

  // Timezone
  if (!config.defaultTimezone || config.defaultTimezone.trim() === '') {
    errors.push('Default Timezone is required');
  }

  // Date Format
  if (!config.dateFormat || config.dateFormat.trim() === '') {
    errors.push('Date Format is required');
  }

  // Time Format
  if (!config.timeFormat || config.timeFormat.trim() === '') {
    errors.push('Time Format is required');
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}
