# SMS & WhatsApp Configuration Pages - Implementation Guide

**Created**: October 22, 2025
**Purpose**: Complete guide to implement SMS Gateway and WhatsApp Settings management pages
**Status**: Ready for implementation

---

## Overview

The backend database tables and notification system are ready for SMS and WhatsApp. The frontend configuration pages need to be created to allow administrators to manage these settings through the UI.

**Database Tables Exist**:
- ✅ `SmsGatewaySettings` (24 columns)
- ✅ `WhatsAppSettings` (31 columns)

**Backend Services Needed**:
- ⏳ API endpoints for SMS settings CRUD
- ⏳ API endpoints for WhatsApp settings CRUD
- ⏳ Service interfaces (ISmsService, IWhatsAppService)

**Frontend Components Needed**:
- ⏳ SMS Gateway Management Component
- ⏳ WhatsApp Settings Management Component
- ⏳ Navigation links in dashboard
- ⏳ Routes configuration

---

## Part 1: SMS Gateway Settings Page

### Database Schema Reference

**Table**: `SmsGatewaySettings`

| Column | Data Type | Purpose |
|--------|-----------|---------|
| Id | uniqueidentifier | Primary key |
| Name | nvarchar | Configuration name (e.g., "Twilio Production") |
| Provider | nvarchar | Provider name (Twilio, AWS SNS, etc.) |
| ApiUrl | nvarchar | API endpoint URL |
| AccountSid | nvarchar | Provider account SID/ID |
| AuthToken | nvarchar | Authentication token/API key |
| FromNumber | nvarchar | Sender phone number (+1234567890) |
| SenderName | nvarchar | Sender name for display |
| IsActive | bit | Active status |
| IsDefault | bit | Default configuration flag |
| MaxSmsPerHour | int | Rate limit |
| CostPerSms | decimal | Cost tracking per SMS |
| TimeoutSeconds | int | API timeout |
| CompanyId | uniqueidentifier | Multi-tenant isolation |
| AdditionalConfig | nvarchar | JSON for provider-specific settings |
| TestNotes | nvarchar | Test results/notes |
| LastTestedAt | datetime2 | Last test timestamp |
| CreatedAt | datetime2 | Creation timestamp |
| CreatedBy | uniqueidentifier | Creator user ID |
| UpdatedAt | datetime2 | Last update timestamp |
| UpdatedBy | uniqueidentifier | Last updater user ID |
| IsDeleted | bit | Soft delete flag |
| DeletedAt | datetime2 | Deletion timestamp |
| DeletedBy | uniqueidentifier | Deleter user ID |

### Component Structure

**Location**: `src/app/components/admin/sms-gateway-management/`

**Files to Create**:
```
sms-gateway-management/
├── sms-gateway-management.component.ts
├── sms-gateway-management.component.html
├── sms-gateway-management.component.scss
└── README.md
```

### TypeScript Component Template

```typescript
// sms-gateway-management.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SmsGatewayService } from '../../../services/sms-gateway.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';

export interface SmsGatewaySettings {
  id: string;
  name: string;
  provider: string;
  apiUrl: string;
  accountSid: string;
  authToken: string;
  fromNumber: string;
  senderName: string;
  isActive: boolean;
  isDefault: boolean;
  maxSmsPerHour: number;
  costPerSms: number;
  timeoutSeconds: number;
  companyId: string;
  additionalConfig?: string;
  testNotes?: string;
  lastTestedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateSmsGatewayRequest {
  name: string;
  provider: string;
  apiUrl: string;
  accountSid: string;
  authToken: string;
  fromNumber: string;
  senderName: string;
  isActive: boolean;
  isDefault: boolean;
  maxSmsPerHour?: number;
  costPerSms?: number;
  timeoutSeconds?: number;
  additionalConfig?: string;
  testNotes?: string;
}

@Component({
  selector: 'app-sms-gateway-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sms-gateway-management.component.html',
  styleUrls: ['./sms-gateway-management.component.scss']
})
export class SmsGatewayManagementComponent implements OnInit {
  // Data
  settings: SmsGatewaySettings[] = [];
  filteredSettings: SmsGatewaySettings[] = [];

  // UI State
  loading = false;
  showModal = false;
  isEditMode = false;
  errorMessage = '';
  successMessage = '';

  // Form
  form: CreateSmsGatewayRequest = this.getEmptyForm();
  selectedId: string | null = null;

  // Test SMS
  isTestingSms = false;
  testPhoneNumber = '';
  testMessage = '';
  testResult: { success: boolean; message: string } | null = null;

  // Providers
  providers = ['Twilio', 'AWS SNS', 'Nexmo', 'MessageBird', 'Custom'];

  // Filter
  statusFilter: 'all' | 'active' | 'inactive' = 'active';

  constructor(
    private smsGatewayService: SmsGatewayService,
    private authService: AuthService,
    private logger: LoggerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  getEmptyForm(): CreateSmsGatewayRequest {
    return {
      name: '',
      provider: 'Twilio',
      apiUrl: '',
      accountSid: '',
      authToken: '',
      fromNumber: '',
      senderName: '',
      isActive: true,
      isDefault: false,
      maxSmsPerHour: 1000,
      costPerSms: 0.0075,
      timeoutSeconds: 30
    };
  }

  loadSettings(): void {
    this.loading = true;
    this.smsGatewayService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.settings = response.data;
          this.applyFilter();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load SMS gateway settings';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    if (this.statusFilter === 'all') {
      this.filteredSettings = this.settings;
    } else if (this.statusFilter === 'active') {
      this.filteredSettings = this.settings.filter(s => s.isActive);
    } else {
      this.filteredSettings = this.settings.filter(s => !s.isActive);
    }
  }

  openAddModal(): void {
    this.form = this.getEmptyForm();
    this.isEditMode = false;
    this.selectedId = null;
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(setting: SmsGatewaySettings): void {
    this.form = {
      name: setting.name,
      provider: setting.provider,
      apiUrl: setting.apiUrl,
      accountSid: setting.accountSid,
      authToken: setting.authToken,
      fromNumber: setting.fromNumber,
      senderName: setting.senderName,
      isActive: setting.isActive,
      isDefault: setting.isDefault,
      maxSmsPerHour: setting.maxSmsPerHour,
      costPerSms: setting.costPerSms,
      timeoutSeconds: setting.timeoutSeconds,
      additionalConfig: setting.additionalConfig,
      testNotes: setting.testNotes
    };
    this.isEditMode = true;
    this.selectedId = setting.id;
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.form = this.getEmptyForm();
    this.selectedId = null;
    this.isEditMode = false;
  }

  save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;

    if (this.isEditMode && this.selectedId) {
      this.smsGatewayService.update(this.selectedId, this.form).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'SMS gateway settings updated successfully';
            this.closeModal();
            this.loadSettings();
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update settings';
          this.loading = false;
        }
      });
    } else {
      this.smsGatewayService.create(this.form).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'SMS gateway settings created successfully';
            this.closeModal();
            this.loadSettings();
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create settings';
          this.loading = false;
        }
      });
    }
  }

  validateForm(): boolean {
    if (!this.form.name || this.form.name.trim() === '') {
      this.errorMessage = 'Name is required';
      return false;
    }
    if (!this.form.apiUrl || this.form.apiUrl.trim() === '') {
      this.errorMessage = 'API URL is required';
      return false;
    }
    if (!this.form.fromNumber || this.form.fromNumber.trim() === '') {
      this.errorMessage = 'From Number is required';
      return false;
    }
    return true;
  }

  toggleActive(setting: SmsGatewaySettings): void {
    const updatedSetting = { ...setting, isActive: !setting.isActive };
    this.smsGatewayService.update(setting.id, updatedSetting).subscribe({
      next: () => {
        this.loadSettings();
      },
      error: (error) => {
        this.errorMessage = 'Failed to update status';
      }
    });
  }

  delete(id: string): void {
    if (!confirm('Are you sure you want to delete this SMS gateway configuration?')) {
      return;
    }

    this.smsGatewayService.delete(id).subscribe({
      next: () => {
        this.successMessage = 'SMS gateway deleted successfully';
        this.loadSettings();
      },
      error: (error) => {
        this.errorMessage = 'Failed to delete SMS gateway';
      }
    });
  }

  testSms(settingId: string): void {
    if (!this.testPhoneNumber || !this.testMessage) {
      this.testResult = { success: false, message: 'Please enter phone number and message' };
      return;
    }

    this.isTestingSms = true;
    this.testResult = null;

    this.smsGatewayService.testSms(settingId, this.testPhoneNumber, this.testMessage).subscribe({
      next: (response) => {
        this.testResult = {
          success: response.isSuccess,
          message: response.message || 'Test SMS sent successfully'
        };
        this.isTestingSms = false;
      },
      error: (error) => {
        this.testResult = {
          success: false,
          message: error.error?.message || 'Failed to send test SMS'
        };
        this.isTestingSms = false;
      }
    });
  }
}
```

### HTML Template

```html
<!-- sms-gateway-management.component.html -->
<div class="sms-gateway-management">
  <div class="page-header">
    <h2><i class="bi bi-phone"></i> SMS Gateway Settings</h2>
    <button class="btn btn-primary" (click)="openAddModal()">
      <i class="bi bi-plus-circle"></i> Add SMS Gateway
    </button>
  </div>

  <!-- Filters -->
  <div class="filters">
    <div class="filter-group">
      <label>Status:</label>
      <select [(ngModel)]="statusFilter" (change)="applyFilter()" class="form-select">
        <option value="all">All</option>
        <option value="active">Active Only</option>
        <option value="inactive">Inactive Only</option>
      </select>
    </div>
  </div>

  <!-- Messages -->
  <div class="alert alert-success" *ngIf="successMessage">
    {{ successMessage }}
    <button class="btn-close" (click)="successMessage = ''"></button>
  </div>
  <div class="alert alert-danger" *ngIf="errorMessage">
    {{ errorMessage }}
    <button class="btn-close" (click)="errorMessage = ''"></button>
  </div>

  <!-- Loading -->
  <div class="loading" *ngIf="loading">
    <div class="spinner-border"></div>
    <p>Loading SMS gateway settings...</p>
  </div>

  <!-- Settings List -->
  <div class="settings-grid" *ngIf="!loading">
    <div class="setting-card" *ngFor="let setting of filteredSettings">
      <div class="card-header">
        <h3>{{ setting.name }}</h3>
        <div class="badge" [class.badge-success]="setting.isActive" [class.badge-secondary]="!setting.isActive">
          {{ setting.isActive ? 'Active' : 'Inactive' }}
        </div>
        <div class="badge badge-primary" *ngIf="setting.isDefault">Default</div>
      </div>

      <div class="card-body">
        <div class="info-row">
          <span class="label">Provider:</span>
          <span class="value">{{ setting.provider }}</span>
        </div>
        <div class="info-row">
          <span class="label">From Number:</span>
          <span class="value">{{ setting.fromNumber }}</span>
        </div>
        <div class="info-row">
          <span class="label">Sender Name:</span>
          <span class="value">{{ setting.senderName }}</span>
        </div>
        <div class="info-row">
          <span class="label">Max SMS/Hour:</span>
          <span class="value">{{ setting.maxSmsPerHour }}</span>
        </div>
        <div class="info-row">
          <span class="label">Cost per SMS:</span>
          <span class="value">${{ setting.costPerSms?.toFixed(4) }}</span>
        </div>
        <div class="info-row" *ngIf="setting.lastTestedAt">
          <span class="label">Last Tested:</span>
          <span class="value">{{ setting.lastTestedAt | date:'short' }}</span>
        </div>
      </div>

      <div class="card-footer">
        <button class="btn btn-sm btn-outline-primary" (click)="openEditModal(setting)">
          <i class="bi bi-pencil"></i> Edit
        </button>
        <button class="btn btn-sm"
                [class.btn-outline-success]="!setting.isActive"
                [class.btn-outline-warning]="setting.isActive"
                (click)="toggleActive(setting)">
          <i class="bi" [class.bi-check-circle]="!setting.isActive" [class.bi-x-circle]="setting.isActive"></i>
          {{ setting.isActive ? 'Deactivate' : 'Activate' }}
        </button>
        <button class="btn btn-sm btn-outline-danger" (click)="delete(setting.id)">
          <i class="bi bi-trash"></i> Delete
        </button>
      </div>
    </div>

    <div class="empty-state" *ngIf="filteredSettings.length === 0">
      <i class="bi bi-phone"></i>
      <h3>No SMS Gateway Configured</h3>
      <p>Add your first SMS gateway to start sending notifications</p>
      <button class="btn btn-primary" (click)="openAddModal()">
        <i class="bi bi-plus-circle"></i> Add SMS Gateway
      </button>
    </div>
  </div>

  <!-- Add/Edit Modal -->
  <div class="modal" [class.show]="showModal" *ngIf="showModal">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ isEditMode ? 'Edit' : 'Add' }} SMS Gateway Settings</h3>
          <button class="btn-close" (click)="closeModal()"></button>
        </div>

        <div class="modal-body">
          <form>
            <!-- Name -->
            <div class="form-group">
              <label>Configuration Name *</label>
              <input type="text" class="form-control" [(ngModel)]="form.name" name="name"
                     placeholder="e.g., Twilio Production" required>
            </div>

            <!-- Provider -->
            <div class="form-group">
              <label>Provider *</label>
              <select class="form-select" [(ngModel)]="form.provider" name="provider">
                <option *ngFor="let provider of providers" [value]="provider">{{ provider }}</option>
              </select>
            </div>

            <!-- API URL -->
            <div class="form-group">
              <label>API URL *</label>
              <input type="url" class="form-control" [(ngModel)]="form.apiUrl" name="apiUrl"
                     placeholder="https://api.twilio.com/2010-04-01/Accounts/{AccountSid}/Messages.json" required>
            </div>

            <!-- Account SID -->
            <div class="form-group">
              <label>Account SID / API Key *</label>
              <input type="text" class="form-control" [(ngModel)]="form.accountSid" name="accountSid"
                     placeholder="ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" required>
            </div>

            <!-- Auth Token -->
            <div class="form-group">
              <label>Auth Token / API Secret *</label>
              <input type="password" class="form-control" [(ngModel)]="form.authToken" name="authToken"
                     placeholder="Enter authentication token" required>
            </div>

            <!-- From Number -->
            <div class="form-group">
              <label>From Number *</label>
              <input type="tel" class="form-control" [(ngModel)]="form.fromNumber" name="fromNumber"
                     placeholder="+1234567890" required>
              <small class="form-text">Include country code (e.g., +1 for USA)</small>
            </div>

            <!-- Sender Name -->
            <div class="form-group">
              <label>Sender Name</label>
              <input type="text" class="form-control" [(ngModel)]="form.senderName" name="senderName"
                     placeholder="CompanyName">
              <small class="form-text">Displayed name for SMS sender</small>
            </div>

            <!-- Max SMS Per Hour -->
            <div class="form-group">
              <label>Max SMS Per Hour</label>
              <input type="number" class="form-control" [(ngModel)]="form.maxSmsPerHour" name="maxSmsPerHour"
                     min="0" placeholder="1000">
            </div>

            <!-- Cost Per SMS -->
            <div class="form-group">
              <label>Cost Per SMS ($)</label>
              <input type="number" class="form-control" [(ngModel)]="form.costPerSms" name="costPerSms"
                     step="0.0001" min="0" placeholder="0.0075">
              <small class="form-text">For cost tracking purposes</small>
            </div>

            <!-- Timeout -->
            <div class="form-group">
              <label>Timeout (seconds)</label>
              <input type="number" class="form-control" [(ngModel)]="form.timeoutSeconds" name="timeoutSeconds"
                     min="5" max="120" placeholder="30">
            </div>

            <!-- Checkboxes -->
            <div class="form-check">
              <input type="checkbox" class="form-check-input" [(ngModel)]="form.isActive" name="isActive" id="isActive">
              <label class="form-check-label" for="isActive">Active</label>
            </div>

            <div class="form-check">
              <input type="checkbox" class="form-check-input" [(ngModel)]="form.isDefault" name="isDefault" id="isDefault">
              <label class="form-check-label" for="isDefault">Set as Default</label>
            </div>

            <!-- Test Notes -->
            <div class="form-group">
              <label>Test Notes</label>
              <textarea class="form-control" [(ngModel)]="form.testNotes" name="testNotes" rows="3"
                        placeholder="Add any notes about test results or configuration"></textarea>
            </div>
          </form>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" (click)="closeModal()">Cancel</button>
          <button class="btn btn-primary" (click)="save()" [disabled]="loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm"></span>
            {{ isEditMode ? 'Update' : 'Create' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</div>
```

### Service Interface

```typescript
// services/sms-gateway.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class SmsGatewayService {
  private apiUrl = `${environment.apiUrl}/api/sms-gateway-settings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  create(data: any): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, data);
  }

  update(id: string, data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  testSms(settingId: string, phoneNumber: string, message: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${settingId}/test`, {
      phoneNumber,
      message
    });
  }
}
```

---

## Part 2: WhatsApp Settings Page

### Database Schema Reference

**Table**: `WhatsAppSettings`

| Column | Data Type | Purpose |
|--------|-----------|---------|
| Id | uniqueidentifier | Primary key |
| Name | nvarchar | Configuration name |
| Provider | nvarchar | Provider (WhatsApp Business API, Twilio, etc.) |
| ApiUrl | nvarchar | API endpoint |
| BusinessAccountId | nvarchar | WhatsApp Business Account ID |
| PhoneNumberId | nvarchar | WhatsApp Phone Number ID |
| AccessToken | nvarchar | API access token |
| WebhookToken | nvarchar | Webhook verification token |
| FromNumber | nvarchar | WhatsApp phone number |
| BusinessName | nvarchar | Business display name |
| IsActive | bit | Active status |
| IsDefault | bit | Default configuration |
| MaxMessagesPerHour | int | Rate limit |
| TimeoutSeconds | int | API timeout |
| CompanyId | uniqueidentifier | Multi-tenant isolation |
| AdditionalConfig | nvarchar | JSON for additional settings |
| TestNotes | nvarchar | Test results/notes |
| LastTestedAt | datetime2 | Last test timestamp |
| MediaStorageType | nvarchar | Media storage type (Local, Azure, AWS) |
| MediaStoragePath | nvarchar | Storage path |
| MediaStorageConfig | nvarchar | Storage configuration JSON |
| MediaPublicBaseUrl | nvarchar | Public URL for media files |
| MediaRetentionDays | int | How long to keep media |
| MaxMediaSizeMB | int | Maximum media file size |
| CreatedAt | datetime2 | Creation timestamp |
| CreatedBy | uniqueidentifier | Creator user ID |
| UpdatedAt | datetime2 | Last update timestamp |
| UpdatedBy | uniqueidentifier | Last updater user ID |
| IsDeleted | bit | Soft delete flag |
| DeletedAt | datetime2 | Deletion timestamp |
| DeletedBy | uniqueidentifier | Deleter user ID |

### Component Structure

**Location**: `src/app/components/admin/whatsapp-settings/`

**Files to Create**:
```
whatsapp-settings/
├── whatsapp-settings.component.ts
├── whatsapp-settings.component.html
├── whatsapp-settings.component.scss
└── README.md
```

### TypeScript Component Template

```typescript
// whatsapp-settings.component.ts
// Similar structure to SMS Gateway component
// Add WhatsApp-specific fields: BusinessAccountId, PhoneNumberId, etc.
// Include media storage configuration
```

### Key Differences for WhatsApp Component

1. **Additional Fields**:
   - Business Account ID
   - Phone Number ID
   - Webhook Token
   - Media storage configuration
   - Media retention settings

2. **Media Configuration Section**:
   ```html
   <div class="media-config-section">
     <h4>Media Storage Configuration</h4>
     <!-- Storage type selector -->
     <!-- Storage path -->
     <!-- Public URL -->
     <!-- Retention days -->
   </div>
   ```

3. **Template Testing**:
   - Test with WhatsApp template messages
   - Template approval status display
   - Template variable preview

---

## Part 3: Backend API Endpoints

### SMS Gateway Endpoints to Create

**Controller**: `SmsGatewaySettingsController.cs`

```csharp
[ApiController]
[Route("api/sms-gateway-settings")]
public class SmsGatewaySettingsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSmsGatewayRequest request)

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSmsGatewayRequest request)

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)

    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestSms(Guid id, [FromBody] TestSmsRequest request)
}
```

### WhatsApp Settings Endpoints to Create

**Controller**: `WhatsAppSettingsController.cs`

```csharp
[ApiController]
[Route("api/whatsapp-settings")]
public class WhatsAppSettingsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWhatsAppSettingsRequest request)

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWhatsAppSettingsRequest request)

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)

    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestWhatsApp(Guid id, [FromBody] TestWhatsAppRequest request)

    [HttpGet("{id}/templates")]
    public async Task<IActionResult> GetTemplates(Guid id)
}
```

---

## Part 4: Navigation & Routing

### Dashboard Navigation Links

**File**: `complaint-system-angular/src/app/components/dashboard/dashboard.html`

**Add After Email Settings Link** (around line 88):

```html
<!-- SMS Gateway Settings -->
<a class="dropdown-item" (click)="navigateToAdmin('sms-gateway')">
  <i class="bi bi-phone"></i> SMS Gateway Settings
</a>

<!-- WhatsApp Settings -->
<a class="dropdown-item" (click)="navigateToAdmin('whatsapp')">
  <i class="bi bi-whatsapp"></i> WhatsApp Settings
</a>
```

### App Routes Configuration

**File**: `complaint-system-angular/src/app/app.routes.ts`

**Add Routes**:

```typescript
{
  path: 'admin/sms-gateway',
  component: SmsGatewayManagementComponent,
  canActivate: [AuthGuard]
},
{
  path: 'admin/whatsapp',
  component: WhatsAppSettingsComponent,
  canActivate: [AuthGuard]
},
```

---

## Part 5: Implementation Checklist

### Phase 1: Backend API (2-3 hours)

- [ ] Create `SmsGatewaySettingsController.cs`
- [ ] Create `WhatsAppSettingsController.cs`
- [ ] Create request/response DTOs
- [ ] Implement CRUD operations in controllers
- [ ] Add validation
- [ ] Test API endpoints with Postman/Swagger

### Phase 2: Frontend Services (1 hour)

- [ ] Create `sms-gateway.service.ts`
- [ ] Create `whatsapp-settings.service.ts`
- [ ] Add models/interfaces
- [ ] Test service methods

### Phase 3: SMS Gateway Component (2-3 hours)

- [ ] Create component folder structure
- [ ] Implement TypeScript component
- [ ] Create HTML template
- [ ] Add SCSS styling
- [ ] Test CRUD operations
- [ ] Test SMS sending

### Phase 4: WhatsApp Component (2-3 hours)

- [ ] Create component folder structure
- [ ] Implement TypeScript component (based on SMS template)
- [ ] Create HTML template with media config
- [ ] Add SCSS styling
- [ ] Test CRUD operations
- [ ] Test WhatsApp message sending

### Phase 5: Integration (1 hour)

- [ ] Add navigation links to dashboard
- [ ] Configure routes in app.routes.ts
- [ ] Test navigation
- [ ] Test page loading
- [ ] Verify permissions/auth guards

### Phase 6: Testing (1-2 hours)

- [ ] Test SMS configuration save/load
- [ ] Test WhatsApp configuration save/load
- [ ] Send test SMS
- [ ] Send test WhatsApp message
- [ ] Test media uploads (WhatsApp)
- [ ] Test rate limiting
- [ ] Test error handling

**Total Estimated Time**: 10-15 hours

---

## Part 6: Sample Provider Configurations

### Twilio SMS Configuration

```json
{
  "provider": "Twilio",
  "apiUrl": "https://api.twilio.com/2010-04-01/Accounts/{AccountSid}/Messages.json",
  "accountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "authToken": "your_auth_token",
  "fromNumber": "+1234567890",
  "senderName": "YourCompany"
}
```

### AWS SNS Configuration

```json
{
  "provider": "AWS SNS",
  "apiUrl": "https://sns.{region}.amazonaws.com",
  "accountSid": "AWS_ACCESS_KEY_ID",
  "authToken": "AWS_SECRET_ACCESS_KEY",
  "additionalConfig": {
    "region": "us-east-1",
    "messageType": "Transactional"
  }
}
```

### WhatsApp Business API Configuration

```json
{
  "provider": "WhatsApp Business API",
  "apiUrl": "https://graph.facebook.com/v17.0",
  "businessAccountId": "123456789012345",
  "phoneNumberId": "109876543210987",
  "accessToken": "your_access_token",
  "fromNumber": "+1234567890",
  "businessName": "Your Company Name",
  "mediaStorageType": "Azure",
  "mediaPublicBaseUrl": "https://yourstorage.blob.core.windows.net/whatsapp-media"
}
```

---

## Part 7: Testing Guide

### Test SMS Gateway

1. **Configure Twilio** (or another provider):
   - Sign up at https://www.twilio.com
   - Get Account SID and Auth Token
   - Purchase a phone number
   - Add configuration in UI

2. **Send Test SMS**:
   - Enter test phone number
   - Enter test message
   - Click "Test SMS"
   - Verify SMS received

3. **Check Logs**:
   ```sql
   SELECT * FROM CommunicationLogs
   WHERE Channel = 1 -- SMS
   ORDER BY CreatedAt DESC
   ```

### Test WhatsApp

1. **Set Up WhatsApp Business API**:
   - Apply for WhatsApp Business API access
   - Get Business Account ID
   - Get Phone Number ID
   - Generate Access Token
   - Create message templates

2. **Configure in UI**:
   - Add WhatsApp configuration
   - Test connection
   - Verify template sync

3. **Send Test Message**:
   - Select approved template
   - Enter recipient number
   - Send test
   - Verify delivery

---

## Part 8: Security Considerations

### Credential Storage

1. **Auth Tokens / API Keys**:
   - Store encrypted in database
   - Never log in plain text
   - Mask in UI (show only last 4 chars)

2. **API Access**:
   - Require admin role
   - Add audit logging
   - Rate limit test endpoints

3. **Webhook Security**:
   - Validate webhook signatures
   - Use secure webhook tokens
   - Implement IP whitelisting

---

## Part 9: Cost Tracking

### SMS Cost Tracking

```typescript
// Add to notification dispatcher
const cost = await calculateSmsCost(message, recipient);
await logCommunicationWithCost(cost);
```

### Monthly Report Query

```sql
SELECT
    Provider,
    COUNT(*) as MessagesSent,
    SUM(CostPerSms) as TotalCost,
    AVG(CostPerSms) as AverageCost
FROM CommunicationLogs cl
INNER JOIN SmsGatewaySettings sgs ON cl.ProviderId = sgs.Id
WHERE cl.Channel = 1 -- SMS
  AND cl.Status = 2 -- Sent
  AND cl.CreatedAt >= DATEADD(month, -1, GETDATE())
GROUP BY Provider
```

---

## Part 10: Quick Start Template Files

I've created reference implementations in the email settings component. Use those as templates:

**Reference File**: `complaint-system-angular/src/app/components/admin/email-settings/email-settings-management.component.ts`

**Steps**:
1. Copy email-settings folder
2. Rename to sms-gateway-management
3. Replace "Email" with "SMS Gateway" throughout
4. Update fields to match SmsGatewaySettings schema
5. Repeat for WhatsApp

---

## Conclusion

This guide provides everything needed to implement SMS and WhatsApp configuration pages. The database tables are ready, you just need to:

1. Create the backend API controllers
2. Create the frontend Angular components
3. Add navigation and routing
4. Test with actual SMS/WhatsApp providers

**Estimated Implementation Time**: 10-15 hours for a single developer

**Complexity**: Medium - similar to email settings page you already have

**Priority**: Medium - system works without these, but they enable SMS/WhatsApp notifications

---

**Created**: October 22, 2025
**Last Updated**: October 22, 2025
**Status**: Documentation complete, ready for implementation
