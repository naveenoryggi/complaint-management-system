import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  StockItem,
  StockItemLookup,
  CreateStockItemRequest,
  UpdateStockItemRequest,
  AdjustStockRequest,
  TransferStockRequest,
  StockMovement,
  CreateStockMovementRequest,
  StockMovementFilter,
  MovementTypeLookup,
  StockLocation,
  StockLocationLookup,
  CreateStockLocationRequest,
  UpdateStockLocationRequest,
  StockMovementType,
  StockMovementStatus
} from '../models/stock.model';

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
    page: number;
    pageSize: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
  };
}

@Injectable({
  providedIn: 'root'
})
export class StockService {
  private get stockItemsUrl(): string {
    return `${runtimeConfig.apiUrl}/StockItems`;
  }
  private get stockMovementsUrl(): string {
    return `${runtimeConfig.apiUrl}/StockMovements`;
  }
  private get stockLocationsUrl(): string {
    return `${runtimeConfig.apiUrl}/StockLocations`;
  }

  constructor(private http: HttpClient) {}

  // ==================== STOCK ITEM OPERATIONS ====================

  getStockItems(params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    locationId?: string;
    stockCategoryId?: string;
    productId?: string;
    lowStock?: boolean;
  }): Observable<PagedResponse<StockItem>> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params?.locationId) httpParams = httpParams.set('locationId', params.locationId);
    if (params?.stockCategoryId) httpParams = httpParams.set('stockCategoryId', params.stockCategoryId);
    if (params?.productId) httpParams = httpParams.set('productId', params.productId);
    if (params?.lowStock !== undefined) httpParams = httpParams.set('lowStock', params.lowStock.toString());

    return this.http.get<PagedResponse<StockItem>>(this.stockItemsUrl, { params: httpParams });
  }

  getStockItemById(id: string): Observable<ApiResponse<StockItem>> {
    return this.http.get<ApiResponse<StockItem>>(`${this.stockItemsUrl}/${id}`);
  }

  getStockItemByProductAndLocation(productId: string, locationId: string | null, stockCategoryId: string): Observable<ApiResponse<StockItem>> {
    let params = new HttpParams()
      .set('productId', productId)
      .set('stockCategoryId', stockCategoryId);
    if (locationId) params = params.set('locationId', locationId);
    return this.http.get<ApiResponse<StockItem>>(`${this.stockItemsUrl}/by-product-location`, { params });
  }

  getStockItemLookup(params?: {
    locationId?: string;
    stockCategoryId?: string;
    availableOnly?: boolean;
  }): Observable<ApiResponse<StockItemLookup[]>> {
    let httpParams = new HttpParams();
    if (params?.locationId) httpParams = httpParams.set('locationId', params.locationId);
    if (params?.stockCategoryId) httpParams = httpParams.set('stockCategoryId', params.stockCategoryId);
    if (params?.availableOnly !== undefined) httpParams = httpParams.set('availableOnly', params.availableOnly.toString());

    return this.http.get<ApiResponse<StockItemLookup[]>>(`${this.stockItemsUrl}/lookup`, { params: httpParams });
  }

  createStockItem(request: CreateStockItemRequest): Observable<ApiResponse<StockItem>> {
    return this.http.post<ApiResponse<StockItem>>(this.stockItemsUrl, request);
  }

  updateStockItem(id: string, request: UpdateStockItemRequest): Observable<ApiResponse<StockItem>> {
    return this.http.put<ApiResponse<StockItem>>(`${this.stockItemsUrl}/${id}`, request);
  }

  deleteStockItem(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.stockItemsUrl}/${id}`);
  }

  adjustStock(id: string, request: AdjustStockRequest): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(`${this.stockItemsUrl}/${id}/adjust`, request);
  }

  transferStock(id: string, request: TransferStockRequest): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(`${this.stockItemsUrl}/${id}/transfer`, request);
  }

  getLowStockItems(): Observable<ApiResponse<StockItem[]>> {
    return this.http.get<ApiResponse<StockItem[]>>(`${this.stockItemsUrl}/low-stock`);
  }

  getExpiringItems(daysAhead: number = 30): Observable<ApiResponse<StockItem[]>> {
    return this.http.get<ApiResponse<StockItem[]>>(`${this.stockItemsUrl}/expiring`, {
      params: new HttpParams().set('daysAhead', daysAhead.toString())
    });
  }

  // ==================== STOCK MOVEMENT OPERATIONS ====================

  getStockMovements(params?: {
    page?: number;
    pageSize?: number;
    fromDate?: string;
    toDate?: string;
    movementType?: StockMovementType;
    status?: StockMovementStatus;
    productId?: string;
    locationId?: string;
    stockCategoryId?: string;
    customerId?: string;
    searchTerm?: string;
  }): Observable<PagedResponse<StockMovement>> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params?.toDate) httpParams = httpParams.set('toDate', params.toDate);
    if (params?.movementType !== undefined) httpParams = httpParams.set('movementType', params.movementType.toString());
    if (params?.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
    if (params?.productId) httpParams = httpParams.set('productId', params.productId);
    if (params?.locationId) httpParams = httpParams.set('locationId', params.locationId);
    if (params?.stockCategoryId) httpParams = httpParams.set('stockCategoryId', params.stockCategoryId);
    if (params?.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params?.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);

    return this.http.get<PagedResponse<StockMovement>>(this.stockMovementsUrl, { params: httpParams });
  }

  getStockMovementById(id: string): Observable<ApiResponse<StockMovement>> {
    return this.http.get<ApiResponse<StockMovement>>(`${this.stockMovementsUrl}/${id}`);
  }

  getStockMovementByNumber(movementNumber: string): Observable<ApiResponse<StockMovement>> {
    return this.http.get<ApiResponse<StockMovement>>(`${this.stockMovementsUrl}/by-number/${movementNumber}`);
  }

  getMovementsByStockItem(stockItemId: string, limit: number = 50): Observable<ApiResponse<StockMovement[]>> {
    return this.http.get<ApiResponse<StockMovement[]>>(`${this.stockMovementsUrl}/by-stock-item/${stockItemId}`, {
      params: new HttpParams().set('limit', limit.toString())
    });
  }

  getMovementsByAsset(assetId: string, limit: number = 50): Observable<ApiResponse<StockMovement[]>> {
    return this.http.get<ApiResponse<StockMovement[]>>(`${this.stockMovementsUrl}/by-asset/${assetId}`, {
      params: new HttpParams().set('limit', limit.toString())
    });
  }

  getMovementsByProduct(productId: string, limit: number = 50): Observable<ApiResponse<StockMovement[]>> {
    return this.http.get<ApiResponse<StockMovement[]>>(`${this.stockMovementsUrl}/by-product/${productId}`, {
      params: new HttpParams().set('limit', limit.toString())
    });
  }

  createStockMovement(request: CreateStockMovementRequest): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(this.stockMovementsUrl, request);
  }

  approveMovement(id: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.stockMovementsUrl}/${id}/approve`, {});
  }

  rejectMovement(id: string, reason: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.stockMovementsUrl}/${id}/reject`, { reason });
  }

  cancelMovement(id: string, reason: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.stockMovementsUrl}/${id}/cancel`, { reason });
  }

  reverseMovement(id: string, reason: string): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(`${this.stockMovementsUrl}/${id}/reverse`, { reason });
  }

  getMovementTypes(): Observable<MovementTypeLookup[]> {
    return this.http.get<MovementTypeLookup[]>(`${this.stockMovementsUrl}/movement-types`);
  }

  getMovementStatuses(): Observable<{ value: number; name: string }[]> {
    return this.http.get<{ value: number; name: string }[]>(`${this.stockMovementsUrl}/statuses`);
  }

  // ==================== STOCK LOCATION OPERATIONS ====================

  getStockLocations(params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    isActive?: boolean;
  }): Observable<PagedResponse<StockLocation>> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());

    return this.http.get<PagedResponse<StockLocation>>(this.stockLocationsUrl, { params: httpParams });
  }

  getStockLocationById(id: string): Observable<ApiResponse<StockLocation>> {
    return this.http.get<ApiResponse<StockLocation>>(`${this.stockLocationsUrl}/${id}`);
  }

  getStockLocationByCode(code: string): Observable<ApiResponse<StockLocation>> {
    return this.http.get<ApiResponse<StockLocation>>(`${this.stockLocationsUrl}/code/${code}`);
  }

  getStockLocationLookup(activeOnly: boolean = true): Observable<ApiResponse<StockLocationLookup[]>> {
    return this.http.get<ApiResponse<StockLocationLookup[]>>(`${this.stockLocationsUrl}/lookup`, {
      params: new HttpParams().set('activeOnly', activeOnly.toString())
    });
  }

  getStockLocationHierarchy(parentId?: string): Observable<ApiResponse<StockLocation[]>> {
    let params = new HttpParams();
    if (parentId) params = params.set('parentId', parentId);
    return this.http.get<ApiResponse<StockLocation[]>>(`${this.stockLocationsUrl}/hierarchy`, { params });
  }

  createStockLocation(request: CreateStockLocationRequest): Observable<ApiResponse<StockLocation>> {
    return this.http.post<ApiResponse<StockLocation>>(this.stockLocationsUrl, request);
  }

  updateStockLocation(id: string, request: UpdateStockLocationRequest): Observable<ApiResponse<StockLocation>> {
    return this.http.put<ApiResponse<StockLocation>>(`${this.stockLocationsUrl}/${id}`, request);
  }

  deleteStockLocation(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.stockLocationsUrl}/${id}`);
  }

  setDefaultLocation(id: string): Observable<ApiResponse<StockLocation>> {
    return this.http.post<ApiResponse<StockLocation>>(`${this.stockLocationsUrl}/${id}/set-default`, {});
  }
}
