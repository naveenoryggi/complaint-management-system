export interface Customer {
  id: string;
  companyId: string;
  code: string;
  name: string;
  legalName?: string;
  type: CustomerType;
  typeName: string;
  status: CustomerStatus;
  statusName: string;

  // Contact Information
  primaryEmail: string;
  primaryPhone?: string;
  website?: string;

  // Billing Address
  billingAddressLine1?: string;
  billingAddressLine2?: string;
  billingCity?: string;
  billingState?: string;
  billingCountry?: string;
  billingPostalCode?: string;

  // Business Information
  taxId?: string;
  panNumber?: string;
  industry?: string;
  segment?: CustomerSegment;
  segmentName?: string;
  employeeCount?: number;
  annualRevenue?: number;
  currency: string;

  // Account Information
  customerSince?: Date;
  creditTerms?: string;
  creditLimit?: number;
  customerRating: number;
  statusReason?: string;

  // Account Management
  accountManagerId?: string;
  accountManagerName?: string;

  // Portal Settings
  portalEnabled: boolean;

  // Statistics
  locationCount: number;
  contactCount: number;
  activeTicketCount: number;
  contractCount: number;
  assetCount: number;

  // Metadata
  notes?: string;
  tags?: string;

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface CustomerSummary {
  id: string;
  code: string;
  name: string;
  type: CustomerType;
  typeName: string;
  status: CustomerStatus;
  statusName: string;
  primaryEmail: string;
  billingCity?: string;
  billingCountry?: string;
  segment?: CustomerSegment;
  segmentName?: string;
  locationCount: number;
  activeTicketCount: number;
  customerSince?: Date;
}

export interface CreateCustomerRequest {
  code: string;
  name: string;
  legalName?: string;
  type: CustomerType;
  primaryEmail: string;
  primaryPhone?: string;
  website?: string;
  billingAddressLine1?: string;
  billingAddressLine2?: string;
  billingCity?: string;
  billingState?: string;
  billingCountry?: string;
  billingPostalCode?: string;
  taxId?: string;
  panNumber?: string;
  industry?: string;
  segment?: CustomerSegment;
  employeeCount?: number;
  annualRevenue?: number;
  currency?: string;
  customerSince?: Date;
  creditTerms?: string;
  creditLimit?: number;
  accountManagerId?: string;
  portalEnabled?: boolean;
  notes?: string;
  tags?: string;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  status: CustomerStatus;
  statusReason?: string;
  customerRating?: number;
}

export enum CustomerType {
  Direct = 0,
  Partner = 1,
  Reseller = 2,
  Distributor = 3
}

export enum CustomerStatus {
  Active = 0,
  Inactive = 1,
  Suspended = 2,
  Churned = 3,
  Prospect = 4
}

export enum CustomerSegment {
  Enterprise = 0,
  SMB = 1,
  Startup = 2,
  Government = 3,
  Education = 4,
  NonProfit = 5
}

export const CustomerTypeLabels: { [key: number]: string } = {
  [CustomerType.Direct]: 'Direct',
  [CustomerType.Partner]: 'Partner',
  [CustomerType.Reseller]: 'Reseller',
  [CustomerType.Distributor]: 'Distributor'
};

export const CustomerStatusLabels: { [key: number]: string } = {
  [CustomerStatus.Active]: 'Active',
  [CustomerStatus.Inactive]: 'Inactive',
  [CustomerStatus.Suspended]: 'Suspended',
  [CustomerStatus.Churned]: 'Churned',
  [CustomerStatus.Prospect]: 'Prospect'
};

export const CustomerSegmentLabels: { [key: number]: string } = {
  [CustomerSegment.Enterprise]: 'Enterprise',
  [CustomerSegment.SMB]: 'SMB',
  [CustomerSegment.Startup]: 'Startup',
  [CustomerSegment.Government]: 'Government',
  [CustomerSegment.Education]: 'Education',
  [CustomerSegment.NonProfit]: 'Non-Profit'
};

// Lookup interface for dropdown selections
export interface CustomerLookup {
  id: string;
  code: string;
  name: string;
}
