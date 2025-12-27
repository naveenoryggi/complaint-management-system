import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Complaint,
  CreateComplaintRequest,
  UpdateComplaintRequest,
  PagedResult,
  ApiResponse,
  ComplaintStatus,
  ComplaintPriority,
  ComplaintHistory
} from '../models/complaint.model';
import {
  AutoAssignRequest,
  AssignToPoolRequest,
  ReassignRequest,
  ValidateAssignmentRequest,
  AssignmentValidationResult,
  ResourcePoolCandidate,
  AssignmentCandidateOptions
} from '../models/assignment.model';

@Injectable({
  providedIn: 'root'
})
export class ComplaintService {
  private apiUrl = `${environment.apiUrl}/complaints`;

  constructor(private http: HttpClient) {}

  getComplaints(
    page: number = 1,
    pageSize: number = 10,
    statusMasterId?: string,  // Changed from enum to Master ID (GUID)
    priorityMasterId?: string,  // Changed from enum to Master ID (GUID)
    searchTerm?: string,
    assignedToId?: string,  // Filter by assigned user (for handlers)
    complainantId?: string,  // Filter by complainant (for complainants)
    unassignedOnly?: boolean,  // Filter for unassigned complaints only
    waitingForResponseOnly?: boolean  // Filter for complaints awaiting handler response
  ): Observable<ApiResponse<PagedResult<Complaint>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (statusMasterId) params = params.set('statusMasterId', statusMasterId);
    if (priorityMasterId) params = params.set('priorityMasterId', priorityMasterId);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (assignedToId) params = params.set('assignedToId', assignedToId);
    if (complainantId) params = params.set('complainantId', complainantId);
    if (unassignedOnly) params = params.set('unassignedOnly', 'true');
    if (waitingForResponseOnly) params = params.set('waitingForResponseOnly', 'true');

    return this.http.get<ApiResponse<PagedResult<Complaint>>>(this.apiUrl, { params });
  }

  getComplaintById(id: string): Observable<ApiResponse<Complaint>> {
    return this.http.get<ApiResponse<Complaint>>(`${this.apiUrl}/${id}`);
  }

  createComplaint(request: CreateComplaintRequest): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(this.apiUrl, request);
  }

  updateComplaint(id: string, request: UpdateComplaintRequest): Observable<ApiResponse<Complaint>> {
    return this.http.put<ApiResponse<Complaint>>(`${this.apiUrl}/${id}`, request);
  }

  assignComplaint(complaintId: string, userId: string): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/assign/${userId}`, {});
  }

  escalateComplaint(complaintId: string, reason: string): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/escalate`, { reason });
  }

  closeComplaint(complaintId: string, resolutionNotes: string): Observable<ApiResponse<Complaint>> {
    return this.http.put<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/close`, JSON.stringify(resolutionNotes), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  reopenComplaint(complaintId: string, reason: string): Observable<ApiResponse<Complaint>> {
    return this.http.put<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/reopen`, { reason });
  }

  deleteComplaint(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  uploadAttachments(complaintId: string, formData: FormData): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${complaintId}/attachments`, formData);
  }

  getAttachments(complaintId: string): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/${complaintId}/attachments`);
  }

  deleteAttachment(complaintId: string, attachmentId: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${complaintId}/attachments/${attachmentId}`);
  }

  getComplaintHistory(complaintId: string): Observable<ApiResponse<ComplaintHistory>> {
    return this.http.get<ApiResponse<ComplaintHistory>>(`${this.apiUrl}/${complaintId}/history`);
  }

  // Advanced Assignment Methods

  autoAssignComplaint(complaintId: string, request: AutoAssignRequest): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/auto-assign`, request);
  }

  assignComplaintToPool(complaintId: string, request: AssignToPoolRequest): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/assign-to-pool`, request);
  }

  reassignComplaint(complaintId: string, request: ReassignRequest): Observable<ApiResponse<Complaint>> {
    return this.http.post<ApiResponse<Complaint>>(`${this.apiUrl}/${complaintId}/reassign`, request);
  }

  getAssignmentCandidates(complaintId: string, options: AssignmentCandidateOptions = {}): Observable<ApiResponse<ResourcePoolCandidate[]>> {
    const params = new HttpParams()
      .set('maxCandidates', (options.maxCandidates || 10).toString())
      .set('includeUserDetails', (options.includeUserDetails !== false).toString())
      .set('calculateSkillScores', (options.calculateSkillScores !== false).toString());

    return this.http.get<ApiResponse<ResourcePoolCandidate[]>>(`${this.apiUrl}/${complaintId}/assignment-candidates`, { params });
  }

  validateAssignment(complaintId: string, request: ValidateAssignmentRequest): Observable<ApiResponse<AssignmentValidationResult>> {
    return this.http.post<ApiResponse<AssignmentValidationResult>>(`${this.apiUrl}/${complaintId}/validate-assignment`, request);
  }
}
