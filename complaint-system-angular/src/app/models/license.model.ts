/**
 * License Module enum - matches backend LicenseModule enum
 */
export enum LicenseModule {
  Core = 0,
  CRM_Customer = 1,
  CRM_Product = 2,
  CRM_Sales = 3,
  ServiceContract = 4,
  AssetManagement = 5,
  CustomerPortal = 6,
  AdvancedReporting = 7,
  APIAccess = 8,
  WhiteLabeling = 9,
  EmailTicketing = 10,
  MultiLanguage = 11,
  AdvancedSLA = 12,
  WorkflowAutomation = 13,
  CRM_Project = 14
}

/**
 * License Type enum
 */
export enum LicenseType {
  Trial = 0,
  Basic = 1,
  Standard = 2,
  Professional = 3,
  Enterprise = 4,
  Custom = 5
}

/**
 * License Status enum
 */
export enum LicenseStatus {
  Pending = 0,
  Active = 1,
  Expired = 2,
  Suspended = 3,
  Revoked = 4,
  GracePeriod = 5
}

/**
 * Module metadata DTO for UI display
 */
export interface ModuleMetadata {
  module: LicenseModule;
  code: string;
  name: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  displayOrder: number;
  isCore: boolean;
  features: string[];
}

/**
 * Module activation details
 */
export interface ModuleActivation {
  moduleCode: string;
  moduleName: string;
  description?: string;
  icon?: string;
  route?: string;
  isEnabled: boolean;
  enabledAt?: Date;
  expiresAt?: Date;
  displayOrder: number;
  moduleConfig?: string;
}

/**
 * Module configuration for UI
 */
export interface ModuleConfig {
  moduleCode: string;
  moduleName: string;
  description?: string;
  icon?: string;
  route?: string;
  color?: string;
  isEnabled: boolean;
  isAvailable: boolean;
  displayOrder: number;
  features: ModuleFeature[];
}

/**
 * Module feature details
 */
export interface ModuleFeature {
  featureCode: string;
  featureName: string;
  description?: string;
  isEnabled: boolean;
}

/**
 * License info DTO
 */
export interface LicenseInfo {
  id: string;
  companyId: string;
  name?: string;
  licenseType: string;
  status: string;
  issuedAt: Date;
  expiresAt?: Date;
  activatedAt?: Date;
  activatedBy?: string;
  isActive: boolean;
  daysRemaining: number;
  isExpired: boolean;
  isInGracePeriod: boolean;

  // Limits
  maxUsers?: number;
  maxCustomers?: number;
  maxProducts?: number;
  maxAssets?: number;
  maxContracts?: number;

  // Current usage
  currentUsers: number;
  currentCustomers: number;
  currentProducts: number;
  currentAssets: number;
  currentContracts: number;

  // Module information
  enabledModules: ModuleActivation[];
}

/**
 * License validation result
 */
export interface LicenseValidationResult {
  isValid: boolean;
  status: string;
  message?: string;
  warnings: string[];
  errors: string[];
  expiresAt?: Date;
  daysRemaining: number;
}

/**
 * License limit check result
 */
export interface LicenseLimitCheckResult {
  hasExceededLimits: boolean;
  violations: LimitViolation[];
  warnings: LimitWarning[];
}

export interface LimitViolation {
  limitType: string;
  limit: number;
  current: number;
  message: string;
}

export interface LimitWarning {
  limitType: string;
  limit: number;
  current: number;
  percentUsed: number;
  message: string;
}

/**
 * Activate license request
 */
export interface ActivateLicenseRequest {
  licenseKey: string;
}

/**
 * API Response wrapper
 */
export interface ApiResponse<T> {
  data: T;
  isSuccess: boolean;
  message?: string;
  errors?: string[];
}
