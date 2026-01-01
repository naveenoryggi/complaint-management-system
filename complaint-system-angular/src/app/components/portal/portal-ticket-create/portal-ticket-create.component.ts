import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PortalService } from '../../../services/portal.service';
import { PortalAuthService } from '../../../services/portal-auth.service';
import {
  PortalCategory,
  PortalPriority,
  PortalProductCategory,
  PortalProduct,
  PortalBrand,
  PortalLocation,
  PortalAsset
} from '../../../models/portal.model';

@Component({
  selector: 'app-portal-ticket-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './portal-ticket-create.component.html',
  styleUrls: ['./portal-ticket-create.component.scss']
})
export class PortalTicketCreateComponent implements OnInit {
  ticketForm: FormGroup;

  isLoading = signal(false);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Lookups
  categories = signal<PortalCategory[]>([]);
  priorities = signal<PortalPriority[]>([]);
  locations = signal<PortalLocation[]>([]);
  assets = signal<PortalAsset[]>([]);

  // Product lookups
  productCategories = signal<PortalProductCategory[]>([]);
  products = signal<PortalProduct[]>([]);
  brands = signal<PortalBrand[]>([]);

  // Filtered products based on selection
  filteredProducts = signal<PortalProduct[]>([]);

  user = computed(() => this.authService.user());

  constructor(
    private fb: FormBuilder,
    private portalService: PortalService,
    private authService: PortalAuthService,
    private router: Router
  ) {
    this.ticketForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(200)]],
      description: ['', [Validators.required, Validators.minLength(20)]],
      categoryId: [''],
      priorityId: [''],
      locationId: [''],
      assetId: [''],
      // Product selection fields
      productCategoryId: [''],
      brandId: [''],
      productId: ['']
    });
  }

  ngOnInit(): void {
    this.loadLookups();
    this.setupFormSubscriptions();
  }

  loadLookups(): void {
    this.isLoading.set(true);

    // Load all lookups in parallel
    this.portalService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: (err) => console.error('Error loading categories:', err)
    });

    this.portalService.getPriorities().subscribe({
      next: (data) => this.priorities.set(data),
      error: (err) => console.error('Error loading priorities:', err)
    });

    this.portalService.getLocations().subscribe({
      next: (data) => this.locations.set(data),
      error: (err) => console.error('Error loading locations:', err)
    });

    // Product lookups
    this.portalService.getProductCategories().subscribe({
      next: (data) => {
        this.productCategories.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading product categories:', err);
        this.isLoading.set(false);
      }
    });

    this.portalService.getBrands().subscribe({
      next: (data) => this.brands.set(data),
      error: (err) => console.error('Error loading brands:', err)
    });

    // Load all products initially
    this.portalService.getProducts().subscribe({
      next: (data) => {
        this.products.set(data);
        this.filteredProducts.set(data);
      },
      error: (err) => console.error('Error loading products:', err)
    });
  }

  setupFormSubscriptions(): void {
    // When location changes, reload assets for that location
    this.ticketForm.get('locationId')?.valueChanges.subscribe(locationId => {
      if (locationId) {
        this.portalService.getAssets(locationId).subscribe({
          next: (data) => this.assets.set(data),
          error: (err) => console.error('Error loading assets:', err)
        });
      } else {
        this.assets.set([]);
      }
      // Reset asset selection
      this.ticketForm.patchValue({ assetId: '' });
    });

    // When product category changes, filter products
    this.ticketForm.get('productCategoryId')?.valueChanges.subscribe(categoryId => {
      this.filterProducts();
      // Reset product selection when category changes
      this.ticketForm.patchValue({ productId: '' });
    });

    // When brand changes, filter products
    this.ticketForm.get('brandId')?.valueChanges.subscribe(brandId => {
      this.filterProducts();
      // Reset product selection when brand changes
      this.ticketForm.patchValue({ productId: '' });
    });
  }

  filterProducts(): void {
    const categoryId = this.ticketForm.get('productCategoryId')?.value;
    const brandId = this.ticketForm.get('brandId')?.value;

    let filtered = [...this.products()];

    if (categoryId) {
      filtered = filtered.filter(p => p.categoryId === categoryId);
    }

    if (brandId) {
      filtered = filtered.filter(p => p.brandId === brandId);
    }

    this.filteredProducts.set(filtered);
  }

  onSubmit(): void {
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formValue = this.ticketForm.value;
    const request = {
      title: formValue.title,
      description: formValue.description,
      categoryId: formValue.categoryId || undefined,
      priorityId: formValue.priorityId || undefined,
      locationId: formValue.locationId || undefined,
      assetId: formValue.assetId || undefined,
      productCategoryId: formValue.productCategoryId || undefined,
      productId: formValue.productId || undefined,
      brandId: formValue.brandId || undefined
    };

    this.portalService.createTicket(request).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        if (response.isSuccess && response.data) {
          this.successMessage.set(`Ticket ${response.data.ticketNumber} created successfully!`);
          // Navigate to ticket detail after a short delay
          setTimeout(() => {
            this.router.navigate(['/portal/tickets', response.data!.id]);
          }, 1500);
        } else {
          this.errorMessage.set(response.message || 'Failed to create ticket.');
        }
      },
      error: (error) => {
        this.isSubmitting.set(false);
        console.error('Error creating ticket:', error);
        this.errorMessage.set('An error occurred while creating the ticket. Please try again.');
      }
    });
  }

  // Form field getters
  get title() { return this.ticketForm.get('title'); }
  get description() { return this.ticketForm.get('description'); }
}
