# Frontend Implementation Guide - Complaint Registration Improvements

## ✅ What's Already Complete

### Backend (100% Complete & Tested)
- ✅ Database schema with all migrations applied
- ✅ Auto-population logic fully functional
- ✅ Settings API (GET/PUT endpoints) working
- ✅ All DTOs, handlers, repositories implemented

### Frontend Models & Services (100% Complete)
- ✅ `complaint.model.ts` - Updated with all new fields
- ✅ `complaint-info-settings.model.ts` - Settings interfaces created
- ✅ `complaint.service.ts` - Complete HTTP service for complaints API
- ✅ `complaint-info-settings.service.ts` - Complete HTTP service for settings API

---

## 📋 Remaining Frontend Components to Build

### 1. Complaint List Component
**Location:** `src/app/components/complaints/complaint-list/`

**Files to Create:**
- `complaint-list.component.ts`
- `complaint-list.component.html`
- `complaint-list.component.css`

**Key Features:**
- Display paginated list of complaints
- Filters: Status, Priority, Search
- Columns: Complaint#, Title, Complainant, Status, Priority, Due Date, Actions
- Click row to view details
- "Create New" button

**Sample TypeScript:**
```typescript
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ComplaintService } from '../../../services/complaint.service';
import { Complaint, ComplaintStatus, ComplaintPriority, PagedResult } from '../../../models/complaint.model';

@Component({
  selector: 'app-complaint-list',
  templateUrl: './complaint-list.component.html',
  styleUrls: ['./complaint-list.component.css']
})
export class ComplaintListComponent implements OnInit {
  complaints: Complaint[] = [];
  loading = false;
  error: string | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Filters
  statusFilter?: ComplaintStatus;
  priorityFilter?: ComplaintPriority;
  searchTerm = '';

  // Enums for template
  ComplaintStatus = ComplaintStatus;
  ComplaintPriority = ComplaintPriority;

  constructor(
    private complaintService: ComplaintService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadComplaints();
  }

  loadComplaints(): void {
    this.loading = true;
    this.error = null;

    this.complaintService.getComplaints(
      this.currentPage,
      this.pageSize,
      this.statusFilter,
      this.priorityFilter,
      undefined,
      undefined,
      this.searchTerm
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaints = response.data.items;
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load complaints';
        this.loading = false;
        console.error(err);
      }
    });
  }

  viewComplaint(id: string): void {
    this.router.navigate(['/complaints', id]);
  }

  createComplaint(): void {
    this.router.navigate(['/complaints/new']);
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadComplaints();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadComplaints();
  }

  getPriorityClass(priority: string): string {
    switch(priority) {
      case 'Critical': return 'badge-danger';
      case 'Urgent': return 'badge-warning';
      case 'High': return 'badge-primary';
      case 'Normal': return 'badge-info';
      default: return 'badge-secondary';
    }
  }

  getStatusClass(status: string): string {
    switch(status) {
      case 'Closed': return 'badge-success';
      case 'Resolved': return 'badge-success';
      case 'InProgress': return 'badge-primary';
      case 'Escalated': return 'badge-danger';
      case 'Submitted': return 'badge-info';
      default: return 'badge-secondary';
    }
  }
}
```

---

### 2. Complaint Detail Component
**Location:** `src/app/components/complaints/complaint-detail/`

**Files to Create:**
- `complaint-detail.component.ts`
- `complaint-detail.component.html`
- `complaint-detail.component.css`

**Key Features:**
- Display full complaint details
- **NEW: Complainant Information Card** with:
  - Personal Details (name, employee code, email, phone, preferred contact method)
  - Organizational Hierarchy (company, branch, department, section)
  - Manager Details (name, email, phone) - if available
- Comments section
- Attachments section
- Action buttons (Assign, Escalate, Close, etc.)

**Complainant Info Card Sample (HTML):**
```html
<div class="card mt-3">
  <div class="card-header">
    <h5>Complainant Information</h5>
  </div>
  <div class="card-body">
    <div class="row">
      <!-- Personal Details -->
      <div class="col-md-6">
        <h6 class="text-muted">Personal Details</h6>
        <table class="table table-sm">
          <tr>
            <th>Name:</th>
            <td>{{ complaint.complainantName }}</td>
          </tr>
          <tr>
            <th>Employee Code:</th>
            <td>{{ complaint.complainantEmployeeCode }}</td>
          </tr>
          <tr>
            <th>Email:</th>
            <td>{{ complaint.contactEmail }}</td>
          </tr>
          <tr>
            <th>Phone:</th>
            <td>{{ complaint.contactPhone }}</td>
          </tr>
          <tr *ngIf="complaint.alternatePhone">
            <th>Alternate Phone:</th>
            <td>{{ complaint.alternatePhone }}</td>
          </tr>
          <tr>
            <th>Preferred Contact:</th>
            <td>
              <span class="badge badge-info">
                {{ getPreferredContactLabel(complaint.preferredContactMethod) }}
              </span>
            </td>
          </tr>
        </table>
      </div>

      <!-- Organizational Hierarchy -->
      <div class="col-md-6">
        <h6 class="text-muted">Organizational Details</h6>
        <table class="table table-sm">
          <tr>
            <th>Company:</th>
            <td>{{ complaint.companyName }}</td>
          </tr>
          <tr *ngIf="complaint.complainantBranchName">
            <th>Branch:</th>
            <td>{{ complaint.complainantBranchName }}</td>
          </tr>
          <tr *ngIf="complaint.complainantDepartmentName">
            <th>Department:</th>
            <td>{{ complaint.complainantDepartmentName }}</td>
          </tr>
          <tr *ngIf="complaint.complainantSectionName">
            <th>Section:</th>
            <td>{{ complaint.complainantSectionName }}</td>
          </tr>
        </table>

        <!-- Manager Details -->
        <div *ngIf="complaint.complainantManagerName">
          <h6 class="text-muted mt-3">Manager Details</h6>
          <table class="table table-sm">
            <tr>
              <th>Name:</th>
              <td>{{ complaint.complainantManagerName }}</td>
            </tr>
            <tr *ngIf="complaint.complainantManagerEmail">
              <th>Email:</th>
              <td>{{ complaint.complainantManagerEmail }}</td>
            </tr>
            <tr *ngIf="complaint.complainantManagerPhone">
              <th>Phone:</th>
              <td>{{ complaint.complainantManagerPhone }}</td>
            </tr>
          </table>
        </div>
      </div>
    </div>
  </div>
</div>
```

---

### 3. Complaint Form Component
**Location:** `src/app/components/complaints/complaint-form/`

**Files to Create:**
- `complaint-form.component.ts`
- `complaint-form.component.html`
- `complaint-form.component.css`

**Key Features:**
- Reactive form with validation
- **NEW Fields:**
  - Section dropdown (loads sections based on department)
  - Contact Email (readonly, auto-populated from user)
  - Contact Phone (editable, auto-populated from user)
  - Alternate Phone (optional)
  - Preferred Contact Method (dropdown: Email, Phone, Both, SMS, In-App)
- Auto-population from user profile
- File upload for attachments

**Sample Form Initialization:**
```typescript
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PreferredContactMethod } from '../../../models/complaint.model';
import { AuthService } from '../../../services/auth.service';

export class ComplaintFormComponent implements OnInit {
  complaintForm: FormGroup;
  sections: Section[] = [];

  PreferredContactMethod = PreferredContactMethod;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private complaintService: ComplaintService,
    private sectionService: SectionService
  ) {
    this.complaintForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      categoryId: ['', Validators.required],
      priority: [1, Validators.required],
      branchId: [''],
      departmentId: [''],
      sectionId: [''],
      contactEmail: ['', [Validators.email]],
      contactPhone: [''],
      alternatePhone: [''],
      preferredContactMethod: [PreferredContactMethod.Both],
      isAnonymous: [false],
      tags: ['']
    });
  }

  ngOnInit(): void {
    this.autoPopulateUserInfo();
  }

  autoPopulateUserInfo(): void {
    const currentUser = this.authService.getCurrentUser();

    if (currentUser) {
      this.complaintForm.patchValue({
        branchId: currentUser.branchId,
        departmentId: currentUser.departmentId,
        sectionId: currentUser.sectionId,
        contactEmail: currentUser.email,
        contactPhone: currentUser.phone
      });

      // Make email readonly since it's auto-populated
      this.complaintForm.get('contactEmail')?.disable();

      // Load sections if department is set
      if (currentUser.departmentId) {
        this.loadSections(currentUser.departmentId);
      }
    }
  }

  loadSections(departmentId: string): void {
    this.sectionService.getSectionsByDepartment(departmentId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.sections = response.data;
        }
      },
      error: (err) => console.error('Failed to load sections', err)
    });
  }

  onDepartmentChange(departmentId: string): void {
    this.complaintForm.patchValue({ sectionId: '' });
    this.sections = [];

    if (departmentId) {
      this.loadSections(departmentId);
    }
  }

  onSubmit(): void {
    if (this.complaintForm.valid) {
      const formValue = this.complaintForm.getRawValue(); // getRawValue() includes disabled fields

      this.complaintService.createComplaint(formValue).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.router.navigate(['/complaints', response.data.id]);
          }
        },
        error: (err) => {
          console.error('Failed to create complaint', err);
        }
      });
    }
  }
}
```

---

### 4. Complaint Info Settings Component
**Location:** `src/app/components/admin/complaint-info-settings/`

**Files to Create:**
- `complaint-info-settings.component.ts`
- `complaint-info-settings.component.html`
- `complaint-info-settings.component.css`

**Key Features:**
- Form with 25 checkbox/number settings organized in sections:
  - Handler Visibility (9 checkboxes)
  - Management Visibility (3 checkboxes)
  - Privacy Settings (2 checkboxes + 1 number input)
  - Report Settings (5 checkboxes)
- Save/Reset buttons
- Load settings on init

**Sample TypeScript:**
```typescript
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ComplaintInfoSettingsService } from '../../../services/complaint-info-settings.service';
import { AuthService } from '../../../services/auth.service';
import { ComplaintInformationSettings } from '../../../models/complaint-info-settings.model';

@Component({
  selector: 'app-complaint-info-settings',
  templateUrl: './complaint-info-settings.component.html',
  styleUrls: ['./complaint-info-settings.component.css']
})
export class ComplaintInfoSettingsComponent implements OnInit {
  settingsForm: FormGroup;
  loading = false;
  saving = false;
  error: string | null = null;
  successMessage: string | null = null;
  companyId: string = '';

  constructor(
    private fb: FormBuilder,
    private settingsService: ComplaintInfoSettingsService,
    private authService: AuthService
  ) {
    this.settingsForm = this.fb.group({
      // Handler visibility
      showEmployeeCodeToHandlers: [true],
      showEmailToHandlers: [true],
      showPhoneToHandlers: [true],
      showBranchToHandlers: [true],
      showDepartmentToHandlers: [true],
      showSectionToHandlers: [true],
      showJobTitleToHandlers: [true],
      showManagerDetailsToHandlers: [true],
      showPreviousComplaintsToHandlers: [true],

      // Management visibility
      showEmployeeAddressToManagement: [false],
      showEmergencyContactToManagement: [false],
      showPerformanceMetricsToManagement: [false],

      // Privacy settings
      maskPersonalInfoInLogs: [true],
      redactInfoAfterClosure: [false],
      dataRetentionDays: [0],

      // Report settings
      includeEmployeeCodeInReports: [true],
      includeEmailInReports: [true],
      includePhoneInReports: [true],
      maskEmailInReports: [false],
      maskPhoneInReports: [false]
    });
  }

  ngOnInit(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser?.companyId) {
      this.companyId = currentUser.companyId;
      this.loadSettings();
    }
  }

  loadSettings(): void {
    this.loading = true;
    this.error = null;

    this.settingsService.getSettings(this.companyId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.settingsForm.patchValue(response.data);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load settings';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSave(): void {
    if (this.settingsForm.valid) {
      this.saving = true;
      this.error = null;
      this.successMessage = null;

      this.settingsService.updateSettings(this.companyId, this.settingsForm.value).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Settings updated successfully';
            setTimeout(() => this.successMessage = null, 3000);
          }
          this.saving = false;
        },
        error: (err) => {
          this.error = 'Failed to save settings';
          this.saving = false;
          console.error(err);
        }
      });
    }
  }

  onReset(): void {
    this.loadSettings();
  }
}
```

---

## 🔗 Routing Configuration

Add to `app-routing.module.ts`:

```typescript
const routes: Routes = [
  // ... existing routes

  // Complaints routes
  {
    path: 'complaints',
    children: [
      { path: '', component: ComplaintListComponent },
      { path: 'new', component: ComplaintFormComponent },
      { path: ':id', component: ComplaintDetailComponent },
      { path: ':id/edit', component: ComplaintFormComponent }
    ]
  },

  // Admin routes
  {
    path: 'admin',
    children: [
      // ... existing admin routes
      { path: 'complaint-info-settings', component: ComplaintInfoSettingsComponent }
    ]
  }
];
```

---

## 🎨 UI/UX Guidelines

### Preferred Contact Method Labels
```typescript
getPreferredContactLabel(method: PreferredContactMethod): string {
  switch(method) {
    case PreferredContactMethod.Email: return 'Email Only';
    case PreferredContactMethod.Phone: return 'Phone Only';
    case PreferredContactMethod.Both: return 'Email & Phone';
    case PreferredContactMethod.SMS: return 'SMS';
    case PreferredContactMethod.InApp: return 'In-App Notification';
    default: return 'Not Specified';
  }
}
```

### Bootstrap Classes for Status/Priority
- **Priority:**
  - Critical/Urgent: `badge-danger`
  - High: `badge-warning`
  - Normal: `badge-primary`
  - Low: `badge-secondary`

- **Status:**
  - Closed/Resolved: `badge-success`
  - In Progress: `badge-primary`
  - Escalated: `badge-danger`
  - Submitted: `badge-info`

---

## ✅ Testing Checklist

### Complaint Form
- [ ] Auto-populates user email, phone, branch, department, section
- [ ] Email field is readonly when auto-populated
- [ ] Section dropdown loads based on selected department
- [ ] Preferred contact method dropdown works
- [ ] Form validation works correctly
- [ ] File upload works

### Complaint Detail
- [ ] Complainant info card displays all fields correctly
- [ ] Manager details show when available
- [ ] Contact information displays properly
- [ ] Preferred contact method badge shows correct label

### Settings Page
- [ ] Loads current settings on init
- [ ] All 25 settings can be toggled/edited
- [ ] Save button updates settings successfully
- [ ] Reset button reloads original settings
- [ ] Success/error messages display correctly

---

## 🔧 Environment Configuration

Ensure `environment.ts` has:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5058/api'
};
```

---

## 📊 Backend API Endpoints (Already Working)

### Complaints
- `GET /api/complaints` - List complaints with filters
- `GET /api/complaints/{id}` - Get complaint details
- `POST /api/complaints` - Create complaint (auto-populates fields)
- `PUT /api/complaints/{id}` - Update complaint
- `POST /api/complaints/{id}/assign/{userId}` - Assign complaint
- `POST /api/complaints/{id}/escalate` - Escalate complaint
- `POST /api/complaints/{id}/close` - Close complaint

### Settings
- `GET /api/ComplaintInfoSettings/{companyId}` - Get settings
- `PUT /api/ComplaintInfoSettings/{companyId}` - Update settings

---

## 🚀 Next Steps

1. **Create Complaint List Component** - Start here, basic CRUD
2. **Create Complaint Form Component** - Implement auto-population
3. **Create Complaint Detail Component** - Add complainant info card
4. **Create Settings Component** - Admin configuration page
5. **Add Routing** - Connect all components
6. **Test End-to-End** - Verify full workflow

---

## 📝 Notes

- Backend is **100% complete and tested** ✅
- All TypeScript models are updated ✅
- All services (complaint.service.ts & complaint-info-settings.service.ts) are ready ✅
- Components need to be built following the samples above
- Use Bootstrap 4/5 for styling (already in project)
- Follow existing component patterns in the project

---

**Estimated Time to Complete Frontend:** 6-8 hours
- Complaint List: 1-2 hours
- Complaint Detail + Info Card: 2-3 hours
- Complaint Form + Auto-population: 2-3 hours
- Settings Page: 1-2 hours
- Routing & Testing: 1 hour
