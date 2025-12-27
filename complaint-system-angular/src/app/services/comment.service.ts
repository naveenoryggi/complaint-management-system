import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Comment } from '../models/comment.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface CreateCommentRequest {
  complaintId: string;
  comment: string;
  isInternal: boolean;
  mentionedUserIds?: string[];  // User IDs to notify about this comment
}

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/complaints`;

  constructor(private http: HttpClient) {}

  getComments(complaintId: string, includeInternal: boolean = true): Observable<ApiResponse<Comment[]>> {
    return this.http.get<ApiResponse<Comment[]>>(`${this.apiUrl}/${complaintId}/comments?includeInternal=${includeInternal}`);
  }

  addComment(request: CreateCommentRequest): Observable<ApiResponse<Comment>> {
    return this.http.post<ApiResponse<Comment>>(`${this.apiUrl}/${request.complaintId}/comments`, {
      comment: request.comment,
      isInternal: request.isInternal,
      mentionedUserIds: request.mentionedUserIds
    });
  }
}
