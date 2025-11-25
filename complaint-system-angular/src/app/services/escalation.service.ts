import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  EscalationMatrix,
  EscalationPolicy,
  CreateEscalationMatrixRequest,
  CreateEscalationPolicyRequest,
  UpdateEscalationPolicyRequest,
  PolicyResolutionTestRequest,
  PolicyResolutionResult,
  EscalationHistory,
  CreateEscalationLevelRequest,
  UpdateEscalationLevelRequest
} from '../models/escalation.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class EscalationService {
  private apiUrl = `${environment.apiUrl}/escalation`;

  constructor(private http: HttpClient) {}

  // Escalation Matrix Management
  getMatrices(): Observable<EscalationMatrix[]> {
    return this.http.get<ApiResponse<EscalationMatrix[]>>(`${this.apiUrl}/matrices`)
      .pipe(map(response => response.data));
  }

  getMatrixById(id: string): Observable<EscalationMatrix> {
    return this.http.get<ApiResponse<EscalationMatrix>>(`${this.apiUrl}/matrices/${id}`)
      .pipe(map(response => response.data));
  }

  createMatrix(request: CreateEscalationMatrixRequest): Observable<EscalationMatrix> {
    return this.http.post<ApiResponse<EscalationMatrix>>(`${this.apiUrl}/matrices`, request)
      .pipe(map(response => response.data));
  }

  updateMatrix(id: string, request: CreateEscalationMatrixRequest): Observable<EscalationMatrix> {
    return this.http.put<ApiResponse<EscalationMatrix>>(`${this.apiUrl}/matrices/${id}`, request)
      .pipe(map(response => response.data));
  }

  deleteMatrix(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/matrices/${id}`)
      .pipe(map(() => undefined));
  }

  addLevel(matrixId: string, request: CreateEscalationLevelRequest): Observable<EscalationMatrix> {
    return this.http.post<ApiResponse<EscalationMatrix>>(`${this.apiUrl}/matrices/${matrixId}/levels`, request)
      .pipe(map(response => response.data));
  }

  updateLevel(matrixId: string, levelId: string, request: UpdateEscalationLevelRequest): Observable<EscalationMatrix> {
    return this.http.put<ApiResponse<EscalationMatrix>>(`${this.apiUrl}/matrices/${matrixId}/levels/${levelId}`, request)
      .pipe(map(response => response.data));
  }

  deleteLevel(matrixId: string, levelId: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/matrices/${matrixId}/levels/${levelId}`)
      .pipe(map(() => undefined));
  }

  // Escalation Policy Management
  getPolicies(companyId: string): Observable<EscalationPolicy[]> {
    const params = new HttpParams().set('companyId', companyId);
    return this.http.get<ApiResponse<EscalationPolicy[]>>(`${this.apiUrl}/policies`, { params })
      .pipe(map(response => response.data));
  }

  getPolicyById(id: string): Observable<EscalationPolicy> {
    return this.http.get<ApiResponse<EscalationPolicy>>(`${this.apiUrl}/policies/${id}`)
      .pipe(map(response => response.data));
  }

  createPolicy(request: CreateEscalationPolicyRequest): Observable<EscalationPolicy> {
    return this.http.post<ApiResponse<EscalationPolicy>>(`${this.apiUrl}/policies`, request)
      .pipe(map(response => response.data));
  }

  updatePolicy(id: string, request: UpdateEscalationPolicyRequest): Observable<EscalationPolicy> {
    return this.http.put<ApiResponse<EscalationPolicy>>(`${this.apiUrl}/policies/${id}`, request)
      .pipe(map(response => response.data));
  }

  deletePolicy(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/policies/${id}`)
      .pipe(map(() => undefined));
  }

  testPolicyResolution(request: PolicyResolutionTestRequest): Observable<PolicyResolutionResult> {
    return this.http.post<ApiResponse<PolicyResolutionResult>>(`${this.apiUrl}/policies/resolve`, request)
      .pipe(map(response => response.data));
  }

  resolvePolicy(
    companyId: string,
    branchId?: string,
    departmentId?: string,
    sectionId?: string,
    categoryId?: string
  ): Observable<EscalationPolicy | null> {
    let params = new HttpParams().set('companyId', companyId);
    if (branchId) params = params.set('branchId', branchId);
    if (departmentId) params = params.set('departmentId', departmentId);
    if (sectionId) params = params.set('sectionId', sectionId);
    if (categoryId) params = params.set('categoryId', categoryId);

    return this.http.get<ApiResponse<EscalationPolicy | null>>(`${this.apiUrl}/policies/resolve`, { params })
      .pipe(map(response => response.data));
  }

  // Escalation History
  getEscalationHistory(complaintId: string): Observable<EscalationHistory[]> {
    return this.http.get<ApiResponse<EscalationHistory[]>>(`${this.apiUrl}/complaints/${complaintId}/history`)
      .pipe(map(response => response.data));
  }

  escalateComplaint(
    complaintId: string,
    reason: string,
    escalationMatrixId?: string,
    targetLevel?: number
  ): Observable<EscalationHistory> {
    return this.http.post<ApiResponse<EscalationHistory>>(
      `${this.apiUrl}/complaints/${complaintId}/escalate`,
      { reason, escalationMatrixId, targetLevel }
    ).pipe(map(response => response.data));
  }

  acknowledgeEscalation(escalationHistoryId: string, notes?: string): Observable<void> {
    return this.http.post<ApiResponse<void>>(
      `${this.apiUrl}/history/${escalationHistoryId}/acknowledge`,
      { notes }
    ).pipe(map(() => undefined));
  }

  getPendingEscalations(): Observable<EscalationHistory[]> {
    return this.http.get<ApiResponse<EscalationHistory[]>>(`${this.apiUrl}/pending`)
      .pipe(map(response => response.data));
  }
}
