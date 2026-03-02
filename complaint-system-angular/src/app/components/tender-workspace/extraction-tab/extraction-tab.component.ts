import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { TenderService, ExtractedTenderData, ExtractionResponse, ApplyExtractionResponse } from '../../../services/tender.service';

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
    MatCheckboxModule,
    MatDividerModule,
    MatSelectModule,
    MatFormFieldModule,
    FormsModule
  ],
  templateUrl: './extraction-tab.component.html',
  styleUrls: ['./extraction-tab.component.css']
})
export class ExtractionTabComponent {
  @Input() tenderId!: string;

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);

  extracting = signal(false);
  applying = signal(false);
  extractedData = signal<ExtractedTenderData | null>(null);
  extractionMeta = signal<{ model_used: string; tokens_used: number } | null>(null);
  selectedFile = signal<File | null>(null);
  selectedModel = 'claude-sonnet-4-5-20250514';

  modelOptions = [
    { value: 'claude-sonnet-4-5-20250514', label: 'Claude Sonnet 4.5 (Fast)' },
    { value: 'claude-opus-4-5-20250514', label: 'Claude Opus 4.5 (Best)' },
    { value: 'claude-haiku-3-5-20241022', label: 'Claude Haiku 3.5 (Budget)' }
  ];

  // OEM selection state for multi-OEM filtering
  selectedOemIndices = signal<Set<number>>(new Set());

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile.set(input.files[0]);
      this.extractedData.set(null);
      this.extractionMeta.set(null);
    }
  }

  onExtract() {
    const file = this.selectedFile();
    if (!file || !this.tenderId) return;

    this.extracting.set(true);
    this.tenderService.extractFromPDF(this.tenderId, file, this.selectedModel).subscribe({
      next: (response: ExtractionResponse) => {
        this.extractedData.set(response.extracted_data);
        this.extractionMeta.set({
          model_used: response.model_used,
          tokens_used: response.tokens_used
        });
        // Select all OEMs by default
        const oemCount = response.extracted_data.oem_requirements?.length || 0;
        const allIndices = new Set<number>();
        for (let i = 0; i < oemCount; i++) allIndices.add(i);
        this.selectedOemIndices.set(allIndices);
        this.extracting.set(false);
        this.snackBar.open('Extraction complete! Review the results below.', 'Close', { duration: 5000 });
      },
      error: (error) => {
        console.error('Extraction error:', error);
        this.extracting.set(false);
        this.snackBar.open(error.error?.detail || 'Extraction failed', 'Close', { duration: 5000 });
      }
    });
  }

  toggleOem(index: number) {
    const current = new Set(this.selectedOemIndices());
    if (current.has(index)) {
      current.delete(index);
    } else {
      current.add(index);
    }
    this.selectedOemIndices.set(current);
  }

  isOemSelected(index: number): boolean {
    return this.selectedOemIndices().has(index);
  }

  onApply() {
    const data = this.extractedData();
    if (!data || !this.tenderId) return;

    // Filter OEM requirements to only selected ones
    const filteredData = { ...data };
    if (filteredData.oem_requirements) {
      filteredData.oem_requirements = filteredData.oem_requirements.filter(
        (_, i) => this.selectedOemIndices().has(i)
      );
    }

    this.applying.set(true);
    this.tenderService.applyExtraction(this.tenderId, filteredData).subscribe({
      next: (response: ApplyExtractionResponse) => {
        this.applying.set(false);
        const parts = [
          `${response.fields_updated.length} fields updated`,
          response.criteria_created ? `${response.criteria_created} criteria` : null,
          response.emd_created ? 'EMD created' : null,
          response.fees_created ? `${response.fees_created} fees` : null,
          response.oem_requirements_created ? `${response.oem_requirements_created} OEMs` : null
        ].filter(Boolean);
        this.snackBar.open(`Applied: ${parts.join(', ')}`, 'Close', { duration: 5000 });
      },
      error: (error) => {
        console.error('Apply error:', error);
        this.applying.set(false);
        this.snackBar.open('Failed to apply extraction', 'Close', { duration: 3000 });
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
