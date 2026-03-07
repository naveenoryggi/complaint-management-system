import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import {
  TenderService,
  ComplianceCheckResponse,
  ComplianceCategory,
  ComplianceCheckItem,
  AlignmentCheckResponse,
  AlignmentIssue,
  AutoFillSuggestion,
  ClauseInterpretationResult,
  RiskSummary,
  TechnicalComplianceItem,
} from '../../../services/tender.service';
import { KnowledgeBaseService, KBEntry } from '../../../services/knowledge-base.service';

@Component({
  selector: 'app-compliance-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
  ],
  templateUrl: './compliance-tab.component.html',
  styleUrls: ['./compliance-tab.component.css']
})
export class ComplianceTabComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);
  private kbService = inject(KnowledgeBaseService);

  compliance = signal<ComplianceCheckResponse | null>(null);
  loading = signal(false);

  kbSuggestions = signal<KBEntry[]>([]);
  showKBSuggestions = signal(false);
  kbSuggestionsLoading = signal(false);
  autoFillSuggestions = signal<AutoFillSuggestion[]>([]);
  autoFillLoading = signal(false);

  // Alignment verification
  alignmentResult = signal<AlignmentCheckResponse | null>(null);
  alignmentLoading = signal(false);

  // Clause interpretation
  interpretationResult = signal<ClauseInterpretationResult | null>(null);
  interpretationLoading = signal(false);
  riskSummary = signal<RiskSummary | null>(null);

  // Map categories to workspace tab indices for navigation
  categoryTabMap: { [key: string]: number } = {
    'documents': 1,
    'emd_fees': 5,
    'oem': 4,
    'declarations': 6,
    'criteria': 3,
    'submission_checklist': 8,
  };

  ngOnInit() {
    if (this.tenderId()) {
      this.loadCompliance();
      this.loadKBSuggestions();
      this.loadMatrix();
    }
  }

  loadCompliance() {
    const id = this.tenderId();
    if (!id) return;

    this.loading.set(true);
    this.tenderService.getComplianceCheck(id).subscribe({
      next: (data) => {
        this.compliance.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading compliance:', error);
        this.snackBar.open('Failed to load compliance check', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onRefresh() {
    this.loadCompliance();
  }

  getOverallColor(score: number): string {
    if (score >= 80) return 'primary';
    if (score >= 50) return 'accent';
    return 'warn';
  }

  getProgressBarClass(score: number): string {
    if (score >= 80) return 'progress-green';
    if (score >= 50) return 'progress-amber';
    return 'progress-red';
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'ready': return 'check_circle';
      case 'missing': return 'cancel';
      case 'partial': return 'warning';
      case 'na': return 'remove_circle_outline';
      default: return 'help';
    }
  }

  getStatusClass(status: string): string {
    return `status-${status}`;
  }

  hasMissingItems(category: ComplianceCategory): boolean {
    return category.items.some(i => i.status === 'missing' || i.status === 'partial');
  }

  getTabIndex(category: string): number | null {
    return this.categoryTabMap[category] ?? null;
  }

  loadKBSuggestions() {
    const id = this.tenderId();
    if (!id) return;
    this.kbSuggestionsLoading.set(true);
    this.kbService.getSuggestions({ category: 'technical_response', tender_id: id, limit: 10 }).subscribe({
      next: (items) => { this.kbSuggestions.set(items); this.kbSuggestionsLoading.set(false); },
      error: () => { this.kbSuggestionsLoading.set(false); }
    });
  }

  onCopyKBContent(entry: KBEntry) {
    navigator.clipboard.writeText(entry.content).then(() => {
      this.snackBar.open('Copied to clipboard', 'Close', { duration: 2000 });
      if (this.tenderId()) {
        this.kbService.recordUsage(entry.id, this.tenderId(), 'compliance_tab').subscribe();
      }
    });
  }

  toggleKBSuggestions() {
    this.showKBSuggestions.set(!this.showKBSuggestions());
  }

  getKBPreview(content: string): string {
    return content.length > 120 ? content.substring(0, 120) + '...' : content;
  }

  // --- Alignment Verification ---
  onRunAlignmentCheck() {
    const id = this.tenderId();
    if (!id) return;
    this.alignmentLoading.set(true);
    this.tenderService.runAlignmentCheck(id).subscribe({
      next: (result) => {
        this.alignmentResult.set(result);
        this.alignmentLoading.set(false);
        this.snackBar.open(`Alignment check complete: ${result.alignment_score}%`, 'Close', { duration: 4000 });
      },
      error: (err) => {
        this.alignmentLoading.set(false);
        this.snackBar.open('Alignment check failed', 'Close', { duration: 3000 });
      }
    });
  }

  getAlignmentScoreClass(score: number): string {
    if (score >= 80) return 'alignment-green';
    if (score >= 50) return 'alignment-amber';
    return 'alignment-red';
  }

  getAlignmentLabelClass(label: string): string {
    if (label === 'Fully Aligned') return 'alignment-green';
    if (label === 'Mostly Aligned') return 'alignment-amber';
    return 'alignment-red';
  }

  getSeverityColor(severity: string): string {
    switch (severity) {
      case 'critical': return '#f44336';
      case 'warning': return '#ff9800';
      case 'info': return '#2196f3';
      default: return '#9e9e9e';
    }
  }

  getSeverityIcon(severity: string): string {
    switch (severity) {
      case 'critical': return 'error';
      case 'warning': return 'warning';
      case 'info': return 'info';
      default: return 'help';
    }
  }

  // --- Auto-Fill from KB ---
  onAutoFill() {
    const id = this.tenderId();
    if (!id) return;
    this.autoFillLoading.set(true);
    this.tenderService.getAutoFillSuggestions(id, 'compliance').subscribe({
      next: (suggestions) => {
        this.autoFillSuggestions.set(suggestions);
        this.autoFillLoading.set(false);
        if (suggestions.length === 0) {
          this.snackBar.open('No KB suggestions found for compliance', 'Close', { duration: 3000 });
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

  getAutoFillPreview(content: string): string {
    return content.length > 120 ? content.substring(0, 120) + '...' : content;
  }

  // --- Clause Interpretation ---
  onInterpretClauses() {
    const id = this.tenderId();
    if (!id) return;
    this.interpretationLoading.set(true);
    this.tenderService.interpretClauses(id).subscribe({
      next: (result) => {
        this.interpretationResult.set(result);
        this.interpretationLoading.set(false);
        this.snackBar.open(
          `${result.interpreted} clause(s) interpreted. ${result.risk_distribution.high_risk_count} high-risk.`,
          'Close', { duration: 5000 }
        );
      },
      error: (err) => {
        this.interpretationLoading.set(false);
        this.snackBar.open(err.error?.detail || 'Clause interpretation failed', 'Close', { duration: 5000 });
      }
    });
  }

  onLoadRiskSummary() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.getRiskSummary(id).subscribe({
      next: (summary) => this.riskSummary.set(summary),
      error: () => {}
    });
  }

  getRiskColor(category: string): string {
    switch (category) {
      case 'critical': return '#d32f2f';
      case 'high': return '#f44336';
      case 'medium': return '#ff9800';
      case 'low': return '#4caf50';
      default: return '#9e9e9e';
    }
  }

  getRiskIcon(category: string): string {
    switch (category) {
      case 'critical': return 'dangerous';
      case 'high': return 'warning';
      case 'medium': return 'info';
      case 'low': return 'check_circle';
      default: return 'help';
    }
  }

  // --- Compliance Matrix CRUD ---
  matrixItems = signal<TechnicalComplianceItem[]>([]);
  matrixLoading = signal(false);
  matrixSummary = signal<any>(null);
  showAddItem = signal(false);
  newItem = signal<Partial<TechnicalComplianceItem>>({
    clause_number: '', clause_title: '', compliance_status: 'pending', is_mandatory: false, is_critical: false,
  });
  editingItemId = signal<string | null>(null);

  readonly complianceStatuses = [
    { value: 'complied', label: 'Complied', icon: 'check_circle', color: '#057a55' },
    { value: 'not_complied', label: 'Not Complied', icon: 'cancel', color: '#dc2626' },
    { value: 'partially_complied', label: 'Partially Complied', icon: 'warning', color: '#ea580c' },
    { value: 'pending', label: 'Pending', icon: 'schedule', color: '#6b7280' },
    { value: 'na', label: 'N/A', icon: 'remove_circle_outline', color: '#9e9e9e' },
  ];

  loadMatrix() {
    const id = this.tenderId();
    if (!id) return;
    this.matrixLoading.set(true);
    this.tenderService.getComplianceMatrix(id).subscribe({
      next: (items) => { this.matrixItems.set(items); this.matrixLoading.set(false); this.loadMatrixSummary(); },
      error: () => { this.matrixLoading.set(false); }
    });
  }

  loadMatrixSummary() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.getComplianceMatrixSummary(id).subscribe({
      next: (summary) => this.matrixSummary.set(summary),
      error: () => {}
    });
  }

  updateNewItemField(field: string, value: any) {
    this.newItem.set({ ...this.newItem(), [field]: value });
  }

  onAddMatrixItem() {
    const id = this.tenderId();
    const data = this.newItem();
    if (!id || !data.clause_number || !data.clause_title) return;
    this.tenderService.createComplianceItem(id, data).subscribe({
      next: (created) => {
        this.matrixItems.set([...this.matrixItems(), created]);
        this.showAddItem.set(false);
        this.newItem.set({ clause_number: '', clause_title: '', compliance_status: 'pending', is_mandatory: false, is_critical: false });
        this.snackBar.open('Compliance item added', 'Close', { duration: 3000 });
        this.loadMatrixSummary();
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to add item', 'Close', { duration: 3000 })
    });
  }

  onUpdateStatus(item: TechnicalComplianceItem, newStatus: string) {
    this.tenderService.updateComplianceItem(item.id, { compliance_status: newStatus as any }).subscribe({
      next: (updated) => {
        this.matrixItems.set(this.matrixItems().map(i => i.id === item.id ? updated : i));
        this.loadMatrixSummary();
      },
      error: () => this.snackBar.open('Failed to update status', 'Close', { duration: 3000 })
    });
  }

  onDeleteMatrixItem(item: TechnicalComplianceItem) {
    if (!confirm(`Delete clause "${item.clause_number}"?`)) return;
    this.tenderService.deleteComplianceItem(item.id).subscribe({
      next: () => {
        this.matrixItems.set(this.matrixItems().filter(i => i.id !== item.id));
        this.snackBar.open('Item deleted', 'Close', { duration: 3000 });
        this.loadMatrixSummary();
      },
      error: () => this.snackBar.open('Failed to delete', 'Close', { duration: 3000 })
    });
  }

  getStatusConfig(status: string) {
    return this.complianceStatuses.find(s => s.value === status) || this.complianceStatuses[3];
  }
}
