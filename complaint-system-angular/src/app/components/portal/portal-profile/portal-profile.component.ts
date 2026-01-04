import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PortalAuthService } from '../../../services/portal-auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-portal-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './portal-profile.component.html',
  styleUrls: ['./portal-profile.component.scss']
})
export class PortalProfileComponent implements OnInit {
  private readonly API_URL = `${environment.apiUrl}/portal`;

  profileForm: FormGroup;
  isLoading = signal(false);
  isSaving = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private authService: PortalAuthService,
    private http: HttpClient
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.maxLength(20)]],
      alternatePhone: ['', [Validators.maxLength(20)]],
      designation: ['', [Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  loadProfile(): void {
    this.isLoading.set(true);

    this.http.get<any>(`${this.API_URL}/auth/profile`, {
      headers: this.getHeaders()
    }).subscribe({
      next: (profile) => {
        this.profileForm.patchValue({
          firstName: profile.firstName || '',
          lastName: profile.lastName || '',
          email: profile.email || '',
          phone: profile.phone || '',
          alternatePhone: profile.alternatePhone || '',
          designation: profile.designation || ''
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.errorMessage.set('Failed to load profile. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.http.put<any>(`${this.API_URL}/auth/profile`, this.profileForm.value, {
      headers: this.getHeaders()
    }).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccess) {
          this.successMessage.set('Profile updated successfully!');
          setTimeout(() => this.successMessage.set(null), 3000);
        } else {
          this.errorMessage.set(response.message || 'Failed to update profile.');
        }
      },
      error: (error) => {
        this.isSaving.set(false);
        this.errorMessage.set('Failed to update profile. Please try again.');
        console.error('Error updating profile:', error);
      }
    });
  }

  get user() {
    return this.authService.user();
  }
}
