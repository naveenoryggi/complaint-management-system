import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TenderService } from '../../services/tender.service';

interface ComparisonRow {
  label: string;
  key: string;
  type: 'text' | 'currency' | 'date' | 'status' | 'number';
}

@Component({
  selector: 'app-tender-compare',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './tender-compare.component.html',
  styleUrls: ['./tender-compare.component.css']
})
export class TenderCompareComponent implements OnInit {
  private tenderService = inject(TenderService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  comparisons = signal<any[]>([]);
  loading = signal(false);

  rows: ComparisonRow[] = [
    { label: 'Title', key: 'title', type: 'text' },
    { label: 'Reference Number', key: 'reference_number', type: 'text' },
    { label: 'Issuing Authority', key: 'issuing_authority', type: 'text' },
    { label: 'Status', key: 'status', type: 'status' },
    { label: 'Deadline', key: 'deadline', type: 'date' },
    { label: 'Estimated Value', key: 'estimated_value', type: 'currency' },
    { label: 'Document Count', key: 'document_count', type: 'number' },
    { label: 'EMD Total', key: 'emd_total', type: 'currency' },
    { label: 'Fee Total', key: 'fee_total', type: 'currency' },
    { label: 'OEM Count', key: 'oem_count', type: 'number' },
    { label: 'BOQ Items', key: 'boq_items', type: 'number' }
  ];

  get displayedColumns(): string[] {
    const cols = ['attribute'];
    this.comparisons().forEach((_, i) => cols.push(`tender_${i}`));
    return cols;
  }

  ngOnInit(): void {
    const idsParam = this.route.snapshot.queryParamMap.get('ids');
    if (!idsParam) {
      this.snackBar.open('No tender IDs provided for comparison', 'Close', { duration: 3000 });
      return;
    }
    const ids = idsParam.split(',').map(id => id.trim()).filter(id => !!id).slice(0, 4);
    if (ids.length < 2) {
      this.snackBar.open('At least 2 tender IDs are required for comparison', 'Close', { duration: 3000 });
      return;
    }
    this.loadComparisons(ids);
  }

  private loadComparisons(ids: string[]): void {
    this.loading.set(true);
    this.tenderService.compareTenders(ids).subscribe({
      next: (data: any[]) => {
        this.comparisons.set(data.slice(0, 4));
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Error loading comparisons', err);
        this.snackBar.open('Failed to load tender comparison data', 'Close', { duration: 4000 });
        this.loading.set(false);
      }
    });
  }

  getCellValue(tender: any, row: ComparisonRow): string {
    const val = tender[row.key];
    if (val === null || val === undefined || val === '') return '—';
    switch (row.type) {
      case 'currency': return this.formatCurrency(val);
      case 'date': return this.formatDate(val);
      case 'number': return String(val);
      default: return String(val);
    }
  }

  formatCurrency(val: any): string {
    if (val === null || val === undefined) return '—';
    const num = Number(val);
    if (isNaN(num)) return '—';
    if (num >= 10000000) return `₹${(num / 10000000).toFixed(2)} Cr`;
    if (num >= 100000) return `₹${(num / 100000).toFixed(2)} L`;
    return `₹${num.toLocaleString('en-IN')}`;
  }

  formatDate(date: any): string {
    if (!date) return '—';
    try {
      const d = new Date(date);
      if (isNaN(d.getTime())) return '—';
      return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch {
      return '—';
    }
  }

  getStatusColor(status: string): string {
    const map: Record<string, string> = {
      active: 'status-active',
      draft: 'status-draft',
      submitted: 'status-submitted',
      won: 'status-won',
      lost: 'status-lost',
      cancelled: 'status-cancelled',
      closed: 'status-closed'
    };
    return map[(status || '').toLowerCase()] || 'status-default';
  }

  isDeadlineSoon(tender: any): boolean {
    if (!tender.deadline) return false;
    const deadline = new Date(tender.deadline);
    const now = new Date();
    const diff = (deadline.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
    return diff >= 0 && diff <= 7;
  }

  isDeadlinePast(tender: any): boolean {
    if (!tender.deadline) return false;
    return new Date(tender.deadline) < new Date();
  }

  getBestValue(key: string): number | null {
    const vals = this.comparisons()
      .map(t => Number(t[key]))
      .filter(v => !isNaN(v) && v > 0);
    if (vals.length === 0) return null;
    if (key === 'estimated_value' || key === 'emd_total' || key === 'fee_total') {
      return Math.min(...vals);
    }
    return Math.max(...vals);
  }

  isBestForRow(tender: any, key: string): boolean {
    const rowTypes = ['estimated_value', 'emd_total', 'fee_total', 'document_count', 'oem_count', 'boq_items'];
    if (!rowTypes.includes(key)) return false;
    const best = this.getBestValue(key);
    if (best === null) return false;
    return Number(tender[key]) === best;
  }

  goBack(): void {
    this.router.navigate(['/tender-list']);
  }
}
