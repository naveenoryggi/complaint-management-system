import { Component, OnInit, inject, input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TenderService, PrebidQuery, PrebidQueriesResult, Tender, SuggestResponseResult } from '../../../services/tender.service';

@Component({
  selector: 'app-prebid-queries-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
  ],
  templateUrl: './prebid-queries-tab.component.html',
  styleUrls: ['./prebid-queries-tab.component.css'],
})
export class PrebidQueriesTabComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);

  queries = signal<PrebidQuery[]>([]);
  generating = signal(false);
  exporting = signal(false);
  tenderTitle = signal('');
  tenderReference = signal('');
  selectedCategory = signal<string | null>(null);
  editingIndex = signal<number | null>(null);
  editData: Partial<PrebidQuery> = {};
  suggestingIndex = signal<number | null>(null);
  showAddForm = signal(false);
  newQuery: Partial<PrebidQuery> = { sno: 0, tender_reference: '', existing_clause: '', clarification_sought: '', response: '', category: 'ambiguous_specs', priority: 'medium' };

  categoryLabels: { [key: string]: string } = {
    ambiguous_specs: 'Ambiguous Specs',
    contradictory_clauses: 'Contradictory',
    restrictive_conditions: 'Restrictive',
    missing_timelines: 'Timelines',
    unclear_evaluation: 'Evaluation',
    onerous_commercial: 'Commercial',
  };

  priorityColors: { [key: string]: string } = {
    high: '#f44336',
    medium: '#ff9800',
    low: '#4caf50',
  };

  filteredQueries = computed(() => {
    const cat = this.selectedCategory();
    const all = this.queries();
    if (!cat) return all;
    return all.filter(q => q.category === cat);
  });

  categoryBreakdown = computed(() => {
    const all = this.queries();
    const counts: { [key: string]: number } = {};
    for (const q of all) {
      counts[q.category] = (counts[q.category] || 0) + 1;
    }
    return counts;
  });

  ngOnInit() {
    if (this.tenderId()) {
      this.loadTenderInfo();
    }
  }

  loadTenderInfo() {
    this.tenderService.getTender(this.tenderId()).subscribe({
      next: (tender) => {
        this.tenderTitle.set(tender.title);
        this.tenderReference.set(tender.reference_number || '');
      },
      error: (err) => { console.error('Error loading tender info:', err); }
    });
  }

  onGenerate() {
    const id = this.tenderId();
    if (!id) return;
    this.generating.set(true);
    this.tenderService.generatePrebidQueries(id).subscribe({
      next: (result) => {
        this.queries.set(result.queries);
        if (result.tender_title) this.tenderTitle.set(result.tender_title);
        if (result.tender_reference) this.tenderReference.set(result.tender_reference);
        this.generating.set(false);
        this.snackBar.open(`Generated ${result.total} pre-bid queries`, 'Close', { duration: 4000 });
      },
      error: (err) => {
        this.generating.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to generate queries', 'Close', { duration: 5000 });
      }
    });
  }

  onExport() {
    const id = this.tenderId();
    if (!id) return;
    this.exporting.set(true);
    this.tenderService.exportPrebidQueries(id, this.queries(), this.tenderTitle(), this.tenderReference()).subscribe({
      next: (blob) => {
        this.exporting.set(false);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'prebid-queries.docx';
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        this.snackBar.open('Pre-bid queries exported', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.exporting.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to export', 'Close', { duration: 5000 });
      }
    });
  }

  onCategoryFilter(category: string | null) {
    this.selectedCategory.set(category);
  }

  onStartEdit(index: number) {
    const q = this.queries()[index];
    this.editingIndex.set(index);
    this.editData = { ...q };
  }

  onCancelEdit() {
    this.editingIndex.set(null);
    this.editData = {};
  }

  onSaveEdit() {
    const idx = this.editingIndex();
    if (idx === null) return;
    const updated = [...this.queries()];
    updated[idx] = { ...updated[idx], ...this.editData } as PrebidQuery;
    this.queries.set(updated);
    this.editingIndex.set(null);
    this.editData = {};
    this.snackBar.open('Query updated', 'Close', { duration: 2000 });
  }

  onDeleteQuery(index: number) {
    const updated = this.queries().filter((_, i) => i !== index);
    // Re-number
    updated.forEach((q, i) => q.sno = i + 1);
    this.queries.set(updated);
  }

  onAddManual() {
    this.showAddForm.set(true);
    this.newQuery = {
      sno: this.queries().length + 1,
      tender_reference: '',
      existing_clause: '',
      clarification_sought: '',
      response: '',
      category: 'ambiguous_specs',
      priority: 'medium',
    };
  }

  onSaveNewQuery() {
    if (!this.newQuery.tender_reference || !this.newQuery.clarification_sought) {
      this.snackBar.open('Reference and clarification are required', 'Close', { duration: 3000 });
      return;
    }
    const updated = [...this.queries(), this.newQuery as PrebidQuery];
    this.queries.set(updated);
    this.showAddForm.set(false);
    this.snackBar.open('Query added', 'Close', { duration: 2000 });
  }

  getCategoryLabel(category: string): string {
    return this.categoryLabels[category] || category;
  }

  getCategoryKeys(): string[] {
    return Object.keys(this.categoryBreakdown());
  }

  getPriorityColor(priority: string): string {
    return this.priorityColors[priority] || '#9e9e9e';
  }

  onSuggestResponse(index: number) {
    const id = this.tenderId();
    if (!id) return;
    const query = this.filteredQueries()[index];
    if (!query) return;

    this.suggestingIndex.set(index);
    this.tenderService.suggestPrebidResponse(id, index, query, this.queries()).subscribe({
      next: (result) => {
        // Update the query's response with the AI suggestion
        const allQueries = [...this.queries()];
        const actualIndex = allQueries.findIndex(q => q.sno === query.sno);
        if (actualIndex >= 0) {
          allQueries[actualIndex] = { ...allQueries[actualIndex], response: result.suggested_response };
          this.queries.set(allQueries);
        }
        this.suggestingIndex.set(null);
        this.snackBar.open(`AI suggested response (${result.model_used})`, 'Close', { duration: 4000 });
      },
      error: (err) => {
        this.suggestingIndex.set(null);
        this.snackBar.open(err.error?.detail || 'Failed to generate suggestion', 'Close', { duration: 5000 });
      }
    });
  }
}
