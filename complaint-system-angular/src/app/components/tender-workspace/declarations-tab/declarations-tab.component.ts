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
import {
  DeclarationService,
  DeclarationType,
  DeclarationAnalysisItem,
  BulkDeclarationRequest,
} from '../../../services/declaration.service';

interface DeclarationItem extends DeclarationType {
  selected: boolean;
  mode: 'standard' | 'ai';       // generation mode
  ai_recommended: boolean;        // Claude flagged this
  reason: string | null;          // why AI is recommended
  tender_specific_points: string[]; // specific tender points
}

interface CategoryGroup {
  name: string;
  label: string;
  items: DeclarationItem[];
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
  ],
  templateUrl: './declarations-tab.component.html',
  styleUrls: ['./declarations-tab.component.css'],
})
export class DeclarationsTabComponent implements OnInit {
  tenderId = input<string>('');

  private declarationService = inject(DeclarationService);
  private snackBar = inject(MatSnackBar);

  loading = signal(false);
  analyzing = signal(false);
  generating = signal(false);
  analyzed = signal(false);
  categories = signal<CategoryGroup[]>([]);
  allTypes = signal<DeclarationItem[]>([]);
  signatoryName = signal('');
  designation = signal('');
  generatedDocs = signal<GeneratedDoc[]>([]);

  selectedCount = computed(() => this.allTypes().filter(t => t.selected).length);
  aiSelectedCount = computed(() => this.allTypes().filter(t => t.selected && t.mode === 'ai').length);
  aiRecommendedCount = computed(() => this.allTypes().filter(t => t.ai_recommended).length);
  allSelected = computed(() => {
    const types = this.allTypes();
    return types.length > 0 && types.every(t => t.selected);
  });

  private categoryLabels: Record<string, string> = {
    eligibility: 'Eligibility',
    compliance: 'Compliance',
    financial: 'Financial',
    technical: 'Technical',
  };

  ngOnInit() {
    this.loadTypes();
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
        }));
        this.allTypes.set(items);
        this.groupByCategory(items);
        this.loading.set(false);

        // Auto-analyze if we have a tender ID
        if (this.tenderId()) {
          this.onAnalyzeTender();
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
        this.snackBar.open('Failed to analyze tender requirements', 'Close', { duration: 3000 });
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
        this.snackBar.open('Failed to generate declarations', 'Close', { duration: 3000 });
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
        this.snackBar.open('Failed to generate declaration', 'Close', { duration: 3000 });
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
}
