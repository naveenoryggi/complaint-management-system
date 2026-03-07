import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { TenderService, Tender, TenderDocument } from '../../services/tender.service';
import { TrackingService, EMDRecord, TenderFee } from '../../services/tracking.service';
import { DocumentService } from '../../services/document.service';
import { AssemblyService } from '../../services/assembly.service';
import { CompanyService } from '../../services/company.service';
import { AuthService } from '../../services/auth.service';
import { Company } from '../../models/company.model';
import { AddDocumentsDialogComponent, AddDocumentsDialogData, AddDocumentsDialogResult } from '../add-documents-dialog/add-documents-dialog.component';
import { ExtractionTabComponent } from '../tender-workspace/extraction-tab/extraction-tab.component';
import { CriteriaTabComponent } from '../tender-workspace/criteria-tab/criteria-tab.component';
import { OemRequirementsTabComponent } from '../tender-workspace/oem-requirements-tab/oem-requirements-tab.component';
import { EmdFeesTabComponent } from '../tender-workspace/emd-fees-tab/emd-fees-tab.component';
import { DeclarationsTabComponent } from '../tender-workspace/declarations-tab/declarations-tab.component';
import { ComplianceTabComponent } from '../tender-workspace/compliance-tab/compliance-tab.component';
import { BidPreparationTabComponent } from '../tender-workspace/bid-preparation-tab/bid-preparation-tab.component';
import { SubmissionChecklistTabComponent } from '../tender-workspace/submission-checklist-tab/submission-checklist-tab.component';
import { KnowledgeBaseTabComponent } from '../tender-workspace/knowledge-base-tab/knowledge-base-tab.component';
import { PrebidQueriesTabComponent } from '../tender-workspace/prebid-queries-tab/prebid-queries-tab.component';
import { CollaborationPanelComponent } from '../tender-workspace/collaboration-panel/collaboration-panel.component';
import { ResultsTabComponent } from '../tender-workspace/results-tab/results-tab.component';
import { TenderChatWidgetComponent } from '../tender-workspace/tender-chat-widget/tender-chat-widget.component';
import { KnowledgeBaseService } from '../../services/knowledge-base.service';
import { BidNoBidAnalysis } from '../../services/tender.service';
import { NavigationService } from '../../services/navigation.service';
import { BreadcrumbComponent } from '../shared/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-tender-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatMenuModule,
    MatTabsModule,
    MatProgressBarModule,
    MatButtonToggleModule,
    DragDropModule,
    BreadcrumbComponent,
    ExtractionTabComponent,
    CriteriaTabComponent,
    OemRequirementsTabComponent,
    EmdFeesTabComponent,
    DeclarationsTabComponent,
    ComplianceTabComponent,
    BidPreparationTabComponent,
    SubmissionChecklistTabComponent,
    KnowledgeBaseTabComponent,
    PrebidQueriesTabComponent,
    CollaborationPanelComponent,
    ResultsTabComponent,
    TenderChatWidgetComponent
  ],
  templateUrl: './tender-detail.component.html',
  styleUrls: ['./tender-detail.component.css']
})
export class TenderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tenderService = inject(TenderService);
  private documentService = inject(DocumentService);
  private assemblyService = inject(AssemblyService);
  private companyService = inject(CompanyService);
  private authService = inject(AuthService);
  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private kbService = inject(KnowledgeBaseService);
  private navigationService = inject(NavigationService);

  tender = signal<Tender | null>(null);
  documents = signal<TenderDocument[]>([]);
  loading = signal(false);
  documentsLoading = signal(false);
  company = signal<Company | null>(null);
  tenderId = signal<string>('');
  orderChanged = signal(false);
  savingOrder = signal(false);

  // EMD & Fees (from DB records)
  emdRecords = signal<EMDRecord[]>([]);
  feeRecords = signal<TenderFee[]>([]);

  // Feature 4: Workflow
  allowedTransitionsFromApi = signal<string[]>([]);
  statusHistory = signal<any[]>([]);
  showStatusHistory = signal(false);
  transitionReason = signal('');

  // Feature 8: Bid/No-Bid
  bidNoBidResult = signal<BidNoBidAnalysis | null>(null);
  bidNoBidLoading = signal(false);

  // UX Redesign: Phase selector & mobile drawer
  activePhase = signal<'prepare' | 'review' | 'finalize'>('prepare');
  selectedTabIndex = signal(0);
  mobileTabsOpen = signal(false);

  readonly phaseConfig = {
    prepare: {
      label: 'Prepare', icon: 'edit_note',
      tabs: [
        { index: 0, label: 'Overview', icon: 'info' },
        { index: 1, label: 'Documents', icon: 'description' },
        { index: 2, label: 'Extraction', icon: 'auto_awesome' },
        { index: 3, label: 'Criteria', icon: 'grading' },
        { index: 4, label: 'OEM', icon: 'precision_manufacturing' },
        { index: 5, label: 'EMD', icon: 'account_balance' },
        { index: 6, label: 'Declarations', icon: 'gavel' }
      ]
    },
    review: {
      label: 'Review', icon: 'fact_check',
      tabs: [
        { index: 7, label: 'Bid Prep', icon: 'assignment' },
        { index: 8, label: 'Checklist', icon: 'checklist_rtl' },
        { index: 9, label: 'Compliance', icon: 'checklist' },
        { index: 10, label: 'Queries', icon: 'quiz' }
      ]
    },
    finalize: {
      label: 'Finalize', icon: 'send',
      tabs: [
        { index: 11, label: 'KB', icon: 'auto_stories' },
        { index: 12, label: 'Team', icon: 'groups' },
        { index: 13, label: 'Results', icon: 'emoji_events' }
      ]
    }
  };

  currentPhaseTabs = computed(() => this.phaseConfig[this.activePhase()].tabs);

  readonly phaseKeys: Array<'prepare' | 'review' | 'finalize'> = ['prepare', 'review', 'finalize'];

  displayedColumns: string[] = ['drag', 'name', 'type', 'size', 'generated', 'actions'];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.tenderId.set(id);
      this.loadTender(id);
      this.loadDocuments(id);
      this.loadEmdAndFees(id);
    }
    this.loadCompanySettings();
  }

  loadEmdAndFees(tenderId: string) {
    this.trackingService.listEMDs(tenderId).subscribe({
      next: (records) => this.emdRecords.set(records),
      error: () => {} // silent — not critical
    });
    this.trackingService.listFees(tenderId).subscribe({
      next: (records) => this.feeRecords.set(records),
      error: () => {}
    });
  }

  loadCompanySettings() {
    const user = this.authService.currentUserValue;
    if (user?.companyId) {
      this.companyService.getCompanyById(user.companyId).subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.company.set(response.data);
          }
        },
        error: () => {}
      });
    }
  }

  get companyName(): string {
    return this.company()?.name || 'Your Company Name';
  }

  get companyAddress(): string {
    return this.company()?.address || 'Your Company Address';
  }

  get companyEmail(): string {
    return this.company()?.contactEmail || '';
  }

  loadTender(id: string) {
    this.loading.set(true);

    this.tenderService.getTender(id).subscribe({
      next: (tender) => {
        this.tender.set(tender);
        this.loading.set(false);
        this.loadWorkflowTransitions();
        // Update breadcrumb with tender title
        this.navigationService.updateLastBreadcrumb(
          tender.title?.substring(0, 40) || `Tender #${this.tenderId().substring(0, 8)}`,
          'bi-file-earmark-text'
        );
      },
      error: (error) => {
        console.error('Error loading tender:', error);
        this.snackBar.open('Failed to load tender', 'Close', { duration: 3000 });
        this.loading.set(false);
        this.router.navigate(['/tenders']);
      }
    });
  }

  loadDocuments(tenderId: string) {
    this.documentsLoading.set(true);

    this.tenderService.getTenderDocuments(tenderId).subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.documentsLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading documents:', error);
        this.snackBar.open('Failed to load documents', 'Close', { duration: 3000 });
        this.documentsLoading.set(false);
      }
    });
  }

  onEdit() {
    const tender = this.tender();
    if (tender) {
      this.router.navigate(['/tenders', tender.id, 'edit']);
    }
  }

  onCloneTender() {
    const tender = this.tender();
    if (!tender) return;

    const cloneData: any = {
      title: `${tender.title} (Copy)`,
      reference_number: tender.reference_number || undefined,
      issuing_authority: tender.issuing_authority || undefined,
      portal_name: tender.portal_name || undefined,
      portal_url: tender.portal_url || undefined,
      deadline: tender.deadline || undefined,
      estimated_value: tender.estimated_value || undefined,
      requirements: tender.requirements || undefined,
      notes: tender.notes || undefined,
      status: 'draft'
    };

    this.tenderService.createTender(cloneData).subscribe({
      next: (newTender) => {
        this.snackBar.open('Tender cloned successfully', 'View', { duration: 5000 })
          .onAction().subscribe(() => {
            this.router.navigate(['/tenders', newTender.id]);
          });
        this.router.navigate(['/tenders', newTender.id]);
      },
      error: (error) => {
        console.error('Error cloning tender:', error);
        this.snackBar.open('Failed to clone tender', 'Close', { duration: 3000 });
      }
    });
  }

  onDelete() {
    const tender = this.tender();
    if (tender && confirm(`Are you sure you want to delete tender "${tender.title}"?`)) {
      this.tenderService.deleteTender(tender.id).subscribe({
        next: () => {
          this.snackBar.open('Tender deleted successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/tenders']);
        },
        error: (error) => {
          console.error('Error deleting tender:', error);
          this.snackBar.open('Failed to delete tender', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onAddDocuments() {
    const tender = this.tender();
    if (!tender) return;

    const dialogData: AddDocumentsDialogData = {
      tenderId: tender.id,
      existingDocumentIds: this.documents().map(d => d.document_id)
    };

    const dialogRef = this.dialog.open(AddDocumentsDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe((result: AddDocumentsDialogResult | null) => {
      if (!result) return;

      const allDocs = [...result.selectedDocuments, ...result.uploadedDocuments];
      if (allDocs.length === 0) return;

      let completed = 0;
      const total = allDocs.length;

      allDocs.forEach((doc, index) => {
        this.tenderService.associateDocument(tender.id, {
          document_id: doc.id,
          document_order: this.documents().length + index + 1
        }).subscribe({
          next: () => {
            completed++;
            if (completed === total) {
              this.snackBar.open(`${total} document(s) added to tender`, 'Close', { duration: 3000 });
              this.loadDocuments(tender.id);
            }
          },
          error: (error) => {
            console.error(`Error associating document ${doc.name}:`, error);
            completed++;
            if (completed === total) {
              this.snackBar.open('Some documents failed to associate', 'Close', { duration: 3000 });
              this.loadDocuments(tender.id);
            }
          }
        });
      });
    });
  }

  onPreviewDocument(doc: TenderDocument) {
    if (!doc.document_id) return;

    const isPdf = doc.document?.mime_type?.includes('pdf');
    if (!isPdf) {
      this.onDownloadDocument(doc);
      return;
    }

    this.documentService.downloadDocument(doc.document_id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
      error: (error) => {
        console.error('Error previewing document:', error);
        this.snackBar.open('Failed to preview document', 'Close', { duration: 3000 });
      }
    });
  }

  onDownloadDocument(doc: TenderDocument) {
    if (!doc.document_id) return;

    this.documentService.downloadDocument(doc.document_id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.document?.name || 'download';
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (error) => {
        console.error('Error downloading document:', error);
        this.snackBar.open('Failed to download document', 'Close', { duration: 3000 });
      }
    });
  }

  onRemoveDocument(doc: TenderDocument) {
    const tender = this.tender();
    if (tender && confirm(`Remove "${doc.document?.name}" from this tender?`)) {
      this.tenderService.removeDocumentAssociation(tender.id, doc.document_id).subscribe({
        next: () => {
          this.snackBar.open('Document removed', 'Close', { duration: 3000 });
          this.loadDocuments(tender.id);
        },
        error: (error) => {
          console.error('Error removing document:', error);
          this.snackBar.open('Failed to remove document', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onDocumentDrop(event: CdkDragDrop<TenderDocument[]>) {
    if (event.previousIndex === event.currentIndex) return;

    const docs = [...this.documents()];
    moveItemInArray(docs, event.previousIndex, event.currentIndex);
    this.documents.set(docs);
    this.orderChanged.set(true);
  }

  onSaveOrder() {
    const tender = this.tender();
    if (!tender) return;

    const documentIds = this.documents().map(d => d.document_id);
    this.savingOrder.set(true);

    this.tenderService.reorderDocuments(tender.id, documentIds).subscribe({
      next: () => {
        this.orderChanged.set(false);
        this.savingOrder.set(false);
        this.snackBar.open('Document order saved', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error saving order:', error);
        this.savingOrder.set(false);
        this.snackBar.open('Failed to save order', 'Close', { duration: 3000 });
      }
    });
  }

  onMoveUp(index: number) {
    if (index === 0) return;
    const docs = [...this.documents()];
    moveItemInArray(docs, index, index - 1);
    this.documents.set(docs);
    this.orderChanged.set(true);
  }

  onMoveDown(index: number) {
    const docs = this.documents();
    if (index >= docs.length - 1) return;
    const updated = [...docs];
    moveItemInArray(updated, index, index + 1);
    this.documents.set(updated);
    this.orderChanged.set(true);
  }

  onExportPackage() {
    const tender = this.tender();
    if (!tender) return;

    const documentIds = this.documents().map(d => d.document_id);
    if (documentIds.length === 0) {
      this.snackBar.open('No documents to export', 'Close', { duration: 3000 });
      return;
    }

    this.snackBar.open('Exporting tender package...', '', { duration: 0 });

    this.assemblyService.exportPackage({
      document_ids: documentIds,
      package_name: this.sanitizeFilename(tender.title),
      include_cover: true,
      cover_data: {
        tender_title: tender.title,
        tender_reference: tender.reference_number,
        issuing_authority: tender.issuing_authority || 'N/A',
        company_name: this.companyName,
        company_address: this.companyAddress,
        company_email: this.companyEmail
      },
      merge_pdfs: true
    }).subscribe({
      next: (response) => {
        this.snackBar.dismiss();
        this.snackBar.open('Package exported successfully', 'Download', { duration: 5000 }).onAction().subscribe(() => {
          this.downloadFile(response.file_path);
        });
      },
      error: (error) => {
        console.error('Error exporting package:', error);
        this.snackBar.dismiss();
        this.snackBar.open('Failed to export package', 'Close', { duration: 3000 });
      }
    });
  }

  onMergePDFs() {
    const tender = this.tender();
    if (!tender) return;

    const pdfDocuments = this.documents().filter(d => d.document?.mime_type === 'application/pdf');

    if (pdfDocuments.length === 0) {
      this.snackBar.open('No PDF documents to merge', 'Close', { duration: 3000 });
      return;
    }

    this.snackBar.open('Merging PDFs...', '', { duration: 0 });

    this.assemblyService.mergePDFs({
      document_ids: pdfDocuments.map(d => d.document_id),
      add_cover: true,
      cover_data: {
        tender_title: tender.title,
        tender_reference: tender.reference_number,
        issuing_authority: tender.issuing_authority || 'N/A',
        company_name: this.companyName,
      },
      output_filename: `${this.sanitizeFilename(tender.title)}_merged.pdf`
    }).subscribe({
      next: (response) => {
        this.snackBar.dismiss();
        this.snackBar.open('PDFs merged successfully', 'Download', { duration: 5000 }).onAction().subscribe(() => {
          this.downloadFile(response.file_path);
        });
      },
      error: (error) => {
        console.error('Error merging PDFs:', error);
        this.snackBar.dismiss();
        this.snackBar.open('Failed to merge PDFs', 'Close', { duration: 3000 });
      }
    });
  }

  downloadFile(filePath: string) {
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
        error: (error) => {
          console.error('Error downloading file:', error);
          this.snackBar.open('Failed to download file', 'Close', { duration: 3000 });
        }
      });
    }
  }

  sanitizeFilename(filename: string): string {
    return filename.replace(/[^a-z0-9]/gi, '_').toLowerCase();
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'draft': 'default', 'in_progress': 'primary', 'submitted': 'accent',
      'won': 'success', 'lost': 'warn', 'cancelled': 'default'
    };
    return colors[status] || 'default';
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'draft': 'edit', 'in_progress': 'schedule', 'submitted': 'send',
      'won': 'check_circle', 'lost': 'cancel', 'cancelled': 'block'
    };
    return icons[status] || 'help';
  }

  formatFileSize(bytes: number | undefined): string {
    if (!bytes) return '0 B';
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  }

  formatDeadline(deadline: string | undefined): string {
    if (!deadline) return 'No deadline';
    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffTime = deadlineDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return `Overdue by ${Math.abs(diffDays)} days`;
    else if (diffDays === 0) return 'Due today';
    else if (diffDays === 1) return 'Due tomorrow';
    else if (diffDays <= 7) return `${diffDays} days left`;
    else return deadlineDate.toLocaleDateString('en-IN', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  isDeadlineNear(deadline: string | undefined): boolean {
    if (!deadline) return false;
    const diffDays = Math.ceil((new Date(deadline).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24));
    return diffDays >= 0 && diffDays <= 7;
  }

  isDeadlineOverdue(deadline: string | undefined): boolean {
    if (!deadline) return false;
    return new Date(deadline).getTime() < new Date().getTime();
  }

  statusLabels: { [key: string]: { label: string; icon: string } } = {
    'draft': { label: 'Draft', icon: 'edit' },
    'in_progress': { label: 'In Progress', icon: 'play_arrow' },
    'submitted': { label: 'Submitted', icon: 'send' },
    'won': { label: 'Won', icon: 'check_circle' },
    'lost': { label: 'Lost', icon: 'cancel' },
    'cancelled': { label: 'Cancelled', icon: 'block' },
  };

  loadWorkflowTransitions() {
    const tender = this.tender();
    if (!tender) return;
    this.tenderService.getAllowedTransitions(tender.id).subscribe({
      next: (result) => this.allowedTransitionsFromApi.set(result.allowed_transitions),
      error: () => {
        // Fallback to local transitions
        this.allowedTransitionsFromApi.set(this.getLocalTransitions(tender.status));
      }
    });
  }

  getLocalTransitions(status: string): string[] {
    const map: { [key: string]: string[] } = {
      'draft': ['in_progress', 'cancelled'],
      'in_progress': ['submitted', 'cancelled', 'draft'],
      'submitted': ['won', 'lost', 'cancelled'],
      'won': [], 'lost': [], 'cancelled': []
    };
    return map[status] || [];
  }

  getAvailableTransitions(): { value: string; label: string; icon: string }[] {
    const allowed = this.allowedTransitionsFromApi();
    if (allowed.length === 0) {
      const tender = this.tender();
      if (!tender) return [];
      const local = this.getLocalTransitions(tender.status);
      return local.map(s => ({
        value: s,
        label: this.statusLabels[s]?.label || s,
        icon: this.statusLabels[s]?.icon || 'help'
      }));
    }
    return allowed.map(s => ({
      value: s,
      label: this.statusLabels[s]?.label || s,
      icon: this.statusLabels[s]?.icon || 'help'
    }));
  }

  onChangeStatus(newStatus: string) {
    const tender = this.tender();
    if (!tender) return;

    const label = this.statusLabels[newStatus]?.label || newStatus;

    if (confirm(`Change status to "${label}"?`)) {
      this.tenderService.transitionStatus(tender.id, newStatus, this.transitionReason() || undefined).subscribe({
        next: (result) => {
          this.tender.set({ ...tender, status: newStatus });
          this.snackBar.open(`Status changed to ${label}`, 'Close', { duration: 3000 });
          this.transitionReason.set('');
          this.loadWorkflowTransitions();
          // Auto-harvest KB when tender is submitted or won
          if (newStatus === 'submitted' || newStatus === 'won') {
            this.kbService.harvestFromTender(tender.id).subscribe({
              next: (result) => {
                if (result.created > 0) {
                  this.snackBar.open(
                    `KB auto-harvested: ${result.created} entries added (${result.skipped} skipped)`,
                    'Close', { duration: 5000 }
                  );
                }
              },
              error: () => {}
            });
          }
        },
        error: (error) => {
          console.error('Error changing status:', error);
          const detail = error?.error?.detail || 'Failed to change status';
          this.snackBar.open(detail, 'Close', { duration: 4000 });
        }
      });
    }
  }

  loadStatusHistory() {
    const tender = this.tender();
    if (!tender) return;
    this.tenderService.getStatusHistory(tender.id).subscribe({
      next: (history) => {
        this.statusHistory.set(history);
        this.showStatusHistory.set(true);
      },
      error: () => this.snackBar.open('Failed to load status history', 'Close', { duration: 3000 })
    });
  }

  // Feature 8: Bid/No-Bid
  onRunBidNoBidAnalysis() {
    const tender = this.tender();
    if (!tender) return;
    this.bidNoBidLoading.set(true);
    this.tenderService.analyzeBidDecision(tender.id).subscribe({
      next: (result) => {
        this.bidNoBidResult.set(result);
        this.bidNoBidLoading.set(false);
      },
      error: () => {
        this.bidNoBidLoading.set(false);
        this.snackBar.open('Bid analysis failed', 'Close', { duration: 3000 });
      }
    });
  }

  getRecommendationColor(rec: string): string {
    if (rec === 'bid') return '#4caf50';
    if (rec === 'no_bid') return '#f44336';
    return '#ff9800';
  }

  getRecommendationLabel(rec: string): string {
    if (rec === 'bid') return 'BID';
    if (rec === 'no_bid') return 'NO BID';
    return 'CONDITIONAL';
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', minimumFractionDigits: 0, maximumFractionDigits: 0
    }).format(value);
  }

  // UX Redesign: Phase & Tab methods
  onPhaseChange(phase: 'prepare' | 'review' | 'finalize') {
    this.activePhase.set(phase);
    const firstTab = this.phaseConfig[phase].tabs[0];
    this.selectedTabIndex.set(firstTab.index);
  }

  onTabChange(index: number) {
    this.selectedTabIndex.set(index);
    // Auto-detect phase from tab index
    if (index <= 6) {
      this.activePhase.set('prepare');
    } else if (index <= 10) {
      this.activePhase.set('review');
    } else {
      this.activePhase.set('finalize');
    }
  }

  toggleMobileTabs() {
    this.mobileTabsOpen.set(!this.mobileTabsOpen());
  }

  selectMobileTab(index: number) {
    this.selectedTabIndex.set(index);
    this.onTabChange(index);
    this.mobileTabsOpen.set(false);
  }

  getDeadlineUrgencyClass(deadline: string | undefined): string {
    if (!deadline) return '';
    const diffDays = Math.ceil((new Date(deadline).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24));
    if (diffDays < 0) return 'urgency-overdue';
    if (diffDays <= 3) return 'urgency-critical';
    if (diffDays <= 7) return 'urgency-warning';
    return 'urgency-safe';
  }

  getDeadlineAbsoluteDate(deadline: string | undefined): string {
    if (!deadline) return '';
    return new Date(deadline).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  getStatusPillClass(status: string): string {
    return `status-pill-${status}`;
  }

  // --- Overview: Requirements Rendering Helpers ---

  getRequirementsKeys(): string[] {
    const req = this.tender()?.requirements;
    if (!req || typeof req !== 'object') return [];
    // Only exclude _extraction_meta (internal). Keep emd/tender_fees in generic
    // rendering too — the dedicated card provides rich view, generic is fallback.
    const excludeKeys = new Set(['_extraction_meta']);
    // If dedicated EMD card is showing, exclude from generic to avoid duplication
    if (this.hasEmdOrFees()) {
      excludeKeys.add('emd');
      excludeKeys.add('tender_fees');
    }
    return Object.keys(req).filter(k => {
      if (excludeKeys.has(k)) return false;
      const val = req[k];
      if (val === null || val === undefined) return false;
      if (Array.isArray(val) && val.length === 0) return false;
      if (typeof val === 'string' && val.trim() === '') return false;
      return true;
    });
  }

  // --- EMD & Fee helpers for Overview ---

  getEmdData(): any {
    return this.tender()?.requirements?.emd;
  }

  getFeeData(): any[] {
    return this.tender()?.requirements?.tender_fees || [];
  }

  hasEmdOrFees(): boolean {
    // Check both: requirements JSON (extracted) AND DB records
    const emd = this.getEmdData();
    const hasExtractedEmd = emd && typeof emd === 'object' && Object.keys(emd).length > 0;
    return hasExtractedEmd || this.getFeeData().length > 0
      || this.emdRecords().length > 0 || this.feeRecords().length > 0;
  }

  getEmdModeLabel(mode: string): string {
    const labels: Record<string, string> = {
      'bg': 'Bank Guarantee', 'dd': 'Demand Draft', 'online': 'Online Transfer',
      'fixed_deposit': 'Fixed Deposit', 'insurance_surety': 'Insurance Surety Bond',
      'neft': 'NEFT', 'rtgs': 'RTGS',
    };
    return labels[mode] || mode;
  }

  getFeeTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      'tender_fee': 'Tender Fee', 'processing_fee': 'Processing Fee',
      'document_fee': 'Document Fee', 'e_procurement_fee': 'E-Procurement Fee',
      'bid_security': 'Bid Security',
    };
    return labels[type] || type;
  }

  getBankDetailEntries(bank: any): { label: string; value: string }[] {
    if (!bank || typeof bank !== 'object') return [];
    const map: Record<string, string> = {
      bank_name: 'Bank', branch: 'Branch', account_number: 'Account No.',
      ifsc_code: 'IFSC', account_holder: 'Beneficiary', dd_in_favour_of: 'DD in Favour of',
    };
    return Object.entries(map)
      .filter(([k]) => bank[k])
      .map(([k, label]) => ({ label, value: bank[k] }));
  }

  formatRequirementLabel(key: string): string {
    return key
      .replace(/_/g, ' ')
      .replace(/\b\w/g, c => c.toUpperCase());
  }

  getRequirementIcon(key: string): string {
    const icons: Record<string, string> = {
      eligibility_criteria: 'verified_user',
      technical_requirements: 'precision_manufacturing',
      special_conditions: 'warning',
      document_checklist: 'checklist',
      evaluation_criteria: 'grading',
      financial_requirements: 'payments',
      oem_requirements: 'factory',
      emd_details: 'account_balance',
      tender_fees: 'receipt_long',
      key_dates: 'event',
      contact_details: 'contact_mail',
      scope_of_work: 'work',
      quantities: 'inventory',
    };
    return icons[key] || 'info';
  }

  getRequirementValue(key: string): any {
    return this.tender()?.requirements?.[key];
  }

  isArray(val: any): boolean {
    return Array.isArray(val);
  }

  isObject(val: any): boolean {
    return val !== null && typeof val === 'object' && !Array.isArray(val);
  }

  isSimple(val: any): boolean {
    return typeof val === 'string' || typeof val === 'number' || typeof val === 'boolean';
  }

  getItemText(item: any): string {
    if (typeof item === 'string') return item;
    if (typeof item === 'number' || typeof item === 'boolean') return String(item);
    if (typeof item === 'object' && item !== null) {
      // Common patterns: {name: ...}, {description: ...}, {title: ...}
      return item.name || item.description || item.title || item.document_name || JSON.stringify(item);
    }
    return String(item);
  }

  getItemSubtext(item: any): string | null {
    if (typeof item !== 'object' || item === null) return null;
    const parts: string[] = [];
    if (item.max_marks !== undefined) parts.push(`Max marks: ${item.max_marks}`);
    if (item.weightage !== undefined) parts.push(`Weightage: ${item.weightage}`);
    if (item.mode) parts.push(`Mode: ${item.mode}`);
    if (item.category) parts.push(`Category: ${item.category}`);
    if (item.amount) parts.push(`Amount: ${item.amount}`);
    if (item.date) parts.push(`Date: ${item.date}`);
    if (item.status) parts.push(`Status: ${item.status}`);
    return parts.length > 0 ? parts.join(' · ') : null;
  }

  getObjectEntries(val: any): { key: string; value: any }[] {
    if (!val || typeof val !== 'object' || Array.isArray(val)) return [];
    return Object.entries(val)
      .filter(([, v]) => v !== null && v !== undefined && v !== '')
      .map(([k, v]) => ({ key: this.formatRequirementLabel(k), value: v }));
  }

  hasExtractedRequirements(): boolean {
    return this.getRequirementsKeys().length > 0;
  }
}
