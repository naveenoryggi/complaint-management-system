import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
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
import { TenderService, Tender, TenderDocument } from '../../services/tender.service';
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

@Component({
  selector: 'app-tender-detail',
  standalone: true,
  imports: [
    CommonModule,
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
    DragDropModule,
    ExtractionTabComponent,
    CriteriaTabComponent,
    OemRequirementsTabComponent,
    EmdFeesTabComponent,
    DeclarationsTabComponent
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
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  tender = signal<Tender | null>(null);
  documents = signal<TenderDocument[]>([]);
  loading = signal(false);
  documentsLoading = signal(false);
  company = signal<Company | null>(null);
  tenderId = signal<string>('');
  orderChanged = signal(false);
  savingOrder = signal(false);

  displayedColumns: string[] = ['drag', 'name', 'type', 'size', 'generated', 'actions'];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.tenderId.set(id);
      this.loadTender(id);
      this.loadDocuments(id);
    }
    this.loadCompanySettings();
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

  statusTransitions: { [key: string]: { value: string; label: string; icon: string }[] } = {
    'draft': [
      { value: 'in_progress', label: 'Start Progress', icon: 'play_arrow' }
    ],
    'in_progress': [
      { value: 'submitted', label: 'Mark Submitted', icon: 'send' },
      { value: 'draft', label: 'Back to Draft', icon: 'undo' }
    ],
    'submitted': [
      { value: 'won', label: 'Mark Won', icon: 'check_circle' },
      { value: 'lost', label: 'Mark Lost', icon: 'cancel' }
    ],
    'won': [],
    'lost': [],
    'cancelled': []
  };

  getAvailableTransitions(): { value: string; label: string; icon: string }[] {
    const tender = this.tender();
    if (!tender) return [];
    const transitions = [...(this.statusTransitions[tender.status] || [])];
    if (tender.status !== 'cancelled' && tender.status !== 'won' && tender.status !== 'lost') {
      transitions.push({ value: 'cancelled', label: 'Cancel Tender', icon: 'block' });
    }
    return transitions;
  }

  onChangeStatus(newStatus: string) {
    const tender = this.tender();
    if (!tender) return;

    const labels: { [key: string]: string } = {
      'in_progress': 'In Progress', 'submitted': 'Submitted',
      'won': 'Won', 'lost': 'Lost', 'cancelled': 'Cancelled', 'draft': 'Draft'
    };

    if (confirm(`Change status to "${labels[newStatus] || newStatus}"?`)) {
      this.tenderService.updateTender(tender.id, { status: newStatus }).subscribe({
        next: (updated) => {
          this.tender.set(updated);
          this.snackBar.open(`Status changed to ${labels[newStatus]}`, 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error changing status:', error);
          this.snackBar.open('Failed to change status', 'Close', { duration: 3000 });
        }
      });
    }
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', minimumFractionDigits: 0, maximumFractionDigits: 0
    }).format(value);
  }
}
