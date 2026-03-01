import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime } from 'rxjs/operators';
import {
  ReferenceBundleService,
  ReferenceBundle,
  BundleListResponse
} from '../../services/reference-bundle.service';

@Component({
  selector: 'app-reference-bundles',
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
    MatPaginatorModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './reference-bundles.component.html',
  styleUrls: ['./reference-bundles.component.css']
})
export class ReferenceBundlesComponent implements OnInit {
  private bundleService = inject(ReferenceBundleService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  bundles = signal<ReferenceBundle[]>([]);
  totalBundles = signal(0);
  loading = signal(false);
  showCreateForm = signal(false);

  // Pagination
  pageSize = signal(20);
  pageIndex = signal(0);

  // Filters
  searchControl = new FormControl('');
  clientTypeControl = new FormControl('');
  statusControl = new FormControl('');

  createForm!: FormGroup;

  displayedColumns: string[] = [
    'bundle_name',
    'client_name',
    'client_type',
    'project_name',
    'contract_value',
    'status',
    'completeness_score',
    'actions'
  ];

  clientTypes = [
    { value: '', label: 'All Types' },
    { value: 'government', label: 'Government' },
    { value: 'psu', label: 'PSU' },
    { value: 'private', label: 'Private' },
    { value: 'semi_government', label: 'Semi-Government' }
  ];

  statusOptions = [
    { value: '', label: 'All Status' },
    { value: 'active', label: 'Active' },
    { value: 'completed', label: 'Completed' },
    { value: 'archived', label: 'Archived' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      bundle_name: ['', Validators.required],
      client_name: ['', Validators.required],
      client_type: [''],
      project_name: [''],
      work_order_number: [''],
      contract_value: [null],
      scope_description: ['']
    });

    this.loadBundles();

    this.searchControl.valueChanges.pipe(
      debounceTime(300)
    ).subscribe(() => {
      this.pageIndex.set(0);
      this.loadBundles();
    });

    this.clientTypeControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadBundles();
    });

    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadBundles();
    });
  }

  loadBundles() {
    this.loading.set(true);
    const clientType = this.clientTypeControl.value || undefined;
    const status = this.statusControl.value || undefined;
    const search = this.searchControl.value || undefined;

    this.bundleService.listBundles(
      this.pageIndex() + 1,
      this.pageSize(),
      clientType,
      status,
      search
    ).subscribe({
      next: (response: BundleListResponse) => {
        this.bundles.set(response.items);
        this.totalBundles.set(response.total);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading bundles:', error);
        this.snackBar.open('Failed to load reference bundles', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    this.pageIndex.set(0);
    this.loadBundles();
  }

  onFilterChange() {
    this.pageIndex.set(0);
    this.loadBundles();
  }

  onPageChange(event: PageEvent) {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadBundles();
  }

  onCreateBundle() {
    if (this.createForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    this.bundleService.createBundle(this.createForm.value).subscribe({
      next: () => {
        this.createForm.reset();
        this.showCreateForm.set(false);
        this.loadBundles();
        this.snackBar.open('Reference bundle created successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error creating bundle:', error);
        this.snackBar.open('Failed to create reference bundle', 'Close', { duration: 3000 });
      }
    });
  }

  onDeleteBundle(bundle: ReferenceBundle) {
    if (confirm(`Are you sure you want to delete bundle "${bundle.bundle_name}"?`)) {
      this.bundleService.deleteBundle(bundle.id).subscribe({
        next: () => {
          this.loadBundles();
          this.snackBar.open('Bundle deleted successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting bundle:', error);
          this.snackBar.open('Failed to delete bundle', 'Close', { duration: 3000 });
        }
      });
    }
  }

  formatCurrency(value: number | undefined): string {
    if (!value) return 'N/A';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(value);
  }

  getClientTypeColor(type: string | undefined): string {
    const colors: { [key: string]: string } = {
      'government': 'primary',
      'psu': 'accent',
      'private': 'default',
      'semi_government': 'warn'
    };
    return colors[type || ''] || 'default';
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'active': 'primary',
      'completed': 'accent',
      'archived': 'default'
    };
    return colors[status] || 'default';
  }
}
