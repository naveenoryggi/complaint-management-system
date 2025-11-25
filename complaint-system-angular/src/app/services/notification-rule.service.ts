import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  NotificationRule,
  CreateNotificationRuleRequest,
  UpdateNotificationRuleRequest,
  EventType
} from '../models/communication.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class NotificationRuleService {
  private apiUrl = `${environment.apiUrl}/event-communication-rules`;
  private eventTypesUrl = `${environment.apiUrl}/event-types`;

  constructor(private http: HttpClient) {}

  /**
   * Get all notification rules
   * @param includeInactive Include inactive rules
   */
  getNotificationRules(includeInactive: boolean = false): Observable<NotificationRule[]> {
    let params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    return this.http.get<ApiResponse<NotificationRule[]>>(this.apiUrl, { params })
      .pipe(map(response => response.data || []));
  }

  /**
   * Get a single notification rule by ID
   * @param id The rule ID
   */
  getNotificationRuleById(id: string): Observable<NotificationRule> {
    return this.http.get<ApiResponse<NotificationRule>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data!));
  }

  /**
   * Get notification rules by event type
   * @param eventTypeId The event type ID
   */
  getNotificationRulesByEventType(eventTypeId: string): Observable<NotificationRule[]> {
    return this.http.get<ApiResponse<NotificationRule[]>>(`${this.apiUrl}/by-event/${eventTypeId}`)
      .pipe(map(response => response.data || []));
  }

  /**
   * Create a new notification rule
   * @param request The rule creation request
   */
  createNotificationRule(request: CreateNotificationRuleRequest): Observable<ApiResponse<NotificationRule>> {
    return this.http.post<ApiResponse<NotificationRule>>(this.apiUrl, request);
  }

  /**
   * Update an existing notification rule
   * @param id The rule ID
   * @param request The rule update request
   */
  updateNotificationRule(id: string, request: UpdateNotificationRuleRequest): Observable<ApiResponse<NotificationRule>> {
    return this.http.put<ApiResponse<NotificationRule>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a notification rule
   * @param id The rule ID
   */
  deleteNotificationRule(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get all event types
   * @param includeInactive Include inactive event types
   */
  getEventTypes(includeInactive: boolean = false): Observable<EventType[]> {
    let params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    return this.http.get<EventType[] | ApiResponse<EventType[]>>(this.eventTypesUrl, { params })
      .pipe(map(response => {
        // Handle both direct array response and wrapped ApiResponse
        if (Array.isArray(response)) {
          return response;
        }
        return response.data || [];
      }));
  }

  /**
   * Get event type by ID
   * @param id The event type ID
   */
  getEventTypeById(id: string): Observable<EventType> {
    return this.http.get<EventType | ApiResponse<EventType>>(`${this.eventTypesUrl}/${id}`)
      .pipe(map(response => {
        // Handle both direct object response and wrapped ApiResponse
        if ('data' in response) {
          return response.data!;
        }
        return response as EventType;
      }));
  }

  /**
   * Get event type by code
   * @param code The event type code
   */
  getEventTypeByCode(code: string): Observable<EventType | null> {
    return this.http.get<EventType | ApiResponse<EventType>>(`${this.eventTypesUrl}/by-code/${code}`)
      .pipe(map(response => {
        // Handle both direct object response and wrapped ApiResponse
        if ('data' in response) {
          return response.data || null;
        }
        return response as EventType;
      }));
  }
}
