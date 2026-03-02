import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
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
import { TrackingService, OEMMaster, OEMTenderRequirement } from '../../../services/tracking.service';

@Component({
  selector: 'app-oem-requirements-tab',
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
  templateUrl: './oem-requirements-tab.component.html',
  styleUrls: ['./oem-requirements-tab.component.css']
})
export class OemRequirementsTabComponent implements OnInit {
  @Input() tenderId!: string;

  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  requirements = signal<OEMTenderRequirement[]>([]);
  oems = signal<OEMMaster[]>([]);
  loading = signal(false);
  showForm = signal(false);

  createForm!: FormGroup;

  displayedColumns: string[] = ['oem_name', 'maf_status', 'ms_status', 'partner_cert_status', 'datasheet_status', 'compliance_cert_status', 'notes', 'actions'];

  statusOptions = [
    { value: 'pending', label: 'Pending' },
    { value: 'received', label: 'Received' },
    { value: 'not_required', label: 'Not Required' },
    { value: 'expired', label: 'Expired' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      oem_id: ['', Validators.required],
      maf_status: ['pending'],
      ms_status: ['pending'],
      partner_cert_status: ['pending'],
      datasheet_status: ['pending'],
      compliance_cert_status: ['pending'],
      notes: ['']
    });

    this.loadOEMs();
    if (this.tenderId) {
      this.loadRequirements();
    }
  }

  loadOEMs() {
    this.trackingService.listOEMs().subscribe({
      next: (data) => this.oems.set(data),
      error: () => {}
    });
  }

  loadRequirements() {
    this.loading.set(true);
    this.trackingService.listOEMRequirements(this.tenderId).subscribe({
      next: (data) => {
        this.requirements.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading OEM requirements:', error);
        this.loading.set(false);
      }
    });
  }

  onCreateRequirement() {
    if (this.createForm.invalid) return;

    this.trackingService.createOEMRequirement(this.tenderId, this.createForm.value).subscribe({
      next: () => {
        this.createForm.reset({
          maf_status: 'pending', ms_status: 'pending',
          partner_cert_status: 'pending', datasheet_status: 'pending',
          compliance_cert_status: 'pending'
        });
        this.showForm.set(false);
        this.loadRequirements();
        this.snackBar.open('OEM requirement added', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error:', error);
        this.snackBar.open('Failed to add requirement', 'Close', { duration: 3000 });
      }
    });
  }

  getOEMName(oemId: string): string {
    return this.oems().find(o => o.id === oemId)?.name || 'Unknown';
  }

  getStatusClass(status: string): string {
    const map: { [key: string]: string } = {
      'received': 'status-received',
      'pending': 'status-pending',
      'not_required': 'status-na',
      'expired': 'status-expired'
    };
    return map[status] || '';
  }

  onCycleStatus(req: OEMTenderRequirement, field: string) {
    const cycle = ['pending', 'received', 'not_required'];
    const current = (req as any)[field] || 'pending';
    const idx = cycle.indexOf(current);
    const next = cycle[(idx + 1) % cycle.length];

    this.trackingService.updateOEMRequirement(req.id, { [field]: next } as any).subscribe({
      next: () => {
        this.loadRequirements();
        this.snackBar.open(`Status updated to ${next.replace('_', ' ')}`, 'Close', { duration: 2000 });
      },
      error: () => this.snackBar.open('Failed to update status', 'Close', { duration: 3000 })
    });
  }

  onDeleteRequirement(req: OEMTenderRequirement) {
    if (confirm(`Remove OEM requirement for "${this.getOEMName(req.oem_id)}"?`)) {
      this.trackingService.deleteOEMRequirement(req.id).subscribe({
        next: () => {
          this.loadRequirements();
          this.snackBar.open('Requirement deleted', 'Close', { duration: 3000 });
        },
        error: () => this.snackBar.open('Failed to delete', 'Close', { duration: 3000 })
      });
    }
  }

  getCompletionPercent(): number {
    const reqs = this.requirements();
    if (reqs.length === 0) return 0;
    const total = reqs.length * 5; // 5 status fields per requirement
    let received = 0;
    for (const r of reqs) {
      if (r.maf_status === 'received' || r.maf_status === 'not_required') received++;
      if (r.ms_status === 'received' || r.ms_status === 'not_required') received++;
      if (r.partner_cert_status === 'received' || r.partner_cert_status === 'not_required') received++;
      if (r.datasheet_status === 'received' || r.datasheet_status === 'not_required') received++;
      if (r.compliance_cert_status === 'received' || r.compliance_cert_status === 'not_required') received++;
    }
    return Math.round((received / total) * 100);
  }
}
