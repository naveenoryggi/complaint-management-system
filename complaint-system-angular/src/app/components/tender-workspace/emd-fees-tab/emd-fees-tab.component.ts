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
import { MatTabsModule } from '@angular/material/tabs';
import { TrackingService, EMDRecord, TenderFee } from '../../../services/tracking.service';

@Component({
  selector: 'app-emd-fees-tab',
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
    MatTabsModule
  ],
  templateUrl: './emd-fees-tab.component.html',
  styleUrls: ['./emd-fees-tab.component.css']
})
export class EmdFeesTabComponent implements OnInit {
  @Input() tenderId!: string;

  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  emds = signal<EMDRecord[]>([]);
  fees = signal<TenderFee[]>([]);
  loading = signal(false);
  showEmdForm = signal(false);
  showFeeForm = signal(false);

  emdForm!: FormGroup;
  feeForm!: FormGroup;

  emdColumns: string[] = ['amount', 'mode', 'instrument_number', 'validity', 'status', 'actions'];
  feeColumns: string[] = ['fee_type', 'amount', 'payment_mode', 'payment_reference', 'status', 'actions'];

  modeOptions = [
    { value: 'bg', label: 'Bank Guarantee' },
    { value: 'dd', label: 'Demand Draft' },
    { value: 'online', label: 'Online Transfer' },
    { value: 'fixed_deposit', label: 'Fixed Deposit' },
    { value: 'insurance_surety', label: 'Insurance Surety Bond' }
  ];

  feeTypeOptions = [
    { value: 'tender_fee', label: 'Tender Fee' },
    { value: 'processing_fee', label: 'Processing Fee' },
    { value: 'document_fee', label: 'Document Fee' },
    { value: 'bid_security', label: 'Bid Security' }
  ];

  ngOnInit() {
    this.emdForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0)]],
      mode: ['', Validators.required],
      instrument_number: [''],
      instrument_date: [''],
      issuing_bank: [''],
      validity_start_date: [''],
      validity_end_date: [''],
      notes: ['']
    });

    this.feeForm = this.fb.group({
      fee_type: ['', Validators.required],
      amount: [null, [Validators.required, Validators.min(0)]],
      payment_mode: [''],
      payment_reference: [''],
      payment_date: [''],
      notes: ['']
    });

    if (this.tenderId) {
      this.loadData();
    }
  }

  loadData() {
    this.loading.set(true);
    this.trackingService.listEMDs(this.tenderId).subscribe({
      next: (data) => {
        this.emds.set(data);
        this.loading.set(false);
      },
      error: (err) => { console.error('Error loading EMDs:', err); this.loading.set(false); }
    });
    this.trackingService.listFees(this.tenderId).subscribe({
      next: (data) => this.fees.set(data),
      error: (err) => { console.error('Error loading fees:', err); }
    });
  }

  onCreateEMD() {
    if (this.emdForm.invalid) return;
    this.trackingService.createEMD(this.tenderId, this.emdForm.value).subscribe({
      next: () => {
        this.emdForm.reset();
        this.showEmdForm.set(false);
        this.loadData();
        this.snackBar.open('EMD created', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to create EMD', 'Close', { duration: 5000 })
    });
  }

  onDeleteEMD(emd: EMDRecord) {
    if (confirm('Delete this EMD record?')) {
      this.trackingService.deleteEMD(emd.id).subscribe({
        next: () => { this.loadData(); this.snackBar.open('EMD deleted', 'Close', { duration: 3000 }); },
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete EMD', 'Close', { duration: 5000 })
      });
    }
  }

  onCreateFee() {
    if (this.feeForm.invalid) return;
    this.trackingService.createFee(this.tenderId, this.feeForm.value).subscribe({
      next: () => {
        this.feeForm.reset();
        this.showFeeForm.set(false);
        this.loadData();
        this.snackBar.open('Fee created', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to create fee', 'Close', { duration: 5000 })
    });
  }

  onDeleteFee(fee: TenderFee) {
    if (confirm('Delete this fee record?')) {
      this.trackingService.deleteFee(fee.id).subscribe({
        next: () => { this.loadData(); this.snackBar.open('Fee deleted', 'Close', { duration: 3000 }); },
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete fee', 'Close', { duration: 5000 })
      });
    }
  }

  getModeLabel(mode: string): string {
    return this.modeOptions.find(m => m.value === mode)?.label || mode;
  }

  getFeeTypeLabel(type: string): string {
    return this.feeTypeOptions.find(f => f.value === type)?.label || type;
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(value);
  }

  getValidityText(emd: EMDRecord): string {
    if (!emd.validity_start_date && !emd.validity_end_date) return 'N/A';
    const start = emd.validity_start_date ? new Date(emd.validity_start_date).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: '2-digit' }) : '?';
    const end = emd.validity_end_date ? new Date(emd.validity_end_date).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: '2-digit' }) : '?';
    return `${start} - ${end}`;
  }

  getStatusClass(status: string): string {
    const map: { [key: string]: string } = { 'pending': 'status-pending', 'submitted': 'status-submitted', 'released': 'status-released', 'forfeited': 'status-forfeited' };
    return map[status] || '';
  }
}
