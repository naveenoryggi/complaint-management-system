import { Component, OnInit, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  TenderService,
  TechnicalComplianceItem,
  BoQLineItem,
  BoQSummary,
  TenderMilestone,
  TenderCorrigendum,
  EligibilityResult,
  QCBSResult,
  AutoFillSuggestion,
  BidCostEstimate,
  SolutionGenerationStatus,
  SolutionSectionsResponse,
  ProposalSection,
  SolutionGap,
  SolutionEvaluation,
  DiagramSuggestion,
} from '../../../services/tender.service';
import { KnowledgeBaseService, KBEntry } from '../../../services/knowledge-base.service';

@Component({
  selector: 'app-bid-preparation-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './bid-preparation-tab.component.html',
  styleUrls: ['./bid-preparation-tab.component.css']
})
export class BidPreparationTabComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);
  private kbService = inject(KnowledgeBaseService);

  // Compliance Matrix
  complianceItems = signal<TechnicalComplianceItem[]>([]);
  complianceLoading = signal(false);
  complianceSummary = signal<any>(null);

  // BoQ
  boqItems = signal<BoQLineItem[]>([]);
  boqLoading = signal(false);
  boqSummary = signal<BoQSummary | null>(null);

  // Milestones
  milestones = signal<TenderMilestone[]>([]);
  milestonesLoading = signal(false);

  // Corrigendums
  corrigendums = signal<TenderCorrigendum[]>([]);
  corrigendumLoading = signal(false);

  // Eligibility
  eligibility = signal<EligibilityResult | null>(null);
  eligibilityLoading = signal(false);

  // QCBS
  qcbsResult = signal<QCBSResult | null>(null);
  qcbsParams = {
    technical_weight: 70,
    financial_weight: 30,
    our_technical_score: 0,
    our_financial_bid: 0,
    lowest_financial_bid: 0,
  };

  // New item forms
  showNewCompliance = signal(false);
  showNewBoQ = signal(false);
  showNewMilestone = signal(false);
  showNewCorrigendum = signal(false);

  newCompliance: Partial<TechnicalComplianceItem> = { clause_number: '', clause_title: '', compliance_status: 'pending', is_mandatory: true, is_critical: false };
  newBoQ: Partial<BoQLineItem> = { item_number: '', description: '', unit: 'Nos', quantity: 1, unit_rate: 0, gst_percentage: 18 };
  newMilestone: Partial<TenderMilestone> = { milestone_type: 'bid_submission', label: '', event_date: '' };
  newCorrigendum: Partial<TenderCorrigendum> = { corrigendum_number: 1, title: '' };

  // KB Suggestions
  kbTechnicalSuggestions = signal<KBEntry[]>([]);
  kbPricingSuggestions = signal<KBEntry[]>([]);
  showKBTechnical = signal(false);
  showKBPricing = signal(false);

  // Auto-Fill
  autoFillSuggestions = signal<AutoFillSuggestion[]>([]);
  autoFillLoading = signal(false);

  // Bid Cost Estimator
  bidCostEstimate = signal<BidCostEstimate | null>(null);
  bidCostLoading = signal(false);

  complianceColumns = ['clause_number', 'clause_title', 'section', 'status', 'page_ref', 'actions'];
  boqColumns = ['item_number', 'description', 'unit', 'quantity', 'unit_rate', 'gst', 'total', 'actions'];

  milestoneTypes = [
    { value: 'publish_date', label: 'Publish Date' },
    { value: 'prebid_meeting', label: 'Pre-Bid Meeting' },
    { value: 'corrigendum_deadline', label: 'Corrigendum Deadline' },
    { value: 'bid_submission', label: 'Bid Submission' },
    { value: 'technical_opening', label: 'Technical Opening' },
    { value: 'financial_opening', label: 'Financial Opening' },
    { value: 'presentation', label: 'Presentation' },
    { value: 'award_date', label: 'Award Date' },
    { value: 'contract_signing', label: 'Contract Signing' },
    { value: 'custom', label: 'Custom' }
  ];

  complianceStatuses = [
    { value: 'complied', label: 'Complied', icon: 'check_circle', color: '#4caf50' },
    { value: 'not_complied', label: 'Not Complied', icon: 'cancel', color: '#f44336' },
    { value: 'partially_complied', label: 'Partially', icon: 'warning', color: '#ff9800' },
    { value: 'pending', label: 'Pending', icon: 'hourglass_empty', color: '#9e9e9e' },
    { value: 'na', label: 'N/A', icon: 'remove_circle_outline', color: '#757575' },
  ];

  ngOnInit() {
    if (this.tenderId()) {
      this.loadComplianceMatrix();
      this.loadBoQ();
      this.loadMilestones();
      this.loadCorrigendums();
      this.loadKBSuggestions();
      this.loadSolutionStatus();
    }
  }

  // --- Compliance Matrix ---
  loadComplianceMatrix() {
    this.complianceLoading.set(true);
    this.tenderService.getComplianceMatrix(this.tenderId()).subscribe({
      next: (items) => {
        this.complianceItems.set(items);
        this.complianceLoading.set(false);
        this.loadComplianceSummary();
      },
      error: (err) => { console.error('Error loading compliance matrix:', err); this.complianceLoading.set(false); }
    });
  }

  loadComplianceSummary() {
    this.tenderService.getComplianceMatrixSummary(this.tenderId()).subscribe({
      next: (s) => this.complianceSummary.set(s),
      error: (err) => { console.error('Error loading compliance summary:', err); }
    });
  }

  addComplianceItem() {
    this.tenderService.createComplianceItem(this.tenderId(), this.newCompliance).subscribe({
      next: () => {
        this.showNewCompliance.set(false);
        this.newCompliance = { clause_number: '', clause_title: '', compliance_status: 'pending', is_mandatory: true, is_critical: false };
        this.loadComplianceMatrix();
        this.snackBar.open('Clause added', 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to add clause', 'Close', { duration: 5000 })
    });
  }

  updateComplianceStatus(item: TechnicalComplianceItem, newStatus: string) {
    this.tenderService.updateComplianceItem(item.id, { compliance_status: newStatus as any }).subscribe({
      next: () => this.loadComplianceMatrix(),
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to update', 'Close', { duration: 5000 })
    });
  }

  deleteComplianceItem(item: TechnicalComplianceItem) {
    if (confirm(`Remove clause "${item.clause_number}"?`)) {
      this.tenderService.deleteComplianceItem(item.id).subscribe({
        next: () => this.loadComplianceMatrix(),
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 })
      });
    }
  }

  getStatusConfig(status: string) {
    return this.complianceStatuses.find(s => s.value === status) || this.complianceStatuses[3];
  }

  // --- BoQ ---
  loadBoQ() {
    this.boqLoading.set(true);
    this.tenderService.getBoQItems(this.tenderId()).subscribe({
      next: (items) => {
        this.boqItems.set(items);
        this.boqLoading.set(false);
        this.loadBoQSummary();
      },
      error: (err) => { console.error('Error loading BoQ items:', err); this.boqLoading.set(false); }
    });
  }

  loadBoQSummary() {
    this.tenderService.getBoQSummary(this.tenderId()).subscribe({
      next: (s) => this.boqSummary.set(s),
      error: (err) => { console.error('Error loading BoQ summary:', err); }
    });
  }

  addBoQItem() {
    this.tenderService.createBoQItem(this.tenderId(), this.newBoQ).subscribe({
      next: () => {
        this.showNewBoQ.set(false);
        this.newBoQ = { item_number: '', description: '', unit: 'Nos', quantity: 1, unit_rate: 0, gst_percentage: 18 };
        this.loadBoQ();
        this.snackBar.open('BoQ item added', 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to add item', 'Close', { duration: 5000 })
    });
  }

  deleteBoQItem(item: BoQLineItem) {
    if (confirm(`Remove "${item.description}"?`)) {
      this.tenderService.deleteBoQItem(item.id).subscribe({
        next: () => this.loadBoQ(),
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 })
      });
    }
  }

  // --- Milestones ---
  loadMilestones() {
    this.milestonesLoading.set(true);
    this.tenderService.getMilestones(this.tenderId()).subscribe({
      next: (m) => { this.milestones.set(m); this.milestonesLoading.set(false); },
      error: (err) => { console.error('Error loading milestones:', err); this.milestonesLoading.set(false); }
    });
  }

  addMilestone() {
    this.tenderService.createMilestone(this.tenderId(), this.newMilestone).subscribe({
      next: () => {
        this.showNewMilestone.set(false);
        this.newMilestone = { milestone_type: 'bid_submission', label: '', event_date: '' };
        this.loadMilestones();
        this.snackBar.open('Milestone added', 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to add milestone', 'Close', { duration: 5000 })
    });
  }

  toggleMilestoneComplete(m: TenderMilestone) {
    this.tenderService.updateMilestone(m.id, { is_completed: !m.is_completed }).subscribe({
      next: () => this.loadMilestones(),
      error: (err) => { console.error('Error toggling milestone:', err); }
    });
  }

  deleteMilestone(m: TenderMilestone) {
    if (confirm(`Remove "${m.label}"?`)) {
      this.tenderService.deleteMilestone(m.id).subscribe({
        next: () => this.loadMilestones(),
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 })
      });
    }
  }

  getMilestoneTypeLabel(type: string): string {
    return this.milestoneTypes.find(t => t.value === type)?.label || type;
  }

  getMilestoneDaysLeft(date: string | undefined): string {
    if (!date) return '';
    const d = new Date(date);
    const now = new Date();
    const diff = Math.ceil((d.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    if (diff < 0) return `${Math.abs(diff)}d overdue`;
    if (diff === 0) return 'Today';
    if (diff === 1) return 'Tomorrow';
    return `${diff}d left`;
  }

  isMilestoneUrgent(date: string | undefined): boolean {
    if (!date) return false;
    const diff = Math.ceil((new Date(date).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
    return diff <= 3 && diff >= 0;
  }

  isMilestoneOverdue(date: string | undefined): boolean {
    if (!date) return false;
    return new Date(date).getTime() < Date.now();
  }

  // --- Corrigendums ---
  loadCorrigendums() {
    this.corrigendumLoading.set(true);
    this.tenderService.getCorrigendums(this.tenderId()).subscribe({
      next: (c) => { this.corrigendums.set(c); this.corrigendumLoading.set(false); },
      error: (err) => { console.error('Error loading corrigendums:', err); this.corrigendumLoading.set(false); }
    });
  }

  addCorrigendum() {
    this.tenderService.createCorrigendum(this.tenderId(), this.newCorrigendum).subscribe({
      next: () => {
        this.showNewCorrigendum.set(false);
        const nextNum = this.corrigendums().length + 2;
        this.newCorrigendum = { corrigendum_number: nextNum, title: '' };
        this.loadCorrigendums();
        this.snackBar.open('Corrigendum added', 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to add corrigendum', 'Close', { duration: 5000 })
    });
  }

  acknowledgeCorrigendum(c: TenderCorrigendum) {
    this.tenderService.updateCorrigendum(c.id, { is_acknowledged: true }).subscribe({
      next: () => this.loadCorrigendums(),
      error: (err) => { console.error('Error acknowledging corrigendum:', err); }
    });
  }

  deleteCorrigendum(c: TenderCorrigendum) {
    if (confirm(`Remove Corrigendum #${c.corrigendum_number}?`)) {
      this.tenderService.deleteCorrigendum(c.id).subscribe({
        next: () => this.loadCorrigendums(),
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 })
      });
    }
  }

  // --- Eligibility ---
  runEligibilityCheck() {
    this.eligibilityLoading.set(true);
    this.tenderService.checkEligibility(this.tenderId()).subscribe({
      next: (r) => { this.eligibility.set(r); this.eligibilityLoading.set(false); },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to run eligibility check', 'Close', { duration: 5000 });
        this.eligibilityLoading.set(false);
      }
    });
  }

  // --- QCBS ---
  calculateQCBS() {
    this.tenderService.calculateQCBS(this.tenderId(), this.qcbsParams).subscribe({
      next: (r) => this.qcbsResult.set(r),
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to calculate QCBS', 'Close', { duration: 5000 })
    });
  }

  onQCBSWeightChange() {
    this.qcbsParams.financial_weight = 100 - this.qcbsParams.technical_weight;
  }

  formatINR(amount: number | undefined | null): string {
    if (!amount) return '-';
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
  }

  // --- KB Suggestions ---
  loadKBSuggestions() {
    const id = this.tenderId();
    if (!id) return;
    this.kbService.getSuggestions({ category: 'technical_response', tender_id: id, limit: 10 }).subscribe({
      next: (items) => this.kbTechnicalSuggestions.set(items),
      error: (err) => { console.error('Error loading technical KB suggestions:', err); }
    });
    this.kbService.getSuggestions({ category: 'pricing_data', tender_id: id, limit: 10 }).subscribe({
      next: (items) => this.kbPricingSuggestions.set(items),
      error: (err) => { console.error('Error loading pricing KB suggestions:', err); }
    });
  }

  onCopyKBContent(entry: KBEntry) {
    navigator.clipboard.writeText(entry.content).then(() => {
      this.snackBar.open('Copied to clipboard', 'Close', { duration: 2000 });
      if (this.tenderId()) {
        this.kbService.recordUsage(entry.id, this.tenderId(), 'bid_preparation_tab').subscribe();
      }
    });
  }

  getKBPreview(content: string): string {
    return content.length > 120 ? content.substring(0, 120) + '...' : content;
  }

  // --- Auto-Fill from KB ---
  onAutoFill() {
    const id = this.tenderId();
    if (!id) return;
    this.autoFillLoading.set(true);
    this.tenderService.getAutoFillSuggestions(id, 'bid_preparation').subscribe({
      next: (suggestions) => {
        this.autoFillSuggestions.set(suggestions);
        this.autoFillLoading.set(false);
        if (suggestions.length === 0) {
          this.snackBar.open('No KB suggestions found', 'Close', { duration: 3000 });
        }
      },
      error: (err) => {
        this.autoFillLoading.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to load auto-fill suggestions', 'Close', { duration: 5000 });
      }
    });
  }

  applyAutoFillSuggestion(suggestion: AutoFillSuggestion) {
    navigator.clipboard.writeText(suggestion.content).then(() => {
      this.snackBar.open(`"${suggestion.title}" copied to clipboard`, 'Close', { duration: 2000 });
    });
  }

  applyAllAutoFill() {
    const combined = this.autoFillSuggestions().map(s => `--- ${s.title} ---\n${s.content}`).join('\n\n');
    navigator.clipboard.writeText(combined).then(() => {
      this.snackBar.open(`${this.autoFillSuggestions().length} suggestions copied`, 'Close', { duration: 3000 });
    });
  }

  dismissAutoFill() {
    this.autoFillSuggestions.set([]);
  }

  // --- Bid Cost Estimator ---
  onEstimateBidCost() {
    const id = this.tenderId();
    if (!id) return;
    this.bidCostLoading.set(true);
    this.tenderService.estimateBidCost(id).subscribe({
      next: (estimate) => {
        this.bidCostEstimate.set(estimate);
        this.bidCostLoading.set(false);
      },
      error: (err) => {
        this.bidCostLoading.set(false);
        this.snackBar.open(err.error?.detail || 'Failed to estimate bid cost', 'Close', { duration: 5000 });
      }
    });
  }

  // --- Solution Document Generator ---
  solutionStatus = signal<SolutionGenerationStatus | null>(null);
  solutionSections = signal<ProposalSection[]>([]);
  solutionGenerating = signal(false);
  solutionLoading = signal(false);
  solutionExpandedSection = signal<string | null>(null);
  solutionRegenerating = signal<string | null>(null);
  solutionCustomPrompt = '';

  loadSolutionStatus() {
    const id = this.tenderId();
    if (!id) return;
    this.solutionLoading.set(true);
    this.tenderService.getSolutionStatus(id).subscribe({
      next: (status) => {
        this.solutionStatus.set(status);
        this.solutionLoading.set(false);
        if (status.status === 'completed') {
          this.loadSolutionSections();
        }
      },
      error: (err) => { console.error('Error loading solution status:', err); this.solutionLoading.set(false); }
    });
  }

  loadSolutionSections() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.getSolutionSections(id).subscribe({
      next: (resp) => this.solutionSections.set(resp.sections),
      error: (err) => { console.error('Error loading solution sections:', err); }
    });
  }

  onGenerateSolution(regenerate: boolean = false) {
    const id = this.tenderId();
    if (!id) return;
    this.solutionGenerating.set(true);
    this.solutionStatus.set({
      status: 'started',
      current_step: 'Initializing pipeline...',
      total_steps: 8,
      completed_steps: 0,
      progress_percentage: 0,
    });
    this.tenderService.generateSolution(id, { regenerate }).subscribe({
      next: (result) => {
        this.solutionGenerating.set(false);
        this.loadSolutionStatus();
        this.snackBar.open(
          `Proposal generated: ${result.total_sections} sections, ${result.coverage_percentage?.toFixed(0)}% coverage`,
          'Close', { duration: 5000 }
        );
      },
      error: (err) => {
        this.solutionGenerating.set(false);
        this.solutionStatus.set({
          status: 'failed',
          total_steps: 8,
          completed_steps: 0,
          progress_percentage: 0,
          error_message: err.error?.detail || 'Generation failed',
        });
        this.snackBar.open(err.error?.detail || 'Failed to generate solution document', 'Close', { duration: 5000 });
      }
    });
  }

  onToggleSection(sectionId: string) {
    const current = this.solutionExpandedSection();
    this.solutionExpandedSection.set(current === sectionId ? null : sectionId);
  }

  onRegenerateSection(section: ProposalSection) {
    const id = this.tenderId();
    if (!id) return;
    this.solutionRegenerating.set(section.id);
    this.tenderService.regenerateSolutionSection(id, section.id, this.solutionCustomPrompt || undefined).subscribe({
      next: (result) => {
        this.solutionRegenerating.set(null);
        this.solutionCustomPrompt = '';
        // Update the section in the list
        const sections = this.solutionSections();
        const updated = sections.map(s => s.id === section.id ? {
          ...s,
          content: result.content,
          content_version: result.version,
          status: 'generated',
        } : s);
        this.solutionSections.set(updated);
        this.snackBar.open(`Section ${result.section_number} regenerated (v${result.version})`, 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.solutionRegenerating.set(null);
        this.snackBar.open(err.error?.detail || 'Failed to regenerate section', 'Close', { duration: 3000 });
      }
    });
  }

  onExportSolution() {
    const id = this.tenderId();
    if (!id) return;
    this.tenderService.exportSolutionDocx(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Technical_Proposal.docx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to export document', 'Close', { duration: 3000 });
      }
    });
  }
}
