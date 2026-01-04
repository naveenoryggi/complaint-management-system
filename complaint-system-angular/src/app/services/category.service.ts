import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import { Category } from '../models/category.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface CreateCategoryRequest {
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  defaultPriority: number;
  defaultSlaHours: number;
  isActive: boolean;
  displayOrder: number;
}

export interface UpdateCategoryRequest {
  id: string;
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  defaultPriority: number;
  defaultSlaHours: number;
  isActive: boolean;
  displayOrder: number;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/categories`;
  }

  constructor(private http: HttpClient) {}

  getCategories(activeOnly: boolean = true): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${this.apiUrl}?activeOnly=${activeOnly}`);
  }

  createCategory(request: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, request);
  }

  updateCategory(id: string, request: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.apiUrl}/${id}`, request);
  }

  deleteCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
