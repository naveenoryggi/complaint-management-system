import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { SelectionModel } from '@angular/cdk/collections';
import { DocumentService, Document } from '../../services/document.service';

export interface AddDocumentsDialogData {
  tenderId: string;
  existingDocumentIds: string[];
}

export interface AddDocumentsDialogResult {
  selectedDocuments: Document[];
  uploadedDocuments: Document[];
}

@Component({
  selector: 'app-add-documents-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatChipsModule,
    MatSnackBarModule,
    MatPaginatorModule,
  ],
  templateUrl: './add-documents-dialog.component.html',
  styleUrls: ['./add-documents-dialog.component.css']
})
export class AddDocumentsDialogComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<AddDocumentsDialogComponent>);
  private data: AddDocumentsDialogData = inject(MAT_DIALOG_DATA);
  private documentService = inject(DocumentService);
  private snackBar = inject(MatSnackBar);

  // Library tab
  documents = signal<Document[]>([]);
  loading = signal(false);
  totalDocuments = signal(0);
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  typeFilter = '';
  selection = new SelectionModel<Document>(true, []);
  displayedColumns: string[] = ['select', 'name', 'type', 'size', 'tags'];

  documentTypes = [
    'certificate', 'financial', 'technical', 'legal',
    'compliance', 'proposal', 'supporting', 'other'
  ];

  // Upload tab
  uploadFiles: File[] = [];
  uploadDocType = '';
  uploading = signal(false);
  uploadProgress = signal(0);
  uploadedDocuments: Document[] = [];

  ngOnInit() {
    this.loadDocuments();
  }

  loadDocuments() {
    this.loading.set(true);
    const page = this.pageIndex + 1;

    const search$ = this.searchQuery || this.typeFilter
      ? this.documentService.searchDocuments(
          this.searchQuery || undefined,
          this.typeFilter || undefined,
          undefined, undefined,
          page, this.pageSize
        )
      : this.documentService.listDocuments(page, this.pageSize);

    search$.subscribe({
      next: (response) => {
        // Filter out documents already associated with this tender
        const filtered = response.items.filter(
          doc => !this.data.existingDocumentIds.includes(doc.id)
        );
        this.documents.set(filtered);
        this.totalDocuments.set(response.total);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading documents:', error);
        this.snackBar.open('Failed to load documents', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    this.pageIndex = 0;
    this.loadDocuments();
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadDocuments();
  }

  isAllSelected(): boolean {
    return this.selection.selected.length === this.documents().length && this.documents().length > 0;
  }

  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
    } else {
      this.selection.select(...this.documents());
    }
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '0 B';
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  }

  // Upload tab methods
  onFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.uploadFiles = Array.from(input.files);
    }
  }

  removeFile(index: number) {
    this.uploadFiles.splice(index, 1);
  }

  uploadAllFiles() {
    if (this.uploadFiles.length === 0) return;

    this.uploading.set(true);
    this.uploadProgress.set(0);
    let completed = 0;

    const uploadNext = (index: number) => {
      if (index >= this.uploadFiles.length) {
        this.uploading.set(false);
        this.uploadFiles = [];
        this.snackBar.open(`${completed} document(s) uploaded`, 'Close', { duration: 3000 });
        // Refresh library list
        this.loadDocuments();
        return;
      }

      const file = this.uploadFiles[index];
      const name = file.name.replace(/\.[^/.]+$/, '');

      this.documentService.uploadDocument(file, name, this.uploadDocType || undefined).subscribe({
        next: (response) => {
          completed++;
          this.uploadProgress.set(Math.round((completed / this.uploadFiles.length) * 100));
          // Fetch the full document to add to results
          this.documentService.getDocument(response.id).subscribe({
            next: (doc) => {
              this.uploadedDocuments.push(doc);
              uploadNext(index + 1);
            },
            error: () => uploadNext(index + 1)
          });
        },
        error: (error) => {
          console.error(`Error uploading ${file.name}:`, error);
          this.snackBar.open(`Failed to upload ${file.name}`, 'Close', { duration: 3000 });
          uploadNext(index + 1);
        }
      });
    };

    uploadNext(0);
  }

  getFileIcon(file: File): string {
    if (file.type.includes('pdf')) return 'picture_as_pdf';
    if (file.type.includes('word') || file.name.endsWith('.docx')) return 'description';
    if (file.type.includes('spreadsheet') || file.name.endsWith('.xlsx')) return 'table_chart';
    if (file.type.includes('image')) return 'image';
    return 'insert_drive_file';
  }

  onConfirm() {
    const result: AddDocumentsDialogResult = {
      selectedDocuments: this.selection.selected,
      uploadedDocuments: this.uploadedDocuments
    };
    this.dialogRef.close(result);
  }

  onCancel() {
    this.dialogRef.close(null);
  }

  get totalSelected(): number {
    return this.selection.selected.length + this.uploadedDocuments.length;
  }
}
