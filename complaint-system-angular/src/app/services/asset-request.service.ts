import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AssetRequest,
  AssetRequestListItem,
  AssetRequestFilterDto,
  AssetRequestStatus,
  CreateReturnRequestDto,
  CreateAssignmentRequestDto,
  CreateTransferRequestDto,
  ApproveRequestDto,
  RejectRequestDto,
  EscalateRequestDto,
  ReassignRequestDto,
  AddRequestCommentDto,
  PendingApprovals,
  RequestApprovalHistory
} from '../models/asset-request.model';

export interface ApiResult<T> {
  isSuccess: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class AssetRequestService {
  private apiUrl = `${environment.apiUrl}/assetrequests`;

  constructor(private http: HttpClient) {}

  // Request Creation

  createReturnRequest(dto: CreateReturnRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/return`, dto);
  }

  createAssignmentRequest(dto: CreateAssignmentRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/assignment`, dto);
  }

  createTransferRequest(dto: CreateTransferRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/transfer`, dto);
  }

  // Request Queries

  getRequests(filter: AssetRequestFilterDto): Observable<PagedResult<AssetRequestListItem>> {
    let params = new HttpParams();

    if (filter.requestType !== undefined) {
      params = params.set('requestType', filter.requestType.toString());
    }
    if (filter.status !== undefined) {
      params = params.set('status', filter.status.toString());
    }
    if (filter.storeId) {
      params = params.set('storeId', filter.storeId);
    }
    if (filter.assetId) {
      params = params.set('assetId', filter.assetId);
    }
    if (filter.requestedById) {
      params = params.set('requestedById', filter.requestedById);
    }
    if (filter.currentApproverId) {
      params = params.set('currentApproverId', filter.currentApproverId);
    }
    if (filter.fromDate) {
      params = params.set('fromDate', filter.fromDate.toISOString());
    }
    if (filter.toDate) {
      params = params.set('toDate', filter.toDate.toISOString());
    }
    if (filter.page) {
      params = params.set('page', filter.page.toString());
    }
    if (filter.pageSize) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<PagedResult<AssetRequestListItem>>(this.apiUrl, { params });
  }

  getRequest(id: string): Observable<ApiResult<AssetRequest>> {
    return this.http.get<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}`);
  }

  getRequestByNumber(requestNumber: string): Observable<ApiResult<AssetRequest>> {
    return this.http.get<ApiResult<AssetRequest>>(`${this.apiUrl}/by-number/${requestNumber}`);
  }

  getPendingApprovals(): Observable<ApiResult<PendingApprovals>> {
    return this.http.get<ApiResult<PendingApprovals>>(`${this.apiUrl}/pending-approvals`);
  }

  getMyRequests(status?: AssetRequestStatus): Observable<ApiResult<AssetRequestListItem[]>> {
    let params = new HttpParams();
    if (status !== undefined) {
      params = params.set('status', status.toString());
    }
    return this.http.get<ApiResult<AssetRequestListItem[]>>(`${this.apiUrl}/my-requests`, { params });
  }

  getAssetRequestHistory(assetId: string): Observable<ApiResult<AssetRequestListItem[]>> {
    return this.http.get<ApiResult<AssetRequestListItem[]>>(`${this.apiUrl}/asset/${assetId}/history`);
  }

  // Approval Workflow

  submitRequest(id: string): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/submit`, {});
  }

  approveRequest(id: string, dto: ApproveRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/approve`, dto);
  }

  rejectRequest(id: string, dto: RejectRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/reject`, dto);
  }

  cancelRequest(id: string, reason?: string): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/cancel`, { reason });
  }

  escalateRequest(id: string, dto: EscalateRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/escalate`, dto);
  }

  reassignRequest(id: string, dto: ReassignRequestDto): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/reassign`, dto);
  }

  addComment(id: string, dto: AddRequestCommentDto): Observable<ApiResult<RequestApprovalHistory>> {
    return this.http.post<ApiResult<RequestApprovalHistory>>(`${this.apiUrl}/${id}/comments`, dto);
  }

  // Request Completion

  completeRequest(id: string): Observable<ApiResult<AssetRequest>> {
    return this.http.post<ApiResult<AssetRequest>>(`${this.apiUrl}/${id}/complete`, {});
  }

  // Authorization Checks

  canApprove(id: string): Observable<{ canApprove: boolean }> {
    return this.http.get<{ canApprove: boolean }>(`${this.apiUrl}/${id}/can-approve`);
  }

  canCancel(id: string): Observable<{ canCancel: boolean }> {
    return this.http.get<{ canCancel: boolean }>(`${this.apiUrl}/${id}/can-cancel`);
  }
}
