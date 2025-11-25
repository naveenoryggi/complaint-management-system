# Base Class Pattern - Developer Guide

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Why Use This Pattern](#why-use-this-pattern)
3. [Components Structure](#components-structure)
4. [Quick Start Guide](#quick-start-guide)
5. [Implementation Steps](#implementation-steps)
6. [Advanced Patterns](#advanced-patterns)
7. [Common Issues & Solutions](#common-issues--solutions)
8. [Best Practices](#best-practices)

---

## Architecture Overview

The Base Class Pattern uses TypeScript generic abstract classes to eliminate code duplication across master management components. Instead of each component implementing its own CRUD operations, validation, and state management, they extend a common base class.

### File Structure

```
src/app/components/admin/
├── shared/
│   └── base-master-management.component.ts  # Generic base class
├── status-master-management/
│   └── status-master-management.component.ts  # Extends base
├── priority-master-management/
│   └── priority-master-management.component.ts  # Extends base
├── category-management/
│   └── category-management.component.ts  # Extends base
├── branch-management/
│   └── branch-management.component.ts  # Extends base
├── department-management/
│   └── department-management.component.ts  # Extends base
└── section-management/
    └── section-management.component.ts  # Extends base
```

### Code Reduction Stats

| Component | Before Refactoring | After Refactoring | Lines Saved |
|-----------|-------------------|-------------------|-------------|
| Status Master | ~450 lines | ~326 lines | ~124 lines |
| Priority Master | ~530 lines | ~380 lines | ~150 lines |
| Category | ~440 lines | ~352 lines | ~88 lines |
| Branch | ~564 lines | ~449 lines | ~115 lines |
| Department | ~546 lines | ~446 lines | ~100 lines |
| Section | ~607 lines | ~477 lines | ~130 lines |
| **TOTAL** | **~3,137 lines** | **~2,430 lines** | **~707 lines** |

**Code Duplication Eliminated**: 22.5% reduction across 6 components

---

## Why Use This Pattern

### Benefits

1. **DRY Principle**: Write common CRUD logic once, use everywhere
2. **Consistency**: All components behave the same way
3. **Maintainability**: Fix bugs in one place
4. **Type Safety**: Generic types provide compile-time checking
5. **Easy Testing**: Test base class thoroughly, components inherit reliability
6. **Faster Development**: New components take 30 minutes instead of 2 hours

### What The Base Class Provides

- ✅ CRUD operations (Create, Read, Update, Delete)
- ✅ Form state management
- ✅ Modal open/close logic
- ✅ Loading states
- ✅ Error handling
- ✅ Success message handling
- ✅ Search functionality
- ✅ Filtering by active status
- ✅ Form validation
- ✅ Delete confirmation dialogs
- ✅ Permission checking
- ✅ Logging integration

### What Components Add

- Entity-specific fields (e.g., colorCode for Status, level for Priority)
- Custom validation rules
- Hierarchical loading (e.g., Branch → Department → Section)
- Entity-specific UI behaviors

---

## Components Structure

### Base Class Interfaces

```typescript
// Base entity interface - all entities must implement these fields
export interface BaseMasterEntity {
  id: string;
  name: string;
  code: string;
  description?: string;
  displayOrder?: number;  // Optional - not all entities need ordering
  isActive: boolean;
  isSystem?: boolean;     // Optional - only for Status and Priority
  companyId?: string;
}

// Base create request - required fields for creating
export interface BaseCreateRequest {
  name: string;
  code: string;
  description?: string;
  displayOrder?: number;
  isActive: boolean;
  companyId?: string;
}

// Base update request - code cannot be changed
export interface BaseUpdateRequest {
  name: string;
  description?: string;
  displayOrder?: number;
  isActive: boolean;
}

// Standard API response wrapper
export interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string;
  data?: T;
}
```

### Base Class Declaration

```typescript
@Directive()
export abstract class BaseMasterManagementComponent<
  TEntity extends BaseMasterEntity,
  TCreateRequest extends BaseCreateRequest,
  TUpdateRequest extends BaseUpdateRequest
> implements OnInit {
  // State management
  items: TEntity[] = [];
  filteredItems: TEntity[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';
  showActiveOnly = true;
  canManageSettings = false;

  // Modal state
  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  modalTitle = '';

  // Form (must be implemented by child class)
  abstract form: (TCreateRequest & { id?: string }) | (TUpdateRequest & { code?: string });

  // Delete state
  showDeleteConfirm = false;
  itemToDelete: TEntity | null = null;
  deleteLoading = false;

  // Abstract methods that child classes must implement
  protected abstract get entityName(): string;
  protected abstract getEmptyForm(): TCreateRequest;
  protected abstract loadItems(): void;
  protected abstract filterItems(): void;
  protected abstract createItem(request: TCreateRequest): Observable<ApiResponse<TEntity>>;
  protected abstract updateItem(id: string, request: TUpdateRequest): Observable<ApiResponse<TEntity>>;
  protected abstract deleteItem(id: string): Observable<ApiResponse<boolean>>;
}
```

---

## Quick Start Guide

### Step 1: Define Your Models

```typescript
// models/my-entity.model.ts
export interface MyEntity {
  id: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  // Add entity-specific fields
  myCustomField: string;
}

export interface CreateMyEntityRequest {
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  myCustomField: string;
}

export interface UpdateMyEntityRequest {
  name: string;
  description?: string;
  isActive: boolean;
  myCustomField: string;
}
```

### Step 2: Create Your Service

```typescript
// services/my-entity.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../components/admin/shared/base-master-management.component';

@Injectable({
  providedIn: 'root'
})
export class MyEntityService {
  private apiUrl = 'http://localhost:5058/api/myentities';

  constructor(private http: HttpClient) {}

  getEntities(): Observable<ApiResponse<MyEntity[]>> {
    return this.http.get<ApiResponse<MyEntity[]>>(this.apiUrl);
  }

  createEntity(request: CreateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
    return this.http.post<ApiResponse<MyEntity>>(this.apiUrl, request);
  }

  updateEntity(id: string, request: UpdateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
    return this.http.put<ApiResponse<MyEntity>>(`${this.apiUrl}/${id}`, request);
  }

  deleteEntity(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
```

### Step 3: Create Your Component

```typescript
// components/admin/my-entity-management/my-entity-management.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MyEntityService } from '../../../services/my-entity.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { MyEntity, CreateMyEntityRequest, UpdateMyEntityRequest } from '../../../models/my-entity.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-my-entity-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-entity-management.component.html',
  styleUrls: ['./my-entity-management.component.scss']
})
export class MyEntityManagementComponent
  extends BaseMasterManagementComponent<MyEntity, CreateMyEntityRequest, UpdateMyEntityRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateMyEntityRequest & { id?: string }) | (UpdateMyEntityRequest & { code?: string });

  // Implement abstract entityName
  protected override get entityName(): string {
    return 'MyEntity';
  }

  // Getters/setters for entity-specific fields
  get myCustomField(): string {
    return (this.form as any).myCustomField || '';
  }

  set myCustomField(value: string) {
    (this.form as any).myCustomField = value;
  }

  constructor(
    private myEntityService: MyEntityService,
    authService: AuthService,
    logger: LoggerService
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    super.ngOnInit();
  }

  // Implement abstract methods
  protected override getEmptyForm(): CreateMyEntityRequest {
    return {
      name: '',
      code: '',
      description: '',
      isActive: true,
      myCustomField: ''
    };
  }

  protected override loadItems(): void {
    this.loading = true;
    this.errorMessage = '';

    this.myEntityService.getEntities().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.items = response.data;
          this.filterItems();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load entities. Please try again.';
        this.loading = false;
        this.logger.error('Error loading entities', error, 'MyEntityManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status
    if (this.showActiveOnly) {
      filtered = filtered.filter(e => e.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(e =>
        e.name.toLowerCase().includes(search) ||
        e.code.toLowerCase().includes(search) ||
        (e.description && e.description.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
    return this.myEntityService.createEntity(request);
  }

  protected override updateItem(id: string, request: UpdateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
    return this.myEntityService.updateEntity(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.myEntityService.deleteEntity(id);
  }

  // Optional: Override openEditModal if you need custom behavior
  override openEditModal(item: MyEntity): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit entities';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit MyEntity';

    // Create edit form with all entity-specific fields
    this.form = {
      id: item.id,
      name: item.name,
      code: item.code,
      description: item.description || '',
      isActive: item.isActive,
      myCustomField: item.myCustomField
    } as (CreateMyEntityRequest & { id?: string }) | (UpdateMyEntityRequest & { code?: string });

    this.showModal = true;
    this.errorMessage = '';
  }

  // Optional: Override validateForm for custom validation
  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Add custom validation
    if (!this.myCustomField) {
      this.errorMessage = 'My custom field is required';
      return false;
    }

    return true;
  }

  // Alias getters for template compatibility
  get entities(): MyEntity[] {
    return this.items;
  }

  get filteredEntities(): MyEntity[] {
    return this.filteredItems;
  }
}
```

### Step 4: Create Your Template

```html
<!-- my-entity-management.component.html -->
<div class="entity-management">
  <h2>MyEntity Management</h2>

  <!-- Error/Success Messages -->
  <div *ngIf="errorMessage" class="alert alert-danger">{{ errorMessage }}</div>
  <div *ngIf="successMessage" class="alert alert-success">{{ successMessage }}</div>

  <!-- Toolbar -->
  <div class="toolbar">
    <button (click)="openCreateModal()" [disabled]="!canManageSettings">
      Create New MyEntity
    </button>
    <input [(ngModel)]="searchTerm" (ngModelChange)="onSearchChange()" placeholder="Search...">
    <label>
      <input type="checkbox" [(ngModel)]="showActiveOnly" (ngModelChange)="onActiveFilterChange()">
      Show Active Only
    </label>
  </div>

  <!-- Loading Spinner -->
  <div *ngIf="loading" class="loading">Loading...</div>

  <!-- Entity List -->
  <table *ngIf="!loading">
    <thead>
      <tr>
        <th>Name</th>
        <th>Code</th>
        <th>Custom Field</th>
        <th>Status</th>
        <th>Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let entity of filteredEntities; trackBy: trackById">
        <td>{{ entity.name }}</td>
        <td>{{ entity.code }}</td>
        <td>{{ entity.myCustomField }}</td>
        <td>{{ entity.isActive ? 'Active' : 'Inactive' }}</td>
        <td>
          <button (click)="openEditModal(entity)" [disabled]="!canManageSettings">Edit</button>
          <button (click)="confirmDelete(entity)" [disabled]="!canManageSettings">Delete</button>
        </td>
      </tr>
    </tbody>
  </table>

  <!-- Create/Edit Modal -->
  <div *ngIf="showModal" class="modal">
    <div class="modal-content">
      <h3>{{ modalTitle }}</h3>
      <form>
        <label>
          Name:
          <input [(ngModel)]="formName" name="name" required>
        </label>
        <label>
          Code:
          <input [(ngModel)]="formCode" name="code" [readonly]="modalMode === 'edit'" required>
        </label>
        <label>
          Description:
          <textarea [(ngModel)]="formDescription" name="description"></textarea>
        </label>
        <label>
          Custom Field:
          <input [(ngModel)]="myCustomField" name="myCustomField" required>
        </label>
        <label>
          <input type="checkbox" [(ngModel)]="formIsActive" name="isActive">
          Active
        </label>
        <div class="modal-actions">
          <button type="button" (click)="save()">Save</button>
          <button type="button" (click)="closeModal()">Cancel</button>
        </div>
      </form>
    </div>
  </div>

  <!-- Delete Confirmation Modal -->
  <div *ngIf="showDeleteConfirm" class="modal">
    <div class="modal-content">
      <h3>Confirm Delete</h3>
      <p>Are you sure you want to delete this entity?</p>
      <div class="modal-actions">
        <button (click)="executeDelete()">Confirm</button>
        <button (click)="cancelDelete()">Cancel</button>
      </div>
    </div>
  </div>
</div>
```

---

## Implementation Steps

### Step-by-Step Process

#### 1. Analyze Existing Component
Before refactoring, identify:
- Common CRUD operations
- Entity-specific fields beyond base properties
- Custom validation rules
- Special UI behaviors
- Service method signatures

#### 2. Update Entity Models
Ensure your entity extends the base interface concept:
```typescript
export interface MyEntity {
  // Base fields
  id: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;

  // Entity-specific fields
  myField1: string;
  myField2: number;
}
```

#### 3. Create Getters/Setters for Entity-Specific Fields
```typescript
get myField1(): string {
  return (this.form as any).myField1 || '';
}

set myField1(value: string) {
  (this.form as any).myField1 = value;
}
```

**Why use `(this.form as any)`?**
Because `form` is a union type that TypeScript can't statically resolve. Entity-specific fields aren't in the base interface, so we need to cast to `any` to access them.

#### 4. Implement Abstract Methods

**getEmptyForm()**: Return a fresh create request
```typescript
protected override getEmptyForm(): CreateMyEntityRequest {
  return {
    name: '',
    code: '',
    description: '',
    isActive: true,
    myField1: '',
    myField2: 0
  };
}
```

**loadItems()**: Fetch items from API
```typescript
protected override loadItems(): void {
  this.loading = true;
  this.myService.getItems().subscribe({
    next: (response) => {
      if (response.isSuccess && response.data) {
        this.items = response.data;
        this.filterItems();
      }
      this.loading = false;
    },
    error: (error) => {
      this.errorMessage = 'Failed to load items';
      this.loading = false;
      this.logger.error('Load error', error, 'MyComponent');
    }
  });
}
```

**filterItems()**: Client-side filtering
```typescript
protected override filterItems(): void {
  let filtered = this.items;

  if (this.showActiveOnly) {
    filtered = filtered.filter(i => i.isActive);
  }

  if (this.searchTerm) {
    const search = this.searchTerm.toLowerCase();
    filtered = filtered.filter(i =>
      i.name.toLowerCase().includes(search) ||
      i.code.toLowerCase().includes(search)
    );
  }

  this.filteredItems = filtered;
}
```

**createItem()**: Wrap service call
```typescript
protected override createItem(request: CreateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
  return this.myService.create(request);
}
```

**updateItem()**: Wrap service call
```typescript
protected override updateItem(id: string, request: UpdateMyEntityRequest): Observable<ApiResponse<MyEntity>> {
  return this.myService.update(id, request);
}
```

**deleteItem()**: Wrap service call
```typescript
protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
  return this.myService.delete(id);
}
```

#### 5. Override Methods When Needed

**Override openEditModal() for custom fields**:
```typescript
override openEditModal(item: MyEntity): void {
  if (!this.canManageSettings) {
    this.errorMessage = 'No permission';
    return;
  }

  this.modalMode = 'edit';
  this.modalTitle = 'Edit MyEntity';

  this.form = {
    id: item.id,
    name: item.name,
    code: item.code,
    description: item.description || '',
    isActive: item.isActive,
    myField1: item.myField1,
    myField2: item.myField2
  } as (CreateMyEntityRequest & { id?: string }) | (UpdateMyEntityRequest & { code?: string });

  this.showModal = true;
  this.errorMessage = '';
}
```

**Override validateForm() for custom validation**:
```typescript
protected override validateForm(): boolean {
  // Always call super first
  if (!super.validateForm()) {
    return false;
  }

  // Add custom validation
  if (this.myField2 < 0 || this.myField2 > 100) {
    this.errorMessage = 'Field must be between 0 and 100';
    return false;
  }

  return true;
}
```

**Override save() for complex update requests**:
```typescript
override save(): void {
  if (!this.validateForm()) {
    return;
  }

  this.loading = true;
  this.errorMessage = '';

  if (this.modalMode === 'create') {
    this.createItem(this.form as CreateMyEntityRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Created successfully';
          this.closeModal();
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || 'Failed to create';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create';
        this.loading = false;
      }
    });
  } else {
    // Build custom update request
    const updateRequest = {
      name: this.formName,
      description: this.formDescription,
      isActive: this.formIsActive,
      myField1: this.myField1,
      myField2: this.myField2
    } as UpdateMyEntityRequest;

    this.updateItem(this.formId, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Updated successfully';
          this.closeModal();
          this.loadItems();
          setTimeout(() => this.successMessage = '', 3000);
        } else {
          this.errorMessage = response.message || 'Failed to update';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update';
        this.loading = false;
      }
    });
  }
}
```

---

## Advanced Patterns

### Pattern 1: Hierarchical Loading (Branch → Department → Section)

**Use Case**: Entity depends on parent entity selection

```typescript
export class DepartmentManagementComponent extends BaseMasterManagementComponent<...> {
  branches: Branch[] = [];
  selectedBranchId = '';

  override ngOnInit(): void {
    this.canManageSettings = this.authService.hasPermission('ManageSettings');
    this.loadBranches();  // Load parent first
  }

  loadBranches(): void {
    this.branchService.getBranches(companyId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.branches = response.data;
          if (this.branches.length > 0 && !this.selectedBranchId) {
            this.selectedBranchId = this.branches[0].id;
            this.onBranchChange();  // Auto-load departments for first branch
          }
        }
      }
    });
  }

  onBranchChange(): void {
    if (this.selectedBranchId) {
      this.loadItems();  // Load departments for selected branch
    } else {
      this.items = [];
      this.filteredItems = [];
    }
  }

  protected override loadItems(): void {
    if (!this.selectedBranchId) {
      this.items = [];
      return;
    }

    this.loading = true;
    this.departmentService.getDepartments(this.selectedBranchId).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      }
    });
  }

  override openCreateModal(): void {
    if (!this.selectedBranchId) {
      this.errorMessage = 'Please select a branch first';
      return;
    }

    super.openCreateModal();
    this.branchId = this.selectedBranchId;  // Pre-fill parent ID
  }
}
```

### Pattern 2: Three-Way Status Filter (All/Active/Inactive)

**Use Case**: More granular filtering than boolean

```typescript
export class BranchManagementComponent extends BaseMasterManagementComponent<...> {
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  protected override filterItems(): void {
    let filtered = this.items;

    // Three-way filter instead of boolean
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(b => b.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(b => !b.isActive);
    }
    // 'all' shows everything

    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(b =>
        b.name.toLowerCase().includes(search) ||
        b.code.toLowerCase().includes(search)
      );
    }

    this.filteredItems = filtered;
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.statusFilter = filter;
    this.filterItems();
  }
}
```

**Template**:
```html
<div class="filter-buttons">
  <button [class.active]="statusFilter === 'all'" (click)="setStatusFilter('all')">
    All ({{ items.length }})
  </button>
  <button [class.active]="statusFilter === 'active'" (click)="setStatusFilter('active')">
    Active ({{ getActiveCount() }})
  </button>
  <button [class.active]="statusFilter === 'inactive'" (click)="setStatusFilter('inactive')">
    Inactive ({{ getInactiveCount() }})
  </button>
</div>
```

### Pattern 3: Inverted Property Mapping (showInactive ↔ showActiveOnly)

**Use Case**: Existing template uses opposite naming convention

```typescript
export class SectionManagementComponent extends BaseMasterManagementComponent<...> {
  // Base class has showActiveOnly, but our template uses showInactive
  get showInactive(): boolean {
    return !this.showActiveOnly;
  }

  set showInactive(value: boolean) {
    this.showActiveOnly = !value;
  }
}
```

**Template**:
```html
<!-- Template can use showInactive while base class uses showActiveOnly -->
<label>
  <input type="checkbox" [(ngModel)]="showInactive" (ngModelChange)="onActiveFilterChange()">
  Show Inactive Items
</label>
```

### Pattern 4: System Entity Warning

**Use Case**: Warn when editing/deleting system-defined entities

```typescript
export class StatusMasterManagementComponent extends BaseMasterManagementComponent<...> {
  override openEditModal(item: ComplaintStatusMaster): void {
    super.openEditModal(item);

    // Add warning for system entities
    if (item.isSystem) {
      this.errorMessage = '⚠️ This is a system-defined status. Changes may affect core functionality.';
    }
  }

  isEditingSystemEntity(): boolean {
    if (this.modalMode !== 'edit' || !this.formId) return false;
    const item = this.items.find(i => i.id === this.formId);
    return item?.isSystem === true;
  }
}
```

**Template**:
```html
<div *ngIf="isEditingSystemEntity()" class="alert alert-warning">
  ⚠️ You are editing a system-defined entity. Changes may affect core functionality.
</div>
```

### Pattern 5: Cross-Field Validation

**Use Case**: Field B depends on field A

```typescript
export class PriorityMasterManagementComponent extends BaseMasterManagementComponent<...> {
  protected override validateForm(): boolean {
    if (!super.validateForm()) {
      return false;
    }

    // Cross-field validation
    const slaResponse = this.slaResponseHours;
    const slaResolution = this.slaResolutionHours;

    if (slaResponse > slaResolution) {
      this.errorMessage = 'SLA Response hours cannot be greater than SLA Resolution hours';
      return false;
    }

    return true;
  }
}
```

### Pattern 6: Inline Status Toggle

**Use Case**: Toggle entity status without opening edit modal

```typescript
export class BranchManagementComponent extends BaseMasterManagementComponent<...> {
  toggleBranchStatus(branch: Branch): void {
    const updateRequest: UpdateBranchRequest = {
      name: branch.name,
      code: branch.code,
      description: branch.description,
      // ... include all required fields
      isActive: !branch.isActive  // Toggle status
    };

    this.branchService.updateBranch(branch.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          branch.isActive = !branch.isActive;  // Update UI optimistically
          this.successMessage = `Branch ${branch.isActive ? 'activated' : 'deactivated'}`;
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update status';
      }
    });
  }
}
```

### Pattern 7: Optional Base Properties

**Use Case**: Entity doesn't have displayOrder or isSystem

```typescript
// In base-master-management.component.ts
export interface BaseMasterEntity {
  id: string;
  name: string;
  code: string;
  description?: string;
  displayOrder?: number;  // Optional
  isActive: boolean;
  isSystem?: boolean;     // Optional
  companyId?: string;
}

// Base class validation handles optional fields
protected validateForm(): boolean {
  // ... name and code validation ...

  // Only validate displayOrder if it exists
  if (this.formDisplayOrder !== undefined && this.formDisplayOrder !== null) {
    if (this.formDisplayOrder < 0 || this.formDisplayOrder > 9999) {
      this.errorMessage = 'Display order must be between 0 and 9999';
      return false;
    }
  }

  return true;
}
```

---

## Common Issues & Solutions

### Issue 1: "Property does not exist on type union"

**Error**:
```
Property 'myField' does not exist on type
'(CreateRequest & { id?: string }) | (UpdateRequest & { code?: string })'
```

**Solution**: Use getter/setter pattern with `(this.form as any)`
```typescript
get myField(): string {
  return (this.form as any).myField || '';
}

set myField(value: string) {
  (this.form as any).myField = value;
}
```

### Issue 2: Code Field Not Read-Only in Edit Mode

**Problem**: Users can modify code in edit mode, violating business rules

**Solution**: Use `[readonly]` attribute in template
```html
<input
  [(ngModel)]="formCode"
  name="code"
  [readonly]="modalMode === 'edit'"
  required>
```

### Issue 3: ApiResponse Not Properly Unwrapped

**Problem**: Service returns `ApiResponse<T[]>` but component expects `T[]`

**Solution**: Check service method - GET operations should unwrap, CUD operations should not
```typescript
// GET operations - unwrap in service
getItems(): Observable<Item[]> {
  return this.http.get<ApiResponse<Item[]>>(this.apiUrl)
    .pipe(map(response => response.data || []));
}

// CUD operations - return ApiResponse wrapper
createItem(request: CreateRequest): Observable<ApiResponse<Item>> {
  return this.http.post<ApiResponse<Item>>(this.apiUrl, request);
}
```

### Issue 4: Form Not Resetting After Modal Close

**Problem**: Opening create modal after edit shows previous edit data

**Solution**: Base class `closeModal()` already calls `getEmptyForm()`, ensure it's implemented correctly
```typescript
protected override getEmptyForm(): CreateRequest {
  return {
    name: '',
    code: '',
    description: '',
    isActive: true,
    // Reset ALL fields including entity-specific ones
    myField: ''
  };
}
```

### Issue 5: Parent ID Not Set in Hierarchical Create

**Problem**: Creating child entity without parent ID fails validation

**Solution**: Override `openCreateModal()` to pre-fill parent ID
```typescript
override openCreateModal(): void {
  if (!this.selectedParentId) {
    this.errorMessage = 'Please select a parent first';
    return;
  }

  super.openCreateModal();
  this.modalTitle = 'Create New Child Entity';
  // Set parent ID AFTER super.openCreateModal() which calls getEmptyForm()
  this.parentId = this.selectedParentId;
}
```

### Issue 6: Angular Cache Showing Stale Errors

**Problem**: Build succeeds but stderr shows old TypeScript errors

**Solution**: Clear Angular cache
```bash
# Windows PowerShell
rd /s /q .angular
rd /s /q dist
rd /s /q node_modules\.cache

# Then rebuild
npm start
```

### Issue 7: displayOrder Validation Failing for Entities Without Ordering

**Problem**: Base class validates displayOrder but entity doesn't use it

**Solution**: Base class already handles this with optional check:
```typescript
// In base class
if (this.formDisplayOrder !== undefined && this.formDisplayOrder !== null) {
  // Only validate if displayOrder exists
  if (this.formDisplayOrder < 0 || this.formDisplayOrder > 9999) {
    this.errorMessage = 'Display order must be between 0 and 9999';
    return false;
  }
}
```

Ensure your entity model makes displayOrder optional:
```typescript
export interface MyEntity {
  // ... other fields ...
  displayOrder?: number;  // Optional
}
```

---

## Best Practices

### 1. Always Call super Methods First

```typescript
// ✅ CORRECT
protected override validateForm(): boolean {
  if (!super.validateForm()) {
    return false;
  }
  // Your custom validation here
  return true;
}

// ❌ WRONG
protected override validateForm(): boolean {
  // Your custom validation
  return super.validateForm();  // Base validation runs last
}
```

### 2. Use LoggerService, Not console.log

```typescript
// ✅ CORRECT
this.logger.error('Error loading items', error, 'MyComponent');

// ❌ WRONG
console.error('Error loading items', error);
```

### 3. Handle Both Success and Error Responses

```typescript
this.service.create(request).subscribe({
  next: (response) => {
    if (response.isSuccess) {
      // Handle success
      this.successMessage = 'Created successfully';
      this.loadItems();
    } else {
      // Handle API-level failure (e.g., validation error)
      this.errorMessage = response.message || 'Failed to create';
    }
    this.loading = false;
  },
  error: (error) => {
    // Handle HTTP-level error (e.g., 500, network error)
    this.errorMessage = error.error?.message || 'Failed to create. Please try again.';
    this.loading = false;
  }
});
```

### 4. Auto-Dismiss Success Messages

```typescript
this.successMessage = 'Operation successful';
setTimeout(() => this.successMessage = '', 3000);
```

### 5. Pre-Fill Parent IDs in Hierarchical Entities

```typescript
override openCreateModal(): void {
  if (!this.selectedDepartmentId) {
    this.errorMessage = 'Please select a department first';
    return;
  }

  super.openCreateModal();
  this.departmentId = this.selectedDepartmentId;  // Pre-fill
}
```

### 6. Provide Template Aliases for Backward Compatibility

```typescript
// If existing template uses 'categories' instead of 'items'
get categories(): Category[] {
  return this.items;
}

get filteredCategories(): Category[] {
  return this.filteredItems;
}
```

### 7. Type Union Forms Correctly

```typescript
// ✅ CORRECT
form!: (CreateRequest & { id?: string }) | (UpdateRequest & { code?: string });

// ❌ WRONG
form!: CreateRequest | UpdateRequest;  // Missing id and code for opposite type
```

### 8. Set modalTitle in Override Methods

```typescript
override openCreateModal(): void {
  super.openCreateModal();
  this.modalTitle = 'Create New MyEntity';  // Customize title
}
```

### 9. Test With and Without Permissions

Ensure your component handles `canManageSettings = false`:
```typescript
override openEditModal(item: MyEntity): void {
  if (!this.canManageSettings) {
    this.errorMessage = 'You do not have permission to edit entities';
    return;
  }
  // ... rest of edit logic
}
```

### 10. Sort Filtered Items When displayOrder Matters

```typescript
protected override filterItems(): void {
  let filtered = this.items;

  // ... filtering logic ...

  // Sort by displayOrder if entity uses it
  this.filteredItems = filtered.sort((a, b) =>
    (a.displayOrder || 0) - (b.displayOrder || 0)
  );
}
```

---

## Testing Your Implementation

### Unit Test Template

```typescript
describe('MyEntityManagementComponent', () => {
  let component: MyEntityManagementComponent;
  let fixture: ComponentFixture<MyEntityManagementComponent>;
  let mockService: jasmine.SpyObj<MyEntityService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockLogger: jasmine.SpyObj<LoggerService>;

  beforeEach(() => {
    mockService = jasmine.createSpyObj('MyEntityService', ['getEntities', 'createEntity', 'updateEntity', 'deleteEntity']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['hasPermission']);
    mockLogger = jasmine.createSpyObj('LoggerService', ['info', 'error']);

    TestBed.configureTestingModule({
      imports: [MyEntityManagementComponent],
      providers: [
        { provide: MyEntityService, useValue: mockService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: LoggerService, useValue: mockLogger }
      ]
    });

    fixture = TestBed.createComponent(MyEntityManagementComponent);
    component = fixture.componentInstance;
  });

  it('should load items on init', () => {
    const mockData = [{ id: '1', name: 'Test', code: 'TEST', isActive: true }];
    mockService.getEntities.and.returnValue(of({ isSuccess: true, data: mockData }));

    component.ngOnInit();

    expect(mockService.getEntities).toHaveBeenCalled();
    expect(component.items.length).toBe(1);
  });

  it('should validate required fields', () => {
    component.form = component.getEmptyForm();
    component.formName = '';

    const isValid = component.validateForm();

    expect(isValid).toBe(false);
    expect(component.errorMessage).toContain('name');
  });

  // ... more tests
});
```

---

## Migration Checklist

When refactoring an existing component:

- [ ] Read existing component thoroughly
- [ ] Identify entity-specific fields and behaviors
- [ ] Update entity models if needed
- [ ] Create service wrapper methods if needed
- [ ] Extend BaseMasterManagementComponent
- [ ] Implement all abstract methods
- [ ] Create getters/setters for entity-specific fields
- [ ] Override methods that need custom behavior
- [ ] Test CRUD operations
- [ ] Test validation
- [ ] Test with and without permissions
- [ ] Test hierarchical loading (if applicable)
- [ ] Check browser console for errors
- [ ] Verify no TypeScript compilation errors
- [ ] Update template if needed
- [ ] Test search and filtering
- [ ] Test modal open/close
- [ ] Test delete confirmation
- [ ] Document any special patterns used

---

## FAQ

**Q: Why use `(this.form as any)` instead of proper typing?**

A: The form property is a union type `(CreateRequest & { id?: string }) | (UpdateRequest & { code?: string })`. TypeScript cannot statically determine which type it is at any given time, so accessing entity-specific fields requires casting. This is a known limitation of TypeScript's union types with discriminated properties.

**Q: Can I use this pattern with non-master entities?**

A: Yes! Any entity with CRUD operations can benefit. The pattern works best when entities share common fields (id, name, code, description, isActive).

**Q: Should I always call super methods?**

A: For validation and lifecycle methods (ngOnInit), yes. For other methods, only if you want to keep base behavior.

**Q: What if my API doesn't return ApiResponse wrapper?**

A: Update your service to unwrap responses, or modify the base class interfaces to match your API contract.

**Q: How do I add custom toolbar buttons?**

A: Just add them to your template - the base class doesn't restrict template customization.

**Q: Can I have multiple forms on the same page?**

A: Yes, but you'll need to manage multiple form states separately. The base class manages a single form.

---

## Summary

The Base Class Pattern provides:
- **22.5% code reduction** across 6 components
- **Consistent behavior** across all master management components
- **Type safety** with TypeScript generics
- **Easy maintenance** - fix bugs once, benefit everywhere
- **Fast development** - new components in 30 minutes
- **Flexible architecture** - override only what you need

Follow this guide to refactor existing components or create new ones following the pattern. The result is cleaner, more maintainable code with fewer bugs.

---

**Document Version**: 1.0
**Last Updated**: 2025-10-20
**Maintained By**: Development Team
**Related Documents**:
- UI_TESTING_CHECKLIST.md
- ARCHITECTURE.md
- API Documentation
