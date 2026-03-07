import { Component, OnInit, input, signal, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TenderService } from '../../../services/tender.service';

export interface TenderResult {
  result: string;
  awarded_value: number | null;
  award_date: string;
  winning_bidder: string;
  loss_reason: string;
  lessons_learned: string;
  contract_number: string;
  contract_start: string;
  contract_end: string;
  [key: string]: string | number | null;
}

export interface ResultType {
  value: string;
  label: string;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-results-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './results-tab.component.html',
  styleUrls: ['./results-tab.component.css'],
})
export class ResultsTabComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);

  result = signal<TenderResult | null>(null);
  loading = signal(false);
  editing = signal(false);

  resultForm: TenderResult = {
    result: '',
    awarded_value: null,
    award_date: '',
    winning_bidder: '',
    loss_reason: '',
    lessons_learned: '',
    contract_number: '',
    contract_start: '',
    contract_end: '',
  };

  resultTypes: ResultType[] = [
    { value: 'won', label: 'Won', icon: 'emoji_events', color: '#4caf50' },
    { value: 'lost', label: 'Lost', icon: 'cancel', color: '#f44336' },
    { value: 'cancelled', label: 'Cancelled', icon: 'block', color: '#9e9e9e' },
  ];

  constructor() {
    effect(() => {
      const id = this.tenderId();
      if (id) {
        this.loadResult();
      }
    });
  }

  ngOnInit(): void {}

  loadResult(): void {
    const id = this.tenderId();
    if (!id) return;
    this.loading.set(true);
    (this.tenderService as any)
      .getTenderResult(id)
      .subscribe({
        next: (data: TenderResult | null) => {
          this.result.set(data || null);
          if (data) {
            this.resultForm = { ...data };
          }
          this.loading.set(false);
        },
        error: (err: any) => {
          console.error('Error loading tender result:', err);
          this.result.set(null);
          this.loading.set(false);
        },
      });
  }

  saveResult(): void {
    const id = this.tenderId();
    if (!id) return;
    if (!this.resultForm.result) {
      this.snackBar.open('Please select a result type.', 'Close', { duration: 3000 });
      return;
    }
    this.loading.set(true);
    const existing = this.result();
    const call = existing
      ? (this.tenderService as any).updateTenderResult(id, this.resultForm)
      : (this.tenderService as any).createTenderResult(id, this.resultForm);

    call.subscribe({
      next: (data: TenderResult) => {
        this.result.set(data);
        this.editing.set(false);
        this.loading.set(false);
        this.snackBar.open('Result saved successfully.', 'Close', { duration: 2500 });
      },
      error: (err: any) => {
        this.snackBar.open(err.error?.detail || 'Failed to save result.', 'Close', { duration: 5000 });
        this.loading.set(false);
      },
    });
  }

  toggleEdit(): void {
    if (!this.editing()) {
      const current = this.result();
      if (current) {
        this.resultForm = { ...current };
      }
    }
    this.editing.update(v => !v);
  }

  formatCurrency(val: number | null | undefined): string {
    if (val === null || val === undefined) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0,
    }).format(val);
  }

  formatDate(date: string): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  getResultType(value: string): ResultType | undefined {
    return this.resultTypes.find(r => r.value === value);
  }

  cancelEdit(): void {
    this.editing.set(false);
    const current = this.result();
    if (current) {
      this.resultForm = { ...current };
    } else {
      this.resultForm = {
        result: '',
        awarded_value: null,
        award_date: '',
        winning_bidder: '',
        loss_reason: '',
        lessons_learned: '',
        contract_number: '',
        contract_start: '',
        contract_end: '',
      };
    }
  }
}
