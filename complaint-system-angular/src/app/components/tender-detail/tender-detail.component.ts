import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
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
import { TenderService, Tender, TenderDocument } from '../../services/tender.service';
import { AssemblyService } from '../../services/assembly.service';

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
    MatMenuModule
  ],
  templateUrl: './tender-detail.component.html',
  styleUrls: ['./tender-detail.component.css']
})
export class TenderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tenderService = inject(TenderService);
  private assemblyService = inject(AssemblyService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  tender = signal<Tender | null>(null);
  documents = signal<TenderDocument[]>([]);
  loading = signal(false);
  documentsLoading = signal(false);

  displayedColumns: string[] = ['name', 'type', 'size', 'generated', 'actions'];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadTender(id);
      this.loadDocuments(id);
    }
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
    // TODO: Open dialog to add documents
    this.snackBar.open('Add documents dialog - To be implemented', 'Close', { duration: 3000 });
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
        company_name: 'Your Company Name', // TODO: Get from settings
        company_address: 'Your Company Address' // TODO: Get from settings
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

    const documentIds = this.documents().map(d => d.document_id);
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
        company_name: 'Your Company Name', // TODO: Get from settings
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
    // Parse file path (e.g., "packages/filename.zip" or "assembled/filename.pdf")
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
      'draft': 'default',
      'in_progress': 'primary',
      'submitted': 'accent',
      'won': 'success',
      'lost': 'warn',
      'cancelled': 'default'
    };
    return colors[status] || 'default';
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'draft': 'edit',
      'in_progress': 'schedule',
      'submitted': 'send',
      'won': 'check_circle',
      'lost': 'cancel',
      'cancelled': 'block'
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

    if (diffDays < 0) {
      return `Overdue by ${Math.abs(diffDays)} days`;
    } else if (diffDays === 0) {
      return 'Due today';
    } else if (diffDays === 1) {
      return 'Due tomorrow';
    } else if (diffDays <= 7) {
      return `${diffDays} days left`;
    } else {
      return deadlineDate.toLocaleDateString('en-IN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    }
  }

  isDeadlineNear(deadline: string | undefined): boolean {
    if (!deadline) return false;

    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffTime = deadlineDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays >= 0 && diffDays <= 7;
  }

  isDeadlineOverdue(deadline: string | undefined): boolean {
    if (!deadline) return false;

    const deadlineDate = new Date(deadline);
    const now = new Date();

    return deadlineDate.getTime() < now.getTime();
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }
}
