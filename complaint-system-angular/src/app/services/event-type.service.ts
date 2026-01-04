import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import { EventType, CreateEventTypeRequest, UpdateEventTypeRequest, EventTypeFilters } from '../models/event-type.model';

@Injectable({
  providedIn: 'root'
})
export class EventTypeService {
  private get apiUrl(): string {
    return `${runtimeConfig.apiUrl}/event-types`;
  }

  constructor(private http: HttpClient) {}

  getEventTypes(filters?: EventTypeFilters): Observable<EventType[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.includeInactive !== undefined) {
        params = params.set('includeInactive', filters.includeInactive.toString());
      }
      if (filters.entityType) {
        params = params.set('entityType', filters.entityType);
      }
      if (filters.category) {
        params = params.set('category', filters.category);
      }
      if (filters.companyId) {
        params = params.set('companyId', filters.companyId);
      }
    }

    return this.http.get<EventType[]>(this.apiUrl, { params });
  }

  getEventTypeById(id: string): Observable<EventType> {
    return this.http.get<EventType>(`${this.apiUrl}/${id}`);
  }

  getEventTypeByCode(code: string, companyId?: string): Observable<EventType> {
    let params = new HttpParams();
    if (companyId) {
      params = params.set('companyId', companyId);
    }
    return this.http.get<EventType>(`${this.apiUrl}/by-code/${code}`, { params });
  }

  getEntityTypes(): Observable<{ entityTypes: string[] }> {
    return this.http.get<{ entityTypes: string[] }>(`${this.apiUrl}/entity-types`);
  }

  getCategories(): Observable<{ categories: string[] }> {
    return this.http.get<{ categories: string[] }>(`${this.apiUrl}/categories`);
  }

  createEventType(request: CreateEventTypeRequest): Observable<EventType> {
    return this.http.post<EventType>(this.apiUrl, request);
  }

  updateEventType(id: string, request: UpdateEventTypeRequest): Observable<EventType> {
    return this.http.put<EventType>(`${this.apiUrl}/${id}`, request);
  }

  deleteEventType(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
