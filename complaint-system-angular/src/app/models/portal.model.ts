// Portal Authentication Models
export interface PortalLoginRequest {
  email: string;
  password: string;
}

export interface PortalLoginResponse {
  isSuccess: boolean;
  message: string;
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: PortalUser;
  mustChangePassword: boolean;
  isAccountLocked: boolean;
  lockedUntil?: string;
}

export interface PortalUser {
  id: string;
  customerId: string;
  customerName: string;
  customerCode: string;
  locationId?: string;
  locationName?: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  jobTitle?: string;
  portalRole: number;
  portalRoleName: string;
  preferredLanguage?: string;
  preferredTimeZone?: string;
  lastLoginAt?: string;
  permissions: string[];
}

// Portal Dashboard Models
export interface PortalDashboard {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  averageResolutionTime: number;
  recentTickets: PortalTicketSummary[];
  ticketsByStatus: { status: string; count: number }[];
  ticketsByPriority: { priority: string; count: number }[];
}

// Portal Ticket Models
export interface PortalTicketSummary {
  id: string;
  ticketNumber: string;
  subject: string;
  description?: string;
  status: string;
  statusColor?: string;
  priority: string;
  priorityColor?: string;
  category?: string;
  createdAt: string;
  updatedAt?: string;
  resolvedAt?: string;
  closedAt?: string;
  assignedTo?: string;
}

export interface PortalTicketDetail extends PortalTicketSummary {
  customerName: string;
  locationName?: string;
  contactName: string;
  contactEmail: string;
  contactPhone?: string;
  attachments: PortalAttachment[];
  comments: PortalComment[];
  history: PortalTicketHistory[];
  rating?: number;
  ratingFeedback?: string;
  productCategoryName?: string;
  productName?: string;
  brandName?: string;
}

export interface PortalAttachment {
  id: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploadedAt: string;
  uploadedBy: string;
}

export interface PortalComment {
  id: string;
  content: string;
  createdAt: string;
  createdBy: string;
  isInternal: boolean;
}

export interface PortalTicketHistory {
  id: string;
  action: string;
  details: string;
  createdAt: string;
  createdBy: string;
}

// Portal Ticket Creation
export interface PortalCreateTicketRequest {
  title: string;
  description: string;
  categoryId?: string;
  priorityId?: string;
  locationId?: string;
  assetId?: string;
  productCategoryId?: string;
  productId?: string;
  brandId?: string;
  attachments?: File[];
}

// Portal Lookup Models
export interface PortalCategory {
  id: string;
  code: string;
  name: string;
  description?: string;
  parentId?: string;
  parentName?: string;
  icon?: string;
  color?: string;
  hasChildren: boolean;
}

export interface PortalPriority {
  id: string;
  code: string;
  name: string;
  color?: string;
  level: number;
}

export interface PortalStatus {
  id: string;
  code: string;
  name: string;
  color?: string;
  isDefault: boolean;
  isFinal: boolean;
}

export interface PortalProductCategory {
  id: string;
  code: string;
  name: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  level: number;
  icon?: string;
  color?: string;
  hasChildren: boolean;
}

export interface PortalProduct {
  id: string;
  productCode: string;
  name: string;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  brandId?: string;
  brandName?: string;
  imageUrl?: string;
  isUnderWarranty: boolean;
}

export interface PortalBrand {
  id: string;
  code: string;
  name: string;
  logoUrl?: string;
  website?: string;
}

export interface PortalLocation {
  id: string;
  name: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
}

export interface PortalAsset {
  id: string;
  assetCode: string;
  name: string;
  serialNumber?: string;
  locationId?: string;
  locationName?: string;
  productName?: string;
  brandName?: string;
}
