import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { runtimeConfig } from '../../environments/environment';
import { Section, CreateSectionRequest, UpdateSectionRequest } from '../models/section.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SectionService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/sections`;
  }

  constructor(private http: HttpClient) {}

  /**
   * Get all sections for a department
   * @param departmentId The department ID
   * @param activeOnly Filter to only active sections
   */
  getSections(departmentId: string, activeOnly: boolean = false): Observable<Section[]> {
    const params = new HttpParams()
      .set('departmentId', departmentId)
      .set('activeOnly', activeOnly.toString());

    return this.http.get<ApiResponse<Section[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single section by ID
   * @param id The section ID
   */
  getSectionById(id: string): Observable<Section> {
    return this.http.get<ApiResponse<Section>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Create a new section
   * @param request The section creation request
   */
  createSection(request: CreateSectionRequest): Observable<ApiResponse<Section>> {
    return this.http.post<ApiResponse<Section>>(this.apiUrl, request);
  }

  /**
   * Update an existing section
   * @param id The section ID
   * @param request The section update request
   */
  updateSection(id: string, request: UpdateSectionRequest): Observable<ApiResponse<Section>> {
    return this.http.put<ApiResponse<Section>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a section
   * @param id The section ID
   */
  deleteSection(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
