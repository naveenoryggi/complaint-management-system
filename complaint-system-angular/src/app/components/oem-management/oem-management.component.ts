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
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { TrackingService, OEMMaster, OEMTenderRequirement } from '../../services/tracking.service';
import { TenderService, Tender } from '../../services/tender.service';

@Component({
  selector: 'app-oem-management',
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
    MatTooltipModule,
    MatTabsModule,
    MatCheckboxModule
  ],
  templateUrl: './oem-management.component.html',
  styleUrls: ['./oem-management.component.css']
})
export class OemManagementComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  oems = signal<OEMMaster[]>([]);
  requirements = signal<OEMTenderRequirement[]>([]);
  tenders = signal<Tender[]>([]);
  loading = signal(false);
  requirementsLoading = signal(false);
  showCreateForm = signal(false);
  showRequirementForm = signal(false);
  selectedTenderId = signal<string>('');
  editingOEM = signal<OEMMaster | null>(null);

  createForm!: FormGroup;
  requirementForm!: FormGroup;

  oemColumns: string[] = ['name', 'country', 'is_indian', 'partner_tier', 'product_categories', 'actions'];
  reqColumns: string[] = ['oem_name', 'maf_status', 'ms_status', 'partner_cert_status', 'datasheet_status', 'compliance_cert_status', 'actions'];

  tierOptions = [
    { value: 'platinum', label: 'Platinum' },
    { value: 'gold', label: 'Gold' },
    { value: 'silver', label: 'Silver' },
    { value: 'bronze', label: 'Bronze' },
    { value: 'registered', label: 'Registered' }
  ];

  statusOptions = [
    { value: 'pending', label: 'Pending' },
    { value: 'received', label: 'Received' },
    { value: 'not_required', label: 'Not Required' },
    { value: 'expired', label: 'Expired' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      name: ['', Validators.required],
      country: ['India'],
      is_indian: [true],
      india_distributor: [''],
      partner_tier: [''],
      product_categories: ['']
    });

    this.requirementForm = this.fb.group({
      oem_id: ['', Validators.required],
      maf_status: ['pending'],
      ms_status: ['pending'],
      partner_cert_status: ['pending'],
      datasheet_status: ['pending'],
      compliance_cert_status: ['pending'],
      notes: ['']
    });

    this.loadOEMs();
    this.loadTenders();
  }

  loadOEMs() {
    this.loading.set(true);
    this.trackingService.listOEMs().subscribe({
      next: (data) => {
        this.oems.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading OEMs:', err);
        this.snackBar.open(err.error?.detail || 'Failed to load OEMs', 'Close', { duration: 5000 });
        this.loading.set(false);
      }
    });
  }

  loadTenders() {
    this.tenderService.listTenders(1, 100).subscribe({
      next: (response) => this.tenders.set(response.items),
      error: (err) => { console.error('Error loading tenders:', err); }
    });
  }

  loadRequirements() {
    const tenderId = this.selectedTenderId();
    if (!tenderId) return;

    this.requirementsLoading.set(true);
    this.trackingService.listOEMRequirements(tenderId).subscribe({
      next: (data) => {
        this.requirements.set(data);
        this.requirementsLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading requirements:', err);
        this.snackBar.open(err.error?.detail || 'Failed to load OEM requirements', 'Close', { duration: 5000 });
        this.requirementsLoading.set(false);
      }
    });
  }

  onTenderChange(tenderId: string) {
    this.selectedTenderId.set(tenderId);
    this.loadRequirements();
  }

  onCreateOEM() {
    if (this.createForm.invalid) return;

    const formValue = this.createForm.value;
    const data: Partial<OEMMaster> = {
      ...formValue,
      product_categories: formValue.product_categories
        ? formValue.product_categories.split(',').map((s: string) => s.trim()).filter((s: string) => s)
        : []
    };

    this.trackingService.createOEM(data).subscribe({
      next: () => {
        this.createForm.reset({ country: 'India', is_indian: true });
        this.showCreateForm.set(false);
        this.loadOEMs();
        this.snackBar.open('OEM created successfully', 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error creating OEM:', err);
        this.snackBar.open(err.error?.detail || 'Failed to create OEM', 'Close', { duration: 5000 });
      }
    });
  }

  onEditOEM(oem: OEMMaster) {
    this.editingOEM.set(oem);
    this.createForm.patchValue({
      name: oem.name,
      country: oem.country || 'India',
      is_indian: oem.is_indian,
      india_distributor: oem.india_distributor || '',
      partner_tier: oem.partner_tier || '',
      product_categories: oem.product_categories?.join(', ') || ''
    });
    this.showCreateForm.set(true);
  }

  onSaveOEM() {
    if (this.createForm.invalid) return;

    const editing = this.editingOEM();
    const formValue = this.createForm.value;
    const data: Partial<OEMMaster> = {
      ...formValue,
      product_categories: formValue.product_categories
        ? formValue.product_categories.split(',').map((s: string) => s.trim()).filter((s: string) => s)
        : []
    };

    if (editing) {
      this.trackingService.updateOEM(editing.id, data).subscribe({
        next: () => {
          this.editingOEM.set(null);
          this.createForm.reset({ country: 'India', is_indian: true });
          this.showCreateForm.set(false);
          this.loadOEMs();
          this.snackBar.open('OEM updated successfully', 'Close', { duration: 3000 });
        },
        error: (err) => {
          console.error('Error updating OEM:', err);
          this.snackBar.open(err.error?.detail || 'Failed to update OEM', 'Close', { duration: 5000 });
        }
      });
    } else {
      this.onCreateOEM();
    }
  }

  onCancelEdit() {
    this.editingOEM.set(null);
    this.createForm.reset({ country: 'India', is_indian: true });
    this.showCreateForm.set(false);
  }

  onDeleteOEM(oem: OEMMaster) {
    if (confirm(`Delete OEM "${oem.name}"?`)) {
      this.trackingService.deleteOEM(oem.id).subscribe({
        next: () => {
          this.loadOEMs();
          this.snackBar.open('OEM deleted', 'Close', { duration: 3000 });
        },
        error: (err) => {
          console.error('Error deleting OEM:', err);
          this.snackBar.open(err.error?.detail || 'Failed to delete OEM', 'Close', { duration: 5000 });
        }
      });
    }
  }

  onCreateRequirement() {
    if (this.requirementForm.invalid || !this.selectedTenderId()) return;

    this.trackingService.createOEMRequirement(this.selectedTenderId(), this.requirementForm.value).subscribe({
      next: () => {
        this.requirementForm.reset({
          maf_status: 'pending', ms_status: 'pending',
          partner_cert_status: 'pending', datasheet_status: 'pending',
          compliance_cert_status: 'pending'
        });
        this.showRequirementForm.set(false);
        this.loadRequirements();
        this.snackBar.open('OEM requirement added', 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error creating requirement:', err);
        this.snackBar.open(err.error?.detail || 'Failed to add requirement', 'Close', { duration: 5000 });
      }
    });
  }

  getOEMName(oemId: string): string {
    const oem = this.oems().find(o => o.id === oemId);
    return oem?.name || 'Unknown OEM';
  }

  getStatusClass(status: string): string {
    const map: { [key: string]: string } = {
      'received': 'status-received',
      'pending': 'status-pending',
      'not_required': 'status-na',
      'expired': 'status-expired'
    };
    return map[status] || 'status-pending';
  }
}
