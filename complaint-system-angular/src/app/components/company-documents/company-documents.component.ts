import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { BreadcrumbComponent } from '../shared/breadcrumb/breadcrumb.component';
import {
  CompanyDocumentService,
  CompanyDocument,
  CategorySummary,
} from '../../services/company-document.service';
import { DocumentService } from '../../services/document.service';

@Component({
  selector: 'app-company-documents',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatDialogModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    BreadcrumbComponent,
  ],
  templateUrl: './company-documents.component.html',
  styleUrls: ['./company-documents.component.css'],
})
export class CompanyDocumentsComponent implements OnInit {
  private docService = inject(CompanyDocumentService);
  private documentService = inject(DocumentService);
  private snackBar = inject(MatSnackBar);

  documents = signal<CompanyDocument[]>([]);
  categories = signal<CategorySummary[]>([]);
  loading = signal(false);
  selectedCategory = signal<string | null>(null);

  // Upload form state
  showUploadForm = signal(false);
  uploadFile: File | null = null;
  uploadName = '';
  uploadCategory = 'pan_card';
  uploadDescription = '';
  uploadExpiryDate = '';
  uploading = signal(false);

  // Edit state
  editingDoc = signal<CompanyDocument | null>(null);
  editName = '';
  editCategory = '';
  editExpiry = '';

  displayedColumns = ['name', 'category', 'size', 'expiry', 'actions'];

  readonly categoryOptions = [
    { value: 'pan_card', label: 'PAN Card' },
    { value: 'gst_certificate', label: 'GST Certificate' },
    { value: 'company_registration', label: 'Company Registration' },
    { value: 'msme_certificate', label: 'MSME Certificate' },
    { value: 'solvency_certificate', label: 'Solvency Certificate' },
    { value: 'turnover_certificate', label: 'Turnover Certificate' },
    { value: 'iso_certificate', label: 'ISO Certificate' },
    { value: 'audited_balance_sheet', label: 'Audited Balance Sheet' },
    { value: 'power_of_attorney', label: 'Power of Attorney' },
    { value: 'board_resolution', label: 'Board Resolution' },
    { value: 'epf_certificate', label: 'EPF Certificate' },
    { value: 'esi_certificate', label: 'ESI Certificate' },
    { value: 'other', label: 'Other' },
  ];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading.set(true);
    this.docService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats),
      error: (err) => { console.error('Error loading document categories:', err); },
    });
    this.docService.list(this.selectedCategory() || undefined).subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.loading.set(false);
      },
      error: (err) => {
        this.snackBar.open(err.error?.detail || 'Failed to load documents', 'Close', { duration: 5000 });
        this.loading.set(false);
      },
    });
  }

  filterByCategory(category: string | null) {
    this.selectedCategory.set(category);
    this.loadData();
  }

  getCategoryIcon(cat: CategorySummary): string {
    if (cat.count === 0) return 'cancel';
    if (cat.has_expired) return 'error';
    if (cat.has_expiring) return 'schedule';
    return 'check_circle';
  }

  getCategoryIconColor(cat: CategorySummary): string {
    if (cat.count === 0) return '#e53935';
    if (cat.has_expired) return '#e53935';
    if (cat.has_expiring) return '#ff9800';
    return '#4caf50';
  }

  get expiringDocs(): CompanyDocument[] {
    const today = new Date().toISOString().slice(0, 10);
    const threshold = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
    return this.documents().filter(
      (d) => d.expiry_date && d.expiry_date <= threshold
    );
  }

  getCategoryLabel(value: string): string {
    return this.categoryOptions.find((c) => c.value === value)?.label || value;
  }

  // Upload
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFile = input.files[0];
      if (!this.uploadName) {
        this.uploadName = this.uploadFile.name.replace(/\.[^.]+$/, '');
      }
    }
  }

  onUpload() {
    if (!this.uploadFile) return;
    this.uploading.set(true);
    this.docService
      .upload(
        this.uploadFile,
        this.uploadName,
        this.uploadCategory,
        this.uploadDescription || undefined,
        this.uploadExpiryDate || undefined
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Document uploaded', 'Close', { duration: 3000 });
          this.resetUploadForm();
          this.loadData();
        },
        error: (err) => {
          this.snackBar.open(err.error?.detail || 'Upload failed', 'Close', { duration: 5000 });
          this.uploading.set(false);
        },
      });
  }

  resetUploadForm() {
    this.showUploadForm.set(false);
    this.uploadFile = null;
    this.uploadName = '';
    this.uploadCategory = 'pan_card';
    this.uploadDescription = '';
    this.uploadExpiryDate = '';
    this.uploading.set(false);
  }

  // Edit
  startEdit(doc: CompanyDocument) {
    this.editingDoc.set(doc);
    this.editName = doc.name;
    this.editCategory = doc.company_doc_category;
    this.editExpiry = doc.expiry_date || '';
  }

  saveEdit() {
    const doc = this.editingDoc();
    if (!doc) return;
    this.docService
      .update(doc.id, {
        name: this.editName,
        company_doc_category: this.editCategory,
        expiry_date: this.editExpiry || undefined,
      })
      .subscribe({
        next: () => {
          this.snackBar.open('Document updated', 'Close', { duration: 3000 });
          this.editingDoc.set(null);
          this.loadData();
        },
        error: (err) => this.snackBar.open(err.error?.detail || 'Update failed', 'Close', { duration: 5000 }),
      });
  }

  cancelEdit() {
    this.editingDoc.set(null);
  }

  // Download
  onDownload(doc: CompanyDocument) {
    this.documentService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.name;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Download failed', 'Close', { duration: 5000 }),
    });
  }

  // Delete
  onDelete(doc: CompanyDocument) {
    if (!confirm(`Delete "${doc.name}"?`)) return;
    this.docService.delete(doc.id).subscribe({
      next: () => {
        this.snackBar.open('Document deleted', 'Close', { duration: 3000 });
        this.loadData();
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Delete failed', 'Close', { duration: 5000 }),
    });
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '0 B';
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return (bytes / Math.pow(1024, i)).toFixed(1) + ' ' + sizes[i];
  }

  isExpiringSoon(doc: CompanyDocument): boolean {
    if (!doc.expiry_date) return false;
    const threshold = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
    return doc.expiry_date <= threshold;
  }

  isExpired(doc: CompanyDocument): boolean {
    if (!doc.expiry_date) return false;
    return doc.expiry_date < new Date().toISOString().slice(0, 10);
  }
}
