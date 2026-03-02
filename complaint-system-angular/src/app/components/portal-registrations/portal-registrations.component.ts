import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TrackingService, PortalRegistration } from '../../services/tracking.service';

@Component({
  selector: 'app-portal-registrations',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './portal-registrations.component.html',
  styleUrls: ['./portal-registrations.component.css']
})
export class PortalRegistrationsComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  portals = signal<PortalRegistration[]>([]);
  loading = signal(false);
  showCreateForm = signal(false);
  seeding = signal(false);
  editingPortal = signal<PortalRegistration | null>(null);

  createForm!: FormGroup;

  displayedColumns: string[] = [
    'portal_name', 'portal_type', 'registration_status',
    'registration_number', 'dsc_holder_name', 'dsc_expiry_date', 'actions'
  ];

  portalTypes = [
    { value: 'central', label: 'Central Government' },
    { value: 'state', label: 'State Government' },
    { value: 'psu', label: 'PSU' },
    { value: 'private', label: 'Private' },
    { value: 'defence', label: 'Defence' }
  ];

  statusOptions = [
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
    { value: 'pending', label: 'Pending' },
    { value: 'expired', label: 'Expired' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      portal_name: ['', Validators.required],
      portal_url: [''],
      portal_type: [''],
      registration_status: ['active'],
      registration_number: [''],
      dsc_class: [''],
      dsc_holder_name: [''],
      dsc_expiry_date: ['']
    });

    this.loadPortals();
  }

  loadPortals() {
    this.loading.set(true);
    this.trackingService.listPortals().subscribe({
      next: (data) => {
        this.portals.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading portals:', error);
        this.snackBar.open('Failed to load portals', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onCreatePortal() {
    if (this.createForm.invalid) return;

    this.trackingService.createPortal(this.createForm.value).subscribe({
      next: () => {
        this.createForm.reset({ registration_status: 'active' });
        this.showCreateForm.set(false);
        this.loadPortals();
        this.snackBar.open('Portal registration added', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error creating portal:', error);
        this.snackBar.open('Failed to add portal', 'Close', { duration: 3000 });
      }
    });
  }

  onEditPortal(portal: PortalRegistration) {
    this.editingPortal.set(portal);
    this.createForm.patchValue({
      portal_name: portal.portal_name,
      portal_url: portal.portal_url || '',
      portal_type: portal.portal_type || '',
      registration_status: portal.registration_status || 'active',
      registration_number: portal.registration_number || '',
      dsc_class: portal.dsc_class || '',
      dsc_holder_name: portal.dsc_holder_name || '',
      dsc_expiry_date: portal.dsc_expiry_date || ''
    });
    this.showCreateForm.set(true);
  }

  onSavePortal() {
    if (this.createForm.invalid) return;

    const editing = this.editingPortal();
    if (editing) {
      this.trackingService.updatePortal(editing.id, this.createForm.value).subscribe({
        next: () => {
          this.editingPortal.set(null);
          this.createForm.reset({ registration_status: 'active' });
          this.showCreateForm.set(false);
          this.loadPortals();
          this.snackBar.open('Portal updated successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error updating portal:', error);
          this.snackBar.open('Failed to update portal', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.onCreatePortal();
    }
  }

  onCancelEdit() {
    this.editingPortal.set(null);
    this.createForm.reset({ registration_status: 'active' });
    this.showCreateForm.set(false);
  }

  onDeletePortal(portal: PortalRegistration) {
    if (confirm(`Delete portal "${portal.portal_name}"?`)) {
      this.trackingService.deletePortal(portal.id).subscribe({
        next: () => {
          this.loadPortals();
          this.snackBar.open('Portal deleted', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting portal:', error);
          this.snackBar.open('Failed to delete portal', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onSeedIndianPortals() {
    this.seeding.set(true);
    this.trackingService.seedIndianPortals().subscribe({
      next: (response) => {
        this.seeding.set(false);
        this.loadPortals();
        this.snackBar.open(`Indian portals seeded successfully`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error seeding portals:', error);
        this.seeding.set(false);
        this.snackBar.open('Failed to seed portals', 'Close', { duration: 3000 });
      }
    });
  }

  getDscExpiryClass(expiryDate: string | undefined): string {
    if (!expiryDate) return '';

    const expiry = new Date(expiryDate);
    const now = new Date();
    const diffDays = Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'dsc-expired';
    if (diffDays <= 30) return 'dsc-warning';
    return 'dsc-valid';
  }

  formatDscExpiry(expiryDate: string | undefined): string {
    if (!expiryDate) return 'N/A';

    const expiry = new Date(expiryDate);
    const now = new Date();
    const diffDays = Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

    const dateStr = expiry.toLocaleDateString('en-IN', { year: 'numeric', month: 'short', day: 'numeric' });

    if (diffDays < 0) return `${dateStr} (Expired)`;
    if (diffDays <= 30) return `${dateStr} (${diffDays}d left)`;
    return dateStr;
  }

  getStatusClass(status: string): string {
    const map: { [key: string]: string } = {
      'active': 'status-active',
      'inactive': 'status-inactive',
      'pending': 'status-pending',
      'expired': 'status-expired'
    };
    return map[status] || '';
  }
}
