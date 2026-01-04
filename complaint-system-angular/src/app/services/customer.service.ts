import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import { Customer, CustomerSummary, CreateCustomerRequest, UpdateCustomerRequest } from '../models/customer.model';

export interface PagedResponse<T> {
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

export interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private get baseUrl(): string {
    return `${runtimeConfig.apiUrl}/customers`;
  }

  constructor(private http: HttpClient) {}

  getCustomers(params?: {
    search?: string;
    status?: number;
    type?: number;
    segment?: number;
    city?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PagedResponse<CustomerSummary>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
      if (params.type !== undefined) httpParams = httpParams.set('type', params.type.toString());
      if (params.segment !== undefined) httpParams = httpParams.set('segment', params.segment.toString());
      if (params.city) httpParams = httpParams.set('city', params.city);
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }

    return this.http.get<PagedResponse<CustomerSummary>>(this.baseUrl, { params: httpParams });
  }

  getCustomerById(id: string): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.baseUrl}/${id}`);
  }

  getCustomerByCode(code: string): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.baseUrl}/by-code/${code}`);
  }

  createCustomer(request: CreateCustomerRequest): Observable<ApiResponse<Customer>> {
    return this.http.post<ApiResponse<Customer>>(this.baseUrl, request);
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): Observable<ApiResponse<Customer>> {
    return this.http.put<ApiResponse<Customer>>(`${this.baseUrl}/${id}`, request);
  }

  deleteCustomer(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  getCustomerLookup(search?: string): Observable<ApiResponse<{ id: string; code: string; name: string }[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<{ id: string; code: string; name: string }[]>>(`${this.baseUrl}/lookup`, { params });
  }
}
