# SLA Visibility System - Technical Specification
**Version:** 1.0
**Date:** November 2, 2025
**Author:** Claude Code Architecture Team

---

## 📑 Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Architecture](#system-architecture)
3. [API Endpoints](#api-endpoints)
4. [Data Models](#data-models)
5. [Frontend Components](#frontend-components)
6. [Data Flow](#data-flow)
7. [Implementation Phases](#implementation-phases)
8. [Database Schema](#database-schema)
9. [Business Logic](#business-logic)
10. [Security Considerations](#security-considerations)

---

## 1. Executive Summary

### Purpose
Provide comprehensive SLA visibility across all user roles (Admin, User, Handler) to ensure transparency, accountability, and proactive complaint management.

### Scope
- **Phase 1:** Basic SLA visibility (badges, timers, progress bars)
- **Phase 2:** Progress tracking and notifications
- **Phase 3:** Proactive features and automation
- **Phase 4:** Advanced analytics and reporting

### Key Stakeholders
- **Admins:** Configure SLA policies, monitor compliance
- **Users:** Understand service expectations
- **Handlers:** Prioritize work, avoid SLA breaches

---

## 2. System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     FRONTEND (Angular 18)                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Admin Views  │  │  User Views  │  │Handler Views │     │
│  ├──────────────┤  ├──────────────┤  ├──────────────┤     │
│  │ • SLA Matrix │  │ • Expectation│  │ • Urgency    │     │
│  │ • Calculator │  │   Display    │  │   Dashboard  │     │
│  │ • Coverage   │  │ • Progress   │  │ • Countdown  │     │
│  │   Report     │  │   Tracker    │  │   Timers     │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│           │                │                 │               │
│           └────────────────┴─────────────────┘               │
│                            │                                  │
│                   ┌────────▼────────┐                        │
│                   │  SLA Service    │                        │
│                   │  (TypeScript)   │                        │
│                   └────────┬────────┘                        │
└─────────────────────────────┼───────────────────────────────┘
                              │
                         HTTP/REST
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                     BACKEND (.NET Core)                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              SLA Controller                          │  │
│  │  • GetApplicableSLA(categoryId, priorityId)         │  │
│  │  • CalculateSLAStatus(complaintId)                   │  │
│  │  • GetSLATimeline(complaintId)                       │  │
│  │  • GetSLABreachWarnings()                            │  │
│  └──────────────────────────────────────────────────────┘  │
│           │                                                  │
│  ┌────────▼──────────────────────────────────────────────┐ │
│  │         SLA Calculator Service                        │ │
│  │  • Calculate response/resolution times                │ │
│  │  • Apply working hours logic                          │ │
│  │  • Calculate time remaining                           │ │
│  │  • Determine urgency level                            │ │
│  └──────────────────────────────────────────────────────┘  │
│           │                                                  │
│  ┌────────▼──────────────────────────────────────────────┐ │
│  │              Repositories                             │ │
│  │  • SLA Levels                                         │ │
│  │  • Category-SLA Mappings                              │ │
│  │  • Priority-SLA Mappings                              │ │
│  │  • SLA Events (tracking)                              │ │
│  └──────────────────────────────────────────────────────┘  │
│           │                                                  │
└───────────┼──────────────────────────────────────────────────┘
            │
┌───────────▼──────────────────────────────────────────────────┐
│                    DATABASE (SQL Server)                      │
├──────────────────────────────────────────────────────────────┤
│  • SLALevels                                                 │
│  • CategorySLAMappings                                       │
│  • PrioritySLAMappings                                       │
│  • SLAEvents (audit trail)                                   │
│  • Complaints (with SLA fields)                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. API Endpoints

### 3.1 Existing SLA Endpoints
Already implemented in your system:

```typescript
// Base URL: /api/sla

GET    /settings                          // Get global SLA settings
PUT    /settings                          // Update SLA settings

GET    /levels                            // Get all SLA levels
GET    /levels/{id}                       // Get specific SLA level
POST   /levels                            // Create SLA level
PUT    /levels/{id}                       // Update SLA level
DELETE /levels/{id}                       // Delete SLA level

GET    /category-mappings                 // Get category-SLA mappings
POST   /category-mappings                 // Create category mapping
POST   /category-mappings/bulk            // Bulk update mappings
DELETE /category-mappings/{id}            // Delete mapping

GET    /priority-mappings                 // Get priority-SLA mappings
POST   /priority-mappings                 // Create priority mapping
POST   /priority-mappings/bulk            // Bulk update mappings
DELETE /priority-mappings/{id}            // Delete mapping
```

### 3.2 NEW Endpoints Needed for Visibility

```typescript
// SLA Applicability & Calculation
GET    /api/sla/applicable?categoryId={guid}&priorityId={guid}
// Returns: Applicable SLA level with response/resolution times
Response: {
  slaLevelId: string,
  slaLevelName: string,
  colorCode: string,
  responseTimeMinutes: number,
  resolutionTimeMinutes: number,
  responseTimeDisplay: string,    // "4 hours"
  resolutionTimeDisplay: string,  // "24 hours"
  source: "category" | "priority" | "default"
}

// SLA Status for Specific Complaint
GET    /api/sla/status/{complaintId}
// Returns: Current SLA status with timing details
Response: {
  complaintId: string,
  slaLevel: SLALevel,
  responseStatus: {
    targetMinutes: number,
    elapsedMinutes: number,
    remainingMinutes: number,
    percentComplete: number,
    status: "met" | "pending" | "breached",
    dueDate: DateTime,
    metDate?: DateTime
  },
  resolutionStatus: {
    targetMinutes: number,
    elapsedMinutes: number,
    remainingMinutes: number,
    percentComplete: number,
    status: "on-track" | "warning" | "urgent" | "breached",
    dueDate: DateTime,
    resolvedDate?: DateTime
  },
  urgencyLevel: "green" | "yellow" | "orange" | "red",
  isPaused: boolean,
  pausedAt?: DateTime,
  pauseReason?: string
}

// SLA Timeline/Events
GET    /api/sla/timeline/{complaintId}
// Returns: Chronological SLA events
Response: {
  events: [
    {
      timestamp: DateTime,
      eventType: "submitted" | "response_given" | "paused" | "resumed" | "resolved" | "breached",
      description: string,
      actor?: string,
      slaSnapshot: { /* SLA status at that time */ }
    }
  ]
}

// Bulk SLA Status (for list views)
POST   /api/sla/status/bulk
Body: { complaintIds: string[] }
// Returns: Map of complaintId -> SLA status
Response: {
  [complaintId]: { urgencyLevel, remainingMinutes, percentComplete, status }
}

// SLA Coverage Matrix (Admin)
GET    /api/sla/coverage-matrix
// Returns: All category-priority combinations with SLA info
Response: {
  categories: Category[],
  priorities: Priority[],
  mappings: {
    [categoryId_priorityId]: {
      slaLevelId?: string,
      slaLevelName?: string,
      responseTime?: string,
      resolutionTime?: string,
      isMapped: boolean
    }
  },
  unmappedCount: number
}

// SLA Breach Warnings
GET    /api/sla/warnings?userId={guid}&onlyMyTickets={bool}
// Returns: Complaints approaching SLA breach
Response: {
  warnings: [
    {
      complaintId: string,
      complaintNumber: string,
      title: string,
      assignedTo: string,
      urgencyLevel: string,
      remainingMinutes: number,
      percentComplete: number,
      estimatedBreachTime: DateTime
    }
  ]
}
```

---

## 4. Data Models

### 4.1 Frontend TypeScript Models

```typescript
// SLA Status for UI Display
export interface SLAStatusDisplay {
  complaintId: string;
  complaintNumber: string;
  slaLevel: {
    id: string;
    name: string;
    colorCode: string;
  };

  response: {
    targetHours: number;
    elapsedHours: number;
    remainingHours: number;
    percentComplete: number;
    status: 'met' | 'pending' | 'breached';
    dueDate: Date;
    metDate?: Date;
  };

  resolution: {
    targetHours: number;
    elapsedHours: number;
    remainingHours: number;
    percentComplete: number;
    status: 'on-track' | 'warning' | 'urgent' | 'breached';
    dueDate: Date;
    resolvedDate?: Date;
  };

  urgencyLevel: 'green' | 'yellow' | 'orange' | 'red';
  urgencyLabel: 'On Track' | 'Warning' | 'Urgent' | 'Breached';
  isPaused: boolean;
  pauseReason?: string;
}

// SLA Timeline Event
export interface SLATimelineEvent {
  timestamp: Date;
  eventType: 'submitted' | 'response_given' | 'paused' | 'resumed' | 'resolved' | 'breached';
  description: string;
  actor?: string;
  icon: string;
  colorClass: string;
}

// SLA Coverage Matrix Cell
export interface SLACoverageCell {
  categoryId: string;
  categoryName: string;
  priorityId: string;
  priorityName: string;
  slaLevelId?: string;
  slaLevelName?: string;
  responseTime?: string;
  resolutionTime?: string;
  isMapped: boolean;
  colorCode?: string;
}
```

### 4.2 Backend C# Models

```csharp
// SLA Status DTO
public class SLAStatusDto
{
    public Guid ComplaintId { get; set; }
    public SLALevelDto SlaLevel { get; set; }
    public SLATimingDto ResponseStatus { get; set; }
    public SLATimingDto ResolutionStatus { get; set; }
    public string UrgencyLevel { get; set; }  // green, yellow, orange, red
    public bool IsPaused { get; set; }
    public DateTime? PausedAt { get; set; }
    public string? PauseReason { get; set; }
}

// SLA Timing Details
public class SLATimingDto
{
    public int TargetMinutes { get; set; }
    public int ElapsedMinutes { get; set; }
    public int RemainingMinutes { get; set; }
    public decimal PercentComplete { get; set; }
    public string Status { get; set; }  // met, pending, breached, on-track, warning, urgent
    public DateTime DueDate { get; set; }
    public DateTime? MetDate { get; set; }
}

// SLA Timeline Event
public class SLATimelineEventDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public string? Actor { get; set; }
}
```

---

## 5. Frontend Components

### 5.1 New Components to Create

#### A. SLA Badge Component
```typescript
// Path: src/app/components/shared/sla-badge/sla-badge.component.ts

@Component({
  selector: 'app-sla-badge',
  standalone: true,
  template: `
    <span class="sla-badge" [ngClass]="badgeClass" [title]="tooltip">
      <i class="bi bi-clock-history"></i>
      <span class="sla-name">{{ slaLevel }}</span>
      <span class="sla-time">{{ timeDisplay }}</span>
    </span>
  `
})
export class SLABadgeComponent {
  @Input() slaLevel: string = '';
  @Input() urgency: 'green' | 'yellow' | 'orange' | 'red' = 'green';
  @Input() remainingTime: string = '';
  @Input() showTime: boolean = true;

  get badgeClass(): string {
    return `sla-${this.urgency}`;
  }

  get timeDisplay(): string {
    return this.showTime ? this.remainingTime : '';
  }

  get tooltip(): string {
    return `${this.slaLevel} SLA - ${this.remainingTime} remaining`;
  }
}
```

#### B. SLA Progress Bar Component
```typescript
// Path: src/app/components/shared/sla-progress-bar/sla-progress-bar.component.ts

@Component({
  selector: 'app-sla-progress-bar',
  standalone: true,
  template: `
    <div class="sla-progress-container">
      <div class="sla-progress-header">
        <span class="sla-label">{{ label }}</span>
        <span class="sla-percentage" [ngClass]="urgencyClass">{{ percentComplete }}%</span>
      </div>
      <div class="progress">
        <div class="progress-bar"
             [ngClass]="urgencyClass"
             [style.width.%]="percentComplete"
             role="progressbar"
             [attr.aria-valuenow]="percentComplete"
             [attr.aria-valuemin]="0"
             [attr.aria-valuemax]="100">
        </div>
      </div>
      <div class="sla-progress-footer">
        <span class="time-elapsed">{{ elapsedTime }}</span>
        <span class="time-remaining">{{ remainingTime }} left</span>
      </div>
    </div>
  `
})
export class SLAProgressBarComponent {
  @Input() label: string = 'Resolution Progress';
  @Input() percentComplete: number = 0;
  @Input() elapsedTime: string = '';
  @Input() remainingTime: string = '';
  @Input() urgency: 'green' | 'yellow' | 'orange' | 'red' = 'green';

  get urgencyClass(): string {
    return `progress-${this.urgency}`;
  }
}
```

#### C. SLA Info Panel Component
```typescript
// Path: src/app/components/shared/sla-info-panel/sla-info-panel.component.ts

@Component({
  selector: 'app-sla-info-panel',
  standalone: true,
  template: `
    <div class="sla-info-panel" *ngIf="slaStatus">
      <div class="panel-header">
        <i class="bi bi-shield-check"></i>
        <h6>Service Level Agreement</h6>
        <app-sla-badge
          [slaLevel]="slaStatus.slaLevel.name"
          [urgency]="slaStatus.urgencyLevel"
          [remainingTime]="formatTime(slaStatus.resolution.remainingMinutes)">
        </app-sla-badge>
      </div>

      <div class="panel-body">
        <!-- Response SLA -->
        <div class="sla-section" *ngIf="slaStatus.response">
          <h6>Initial Response</h6>
          <app-sla-progress-bar
            label="Response Time"
            [percentComplete]="slaStatus.response.percentComplete"
            [elapsedTime]="formatTime(slaStatus.response.elapsedMinutes)"
            [remainingTime]="formatTime(slaStatus.response.remainingMinutes)"
            [urgency]="getResponseUrgency()">
          </app-sla-progress-bar>
        </div>

        <!-- Resolution SLA -->
        <div class="sla-section">
          <h6>Resolution Time</h6>
          <app-sla-progress-bar
            label="Resolution Progress"
            [percentComplete]="slaStatus.resolution.percentComplete"
            [elapsedTime]="formatTime(slaStatus.resolution.elapsedMinutes)"
            [remainingTime]="formatTime(slaStatus.resolution.remainingMinutes)"
            [urgency]="slaStatus.urgencyLevel">
          </app-sla-progress-bar>
        </div>

        <!-- What This Means -->
        <div class="sla-explanation" *ngIf="showExplanation">
          <i class="bi bi-info-circle"></i>
          <p>{{ getExplanationText() }}</p>
        </div>
      </div>
    </div>
  `
})
export class SLAInfoPanelComponent implements OnInit {
  @Input() complaintId: string = '';
  @Input() showExplanation: boolean = true;
  @Input() viewMode: 'user' | 'handler' = 'user';

  slaStatus: SLAStatusDisplay | null = null;

  constructor(private slaService: SLAService) {}

  ngOnInit(): void {
    this.loadSLAStatus();
  }

  loadSLAStatus(): void {
    this.slaService.getSLAStatus(this.complaintId).subscribe(status => {
      this.slaStatus = status;
    });
  }

  formatTime(minutes: number): string {
    // Implementation
  }

  getResponseUrgency(): 'green' | 'yellow' | 'orange' | 'red' {
    // Implementation
  }

  getExplanationText(): string {
    // Implementation based on viewMode
  }
}
```

#### D. SLA Coverage Matrix Component (Admin)
```typescript
// Path: src/app/components/admin/sla-coverage-matrix/sla-coverage-matrix.component.ts

@Component({
  selector: 'app-sla-coverage-matrix',
  standalone: true,
  template: `
    <div class="sla-coverage-matrix">
      <div class="matrix-header">
        <h4>SLA Coverage Matrix</h4>
        <button class="btn btn-primary" (click)="exportToExcel()">
          <i class="bi bi-download"></i> Export
        </button>
      </div>

      <div class="matrix-stats">
        <div class="stat-card">
          <span class="stat-value">{{ mappedCount }}</span>
          <span class="stat-label">Mapped Combinations</span>
        </div>
        <div class="stat-card warning">
          <span class="stat-value">{{ unmappedCount }}</span>
          <span class="stat-label">Unmapped Combinations</span>
        </div>
      </div>

      <div class="matrix-table-wrapper">
        <table class="matrix-table">
          <thead>
            <tr>
              <th>Category</th>
              <th *ngFor="let priority of priorities">{{ priority.name }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let category of categories">
              <td class="category-cell">{{ category.name }}</td>
              <td *ngFor="let priority of priorities"
                  [ngClass]="getCellClass(category.id, priority.id)"
                  (click)="editMapping(category.id, priority.id)">
                <div class="mapping-cell">
                  <span class="sla-name">{{ getSLAName(category.id, priority.id) }}</span>
                  <span class="sla-times">{{ getSLATimes(category.id, priority.id) }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class SLACoverageMatrixComponent implements OnInit {
  categories: any[] = [];
  priorities: any[] = [];
  mappings: Map<string, any> = new Map();
  mappedCount: number = 0;
  unmappedCount: number = 0;

  // Implementation
}
```

### 5.2 Enhanced Existing Components

#### Complaint List Component
```typescript
// Add SLA column to table
columns: TableColumn[] = [
  { key: 'complaintNumber', label: 'Number', sortable: true },
  { key: 'title', label: 'Title', sortable: true },
  { key: 'categoryName', label: 'Category', sortable: true },
  { key: 'status', label: 'Status', sortable: true },
  // NEW COLUMN
  {
    key: 'slaStatus',
    label: 'SLA Status',
    sortable: true,
    template: 'sla-status'  // Custom template
  },
  { key: 'assignedToName', label: 'Assigned To', sortable: true },
  { key: 'submittedAt', label: 'Submitted', sortable: true }
];
```

#### Complaint Detail Component
```typescript
// Add SLA section after complaint info
<div class="row">
  <div class="col-md-8">
    <!-- Existing complaint information -->
  </div>
  <div class="col-md-4">
    <!-- NEW: SLA Info Panel -->
    <app-sla-info-panel
      [complaintId]="complaint.id"
      [viewMode]="isHandler ? 'handler' : 'user'"
      [showExplanation]="!isHandler">
    </app-sla-info-panel>
  </div>
</div>
```

---

## 6. Data Flow

### 6.1 SLA Calculation Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. COMPLAINT CREATION                                        │
├─────────────────────────────────────────────────────────────┤
│  User selects:                                               │
│  • Category: "Attendance Issues"                             │
│  • Priority: "Critical"                                      │
│                                                              │
│  ┌─────────────────────────────────────────────┐            │
│  │ Frontend: Check Applicable SLA               │            │
│  │ GET /api/sla/applicable?                     │            │
│  │     categoryId=X&priorityId=Y                │            │
│  └─────────────────┬────────────────────────────┘            │
│                    │                                          │
│                    ▼                                          │
│  ┌─────────────────────────────────────────────┐            │
│  │ Backend: SLA Calculator Service              │            │
│  │ 1. Check Category-SLA mapping                │            │
│  │ 2. Check Priority-SLA mapping                │            │
│  │ 3. Return applicable SLA level               │            │
│  └─────────────────┬────────────────────────────┘            │
│                    │                                          │
│                    ▼                                          │
│  ┌─────────────────────────────────────────────┐            │
│  │ Display: "Gold SLA - 4h Response / 24h Res" │            │
│  └─────────────────────────────────────────────┘            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 2. COMPLAINT SUBMISSION                                      │
├─────────────────────────────────────────────────────────────┤
│  Backend creates complaint record with:                      │
│  • submittedAt: NOW()                                        │
│  • responseDeadline: submittedAt + 4 hours                   │
│  • resolutionDeadline: submittedAt + 24 hours                │
│  • slaLevelId: Gold Level ID                                 │
│  • slaResponseMinutes: 240 (4h)                              │
│  • slaResolutionMinutes: 1440 (24h)                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 3. VIEWING COMPLAINT (Real-time calculation)                 │
├─────────────────────────────────────────────────────────────┤
│  Frontend requests:                                          │
│  GET /api/sla/status/{complaintId}                           │
│                                                              │
│  Backend calculates:                                         │
│  • elapsedMinutes = NOW() - submittedAt                      │
│  • remainingMinutes = target - elapsed                       │
│  • percentComplete = (elapsed / target) * 100                │
│  • urgency = based on percentComplete                        │
│  • status = "on-track" | "warning" | "urgent" | "breached"   │
│                                                              │
│  Returns live SLA status to frontend                         │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Urgency Level Determination

```
Urgency Level Logic:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

percentComplete = (elapsedMinutes / targetMinutes) * 100

IF percentComplete >= 100:
  urgency = "red" (BREACHED)
  status = "breached"

ELSE IF percentComplete >= 90:
  urgency = "red" (URGENT)
  status = "urgent"

ELSE IF percentComplete >= 75:
  urgency = "orange" (CRITICAL)
  status = "warning"

ELSE IF percentComplete >= 60:
  urgency = "yellow" (WARNING)
  status = "warning"

ELSE:
  urgency = "green" (ON TRACK)
  status = "on-track"
```

---

## 7. Implementation Phases

### Phase 1: Basic Visibility (Days 1-3)
**Goal:** Show SLA information on key pages

#### Day 1: Backend Foundation
- [ ] Create new SLA endpoints:
  - `/api/sla/applicable`
  - `/api/sla/status/{id}`
  - `/api/sla/status/bulk`
- [ ] Implement SLA calculator methods
- [ ] Add SLA fields to Complaint entity if missing
- [ ] Test API endpoints with Postman

#### Day 2: Frontend Components
- [ ] Create SLA badge component
- [ ] Create SLA progress bar component
- [ ] Create SLA info panel component
- [ ] Add SLA service methods in Angular

#### Day 3: Integration
- [ ] Add SLA badge to complaint cards (dashboard, list)
- [ ] Add SLA info panel to complaint detail page
- [ ] Add SLA column to complaint list table
- [ ] Test end-to-end

**Deliverables:**
- ✅ SLA badges visible on all complaint cards
- ✅ Time remaining displayed
- ✅ SLA info section on detail page
- ✅ Color-coded urgency levels

---

### Phase 2: Progress Tracking (Week 2)
**Goal:** Detailed tracking and countdown timers

- [ ] Implement real-time countdown timers
- [ ] Add SLA timeline component
- [ ] Create handler dashboard with SLA sorting
- [ ] Add SLA event logging
- [ ] Implement SLA pause/resume logic

**Deliverables:**
- ✅ Live countdown timers
- ✅ Detailed progress bars
- ✅ SLA event history
- ✅ Pause functionality for "Pending Info" status

---

### Phase 3: Proactive Features (Weeks 3-4)
**Goal:** Warnings and automation

- [ ] SLA breach warning notifications
- [ ] Auto-escalation on breach
- [ ] Email alerts at 75%, 90%, 100%
- [ ] SLA coverage matrix for admins
- [ ] SLA test calculator

**Deliverables:**
- ✅ Proactive breach warnings
- ✅ Automated escalation
- ✅ Admin SLA management tools
- ✅ Coverage gap identification

---

### Phase 4: Advanced Analytics (Month 2)
**Goal:** Reporting and insights

- [ ] SLA compliance dashboard
- [ ] Average resolution time by category
- [ ] Handler performance metrics
- [ ] SLA breach analysis reports
- [ ] Trend analysis and forecasting

**Deliverables:**
- ✅ Executive SLA reports
- ✅ Team performance dashboards
- ✅ Compliance tracking
- ✅ Predictive analytics

---

## 8. Database Schema

### 8.1 Existing Tables
Your system already has:
- `SLALevels`
- `CategorySLAMappings`
- `PrioritySLAMappings`
- `Complaints` (with SLA fields)

### 8.2 New Tables Needed

```sql
-- SLA Events Tracking (Audit Trail)
CREATE TABLE SLAEvents (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ComplaintId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL,  -- submitted, response_given, paused, resumed, resolved, breached
    EventTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ActorId UNIQUEIDENTIFIER NULL,  -- User who triggered the event
    Description NVARCHAR(500) NULL,
    SLASnapshotJson NVARCHAR(MAX) NULL,  -- JSON snapshot of SLA status at event time

    CONSTRAINT FK_SLAEvents_Complaints FOREIGN KEY (ComplaintId) REFERENCES Complaints(Id),
    CONSTRAINT FK_SLAEvents_Users FOREIGN KEY (ActorId) REFERENCES Users(Id),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_SLAEvents_ComplaintId ON SLAEvents(ComplaintId);
CREATE INDEX IX_SLAEvents_EventType ON SLAEvents(EventType);
CREATE INDEX IX_SLAEvents_EventTimestamp ON SLAEvents(EventTimestamp);
```

### 8.3 Complaint Table Enhancements

```sql
-- Add SLA tracking fields to Complaints table if not already present
ALTER TABLE Complaints ADD COLUMN IF NOT EXISTS:
    SLALevelId UNIQUEIDENTIFIER NULL,
    SLAResponseTargetMinutes INT NULL,
    SLAResolutionTargetMinutes INT NULL,
    ResponseDeadline DATETIME2 NULL,
    ResolutionDeadline DATETIME2 NULL,
    ResponseMetAt DATETIME2 NULL,
    SLAIsPaused BIT NOT NULL DEFAULT 0,
    SLAPausedAt DATETIME2 NULL,
    SLAPauseReason NVARCHAR(500) NULL,
    SLAPausedMinutes INT NOT NULL DEFAULT 0,  -- Total minutes paused

    CONSTRAINT FK_Complaints_SLALevel FOREIGN KEY (SLALevelId) REFERENCES SLALevels(Id);
```

---

## 9. Business Logic

### 9.1 SLA Calculation Algorithm

```csharp
public class SLACalculatorService
{
    public SLAStatusDto CalculateSLAStatus(Complaint complaint, SLASettings settings)
    {
        // 1. Get applicable SLA level
        var slaLevel = GetApplicableSLALevel(complaint.CategoryId, complaint.PriorityId);

        // 2. Calculate response SLA
        var responseStatus = CalculateTimingStatus(
            startTime: complaint.SubmittedAt,
            targetMinutes: slaLevel.ResponseTimeInMinutes,
            metTime: complaint.ResponseMetAt,
            pausedMinutes: complaint.SLAPausedMinutes,
            settings: settings
        );

        // 3. Calculate resolution SLA
        var resolutionStatus = CalculateTimingStatus(
            startTime: complaint.SubmittedAt,
            targetMinutes: slaLevel.ResolutionTimeInMinutes,
            metTime: complaint.ResolvedAt ?? complaint.ClosedAt,
            pausedMinutes: complaint.SLAPausedMinutes,
            settings: settings
        );

        // 4. Determine overall urgency
        var urgencyLevel = DetermineUrgencyLevel(resolutionStatus.PercentComplete);

        return new SLAStatusDto
        {
            ComplaintId = complaint.Id,
            SlaLevel = MapToDto(slaLevel),
            ResponseStatus = responseStatus,
            ResolutionStatus = resolutionStatus,
            UrgencyLevel = urgencyLevel,
            IsPaused = complaint.SLAIsPaused,
            PausedAt = complaint.SLAPausedAt,
            PauseReason = complaint.SLAPauseReason
        };
    }

    private SLATimingDto CalculateTimingStatus(
        DateTime startTime,
        int targetMinutes,
        DateTime? metTime,
        int pausedMinutes,
        SLASettings settings)
    {
        var now = DateTime.UtcNow;
        var adjustedNow = metTime ?? now;

        // Calculate elapsed time (excluding paused time)
        var elapsedMinutes = (int)(adjustedNow - startTime).TotalMinutes - pausedMinutes;

        // Apply working hours logic if enabled
        if (settings.WorkingHoursOnly)
        {
            elapsedMinutes = CalculateWorkingMinutes(startTime, adjustedNow, settings);
        }

        var remainingMinutes = targetMinutes - elapsedMinutes;
        var percentComplete = (decimal)elapsedMinutes / targetMinutes * 100;

        var status = DetermineStatus(percentComplete, metTime.HasValue);
        var dueDate = CalculateDueDate(startTime, targetMinutes, settings);

        return new SLATimingDto
        {
            TargetMinutes = targetMinutes,
            ElapsedMinutes = Math.Max(0, elapsedMinutes),
            RemainingMinutes = Math.Max(0, remainingMinutes),
            PercentComplete = Math.Min(100, Math.Max(0, percentComplete)),
            Status = status,
            DueDate = dueDate,
            MetDate = metTime
        };
    }

    private string DetermineStatus(decimal percentComplete, bool isMet)
    {
        if (isMet) return "met";
        if (percentComplete >= 100) return "breached";
        if (percentComplete >= 90) return "urgent";
        if (percentComplete >= 75) return "warning";
        return "on-track";
    }

    private string DetermineUrgencyLevel(decimal percentComplete)
    {
        if (percentComplete >= 100) return "red";
        if (percentComplete >= 90) return "red";
        if (percentComplete >= 75) return "orange";
        if (percentComplete >= 60) return "yellow";
        return "green";
    }
}
```

### 9.2 Working Hours Calculation

```csharp
private int CalculateWorkingMinutes(DateTime start, DateTime end, SLASettings settings)
{
    int workingMinutes = 0;
    var current = start;

    var workingDays = ParseWorkingDays(settings.WorkingDays);  // e.g., "1,2,3,4,5" for Mon-Fri
    var startHour = TimeSpan.Parse(settings.WorkingHoursStart);  // e.g., "09:00"
    var endHour = TimeSpan.Parse(settings.WorkingHoursEnd);      // e.g., "17:00"

    while (current < end)
    {
        var currentDayOfWeek = (int)current.DayOfWeek;

        // Skip if not a working day
        if (!workingDays.Contains(currentDayOfWeek))
        {
            current = current.Date.AddDays(1).Add(startHour);
            continue;
        }

        // Check if within working hours
        var currentTime = current.TimeOfDay;
        if (currentTime >= startHour && currentTime < endHour)
        {
            // Count this minute
            workingMinutes++;
            current = current.AddMinutes(1);
        }
        else if (currentTime < startHour)
        {
            // Jump to start of working hours
            current = current.Date.Add(startHour);
        }
        else
        {
            // Jump to next day's working hours
            current = current.Date.AddDays(1).Add(startHour);
        }
    }

    return workingMinutes;
}
```

---

## 10. Security Considerations

### 10.1 Authorization Rules

```typescript
// Role-based SLA visibility

Admin:
  ✅ View all SLA configurations
  ✅ Edit SLA levels, mappings
  ✅ View SLA coverage matrix
  ✅ Access all complaint SLA data
  ✅ View SLA analytics and reports

Handler:
  ✅ View SLA for assigned complaints
  ✅ View SLA warnings for their tickets
  ✅ View SLA timeline
  ❌ Cannot edit SLA configurations
  ✅ Can view department-wide SLA metrics

User (Complainant):
  ✅ View SLA for their own complaints
  ✅ View expected resolution time
  ✅ View progress toward resolution
  ❌ Cannot see handler-specific SLA data
  ❌ Cannot see breach warnings
  ❌ Cannot access SLA configurations
```

### 10.2 API Security

```csharp
// Enforce authorization on new endpoints

[Authorize]
[HttpGet("applicable")]
public async Task<IActionResult> GetApplicableSLA([FromQuery] Guid categoryId, [FromQuery] Guid priorityId)
{
    // Any authenticated user can check applicable SLA
    var sla = await _slaService.GetApplicableSLA(categoryId, priorityId);
    return Ok(sla);
}

[Authorize]
[HttpGet("status/{complaintId}")]
public async Task<IActionResult> GetSLAStatus(Guid complaintId)
{
    // Check if user has permission to view this complaint
    var hasAccess = await _authService.CanViewComplaint(User, complaintId);
    if (!hasAccess) return Forbid();

    var status = await _slaService.CalculateSLAStatus(complaintId);
    return Ok(status);
}

[Authorize(Roles = "Admin")]
[HttpGet("coverage-matrix")]
public async Task<IActionResult> GetCoverageMatrix()
{
    // Admin only
    var matrix = await _slaService.GetCoverageMatrix();
    return Ok(matrix);
}
```

---

## 11. Testing Strategy

### 11.1 Unit Tests

```csharp
[TestClass]
public class SLACalculatorServiceTests
{
    [TestMethod]
    public void CalculateSLAStatus_WithinTarget_ReturnsOnTrack()
    {
        // Arrange
        var complaint = CreateTestComplaint(
            submittedAt: DateTime.UtcNow.AddHours(-2),
            targetMinutes: 240  // 4 hours
        );

        // Act
        var status = _service.CalculateSLAStatus(complaint);

        // Assert
        Assert.AreEqual("on-track", status.ResolutionStatus.Status);
        Assert.AreEqual("green", status.UrgencyLevel);
        Assert.IsTrue(status.ResolutionStatus.RemainingMinutes > 0);
    }

    [TestMethod]
    public void CalculateSLAStatus_Breached_ReturnsRed()
    {
        // Arrange
        var complaint = CreateTestComplaint(
            submittedAt: DateTime.UtcNow.AddHours(-25),
            targetMinutes: 1440  // 24 hours
        );

        // Act
        var status = _service.CalculateSLAStatus(complaint);

        // Assert
        Assert.AreEqual("breached", status.ResolutionStatus.Status);
        Assert.AreEqual("red", status.UrgencyLevel);
        Assert.IsTrue(status.ResolutionStatus.PercentComplete >= 100);
    }
}
```

### 11.2 Integration Tests

```typescript
describe('SLA Service Integration Tests', () => {
  it('should fetch applicable SLA for category and priority', async () => {
    const response = await slaService.getApplicableSLA(categoryId, priorityId).toPromise();

    expect(response.isSuccess).toBe(true);
    expect(response.data.slaLevelName).toBeDefined();
    expect(response.data.responseTimeMinutes).toBeGreaterThan(0);
  });

  it('should calculate real-time SLA status', async () => {
    const status = await slaService.getSLAStatus(complaintId).toPromise();

    expect(status.urgencyLevel).toMatch(/green|yellow|orange|red/);
    expect(status.resolution.percentComplete).toBeGreaterThanOrEqual(0);
    expect(status.resolution.percentComplete).toBeLessThanOrEqual(100);
  });
});
```

### 11.3 E2E Tests with Playwright

```typescript
test('Admin can view SLA coverage matrix', async ({ page }) => {
  await page.goto('http://localhost:4200/admin/sla-coverage-matrix');

  // Verify matrix loads
  await expect(page.locator('.sla-coverage-matrix')).toBeVisible();

  // Verify categories and priorities displayed
  await expect(page.locator('table.matrix-table')).toBeVisible();

  // Verify unmapped combinations highlighted
  const unmappedCells = page.locator('td.unmapped');
  await expect(unmappedCells).toHaveCount(greaterThan(0));
});

test('User sees SLA expectations during complaint creation', async ({ page }) => {
  await page.goto('http://localhost:4200/complaints/create');

  // Select category
  await page.selectOption('[name="categoryId"]', categoryId);

  // Select priority
  await page.selectOption('[name="priorityId"]', priorityId);

  // Verify SLA info displays
  await expect(page.locator('.sla-expectation-panel')).toBeVisible();
  await expect(page.locator('.sla-response-time')).toContainText('4 hours');
  await expect(page.locator('.sla-resolution-time')).toContainText('24 hours');
});

test('Handler sees SLA urgency on dashboard', async ({ page }) => {
  await page.goto('http://localhost:4200/dashboard');

  // Verify SLA badges visible
  const slaBadges = page.locator('.sla-badge');
  await expect(slaBadges.first()).toBeVisible();

  // Verify urgency colors
  const urgentBadge = page.locator('.sla-badge.sla-red').first();
  await expect(urgentBadge).toBeVisible();
  await expect(urgentBadge).toContainText(/\d+h \d+m/);  // Time format
});
```

---

## 12. Performance Considerations

### 12.1 Caching Strategy

```typescript
// Frontend: Cache SLA levels and mappings
@Injectable({ providedIn: 'root' })
export class SLAService {
  private slaLevelsCache$ = new BehaviorSubject<SLALevel[]>([]);
  private categoryMappingsCache$ = new BehaviorSubject<Map<string, string>>(new Map());
  private priorityMappingsCache$ = new BehaviorSubject<Map<string, string>>(new Map());

  // Cache for 10 minutes
  private cacheExpiry = 10 * 60 * 1000;
  private lastCacheTime = 0;

  getSLALevels(): Observable<SLALevel[]> {
    const now = Date.now();
    if (now - this.lastCacheTime > this.cacheExpiry) {
      // Refresh cache
      return this.http.get<ApiResponse<SLALevel[]>>('/api/sla/levels').pipe(
        tap(response => {
          this.slaLevelsCache$.next(response.data);
          this.lastCacheTime = now;
        }),
        map(response => response.data)
      );
    }
    return this.slaLevelsCache$.asObservable();
  }
}
```

### 12.2 Bulk Operations

```csharp
// Backend: Bulk SLA status calculation for list views
[HttpPost("status/bulk")]
public async Task<IActionResult> GetBulkSLAStatus([FromBody] BulkSLARequest request)
{
    // Process in batches to avoid memory issues
    const int batchSize = 50;
    var results = new Dictionary<Guid, SLAStatusSummaryDto>();

    foreach (var batch in request.ComplaintIds.Chunk(batchSize))
    {
        var complaints = await _complaintRepository.GetByIdsAsync(batch);

        foreach (var complaint in complaints)
        {
            var status = await _slaService.CalculateSLAStatusSummary(complaint);
            results[complaint.Id] = status;
        }
    }

    return Ok(results);
}
```

---

## 13. Deployment Checklist

### Backend Deployment
- [ ] Run database migrations (add SLAEvents table, update Complaints table)
- [ ] Deploy updated .NET application
- [ ] Verify SLA endpoints accessible
- [ ] Test SLA calculation with existing complaints
- [ ] Set up SLA breach monitoring (background jobs)

### Frontend Deployment
- [ ] Build Angular application with new SLA components
- [ ] Deploy to hosting environment
- [ ] Clear browser cache / CDN cache
- [ ] Verify SLA badges display correctly
- [ ] Test on multiple browsers

### Configuration
- [ ] Set up initial SLA levels
- [ ] Configure category-SLA mappings
- [ ] Configure priority-SLA mappings
- [ ] Set global SLA settings (working hours, timezone)
- [ ] Configure SLA breach notifications

---

## 14. Monitoring & Alerts

### Metrics to Track
- SLA compliance rate (% of tickets meeting SLA)
- Average response time by category
- Average resolution time by category
- SLA breach count (daily, weekly, monthly)
- Complaints approaching breach (< 10% time remaining)

### Alert Triggers
- SLA breach occurs → Notify handler and manager
- Complaint reaches 90% SLA → Warning notification
- Daily SLA breach report → Email to management
- SLA coverage gap detected → Notify admin

---

## 15. Future Enhancements

### Advanced Features (Post Phase 4)
1. **AI-Powered SLA Prediction**
   - Predict likelihood of SLA breach based on complaint characteristics
   - Recommend optimal handler assignment

2. **Dynamic SLA Adjustment**
   - Auto-adjust SLA based on historical data
   - Season-specific SLA levels

3. **Multi-Tier SLA**
   - Different SLA for first response, updates, and final resolution
   - Interim milestone tracking

4. **SLA Benchmarking**
   - Compare SLA performance against industry standards
   - Competitive analysis

5. **Customer SLA Dashboard**
   - Public-facing SLA performance metrics
   - Transparency and trust building

---

## 16. Conclusion

This technical specification provides a comprehensive blueprint for implementing SLA visibility across your complaint management system. The phased approach ensures:

1. **Quick Wins:** Basic visibility in 3 days
2. **Incremental Value:** Each phase adds meaningful functionality
3. **Scalability:** Architecture supports future enhancements
4. **Maintainability:** Clean separation of concerns

### Success Metrics
- **Admin:** 100% SLA coverage (no unmapped category-priority combinations)
- **User:** 90%+ satisfaction with transparency
- **Handler:** 85%+ SLA compliance rate

### Next Steps
1. Review and approve specification
2. Set up development environment
3. Begin Phase 1 implementation
4. Iterate based on user feedback

---

**Document Control**
- Version: 1.0
- Last Updated: November 2, 2025
- Next Review: December 2, 2025
- Owner: Development Team
