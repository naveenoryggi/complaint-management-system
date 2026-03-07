import { Component, OnInit, inject, signal, input, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import {
  DeclarationService,
  DeclarationType,
  DeclarationAnalysisItem,
  DeclarationPreview,
  ValidationResult,
  BulkDeclarationRequest,
  SaveAndLinkRequest,
} from '../../../services/declaration.service';
import { KnowledgeBaseService, KBEntry } from '../../../services/knowledge-base.service';
import { TenderService, AutoFillSuggestion } from '../../../services/tender.service';

interface DeclarationItem extends DeclarationType {
  selected: boolean;
  mode: 'standard' | 'ai';       // generation mode
  ai_recommended: boolean;        // Claude flagged this
  reason: string | null;          // why AI is recommended
  tender_specific_points: string[]; // specific tender points
  previewing: boolean;            // loading state for single preview
  previewContent: string | null;  // inline preview text
  previewExpanded: boolean;       // show/hide preview content
}

interface CategoryGroup {
  name: string;
  label: string;
  items: DeclarationItem[];
}

interface PreviewItem extends DeclarationPreview {
  approved: boolean;
  expanded: boolean;
  regenerating: boolean;
}

interface GeneratedDoc {
  key: string;
  name: string;
  filename: string;
  mode: 'standard' | 'ai';
}

@Component({
  selector: 'app-declarations-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatDividerModule,
    MatChipsModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatSelectModule,
  ],
  templateUrl: './declarations-tab.component.html',
  styleUrls: ['./declarations-tab.component.css'],
})
export class DeclarationsTabComponent implements OnInit {
  tenderId = input<string>('');

  private declarationService = inject(DeclarationService);
  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);
  private kbService = inject(KnowledgeBaseService);

  loading = signal(false);
  analyzing = signal(false);
  generating = signal(false);
  analyzed = signal(false);
  categories = signal<CategoryGroup[]>([]);
  allTypes = signal<DeclarationItem[]>([]);
  signatoryName = signal('');
  designation = signal('');
  bidderRole = signal<'bidder' | 'oem'>('bidder');
  generatedDocs = signal<GeneratedDoc[]>([]);

  previews = signal<PreviewItem[]>([]);
  previewing = signal(false);

  saving = signal(false);
  saveResult = signal<{ saved: number; linked: number } | null>(null);

  kbSuggestions = signal<KBEntry[]>([]);
  showKBSuggestions = signal(false);
  autoFillSuggestions = signal<AutoFillSuggestion[]>([]);
  autoFillLoading = signal(false);

  selectedCount = computed(() => this.allTypes().filter(t => t.selected).length);
  aiSelectedCount = computed(() => this.allTypes().filter(t => t.selected && t.mode === 'ai').length);
  aiRecommendedCount = computed(() => this.allTypes().filter(t => t.ai_recommended).length);
  allSelected = computed(() => {
    const types = this.allTypes();
    return types.length > 0 && types.every(t => t.selected);
  });
  approvedCount = computed(() => this.previews().filter(p => p.approved).length);
  previewsWithWarnings = computed(() => this.previews().filter(p => p.validation?.status === 'warning').length);

  private categoryLabels: Record<string, string> = {
    eligibility: 'Eligibility',
    compliance: 'Compliance',
    financial: 'Financial',
    technical: 'Technical',
  };

  ngOnInit() {
    this.loadTypes();
    this.loadKBSuggestions();
  }

  loadTypes() {
    this.loading.set(true);
    this.declarationService.getDeclarationTypes().subscribe({
      next: (types) => {
        const items: DeclarationItem[] = types.map(t => ({
          ...t,
          selected: true,
          mode: 'standard' as const,
          ai_recommended: false,
          reason: null,
          tender_specific_points: [],
          previewing: false,
          previewContent: null,
          previewExpanded: false,
        }));
        this.allTypes.set(items);
        this.groupByCategory(items);
        this.loading.set(false);

        // Check for cached analysis before calling API
        const tid = this.tenderId();
        if (tid) {
          const cached = this.loadCachedAnalysis(tid);
          if (cached) {
            this.applyCachedAnalysis(cached);
          } else {
            this.onAnalyzeTender();
          }
        }
      },
      error: (error) => {
        console.error('Error loading declaration types:', error);
        this.snackBar.open('Failed to load declaration types', 'Close', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  private groupByCategory(items: DeclarationItem[]) {
    const grouped: Record<string, DeclarationItem[]> = {};
    for (const item of items) {
      if (!grouped[item.category]) {
        grouped[item.category] = [];
      }
      grouped[item.category].push(item);
    }

    const order = ['eligibility', 'compliance', 'financial', 'technical'];
    const categories: CategoryGroup[] = order
      .filter(cat => grouped[cat])
      .map(cat => ({
        name: cat,
        label: this.categoryLabels[cat] || cat,
        items: grouped[cat],
      }));

    this.categories.set(categories);
  }

  // -----------------------------------------------------------------------
  // Tender Analysis
  // -----------------------------------------------------------------------

  onAnalyzeTender() {
    const tid = this.tenderId();
    if (!tid) {
      this.snackBar.open('No tender linked — analysis requires a tender', 'Close', { duration: 3000 });
      return;
    }

    this.analyzing.set(true);

    this.declarationService.analyzeTender(tid).subscribe({
      next: (result) => {
        this.analyzing.set(false);
        this.analyzed.set(true);

        // Cache analysis results to avoid re-consuming tokens
        this.saveCachedAnalysis(tid, result.analysis);

        // Merge analysis into declaration items
        const analysisMap = new Map<string, DeclarationAnalysisItem>();
        for (const a of result.analysis) {
          analysisMap.set(a.key, a);
        }

        const updated = this.allTypes().map(item => {
          const analysis = analysisMap.get(item.key);
          if (analysis) {
            return {
              ...item,
              ai_recommended: analysis.ai_recommended,
              reason: analysis.reason,
              tender_specific_points: analysis.tender_specific_points || [],
              mode: analysis.ai_recommended ? 'ai' as const : item.mode,
            };
          }
          return item;
        });

        this.allTypes.set(updated);
        this.groupByCategory(updated);

        if (result.ai_recommended_count > 0) {
          this.snackBar.open(
            `Analysis complete: ${result.ai_recommended_count} declaration(s) need AI customization`,
            'Close',
            { duration: 5000 }
          );
        } else {
          this.snackBar.open(
            'Analysis complete: all declarations can use standard templates',
            'Close',
            { duration: 4000 }
          );
        }
      },
      error: (error) => {
        console.error('Error analyzing tender:', error);
        this.analyzing.set(false);
        this.snackBar.open(error.error?.detail || 'Failed to analyze tender requirements', 'Close', { duration: 5000 });
      },
    });
  }

  // -----------------------------------------------------------------------
  // Selection toggles
  // -----------------------------------------------------------------------

  toggleAll(checked: boolean) {
    const updated = this.allTypes().map(t => ({ ...t, selected: checked }));
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }

  toggleCategory(categoryName: string, checked: boolean) {
    const updated = this.allTypes().map(t =>
      t.category === categoryName ? { ...t, selected: checked } : t
    );
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }

  toggleItem(key: string) {
    const updated = this.allTypes().map(t =>
      t.key === key ? { ...t, selected: !t.selected } : t
    );
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }

  toggleItemMode(key: string) {
    const updated = this.allTypes().map(t =>
      t.key === key ? { ...t, mode: t.mode === 'ai' ? 'standard' as const : 'ai' as const } : t
    );
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }

  isCategoryAllSelected(categoryName: string): boolean {
    const items = this.allTypes().filter(t => t.category === categoryName);
    return items.length > 0 && items.every(t => t.selected);
  }

  isCategoryPartiallySelected(categoryName: string): boolean {
    const items = this.allTypes().filter(t => t.category === categoryName);
    const count = items.filter(t => t.selected).length;
    return count > 0 && count < items.length;
  }

  // -----------------------------------------------------------------------
  // Generation
  // -----------------------------------------------------------------------

  onGenerateSelected() {
    const selected = this.allTypes().filter(t => t.selected);
    if (selected.length === 0) {
      this.snackBar.open('Please select at least one declaration', 'Close', { duration: 3000 });
      return;
    }

    this.generating.set(true);

    const aiTypes = selected.filter(t => t.mode === 'ai').map(t => t.key);
    const analysisResults = this.allTypes()
      .filter(t => t.ai_recommended)
      .map(t => ({
        key: t.key,
        ai_recommended: t.ai_recommended,
        reason: t.reason,
        tender_specific_points: t.tender_specific_points,
      }));

    const request: BulkDeclarationRequest = {
      declaration_types: selected.map(t => t.key),
      tender_id: this.tenderId() || undefined,
      signatory_name: this.signatoryName() || undefined,
      designation: this.designation() || undefined,
      ai_types: aiTypes.length > 0 ? aiTypes : undefined,
      analysis_results: analysisResults.length > 0 ? analysisResults : undefined,
      bidder_role: this.bidderRole(),
    };

    this.declarationService.generateBulk(request).subscribe({
      next: (blob) => {
        this.generating.set(false);
        this.downloadBlob(blob, 'declarations.zip');

        const aiCount = aiTypes.length;
        const stdCount = selected.length - aiCount;
        let msg = `${selected.length} declaration(s) generated`;
        if (aiCount > 0) {
          msg += ` (${stdCount} standard, ${aiCount} AI-customized)`;
        }
        this.snackBar.open(msg, 'Close', { duration: 5000 });

        const docs: GeneratedDoc[] = selected.map(item => ({
          key: item.key,
          name: item.name,
          filename: `${item.key.replace(/_/g, '-')}-declaration${item.mode === 'ai' ? '-custom' : ''}.docx`,
          mode: item.mode,
        }));
        this.generatedDocs.set(docs);
      },
      error: (error) => {
        console.error('Error generating declarations:', error);
        this.generating.set(false);
        this.snackBar.open(error.error?.detail || 'Failed to generate declarations', 'Close', { duration: 5000 });
      },
    });
  }

  onGenerateSingle(key: string) {
    const item = this.allTypes().find(t => t.key === key);
    if (!item) return;

    this.declarationService.generateSingle(
      key,
      this.tenderId() || undefined,
      this.signatoryName() || undefined,
      this.designation() || undefined,
      item.mode,
      item.tender_specific_points.length > 0 ? item.tender_specific_points : undefined,
      this.bidderRole(),
    ).subscribe({
      next: (blob) => {
        const suffix = item.mode === 'ai' ? '-custom' : '';
        const filename = `${key.replace(/_/g, '-')}-declaration${suffix}.docx`;
        this.downloadBlob(blob, filename);
        this.snackBar.open(
          `${item.name} downloaded (${item.mode === 'ai' ? 'AI-customized' : 'standard'})`,
          'Close',
          { duration: 3000 }
        );
      },
      error: (error) => {
        console.error('Error generating declaration:', error);
        this.snackBar.open(error.error?.detail || 'Failed to generate declaration', 'Close', { duration: 5000 });
      },
    });
  }

  private downloadBlob(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  }

  getCategoryIcon(category: string): string {
    const icons: Record<string, string> = {
      eligibility: 'verified_user',
      compliance: 'gavel',
      financial: 'account_balance',
      technical: 'precision_manufacturing',
    };
    return icons[category] || 'description';
  }

  loadKBSuggestions() {
    const id = this.tenderId();
    if (!id) return;
    this.kbService.getSuggestions({ category: 'declaration_text', tender_id: id, limit: 10 }).subscribe({
      next: (items) => this.kbSuggestions.set(items),
      error: () => {}
    });
  }

  onCopyKBContent(entry: KBEntry) {
    navigator.clipboard.writeText(entry.content).then(() => {
      this.snackBar.open('Copied to clipboard', 'Close', { duration: 2000 });
      if (this.tenderId()) {
        this.kbService.recordUsage(entry.id, this.tenderId(), 'declarations_tab').subscribe();
      }
    });
  }

  toggleKBSuggestions() {
    this.showKBSuggestions.set(!this.showKBSuggestions());
  }

  getKBPreview(content: string): string {
    return content.length > 120 ? content.substring(0, 120) + '...' : content;
  }

  // --- Auto-Fill from KB ---
  onAutoFill() {
    const id = this.tenderId();
    if (!id) {
      this.snackBar.open('No tender linked', 'Close', { duration: 3000 });
      return;
    }
    this.autoFillLoading.set(true);
    this.tenderService.getAutoFillSuggestions(id, 'declarations').subscribe({
      next: (suggestions) => {
        this.autoFillSuggestions.set(suggestions);
        this.autoFillLoading.set(false);
        if (suggestions.length === 0) {
          this.snackBar.open('No KB suggestions found for declarations', 'Close', { duration: 3000 });
        } else {
          this.snackBar.open(`${suggestions.length} auto-fill suggestion(s) found`, 'Close', { duration: 3000 });
        }
      },
      error: () => {
        this.autoFillLoading.set(false);
        this.snackBar.open('Failed to load auto-fill suggestions', 'Close', { duration: 3000 });
      }
    });
  }

  applyAutoFillSuggestion(suggestion: AutoFillSuggestion) {
    navigator.clipboard.writeText(suggestion.content).then(() => {
      this.snackBar.open(`"${suggestion.title}" copied to clipboard`, 'Close', { duration: 2000 });
    });
  }

  applyAllAutoFill() {
    const suggestions = this.autoFillSuggestions();
    const combined = suggestions.map(s => `--- ${s.title} ---\n${s.content}`).join('\n\n');
    navigator.clipboard.writeText(combined).then(() => {
      this.snackBar.open(`${suggestions.length} suggestions copied to clipboard`, 'Close', { duration: 3000 });
    });
  }

  dismissAutoFill() {
    this.autoFillSuggestions.set([]);
  }

  // --- Preview & Approve ---
  onGeneratePreview() {
    const selected = this.allTypes().filter(t => t.selected);
    if (selected.length === 0) {
      this.snackBar.open('Please select at least one declaration', 'Close', { duration: 3000 });
      return;
    }

    this.previewing.set(true);
    this.previews.set([]);
    this.saveResult.set(null);

    const aiTypes = selected.filter(t => t.mode === 'ai').map(t => t.key);
    const analysisResults = this.allTypes()
      .filter(t => t.ai_recommended)
      .map(t => ({
        key: t.key,
        ai_recommended: t.ai_recommended,
        reason: t.reason,
        tender_specific_points: t.tender_specific_points,
      }));

    const request: BulkDeclarationRequest = {
      declaration_types: selected.map(t => t.key),
      tender_id: this.tenderId() || undefined,
      signatory_name: this.signatoryName() || undefined,
      designation: this.designation() || undefined,
      ai_types: aiTypes.length > 0 ? aiTypes : undefined,
      analysis_results: analysisResults.length > 0 ? analysisResults : undefined,
      bidder_role: this.bidderRole(),
    };

    this.declarationService.previewBulk(request).subscribe({
      next: (response) => {
        this.previewing.set(false);
        const items: PreviewItem[] = response.previews.map(p => ({
          ...p,
          approved: false,
          expanded: true,
          regenerating: false,
        }));
        this.previews.set(items);
        this.snackBar.open(
          `${items.length} preview(s) generated — review and approve before saving`,
          'Close',
          { duration: 5000 }
        );
      },
      error: (error) => {
        console.error('Error generating previews:', error);
        this.previewing.set(false);
        this.snackBar.open(error.error?.detail || 'Failed to generate previews', 'Close', { duration: 5000 });
      },
    });
  }

  onRegenerateSingle(key: string) {
    const current = this.previews();
    const idx = current.findIndex(p => p.key === key);
    if (idx === -1) return;

    const item = this.allTypes().find(t => t.key === key);
    if (!item) return;

    // Mark as regenerating
    const updated = [...current];
    updated[idx] = { ...updated[idx], regenerating: true };
    this.previews.set(updated);

    this.declarationService.previewSingle(
      key,
      this.tenderId() || undefined,
      this.signatoryName() || undefined,
      this.designation() || undefined,
      item.mode,
      item.tender_specific_points.length > 0 ? item.tender_specific_points : undefined,
      this.bidderRole(),
    ).subscribe({
      next: (preview) => {
        const latest = [...this.previews()];
        const i = latest.findIndex(p => p.key === key);
        if (i !== -1) {
          latest[i] = {
            ...preview,
            approved: false,
            expanded: true,
            regenerating: false,
          };
          this.previews.set(latest);
        }
        this.snackBar.open(`${preview.name} regenerated`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error regenerating preview:', error);
        const latest = [...this.previews()];
        const i = latest.findIndex(p => p.key === key);
        if (i !== -1) {
          latest[i] = { ...latest[i], regenerating: false };
          this.previews.set(latest);
        }
        this.snackBar.open('Failed to regenerate', 'Close', { duration: 3000 });
      },
    });
  }

  toggleApproval(key: string) {
    const updated = this.previews().map(p =>
      p.key === key ? { ...p, approved: !p.approved } : p
    );
    this.previews.set(updated);
  }

  approveAll() {
    const updated = this.previews().map(p => ({ ...p, approved: true }));
    this.previews.set(updated);
  }

  rejectAll() {
    const updated = this.previews().map(p => ({ ...p, approved: false }));
    this.previews.set(updated);
  }

  toggleExpand(key: string) {
    const updated = this.previews().map(p =>
      p.key === key ? { ...p, expanded: !p.expanded } : p
    );
    this.previews.set(updated);
  }

  // --- Save & Link to Checklist ---
  onSaveAndLink() {
    const tid = this.tenderId();
    if (!tid) {
      this.snackBar.open('No tender linked — cannot save declarations', 'Close', { duration: 3000 });
      return;
    }

    // If previews exist, only save approved ones
    const currentPreviews = this.previews();
    let typesToSave: string[];
    if (currentPreviews.length > 0) {
      const approved = currentPreviews.filter(p => p.approved);
      if (approved.length === 0) {
        this.snackBar.open('No declarations approved — approve at least one to save', 'Close', { duration: 3000 });
        return;
      }
      typesToSave = approved.map(p => p.key);
    } else {
      const selected = this.allTypes().filter(t => t.selected);
      if (selected.length === 0) {
        this.snackBar.open('No declarations selected', 'Close', { duration: 3000 });
        return;
      }
      typesToSave = selected.map(t => t.key);
    }

    this.saving.set(true);

    const aiTypes = this.allTypes().filter(t => t.mode === 'ai' && typesToSave.includes(t.key)).map(t => t.key);
    const analysisResults = this.allTypes()
      .filter(t => t.ai_recommended)
      .map(t => ({
        key: t.key,
        ai_recommended: t.ai_recommended,
        reason: t.reason,
        tender_specific_points: t.tender_specific_points,
      }));

    const request: SaveAndLinkRequest = {
      tender_id: tid,
      declaration_types: typesToSave,
      signatory_name: this.signatoryName() || undefined,
      designation: this.designation() || undefined,
      ai_types: aiTypes.length > 0 ? aiTypes : undefined,
      analysis_results: analysisResults.length > 0 ? analysisResults : undefined,
      bidder_role: this.bidderRole(),
    };

    this.declarationService.saveAndLink(request).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.saveResult.set({ saved: result.saved, linked: result.linked });
        if (result.linked === 0 && result.saved > 0) {
          this.snackBar.open(
            `${result.saved} declaration(s) saved. No checklist items matched — generate submission checklist first.`,
            'Close',
            { duration: 6000 }
          );
        } else {
          this.snackBar.open(
            `${result.saved} saved, ${result.linked} linked to checklist`,
            'Close',
            { duration: 5000 }
          );
        }
      },
      error: (error) => {
        console.error('Error saving declarations:', error);
        this.saving.set(false);
        this.snackBar.open(error.error?.detail || 'Failed to save declarations', 'Close', { duration: 5000 });
      },
    });
  }

  // --- Per-Item Preview ---
  onPreviewSingle(key: string) {
    const item = this.allTypes().find(t => t.key === key);
    if (!item) return;

    // Update item to show loading
    const updated = this.allTypes().map(t =>
      t.key === key ? { ...t, previewing: true } : t
    );
    this.allTypes.set(updated);
    this.groupByCategory(updated);

    this.declarationService.previewSingle(
      key,
      this.tenderId() || undefined,
      this.signatoryName() || undefined,
      this.designation() || undefined,
      item.mode,
      item.tender_specific_points.length > 0 ? item.tender_specific_points : undefined,
      this.bidderRole(),
    ).subscribe({
      next: (preview) => {
        const latest = this.allTypes().map(t =>
          t.key === key ? { ...t, previewing: false, previewContent: preview.content, previewExpanded: true } : t
        );
        this.allTypes.set(latest);
        this.groupByCategory(latest);
      },
      error: (error) => {
        console.error('Error previewing declaration:', error);
        const latest = this.allTypes().map(t =>
          t.key === key ? { ...t, previewing: false } : t
        );
        this.allTypes.set(latest);
        this.groupByCategory(latest);
        const detail = error.error?.detail || (typeof error.error === 'string' ? error.error : null);
        this.snackBar.open(detail || `Failed to generate preview (HTTP ${error.status})`, 'Close', { duration: 5000 });
      },
    });
  }

  toggleItemPreview(key: string) {
    const updated = this.allTypes().map(t =>
      t.key === key ? { ...t, previewExpanded: !t.previewExpanded } : t
    );
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }

  // --- Analysis Caching ---
  private cacheKey(tenderId: string): string {
    return `decl_analysis_${tenderId}`;
  }

  private loadCachedAnalysis(tenderId: string): DeclarationAnalysisItem[] | null {
    try {
      const raw = sessionStorage.getItem(this.cacheKey(tenderId));
      if (!raw) return null;
      return JSON.parse(raw) as DeclarationAnalysisItem[];
    } catch {
      return null;
    }
  }

  private saveCachedAnalysis(tenderId: string, analysis: DeclarationAnalysisItem[]) {
    try {
      sessionStorage.setItem(this.cacheKey(tenderId), JSON.stringify(analysis));
    } catch {
      // sessionStorage quota exceeded — ignore
    }
  }

  private applyCachedAnalysis(analysis: DeclarationAnalysisItem[]) {
    this.analyzed.set(true);
    const analysisMap = new Map<string, DeclarationAnalysisItem>();
    for (const a of analysis) {
      analysisMap.set(a.key, a);
    }
    const updated = this.allTypes().map(item => {
      const a = analysisMap.get(item.key);
      if (a) {
        return {
          ...item,
          ai_recommended: a.ai_recommended,
          reason: a.reason,
          tender_specific_points: a.tender_specific_points || [],
          mode: a.ai_recommended ? 'ai' as const : item.mode,
        };
      }
      return item;
    });
    this.allTypes.set(updated);
    this.groupByCategory(updated);
  }
}
