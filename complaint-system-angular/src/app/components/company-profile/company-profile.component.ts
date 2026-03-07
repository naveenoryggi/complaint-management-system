import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  CompanyMasterService,
  CompanyProfile,
  Certification,
  Personnel
} from '../../services/company-master.service';

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './company-profile.component.html',
  styleUrls: ['./company-profile.component.css']
})
export class CompanyProfileComponent implements OnInit {
  private companyService = inject(CompanyMasterService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  profile = signal<CompanyProfile | null>(null);
  certifications = signal<Certification[]>([]);
  personnel = signal<Personnel[]>([]);
  loading = signal(false);
  editMode = signal(false);

  profileForm!: FormGroup;
  certForm!: FormGroup;
  personnelForm!: FormGroup;

  certColumns: string[] = ['name', 'cert_type', 'issuing_body', 'certificate_number', 'expiry_date', 'is_valid', 'actions'];
  personnelColumns: string[] = ['name', 'designation', 'role_in_tender', 'qualification', 'experience_years', 'actions'];

  ngOnInit() {
    this.initForms();
    this.loadProfile();
    this.loadCertifications();
    this.loadPersonnel();
    // Start with profile form disabled (read-only mode)
    this.profileForm.disable();
  }

  private initForms() {
    this.profileForm = this.fb.group({
      company_name: ['', Validators.required],
      cin_number: [''],
      pan_number: [''],
      gstin: [''],
      msme_registration: [''],
      registered_address: [''],
      corporate_address: [''],
      website: [''],
      phone: [''],
      email: ['', Validators.email],
      year_established: [null],
      employee_count: [null],
      pf_registration_number: [''],
      pf_registration_date: [''],
      esi_registration_number: [''],
      esi_registration_date: [''],
      bank_name: [''],
      account_number: [''],
      ifsc_code: [''],
      branch_name: ['']
    });

    this.certForm = this.fb.group({
      name: ['', Validators.required],
      cert_type: [''],
      issuing_body: [''],
      certificate_number: [''],
      issue_date: [''],
      expiry_date: ['']
    });

    this.personnelForm = this.fb.group({
      name: ['', Validators.required],
      designation: [''],
      role_in_tender: [''],
      qualification: [''],
      experience_years: [null]
    });
  }

  loadProfile() {
    this.loading.set(true);
    this.companyService.getProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.profileForm.patchValue(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.loading.set(false);
        if (error.status !== 404) {
          this.snackBar.open('Failed to load company profile', 'Close', { duration: 3000 });
        }
      }
    });
  }

  loadCertifications() {
    this.companyService.listCertifications().subscribe({
      next: (data) => this.certifications.set(data),
      error: (error) => {
        console.error('Error loading certifications:', error);
        this.snackBar.open('Failed to load certifications', 'Close', { duration: 3000 });
      }
    });
  }

  loadPersonnel() {
    this.companyService.listPersonnel().subscribe({
      next: (data) => this.personnel.set(data),
      error: (error) => {
        console.error('Error loading personnel:', error);
        this.snackBar.open('Failed to load personnel', 'Close', { duration: 3000 });
      }
    });
  }

  enableEdit() {
    this.editMode.set(true);
    this.profileForm.enable();
  }

  cancelEdit() {
    this.editMode.set(false);
    this.profileForm.disable();
    // Restore original values
    const p = this.profile();
    if (p) this.profileForm.patchValue(p);
  }

  saveProfile() {
    // Temporarily enable to read values (disabled controls excluded from .value)
    this.profileForm.enable();

    if (this.profileForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    const data = this.profileForm.value;
    const request = this.profile()
      ? this.companyService.updateProfile(data)
      : this.companyService.createProfile(data);

    request.subscribe({
      next: (result) => {
        this.profile.set(result);
        this.editMode.set(false);
        this.profileForm.disable();
        this.snackBar.open('Company profile saved successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error saving profile:', error);
        this.snackBar.open('Failed to save company profile', 'Close', { duration: 3000 });
      }
    });
  }

  addCertification() {
    if (this.certForm.invalid) {
      this.snackBar.open('Please provide a certification name', 'Close', { duration: 3000 });
      return;
    }

    this.companyService.createCertification(this.certForm.value).subscribe({
      next: () => {
        this.certForm.reset();
        this.loadCertifications();
        this.snackBar.open('Certification added successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error adding certification:', error);
        this.snackBar.open('Failed to add certification', 'Close', { duration: 3000 });
      }
    });
  }

  deleteCertification(id: string) {
    if (confirm('Are you sure you want to delete this certification?')) {
      this.companyService.deleteCertification(id).subscribe({
        next: () => {
          this.loadCertifications();
          this.snackBar.open('Certification deleted successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting certification:', error);
          this.snackBar.open('Failed to delete certification', 'Close', { duration: 3000 });
        }
      });
    }
  }

  addPersonnel() {
    if (this.personnelForm.invalid) {
      this.snackBar.open('Please provide a personnel name', 'Close', { duration: 3000 });
      return;
    }

    this.companyService.createPersonnel(this.personnelForm.value).subscribe({
      next: () => {
        this.personnelForm.reset();
        this.loadPersonnel();
        this.snackBar.open('Personnel added successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error adding personnel:', error);
        this.snackBar.open('Failed to add personnel', 'Close', { duration: 3000 });
      }
    });
  }

  deletePersonnel(id: string) {
    if (confirm('Are you sure you want to delete this personnel record?')) {
      this.companyService.deletePersonnel(id).subscribe({
        next: () => {
          this.loadPersonnel();
          this.snackBar.open('Personnel deleted successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting personnel:', error);
          this.snackBar.open('Failed to delete personnel', 'Close', { duration: 3000 });
        }
      });
    }
  }

  // Brand Assets
  uploadingAsset = signal<string | null>(null);

  brandAssets: { key: string; label: string; icon: string; accept: string; hint: string }[] = [
    { key: 'logo', label: 'Company Logo', icon: 'image', accept: 'image/*,.svg', hint: 'PNG, JPG, SVG' },
    { key: 'letterhead', label: 'Letterhead Template', icon: 'article', accept: 'image/*,.docx,.doc,.pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/msword,application/pdf', hint: 'DOCX, PDF, or image' },
    { key: 'signature', label: 'Authorized Signature', icon: 'draw', accept: 'image/*', hint: 'PNG, JPG (sign + stamp image)' },
    { key: 'stamp', label: 'Company Stamp / Seal', icon: 'approval', accept: 'image/*', hint: 'PNG, JPG image' },
  ];

  getAssetPath(key: string): string | null | undefined {
    const p = this.profile();
    if (!p) return null;
    const fieldMap: Record<string, string | null | undefined> = {
      logo: p.logo_path,
      letterhead: p.letterhead_path,
      signature: p.signature_path,
      stamp: p.stamp_path,
    };
    return fieldMap[key] ?? null;
  }

  triggerFileInput(assetKey: string) {
    const el = window.document.getElementById('asset-input-' + assetKey) as HTMLInputElement;
    if (el) el.click();
  }

  onBrandAssetSelected(event: Event, assetType: string) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    this.uploadingAsset.set(assetType);

    this.companyService.uploadBrandAsset(assetType, file).subscribe({
      next: () => {
        this.uploadingAsset.set(null);
        this.loadProfile();
        this.snackBar.open(`${assetType} uploaded successfully`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(`Error uploading ${assetType}:`, error);
        this.uploadingAsset.set(null);
        this.snackBar.open(error.error?.detail || `Failed to upload ${assetType}`, 'Close', { duration: 5000 });
      },
    });

    input.value = '';
  }

  onDeleteBrandAsset(assetType: string) {
    if (!confirm(`Remove the ${assetType} file?`)) return;

    this.companyService.deleteBrandAsset(assetType).subscribe({
      next: () => {
        this.loadProfile();
        this.snackBar.open(`${assetType} removed`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(`Error deleting ${assetType}:`, error);
        this.snackBar.open(`Failed to remove ${assetType}`, 'Close', { duration: 3000 });
      },
    });
  }
}
