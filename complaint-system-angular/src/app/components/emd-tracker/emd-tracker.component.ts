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
import { TrackingService, EMDRecord, TenderFee } from '../../services/tracking.service';
import { TenderService, Tender } from '../../services/tender.service';

@Component({
  selector: 'app-emd-tracker',
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
  templateUrl: './emd-tracker.component.html',
  styleUrls: ['./emd-tracker.component.css']
})
export class EmdTrackerComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  emds = signal<EMDRecord[]>([]);
  fees = signal<TenderFee[]>([]);
  tenders = signal<Tender[]>([]);
  emdLoading = signal(false);
  feeLoading = signal(false);
  showEmdForm = signal(false);
  showFeeForm = signal(false);
  editingEMD = signal<EMDRecord | null>(null);
  editingFee = signal<TenderFee | null>(null);

  emdForm!: FormGroup;
  feeForm!: FormGroup;

  emdStatusFilter = new FormControl('');

  emdColumns: string[] = ['tender_name', 'amount', 'mode', 'instrument_number', 'validity', 'status', 'actions'];
  feeColumns: string[] = ['tender_name', 'fee_type', 'amount', 'payment_mode', 'payment_reference', 'status', 'actions'];

  modeOptions = [
    { value: 'bg', label: 'Bank Guarantee' },
    { value: 'dd', label: 'Demand Draft' },
    { value: 'online', label: 'Online Transfer' },
    { value: 'fixed_deposit', label: 'Fixed Deposit' },
    { value: 'insurance_surety', label: 'Insurance Surety Bond' }
  ];

  emdStatusOptions = [
    { value: '', label: 'All Status' },
    { value: 'pending', label: 'Pending' },
    { value: 'submitted', label: 'Submitted' },
    { value: 'released', label: 'Released' },
    { value: 'forfeited', label: 'Forfeited' }
  ];

  feeTypeOptions = [
    { value: 'tender_fee', label: 'Tender Fee' },
    { value: 'processing_fee', label: 'Processing Fee' },
    { value: 'document_fee', label: 'Document Fee' },
    { value: 'bid_security', label: 'Bid Security' }
  ];

  ngOnInit() {
    this.emdForm = this.fb.group({
      tender_id: ['', Validators.required],
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
      tender_id: ['', Validators.required],
      fee_type: ['', Validators.required],
      amount: [null, [Validators.required, Validators.min(0)]],
      payment_mode: [''],
      payment_reference: [''],
      payment_date: [''],
      notes: ['']
    });

    this.loadTenders();
    this.loadEMDs();
    this.loadFees();

    this.emdStatusFilter.valueChanges.subscribe(() => this.loadEMDs());
  }

  loadTenders() {
    this.tenderService.listTenders(1, 200).subscribe({
      next: (response) => this.tenders.set(response.items),
      error: (err) => { console.error('Error loading tenders:', err); }
    });
  }

  loadEMDs() {
    this.emdLoading.set(true);
    this.trackingService.listEMDs().subscribe({
      next: (data) => {
        const statusFilter = this.emdStatusFilter.value;
        this.emds.set(statusFilter ? data.filter(e => e.status === statusFilter) : data);
        this.emdLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading EMDs:', err);
        this.snackBar.open(err.error?.detail || 'Failed to load EMD records', 'Close', { duration: 5000 });
        this.emdLoading.set(false);
      }
    });
  }

  loadFees() {
    this.feeLoading.set(true);
    this.trackingService.listFees().subscribe({
      next: (data) => {
        this.fees.set(data);
        this.feeLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading fees:', err);
        this.snackBar.open(err.error?.detail || 'Failed to load tender fees', 'Close', { duration: 5000 });
        this.feeLoading.set(false);
      }
    });
  }

  onCreateEMD() {
    if (this.emdForm.invalid) return;

    const tenderId = this.emdForm.value.tender_id;
    const data = { ...this.emdForm.value };
    delete data.tender_id;

    this.trackingService.createEMD(tenderId, data).subscribe({
      next: () => {
        this.emdForm.reset();
        this.showEmdForm.set(false);
        this.loadEMDs();
        this.snackBar.open('EMD record created', 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error creating EMD:', err);
        this.snackBar.open(err.error?.detail || 'Failed to create EMD record', 'Close', { duration: 5000 });
      }
    });
  }

  onEditEMD(emd: EMDRecord) {
    this.editingEMD.set(emd);
    this.emdForm.patchValue({
      tender_id: emd.tender_id,
      amount: emd.amount,
      mode: emd.mode,
      instrument_number: emd.instrument_number || '',
      instrument_date: emd.instrument_date || '',
      issuing_bank: emd.issuing_bank || '',
      validity_start_date: emd.validity_start_date || '',
      validity_end_date: emd.validity_end_date || '',
      notes: emd.notes || ''
    });
    this.showEmdForm.set(true);
  }

  onSaveEMD() {
    if (this.emdForm.invalid) return;
    const editing = this.editingEMD();
    if (editing) {
      const data = { ...this.emdForm.value };
      delete data.tender_id;
      this.trackingService.updateEMD(editing.id, data).subscribe({
        next: () => {
          this.editingEMD.set(null);
          this.emdForm.reset();
          this.showEmdForm.set(false);
          this.loadEMDs();
          this.snackBar.open('EMD updated', 'Close', { duration: 3000 });
        },
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to update EMD', 'Close', { duration: 5000 })
      });
    } else {
      this.onCreateEMD();
    }
  }

  onCancelEmdEdit() {
    this.editingEMD.set(null);
    this.emdForm.reset();
    this.showEmdForm.set(false);
  }

  onCycleEmdStatus(emd: EMDRecord) {
    const cycle = ['pending', 'submitted', 'released'];
    const idx = cycle.indexOf(emd.status);
    const nextStatus = cycle[(idx + 1) % cycle.length];
    this.trackingService.updateEMD(emd.id, { status: nextStatus } as any).subscribe({
      next: () => {
        this.loadEMDs();
        this.snackBar.open(`EMD status changed to ${nextStatus}`, 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to update status', 'Close', { duration: 5000 })
    });
  }

  onDeleteEMD(emd: EMDRecord) {
    if (confirm('Delete this EMD record?')) {
      this.trackingService.deleteEMD(emd.id).subscribe({
        next: () => {
          this.loadEMDs();
          this.snackBar.open('EMD record deleted', 'Close', { duration: 3000 });
        },
        error: (err) => {
          console.error('Error deleting EMD:', err);
          this.snackBar.open(err.error?.detail || 'Failed to delete EMD', 'Close', { duration: 5000 });
        }
      });
    }
  }

  onCreateFee() {
    if (this.feeForm.invalid) return;

    const tenderId = this.feeForm.value.tender_id;
    const data = { ...this.feeForm.value };
    delete data.tender_id;

    this.trackingService.createFee(tenderId, data).subscribe({
      next: () => {
        this.feeForm.reset();
        this.showFeeForm.set(false);
        this.loadFees();
        this.snackBar.open('Tender fee created', 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error creating fee:', err);
        this.snackBar.open(err.error?.detail || 'Failed to create fee', 'Close', { duration: 5000 });
      }
    });
  }

  onEditFee(fee: TenderFee) {
    this.editingFee.set(fee);
    this.feeForm.patchValue({
      tender_id: fee.tender_id,
      fee_type: fee.fee_type,
      amount: fee.amount,
      payment_mode: fee.payment_mode || '',
      payment_reference: fee.payment_reference || '',
      payment_date: fee.payment_date || '',
      notes: fee.notes || ''
    });
    this.showFeeForm.set(true);
  }

  onSaveFee() {
    if (this.feeForm.invalid) return;
    const editing = this.editingFee();
    if (editing) {
      const data = { ...this.feeForm.value };
      delete data.tender_id;
      this.trackingService.updateFee(editing.id, data).subscribe({
        next: () => {
          this.editingFee.set(null);
          this.feeForm.reset();
          this.showFeeForm.set(false);
          this.loadFees();
          this.snackBar.open('Fee updated', 'Close', { duration: 3000 });
        },
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to update fee', 'Close', { duration: 5000 })
      });
    } else {
      this.onCreateFee();
    }
  }

  onCancelFeeEdit() {
    this.editingFee.set(null);
    this.feeForm.reset();
    this.showFeeForm.set(false);
  }

  onDeleteFee(fee: TenderFee) {
    if (confirm('Delete this fee record?')) {
      this.trackingService.deleteFee(fee.id).subscribe({
        next: () => {
          this.loadFees();
          this.snackBar.open('Fee record deleted', 'Close', { duration: 3000 });
        },
        error: (err) => {
          console.error('Error deleting fee:', err);
          this.snackBar.open(err.error?.detail || 'Failed to delete fee', 'Close', { duration: 5000 });
        }
      });
    }
  }

  getTenderName(tenderId: string): string {
    const tender = this.tenders().find(t => t.id === tenderId);
    return tender?.title || 'Unknown Tender';
  }

  getModeLabel(mode: string): string {
    const opt = this.modeOptions.find(m => m.value === mode);
    return opt?.label || mode;
  }

  getFeeTypeLabel(feeType: string): string {
    const opt = this.feeTypeOptions.find(f => f.value === feeType);
    return opt?.label || feeType;
  }

  getEmdStatusClass(status: string): string {
    const map: { [key: string]: string } = {
      'pending': 'status-pending',
      'submitted': 'status-submitted',
      'released': 'status-released',
      'forfeited': 'status-forfeited'
    };
    return map[status] || '';
  }

  getValidityText(emd: EMDRecord): string {
    if (!emd.validity_start_date && !emd.validity_end_date) return 'N/A';
    const start = emd.validity_start_date ? new Date(emd.validity_start_date).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: '2-digit' }) : '?';
    const end = emd.validity_end_date ? new Date(emd.validity_end_date).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: '2-digit' }) : '?';
    return `${start} - ${end}`;
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', maximumFractionDigits: 0
    }).format(value);
  }

  getTotalEmdAmount(): number {
    return this.emds().reduce((sum, e) => sum + (e.amount || 0), 0);
  }

  getTotalFeeAmount(): number {
    return this.fees().reduce((sum, f) => sum + (f.amount || 0), 0);
  }
}
