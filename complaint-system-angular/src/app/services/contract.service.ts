import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  Contract,
  ContractSummary,
  ContractLookup,
  CreateContractRequest,
  UpdateContractRequest,
  ActivateContractRequest,
  RenewContractRequest,
  TerminateContractRequest,
  SuspendRequest,
  ReactivateRequest,
  ContractItem,
  CreateContractItemRequest,
  UpdateContractItemRequest,
  Warranty,
  WarrantySummary,
  WarrantyLookup,
  CreateWarrantyRequest,
  UpdateWarrantyRequest,
  ContractRenewalHistory,
  ContractRenewalSummary,
  ExpiringContractsReport,
  ContractStatistics,
  CoverageCheckResult,
  RecordUsageRequest,
  RecordIncidentRequest,
  ContractType,
  ContractStatus
} from '../models/contract.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

interface PagedResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private get baseUrl(): string {
    return `${runtimeConfig.apiUrl}/contracts`;
  }

  constructor(private http: HttpClient) {}

  // ==================== CONTRACT CRUD ====================

  getContracts(params?: {
    search?: string;
    customerId?: string;
    partnerId?: string;
    type?: ContractType;
    status?: ContractStatus;
    coverageType?: number;
    startDateFrom?: string;
    startDateTo?: string;
    endDateFrom?: string;
    endDateTo?: string;
    expiringWithinDays?: number;
    autoRenew?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: string;
  }): Observable<PagedResponse<ContractSummary>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('searchTerm', params.search);
    if (params?.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params?.partnerId) httpParams = httpParams.set('partnerId', params.partnerId);
    if (params?.type !== undefined) httpParams = httpParams.set('type', params.type.toString());
    if (params?.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
    if (params?.coverageType !== undefined) httpParams = httpParams.set('coverageType', params.coverageType.toString());
    if (params?.startDateFrom) httpParams = httpParams.set('startDateFrom', params.startDateFrom);
    if (params?.startDateTo) httpParams = httpParams.set('startDateTo', params.startDateTo);
    if (params?.endDateFrom) httpParams = httpParams.set('endDateFrom', params.endDateFrom);
    if (params?.endDateTo) httpParams = httpParams.set('endDateTo', params.endDateTo);
    if (params?.expiringWithinDays !== undefined) httpParams = httpParams.set('expiringWithinDays', params.expiringWithinDays.toString());
    if (params?.autoRenew !== undefined) httpParams = httpParams.set('autoRenew', params.autoRenew.toString());
    if (params?.pageNumber) httpParams = httpParams.set('page', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params?.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);

    return this.http.get<PagedResponse<ContractSummary>>(this.baseUrl, { params: httpParams });
  }

  getContractById(id: string): Observable<ApiResponse<Contract>> {
    return this.http.get<ApiResponse<Contract>>(`${this.baseUrl}/${id}`);
  }

  getContractByNumber(contractNumber: string): Observable<ApiResponse<Contract>> {
    return this.http.get<ApiResponse<Contract>>(`${this.baseUrl}/by-number/${contractNumber}`);
  }

  createContract(request: CreateContractRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(this.baseUrl, request);
  }

  updateContract(id: string, request: UpdateContractRequest): Observable<ApiResponse<Contract>> {
    return this.http.put<ApiResponse<Contract>>(`${this.baseUrl}/${id}`, request);
  }

  deleteContract(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  // ==================== CONTRACT STATUS OPERATIONS ====================

  activateContract(id: string, request?: ActivateContractRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${id}/activate`, request || {});
  }

  renewContract(id: string, request: RenewContractRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${id}/renew`, request);
  }

  terminateContract(id: string, request: TerminateContractRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${id}/terminate`, request);
  }

  suspendContract(id: string, request: SuspendRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${id}/suspend`, request);
  }

  reactivateContract(id: string, request?: ReactivateRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${id}/reactivate`, request || {});
  }

  // ==================== CONTRACT ITEMS ====================

  getContractItems(contractId: string): Observable<ApiResponse<ContractItem[]>> {
    return this.http.get<ApiResponse<ContractItem[]>>(`${this.baseUrl}/${contractId}/items`);
  }

  addContractItem(contractId: string, request: CreateContractItemRequest): Observable<ApiResponse<ContractItem>> {
    return this.http.post<ApiResponse<ContractItem>>(`${this.baseUrl}/${contractId}/items`, request);
  }

  updateContractItem(contractId: string, itemId: string, request: UpdateContractItemRequest): Observable<ApiResponse<ContractItem>> {
    return this.http.put<ApiResponse<ContractItem>>(`${this.baseUrl}/${contractId}/items/${itemId}`, request);
  }

  removeContractItem(contractId: string, itemId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${contractId}/items/${itemId}`);
  }

  // ==================== COVERAGE CHECKS ====================

  checkCustomerCoverage(customerId: string): Observable<ApiResponse<CoverageCheckResult>> {
    return this.http.get<ApiResponse<CoverageCheckResult>>(`${this.baseUrl}/coverage/customer/${customerId}`);
  }

  checkProductCoverage(customerId: string, productId: string): Observable<ApiResponse<CoverageCheckResult>> {
    return this.http.get<ApiResponse<CoverageCheckResult>>(`${this.baseUrl}/coverage/customer/${customerId}/product/${productId}`);
  }

  checkAssetCoverage(assetId: string): Observable<ApiResponse<CoverageCheckResult>> {
    return this.http.get<ApiResponse<CoverageCheckResult>>(`${this.baseUrl}/coverage/asset/${assetId}`);
  }

  // ==================== REPORTING & STATISTICS ====================

  getExpiringContracts(days: number = 30): Observable<ApiResponse<ExpiringContractsReport>> {
    return this.http.get<ApiResponse<ExpiringContractsReport>>(`${this.baseUrl}/expiring?days=${days}`);
  }

  getContractsByCustomer(customerId: string, activeOnly: boolean = false): Observable<ApiResponse<ContractSummary[]>> {
    return this.http.get<ApiResponse<ContractSummary[]>>(`${this.baseUrl}/customer/${customerId}?activeOnly=${activeOnly}`);
  }

  getRenewalHistory(contractId: string): Observable<ApiResponse<ContractRenewalSummary[]>> {
    return this.http.get<ApiResponse<ContractRenewalSummary[]>>(`${this.baseUrl}/${contractId}/renewals`);
  }

  getContractStatistics(): Observable<ApiResponse<ContractStatistics>> {
    return this.http.get<ApiResponse<ContractStatistics>>(`${this.baseUrl}/statistics`);
  }

  getContractLookup(customerId?: string, status?: ContractStatus): Observable<ApiResponse<ContractLookup[]>> {
    let params = new HttpParams();
    if (customerId) params = params.set('customerId', customerId);
    if (status !== undefined) params = params.set('status', status.toString());
    return this.http.get<ApiResponse<ContractLookup[]>>(`${this.baseUrl}/lookup`, { params });
  }

  // ==================== USAGE TRACKING ====================

  recordUsageHours(contractId: string, request: RecordUsageRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${contractId}/usage/hours`, request);
  }

  recordIncident(contractId: string, request: RecordIncidentRequest): Observable<ApiResponse<Contract>> {
    return this.http.post<ApiResponse<Contract>>(`${this.baseUrl}/${contractId}/usage/incident`, request);
  }

  processAutoRenewals(): Observable<ApiResponse<{ renewedCount: number }>> {
    return this.http.post<ApiResponse<{ renewedCount: number }>>(`${this.baseUrl}/process-auto-renewals`, {});
  }

  // ==================== WARRANTY OPERATIONS ====================

  getWarranties(params?: {
    search?: string;
    productId?: string;
    categoryId?: string;
    type?: number;
    coverageType?: number;
    isActive?: boolean;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PagedResponse<WarrantySummary>> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('searchTerm', params.search);
    if (params?.productId) httpParams = httpParams.set('productId', params.productId);
    if (params?.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params?.type !== undefined) httpParams = httpParams.set('type', params.type.toString());
    if (params?.coverageType !== undefined) httpParams = httpParams.set('coverageType', params.coverageType.toString());
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params?.pageNumber) httpParams = httpParams.set('page', params.pageNumber.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResponse<WarrantySummary>>(`${this.baseUrl}/warranties`, { params: httpParams });
  }

  getWarrantyById(id: string): Observable<ApiResponse<Warranty>> {
    return this.http.get<ApiResponse<Warranty>>(`${this.baseUrl}/warranties/${id}`);
  }

  getWarrantyByCode(code: string): Observable<ApiResponse<Warranty>> {
    return this.http.get<ApiResponse<Warranty>>(`${this.baseUrl}/warranties/by-code/${code}`);
  }

  createWarranty(request: CreateWarrantyRequest): Observable<ApiResponse<Warranty>> {
    return this.http.post<ApiResponse<Warranty>>(`${this.baseUrl}/warranties`, request);
  }

  updateWarranty(id: string, request: UpdateWarrantyRequest): Observable<ApiResponse<Warranty>> {
    return this.http.put<ApiResponse<Warranty>>(`${this.baseUrl}/warranties/${id}`, request);
  }

  deleteWarranty(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/warranties/${id}`);
  }

  getWarrantyLookup(productId?: string, categoryId?: string): Observable<ApiResponse<WarrantyLookup[]>> {
    let params = new HttpParams();
    if (productId) params = params.set('productId', productId);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<ApiResponse<WarrantyLookup[]>>(`${this.baseUrl}/warranties/lookup`, { params });
  }

  checkWarrantyStatus(productId: string, purchaseDate: string): Observable<ApiResponse<CoverageCheckResult>> {
    return this.http.get<ApiResponse<CoverageCheckResult>>(`${this.baseUrl}/warranties/check/${productId}?purchaseDate=${purchaseDate}`);
  }
}
