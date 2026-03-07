import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { TrackingService, OEMMaster, OEMTenderRequirement } from '../../../services/tracking.service';

interface ExtractedOEM {
  oem_name: string;
  product_category?: string;
  maf_required?: boolean;
  make_model?: string;
  is_approved_make?: boolean;
}

@Component({
  selector: 'app-oem-requirements-tab',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './oem-requirements-tab.component.html',
  styleUrls: ['./oem-requirements-tab.component.css']
})
export class OemRequirementsTabComponent implements OnInit, OnChanges {
  @Input() tenderId!: string;
  @Input() tenderRequirements: any;
  @Output() navigateToTab = new EventEmitter<number>();

  private trackingService = inject(TrackingService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  requirements = signal<OEMTenderRequirement[]>([]);
  oems = signal<OEMMaster[]>([]);
  loading = signal(false);
  showForm = signal(false);
  showNewOemForm = signal(false);
  addingOemId = signal<string | null>(null);
  importingIndex = signal<number | null>(null);
  importingAll = signal(false);

  // Extracted OEMs from tender requirements JSON
  extractedOems = signal<ExtractedOEM[]>([]);

  // Quick-add search
  searchControl = new FormControl('');
  searchQuery = signal('');

  // Filtered OEMs: those not already added to this tender
  availableOems = computed(() => {
    const allOems = this.oems();
    const existingOemIds = new Set(this.requirements().map(r => r.oem_id));
    const query = this.searchQuery().toLowerCase();
    return allOems
      .filter(o => !existingOemIds.has(o.id))
      .filter(o => !query || o.name.toLowerCase().includes(query)
        || (o.country && o.country.toLowerCase().includes(query))
        || (o.product_categories && o.product_categories.some((c: string) => c.toLowerCase().includes(query)))
      );
  });

  // Extracted OEMs that haven't been imported yet
  unimportedExtractedOems = computed(() => {
    const existingNames = new Set(
      this.requirements().map(r => this.getOEMName(r.oem_id).toLowerCase())
    );
    return this.extractedOems().filter(
      e => !existingNames.has(e.oem_name.toLowerCase())
    );
  });

  createForm!: FormGroup;
  newOemForm!: FormGroup;

  statusOptions = [
    { value: 'pending', label: 'Pending' },
    { value: 'received', label: 'Received' },
    { value: 'not_required', label: 'Not Required' },
    { value: 'expired', label: 'Expired' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      oem_id: ['', Validators.required],
      maf_status: ['pending'],
      ms_status: ['pending'],
      partner_cert_status: ['pending'],
      datasheet_status: ['pending'],
      compliance_cert_status: ['pending'],
      notes: ['']
    });

    this.newOemForm = this.fb.group({
      name: ['', Validators.required],
      country: ['India'],
      product_category: [''],
      notes: ['']
    });

    this.searchControl.valueChanges.subscribe(val => {
      this.searchQuery.set(val || '');
    });

    this.loadOEMs();
    if (this.tenderId) {
      this.loadRequirements();
    }
    this.parseExtractedOems();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['tenderRequirements'] && !changes['tenderRequirements'].firstChange) {
      this.parseExtractedOems();
    }
  }

  private parseExtractedOems() {
    const oems: ExtractedOEM[] = [];

    // 1. Direct oem_requirements from extraction (best source)
    if (this.tenderRequirements?.oem_requirements?.length) {
      for (const o of this.tenderRequirements.oem_requirements) {
        if (o.oem_name) {
          oems.push(o);
        }
      }
    }

    // 2. If no direct OEMs, heuristically scan other extracted fields (zero AI tokens)
    if (oems.length === 0 && this.tenderRequirements) {
      const detected = this.heuristicOemScan(this.tenderRequirements);
      oems.push(...detected);
    }

    this.extractedOems.set(oems);
  }

  /**
   * Heuristic OEM detection from already-extracted requirements text.
   * Scans technical_requirements, document_checklist, eligibility_criteria, special_conditions
   * for OEM/manufacturer/brand references. Zero AI calls — pure regex + keyword matching.
   */
  private heuristicOemScan(req: any): ExtractedOEM[] {
    const found: ExtractedOEM[] = [];
    const seenNames = new Set<string>();

    // Collect all text strings from requirements
    const texts: string[] = [];
    for (const key of ['technical_requirements', 'eligibility_criteria', 'special_conditions', 'document_checklist']) {
      const val = req[key];
      if (Array.isArray(val)) {
        for (const item of val) {
          if (typeof item === 'string') texts.push(item);
          else if (item?.description) texts.push(item.description);
          else if (item?.name) texts.push(item.name);
        }
      } else if (typeof val === 'string') {
        texts.push(val);
      }
    }

    // Pattern 1: "make: XYZ" or "brand: XYZ" or "manufacturer: XYZ"
    const makePattern = /(?:make|brand|manufacturer|oem)\s*[:=]\s*([A-Z][A-Za-z\s&.]+?)(?:\s*[,;/]|\s+or\s|\s*$)/gi;
    for (const text of texts) {
      let match;
      while ((match = makePattern.exec(text)) !== null) {
        const name = match[1].trim();
        if (name.length > 2 && name.length < 60 && !seenNames.has(name.toLowerCase())) {
          seenNames.add(name.toLowerCase());
          found.push({ oem_name: name, product_category: this.guessCategory(text), maf_required: true });
        }
      }
    }

    // Pattern 2: Known brand names in text
    const knownBrands = [
      'Cisco', 'HP', 'HPE', 'Dell', 'Lenovo', 'IBM', 'Microsoft', 'Oracle', 'SAP',
      'Juniper', 'Fortinet', 'Palo Alto', 'Check Point', 'SonicWall',
      'VMware', 'Red Hat', 'Citrix', 'Nutanix', 'NetApp',
      'Huawei', 'ZTE', 'Samsung', 'LG', 'Sony', 'Epson', 'Canon', 'Ricoh',
      'Schneider', 'APC', 'Emerson', 'Vertiv', 'Eaton', 'Luminous',
      'Honeywell', 'Bosch', 'Siemens', 'ABB', 'Havells', 'Crompton',
      'D-Link', 'TP-Link', 'Aruba', 'Ruckus', 'Cambium',
      'Poly', 'Logitech', 'Jabra', 'Yealink', 'Grandstream',
      'Tata', 'HCL', 'Wipro', 'Infosys', 'TCS',
      'Acer', 'ASUS', 'BenQ', 'ViewSonic', 'Epson',
      'Hikvision', 'Dahua', 'CP Plus', 'Pelco',
      'Tyco', 'UTC', 'Panasonic', 'NEC', 'Hitachi',
    ];

    const fullText = texts.join(' ');
    for (const brand of knownBrands) {
      if (seenNames.has(brand.toLowerCase())) continue;
      // Word boundary check
      const regex = new RegExp(`\\b${brand.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\b`, 'i');
      if (regex.test(fullText)) {
        seenNames.add(brand.toLowerCase());
        found.push({
          oem_name: brand,
          product_category: this.guessCategory(fullText.substring(
            Math.max(0, fullText.toLowerCase().indexOf(brand.toLowerCase()) - 50),
            fullText.toLowerCase().indexOf(brand.toLowerCase()) + brand.length + 50
          )),
          maf_required: true,
          is_approved_make: true,
        });
      }
    }

    // Pattern 3: "MAF" or "Manufacturer Authorization" mentions — flag that OEM auth is needed
    const mafPattern = /(?:MAF|manufacturer\s+authoriz|OEM\s+authoriz|authorized\s+dealer)/gi;
    if (mafPattern.test(fullText) && found.length === 0) {
      // MAF is mentioned but no specific OEM found — add a generic hint
      found.push({
        oem_name: 'OEM Authorization Required',
        product_category: 'Check tender for specific OEM/brand names',
        maf_required: true,
      });
    }

    return found;
  }

  private guessCategory(context: string): string {
    const lower = context.toLowerCase();
    const categories: [string, string[]][] = [
      ['Networking', ['switch', 'router', 'network', 'lan', 'wan', 'firewall', 'wifi', 'access point']],
      ['Server / Storage', ['server', 'storage', 'nas', 'san', 'rack', 'blade']],
      ['Computing', ['desktop', 'laptop', 'workstation', 'pc', 'computer', 'tablet']],
      ['UPS / Power', ['ups', 'power', 'battery', 'inverter', 'stabilizer']],
      ['Security / CCTV', ['cctv', 'camera', 'surveillance', 'dvr', 'nvr', 'access control']],
      ['Software', ['software', 'license', 'os ', 'operating system', 'erp', 'database']],
      ['Printing', ['printer', 'scanner', 'copier', 'mfp', 'multifunct']],
      ['Display / AV', ['display', 'monitor', 'projector', 'led wall', 'video conference']],
      ['Telecom', ['telephone', 'pbx', 'ip phone', 'voip', 'intercom']],
    ];
    for (const [cat, keywords] of categories) {
      if (keywords.some(kw => lower.includes(kw))) return cat;
    }
    return '';
  }

  loadOEMs() {
    this.trackingService.listOEMs().subscribe({
      next: (data) => this.oems.set(data),
      error: (err) => { console.error('Error loading OEMs:', err); }
    });
  }

  loadRequirements() {
    this.loading.set(true);
    this.trackingService.listOEMRequirements(this.tenderId).subscribe({
      next: (data) => {
        this.requirements.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading OEM requirements:', error);
        this.loading.set(false);
      }
    });
  }

  // Import a single extracted OEM: create in master + link to tender
  onImportExtractedOem(extractedOem: ExtractedOEM, index: number) {
    this.importingIndex.set(index);

    // Check if OEM already exists in master list by name
    const existingOem = this.oems().find(
      o => o.name.toLowerCase() === extractedOem.oem_name.toLowerCase()
    );

    if (existingOem) {
      // OEM exists, just create the tender requirement
      this.createRequirementForOem(existingOem.id, extractedOem);
    } else {
      // Create new OEM in master list first
      const oemData: Partial<OEMMaster> = {
        name: extractedOem.oem_name,
        product_categories: extractedOem.product_category ? [extractedOem.product_category] : [],
      };

      this.trackingService.createOEM(oemData).subscribe({
        next: (newOem) => {
          this.createRequirementForOem(newOem.id, extractedOem);
          this.loadOEMs();
        },
        error: (err) => {
          console.error('Error creating OEM:', err);
          this.importingIndex.set(null);
          this.snackBar.open(err.error?.detail || 'Failed to create OEM', 'Close', { duration: 5000 });
        }
      });
    }
  }

  private createRequirementForOem(oemId: string, extractedOem: ExtractedOEM) {
    const noteParts: string[] = [];
    if (extractedOem.product_category) noteParts.push(`Product: ${extractedOem.product_category}`);
    if (extractedOem.make_model) noteParts.push(`Model: ${extractedOem.make_model}`);
    if (extractedOem.is_approved_make) noteParts.push('Approved make');

    const data = {
      oem_id: oemId,
      maf_status: extractedOem.maf_required ? 'pending' : 'not_required',
      ms_status: 'pending',
      partner_cert_status: 'pending',
      datasheet_status: 'pending',
      compliance_cert_status: 'pending',
      notes: noteParts.join(' | ') || 'Imported from extraction'
    };

    this.trackingService.createOEMRequirement(this.tenderId, data).subscribe({
      next: () => {
        this.importingIndex.set(null);
        this.loadRequirements();
        this.snackBar.open(`${extractedOem.oem_name} imported`, 'Close', { duration: 2000 });
      },
      error: (err) => {
        console.error('Error importing OEM:', err);
        this.importingIndex.set(null);
        this.snackBar.open(err.error?.detail || 'Failed to import OEM', 'Close', { duration: 5000 });
      }
    });
  }

  // Import all unimported extracted OEMs at once
  onImportAllExtracted() {
    const toImport = this.unimportedExtractedOems();
    if (toImport.length === 0) return;

    this.importingAll.set(true);
    let completed = 0;
    const total = toImport.length;

    for (const extractedOem of toImport) {
      const existingOem = this.oems().find(
        o => o.name.toLowerCase() === extractedOem.oem_name.toLowerCase()
      );

      const importOne = (oemId: string) => {
        const noteParts: string[] = [];
        if (extractedOem.product_category) noteParts.push(`Product: ${extractedOem.product_category}`);
        if (extractedOem.make_model) noteParts.push(`Model: ${extractedOem.make_model}`);
        if (extractedOem.is_approved_make) noteParts.push('Approved make');

        const data = {
          oem_id: oemId,
          maf_status: extractedOem.maf_required ? 'pending' : 'not_required',
          ms_status: 'pending',
          partner_cert_status: 'pending',
          datasheet_status: 'pending',
          compliance_cert_status: 'pending',
          notes: noteParts.join(' | ') || 'Imported from extraction'
        };

        this.trackingService.createOEMRequirement(this.tenderId, data).subscribe({
          next: () => {
            completed++;
            if (completed === total) {
              this.importingAll.set(false);
              this.loadOEMs();
              this.loadRequirements();
              this.snackBar.open(`${total} OEM(s) imported successfully`, 'Close', { duration: 3000 });
            }
          },
          error: (err) => {
            console.error('Error importing OEM requirement:', err);
            completed++;
            if (completed === total) {
              this.importingAll.set(false);
              this.loadOEMs();
              this.loadRequirements();
              this.snackBar.open(`Import completed with some errors`, 'Close', { duration: 3000 });
            }
          }
        });
      };

      if (existingOem) {
        importOne(existingOem.id);
      } else {
        const oemData: Partial<OEMMaster> = {
          name: extractedOem.oem_name,
          product_categories: extractedOem.product_category ? [extractedOem.product_category] : [],
        };
        this.trackingService.createOEM(oemData).subscribe({
          next: (newOem) => importOne(newOem.id),
          error: (err) => {
            console.error('Error creating OEM during bulk import:', err);
            completed++;
            if (completed === total) {
              this.importingAll.set(false);
              this.loadOEMs();
              this.loadRequirements();
            }
          }
        });
      }
    }
  }

  // Quick-add: one-click add existing OEM to tender
  onQuickAdd(oem: OEMMaster) {
    this.addingOemId.set(oem.id);
    const data = {
      oem_id: oem.id,
      maf_status: 'pending',
      ms_status: 'pending',
      partner_cert_status: 'pending',
      datasheet_status: 'pending',
      compliance_cert_status: 'pending',
      notes: oem.product_categories?.length ? `Products: ${oem.product_categories.join(', ')}` : ''
    };

    this.trackingService.createOEMRequirement(this.tenderId, data).subscribe({
      next: () => {
        this.addingOemId.set(null);
        this.loadRequirements();
        this.snackBar.open(`${oem.name} added to tender`, 'Close', { duration: 2000 });
      },
      error: (err) => {
        console.error('Error adding OEM:', err);
        this.addingOemId.set(null);
        this.snackBar.open(err.error?.detail || 'Failed to add OEM', 'Close', { duration: 5000 });
      }
    });
  }

  // Create new OEM in master list and immediately add to tender
  onCreateNewOem() {
    if (this.newOemForm.invalid) return;

    const formVal = this.newOemForm.value;
    const oemData: Partial<OEMMaster> = {
      name: formVal.name,
      country: formVal.country || 'India',
      product_categories: formVal.product_category ? [formVal.product_category] : [],
    };

    this.trackingService.createOEM(oemData).subscribe({
      next: (newOem) => {
        const reqData = {
          oem_id: newOem.id,
          maf_status: 'pending',
          ms_status: 'pending',
          partner_cert_status: 'pending',
          datasheet_status: 'pending',
          compliance_cert_status: 'pending',
          notes: formVal.notes || ''
        };
        this.trackingService.createOEMRequirement(this.tenderId, reqData).subscribe({
          next: () => {
            this.newOemForm.reset({ country: 'India' });
            this.showNewOemForm.set(false);
            this.loadOEMs();
            this.loadRequirements();
            this.snackBar.open(`${newOem.name} created and added`, 'Close', { duration: 3000 });
          },
          error: (err) => {
            console.error('Error linking OEM to tender:', err);
            this.loadOEMs();
            this.snackBar.open(err.error?.detail || 'OEM created but failed to link', 'Close', { duration: 5000 });
          }
        });
      },
      error: (err) => {
        console.error('Error creating OEM:', err);
        this.snackBar.open(err.error?.detail || 'Failed to create OEM', 'Close', { duration: 5000 });
      }
    });
  }

  onCreateRequirement() {
    if (this.createForm.invalid) return;

    this.trackingService.createOEMRequirement(this.tenderId, this.createForm.value).subscribe({
      next: () => {
        this.createForm.reset({
          maf_status: 'pending', ms_status: 'pending',
          partner_cert_status: 'pending', datasheet_status: 'pending',
          compliance_cert_status: 'pending'
        });
        this.showForm.set(false);
        this.loadRequirements();
        this.snackBar.open('OEM requirement added', 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error('Error adding requirement:', err);
        this.snackBar.open(err.error?.detail || 'Failed to add requirement', 'Close', { duration: 5000 });
      }
    });
  }

  getOEMName(oemId: string): string {
    return this.oems().find(o => o.id === oemId)?.name || 'Unknown';
  }

  getStatusClass(status: string): string {
    const map: { [key: string]: string } = {
      'received': 'status-received',
      'pending': 'status-pending',
      'not_required': 'status-na',
      'expired': 'status-expired'
    };
    return map[status] || '';
  }

  onCycleStatus(req: OEMTenderRequirement, field: string) {
    const cycle = ['pending', 'received', 'not_required'];
    const current = (req as any)[field] || 'pending';
    const idx = cycle.indexOf(current);
    const next = cycle[(idx + 1) % cycle.length];

    this.trackingService.updateOEMRequirement(req.id, { [field]: next } as any).subscribe({
      next: () => {
        this.loadRequirements();
        this.snackBar.open(`Status updated to ${next.replace('_', ' ')}`, 'Close', { duration: 2000 });
      },
      error: (err) => this.snackBar.open(err.error?.detail || 'Failed to update status', 'Close', { duration: 5000 })
    });
  }

  onDeleteRequirement(req: OEMTenderRequirement) {
    if (confirm(`Remove OEM requirement for "${this.getOEMName(req.oem_id)}"?`)) {
      this.trackingService.deleteOEMRequirement(req.id).subscribe({
        next: () => {
          this.loadRequirements();
          this.snackBar.open('Requirement deleted', 'Close', { duration: 3000 });
        },
        error: (err) => this.snackBar.open(err.error?.detail || 'Failed to delete', 'Close', { duration: 5000 })
      });
    }
  }

  getOEMDetail(oemId: string, field: string): string | null {
    const oem = this.oems().find(o => o.id === oemId);
    if (!oem) return null;
    const val = (oem as any)[field];
    return val || null;
  }

  getStatusIcon(status: string): string {
    const map: Record<string, string> = {
      received: 'check_circle',
      pending: 'schedule',
      not_required: 'remove_circle_outline',
      expired: 'error',
    };
    return map[status] || 'help';
  }

  goToExtraction() {
    this.navigateToTab.emit(2); // Tab index 2 = AI Extraction
  }

  getCompletionPercent(): number {
    const reqs = this.requirements();
    if (reqs.length === 0) return 0;
    const total = reqs.length * 5;
    let received = 0;
    for (const r of reqs) {
      if (r.maf_status === 'received' || r.maf_status === 'not_required') received++;
      if (r.ms_status === 'received' || r.ms_status === 'not_required') received++;
      if (r.partner_cert_status === 'received' || r.partner_cert_status === 'not_required') received++;
      if (r.datasheet_status === 'received' || r.datasheet_status === 'not_required') received++;
      if (r.compliance_cert_status === 'received' || r.compliance_cert_status === 'not_required') received++;
    }
    return Math.round((received / total) * 100);
  }
}
