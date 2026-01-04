import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';

export interface Asset {
  id: string;
  companyId: string;
  customerId: string;
  customerName: string;
  locationId?: string;
  locationName?: string;
  productId?: string;
  contractId?: string;
  contractNumber?: string;
  warrantyId?: string;

  // Identity
  assetTag: string;
  serialNumber?: string;
  name: string;
  description?: string;
  type: number;
  typeName: string;
  barcode?: string;
  rfidTag?: string;

  // Product Details
  productName?: string;
  productCode?: string;
  manufacturer?: string;
  model?: string;
  version?: string;
  sku?: string;

  // Lifecycle Dates
  manufactureDate?: Date;
  purchaseDate?: Date;
  receivedDate?: Date;
  installationDate?: Date;
  commissionedDate?: Date;
  warrantyStartDate?: Date;
  warrantyEndDate?: Date;
  extendedWarrantyEndDate?: Date;
  lastServiceDate?: Date;
  nextServiceDate?: Date;
  endOfLifeDate?: Date;
  retiredDate?: Date;
  disposedDate?: Date;

  // Computed warranty status
  isUnderWarranty: boolean;
  warrantyDaysRemaining?: number;
  isWarrantyExpiringSoon: boolean;

  // Financials
  purchasePrice?: number;
  currentValue?: number;
  salvageValue?: number;
  replacementCost?: number;
  currency: string;
  poNumber?: string;
  invoiceNumber?: string;
  vendor?: string;

  // Depreciation
  depreciationMethod: number;
  depreciationMethodName: string;
  usefulLifeMonths?: number;
  depreciationRate?: number;
  accumulatedDepreciation?: number;

  // Ownership
  ownershipType: number;
  ownershipTypeName: string;
  leaseAgreementNumber?: string;
  leaseStartDate?: Date;
  leaseEndDate?: Date;
  leasePayment?: number;

  // Status
  status: number;
  statusName: string;
  condition: number;
  conditionName: string;
  statusReason?: string;
  isOperational: boolean;

  // Stock Category (dynamic)
  stockCategoryId?: string;
  stockCategoryCode?: string;
  stockCategoryName?: string;
  stockCategoryColor?: string;

  // Configuration
  firmwareVersion?: string;
  softwareVersion?: string;
  operatingSystem?: string;
  ipAddress?: string;
  macAddress?: string;
  hostname?: string;

  // Physical Location
  building?: string;
  floor?: string;
  room?: string;
  rack?: string;
  rackUnit?: string;
  latitude?: number;
  longitude?: number;

  // Maintenance
  maintenanceFrequency: number;
  maintenanceFrequencyName: string;
  maintenanceIntervalDays?: number;
  serviceCallCount: number;
  totalDowntimeHours?: number;
  mtbf?: number;
  mttr?: number;

  // Usage Tracking
  currentMeterReading?: number;
  meterUnit?: string;
  lastMeterReadingDate?: Date;
  averageDailyUsage?: number;

  // Assignment
  assignedUserId?: string;
  assignedUserName?: string;
  departmentId?: string;
  departmentName?: string;
  costCenter?: string;
  assignmentPurpose: number;
  assignmentPurposeName: string;
  assignmentDate?: Date;
  expectedReturnDate?: Date;
  actualReturnDate?: Date;
  assignmentNotes?: string;
  isTemporaryAssignment: boolean;
  isOverdue: boolean;

  // External References
  externalAssetId?: string;
  parentAssetId?: string;
  parentAssetTag?: string;

  // Additional
  notes?: string;
  tags: string[];
  childAssetCount: number;
  serviceHistoryCount: number;
  contractItemCount: number;

  // Audit
  createdAt: Date;
  updatedAt?: Date;
}

export interface AssetSummary {
  id: string;
  assetTag: string;
  serialNumber?: string;
  name: string;
  type: number;
  typeName: string;
  customerId: string;
  customerName: string;
  locationName?: string;
  productName?: string;
  manufacturer?: string;
  model?: string;
  status: number;
  statusName: string;
  condition: number;
  conditionName: string;
  isOperational: boolean;
  isUnderWarranty: boolean;
  isUnderContract: boolean;
  warrantyEndDate?: Date;
  lastServiceDate?: Date;
  nextServiceDate?: Date;
  currentValue?: number;
  currency: string;
  serviceCallCount: number;
  // Stock Category (dynamic)
  stockCategoryId?: string;
  stockCategoryCode?: string;
  stockCategoryName?: string;
  stockCategoryColor?: string;
}

export interface AssetLookup {
  id: string;
  assetTag: string;
  serialNumber?: string;
  name: string;
  displayName: string;
  type: number;
  status: number;
  customerId: string;
  customerName: string;
  isOperational: boolean;
  isUnderWarranty: boolean;
}

export interface CreateAssetRequest {
  customerId: string;
  locationId?: string;
  productId?: string;
  contractId?: string;
  warrantyId?: string;
  stockCategoryId?: string;  // Dynamic stock category reference
  assetTag?: string;
  serialNumber?: string;
  name: string;
  description?: string;
  type: number;
  barcode?: string;
  rfidTag?: string;
  productName?: string;
  productCode?: string;
  manufacturer?: string;
  model?: string;
  version?: string;
  sku?: string;
  manufactureDate?: Date;
  purchaseDate?: Date;
  receivedDate?: Date;
  installationDate?: Date;
  commissionedDate?: Date;
  warrantyStartDate?: Date;
  warrantyEndDate?: Date;
  extendedWarrantyEndDate?: Date;
  endOfLifeDate?: Date;
  purchasePrice?: number;
  currentValue?: number;
  salvageValue?: number;
  replacementCost?: number;
  currency?: string;
  poNumber?: string;
  invoiceNumber?: string;
  vendor?: string;
  depreciationMethod?: number;
  usefulLifeMonths?: number;
  depreciationRate?: number;
  ownershipType?: number;
  leaseAgreementNumber?: string;
  leaseStartDate?: Date;
  leaseEndDate?: Date;
  leasePayment?: number;
  status?: number;
  condition?: number;
  statusReason?: string;
  isOperational?: boolean;
  specifications?: string;
  configuration?: string;
  firmwareVersion?: string;
  softwareVersion?: string;
  operatingSystem?: string;
  ipAddress?: string;
  macAddress?: string;
  hostname?: string;
  building?: string;
  floor?: string;
  room?: string;
  rack?: string;
  rackUnit?: string;
  latitude?: number;
  longitude?: number;
  maintenanceFrequency?: number;
  maintenanceIntervalDays?: number;
  currentMeterReading?: number;
  meterUnit?: string;
  assignedUserId?: string;
  departmentId?: string;
  costCenter?: string;
  assignmentPurpose?: number;
  assignmentDate?: Date;
  expectedReturnDate?: Date;
  assignmentNotes?: string;
  externalAssetId?: string;
  parentAssetId?: string;
  notes?: string;
  tags?: string[];
  customFields?: string;
}

export interface TransferAssetRequest {
  newCustomerId: string;
  newLocationId?: string;
  transferDate: Date;
  transferReason?: string;
  notes?: string;
}

export interface AssignAssetRequest {
  assignedUserId?: string;
  customerId?: string;
  departmentId?: string;
  assignmentPurpose: number;
  assignmentDate?: Date;
  expectedReturnDate?: Date;
  assignmentNotes?: string;
}

export interface ReturnAssetRequest {
  returnDate: Date;
  returnNotes?: string;
  conditionOnReturn?: number;
}

export interface AssetStatistics {
  totalAssets: number;
  operationalAssets: number;
  nonOperationalAssets: number;
  assetsUnderWarranty: number;
  assetsUnderContract: number;
  assetsNeedingService: number;
  retiredAssets: number;
  byStatus: { [key: string]: number };
  byType: { [key: string]: number };
  byCondition: { [key: string]: number };
  byCustomer: { [key: string]: number };
  totalAssetValue: number;
  totalAccumulatedDepreciation: number;
  currency: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

// Enums
export const AssetStatus = {
  Ordered: 0,
  Received: 1,
  InStock: 2,
  Deployed: 3,
  InService: 4,
  UnderRepair: 5,
  OutOfService: 6,
  Retired: 7,
  Disposed: 8,
  Lost: 9,
  InTransit: 10,
  OnLoan: 11,
  Returned: 12,
  Reserved: 13
};

export const AssetType = {
  Hardware: 1,
  Software: 2,
  Network: 3,
  Peripheral: 4,
  Mobile: 5,
  Furniture: 6,
  Vehicle: 7,
  Machinery: 8,
  Tool: 9,
  OfficeEquipment: 10,
  MedicalEquipment: 11,
  LabEquipment: 12,
  Other: 99
};

export const AssetCondition = {
  New: 1,
  Excellent: 2,
  Good: 3,
  Fair: 4,
  Poor: 5,
  NonFunctional: 6,
  BeyondRepair: 7,
  Refurbished: 8
};

export const AssetOwnershipType = {
  Owned: 1,
  Leased: 2,
  Rented: 3,
  Loaned: 4,
  Financed: 5,
  CustomerOwned: 6,
  Consignment: 7,
  Demo: 8,
  Development: 9
};

export const AssetAssignmentPurpose = {
  Permanent: 1,
  Demo: 2,
  Development: 3,
  TemporaryLoan: 4,
  Training: 5,
  POC: 6,
  Replacement: 7
};

@Injectable({
  providedIn: 'root'
})
export class AssetService {
  private get API_URL(): string {
    return `${runtimeConfig.apiUrl}/assets`;
  }

  constructor(private http: HttpClient) {}

  // Get paginated assets
  getAssets(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    status?: number,
    type?: number,
    customerId?: string,
    locationId?: string,
    isOperational?: boolean,
    isUnderWarranty?: boolean,
    sortBy?: string,
    sortDirection?: string
  ): Observable<PagedResult<AssetSummary>> {
    let params = new HttpParams()
      .set('page', pageNumber.toString())  // Backend expects 'page' not 'pageNumber'
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (status !== undefined) params = params.set('status', status.toString());
    if (type !== undefined) params = params.set('type', type.toString());
    if (customerId) params = params.set('customerId', customerId);
    if (locationId) params = params.set('locationId', locationId);
    if (isOperational !== undefined) params = params.set('isOperational', isOperational.toString());
    if (isUnderWarranty !== undefined) params = params.set('isUnderWarranty', isUnderWarranty.toString());
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDirection) params = params.set('sortDescending', sortDirection === 'desc' ? 'true' : 'false');

    // Backend returns PagedResult directly, not wrapped in ApiResult
    return this.http.get<PagedResult<AssetSummary>>(this.API_URL, { params });
  }

  // Get single asset by ID
  getAsset(id: string): Observable<ApiResult<Asset>> {
    return this.http.get<ApiResult<Asset>>(`${this.API_URL}/${id}`);
  }

  // Get asset by tag
  getAssetByTag(assetTag: string): Observable<ApiResult<Asset>> {
    return this.http.get<ApiResult<Asset>>(`${this.API_URL}/by-tag/${assetTag}`);
  }

  // Get asset by serial number
  getAssetBySerialNumber(serialNumber: string): Observable<ApiResult<Asset>> {
    return this.http.get<ApiResult<Asset>>(`${this.API_URL}/by-serial/${serialNumber}`);
  }

  // Create asset
  createAsset(request: CreateAssetRequest): Observable<ApiResult<Asset>> {
    return this.http.post<ApiResult<Asset>>(this.API_URL, request);
  }

  // Update asset
  updateAsset(id: string, request: Partial<CreateAssetRequest>): Observable<ApiResult<Asset>> {
    return this.http.put<ApiResult<Asset>>(`${this.API_URL}/${id}`, request);
  }

  // Update asset status
  updateAssetStatus(id: string, status: number, condition?: number, statusReason?: string): Observable<ApiResult<Asset>> {
    return this.http.patch<ApiResult<Asset>>(`${this.API_URL}/${id}/status`, {
      status,
      condition,
      statusReason
    });
  }

  // Delete asset
  deleteAsset(id: string): Observable<ApiResult<boolean>> {
    return this.http.delete<ApiResult<boolean>>(`${this.API_URL}/${id}`);
  }

  // Transfer asset to another customer
  transferAsset(id: string, request: TransferAssetRequest): Observable<ApiResult<Asset>> {
    return this.http.post<ApiResult<Asset>>(`${this.API_URL}/${id}/transfer`, request);
  }

  // Assign asset to user/customer
  assignAsset(id: string, request: AssignAssetRequest): Observable<ApiResult<Asset>> {
    return this.http.post<ApiResult<Asset>>(`${this.API_URL}/${id}/assign`, request);
  }

  // Return assigned asset
  returnAsset(id: string, request: ReturnAssetRequest): Observable<ApiResult<Asset>> {
    return this.http.post<ApiResult<Asset>>(`${this.API_URL}/${id}/return`, request);
  }

  // Record meter reading
  recordMeterReading(id: string, meterReading: number, meterUnit?: string, notes?: string): Observable<ApiResult<Asset>> {
    return this.http.post<ApiResult<Asset>>(`${this.API_URL}/${id}/meter-reading`, {
      meterReading,
      meterUnit,
      notes
    });
  }

  // Get assets by customer
  getCustomerAssets(customerId: string): Observable<ApiResult<AssetSummary[]>> {
    return this.http.get<ApiResult<AssetSummary[]>>(`${this.API_URL}/customer/${customerId}`);
  }

  // Get assets by location
  getLocationAssets(locationId: string): Observable<ApiResult<AssetSummary[]>> {
    return this.http.get<ApiResult<AssetSummary[]>>(`${this.API_URL}/location/${locationId}`);
  }

  // Get child assets
  getChildAssets(parentAssetId: string): Observable<ApiResult<AssetSummary[]>> {
    return this.http.get<ApiResult<AssetSummary[]>>(`${this.API_URL}/${parentAssetId}/children`);
  }

  // Get asset statistics
  getStatistics(customerId?: string): Observable<ApiResult<AssetStatistics>> {
    let params = new HttpParams();
    if (customerId) params = params.set('customerId', customerId);
    return this.http.get<ApiResult<AssetStatistics>>(`${this.API_URL}/statistics`, { params });
  }

  // Get assets needing service
  getAssetsNeedingService(daysUntilService?: number): Observable<ApiResult<AssetSummary[]>> {
    let params = new HttpParams();
    if (daysUntilService) params = params.set('daysUntilService', daysUntilService.toString());
    return this.http.get<ApiResult<AssetSummary[]>>(`${this.API_URL}/needing-service`, { params });
  }

  // Get assets with expiring warranty
  getAssetsWithExpiringWarranty(daysUntilExpiry?: number): Observable<ApiResult<AssetSummary[]>> {
    let params = new HttpParams();
    if (daysUntilExpiry) params = params.set('daysUntilExpiry', daysUntilExpiry.toString());
    return this.http.get<ApiResult<AssetSummary[]>>(`${this.API_URL}/expiring-warranty`, { params });
  }

  // Get asset lookups
  getLookups(customerId?: string, operationalOnly?: boolean): Observable<ApiResult<AssetLookup[]>> {
    let params = new HttpParams();
    if (customerId) params = params.set('customerId', customerId);
    if (operationalOnly) params = params.set('isOperational', operationalOnly.toString());
    return this.http.get<ApiResult<AssetLookup[]>>(`${this.API_URL}/lookup`, { params });
  }

  // Search assets
  searchAssets(searchTerm: string, limit?: number): Observable<ApiResult<AssetLookup[]>> {
    let params = new HttpParams().set('searchTerm', searchTerm);
    if (limit) params = params.set('limit', limit.toString());
    return this.http.get<ApiResult<AssetLookup[]>>(`${this.API_URL}/search`, { params });
  }

  // Validate asset tag
  validateAssetTag(assetTag: string, excludeId?: string): Observable<ApiResult<boolean>> {
    let params = new HttpParams().set('assetTag', assetTag);
    if (excludeId) params = params.set('excludeId', excludeId);
    return this.http.get<ApiResult<boolean>>(`${this.API_URL}/validate-tag`, { params });
  }

  // Generate asset tag
  generateAssetTag(): Observable<ApiResult<string>> {
    return this.http.get<ApiResult<string>>(`${this.API_URL}/generate-tag`);
  }

  // Get service history
  getServiceHistory(assetId: string): Observable<ApiResult<any[]>> {
    return this.http.get<ApiResult<any[]>>(`${this.API_URL}/${assetId}/service-history`);
  }
}
