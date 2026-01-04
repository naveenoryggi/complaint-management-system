import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  PortalDashboard,
  PortalTicketSummary,
  PortalTicketDetail,
  PortalCreateTicketRequest,
  PortalCategory,
  PortalPriority,
  PortalStatus,
  PortalProductCategory,
  PortalProduct,
  PortalBrand,
  PortalLocation,
  PortalAsset
} from '../models/portal.model';
import { PortalAuthService } from './portal-auth.service';

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

@Injectable({
  providedIn: 'root'
})
export class PortalService {
  private get API_URL(): string {
    return `${runtimeConfig.apiUrl}/portal`;
  }

  constructor(
    private http: HttpClient,
    private authService: PortalAuthService
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Dashboard
  getDashboard(): Observable<PortalDashboard> {
    return this.http.get<PortalDashboard>(`${this.API_URL}/dashboard`, {
      headers: this.getHeaders()
    });
  }

  // Tickets
  getTickets(
    page: number = 1,
    pageSize: number = 10,
    status?: string,
    priority?: string,
    searchTerm?: string,
    sortBy?: string,
    sortDirection?: string
  ): Observable<PagedResult<PortalTicketSummary>> {
    let params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);
    if (priority) params = params.set('priority', priority);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDirection) params = params.set('sortDirection', sortDirection);

    return this.http.get<PagedResult<PortalTicketSummary>>(`${this.API_URL}/tickets`, {
      headers: this.getHeaders(),
      params
    });
  }

  getTicketById(ticketId: string): Observable<PortalTicketDetail> {
    return this.http.get<PortalTicketDetail>(`${this.API_URL}/tickets/${ticketId}`, {
      headers: this.getHeaders()
    });
  }

  getTicketByNumber(ticketNumber: string): Observable<PortalTicketDetail> {
    return this.http.get<PortalTicketDetail>(`${this.API_URL}/tickets/by-number/${ticketNumber}`, {
      headers: this.getHeaders()
    });
  }

  createTicket(request: PortalCreateTicketRequest): Observable<ApiResult<PortalTicketDetail>> {
    return this.http.post<ApiResult<PortalTicketDetail>>(`${this.API_URL}/tickets`, request, {
      headers: this.getHeaders()
    });
  }

  addComment(ticketId: string, content: string): Observable<ApiResult<any>> {
    return this.http.post<ApiResult<any>>(
      `${this.API_URL}/tickets/${ticketId}/comments`,
      { content },
      { headers: this.getHeaders() }
    );
  }

  rateTicket(ticketId: string, rating: number, feedback?: string): Observable<ApiResult<any>> {
    return this.http.post<ApiResult<any>>(
      `${this.API_URL}/tickets/${ticketId}/rate`,
      { rating, feedback },
      { headers: this.getHeaders() }
    );
  }

  reopenTicket(ticketId: string, reason: string): Observable<ApiResult<PortalTicketDetail>> {
    return this.http.post<ApiResult<PortalTicketDetail>>(
      `${this.API_URL}/tickets/${ticketId}/reopen`,
      { reason },
      { headers: this.getHeaders() }
    );
  }

  // Lookups - Categories (for tickets)
  getCategories(): Observable<PortalCategory[]> {
    return this.http.get<PortalCategory[]>(`${this.API_URL}/lookups/categories`, {
      headers: this.getHeaders()
    });
  }

  getPriorities(): Observable<PortalPriority[]> {
    return this.http.get<PortalPriority[]>(`${this.API_URL}/lookups/priorities`, {
      headers: this.getHeaders()
    });
  }

  getStatuses(): Observable<PortalStatus[]> {
    return this.http.get<PortalStatus[]>(`${this.API_URL}/lookups/statuses`, {
      headers: this.getHeaders()
    });
  }

  getLocations(): Observable<PortalLocation[]> {
    return this.http.get<PortalLocation[]>(`${this.API_URL}/lookups/locations`, {
      headers: this.getHeaders()
    });
  }

  getAssets(locationId?: string): Observable<PortalAsset[]> {
    let params = new HttpParams();
    if (locationId) {
      params = params.set('locationId', locationId);
    }
    return this.http.get<PortalAsset[]>(`${this.API_URL}/lookups/assets`, {
      headers: this.getHeaders(),
      params
    });
  }

  // Product Lookups - NEW
  getProductCategories(parentId?: string): Observable<PortalProductCategory[]> {
    let params = new HttpParams();
    if (parentId) {
      params = params.set('parentId', parentId);
    }
    return this.http.get<PortalProductCategory[]>(`${this.API_URL}/lookups/product-categories`, {
      headers: this.getHeaders(),
      params
    });
  }

  getProducts(categoryId?: string, brandId?: string): Observable<PortalProduct[]> {
    let params = new HttpParams();
    if (categoryId) {
      params = params.set('categoryId', categoryId);
    }
    if (brandId) {
      params = params.set('brandId', brandId);
    }
    return this.http.get<PortalProduct[]>(`${this.API_URL}/lookups/products`, {
      headers: this.getHeaders(),
      params
    });
  }

  getBrands(): Observable<PortalBrand[]> {
    return this.http.get<PortalBrand[]>(`${this.API_URL}/lookups/brands`, {
      headers: this.getHeaders()
    });
  }

  // Assets
  getAssetDetails(assetId: string): Observable<PortalAsset> {
    return this.http.get<PortalAsset>(`${this.API_URL}/assets/${assetId}`, {
      headers: this.getHeaders()
    });
  }
}
