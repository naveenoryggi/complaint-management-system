import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AIService, GenerationType, AIGenerateRequest, AIGenerationResponse } from '../../services/ai.service';
import { QuillModule } from 'ngx-quill';

@Component({
  selector: 'app-ai-generator',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatIconModule,
    MatExpansionModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatDividerModule,
    MatTooltipModule,
    QuillModule
  ],
  templateUrl: './ai-generator.component.html',
  styleUrls: ['./ai-generator.component.css']
})
export class AIGeneratorComponent implements OnInit {
  private fb = inject(FormBuilder);
  private aiService = inject(AIService);
  private snackBar = inject(MatSnackBar);

  generationForm!: FormGroup;
  generationTypes = signal<GenerationType[]>([]);
  loading = signal(false);
  generatedContent = signal<string | null>(null);
  generationResult = signal<AIGenerationResponse | null>(null);
  showAdvanced = signal(false);

  // Quill editor config
  quillModules = {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote', 'code-block'],
      [{ 'header': 1 }, { 'header': 2 }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      [{ 'size': ['small', false, 'large', 'huge'] }],
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'color': [] }, { 'background': [] }],
      [{ 'align': [] }],
      ['clean']
    ]
  };

  ngOnInit() {
    this.initForm();
    this.loadGenerationTypes();
  }

  initForm() {
    this.generationForm = this.fb.group({
      generation_type: ['technical_solution', Validators.required],
      context: this.fb.group({
        tender_title: ['', Validators.required],
        tender_reference: [''],
        issuing_authority: ['', Validators.required],
        company_name: ['', Validators.required],
        company_profile: [''],
        experience: [''],
        requirements: ['', Validators.required],
        scope_of_work: ['', Validators.required],
        company_address: ['']
      }),
      model: ['claude-sonnet-4'],
      max_tokens: [4000],
      temperature: [0.7],
      save_as_document: [true],
      document_name: ['']
    });
  }

  loadGenerationTypes() {
    this.aiService.getGenerationTypes().subscribe({
      next: (types) => {
        this.generationTypes.set(types);
      },
      error: (error) => {
        console.error('Error loading generation types:', error);
        this.snackBar.open('Failed to load generation types', 'Close', { duration: 3000 });
      }
    });
  }

  onGenerateDocument() {
    if (this.generationForm.valid) {
      this.loading.set(true);
      this.generatedContent.set(null);
      this.generationResult.set(null);

      const request: AIGenerateRequest = this.generationForm.value;

      // Convert requirements from string to array if needed
      if (typeof request.context.requirements === 'string') {
        const reqText = request.context.requirements as string;
        request.context.requirements = reqText.split('\n').filter(r => r.trim());
      }

      this.aiService.generateDocument(request).subscribe({
        next: (result) => {
          this.loading.set(false);
          this.generationResult.set(result);
          this.generatedContent.set(result.content);

          this.snackBar.open(
            `Document generated successfully! Tokens used: ${result.tokens_used}`,
            'Close',
            { duration: 5000 }
          );
        },
        error: (error) => {
          this.loading.set(false);
          console.error('Error generating document:', error);

          let errorMessage = 'Failed to generate document';
          if (error.error?.detail) {
            errorMessage = error.error.detail;
          }

          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
        }
      });
    } else {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    }
  }

  onPreviewPrompt() {
    if (this.generationForm.get('generation_type')?.valid && this.generationForm.get('context')?.valid) {
      const generationType = this.generationForm.get('generation_type')?.value;
      const context = this.generationForm.get('context')?.value;

      this.aiService.previewPrompt(generationType, context).subscribe({
        next: (preview) => {
          const message = `Estimated input tokens: ${preview.estimated_input_tokens}\n\nPrompt preview:\n${preview.formatted_prompt.substring(0, 500)}...`;
          alert(message);
        },
        error: (error) => {
          console.error('Error previewing prompt:', error);
          this.snackBar.open('Failed to preview prompt', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onRegenerateDocument() {
    if (this.generationResult()) {
      this.onGenerateDocument();
    }
  }

  onSaveEditedContent() {
    const editedContent = this.generatedContent();
    if (editedContent) {
      // TODO: Implement save edited content to document library
      this.snackBar.open('Save edited content functionality - To be implemented', 'Close', { duration: 3000 });
    }
  }

  onDownloadDocument() {
    const result = this.generationResult();
    if (result && result.document_id) {
      // TODO: Implement download document
      this.snackBar.open('Download functionality - To be implemented', 'Close', { duration: 3000 });
    }
  }

  getModelCostInfo(model: string): string {
    const costs: { [key: string]: string } = {
      'claude-sonnet-4': 'Balanced ($3/$15 per 1M tokens)',
      'claude-opus-4': 'Highest quality ($15/$75 per 1M tokens)',
      'claude-opus-4-5': 'Latest model ($15/$75 per 1M tokens)'
    };
    return costs[model] || 'Unknown model';
  }

  getGenerationTypeDescription(): string {
    const selectedType = this.generationForm.get('generation_type')?.value;
    const type = this.generationTypes().find(t => t.value === selectedType);
    return type?.description || '';
  }

  get contextForm() {
    return this.generationForm.get('context') as FormGroup;
  }
}
