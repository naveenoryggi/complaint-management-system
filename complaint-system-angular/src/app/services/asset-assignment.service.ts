import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  AssetAssignment,
  AssetAssignmentSummary,
  AssignAssetRequest,
  ReturnAssetRequest,
  TransferAssetRequest,
  ExtendAssignmentRequest,
  AcknowledgeAssetRequest,
  ReportAssetIssueRequest,
  ReportIssueRequest,
  EmployeeAssetDashboard,
  AssetAssignmentStatistics,
  AssetAssignmentPurpose
} from '../models/asset-assignment.model';

@Injectable({
  providedIn: 'root'
})
export class AssetAssignmentService {
  private get API_URL(): string {
    return `${runtimeConfig.apiUrl}/assetassignments`;
  }

  constructor(private http: HttpClient) {}

  // ========== Assignment Operations ==========

  /**
   * Assign an asset to an employee
   */
  assignAsset(request: AssignAssetRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(this.API_URL, request);
  }

  /**
   * Return an assigned asset
   */
  returnAsset(assignmentId: string, request: ReturnAssetRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/return`, request);
  }

  /**
   * Transfer asset to another employee
   */
  transferAsset(assignmentId: string, request: TransferAssetRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/transfer`, request);
  }

  /**
   * Extend an assignment's expected return date
   */
  extendAssignment(assignmentId: string, request: ExtendAssignmentRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/extend`, request);
  }

  /**
   * Acknowledge receipt of an asset
   */
  acknowledgeAsset(assignmentId: string, request: AcknowledgeAssetRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/acknowledge`, request);
  }

  /**
   * Report an issue with an assigned asset (detailed)
   */
  reportAssetIssue(assignmentId: string, request: ReportAssetIssueRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/report-issue`, request);
  }

  /**
   * Report an issue with an assigned asset (simplified)
   */
  reportIssue(assignmentId: string, request: ReportIssueRequest): Observable<AssetAssignment> {
    return this.http.post<AssetAssignment>(`${this.API_URL}/${assignmentId}/report-issue`, request);
  }

  // ========== Query Operations ==========

  /**
   * Get assignment by ID
   */
  getById(id: string): Observable<AssetAssignment> {
    return this.http.get<AssetAssignment>(`${this.API_URL}/${id}`);
  }

  /**
   * Get assignment by assignment number
   */
  getByAssignmentNumber(assignmentNumber: string): Observable<AssetAssignment> {
    return this.http.get<AssetAssignment>(`${this.API_URL}/by-number/${assignmentNumber}`);
  }

  /**
   * Get all assignments with optional filters
   */
  getAssignments(filters?: {
    isActive?: boolean;
    assetId?: string;
    assignedToUserId?: string;
    departmentId?: string;
    purpose?: AssetAssignmentPurpose;
    fromDate?: Date;
    toDate?: Date;
  }): Observable<AssetAssignmentSummary[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.isActive !== undefined) {
        params = params.set('isActive', filters.isActive.toString());
      }
      if (filters.assetId) {
        params = params.set('assetId', filters.assetId);
      }
      if (filters.assignedToUserId) {
        params = params.set('assignedToUserId', filters.assignedToUserId);
      }
      if (filters.departmentId) {
        params = params.set('departmentId', filters.departmentId);
      }
      if (filters.purpose !== undefined) {
        params = params.set('purpose', filters.purpose.toString());
      }
      if (filters.fromDate) {
        params = params.set('fromDate', filters.fromDate.toISOString());
      }
      if (filters.toDate) {
        params = params.set('toDate', filters.toDate.toISOString());
      }
    }

    return this.http.get<AssetAssignmentSummary[]>(this.API_URL, { params });
  }

  /**
   * Get assignment history for a specific asset
   */
  getAssetHistory(assetId: string): Observable<AssetAssignmentSummary[]> {
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/asset/${assetId}/history`);
  }

  /**
   * Get current active assignment for an asset
   */
  getCurrentAssignment(assetId: string): Observable<AssetAssignment> {
    return this.http.get<AssetAssignment>(`${this.API_URL}/asset/${assetId}/current`);
  }

  /**
   * Check if an asset is available for assignment
   */
  isAssetAvailable(assetId: string): Observable<{ assetId: string; isAvailable: boolean }> {
    return this.http.get<{ assetId: string; isAvailable: boolean }>(`${this.API_URL}/asset/${assetId}/available`);
  }

  /**
   * Get all active assignments for an employee
   */
  getEmployeeActiveAssignments(userId: string): Observable<AssetAssignmentSummary[]> {
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/employee/${userId}/active`);
  }

  /**
   * Get assignment history for an employee
   */
  getEmployeeHistory(userId: string, limit?: number): Observable<AssetAssignmentSummary[]> {
    let params = new HttpParams();
    if (limit) {
      params = params.set('limit', limit.toString());
    }
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/employee/${userId}/history`, { params });
  }

  /**
   * Get overdue assignments
   */
  getOverdueAssignments(): Observable<AssetAssignmentSummary[]> {
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/overdue`);
  }

  /**
   * Get assignments pending acknowledgement
   */
  getPendingAcknowledgements(userId?: string): Observable<AssetAssignmentSummary[]> {
    let params = new HttpParams();
    if (userId) {
      params = params.set('userId', userId);
    }
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/pending-acknowledgement`, { params });
  }

  /**
   * Get upcoming returns
   */
  getUpcomingReturns(days: number = 7): Observable<AssetAssignmentSummary[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<AssetAssignmentSummary[]>(`${this.API_URL}/upcoming-returns`, { params });
  }

  // ========== Dashboard Operations ==========

  /**
   * Get current user's asset dashboard
   */
  getMyDashboard(): Observable<EmployeeAssetDashboard> {
    return this.http.get<EmployeeAssetDashboard>(`${this.API_URL}/my-dashboard`);
  }

  /**
   * Get employee's asset dashboard (admin view)
   */
  getEmployeeDashboard(userId: string): Observable<EmployeeAssetDashboard> {
    return this.http.get<EmployeeAssetDashboard>(`${this.API_URL}/employee/${userId}/dashboard`);
  }

  /**
   * Get company-wide assignment statistics
   */
  getStatistics(): Observable<AssetAssignmentStatistics> {
    return this.http.get<AssetAssignmentStatistics>(`${this.API_URL}/statistics`);
  }
}
