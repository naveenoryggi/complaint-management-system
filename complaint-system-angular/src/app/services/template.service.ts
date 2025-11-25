import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  CommunicationTemplate,
  CreateCommunicationTemplateRequest,
  UpdateCommunicationTemplateRequest,
  CommunicationChannel
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
export class TemplateService {
  private apiUrl = `${environment.apiUrl}/communication-templates`;

  constructor(private http: HttpClient) {}

  /**
   * Get all communication templates
   * @param includeInactive Include inactive templates
   * @param channel Filter by communication channel
   */
  getTemplates(includeInactive: boolean = false, channel?: CommunicationChannel): Observable<CommunicationTemplate[]> {
    let params = new HttpParams()
      .set('includeInactive', includeInactive.toString());

    if (channel !== undefined) {
      params = params.set('channel', channel.toString());
    }

    return this.http.get<CommunicationTemplate[] | ApiResponse<CommunicationTemplate[]>>(this.apiUrl, { params })
      .pipe(map(response => {
        // Handle both direct array response and wrapped ApiResponse
        if (Array.isArray(response)) {
          return response;
        }
        return response.data || [];
      }));
  }

  /**
   * Get a single template by ID
   * @param id The template ID
   */
  getTemplateById(id: string): Observable<CommunicationTemplate> {
    return this.http.get<CommunicationTemplate | ApiResponse<CommunicationTemplate>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => {
        // Handle both direct object response and wrapped ApiResponse
        if ('data' in response) {
          return response.data!;
        }
        return response as CommunicationTemplate;
      }));
  }

  /**
   * Get template by code
   * @param code The template code
   */
  getTemplateByCode(code: string): Observable<CommunicationTemplate | null> {
    return this.http.get<CommunicationTemplate | ApiResponse<CommunicationTemplate>>(`${this.apiUrl}/by-code/${code}`)
      .pipe(map(response => {
        // Handle both direct object response and wrapped ApiResponse
        if ('data' in response) {
          return response.data || null;
        }
        return response as CommunicationTemplate;
      }));
  }

  /**
   * Create a new communication template
   * @param request The template creation request
   */
  createTemplate(request: CreateCommunicationTemplateRequest): Observable<ApiResponse<CommunicationTemplate>> {
    return this.http.post<ApiResponse<CommunicationTemplate>>(this.apiUrl, request);
  }

  /**
   * Update an existing communication template
   * @param id The template ID
   * @param request The template update request
   */
  updateTemplate(id: string, request: UpdateCommunicationTemplateRequest): Observable<ApiResponse<CommunicationTemplate>> {
    return this.http.put<ApiResponse<CommunicationTemplate>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete a communication template
   * @param id The template ID
   */
  deleteTemplate(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get available placeholders for templates
   */
  getAvailablePlaceholders(): string[] {
    return [
      '{complaintId}',
      '{complaintNumber}',
      '{title}',
      '{description}',
      '{categoryName}',
      '{priorityName}',
      '{statusName}',
      '{complainantName}',
      '{complainantEmail}',
      '{complainantEmployeeCode}',
      '{assignedToName}',
      '{assignedToEmail}',
      '{assignedToPhone}',
      '{escalationLevel}',
      '{escalationReason}',
      '{closedBy}',
      '{resolution}',
      '{createdDate}',
      '{dueDate}',
      '{closedDate}',
      '{companyName}',
      '{branchName}',
      '{departmentName}',
      '{sectionName}'
    ];
  }

  /**
   * Preview template with sample data
   * @param templateBody The template body with placeholders
   * @param sampleData Sample data to replace placeholders
   */
  previewTemplate(templateBody: string, sampleData: Record<string, string>): string {
    let preview = templateBody;

    Object.keys(sampleData).forEach(key => {
      const placeholder = `{${key}}`;
      preview = preview.replace(new RegExp(placeholder, 'g'), sampleData[key]);
    });

    return preview;
  }
}
