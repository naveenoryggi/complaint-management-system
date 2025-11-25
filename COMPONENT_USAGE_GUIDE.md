# Complaint Management System - Component Usage Guide

## Quick Reference for Developers

This guide provides copy-paste ready code examples for all components in the design system.

---

## 🎨 Color Reference

### Using Colors in HTML
```html
<!-- Text Colors -->
<p class="text-primary">Primary text</p>
<p class="text-secondary">Secondary text</p>
<p class="text-muted">Muted text</p>
<p class="text-success">Success text</p>
<p class="text-warning">Warning text</p>
<p class="text-error">Error text</p>

<!-- Background Colors -->
<div class="bg-primary">Primary background</div>
<div class="bg-success">Success background</div>
<div class="bg-warning">Warning background</div>
<div class="bg-error">Error background</div>
```

### Using Colors in SCSS
```scss
.my-component {
  // Use CSS variables
  color: var(--text-primary);
  background-color: var(--card-background);
  border-color: var(--border-color);

  // For hover states
  &:hover {
    color: var(--primary-color);
    background-color: var(--primary-color-light);
  }
}
```

---

## 🔘 Buttons

### Basic Buttons
```html
<!-- Primary Button (main actions) -->
<button class="btn btn-primary">
  <i class="fas fa-save"></i>
  Save Changes
</button>

<!-- Secondary Button (alternative actions) -->
<button class="btn btn-secondary">
  <i class="fas fa-times"></i>
  Cancel
</button>

<!-- Success Button (confirmations) -->
<button class="btn btn-success">
  <i class="fas fa-check"></i>
  Approve
</button>

<!-- Danger Button (destructive actions) -->
<button class="btn btn-danger">
  <i class="fas fa-trash"></i>
  Delete
</button>

<!-- Ghost Button (subtle actions) -->
<button class="btn btn-ghost">
  <i class="fas fa-eye"></i>
  View Details
</button>
```

### Button Sizes
```html
<!-- Small Button -->
<button class="btn btn-primary btn-sm">Small</button>

<!-- Regular Button (default) -->
<button class="btn btn-primary">Regular</button>

<!-- Large Button -->
<button class="btn btn-primary btn-lg">Large</button>
```

### Action Buttons (for tables/cards)
```html
<button class="btn-view">
  <i class="fas fa-eye"></i>
  <span>View</span>
</button>

<button class="btn-edit">
  <i class="fas fa-edit"></i>
  <span>Edit</span>
</button>

<button class="btn-delete">
  <i class="fas fa-trash"></i>
  <span>Delete</span>
</button>

<button class="btn-sync">
  <i class="fas fa-sync"></i>
  <span>Sync</span>
</button>
```

### Special Buttons
```html
<!-- Back Button -->
<button class="btn-back" (click)="goBack()">
  <i class="fas fa-arrow-left"></i>
  Back to Dashboard
</button>

<!-- Icon-Only Button -->
<button class="btn-icon" title="Edit">
  <i class="fas fa-edit"></i>
</button>

<!-- Close Button (for modals) -->
<button class="btn-close" (click)="closeModal()">
  <i class="fas fa-times"></i>
</button>
```

### Disabled Buttons
```html
<button class="btn btn-primary" disabled>
  Disabled Button
</button>

<button class="btn btn-primary" [disabled]="loading">
  <i *ngIf="loading" class="fas fa-spinner fa-spin"></i>
  {{ loading ? 'Saving...' : 'Save' }}
</button>
```

---

## 📝 Forms

### Basic Form Field
```html
<div class="form-group">
  <label for="email">
    Email Address
    <span class="required">*</span>
  </label>
  <input
    type="email"
    id="email"
    class="form-control"
    [(ngModel)]="formData.email"
    placeholder="Enter email address"
    required
  />
  <small class="form-help-text">
    We'll never share your email with anyone else
  </small>
  <span class="invalid-feedback" *ngIf="emailError">
    {{ emailError }}
  </span>
</div>
```

### Text Input
```html
<div class="form-group">
  <label for="name">Name <span class="required">*</span></label>
  <input
    type="text"
    id="name"
    class="form-control"
    [(ngModel)]="formData.name"
    placeholder="Enter name"
    required
  />
</div>
```

### Textarea
```html
<div class="form-group">
  <label for="description">Description</label>
  <textarea
    id="description"
    class="form-control"
    [(ngModel)]="formData.description"
    rows="4"
    placeholder="Enter description..."
  ></textarea>
</div>
```

### Select Dropdown
```html
<div class="form-group">
  <label for="status">Status</label>
  <select id="status" class="form-control" [(ngModel)]="formData.status">
    <option [ngValue]="undefined">Select status</option>
    <option value="active">Active</option>
    <option value="inactive">Inactive</option>
  </select>
</div>
```

### Checkbox
```html
<div class="checkbox-group">
  <input
    type="checkbox"
    id="isActive"
    [(ngModel)]="formData.isActive"
  />
  <label for="isActive">
    <i class="fas fa-check-circle"></i>
    Active
  </label>
</div>
```

### Radio Buttons
```html
<div class="radio-group">
  <input
    type="radio"
    id="option1"
    name="options"
    value="option1"
    [(ngModel)]="selectedOption"
  />
  <label for="option1">Option 1</label>
</div>

<div class="radio-group">
  <input
    type="radio"
    id="option2"
    name="options"
    value="option2"
    [(ngModel)]="selectedOption"
  />
  <label for="option2">Option 2</label>
</div>
```

### Form with Validation States
```html
<div class="form-group">
  <label for="email">Email</label>
  <input
    type="email"
    id="email"
    class="form-control"
    [class.is-invalid]="emailInvalid"
    [class.is-valid]="emailValid"
    [(ngModel)]="email"
  />
  <span class="invalid-feedback" *ngIf="emailInvalid">
    Please enter a valid email address
  </span>
  <span class="valid-feedback" *ngIf="emailValid">
    Email is valid!
  </span>
</div>
```

### Form Section with Grid Layout
```html
<div class="form-section">
  <h3><i class="fas fa-user"></i> Personal Information</h3>

  <!-- 2-column grid (responsive) -->
  <div class="form-row">
    <div class="form-group">
      <label for="firstName">First Name <span class="required">*</span></label>
      <input type="text" id="firstName" class="form-control" />
    </div>
    <div class="form-group">
      <label for="lastName">Last Name <span class="required">*</span></label>
      <input type="text" id="lastName" class="form-control" />
    </div>
  </div>

  <!-- Full width field -->
  <div class="form-group">
    <label for="email">Email <span class="required">*</span></label>
    <input type="email" id="email" class="form-control" />
  </div>
</div>
```

---

## 🃏 Cards

### Basic Card
```html
<div class="card">
  <div class="card-header">
    <h3>Card Title</h3>
  </div>
  <div class="card-body">
    <p>Card content goes here</p>
  </div>
  <div class="card-footer">
    <small>Last updated: 2025-11-02</small>
  </div>
</div>
```

### Card with Actions
```html
<div class="card">
  <div class="card-header">
    <h3>User Details</h3>
    <div class="card-actions">
      <button class="btn-icon" (click)="editUser()">
        <i class="fas fa-edit"></i>
      </button>
      <button class="btn-icon btn-delete" (click)="deleteUser()">
        <i class="fas fa-trash"></i>
      </button>
    </div>
  </div>
  <div class="card-body">
    <p><strong>Name:</strong> John Doe</p>
    <p><strong>Email:</strong> john@example.com</p>
  </div>
</div>
```

### Elevated Card
```html
<div class="card card-elevated">
  <div class="card-body">
    This card has enhanced shadow effect
  </div>
</div>
```

### Settings/Branch/User Card
```html
<div class="settings-card" [class.inactive]="!settings.isActive">
  <div class="card-header">
    <div class="settings-info">
      <h3>{{ settings.name }}</h3>
      <span class="status-badge" [class.active]="settings.isActive">
        {{ settings.isActive ? 'Active' : 'Inactive' }}
      </span>
      <span class="default-badge" *ngIf="settings.isDefault">
        <i class="fas fa-star"></i> Default
      </span>
    </div>
    <div class="card-actions">
      <button class="btn-icon" (click)="edit(settings)">
        <i class="fas fa-edit"></i>
      </button>
      <button class="btn-icon btn-delete" (click)="delete(settings)">
        <i class="fas fa-trash"></i>
      </button>
    </div>
  </div>

  <div class="card-body">
    <div class="settings-details">
      <div class="detail-item">
        <i class="fas fa-server"></i>
        <span><strong>Host:</strong> {{ settings.host }}</span>
      </div>
      <div class="detail-item">
        <i class="fas fa-envelope"></i>
        <span><strong>Email:</strong> {{ settings.email }}</span>
      </div>
    </div>
  </div>

  <div class="card-footer">
    <small>Created: {{ settings.createdAt | date:'medium' }}</small>
  </div>
</div>
```

---

## 📊 Tables

### Basic Table
```html
<div class="table-container">
  <table class="table">
    <thead>
      <tr>
        <th>Name</th>
        <th>Email</th>
        <th>Role</th>
        <th>Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let user of users">
        <td>{{ user.name }}</td>
        <td>{{ user.email }}</td>
        <td>
          <span class="badge badge-primary">{{ user.role }}</span>
        </td>
        <td>
          <button class="btn-view" (click)="viewUser(user)">View</button>
          <button class="btn-edit" (click)="editUser(user)">Edit</button>
          <button class="btn-delete" (click)="deleteUser(user)">Delete</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

### Table with Empty State
```html
<div class="table-container" *ngIf="users.length > 0; else emptyState">
  <table class="table">
    <!-- table content -->
  </table>
</div>

<ng-template #emptyState>
  <div class="empty-state">
    <i class="fas fa-users"></i>
    <h3>No Users Found</h3>
    <p>Get started by adding your first user</p>
    <button class="btn btn-primary" (click)="createUser()">
      <i class="fas fa-plus"></i>
      Add User
    </button>
  </div>
</ng-template>
```

---

## 🔔 Alerts & Messages

### Alert Types
```html
<!-- Success Alert -->
<div class="alert alert-success">
  <i class="fas fa-check-circle"></i>
  Operation completed successfully!
</div>

<!-- Warning Alert -->
<div class="alert alert-warning">
  <i class="fas fa-exclamation-triangle"></i>
  Please review the information before proceeding
</div>

<!-- Error Alert -->
<div class="alert alert-danger">
  <i class="fas fa-exclamation-circle"></i>
  An error occurred while processing your request
</div>

<!-- Info Alert -->
<div class="alert alert-info">
  <i class="fas fa-info-circle"></i>
  This is important information you should know
</div>
```

### Alert with Title and Close Button
```html
<div class="alert alert-success">
  <div class="alert-title">Success!</div>
  <p>Your changes have been saved successfully.</p>
  <button class="alert-close" (click)="closeAlert()">
    <i class="fas fa-times"></i>
  </button>
</div>
```

---

## 🏷️ Badges

### Status Badges
```html
<span class="badge badge-success">Active</span>
<span class="badge badge-warning">Pending</span>
<span class="badge badge-danger">Inactive</span>
<span class="badge badge-info">New</span>
<span class="badge badge-primary">Featured</span>
<span class="badge badge-secondary">Default</span>
```

### Custom Status Badges
```html
<span class="status-badge active">Active</span>
<span class="status-badge inactive">Inactive</span>
```

### Default Marker Badge
```html
<span class="default-badge">
  <i class="fas fa-star"></i>
  Default
</span>
```

---

## ⏳ Loading States

### Loading Spinner
```html
<div class="loading-state">
  <div class="spinner"></div>
  <p>Loading data...</p>
</div>
```

### Conditional Loading
```html
<div *ngIf="loading" class="loading-container">
  <div class="spinner"></div>
  <p>Please wait...</p>
</div>

<div *ngIf="!loading">
  <!-- Content when loaded -->
</div>
```

### Skeleton Loading
```html
<div class="skeleton" style="height: 40px; margin-bottom: 12px;"></div>
<div class="skeleton" style="height: 40px; margin-bottom: 12px;"></div>
<div class="skeleton" style="height: 80px;"></div>
```

---

## 📭 Empty States

### Basic Empty State
```html
<div class="empty-state">
  <i class="fas fa-inbox"></i>
  <h3>No Data Found</h3>
  <p>There are no items to display at this time</p>
</div>
```

### Empty State with Action
```html
<div class="empty-state">
  <i class="fas fa-users"></i>
  <h3>No Users Found</h3>
  <p>Get started by creating your first user</p>
  <button class="btn btn-primary" (click)="createUser()">
    <i class="fas fa-plus"></i>
    Create New User
  </button>
</div>
```

### No Search Results
```html
<div class="no-results">
  <i class="fas fa-search"></i>
  <h3>No Results Found</h3>
  <p>Try adjusting your search criteria</p>
</div>
```

---

## 🪟 Modals

### Basic Modal
```html
<div class="modal-overlay" *ngIf="showModal" (click)="closeModal()">
  <div class="modal-content" (click)="$event.stopPropagation()">
    <div class="modal-header">
      <h2>
        <i class="fas fa-user"></i>
        User Details
      </h2>
      <button class="btn-close" (click)="closeModal()">
        <i class="fas fa-times"></i>
      </button>
    </div>

    <div class="modal-body">
      <p>Modal content goes here</p>
    </div>

    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="closeModal()">
        Cancel
      </button>
      <button class="btn btn-primary" (click)="saveChanges()">
        <i class="fas fa-save"></i>
        Save Changes
      </button>
    </div>
  </div>
</div>
```

### Confirmation Modal
```html
<div class="modal-overlay" *ngIf="showDeleteConfirm" (click)="cancelDelete()">
  <div class="modal-content" (click)="$event.stopPropagation()">
    <div class="modal-header">
      <h2>
        <i class="fas fa-exclamation-triangle"></i>
        Confirm Deletion
      </h2>
      <button class="btn-close" (click)="cancelDelete()">
        <i class="fas fa-times"></i>
      </button>
    </div>

    <div class="modal-body">
      <p>Are you sure you want to delete <strong>{{ itemName }}</strong>?</p>
      <p class="text-error">
        <i class="fas fa-exclamation-triangle"></i>
        This action cannot be undone.
      </p>
    </div>

    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="cancelDelete()">
        Cancel
      </button>
      <button class="btn btn-danger" (click)="confirmDelete()">
        <i class="fas fa-trash"></i>
        Delete
      </button>
    </div>
  </div>
</div>
```

---

## 📄 Page Layouts

### Standard Page Layout
```html
<div class="[page-name]-container">
  <!-- Page Header -->
  <div class="page-header">
    <div>
      <h1>
        <i class="fas fa-users"></i>
        User Management
      </h1>
      <p class="page-description">
        View and manage system users and permissions
      </p>
    </div>
    <div class="header-actions">
      <button class="btn btn-secondary" (click)="importUsers()">
        <i class="fas fa-download"></i>
        Import Users
      </button>
      <button class="btn btn-primary" (click)="createUser()">
        <i class="fas fa-plus"></i>
        Add User
      </button>
    </div>
  </div>

  <!-- Back Button -->
  <button class="btn-back" (click)="goBack()">
    <i class="fas fa-arrow-left"></i>
    Back to Dashboard
  </button>

  <!-- Info Banner -->
  <div class="info-banner">
    <div class="info-icon">
      <i class="fas fa-info-circle"></i>
    </div>
    <div class="info-content">
      <h3>About User Management</h3>
      <p>Manage system users, roles, and permissions from this page.</p>
      <ul>
        <li><strong>Add Users:</strong> Create new user accounts</li>
        <li><strong>Assign Roles:</strong> Configure user permissions</li>
        <li><strong>Import:</strong> Bulk import from external systems</li>
      </ul>
    </div>
  </div>

  <!-- Alerts -->
  <div class="alert alert-success" *ngIf="successMessage">
    <i class="fas fa-check-circle"></i>
    {{ successMessage }}
  </div>

  <div class="alert alert-danger" *ngIf="errorMessage">
    <i class="fas fa-exclamation-circle"></i>
    {{ errorMessage }}
  </div>

  <!-- Stats Bar -->
  <div class="stats-bar">
    <div class="stat-card">
      <i class="fas fa-users"></i>
      <div class="stat-info">
        <span class="stat-label">Total Users</span>
        <span class="stat-value">{{ totalUsers }}</span>
      </div>
    </div>
    <div class="stat-card">
      <i class="fas fa-user-check"></i>
      <div class="stat-info">
        <span class="stat-label">Active Users</span>
        <span class="stat-value">{{ activeUsers }}</span>
      </div>
    </div>
  </div>

  <!-- Search and Filter -->
  <div class="filter-section">
    <div class="search-box">
      <i class="fas fa-search"></i>
      <input
        type="text"
        class="search-input"
        [(ngModel)]="searchTerm"
        placeholder="Search users..."
      />
    </div>
    <div class="filter-buttons">
      <button class="filter-btn active" (click)="setFilter('all')">
        <i class="fas fa-list"></i>
        All
      </button>
      <button class="filter-btn" (click)="setFilter('active')">
        <i class="fas fa-check-circle"></i>
        Active
      </button>
      <button class="filter-btn" (click)="setFilter('inactive')">
        <i class="fas fa-times-circle"></i>
        Inactive
      </button>
    </div>
  </div>

  <!-- Loading State -->
  <div *ngIf="loading" class="loading-state">
    <div class="spinner"></div>
    <p>Loading users...</p>
  </div>

  <!-- Content Grid -->
  <div *ngIf="!loading && users.length > 0" class="settings-grid">
    <div *ngFor="let user of users" class="user-card">
      <!-- Card content -->
    </div>
  </div>

  <!-- Empty State -->
  <div *ngIf="!loading && users.length === 0" class="empty-state">
    <i class="fas fa-users"></i>
    <h3>No Users Found</h3>
    <p *ngIf="searchTerm">Try adjusting your search</p>
    <p *ngIf="!searchTerm">Get started by adding your first user</p>
    <button class="btn btn-primary" (click)="createUser()">
      <i class="fas fa-plus"></i>
      Add User
    </button>
  </div>

  <!-- Summary Stats (at bottom) -->
  <div *ngIf="users.length > 0" class="summary-stats">
    <div class="stat-item">
      <span class="stat-label">Total Users</span>
      <span class="stat-value">{{ users.length }}</span>
    </div>
    <div class="stat-item">
      <span class="stat-label">Active</span>
      <span class="stat-value stat-active">{{ activeCount }}</span>
    </div>
    <div class="stat-item">
      <span class="stat-label">Inactive</span>
      <span class="stat-value stat-inactive">{{ inactiveCount }}</span>
    </div>
  </div>
</div>
```

---

## 🔍 Search Components

### Basic Search Bar
```html
<div class="search-bar">
  <i class="fas fa-search"></i>
  <input
    type="text"
    class="search-input"
    [(ngModel)]="searchTerm"
    placeholder="Search..."
  />
  <span class="search-hint" *ngIf="searchTerm">
    Showing {{ filteredItems.length }} of {{ totalItems }} results
  </span>
</div>
```

### Search Box (compact version for filters)
```html
<div class="search-box">
  <i class="fas fa-search"></i>
  <input
    type="text"
    class="search-input"
    [(ngModel)]="searchTerm"
    placeholder="Search..."
  />
</div>
```

---

## 📱 Responsive Utilities

### Flex Utilities
```html
<!-- Horizontal flex with centered items -->
<div class="flex items-center gap-4">
  <i class="fas fa-user"></i>
  <span>User Name</span>
</div>

<!-- Vertical flex -->
<div class="flex flex-col gap-2">
  <div>Item 1</div>
  <div>Item 2</div>
</div>

<!-- Space between items -->
<div class="flex justify-between items-center">
  <span>Left</span>
  <span>Right</span>
</div>
```

### Grid Utilities
```html
<!-- 2-column grid (responsive) -->
<div class="grid-2">
  <div>Column 1</div>
  <div>Column 2</div>
</div>

<!-- 3-column grid (responsive) -->
<div class="grid-3">
  <div>Column 1</div>
  <div>Column 2</div>
  <div>Column 3</div>
</div>

<!-- 4-column grid (responsive) -->
<div class="grid-4">
  <div>Column 1</div>
  <div>Column 2</div>
  <div>Column 3</div>
  <div>Column 4</div>
</div>
```

### Responsive Visibility
```html
<!-- Show only on mobile -->
<div class="mobile-only">
  Mobile content
</div>

<!-- Show only on desktop -->
<div class="desktop-only">
  Desktop content
</div>
```

### Spacing Utilities
```html
<!-- Margin bottom -->
<div class="mb-4">Element with bottom margin</div>

<!-- Margin top -->
<div class="mt-6">Element with top margin</div>

<!-- No margin -->
<h3 class="mb-0">Heading with no bottom margin</h3>
```

---

## 🎯 Common Patterns

### Form in Modal
```html
<div class="modal-overlay" *ngIf="showModal">
  <div class="modal-content">
    <div class="modal-header">
      <h2>{{ isEdit ? 'Edit User' : 'Create User' }}</h2>
      <button class="btn-close" (click)="closeModal()">×</button>
    </div>

    <div class="modal-body">
      <div class="alert alert-danger" *ngIf="errorMessage">
        <i class="fas fa-exclamation-circle"></i>
        {{ errorMessage }}
      </div>

      <form (ngSubmit)="saveUser()">
        <div class="form-section">
          <h3>Basic Information</h3>
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">First Name <span class="required">*</span></label>
              <input type="text" id="firstName" class="form-control" required />
            </div>
            <div class="form-group">
              <label for="lastName">Last Name <span class="required">*</span></label>
              <input type="text" id="lastName" class="form-control" required />
            </div>
          </div>
        </div>
      </form>
    </div>

    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="closeModal()">
        Cancel
      </button>
      <button class="btn btn-primary" (click)="saveUser()" [disabled]="loading">
        <i *ngIf="loading" class="fas fa-spinner fa-spin"></i>
        <i *ngIf="!loading" class="fas fa-save"></i>
        {{ loading ? 'Saving...' : 'Save' }}
      </button>
    </div>
  </div>
</div>
```

### Card Grid with Loading
```html
<div *ngIf="loading" class="loading-state">
  <div class="spinner"></div>
  <p>Loading items...</p>
</div>

<div *ngIf="!loading && items.length > 0" class="settings-grid">
  <div *ngFor="let item of items" class="settings-card">
    <!-- Card content -->
  </div>
</div>

<div *ngIf="!loading && items.length === 0" class="empty-state">
  <i class="fas fa-inbox"></i>
  <h3>No Items Found</h3>
  <button class="btn btn-primary" (click)="createItem()">
    <i class="fas fa-plus"></i>
    Create Item
  </button>
</div>
```

### Pagination
```html
<div class="pagination">
  <button class="page-item" [disabled]="currentPage === 1" (click)="previousPage()">
    <span class="page-link">
      <i class="fas fa-chevron-left"></i>
      Previous
    </span>
  </button>

  <div class="page-numbers">
    <button
      *ngFor="let page of pageNumbers"
      class="page-item"
      [class.active]="page === currentPage"
      (click)="goToPage(page)"
    >
      <span class="page-link">{{ page }}</span>
    </button>
  </div>

  <button class="page-item" [disabled]="currentPage === totalPages" (click)="nextPage()">
    <span class="page-link">
      Next
      <i class="fas fa-chevron-right"></i>
    </span>
  </button>
</div>

<div class="pagination-info text-center mt-4">
  Showing {{ (currentPage - 1) * pageSize + 1 }}-{{ Math.min(currentPage * pageSize, totalItems) }}
  of {{ totalItems }} items
</div>
```

---

## 💡 Tips & Best Practices

### 1. Always Use Design Tokens
```scss
// ❌ Don't do this
.my-component {
  color: #333;
  padding: 16px;
}

// ✅ Do this instead
.my-component {
  color: var(--text-primary);
  padding: $spacing-4;
}
```

### 2. Use Utility Classes
```html
<!-- ❌ Don't do this -->
<div style="display: flex; align-items: center; gap: 16px;">

<!-- ✅ Do this instead -->
<div class="flex items-center gap-4">
```

### 3. Follow Component Structure
```html
<!-- ✅ Proper card structure -->
<div class="card">
  <div class="card-header">...</div>
  <div class="card-body">...</div>
  <div class="card-footer">...</div>
</div>
```

### 4. Use Semantic HTML
```html
<!-- ❌ Don't do this -->
<div class="btn" onclick="doSomething()">Click Me</div>

<!-- ✅ Do this instead -->
<button class="btn btn-primary" (click)="doSomething()">
  Click Me
</button>
```

### 5. Include Icons Meaningfully
```html
<!-- ✅ Icons enhance understanding -->
<button class="btn btn-success">
  <i class="fas fa-check"></i>
  Approve
</button>

<div class="alert alert-success">
  <i class="fas fa-check-circle"></i>
  Success message
</div>
```

---

## 🔗 Quick Reference Table

| Component | Class | Usage |
|-----------|-------|-------|
| Primary Button | `.btn .btn-primary` | Main actions |
| Secondary Button | `.btn .btn-secondary` | Alternative actions |
| Success Alert | `.alert .alert-success` | Success messages |
| Error Alert | `.alert .alert-danger` | Error messages |
| Card | `.card` | Container component |
| Modal | `.modal-overlay .modal-content` | Dialogs |
| Form Group | `.form-group` | Form field wrapper |
| Input Field | `.form-control` | Text inputs |
| Badge | `.badge .badge-success` | Status indicators |
| Loading | `.loading-state` | Loading animation |
| Empty State | `.empty-state` | No data display |
| Page Header | `.page-header` | Page title section |
| Stats Bar | `.stats-bar` | Statistics display |

---

**Last Updated:** November 2, 2025
**Version:** 1.0
**Project:** Complaint Management System
