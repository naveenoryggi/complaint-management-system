import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import {
  Project,
  ProjectSummary,
  ProjectLookup,
  CreateProjectRequest,
  UpdateProjectRequest,
  ProjectMilestone,
  CreateMilestoneRequest,
  UpdateMilestoneRequest,
  ProjectTask,
  CreateTaskRequest,
  UpdateTaskRequest,
  ProjectDocument,
  UploadDocumentRequest,
  ProjectTeamMember,
  AddTeamMemberRequest,
  UpdateTeamMemberRequest,
  ProjectStatistics,
  ProjectStatus
} from '../models/project.model';

export interface PagedResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private get baseUrl(): string {
    return `${runtimeConfig.apiUrl}/projects`;
  }

  constructor(private http: HttpClient) {}

  // ============ Projects ============

  getProjects(params?: {
    searchTerm?: string;
    status?: ProjectStatus;
    customerId?: string;
    managerId?: string;
    page?: number;
    pageSize?: number;
  }): Observable<PagedResponse<ProjectSummary>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
      if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
      if (params.managerId) httpParams = httpParams.set('managerId', params.managerId);
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResponse<ProjectSummary>>(this.baseUrl, { params: httpParams });
  }

  getProjectById(id: string): Observable<ApiResponse<Project>> {
    return this.http.get<ApiResponse<Project>>(`${this.baseUrl}/${id}`);
  }

  getProjectByCode(code: string): Observable<ApiResponse<Project>> {
    return this.http.get<ApiResponse<Project>>(`${this.baseUrl}/by-code/${code}`);
  }

  createProject(request: CreateProjectRequest): Observable<ApiResponse<Project>> {
    return this.http.post<ApiResponse<Project>>(this.baseUrl, request);
  }

  updateProject(id: string, request: UpdateProjectRequest): Observable<ApiResponse<Project>> {
    return this.http.put<ApiResponse<Project>>(`${this.baseUrl}/${id}`, request);
  }

  deleteProject(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  getProjectLookup(customerId?: string): Observable<ApiResponse<ProjectLookup[]>> {
    let params = new HttpParams();
    if (customerId) params = params.set('customerId', customerId);
    return this.http.get<ApiResponse<ProjectLookup[]>>(`${this.baseUrl}/lookup`, { params });
  }

  getProjectsByCustomer(customerId: string): Observable<ApiResponse<ProjectSummary[]>> {
    return this.http.get<ApiResponse<ProjectSummary[]>>(`${this.baseUrl}/by-customer/${customerId}`);
  }

  getProjectStatistics(projectId: string): Observable<ApiResponse<ProjectStatistics>> {
    return this.http.get<ApiResponse<ProjectStatistics>>(`${this.baseUrl}/${projectId}/statistics`);
  }

  // ============ Milestones ============

  getMilestones(projectId: string): Observable<ApiResponse<ProjectMilestone[]>> {
    return this.http.get<ApiResponse<ProjectMilestone[]>>(`${this.baseUrl}/${projectId}/milestones`);
  }

  getMilestoneById(milestoneId: string): Observable<ApiResponse<ProjectMilestone>> {
    return this.http.get<ApiResponse<ProjectMilestone>>(`${this.baseUrl}/milestones/${milestoneId}`);
  }

  createMilestone(projectId: string, request: CreateMilestoneRequest): Observable<ApiResponse<ProjectMilestone>> {
    return this.http.post<ApiResponse<ProjectMilestone>>(`${this.baseUrl}/${projectId}/milestones`, request);
  }

  updateMilestone(milestoneId: string, request: UpdateMilestoneRequest): Observable<ApiResponse<ProjectMilestone>> {
    return this.http.put<ApiResponse<ProjectMilestone>>(`${this.baseUrl}/milestones/${milestoneId}`, request);
  }

  deleteMilestone(milestoneId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/milestones/${milestoneId}`);
  }

  // ============ Tasks ============

  getTasks(projectId: string, milestoneId?: string): Observable<ApiResponse<ProjectTask[]>> {
    let params = new HttpParams();
    if (milestoneId) params = params.set('milestoneId', milestoneId);
    return this.http.get<ApiResponse<ProjectTask[]>>(`${this.baseUrl}/${projectId}/tasks`, { params });
  }

  getTaskById(taskId: string): Observable<ApiResponse<ProjectTask>> {
    return this.http.get<ApiResponse<ProjectTask>>(`${this.baseUrl}/tasks/${taskId}`);
  }

  createTask(projectId: string, request: CreateTaskRequest): Observable<ApiResponse<ProjectTask>> {
    return this.http.post<ApiResponse<ProjectTask>>(`${this.baseUrl}/${projectId}/tasks`, request);
  }

  updateTask(taskId: string, request: UpdateTaskRequest): Observable<ApiResponse<ProjectTask>> {
    return this.http.put<ApiResponse<ProjectTask>>(`${this.baseUrl}/tasks/${taskId}`, request);
  }

  deleteTask(taskId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/tasks/${taskId}`);
  }

  // ============ Documents ============

  getDocuments(projectId: string): Observable<ApiResponse<ProjectDocument[]>> {
    return this.http.get<ApiResponse<ProjectDocument[]>>(`${this.baseUrl}/${projectId}/documents`);
  }

  getDocumentById(documentId: string): Observable<ApiResponse<ProjectDocument>> {
    return this.http.get<ApiResponse<ProjectDocument>>(`${this.baseUrl}/documents/${documentId}`);
  }

  uploadDocument(projectId: string, request: UploadDocumentRequest): Observable<ApiResponse<ProjectDocument>> {
    return this.http.post<ApiResponse<ProjectDocument>>(`${this.baseUrl}/${projectId}/documents`, request);
  }

  deleteDocument(documentId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/documents/${documentId}`);
  }

  // ============ Team Members ============

  getTeamMembers(projectId: string): Observable<ApiResponse<ProjectTeamMember[]>> {
    return this.http.get<ApiResponse<ProjectTeamMember[]>>(`${this.baseUrl}/${projectId}/team`);
  }

  addTeamMember(projectId: string, request: AddTeamMemberRequest): Observable<ApiResponse<ProjectTeamMember>> {
    return this.http.post<ApiResponse<ProjectTeamMember>>(`${this.baseUrl}/${projectId}/team`, request);
  }

  updateTeamMember(teamMemberId: string, request: UpdateTeamMemberRequest): Observable<ApiResponse<ProjectTeamMember>> {
    return this.http.put<ApiResponse<ProjectTeamMember>>(`${this.baseUrl}/team/${teamMemberId}`, request);
  }

  removeTeamMember(teamMemberId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/team/${teamMemberId}`);
  }
}
