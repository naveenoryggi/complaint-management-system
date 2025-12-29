import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { runtimeConfig } from '../../environments/environment';
import { User, CreateUserRequest, UpdateUserRequest } from '../models/user.model';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private get apiUrl() { return `${runtimeConfig.apiUrl}/users`; }

  constructor(private http: HttpClient) {}

  getUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(this.apiUrl);
  }

  searchUsers(searchTerm: string, searchFields?: string[], limit: number = 20): Observable<ApiResponse<User[]>> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (searchFields && searchFields.length > 0) {
      searchFields.forEach(field => {
        params = params.append('searchFields', field);
      });
    }
    params = params.set('limit', limit.toString());
    return this.http.get<ApiResponse<User[]>>(`${this.apiUrl}/search`, { params });
  }

  getUserById(id: string): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(this.apiUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/${id}`, request);
  }

  deleteUser(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
