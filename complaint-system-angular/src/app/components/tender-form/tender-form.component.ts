import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { TenderService, Tender, TenderCreate, TenderUpdate } from '../../services/tender.service';
import { TrackingService, PortalRegistration } from '../../services/tracking.service';

@Component({
  selector: 'app-tender-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDividerModule
  ],
  templateUrl: './tender-form.component.html',
  styleUrls: ['./tender-form.component.css']
})
export class TenderFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tenderService = inject(TenderService);
  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);

  tenderForm!: FormGroup;
  loading = signal(false);
  isEditMode = signal(false);
  tenderId = signal<string | null>(null);
  extracting = signal(false);
  uploadedFiles = signal<File[]>([]);
  extractionDone = signal(false);

  statusOptions = [
    { value: 'draft', label: 'Draft' },
    { value: 'in_progress', label: 'In Progress' },
    { value: 'submitted', label: 'Submitted' },
    { value: 'won', label: 'Won' },
    { value: 'lost', label: 'Lost' },
    { value: 'cancelled', label: 'Cancelled' }
  ];

  defaultPortals = [
    'GeM (Government e-Marketplace)',
    'CPPP (Central Public Procurement Portal)',
    'National e-Procurement Portal',
    'State e-Procurement Portal',
    'Other'
  ];

  portalOptions: string[] = [...this.defaultPortals];

  ngOnInit() {
    this.initForm();
    this.checkEditMode();
    this.loadPortalNames();
  }

  loadPortalNames() {
    this.trackingService.listPortals().subscribe({
      next: (portals) => {
        if (portals.length > 0) {
          const portalNames = portals
            .map(p => p.portal_name)
            .filter((name, index, self) => self.indexOf(name) === index);
          this.portalOptions = [...portalNames, ...this.defaultPortals.filter(d => !portalNames.includes(d))];
        }
      },
      error: () => {} // Silently fall back to defaults
    });
  }

  initForm() {
    this.tenderForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(500)]],
      reference_number: ['', Validators.maxLength(100)],
      issuing_authority: ['', Validators.maxLength(300)],
      portal_name: [''],
      portal_url: [''],
      deadline: [null],
      estimated_value: [null, Validators.min(0)],
      requirements: [''],
      notes: [''],
      status: ['draft', Validators.required]
    });
  }

  checkEditMode() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.tenderId.set(id);
      this.loadTender(id);
    }
  }

  loadTender(id: string) {
    this.loading.set(true);

    this.tenderService.getTender(id).subscribe({
      next: (tender: Tender) => {
        this.tenderForm.patchValue({
          title: tender.title,
          reference_number: tender.reference_number,
          issuing_authority: tender.issuing_authority,
          portal_name: tender.portal_name,
          portal_url: tender.portal_url,
          deadline: tender.deadline ? new Date(tender.deadline) : null,
          estimated_value: tender.estimated_value,
          requirements: this.formatRequirementsForEdit(tender.requirements),
          notes: tender.notes,
          status: tender.status
        });
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

  formatRequirementsForEdit(requirements: any): string {
    if (!requirements) return '';
    if (typeof requirements === 'string') return requirements;

    // If it's an object with arrays, format nicely
    if (typeof requirements === 'object') {
      let formatted = '';
      for (const [key, value] of Object.entries(requirements)) {
        if (Array.isArray(value)) {
          formatted += `${key}:\n${value.map(v => `- ${v}`).join('\n')}\n\n`;
        } else {
          formatted += `${key}: ${value}\n`;
        }
      }
      return formatted;
    }

    return JSON.stringify(requirements, null, 2);
  }

  parseRequirements(requirementsText: string): any {
    if (!requirementsText.trim()) return null;

    try {
      // Try to parse as JSON first
      return JSON.parse(requirementsText);
    } catch {
      // If not JSON, structure it as sections
      const lines = requirementsText.split('\n');
      const requirements: any = { items: [] };
      let currentSection = 'items';

      for (const line of lines) {
        const trimmed = line.trim();
        if (!trimmed) continue;

        // Check if it's a section header (ends with :)
        if (trimmed.endsWith(':') && !trimmed.startsWith('-')) {
          currentSection = trimmed.slice(0, -1).toLowerCase().replace(/\s+/g, '_');
          requirements[currentSection] = [];
        } else if (trimmed.startsWith('-')) {
          // It's a list item
          const item = trimmed.substring(1).trim();
          if (!requirements[currentSection]) {
            requirements[currentSection] = [];
          }
          requirements[currentSection].push(item);
        } else {
          // Regular text
          if (!requirements[currentSection]) {
            requirements[currentSection] = [];
          }
          requirements[currentSection].push(trimmed);
        }
      }

      return requirements;
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);
      this.uploadedFiles.set(files);
      this.extractFromFiles(files);
    }
  }

  extractFromFiles(files: File[]) {
    this.extracting.set(true);
    this.snackBar.open('Extracting tender details from PDF... This may take a moment.', 'Close', { duration: 10000 });

    this.tenderService.extractPreview(files).subscribe({
      next: (response) => {
        this.extracting.set(false);
        this.extractionDone.set(true);
        const data = response.extracted_data;

        // Auto-fill form fields from extracted data
        if (data.title) this.tenderForm.patchValue({ title: data.title });
        if (data.reference_number) this.tenderForm.patchValue({ reference_number: data.reference_number });
        if (data.issuing_authority) this.tenderForm.patchValue({ issuing_authority: data.issuing_authority });
        if (data.deadline) {
          try {
            this.tenderForm.patchValue({ deadline: new Date(data.deadline) });
          } catch {}
        }
        if (data.estimated_value) this.tenderForm.patchValue({ estimated_value: data.estimated_value });

        // Build requirements text from extracted arrays
        const reqParts: string[] = [];
        if (data.eligibility_criteria?.length) {
          reqParts.push('Eligibility Criteria:');
          data.eligibility_criteria.forEach((c: any) => {
            reqParts.push(`- ${typeof c === 'string' ? c : c.description || JSON.stringify(c)}`);
          });
          reqParts.push('');
        }
        if (data.technical_requirements?.length) {
          reqParts.push('Technical Requirements:');
          data.technical_requirements.forEach((r: any) => {
            reqParts.push(`- ${typeof r === 'string' ? r : r.description || JSON.stringify(r)}`);
          });
          reqParts.push('');
        }
        if (data.special_conditions?.length) {
          reqParts.push('Special Conditions:');
          data.special_conditions.forEach((s: any) => reqParts.push(`- ${s}`));
          reqParts.push('');
        }
        if (reqParts.length) {
          this.tenderForm.patchValue({ requirements: reqParts.join('\n') });
        }

        // Build notes from other extracted info
        const noteParts: string[] = [];
        if (data.emd) {
          noteParts.push(`EMD: ₹${data.emd.amount?.toLocaleString('en-IN') || 'N/A'} (${data.emd.mode || 'N/A'})`);
        }
        if (data.important_dates) {
          noteParts.push('Important Dates:');
          for (const [key, val] of Object.entries(data.important_dates)) {
            noteParts.push(`  ${key}: ${val}`);
          }
        }
        if (data.contact_info) {
          noteParts.push(`Contact: ${JSON.stringify(data.contact_info)}`);
        }
        if (noteParts.length) {
          this.tenderForm.patchValue({ notes: noteParts.join('\n') });
        }

        // Store full extracted data for later use (after tender creation)
        (this as any)._extractedData = data;

        const fieldsPopulated = [data.title, data.reference_number, data.issuing_authority, data.deadline, data.estimated_value]
          .filter(Boolean).length;
        this.snackBar.open(`Extracted ${fieldsPopulated} fields from tender document`, 'Close', { duration: 5000 });
      },
      error: (error) => {
        this.extracting.set(false);
        const detail = error.error?.detail || 'Failed to extract data from PDF';
        this.snackBar.open(detail, 'Close', { duration: 5000 });
      }
    });
  }

  onSubmit() {
    if (this.tenderForm.valid) {
      this.loading.set(true);

      const formValue = this.tenderForm.value;

      // Parse requirements
      const requirements = this.parseRequirements(formValue.requirements || '');

      // Format deadline as ISO string
      const deadline = formValue.deadline ? new Date(formValue.deadline).toISOString() : undefined;

      if (this.isEditMode()) {
        // Update existing tender
        const updateData: TenderUpdate = {
          title: formValue.title,
          reference_number: formValue.reference_number || undefined,
          issuing_authority: formValue.issuing_authority || undefined,
          portal_name: formValue.portal_name || undefined,
          portal_url: formValue.portal_url || undefined,
          deadline: deadline,
          estimated_value: formValue.estimated_value || undefined,
          requirements: requirements,
          notes: formValue.notes || undefined,
          status: formValue.status
        };

        this.tenderService.updateTender(this.tenderId()!, updateData).subscribe({
          next: (tender) => {
            this.loading.set(false);
            this.snackBar.open('Tender updated successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/tenders', tender.id]);
          },
          error: (error) => {
            console.error('Error updating tender:', error);
            this.snackBar.open('Failed to update tender', 'Close', { duration: 3000 });
            this.loading.set(false);
          }
        });
      } else {
        // Create new tender
        const createData: TenderCreate = {
          title: formValue.title,
          reference_number: formValue.reference_number || undefined,
          issuing_authority: formValue.issuing_authority || undefined,
          portal_name: formValue.portal_name || undefined,
          portal_url: formValue.portal_url || undefined,
          deadline: deadline,
          estimated_value: formValue.estimated_value || undefined,
          requirements: requirements,
          notes: formValue.notes || undefined,
          status: formValue.status || 'draft'
        };

        this.tenderService.createTender(createData).subscribe({
          next: (tender) => {
            // If we have uploaded files, run full extraction to create EMD/checklist/criteria records
            if (this.uploadedFiles().length > 0 && this.extractionDone()) {
              this.snackBar.open('Tender created! Applying extraction data...', 'Close', { duration: 3000 });
              this.tenderService.extractFromPDF(tender.id, this.uploadedFiles()).subscribe({
                next: () => {
                  this.loading.set(false);
                  this.snackBar.open('Tender created with full extraction applied', 'Close', { duration: 3000 });
                  this.router.navigate(['/tenders', tender.id]);
                },
                error: () => {
                  this.loading.set(false);
                  this.snackBar.open('Tender created (extraction apply failed — run manually from workspace)', 'Close', { duration: 5000 });
                  this.router.navigate(['/tenders', tender.id]);
                }
              });
            } else {
              this.loading.set(false);
              this.snackBar.open('Tender created successfully', 'Close', { duration: 3000 });
              this.router.navigate(['/tenders', tender.id]);
            }
          },
          error: (error) => {
            console.error('Error creating tender:', error);
            this.snackBar.open('Failed to create tender', 'Close', { duration: 3000 });
            this.loading.set(false);
          }
        });
      }
    } else {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    }
  }

  onCancel() {
    this.router.navigate(['/tenders']);
  }

  formatCurrency(value: number | null): string {
    if (!value) return '';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }
}
