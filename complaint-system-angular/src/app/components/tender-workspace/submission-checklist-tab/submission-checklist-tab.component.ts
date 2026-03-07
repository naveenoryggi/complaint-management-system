import { Component, OnInit, inject, input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSliderModule } from '@angular/material/slider';
import { MatTabsModule } from '@angular/material/tabs';
import { AssemblyService } from '../../../services/assembly.service';
import {
  TenderService,
  SubmissionChecklistItem,
  ChecklistSummary,
  CoverPackage,
  CoverAssemblyResult,
  ChecklistAnalysisItem,
  ChecklistAnalysisResult,
  DuplicateItem,
  DuplicatePreviewResult,
} from '../../../services/tender.service';

@Component({
  selector: 'app-submission-checklist-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatBadgeModule,
    MatSliderModule,
    MatTabsModule
  ],
  templateUrl: './submission-checklist-tab.component.html',
  styleUrls: ['./submission-checklist-tab.component.css']
})
export class SubmissionChecklistTabComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private assemblyService = inject(AssemblyService);
  private snackBar = inject(MatSnackBar);

  checklistItems = signal<SubmissionChecklistItem[]>([]);
  summary = signal<ChecklistSummary | null>(null);
  covers = signal<CoverPackage[]>([]);
  loading = signal(false);
  generating = signal(false);
  assemblingCover = signal<string>('');  // cover name currently being assembled
  assemblingAll = signal(false);

  // Filters
  modeFilter = signal<string>('all');
  categoryFilter = signal<string>('all');
  viewMode = signal<'checklist' | 'covers'>('checklist');

  // AI Analysis
  analyzing = signal(false);
  analysisResult = signal<ChecklistAnalysisResult | null>(null);
  analysisMap = signal<Map<string, ChecklistAnalysisItem>>(new Map());
  showIrrelevant = signal(true);
  removingIrrelevant = signal(false);

  // Duplicate Detection
  showDedupPanel = signal(false);
  dedupThreshold = signal(0.6);
  dedupLoading = signal(false);
  dedupRemoving = signal(false);
  dedupResult = signal<DuplicatePreviewResult | null>(null);

  // New item form
  showNewItem = signal(false);
  newItem: Partial<SubmissionChecklistItem> = {};

  // Filters
  originFilter = signal<string>('all');

  displayedColumns = ['critical', 'document', 'origin', 'category', 'mode', 'envelope', 'online', 'offline', 'notarization', 'actions'];

  filteredItems = computed(() => {
    let items = this.checklistItems();
    const mode = this.modeFilter();
    const category = this.categoryFilter();
    const origin = this.originFilter();
    const aMap = this.analysisMap();

    if (mode !== 'all') {
      if (mode === 'online' || mode === 'offline') {
        items = items.filter(i => i.submission_mode === mode || i.submission_mode === 'both');
      } else {
        items = items.filter(i => i.submission_mode === mode);
      }
    }
    if (category !== 'all') {
      items = items.filter(i => i.document_category === category);
    }
    if (origin !== 'all') {
      items = items.filter(i => i.document_origin === origin);
    }
    // Hide irrelevant items if analysis done and toggle is off
    if (aMap.size > 0 && !this.showIrrelevant()) {
      items = items.filter(i => {
        const a = aMap.get(i.id);
        return !a || a.relevant;
      });
    }
    return items;
  });

  // Status configs
  onlineStatuses = [
    { value: 'not_started', label: 'Not Started', icon: 'radio_button_unchecked', color: '#9e9e9e' },
    { value: 'in_progress', label: 'In Progress', icon: 'pending', color: '#ff9800' },
    { value: 'uploaded', label: 'Uploaded', icon: 'cloud_upload', color: '#2196f3' },
    { value: 'digitally_signed', label: 'Digitally Signed', icon: 'verified', color: '#4caf50' },
    { value: 'verified', label: 'Verified', icon: 'check_circle', color: '#2e7d32' },
  ];

  offlineStatuses = [
    { value: 'not_started', label: 'Not Started', icon: 'radio_button_unchecked', color: '#9e9e9e' },
    { value: 'prepared', label: 'Prepared', icon: 'description', color: '#ff9800' },
    { value: 'dispatched', label: 'Dispatched', icon: 'local_shipping', color: '#2196f3' },
    { value: 'received', label: 'Received', icon: 'markunread_mailbox', color: '#4caf50' },
    { value: 'verified', label: 'Verified', icon: 'check_circle', color: '#2e7d32' },
  ];

  notarizationStatuses = [
    { value: 'not_required', label: 'Not Required', icon: 'remove_circle_outline', color: '#9e9e9e' },
    { value: 'pending', label: 'Pending', icon: 'pending', color: '#f44336' },
    { value: 'in_progress', label: 'In Progress', icon: 'hourglass_empty', color: '#ff9800' },
    { value: 'completed', label: 'Completed', icon: 'task_alt', color: '#4caf50' },
    { value: 'verified', label: 'Verified', icon: 'verified', color: '#2e7d32' },
  ];

  categories = [
    { value: 'technical', label: 'Technical', icon: 'engineering' },
    { value: 'financial', label: 'Financial', icon: 'payments' },
    { value: 'emd', label: 'EMD', icon: 'account_balance' },
    { value: 'oem', label: 'OEM', icon: 'precision_manufacturing' },
    { value: 'declaration', label: 'Declaration', icon: 'gavel' },
    { value: 'certificate', label: 'Certificate', icon: 'verified_user' },
    { value: 'legal', label: 'Legal', icon: 'policy' },
    { value: 'company_profile', label: 'Company', icon: 'business' },
    { value: 'experience', label: 'Experience', icon: 'work_history' },
    { value: 'other', label: 'Other', icon: 'folder' },
  ];

  envelopes = [
    { value: 'envelope_a', label: 'Envelope A (Technical)' },
    { value: 'envelope_b', label: 'Envelope B (Financial)' },
    { value: 'superscription', label: 'Superscription' },
  ];

  documentOrigins = [
    { value: 'tender_provided', label: 'Tender Format', icon: 'article', color: '#1565c0', bgColor: '#e3f2fd', hint: 'Format/annexure given in tender — fill and submit' },
    { value: 'self_generated', label: 'Self Generated', icon: 'edit_note', color: '#2e7d32', bgColor: '#e8f5e9', hint: 'Bidder creates — platform can help generate' },
    { value: 'pre_existing', label: 'Pre-existing', icon: 'inventory', color: '#e65100', bgColor: '#fff3e0', hint: 'Already have or obtain from third party' },
  ];

  ngOnInit() {
    if (this.tenderId()) {
      this.loadData();
    }
  }

  loadData() {
    const id = this.tenderId();
    if (!id) return;

    this.loading.set(true);
    this.tenderService.getChecklistItems(id).subscribe({
      next: (items) => {
        this.checklistItems.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to load checklist', 'Close', { duration: 5000 });
        this.loading.set(false);
      }
    });
    this.loadSummary();
    this.loadCovers();
  }

  loadSummary() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.getChecklistSummary(id).subscribe({
      next: (s) => this.summary.set(s),
      error: (err) => { console.error('Error loading checklist summary:', err); }
    });
  }

  loadCovers() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.getChecklistCovers(id).subscribe({
      next: (c) => this.covers.set(c),
      error: (err) => { console.error('Error loading covers:', err); }
    });
  }

  onGenerateChecklist() {
    const id = this.tenderId();
    if (!id) return;

    this.generating.set(true);
    this.tenderService.generateChecklist(id).subscribe({
      next: (res) => {
        this.snackBar.open(res.message, 'Close', { duration: 3000 });
        this.generating.set(false);
        this.loadData();
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to generate checklist', 'Close', { duration: 5000 });
        this.generating.set(false);
      }
    });
  }

  // --- AI Analysis ---
  private cacheKey(tenderId: string): string {
    return `checklist_analysis_${tenderId}`;
  }

  onAnalyzeChecklist() {
    const id = this.tenderId();
    if (!id) return;

    // Check cache first
    try {
      const cached = sessionStorage.getItem(this.cacheKey(id));
      if (cached) {
        const result = JSON.parse(cached) as ChecklistAnalysisResult;
        this.applyAnalysisResult(result);
        this.snackBar.open('Loaded cached analysis. Click "Re-analyze" to refresh.', 'Close', { duration: 4000 });
        return;
      }
    } catch { }

    this.analyzing.set(true);
    this.tenderService.analyzeChecklist(id).subscribe({
      next: (result) => {
        this.analyzing.set(false);
        this.applyAnalysisResult(result);
        // Cache
        try { sessionStorage.setItem(this.cacheKey(id), JSON.stringify(result)); } catch { }
        // Reload items from DB to pick up persisted is_critical changes
        this.loadData();
        const updated = result.updated_count || 0;
        this.snackBar.open(
          `Analysis complete: ${result.relevant_count} relevant, ${result.irrelevant_count} not needed. ${updated} item(s) updated.`,
          'Close', { duration: 5000 }
        );
      },
      error: (err) => {
        this.analyzing.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to analyze checklist', 'Close', { duration: 5000 });
      }
    });
  }

  onReanalyze() {
    const id = this.tenderId();
    if (!id) return;
    try { sessionStorage.removeItem(this.cacheKey(id)); } catch { }
    this.analysisResult.set(null);
    this.analysisMap.set(new Map());
    this.analyzing.set(true);
    this.tenderService.analyzeChecklist(id).subscribe({
      next: (result) => {
        this.analyzing.set(false);
        this.applyAnalysisResult(result);
        try { sessionStorage.setItem(this.cacheKey(id), JSON.stringify(result)); } catch { }
        this.loadData();
        const updated = result.updated_count || 0;
        this.snackBar.open(
          `Re-analysis complete: ${result.relevant_count} relevant, ${result.irrelevant_count} not needed. ${updated} updated.`,
          'Close', { duration: 5000 }
        );
      },
      error: (err) => {
        this.analyzing.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to analyze checklist', 'Close', { duration: 5000 });
      }
    });
  }

  private applyAnalysisResult(result: ChecklistAnalysisResult) {
    this.analysisResult.set(result);
    const map = new Map<string, ChecklistAnalysisItem>();
    for (const a of result.analysis) {
      map.set(a.item_id, a);
    }
    this.analysisMap.set(map);

    // Update is_critical based on AI's is_required determination
    const items = this.checklistItems();
    const updated = items.map(item => {
      const a = map.get(item.id);
      if (a) {
        return { ...item, is_critical: a.is_required };
      }
      return item;
    });
    this.checklistItems.set(updated);
  }

  getItemAnalysis(itemId: string): ChecklistAnalysisItem | undefined {
    return this.analysisMap().get(itemId);
  }

  getIrrelevantItemIds(): string[] {
    const ids: string[] = [];
    this.analysisMap().forEach((a, id) => {
      if (!a.relevant) ids.push(id);
    });
    return ids;
  }

  onRemoveIrrelevant() {
    const id = this.tenderId();
    if (!id) return;
    const itemIds = this.getIrrelevantItemIds();
    if (itemIds.length === 0) {
      this.snackBar.open('No irrelevant items to remove', 'Close', { duration: 3000 });
      return;
    }
    if (!confirm(`Remove ${itemIds.length} irrelevant item(s) from checklist?`)) return;

    this.removingIrrelevant.set(true);
    this.tenderService.removeIrrelevantItems(id, itemIds).subscribe({
      next: (result) => {
        this.removingIrrelevant.set(false);
        // Remove from local state
        this.checklistItems.set(this.checklistItems().filter(i => !itemIds.includes(i.id)));
        // Clear analysis for removed items
        const map = new Map(this.analysisMap());
        for (const iid of itemIds) map.delete(iid);
        this.analysisMap.set(map);
        this.loadSummary();
        this.loadCovers();
        this.snackBar.open(result.message, 'Close', { duration: 4000 });
      },
      error: (err) => {
        this.removingIrrelevant.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to remove items', 'Close', { duration: 5000 });
      }
    });
  }

  // --- Duplicate Detection ---
  onToggleDedupPanel() {
    const opening = !this.showDedupPanel();
    this.showDedupPanel.set(opening);
    if (opening) {
      this.onPreviewDuplicates();
    }
  }

  onPreviewDuplicates() {
    const id = this.tenderId();
    if (!id) return;
    this.dedupLoading.set(true);
    this.tenderService.previewDuplicates(id, this.dedupThreshold()).subscribe({
      next: (result) => {
        this.dedupLoading.set(false);
        this.dedupResult.set(result);
      },
      error: (err) => {
        this.dedupLoading.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to check duplicates', 'Close', { duration: 5000 });
      }
    });
  }

  onRemoveDuplicates() {
    const id = this.tenderId();
    if (!id) return;
    const result = this.dedupResult();
    if (!result || result.duplicate_count === 0) return;
    if (!confirm(`Remove ${result.duplicate_count} duplicate(s) at threshold ${result.threshold}?`)) return;

    this.dedupRemoving.set(true);
    this.tenderService.deduplicateChecklist(id, this.dedupThreshold()).subscribe({
      next: (res) => {
        this.dedupRemoving.set(false);
        this.dedupResult.set(null);
        this.snackBar.open(res.message, 'Close', { duration: 4000 });
        this.loadData();
      },
      error: (err) => {
        this.dedupRemoving.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to remove duplicates', 'Close', { duration: 5000 });
      }
    });
  }

  getMatchTypeLabel(type: string): string {
    switch (type) {
      case 'exact': return 'Exact match';
      case 'core_name': return 'Same core name';
      case 'substring': return 'Name contained in other';
      case 'word_overlap': return 'Similar words';
      default: return type;
    }
  }

  onStatusChange(item: SubmissionChecklistItem, field: 'online_status' | 'offline_status' | 'notarization_status', value: string) {
    const update: any = {};
    update[field] = value;
    this.tenderService.updateChecklistItem(item.id, update).subscribe({
      next: (updated) => {
        const items = this.checklistItems().map(i => i.id === updated.id ? updated : i);
        this.checklistItems.set(items);
        this.loadSummary();
        this.loadCovers();
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to update status', 'Close', { duration: 5000 });
      }
    });
  }

  onCoverChange(item: SubmissionChecklistItem, coverName: string) {
    this.tenderService.updateChecklistItem(item.id, { cover_name: coverName } as any).subscribe({
      next: (updated) => {
        const items = this.checklistItems().map(i => i.id === updated.id ? updated : i);
        this.checklistItems.set(items);
        this.loadCovers();
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to update cover', 'Close', { duration: 5000 });
      }
    });
  }

  onNotarizationTypeChange(item: SubmissionChecklistItem, type: string) {
    const update: any = { notarization_type: type };
    if (type) {
      update.notarization_required = true;
      if (item.notarization_status === 'not_required') {
        update.notarization_status = 'pending';
      }
    } else {
      update.notarization_required = false;
      update.notarization_status = 'not_required';
    }
    this.tenderService.updateChecklistItem(item.id, update).subscribe({
      next: (updated) => {
        const items = this.checklistItems().map(i => i.id === updated.id ? updated : i);
        this.checklistItems.set(items);
        this.loadSummary();
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to update', 'Close', { duration: 5000 });
      }
    });
  }

  onDeleteItem(item: SubmissionChecklistItem) {
    if (!confirm(`Delete "${item.document_name}"?`)) return;
    this.tenderService.deleteChecklistItem(item.id).subscribe({
      next: () => {
        this.checklistItems.set(this.checklistItems().filter(i => i.id !== item.id));
        this.loadSummary();
        this.loadCovers();
        this.snackBar.open('Item deleted', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 });
      }
    });
  }

  onAddCustomItem() {
    const id = this.tenderId();
    if (!id || !this.newItem.document_name) return;

    this.tenderService.createChecklistItem(id, {
      document_name: this.newItem.document_name,
      document_category: this.newItem.document_category || 'other',
      submission_mode: this.newItem.submission_mode || 'online',
      is_critical: this.newItem.is_critical || false,
      envelope: this.newItem.envelope || 'envelope_a',
      cover_name: this.newItem.cover_name || '',
      notarization_required: this.newItem.notarization_required || false,
      notarization_type: this.newItem.notarization_type || undefined,
      document_origin: (this.newItem as any).document_origin || 'self_generated',
      format_reference: (this.newItem as any).format_reference || undefined,
      obtaining_source: (this.newItem as any).obtaining_source || undefined,
      can_auto_generate: (this.newItem as any).can_auto_generate || false,
      sort_order: this.checklistItems().length + 1,
      source: 'manual',
    }).subscribe({
      next: (created) => {
        this.checklistItems.set([...this.checklistItems(), created]);
        this.newItem = {};
        this.showNewItem.set(false);
        this.loadSummary();
        this.loadCovers();
        this.snackBar.open('Item added', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to add item', 'Close', { duration: 5000 });
      }
    });
  }

  // --- Document Staging ---
  onLinkDocument(item: SubmissionChecklistItem, documentId: string) {
    if (!documentId) return;
    this.tenderService.linkDocumentToChecklist(item.id, documentId).subscribe({
      next: (updated) => {
        const items = this.checklistItems().map(i => i.id === updated.id ? updated : i);
        this.checklistItems.set(items);
        this.loadCovers();
        this.snackBar.open('Document staged', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to link document', 'Close', { duration: 5000 });
      }
    });
  }

  onUnlinkDocument(item: SubmissionChecklistItem) {
    this.tenderService.unlinkDocumentFromChecklist(item.id).subscribe({
      next: (updated) => {
        const items = this.checklistItems().map(i => i.id === updated.id ? updated : i);
        this.checklistItems.set(items);
        this.loadCovers();
        this.snackBar.open('Document unstaged', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to unlink document', 'Close', { duration: 5000 });
      }
    });
  }

  // --- Cover Assembly ---
  onAssembleCover(coverName: string) {
    const id = this.tenderId();
    if (!id) return;

    this.assemblingCover.set(coverName);
    this.tenderService.assembleCover(id, coverName).subscribe({
      next: (result) => {
        this.assemblingCover.set('');
        this.snackBar.open(result.message, 'Download', { duration: 8000 })
          .onAction().subscribe(() => {
            this.downloadAssembledFile(result.file_path);
          });
      },
      error: (err) => {
        this.assemblingCover.set('');
        const msg = err.error?.detail || 'Failed to assemble cover';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }

  onAssembleAllCovers() {
    const id = this.tenderId();
    if (!id) return;

    this.assemblingAll.set(true);
    this.tenderService.assembleAllCovers(id).subscribe({
      next: (result) => {
        this.assemblingAll.set(false);
        this.snackBar.open(result.message, 'Download ZIP', { duration: 10000 })
          .onAction().subscribe(() => {
            this.downloadAssembledFile(result.file_path);
          });
      },
      error: (err) => {
        this.assemblingAll.set(false);
        const msg = err.error?.detail || 'Failed to assemble bid package';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }

  downloadAssembledFile(filePath: string) {
    const parts = filePath.split('/');
    if (parts.length === 2) {
      const fileType = parts[0] as 'assembled' | 'packages';
      const filename = parts[1];

      this.assemblyService.downloadFile(fileType, filename).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = filename;
          document.body.appendChild(a);
          a.click();
          window.URL.revokeObjectURL(url);
          document.body.removeChild(a);
        },
        error: (err) => {
          this.snackBar.open(err.error?.detail || 'Failed to download file', 'Close', { duration: 5000 });
        }
      });
    }
  }

  getStagedCount(cover: CoverPackage): number {
    return cover.documents.filter(d => d.online_status !== 'not_started' || d.offline_status !== 'not_started').length;
  }

  // Helpers
  getCategoryIcon(cat: string): string {
    return this.categories.find(c => c.value === cat)?.icon || 'folder';
  }

  getCategoryLabel(cat: string): string {
    return this.categories.find(c => c.value === cat)?.label || cat;
  }

  getModeLabel(mode: string): string {
    return mode === 'both' ? 'BOTH' : mode.toUpperCase().substring(0, 3);
  }

  getModeColor(mode: string): string {
    switch (mode) {
      case 'online': return 'primary';
      case 'offline': return 'accent';
      case 'both': return 'warn';
      default: return '';
    }
  }

  getEnvelopeLabel(envelope: string): string {
    switch (envelope) {
      case 'envelope_a': return 'Cover A';
      case 'envelope_b': return 'Cover B';
      case 'superscription': return 'Super';
      default: return envelope || '--';
    }
  }

  getProgressBarClass(pct: number): string {
    if (pct >= 80) return 'progress-green';
    if (pct >= 50) return 'progress-amber';
    return 'progress-red';
  }

  getStatusConfig(statuses: any[], value: string) {
    return statuses.find(s => s.value === value) || statuses[0];
  }

  hasOnlineColumn(item: SubmissionChecklistItem): boolean {
    return item.submission_mode === 'online' || item.submission_mode === 'both';
  }

  hasOfflineColumn(item: SubmissionChecklistItem): boolean {
    return item.submission_mode === 'offline' || item.submission_mode === 'both';
  }

  getNotarizationLabel(type: string | undefined): string {
    if (!type) return '--';
    return type === 'judicial' ? 'Judicial' : 'Non-Judicial';
  }

  getCoverReadinessPercentage(cover: CoverPackage): number {
    return cover.total > 0 ? Math.round(cover.ready / cover.total * 100) : 0;
  }

  getOriginConfig(origin: string) {
    return this.documentOrigins.find(o => o.value === origin) || this.documentOrigins[1];
  }

  getOriginLabel(origin: string): string {
    return this.getOriginConfig(origin).label;
  }

  getOriginIcon(origin: string): string {
    return this.getOriginConfig(origin).icon;
  }

  getOriginHint(item: SubmissionChecklistItem): string {
    const config = this.getOriginConfig(item.document_origin);
    let hint = config.hint;
    if (item.format_reference) {
      hint += ` | Ref: ${item.format_reference}`;
    }
    if (item.obtaining_source) {
      hint += ` | Source: ${item.obtaining_source}`;
    }
    return hint;
  }
}
