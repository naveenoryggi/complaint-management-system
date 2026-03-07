import { Component, Input, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import {
  TenderService, ExtractedTenderData, ExtractionResponse, ApplyExtractionResponse
} from '../../../services/tender.service';

@Component({
  selector: 'app-extraction-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatChipsModule,
    MatDividerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './extraction-tab.component.html',
  styleUrls: ['./extraction-tab.component.css']
})
export class ExtractionTabComponent implements OnChanges {
  @Input() tenderId!: string;
  @Input() tenderRequirements: any;

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);

  extracting = signal(false);
  extractedData = signal<ExtractedTenderData | null>(null);
  extractionMeta = signal<{
    model_used: string;
    tokens_used: number;
    extraction_mode?: string;
    files_processed?: number;
    batches_used?: number;
  } | null>(null);
  appliedSummary = signal<ApplyExtractionResponse | null>(null);

  selectedFiles = signal<File[]>([]);
  dragOver = signal(false);

  selectedModel = 'claude-sonnet-4-6';

  modelOptions = [
    { value: 'claude-sonnet-4-6', label: 'Claude Sonnet 4.6 (Fast)' },
    { value: 'claude-opus-4-6', label: 'Claude Opus 4.6 (Best)' },
    { value: 'claude-haiku-4-5-20251001', label: 'Claude Haiku 4.5 (Budget)' }
  ];

  ngOnChanges(changes: SimpleChanges) {
    // Load previously extracted data from stored tender requirements
    // This fires when parent loads tender data (async), so tenderRequirements arrives after init
    if (changes['tenderRequirements'] && this.tenderRequirements && !this.extractedData()) {
      this.loadStoredExtraction(this.tenderRequirements);
    }
  }

  ngOnInit() {
    // Fallback: if tenderRequirements was already set before ngOnChanges
    if (this.tenderRequirements && !this.extractedData()) {
      this.loadStoredExtraction(this.tenderRequirements);
    }
  }

  private loadStoredExtraction(requirements: any) {
    if (!requirements || typeof requirements !== 'object') return;

    // Check if there's meaningful extracted data (not just empty object)
    const hasData = Object.keys(requirements).some(
      k => k !== '_extraction_meta' && requirements[k] !== null && requirements[k] !== undefined
    );
    if (!hasData) return;

    // Build ExtractedTenderData from stored requirements (exclude internal meta)
    const { _extraction_meta, ...extractedFields } = requirements;
    this.extractedData.set(extractedFields as ExtractedTenderData);

    // Restore extraction metadata if available
    if (_extraction_meta) {
      this.extractionMeta.set({
        model_used: _extraction_meta.model_used || 'unknown',
        tokens_used: _extraction_meta.tokens_used || 0,
        extraction_mode: _extraction_meta.extraction_mode,
        files_processed: _extraction_meta.files_processed,
      });
    }

    // Mark as previously applied (show a subtle banner)
    this.previouslyExtracted.set(true);
  }

  previouslyExtracted = signal(false);

  // --- File handling ---

  onFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.addFiles(Array.from(input.files));
      input.value = '';
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);

    const files = event.dataTransfer?.files;
    if (files?.length) {
      const pdfFiles = Array.from(files).filter(f => f.name.toLowerCase().endsWith('.pdf'));
      if (pdfFiles.length === 0) {
        this.snackBar.open('Only PDF files are supported', 'Close', { duration: 3000 });
        return;
      }
      if (pdfFiles.length < files.length) {
        this.snackBar.open(`${files.length - pdfFiles.length} non-PDF file(s) skipped`, 'Close', { duration: 3000 });
      }
      this.addFiles(pdfFiles);
    }
  }

  private addFiles(newFiles: File[]) {
    const current = this.selectedFiles();
    const existingKeys = new Set(current.map(f => `${f.name}_${f.size}`));
    const unique = newFiles.filter(f => !existingKeys.has(`${f.name}_${f.size}`));
    this.selectedFiles.set([...current, ...unique]);
    this.extractedData.set(null);
    this.extractionMeta.set(null);
    this.appliedSummary.set(null);
  }

  removeFile(index: number) {
    const current = [...this.selectedFiles()];
    current.splice(index, 1);
    this.selectedFiles.set(current);
    this.extractedData.set(null);
    this.extractionMeta.set(null);
    this.appliedSummary.set(null);
  }

  clearFiles() {
    this.selectedFiles.set([]);
    this.extractedData.set(null);
    this.extractionMeta.set(null);
    this.appliedSummary.set(null);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(0)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  // --- Extraction (auto-applies to tender) ---

  onExtract() {
    const files = this.selectedFiles();
    if (!files.length || !this.tenderId) return;

    this.extracting.set(true);
    this.extractedData.set(null);
    this.appliedSummary.set(null);
    this.previouslyExtracted.set(false);

    this.tenderService.extractFromPDF(this.tenderId, files, this.selectedModel).subscribe({
      next: (response: ExtractionResponse) => {
        this.extractedData.set(response.extracted_data);
        this.extractionMeta.set({
          model_used: response.model_used,
          tokens_used: response.tokens_used,
          extraction_mode: response.extraction_mode,
          files_processed: response.files_processed,
          batches_used: response.batches_used,
        });
        this.appliedSummary.set(response.applied_summary || null);
        this.extracting.set(false);

        // Build snackbar message from applied summary
        if (response.applied_summary) {
          const s = response.applied_summary;
          const parts = [
            `${s.fields_updated.length} fields updated`,
            s.criteria_created ? `${s.criteria_created} criteria` : null,
            s.emd_created ? 'EMD created' : null,
            s.fees_created ? `${s.fees_created} fees` : null,
            s.oem_requirements_created ? `${s.oem_requirements_created} OEMs` : null,
          ].filter(Boolean);
          this.snackBar.open(`Extraction complete & applied: ${parts.join(', ')}`, 'Close', { duration: 6000 });
        } else {
          this.snackBar.open('Extraction complete & applied to tender.', 'Close', { duration: 5000 });
        }
      },
      error: (error) => {
        console.error('Extraction error:', error);
        this.extracting.set(false);
        this.snackBar.open(error.error?.detail || 'Extraction failed', 'Close', { duration: 5000 });
      }
    });
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency', currency: 'INR', maximumFractionDigits: 0
    }).format(value);
  }
}
