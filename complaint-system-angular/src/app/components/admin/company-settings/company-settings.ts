import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CompanyService } from '../../../services/company.service';
import { AuthService } from '../../../services/auth.service';
import { Company, UpdateCompanyRequest } from '../../../models/company.model';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';

@Component({
  selector: 'app-company-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UserAutocompleteComponent],
  templateUrl: './company-settings.html',
  styleUrl: './company-settings.scss'
})
export class CompanySettings implements OnInit {
  company: Company | null = null;
  companyForm!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  selectedFile: File | null = null;
  logoPreviewUrl: string | null = null;
  uploadingLogo = false;

  // User display names for autocomplete
  managerName?: string;
  secondaryManagerName?: string;
  hrResponsibleName?: string;

  constructor(
    private companyService: CompanyService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadCompanyDetails();
  }

  // Getters/setters for company management fields
  get managerId(): string | undefined {
    return (this.company as any)?.managerId;
  }

  set managerId(value: string | undefined) {
    if (this.company) {
      (this.company as any).managerId = value;
    }
  }

  get secondaryManagerId(): string | undefined {
    return (this.company as any)?.secondaryManagerId;
  }

  set secondaryManagerId(value: string | undefined) {
    if (this.company) {
      (this.company as any).secondaryManagerId = value;
    }
  }

  get hrResponsibleId(): string | undefined {
    return (this.company as any)?.hrResponsibleId;
  }

  set hrResponsibleId(value: string | undefined) {
    if (this.company) {
      (this.company as any).hrResponsibleId = value;
    }
  }

  initForm(): void {
    this.companyForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      contactEmail: ['', [Validators.email]],
      contactPhone: ['', Validators.maxLength(20)],
      address: ['', Validators.maxLength(500)]
    });
  }

  loadCompanyDetails(): void {
    const user = this.authService.currentUserValue;
    if (!user?.companyId) {
      this.errorMessage = 'Company information not found';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.companyService.getCompanyById(user.companyId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.company = response.data;
          this.companyForm.patchValue({
            name: this.company.name,
            description: this.company.description,
            contactEmail: this.company.contactEmail,
            contactPhone: this.company.contactPhone,
            address: this.company.address
          });
          if (this.company.logoUrl) {
            this.logoPreviewUrl = this.companyService.getLogoUrl(this.company.logoUrl);
          }
          // Set user display names for autocomplete
          this.managerName = (this.company as any).managerName;
          this.secondaryManagerName = (this.company as any).secondaryManagerName;
          this.hrResponsibleName = (this.company as any).hrResponsibleName;
        } else {
          this.errorMessage = response.message || 'Failed to load company details';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load company details. Please try again.';
        this.loading = false;
        console.error('Error loading company:', error);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/svg+xml', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        this.errorMessage = 'Invalid file type. Please upload a JPG, PNG, GIF, SVG, or WEBP image.';
        return;
      }

      // Validate file size (5MB max)
      const maxSize = 5 * 1024 * 1024; // 5MB in bytes
      if (file.size > maxSize) {
        this.errorMessage = 'File size exceeds 5MB limit.';
        return;
      }

      this.selectedFile = file;
      this.errorMessage = '';

      // Preview the image
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreviewUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadLogo(): void {
    if (!this.selectedFile || !this.company) {
      return;
    }

    this.uploadingLogo = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.companyService.uploadLogo(this.company.id, this.selectedFile).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.successMessage = 'Logo uploaded successfully!';
          this.logoPreviewUrl = this.companyService.getLogoUrl(response.data.logoUrl);
          if (this.company) {
            this.company.logoUrl = response.data.logoUrl;
            this.company.logoFileName = response.data.fileName;
          }
          this.selectedFile = null;
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message || 'Failed to upload logo';
        }
        this.uploadingLogo = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to upload logo. Please try again.';
        this.uploadingLogo = false;
        console.error('Error uploading logo:', error);
      }
    });
  }

  deleteLogo(): void {
    if (!this.company || !this.company.logoUrl) {
      return;
    }

    if (!confirm('Are you sure you want to delete the company logo?')) {
      return;
    }

    this.uploadingLogo = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.companyService.deleteLogo(this.company.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Logo deleted successfully!';
          this.logoPreviewUrl = null;
          if (this.company) {
            this.company.logoUrl = undefined;
            this.company.logoFileName = undefined;
          }
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message || 'Failed to delete logo';
        }
        this.uploadingLogo = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete logo. Please try again.';
        this.uploadingLogo = false;
        console.error('Error deleting logo:', error);
      }
    });
  }

  cancelLogoSelection(): void {
    this.selectedFile = null;
    if (this.company?.logoUrl) {
      this.logoPreviewUrl = this.companyService.getLogoUrl(this.company.logoUrl);
    } else {
      this.logoPreviewUrl = null;
    }
  }

  saveCompanyDetails(): void {
    if (this.companyForm.invalid || !this.company) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      Object.keys(this.companyForm.controls).forEach(key => {
        const control = this.companyForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: UpdateCompanyRequest = {
      ...this.companyForm.value,
      managerId: this.managerId,
      secondaryManagerId: this.secondaryManagerId,
      hrResponsibleId: this.hrResponsibleId
    };

    this.companyService.updateCompany(this.company.id, request).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.company = response.data;
          this.successMessage = 'Company details updated successfully!';
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message || 'Failed to update company details';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update company details. Please try again.';
        this.loading = false;
        console.error('Error updating company:', error);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.companyForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  // User selection handlers
  onManagerSelected(user: UserSearchResult | null): void {
    this.managerId = user?.id;
    this.managerName = user?.fullName;
  }

  onSecondaryManagerSelected(user: UserSearchResult | null): void {
    this.secondaryManagerId = user?.id;
    this.secondaryManagerName = user?.fullName;
  }

  onHrResponsibleSelected(user: UserSearchResult | null): void {
    this.hrResponsibleId = user?.id;
    this.hrResponsibleName = user?.fullName;
  }
}
