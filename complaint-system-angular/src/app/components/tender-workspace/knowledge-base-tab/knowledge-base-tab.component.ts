import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  KnowledgeBaseService,
  KBEntry,
  KBCategoryStat,
  KBEntryCreate,
  KBEntryUpdate,
} from '../../../services/knowledge-base.service';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

interface CategoryChip {
  key: string | null; // null = "All"
  label: string;
  icon: string;
  count: number;
}

const CATEGORY_META: { [key: string]: { label: string; icon: string } } = {
  company_snippet:    { label: 'Company',       icon: 'business' },
  financial_data:     { label: 'Financial',     icon: 'account_balance' },
  experience_summary: { label: 'Experience',    icon: 'work_history' },
  declaration_text:   { label: 'Declarations',  icon: 'gavel' },
  technical_response: { label: 'Technical',     icon: 'engineering' },
  personnel_bio:      { label: 'Personnel',     icon: 'people' },
  pricing_data:       { label: 'Pricing',       icon: 'attach_money' },
  standard_clause:    { label: 'Clauses',       icon: 'description' },
  submission_note:    { label: 'Notes',          icon: 'sticky_note_2' },
};

@Component({
  selector: 'app-knowledge-base-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatDividerModule,
    MatBadgeModule,
    MatCheckboxModule,
    MatSnackBarModule,
  ],
  templateUrl: './knowledge-base-tab.component.html',
  styleUrls: ['./knowledge-base-tab.component.css'],
})
export class KnowledgeBaseTabComponent implements OnInit {
  tenderId = input<string>('');

  private kbService = inject(KnowledgeBaseService);
  private snackBar = inject(MatSnackBar);

  // State
  entries = signal<KBEntry[]>([]);
  categoryStats = signal<KBCategoryStat[]>([]);
  categoryChips = signal<CategoryChip[]>([]);
  loading = signal(false);
  harvesting = signal(false);
  totalEntries = signal(0);
  currentPage = signal(1);
  totalPages = signal(0);

  // Filters
  selectedCategory = signal<string | null>(null);
  searchQuery = signal('');

  // Add form
  showAddForm = signal(false);
  newEntry: KBEntryCreate = this._emptyEntry();

  // Edit state
  editingEntryId = signal<string | null>(null);
  editData: KBEntryUpdate = {};

  // Generate summary
  showGeneratePanel = signal(false);
  selectedForSummary = signal<Set<string>>(new Set());
  generatingSummary = signal(false);

  // Search debounce
  private searchSubject = new Subject<string>();

  // Category options for dropdown
  categoryOptions = Object.entries(CATEGORY_META).map(([key, meta]) => ({
    value: key,
    label: meta.label,
  }));

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.currentPage.set(1);
      this.loadEntries();
    });

    this.loadCategoryStats();
    this.loadEntries();
  }

  // ---------------------------------------------------------------------------
  // Data loading
  // ---------------------------------------------------------------------------

  loadEntries() {
    this.loading.set(true);
    this.kbService.listEntries({
      category: this.selectedCategory() || undefined,
      search: this.searchQuery() || undefined,
      page: this.currentPage(),
      page_size: 50,
    }).subscribe({
      next: (res) => {
        this.entries.set(res.items);
        this.totalEntries.set(res.total);
        this.totalPages.set(res.total_pages);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load KB entries', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  loadCategoryStats() {
    this.kbService.getCategoryStats().subscribe({
      next: (stats) => {
        this.categoryStats.set(stats);
        this.buildCategoryChips(stats);
      },
      error: () => {},
    });
  }

  private buildCategoryChips(stats: KBCategoryStat[]) {
    const total = stats.reduce((sum, s) => sum + s.count, 0);
    const chips: CategoryChip[] = [
      { key: null, label: 'All', icon: 'apps', count: total },
    ];
    for (const stat of stats) {
      const meta = CATEGORY_META[stat.category];
      if (meta) {
        chips.push({
          key: stat.category,
          label: meta.label,
          icon: meta.icon,
          count: stat.count,
        });
      }
    }
    this.categoryChips.set(chips);
  }

  // ---------------------------------------------------------------------------
  // Filters
  // ---------------------------------------------------------------------------

  onCategorySelect(key: string | null) {
    this.selectedCategory.set(key);
    this.currentPage.set(1);
    this.loadEntries();
  }

  onSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value);
  }

  onPageChange(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.loadEntries();
  }

  // ---------------------------------------------------------------------------
  // Actions
  // ---------------------------------------------------------------------------

  onCopyContent(entry: KBEntry) {
    navigator.clipboard.writeText(entry.content).then(() => {
      this.snackBar.open('Content copied to clipboard', 'Close', { duration: 2000 });
      // Record usage if inside a tender
      if (this.tenderId()) {
        this.kbService.recordUsage(entry.id, this.tenderId(), 'clipboard_copy').subscribe();
      }
    });
  }

  onStartEdit(entry: KBEntry) {
    this.editingEntryId.set(entry.id);
    this.editData = {
      title: entry.title,
      content: entry.content,
      category: entry.category,
      subcategory: entry.subcategory || undefined,
      tags: entry.tags || undefined,
      fiscal_year: entry.fiscal_year || undefined,
      is_verified: entry.is_verified,
    };
  }

  onCancelEdit() {
    this.editingEntryId.set(null);
    this.editData = {};
  }

  onSaveEdit(entryId: string) {
    this.kbService.updateEntry(entryId, this.editData).subscribe({
      next: () => {
        this.snackBar.open('Entry updated', 'Close', { duration: 2000 });
        this.editingEntryId.set(null);
        this.editData = {};
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.snackBar.open('Failed to update entry', 'Close', { duration: 3000 });
      },
    });
  }

  onArchive(entry: KBEntry) {
    this.kbService.updateEntry(entry.id, { is_archived: true }).subscribe({
      next: () => {
        this.snackBar.open('Entry archived', 'Close', { duration: 2000 });
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.snackBar.open('Failed to archive entry', 'Close', { duration: 3000 });
      },
    });
  }

  onDelete(entry: KBEntry) {
    if (!confirm(`Delete "${entry.title}"? This cannot be undone.`)) return;
    this.kbService.deleteEntry(entry.id).subscribe({
      next: () => {
        this.snackBar.open('Entry deleted', 'Close', { duration: 2000 });
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.snackBar.open('Failed to delete entry', 'Close', { duration: 3000 });
      },
    });
  }

  onToggleVerified(entry: KBEntry) {
    this.kbService.updateEntry(entry.id, { is_verified: !entry.is_verified }).subscribe({
      next: () => {
        this.loadEntries();
      },
      error: () => {
        this.snackBar.open('Failed to update verification', 'Close', { duration: 3000 });
      },
    });
  }

  // ---------------------------------------------------------------------------
  // Add form
  // ---------------------------------------------------------------------------

  onOpenAddForm() {
    this.showAddForm.set(true);
    this.newEntry = this._emptyEntry();
    if (this.tenderId()) {
      this.newEntry.source_tender_id = this.tenderId();
    }
  }

  onCancelAdd() {
    this.showAddForm.set(false);
  }

  onSaveNew() {
    if (!this.newEntry.title || !this.newEntry.content || !this.newEntry.category) {
      this.snackBar.open('Title, content, and category are required', 'Close', { duration: 3000 });
      return;
    }
    this.kbService.createEntry(this.newEntry).subscribe({
      next: () => {
        this.snackBar.open('Entry created', 'Close', { duration: 2000 });
        this.showAddForm.set(false);
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.snackBar.open('Failed to create entry', 'Close', { duration: 3000 });
      },
    });
  }

  // ---------------------------------------------------------------------------
  // Harvesting
  // ---------------------------------------------------------------------------

  onHarvestFromTender() {
    const id = this.tenderId();
    if (!id) return;
    this.harvesting.set(true);
    this.kbService.harvestFromTender(id).subscribe({
      next: (result) => {
        this.harvesting.set(false);
        this.snackBar.open(
          `Harvested ${result.created} entries (${result.skipped} duplicates skipped)`,
          'Close', { duration: 4000 }
        );
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.harvesting.set(false);
        this.snackBar.open('Failed to harvest from tender', 'Close', { duration: 3000 });
      },
    });
  }

  onHarvestCompanyData() {
    this.harvesting.set(true);
    this.kbService.harvestCompanyData().subscribe({
      next: (result) => {
        this.harvesting.set(false);
        this.snackBar.open(
          `Synced ${result.created} entries from company data (${result.skipped} skipped)`,
          'Close', { duration: 4000 }
        );
        this.loadEntries();
        this.loadCategoryStats();
      },
      error: () => {
        this.harvesting.set(false);
        this.snackBar.open('Failed to sync company data', 'Close', { duration: 3000 });
      },
    });
  }

  // ---------------------------------------------------------------------------
  // Generate Summary
  // ---------------------------------------------------------------------------

  onToggleGeneratePanel() {
    const show = !this.showGeneratePanel();
    this.showGeneratePanel.set(show);
    if (!show) {
      this.selectedForSummary.set(new Set());
    }
  }

  onToggleSelectEntry(entryId: string) {
    const current = new Set(this.selectedForSummary());
    if (current.has(entryId)) {
      current.delete(entryId);
    } else {
      current.add(entryId);
    }
    this.selectedForSummary.set(current);
  }

  isEntrySelected(entryId: string): boolean {
    return this.selectedForSummary().has(entryId);
  }

  onSelectAll() {
    const all = new Set(this.entries().map(e => e.id));
    this.selectedForSummary.set(all);
  }

  onDeselectAll() {
    this.selectedForSummary.set(new Set());
  }

  onGenerateSummary() {
    const ids = Array.from(this.selectedForSummary());
    const tid = this.tenderId();
    if (ids.length === 0 || !tid) {
      this.snackBar.open('Select at least one entry', 'Close', { duration: 3000 });
      return;
    }
    this.generatingSummary.set(true);
    this.kbService.generateSummary(tid, ids).subscribe({
      next: (blob) => {
        this.generatingSummary.set(false);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'kb_summary.docx';
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        this.snackBar.open('Summary document downloaded', 'Close', { duration: 3000 });
      },
      error: () => {
        this.generatingSummary.set(false);
        this.snackBar.open('Failed to generate summary', 'Close', { duration: 3000 });
      },
    });
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  getCategoryIcon(category: string): string {
    return CATEGORY_META[category]?.icon || 'article';
  }

  getCategoryLabel(category: string): string {
    return CATEGORY_META[category]?.label || category;
  }

  getSourceIcon(sourceType: string): string {
    switch (sourceType) {
      case 'auto_harvested': return 'agriculture';
      case 'ai_generated': return 'auto_awesome';
      default: return 'edit';
    }
  }

  getSourceLabel(sourceType: string): string {
    switch (sourceType) {
      case 'auto_harvested': return 'Harvested';
      case 'ai_generated': return 'AI Generated';
      default: return 'Manual';
    }
  }

  getFreshnessClass(entry: KBEntry): string {
    if (!entry.valid_until) return 'fresh';
    const until = new Date(entry.valid_until);
    const now = new Date();
    const daysUntil = Math.ceil((until.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    if (daysUntil < 0) return 'stale';
    if (daysUntil <= 90) return 'expiring';
    return 'fresh';
  }

  getContentPreview(content: string, maxLen: number = 150): string {
    if (content.length <= maxLen) return content;
    return content.substring(0, maxLen) + '...';
  }

  getTagsString(): string {
    return (this.newEntry.tags || []).join(', ');
  }

  onTagsInput(value: string) {
    this.newEntry.tags = value.split(',').map(t => t.trim()).filter(t => t.length > 0);
  }

  private _emptyEntry(): KBEntryCreate {
    return {
      title: '',
      content: '',
      category: 'company_snippet',
      subcategory: '',
      tags: [],
      fiscal_year: '',
      source_type: 'manual',
    };
  }
}
