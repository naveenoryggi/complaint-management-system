import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ComplaintService } from '../../../services/complaint.service';
import { AuthService } from '../../../services/auth.service';
import { CategoryService } from '../../../services/category.service';
import { BranchService } from '../../../services/branch.service';
import { DepartmentService } from '../../../services/department.service';
import { SectionService } from '../../../services/section.service';
import { PriorityMasterService } from '../../../services/priority-master.service';
import { CustomerService } from '../../../services/customer.service';
import { ProjectService } from '../../../services/project.service';
import { ProductService } from '../../../services/product.service';
import { PreferredContactMethod, ComplaintPriority } from '../../../models/complaint.model';
import { CustomerLookup } from '../../../models/customer.model';
import { ProjectLookup } from '../../../models/project.model';
import { CategoryLookup, ProductLookup, CategoryTreeNode } from '../../../models/product.model';
import { MultiSelectDropdownComponent, MultiSelectOption } from '../../shared/multi-select-dropdown/multi-select-dropdown.component';

@Component({
  selector: 'app-complaint-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MultiSelectDropdownComponent],
  templateUrl: './complaint-form.component.html',
  styleUrls: ['./complaint-form.component.scss']
})
export class ComplaintFormComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  complaintForm: FormGroup;
  loading = false;
  error: string | null = null;
  isEditMode = false;
  complaintId: string | null = null;

  // Dropdowns data
  categories: any[] = [];
  branches: any[] = [];
  departments: any[] = [];
  sections: any[] = [];

  // Enterprise fields data
  customers: CustomerLookup[] = [];
  projects: ProjectLookup[] = [];
  parentCategories: CategoryLookup[] = [];  // Top-level categories
  subCategories: CategoryLookup[] = [];     // Child categories of selected parent
  productCategories: CategoryLookup[] = []; // All categories (for backward compatibility)
  products: ProductLookup[] = [];
  isPortalUser = false;
  enterpriseSectionExpanded = true;

  // Hierarchical categories for complaint form with indentation
  flattenedCategories: { id: string; code: string; name: string; level: number; hasChildren: boolean }[] = [];

  // Multi-select options for category and product
  categoryMultiSelectOptions: MultiSelectOption[] = [];
  productMultiSelectOptions: MultiSelectOption[] = [];

  // File upload
  selectedFiles: File[] = [];
  fileUploadError: string | null = null;

  // Enums for template
  PreferredContactMethod = PreferredContactMethod;
  ComplaintPriority = ComplaintPriority;

  // Enum options for dropdowns
  preferredContactOptions = [
    { value: PreferredContactMethod.Email, label: 'Email Only' },
    { value: PreferredContactMethod.Phone, label: 'Phone Only' },
    { value: PreferredContactMethod.Both, label: 'Email & Phone' },
    { value: PreferredContactMethod.SMS, label: 'SMS' },
    { value: PreferredContactMethod.InApp, label: 'In-App Notification' }
  ];

  priorityOptions: { value: string, label: string }[] = [];

  constructor(
    private fb: FormBuilder,
    private complaintService: ComplaintService,
    private authService: AuthService,
    private categoryService: CategoryService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private sectionService: SectionService,
    private priorityMasterService: PriorityMasterService,
    private customerService: CustomerService,
    private projectService: ProjectService,
    private productService: ProductService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.complaintForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      categoryId: ['', Validators.required],
      priorityMasterId: ['', Validators.required],  // Changed from priority enum to Master ID
      branchId: [''],
      departmentId: [''],
      sectionId: [''],
      contactEmail: ['', [Validators.email]],
      contactPhone: [''],
      alternatePhone: [''],
      preferredContactMethod: [PreferredContactMethod.Both],
      isAnonymous: [false],
      tags: [''],
      // Enterprise fields
      customerId: [''],
      projectId: [''],
      parentCategoryIds: [[]],    // Multi-select category IDs (array)
      productCategoryId: [''],   // Subcategory selection (kept for compatibility)
      productIds: [[]]           // Multi-select product IDs (array)
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadBranches();
    this.loadPriorities();
    this.autoPopulateUserInfo();

    // Check if user is portal user (hide project field for portal users)
    const currentUser = this.authService.currentUserValue;
    this.isPortalUser = currentUser?.isPortalUser || false;

    // Load enterprise data
    this.loadCustomers();
    this.loadParentCategories();
    this.loadAllProducts(); // Load all products initially

    // Check if we're in edit mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.complaintId = id;
      this.loadComplaint(id);
    }

    // Add click listener for debugging after view init
    setTimeout(() => {
      this.setupFileInputDebugging();
    }, 1000);
  }

  private setupFileInputDebugging(): void {
    const fileInput = document.getElementById('fileUploadInput') as HTMLInputElement;
    const uploadZone = document.querySelector('.upload-zone');

    console.log('=== FILE INPUT DEBUGGING SETUP ===');
    console.log('File input element found:', !!fileInput);
    console.log('Upload zone element found:', !!uploadZone);

    if (fileInput) {
      console.log('File input properties:');
      console.log('  - type:', fileInput.type);
      console.log('  - accept:', fileInput.accept);
      console.log('  - multiple:', fileInput.multiple);
      console.log('  - disabled:', fileInput.disabled);
      console.log('  - display style:', window.getComputedStyle(fileInput).display);
      console.log('  - visibility:', window.getComputedStyle(fileInput).visibility);
      console.log('  - pointer-events:', window.getComputedStyle(fileInput).pointerEvents);
      console.log('  - z-index:', window.getComputedStyle(fileInput).zIndex);
      console.log('  - position:', window.getComputedStyle(fileInput).position);
      console.log('  - offsetWidth:', fileInput.offsetWidth);
      console.log('  - offsetHeight:', fileInput.offsetHeight);

      // Add direct click listener
      fileInput.addEventListener('click', (e) => {
        console.log('=== FILE INPUT CLICK EVENT ===');
        console.log('Event:', e);
        console.log('Event target:', e.target);
        console.log('Event currentTarget:', e.currentTarget);
        console.log('Default prevented:', e.defaultPrevented);
        console.log('Propagation stopped:', e.cancelBubble);
      });

      // Add focus listener
      fileInput.addEventListener('focus', () => {
        console.log('=== FILE INPUT FOCUSED ===');
      });
    }

    if (uploadZone) {
      console.log('Upload zone properties:');
      console.log('  - pointer-events:', window.getComputedStyle(uploadZone).pointerEvents);
      console.log('  - z-index:', window.getComputedStyle(uploadZone).zIndex);

      uploadZone.addEventListener('click', (e) => {
        console.log('=== UPLOAD ZONE CLICK EVENT ===');
        console.log('Click target:', e.target);
        console.log('Click coordinates:', (e as MouseEvent).clientX, (e as MouseEvent).clientY);
      });
    }
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories = response.data;
        }
      },
      error: (err) => {
        console.error('Failed to load categories', err);
      }
    });
  }

  loadPriorities(): void {
    this.priorityMasterService.getPriorities(undefined, true, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Map ComplaintPriorityMaster to dropdown options
          this.priorityOptions = response.data
            .filter(p => p.isActive)
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map(p => ({
              value: p.id,  // Changed from p.level to p.id (GUID)
              label: p.name
            }));

          // Set default to Medium priority if available
          if (this.priorityOptions.length > 0 && !this.isEditMode) {
            const mediumPriority = this.priorityOptions.find(p => p.label === 'Medium') || this.priorityOptions[Math.floor(this.priorityOptions.length / 2)];
            this.complaintForm.patchValue({ priorityMasterId: mediumPriority.value });
          }
        }
      },
      error: (err) => {
        console.error('Failed to load priorities', err);
      }
    });
  }

  loadBranches(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser && currentUser.companyId) {
      this.branchService.getBranches(currentUser.companyId, true).subscribe({
        next: (branches) => {
          this.branches = branches;
        },
        error: (err) => {
          console.error('Failed to load branches', err);
        }
      });
    }
  }

  loadDepartments(branchId: string): void {
    if (!branchId) {
      this.departments = [];
      this.sections = [];
      return;
    }

    this.departmentService.getDepartments(branchId, true).subscribe({
      next: (departments) => {
        this.departments = departments;
      },
      error: (err) => {
        console.error('Failed to load departments', err);
        this.departments = [];
      }
    });
  }

  loadSections(departmentId: string): void {
    if (!departmentId) {
      this.sections = [];
      return;
    }

    this.sectionService.getSections(departmentId, true).subscribe({
      next: (sections) => {
        this.sections = sections;
      },
      error: (err: any) => {
        console.error('Failed to load sections', err);
        this.sections = [];
      }
    });
  }

  // ============ Enterprise Fields Methods ============

  loadCustomers(): void {
    this.customerService.getCustomerLookup().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.customers = response.data;
        }
      },
      error: (err) => {
        console.error('Failed to load customers', err);
      }
    });
  }

  loadParentCategories(): void {
    // Load category tree for hierarchical display
    this.productService.getCategoryTree().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Flatten tree for dropdown display with indentation info
          this.flattenedCategories = this.flattenCategoryTree(response.data);
          // Also set parentCategories for backward compatibility (top-level only)
          this.parentCategories = response.data.map(c => ({
            id: c.id,
            code: c.code,
            name: c.name,
            fullPath: c.name,
            level: c.level
          }));
          // Create multi-select options from flattened categories
          this.categoryMultiSelectOptions = this.flattenedCategories.map(c => ({
            id: c.id,
            label: c.name,
            code: c.code,
            level: c.level,
            hasChildren: c.hasChildren
          }));
        }
      },
      error: (err) => {
        console.error('Failed to load categories', err);
        // Fallback to flat lookup if tree fails
        this.productService.getCategoryLookup().subscribe({
          next: (fallbackResponse) => {
            if (fallbackResponse.isSuccess && fallbackResponse.data) {
              this.parentCategories = fallbackResponse.data.filter(c => c.level === 0 || c.level === 1);
              this.flattenedCategories = fallbackResponse.data.map(c => ({
                id: c.id,
                code: c.code,
                name: c.name,
                level: c.level,
                hasChildren: false
              }));
              // Create multi-select options from flattened categories
              this.categoryMultiSelectOptions = this.flattenedCategories.map(c => ({
                id: c.id,
                label: c.name,
                code: c.code,
                level: c.level,
                hasChildren: c.hasChildren
              }));
            }
          }
        });
      }
    });
  }

  // Flatten category tree for dropdown display
  private flattenCategoryTree(nodes: CategoryTreeNode[], result: { id: string; code: string; name: string; level: number; hasChildren: boolean }[] = []): { id: string; code: string; name: string; level: number; hasChildren: boolean }[] {
    for (const node of nodes) {
      result.push({
        id: node.id,
        code: node.code,
        name: node.name,
        level: node.level,
        hasChildren: node.hasChildren
      });
      if (node.children && node.children.length > 0) {
        this.flattenCategoryTree(node.children, result);
      }
    }
    return result;
  }

  // Get indentation for category display
  getCategoryIndent(level: number): string {
    return '\u00A0\u00A0\u00A0\u00A0'.repeat(level); // 4 non-breaking spaces per level
  }

  loadSubCategories(parentCategoryId: string): void {
    if (!parentCategoryId) {
      this.subCategories = [];
      return;
    }

    // Load subcategories by parent ID
    this.productService.getCategories({ parentId: parentCategoryId, isActive: true }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data?.items) {
          this.subCategories = response.data.items.map(c => ({
            id: c.id,
            code: c.code,
            name: c.name,
            fullPath: c.fullPath || c.name,
            level: c.level
          }));
        }
      },
      error: (err) => {
        console.error('Failed to load subcategories', err);
        this.subCategories = [];
      }
    });
  }

  loadProjectsByCustomer(customerId: string): void {
    if (!customerId) {
      this.projects = [];
      return;
    }

    this.projectService.getProjectLookup(customerId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.projects = response.data;
        }
      },
      error: (err) => {
        console.error('Failed to load projects', err);
        this.projects = [];
      }
    });
  }

  loadProductsByCategory(categoryId: string): void {
    if (!categoryId) {
      this.products = [];
      this.productMultiSelectOptions = [];
      return;
    }

    this.productService.getProductLookup(undefined, categoryId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.products = response.data;
          // Create multi-select options from products
          this.productMultiSelectOptions = response.data.map(p => ({
            id: p.id,
            label: p.name,
            code: p.productCode
          }));
        }
      },
      error: (err) => {
        console.error('Failed to load products', err);
        this.products = [];
        this.productMultiSelectOptions = [];
      }
    });
  }

  loadAllProducts(): void {
    this.productService.getProductLookup().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.products = response.data;
          // Create multi-select options from products
          this.productMultiSelectOptions = response.data.map(p => ({
            id: p.id,
            label: p.name,
            code: p.productCode
          }));
        }
      },
      error: (err) => {
        console.error('Failed to load products', err);
      }
    });
  }

  onCustomerChange(customerId: string): void {
    // Clear project selection when customer changes
    this.complaintForm.patchValue({ projectId: '' });
    this.projects = [];

    // Load projects for the selected customer (only for non-portal users)
    if (customerId && !this.isPortalUser) {
      this.loadProjectsByCustomer(customerId);
    }
  }

  onParentCategoryChange(categoryId: string): void {
    // Clear product selection when category changes
    this.complaintForm.patchValue({ productCategoryId: '', productIds: [] });
    this.subCategories = [];
    this.products = [];
    this.productMultiSelectOptions = [];

    // Load products for the selected category (or all products if no category)
    if (categoryId) {
      this.loadProductsByCategory(categoryId);
      // Also load subcategories in case they're needed
      this.loadSubCategories(categoryId);
    } else {
      // Load all products when no category selected
      this.loadAllProducts();
    }
  }

  // Handler for multi-select category selection changes
  onCategorySelectionChange(selectedCategoryIds: string[]): void {
    // Clear product selection when categories change
    this.complaintForm.patchValue({ productIds: [] });
    this.products = [];
    this.productMultiSelectOptions = [];

    // If categories are selected, load products for those categories
    if (selectedCategoryIds && selectedCategoryIds.length > 0) {
      // Load products for first selected category (or implement multi-category product loading)
      this.loadProductsByCategory(selectedCategoryIds[0]);
    } else {
      // Load all products when no category selected
      this.loadAllProducts();
    }
  }

  // Handler for multi-select product selection changes
  onProductSelectionChange(selectedProductIds: string[]): void {
    // Products selected - can be used for any additional logic
    console.log('Selected products:', selectedProductIds);
  }

  onProductCategoryChange(categoryId: string): void {
    // Clear product selection when subcategory changes
    this.complaintForm.patchValue({ productId: '' });
    this.products = [];

    // Load products for the selected subcategory
    if (categoryId) {
      this.loadProductsByCategory(categoryId);
    }
  }

  toggleEnterpriseSection(): void {
    this.enterpriseSectionExpanded = !this.enterpriseSectionExpanded;
  }

  autoPopulateUserInfo(): void {
    const currentUser = this.authService.currentUserValue;

    if (currentUser) {
      this.complaintForm.patchValue({
        branchId: currentUser.branchId || '',
        departmentId: currentUser.departmentId || '',
        sectionId: currentUser.sectionId || '',
        contactEmail: currentUser.email,
        contactPhone: currentUser.phone || ''
      });

      // Make email readonly since it's auto-populated
      this.complaintForm.get('contactEmail')?.disable();

      // Load departments if branch is set
      if (currentUser.branchId) {
        this.loadDepartments(currentUser.branchId);
      }

      // Load sections if department is set
      if (currentUser.departmentId) {
        this.loadSections(currentUser.departmentId);
      }
    }
  }

  loadComplaint(id: string): void {
    this.loading = true;
    this.error = null;

    this.complaintService.getComplaintById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const complaint = response.data;

          this.complaintForm.patchValue({
            title: complaint.title,
            description: complaint.description,
            categoryId: complaint.categoryId,
            priorityMasterId: complaint.priorityId,  // Changed from priority to priorityId
            tags: complaint.tags || '',
            // Enterprise fields (productCategoryId is not stored - it's a UI filter)
            customerId: complaint.customerId || '',
            projectId: complaint.projectId || '',
            productId: complaint.productId || ''
          });

          // Load sections for department
          if (complaint.departmentId) {
            this.loadSections(complaint.departmentId);
          }

          // Load cascading enterprise data
          if (complaint.customerId && !this.isPortalUser) {
            this.loadProjectsByCustomer(complaint.customerId);
          }
          // If complaint has a productId, load all products so the current selection is shown
          if (complaint.productId) {
            this.loadAllProducts();
          }
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load complaint';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onBranchChange(branchId: string): void {
    this.complaintForm.patchValue({
      departmentId: '',
      sectionId: ''
    });
    this.departments = [];
    this.sections = [];

    if (branchId) {
      this.loadDepartments(branchId);
    }
  }

  onDepartmentChange(departmentId: string): void {
    this.complaintForm.patchValue({ sectionId: '' });
    this.sections = [];

    if (departmentId) {
      this.loadSections(departmentId);
    }
  }

  onSubmit(): void {
    if (this.complaintForm.valid || this.complaintForm.get('contactEmail')?.disabled) {
      this.loading = true;
      this.error = null;

      // Get form value including disabled fields
      const formValue = this.complaintForm.getRawValue();

      // Handle multi-select fields - convert arrays to first item for now (API expects single values)
      const productCategoryIds: string[] = formValue.parentCategoryIds || [];
      const productIds: string[] = formValue.productIds || [];

      if (this.isEditMode && this.complaintId) {
        // Update existing complaint
        const updateRequest = {
          id: this.complaintId,
          title: formValue.title,
          description: formValue.description,
          categoryId: formValue.categoryId,
          priorityMasterId: formValue.priorityMasterId,  // Changed from priority to priorityMasterId
          tags: formValue.tags,
          // Enterprise fields - use first selected for API (supports single value)
          customerId: formValue.customerId || null,
          projectId: formValue.projectId || null,
          productId: productIds.length > 0 ? productIds[0] : null,
          // Store all selected IDs as comma-separated in tags or notes if needed
          productCategoryIds: productCategoryIds.length > 0 ? productCategoryIds : null,
          productIds: productIds.length > 0 ? productIds : null
        };

        this.complaintService.updateComplaint(this.complaintId, updateRequest).subscribe({
          next: (response) => {
            if (response.isSuccess && response.data) {
              this.router.navigate(['/complaints', response.data.id]);
            }
            this.loading = false;
          },
          error: (err) => {
            this.error = 'Failed to update complaint';
            this.loading = false;
            console.error(err);
          }
        });
      } else {
        // Create new complaint
        // Clean up empty strings to null for optional Guid fields
        const createRequest = {
          ...formValue,
          branchId: formValue.branchId || null,
          departmentId: formValue.departmentId || null,
          sectionId: formValue.sectionId || null,
          alternatePhone: formValue.alternatePhone || null,
          tags: formValue.tags || null,
          // Enterprise fields - use first selected for API (supports single value)
          customerId: formValue.customerId || null,
          projectId: formValue.projectId || null,
          productId: productIds.length > 0 ? productIds[0] : null,
          // Store all selected IDs arrays
          productCategoryIds: productCategoryIds.length > 0 ? productCategoryIds : null,
          productIds: productIds.length > 0 ? productIds : null
        };
        // Remove UI-only fields before sending to API
        delete (createRequest as any).productCategoryId;
        delete (createRequest as any).parentCategoryIds;

        this.complaintService.createComplaint(createRequest).subscribe({
          next: (response) => {
            if (response.isSuccess && response.data) {
              // If files are selected, upload them
              if (this.selectedFiles.length > 0) {
                this.uploadAttachments(response.data.id);
              } else {
                // Navigate to the created complaint detail page
                this.router.navigate(['/complaints', response.data.id]);
              }
            }
            this.loading = false;
          },
          error: (err) => {
            this.error = 'Failed to create complaint';
            this.loading = false;
            console.error(err);
          }
        });
      }
    } else {
      this.error = 'Please fill in all required fields';
      // Mark all fields as touched to show validation errors
      Object.keys(this.complaintForm.controls).forEach(key => {
        this.complaintForm.get(key)?.markAsTouched();
      });
    }
  }

  private uploadAttachments(complaintId: string): void {
    const formData = new FormData();
    this.selectedFiles.forEach(file => {
      formData.append('files', file);
    });

    this.complaintService.uploadAttachments(complaintId, formData).subscribe({
      next: (response: any) => {
        console.log('Attachments uploaded successfully:', response);
        // Navigate to the created complaint detail page
        this.router.navigate(['/complaints', complaintId]);
      },
      error: (error: any) => {
        console.error('Error uploading attachments:', error);
        // Still navigate to the complaint even if attachment upload fails
        // The user can always add attachments later
        this.router.navigate(['/complaints', complaintId]);
      }
    });
  }

  onFileSelect(event: Event): void {
    console.log('=== FILE SELECT EVENT TRIGGERED ===');
    console.log('Event:', event);
    console.log('Event type:', event.type);

    const input = event.target as HTMLInputElement;
    console.log('Input element:', input);
    console.log('Input files:', input.files);
    console.log('Files length:', input.files?.length);

    if (!input.files || input.files.length === 0) {
      console.log('No files selected - returning early');
      return;
    }

    this.fileUploadError = null;
    const files = Array.from(input.files);
    console.log('Files array:', files);

    // Check total files limit
    if (this.selectedFiles.length + files.length > 10) {
      this.fileUploadError = `You can only upload a maximum of 10 files. Currently selected: ${this.selectedFiles.length}`;
      input.value = '';
      return;
    }

    // Validate each file
    const maxFileSize = 10 * 1024 * 1024; // 10MB
    for (const file of files) {
      if (file.size > maxFileSize) {
        this.fileUploadError = `File "${file.name}" exceeds the maximum size of 10MB`;
        input.value = '';
        return;
      }
    }

    // Add files to the selected files array
    this.selectedFiles.push(...files);
    input.value = ''; // Reset input so the same file can be selected again if removed
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.fileUploadError = null;
  }

  clearAllFiles(): void {
    this.selectedFiles = [];
    this.fileUploadError = null;
  }

  // Direct method to trigger file input click
  triggerFileInput(): void {
    console.log('=== TRIGGER FILE INPUT CALLED ===');
    console.log('FileInput ViewChild:', this.fileInput);

    if (this.fileInput && this.fileInput.nativeElement) {
      const input = this.fileInput.nativeElement;
      console.log('Native element:', input);
      console.log('Input disabled:', input.disabled);

      if (!input.disabled) {
        // Use setTimeout to ensure we're outside any event bubbling
        setTimeout(() => {
          console.log('Calling input.click() inside setTimeout');
          input.click();
        }, 0);
      }
    } else {
      console.error('FileInput ViewChild not found!');
      // Fallback to document.getElementById
      const fallbackInput = document.getElementById('fileUploadInput') as HTMLInputElement;
      if (fallbackInput) {
        console.log('Using fallback input');
        setTimeout(() => {
          fallbackInput.click();
        }, 0);
      }
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  getFileIcon(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return 'bi-file-earmark-pdf';
      case 'doc':
      case 'docx':
        return 'bi-file-earmark-word';
      case 'txt':
        return 'bi-file-earmark-text';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif':
      case 'svg':
      case 'webp':
        return 'bi-file-earmark-image';
      case 'mp4':
      case 'webm':
      case 'mov':
      case 'avi':
      case 'mkv':
        return 'bi-file-earmark-play';
      default:
        return 'bi-file-earmark';
    }
  }

  onCancel(): void {
    this.router.navigate(['/dashboard']);
  }

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  get f() {
    return this.complaintForm.controls;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.complaintForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.complaintForm.get(fieldName);

    if (field?.hasError('required')) {
      return 'This field is required';
    }
    if (field?.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `Maximum length is ${maxLength} characters`;
    }

    return '';
  }

  getBranchName(): string {
    const branchId = this.complaintForm.get('branchId')?.value;
    if (!branchId) return '';
    const branch = this.branches.find(b => b.id === branchId);
    return branch?.name || '';
  }

  getDepartmentName(): string {
    const departmentId = this.complaintForm.get('departmentId')?.value;
    if (!departmentId) return '';
    const department = this.departments.find(d => d.id === departmentId);
    return department?.name || '';
  }

  getSectionName(): string {
    const sectionId = this.complaintForm.get('sectionId')?.value;
    if (!sectionId) return '';
    const section = this.sections.find(s => s.id === sectionId);
    return section?.name || '';
  }

}
