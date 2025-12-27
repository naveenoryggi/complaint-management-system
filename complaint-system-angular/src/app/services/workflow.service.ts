import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CategoryWorkflow,
  CategoryWorkflowStatus,
  CategoryWorkflowTransition,
  CreateWorkflowRequest,
  AddStatusRequest,
  AddTransitionRequest,
  AllowedTransitionsResponse,
  TransitionValidationResponse
} from '../models/workflow.model';

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private apiUrl = `${environment.apiUrl}/workflows`;

  constructor(private http: HttpClient) {}

  // Workflow Management
  getAllWorkflows(companyId?: string): Observable<any> {
    let params = new HttpParams();
    if (companyId) {
      params = params.set('companyId', companyId);
    }
    return this.http.get(`${this.apiUrl}`, { params });
  }

  getWorkflowForCategory(categoryId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/category/${categoryId}`);
  }

  createWorkflow(request: CreateWorkflowRequest): Observable<any> {
    return this.http.post(this.apiUrl, request);
  }

  // Status Management
  getWorkflowStatuses(categoryId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/categories/${categoryId}/statuses`);
  }

  getInitialStatus(categoryId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/categories/${categoryId}/initial-status`);
  }

  addStatusToWorkflow(workflowId: string, request: AddStatusRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${workflowId}/statuses`, request);
  }

  removeStatusFromWorkflow(workflowId: string, statusId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${workflowId}/statuses/${statusId}`);
  }

  // Transition Management
  addTransitionRule(workflowId: string, request: AddTransitionRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${workflowId}/transitions`, request);
  }

  removeTransitionRule(workflowId: string, transitionId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${workflowId}/transitions/${transitionId}`);
  }

  getAllowedTransitions(categoryId: string, currentStatusId: string): Observable<AllowedTransitionsResponse> {
    let params = new HttpParams()
      .set('categoryId', categoryId)
      .set('currentStatusId', currentStatusId);
    return this.http.get<AllowedTransitionsResponse>(`${this.apiUrl}/allowed-transitions`, { params });
  }

  checkTransitionAllowed(
    categoryId: string,
    fromStatusId: string,
    toStatusId: string,
    userId: string
  ): Observable<TransitionValidationResponse> {
    return this.http.post<TransitionValidationResponse>(`${this.apiUrl}/check-transition`, {
      categoryId,
      fromStatusId,
      toStatusId,
      userId
    });
  }

  // Complaint Operations
  transitionComplaint(complaintId: string, newStatusId: string, comment?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/complaints/${complaintId}/transition`, {
      newStatusId,
      comment
    });
  }
}
