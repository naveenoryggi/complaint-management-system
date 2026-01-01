// ==================== ENUMS ====================

export enum ContractType {
  Warranty = 1,
  AMC = 2,
  SLA = 3,
  SupportPack = 4,
  Subscription = 5,
  ExtendedWarranty = 6,
  ComprehensiveAMC = 7,
  NonComprehensiveAMC = 8
}

export const ContractTypeLabels: Record<ContractType, string> = {
  [ContractType.Warranty]: 'Warranty',
  [ContractType.AMC]: 'AMC',
  [ContractType.SLA]: 'SLA',
  [ContractType.SupportPack]: 'Support Pack',
  [ContractType.Subscription]: 'Subscription',
  [ContractType.ExtendedWarranty]: 'Extended Warranty',
  [ContractType.ComprehensiveAMC]: 'Comprehensive AMC',
  [ContractType.NonComprehensiveAMC]: 'Non-Comprehensive AMC'
};

export enum ContractStatus {
  Draft = 0,
  Pending = 1,
  Active = 2,
  Expired = 3,
  Renewed = 4,
  Terminated = 5,
  Cancelled = 6,
  Suspended = 7,
  PendingRenewal = 8
}

export const ContractStatusLabels: Record<ContractStatus, string> = {
  [ContractStatus.Draft]: 'Draft',
  [ContractStatus.Pending]: 'Pending',
  [ContractStatus.Active]: 'Active',
  [ContractStatus.Expired]: 'Expired',
  [ContractStatus.Renewed]: 'Renewed',
  [ContractStatus.Terminated]: 'Terminated',
  [ContractStatus.Cancelled]: 'Cancelled',
  [ContractStatus.Suspended]: 'Suspended',
  [ContractStatus.PendingRenewal]: 'Pending Renewal'
};

export enum CoverageType {
  FullCoverage = 1,
  PartsOnly = 2,
  LaborOnly = 3,
  ExtendedWarranty = 4,
  PreventiveMaintenance = 5,
  CorrectiveMaintenance = 6,
  SoftwareSupport = 7,
  HardwareSupport = 8,
  Comprehensive = 9
}

export const CoverageTypeLabels: Record<CoverageType, string> = {
  [CoverageType.FullCoverage]: 'Full Coverage',
  [CoverageType.PartsOnly]: 'Parts Only',
  [CoverageType.LaborOnly]: 'Labor Only',
  [CoverageType.ExtendedWarranty]: 'Extended Warranty',
  [CoverageType.PreventiveMaintenance]: 'Preventive Maintenance',
  [CoverageType.CorrectiveMaintenance]: 'Corrective Maintenance',
  [CoverageType.SoftwareSupport]: 'Software Support',
  [CoverageType.HardwareSupport]: 'Hardware Support',
  [CoverageType.Comprehensive]: 'Comprehensive'
};

export enum BillingFrequency {
  OneTime = 0,
  Monthly = 1,
  Quarterly = 2,
  SemiAnnual = 3,
  Annual = 4,
  BiAnnual = 5
}

export const BillingFrequencyLabels: Record<BillingFrequency, string> = {
  [BillingFrequency.OneTime]: 'One Time',
  [BillingFrequency.Monthly]: 'Monthly',
  [BillingFrequency.Quarterly]: 'Quarterly',
  [BillingFrequency.SemiAnnual]: 'Semi-Annual',
  [BillingFrequency.Annual]: 'Annual',
  [BillingFrequency.BiAnnual]: 'Bi-Annual'
};

export enum ContractItemStatus {
  Covered = 1,
  PendingActivation = 2,
  Expired = 3,
  Terminated = 4,
  Replaced = 5,
  Suspended = 6
}

export const ContractItemStatusLabels: Record<ContractItemStatus, string> = {
  [ContractItemStatus.Covered]: 'Covered',
  [ContractItemStatus.PendingActivation]: 'Pending Activation',
  [ContractItemStatus.Expired]: 'Expired',
  [ContractItemStatus.Terminated]: 'Terminated',
  [ContractItemStatus.Replaced]: 'Replaced',
  [ContractItemStatus.Suspended]: 'Suspended'
};

export enum WarrantyType {
  Standard = 1,
  Extended = 2,
  Limited = 3,
  Lifetime = 4,
  Prorated = 5,
  Express = 6,
  Implied = 7
}

export const WarrantyTypeLabels: Record<WarrantyType, string> = {
  [WarrantyType.Standard]: 'Standard',
  [WarrantyType.Extended]: 'Extended',
  [WarrantyType.Limited]: 'Limited',
  [WarrantyType.Lifetime]: 'Lifetime',
  [WarrantyType.Prorated]: 'Prorated',
  [WarrantyType.Express]: 'Express',
  [WarrantyType.Implied]: 'Implied'
};

export enum SupportHoursType {
  BusinessHours = 1,
  ExtendedHours = 2,
  TwentyFourByFive = 3,
  BusinessHoursAllDays = 4,
  ExtendedHoursAllDays = 5,
  TwentyFourBySeven = 6,
  Custom = 7
}

export const SupportHoursTypeLabels: Record<SupportHoursType, string> = {
  [SupportHoursType.BusinessHours]: '8x5 (Business Hours)',
  [SupportHoursType.ExtendedHours]: '12x5 (Extended)',
  [SupportHoursType.TwentyFourByFive]: '24x5',
  [SupportHoursType.BusinessHoursAllDays]: '8x7',
  [SupportHoursType.ExtendedHoursAllDays]: '12x7',
  [SupportHoursType.TwentyFourBySeven]: '24x7',
  [SupportHoursType.Custom]: 'Custom'
};

export enum RenewalAction {
  AutoRenewed = 1,
  ManualRenewal = 2,
  Declined = 3,
  Pending = 4,
  Upgraded = 5,
  Downgraded = 6,
  Lapsed = 7
}

export const RenewalActionLabels: Record<RenewalAction, string> = {
  [RenewalAction.AutoRenewed]: 'Auto Renewed',
  [RenewalAction.ManualRenewal]: 'Manual Renewal',
  [RenewalAction.Declined]: 'Declined',
  [RenewalAction.Pending]: 'Pending',
  [RenewalAction.Upgraded]: 'Upgraded',
  [RenewalAction.Downgraded]: 'Downgraded',
  [RenewalAction.Lapsed]: 'Lapsed'
};

// ==================== CONTRACT INTERFACES ====================

export interface Contract {
  id: string;
  companyId: string;
  customerId: string;
  customerName: string;
  partnerId?: string;
  partnerName?: string;
  contractNumber: string;
  name: string;
  type: ContractType;
  typeName: string;
  description?: string;
  externalContractId?: string;
  poNumber?: string;
  invoiceNumber?: string;
  startDate: string;
  endDate: string;
  signedDate?: string;
  activatedDate?: string;
  daysRemaining: number;
  isExpiringSoon: boolean;
  autoRenew: boolean;
  renewalPeriodMonths?: number;
  renewalNoticeDays?: number;
  nextRenewalDate?: string;
  renewalCount: number;
  contractValue?: number;
  annualValue?: number;
  monthlyValue?: number;
  currency: string;
  billingFrequency?: BillingFrequency;
  billingFrequencyName?: string;
  paymentTerms?: string;
  coverageType: CoverageType;
  coverageTypeName: string;
  includesOnsite: boolean;
  includesRemote: boolean;
  includesParts: boolean;
  includesLabor: boolean;
  includedHours?: number;
  usedHours: number;
  remainingHours?: number;
  includedIncidents?: number;
  usedIncidents: number;
  slaSettingsId?: string;
  slaSettingsName?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
  supportHoursTypeName?: string;
  status: ContractStatus;
  statusName: string;
  itemCount: number;
  items?: ContractItemSummary[];
  createdAt: string;
  updatedAt?: string;
}

export interface ContractSummary {
  id: string;
  contractNumber: string;
  name: string;
  type: ContractType;
  typeName: string;
  customerId: string;
  customerName: string;
  partnerName?: string;
  startDate: string;
  endDate: string;
  daysRemaining: number;
  isExpiringSoon: boolean;
  contractValue?: number;
  currency: string;
  status: ContractStatus;
  statusName: string;
  coverageType: CoverageType;
  coverageTypeName: string;
  itemCount: number;
  autoRenew: boolean;
}

export interface ContractLookup {
  id: string;
  contractNumber: string;
  name: string;
  displayName: string;
  type: ContractType;
  status: ContractStatus;
  endDate: string;
  isActive: boolean;
}

export interface CreateContractRequest {
  customerId: string;
  partnerId?: string;
  name: string;
  type: ContractType;
  description?: string;
  externalContractId?: string;
  poNumber?: string;
  invoiceNumber?: string;
  startDate: string;
  endDate: string;
  signedDate?: string;
  autoRenew?: boolean;
  renewalPeriodMonths?: number;
  renewalNoticeDays?: number;
  contractValue?: number;
  annualValue?: number;
  monthlyValue?: number;
  currency?: string;
  billingFrequency?: BillingFrequency;
  paymentTerms?: string;
  coverageType: CoverageType;
  includesOnsite?: boolean;
  includesRemote?: boolean;
  includesParts?: boolean;
  includesLabor?: boolean;
  includedHours?: number;
  includedIncidents?: number;
  slaSettingsId?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
  items?: CreateContractItemRequest[];
}

export interface UpdateContractRequest {
  name?: string;
  description?: string;
  externalContractId?: string;
  poNumber?: string;
  invoiceNumber?: string;
  startDate?: string;
  endDate?: string;
  signedDate?: string;
  autoRenew?: boolean;
  renewalPeriodMonths?: number;
  renewalNoticeDays?: number;
  contractValue?: number;
  annualValue?: number;
  monthlyValue?: number;
  billingFrequency?: BillingFrequency;
  paymentTerms?: string;
  coverageType?: CoverageType;
  includesOnsite?: boolean;
  includesRemote?: boolean;
  includesParts?: boolean;
  includesLabor?: boolean;
  includedHours?: number;
  includedIncidents?: number;
  slaSettingsId?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
}

export interface ActivateContractRequest {
  activatedDate?: string;
  notes?: string;
}

export interface RenewContractRequest {
  newStartDate: string;
  newEndDate: string;
  renewalPeriodMonths: number;
  newValue?: number;
  priceChangePercent?: number;
  renewalDiscount?: number;
  poNumber?: string;
  invoiceNumber?: string;
  newType?: ContractType;
  newCoverageType?: CoverageType;
  notes?: string;
}

export interface TerminateContractRequest {
  terminatedDate: string;
  reason: string;
  notes?: string;
}

export interface SuspendRequest {
  reason: string;
  notes?: string;
}

export interface ReactivateRequest {
  notes?: string;
}

// ==================== CONTRACT ITEM INTERFACES ====================

export interface ContractItem {
  id: string;
  contractId: string;
  contractNumber: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  assetId?: string;
  assetTag?: string;
  serialNumber?: string;
  itemDescription?: string;
  serialNumbers?: string;
  quantity: number;
  unitOfMeasure?: string;
  coverageStartDate: string;
  coverageEndDate: string;
  isCovered: boolean;
  daysRemaining: number;
  itemValue?: number;
  discountPercent?: number;
  currency: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  priorityLevel?: number;
  status: ContractItemStatus;
  statusName: string;
  isActive: boolean;
  locationId?: string;
  locationName?: string;
  siteInfo?: string;
  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  maintenanceIntervalDays?: number;
  serviceCallCount: number;
  specialConditions?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ContractItemSummary {
  id: string;
  productCode?: string;
  productName?: string;
  assetTag?: string;
  serialNumber?: string;
  quantity: number;
  coverageEndDate: string;
  isCovered: boolean;
  status: ContractItemStatus;
  statusName: string;
}

export interface CreateContractItemRequest {
  productId?: string;
  assetId?: string;
  itemDescription?: string;
  serialNumber?: string;
  serialNumbers?: string;
  quantity?: number;
  unitOfMeasure?: string;
  coverageStartDate?: string;
  coverageEndDate?: string;
  itemValue?: number;
  discountPercent?: number;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  priorityLevel?: number;
  locationId?: string;
  siteInfo?: string;
  maintenanceIntervalDays?: number;
  specialConditions?: string;
  notes?: string;
}

export interface UpdateContractItemRequest {
  itemDescription?: string;
  serialNumber?: string;
  serialNumbers?: string;
  quantity?: number;
  coverageStartDate?: string;
  coverageEndDate?: string;
  itemValue?: number;
  discountPercent?: number;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  priorityLevel?: number;
  locationId?: string;
  siteInfo?: string;
  maintenanceIntervalDays?: number;
  specialConditions?: string;
  notes?: string;
  status?: ContractItemStatus;
  isActive?: boolean;
}

// ==================== WARRANTY INTERFACES ====================

export interface Warranty {
  id: string;
  companyId: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  categoryId?: string;
  categoryName?: string;
  code: string;
  name: string;
  description?: string;
  type: WarrantyType;
  typeName: string;
  durationMonths: number;
  extendedDurationMonths?: number;
  gracePeriodDays?: number;
  maxDurationMonths?: number;
  coverageType: CoverageType;
  coverageTypeName: string;
  includesParts: boolean;
  includesLabor: boolean;
  includesOnsite: boolean;
  includesShipping: boolean;
  coveredItems?: string;
  excludedItems?: string;
  terms?: string;
  isTransferable: boolean;
  requiresRegistration: boolean;
  registrationDeadlineDays?: number;
  requiresProofOfPurchase: boolean;
  extendedWarrantyPrice?: number;
  extendedWarrantyPricePercent?: number;
  currency: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
  supportHoursTypeName?: string;
  maxClaims?: number;
  maxClaimValue?: number;
  maxClaimValuePercent?: number;
  deductibleAmount?: number;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt?: string;
}

export interface WarrantySummary {
  id: string;
  code: string;
  name: string;
  type: WarrantyType;
  typeName: string;
  productName?: string;
  categoryName?: string;
  durationMonths: number;
  coverageType: CoverageType;
  coverageTypeName: string;
  isActive: boolean;
  isDefault: boolean;
}

export interface WarrantyLookup {
  id: string;
  code: string;
  name: string;
  displayName: string;
  type: WarrantyType;
  durationMonths: number;
  isDefault: boolean;
}

export interface CreateWarrantyRequest {
  productId?: string;
  categoryId?: string;
  code: string;
  name: string;
  description?: string;
  type: WarrantyType;
  durationMonths: number;
  extendedDurationMonths?: number;
  gracePeriodDays?: number;
  maxDurationMonths?: number;
  coverageType: CoverageType;
  includesParts?: boolean;
  includesLabor?: boolean;
  includesOnsite?: boolean;
  includesShipping?: boolean;
  coveredItems?: string;
  excludedItems?: string;
  terms?: string;
  isTransferable?: boolean;
  requiresRegistration?: boolean;
  registrationDeadlineDays?: number;
  requiresProofOfPurchase?: boolean;
  extendedWarrantyPrice?: number;
  extendedWarrantyPricePercent?: number;
  currency?: string;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
  maxClaims?: number;
  maxClaimValue?: number;
  maxClaimValuePercent?: number;
  deductibleAmount?: number;
  isActive?: boolean;
  isDefault?: boolean;
  displayOrder?: number;
}

export interface UpdateWarrantyRequest {
  name?: string;
  description?: string;
  type?: WarrantyType;
  durationMonths?: number;
  extendedDurationMonths?: number;
  gracePeriodDays?: number;
  maxDurationMonths?: number;
  coverageType?: CoverageType;
  includesParts?: boolean;
  includesLabor?: boolean;
  includesOnsite?: boolean;
  includesShipping?: boolean;
  coveredItems?: string;
  excludedItems?: string;
  terms?: string;
  isTransferable?: boolean;
  requiresRegistration?: boolean;
  registrationDeadlineDays?: number;
  requiresProofOfPurchase?: boolean;
  extendedWarrantyPrice?: number;
  extendedWarrantyPricePercent?: number;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
  maxClaims?: number;
  maxClaimValue?: number;
  maxClaimValuePercent?: number;
  deductibleAmount?: number;
  isActive?: boolean;
  isDefault?: boolean;
  displayOrder?: number;
}

// ==================== RENEWAL & REPORTING INTERFACES ====================

export interface ContractRenewalHistory {
  id: string;
  contractId: string;
  contractNumber: string;
  renewalNumber: number;
  previousEndDate: string;
  newStartDate: string;
  newEndDate: string;
  renewalPeriodMonths: number;
  action: RenewalAction;
  actionName: string;
  renewalDate: string;
  processedBy?: string;
  processedByName?: string;
  previousValue?: number;
  newValue?: number;
  priceChangePercent?: number;
  currency: string;
  renewalDiscount?: number;
  invoiceNumber?: string;
  poNumber?: string;
  previousType?: ContractType;
  newType?: ContractType;
  previousCoverageType?: CoverageType;
  newCoverageType?: CoverageType;
  changesSummary?: string;
  noticeSentDate?: string;
  customerResponseDate?: string;
  customerResponse?: string;
  reason?: string;
  notes?: string;
  createdAt: string;
}

export interface ContractRenewalSummary {
  id: string;
  renewalNumber: number;
  action: RenewalAction;
  actionName: string;
  renewalDate: string;
  previousEndDate: string;
  newEndDate: string;
  previousValue?: number;
  newValue?: number;
  priceChangePercent?: number;
}

export interface ExpiringContractsReport {
  totalExpiring: number;
  totalValue: number;
  currency: string;
  contracts: ExpiringContract[];
}

export interface ExpiringContract {
  id: string;
  contractNumber: string;
  name: string;
  customerName: string;
  endDate: string;
  daysRemaining: number;
  contractValue?: number;
  autoRenew: boolean;
  accountManagerName?: string;
}

export interface ContractStatistics {
  totalContracts: number;
  activeContracts: number;
  expiredContracts: number;
  expiringIn30Days: number;
  expiringIn60Days: number;
  expiringIn90Days: number;
  pendingRenewal: number;
  totalContractValue: number;
  activeContractValue: number;
  contractsByType: Record<string, number>;
  contractsByStatus: Record<string, number>;
  contractsByCoverageType: Record<string, number>;
}

export interface CoverageCheckResult {
  customerId: string;
  productId?: string;
  assetId?: string;
  isCovered: boolean;
  coverageType?: CoverageType;
  contractId?: string;
  contractNumber?: string;
  contractName?: string;
  coverageEndDate?: string;
  daysRemaining?: number;
  responseTimeHours?: number;
  resolutionTimeHours?: number;
  supportHoursType?: SupportHoursType;
}

export interface RecordUsageRequest {
  hours: number;
  description?: string;
  serviceDate?: string;
}

export interface RecordIncidentRequest {
  incidentId?: string;
  description?: string;
  incidentDate?: string;
}
