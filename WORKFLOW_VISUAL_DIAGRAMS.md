# Workflow Management - Visual Diagrams

This document contains visual diagrams to help understand the workflow management system.

---

## Diagram 1: Complete System Architecture

```mermaid
graph TB
    Company[Company<br/>TechCorp Inc] --> Category1[Category<br/>IT Support]
    Company --> Category2[Category<br/>HR Issues]
    Company --> Category3[Category<br/>Facilities]

    Category1 --> Workflow1[Workflow<br/>IT Support Workflow<br/>IsActive: true]
    Category2 --> Workflow2[Workflow<br/>HR Workflow<br/>IsActive: true]
    Category3 --> Workflow3[Workflow<br/>Facilities Workflow<br/>IsActive: true]

    Workflow1 --> Status1_1[Status: SUBMITTED<br/>SLA: 24h]
    Workflow1 --> Status1_2[Status: IN PROGRESS<br/>SLA: 48h]
    Workflow1 --> Status1_3[Status: ESCALATED<br/>SLA: 4h]
    Workflow1 --> Status1_4[Status: RESOLVED<br/>SLA: 72h]

    Status1_1 -->|Start Work| Status1_2
    Status1_2 -->|Escalate| Status1_3
    Status1_3 -->|Resolve| Status1_4

    Workflow1 --> Complaint1[Complaint: CMP-1234<br/>Current: IN PROGRESS<br/>Time: 18h/48h]

    style Company fill:#e1f5ff
    style Category1 fill:#fff4e6
    style Workflow1 fill:#e8f5e9
    style Status1_2 fill:#ffeb3b
    style Complaint1 fill:#ffcdd2
```

---

## Diagram 2: Category-Workflow Association

```mermaid
graph LR
    A[Create Workflow] --> B{Select Category}
    B -->|IT Support| C[Workflow for<br/>IT Support]
    B -->|HR Issues| D[Workflow for<br/>HR Issues]
    B -->|Facilities| E[Workflow for<br/>Facilities]

    C --> F[All IT Support<br/>complaints use<br/>this workflow]
    D --> G[All HR<br/>complaints use<br/>this workflow]
    E --> H[All Facilities<br/>complaints use<br/>this workflow]

    style A fill:#4CAF50,color:#fff
    style B fill:#2196F3,color:#fff
    style C fill:#FF9800
    style D fill:#9C27B0,color:#fff
    style E fill:#F44336,color:#fff
```

---

## Diagram 3: Workflow Status Lifecycle with SLA

```mermaid
stateDiagram-v2
    [*] --> Submitted
    Submitted --> InProgress: Start Work
    InProgress --> Escalated: Escalate (requires comment)
    InProgress --> Resolved: Resolve (requires approval)
    Escalated --> Resolved: Resolve (requires approval)
    Resolved --> Closed: Close
    Closed --> [*]

    note right of Submitted
        SLA: 24 hours
        Initial Status
        Auto-assigned
    end note

    note right of InProgress
        SLA: 48 hours
        Active work
        Agent assigned
    end note

    note right of Escalated
        SLA: 4 hours ⚠️
        URGENT
        Manager notified
    end note

    note right of Resolved
        SLA: 72 hours
        Waiting confirmation
        Customer review
    end note
```

---

## Diagram 4: SLA Timeline Visualization

```mermaid
gantt
    title Complaint Lifecycle with SLA Tracking
    dateFormat HH:mm
    axisFormat %H:%M

    section Submitted Status
    SLA: 24h allowed    :active, s1, 00:00, 24h
    Actual: 2h spent    :crit, s2, 00:00, 2h

    section In Progress Status
    SLA: 48h allowed    :active, p1, 02:00, 48h
    Actual: 18h spent   :crit, p2, 02:00, 18h
    Time remaining      :p3, 20:00, 30h

    section Escalated Status
    SLA: 4h allowed     :active, e1, 50:00, 4h
    Expected completion :crit, e2, 50:00, 2h
```

---

## Diagram 5: Workflow Creation Process

```mermaid
sequenceDiagram
    participant Admin
    participant UI as Workflow UI
    participant API as Backend API
    participant DB as Database

    Admin->>UI: Click "Create Workflow"
    UI->>Admin: Show modal with form
    Admin->>UI: Select Category: "IT Support"
    Admin->>UI: Enter Name: "IT Support Workflow"
    Admin->>UI: Enter Description
    Admin->>UI: Check "Active" & "Default"
    Admin->>UI: Click "Create Workflow"

    UI->>API: POST /api/workflows
    Note over API: Validate request
    API->>DB: Insert CategoryWorkflow record
    DB->>API: Return workflow ID
    API->>UI: Success response
    UI->>Admin: Show success message
    Admin->>UI: Click workflow to view
    UI->>Admin: Show workflow details

    Note over Admin,DB: Workflow is now LINKED to "IT Support" category
```

---

## Diagram 6: SLA Monitoring and Escalation

```mermaid
flowchart TD
    Start[Complaint Created] --> CheckStatus{Check Current<br/>Status}
    CheckStatus --> GetSLA[Get SLA for<br/>Current Status]
    GetSLA --> Calculate[Calculate<br/>Time in Status]
    Calculate --> Compare{Time in Status<br/>vs SLA Limit}

    Compare -->|Time < Escalation| Green[Status: On Track ✅<br/>No action needed]
    Compare -->|Time >= Escalation<br/>& < SLA| Yellow[Status: Warning ⚠️<br/>Notify Manager]
    Compare -->|Time >= SLA| Red[Status: BREACH ❌<br/>Escalate + Alert]

    Green --> Wait[Wait 5 minutes]
    Yellow --> Wait
    Red --> AutoEscalate[Auto-escalate to<br/>next level]

    Wait --> CheckStatus
    AutoEscalate --> LogBreach[Log SLA breach<br/>to database]
    LogBreach --> SendAlerts[Send email/SMS<br/>to stakeholders]
    SendAlerts --> CheckStatus

    style Green fill:#4CAF50,color:#fff
    style Yellow fill:#FF9800,color:#fff
    style Red fill:#F44336,color:#fff
```

---

## Diagram 7: Add Status to Workflow Process

```mermaid
sequenceDiagram
    participant Admin
    participant UI as Workflow UI
    participant API as Backend API
    participant DB as Database

    Admin->>UI: Select workflow
    UI->>Admin: Show workflow details
    Admin->>UI: Click "Add Status"
    UI->>API: GET /api/status-master
    API->>DB: Fetch available statuses
    DB->>API: Return status list
    API->>UI: Return statuses
    UI->>Admin: Show modal with status dropdown

    Admin->>UI: Select Status: "In Progress"
    Admin->>UI: Set Display Order: 2
    Admin->>UI: Set Default SLA: 48 hours ⏰
    Admin->>UI: Set Escalation: 8 hours
    Admin->>UI: Click "Add Status"

    UI->>API: POST /api/workflows/{id}/statuses
    Note over API: Validate:<br/>- Status not already added<br/>- SLA > 0<br/>- Display order unique
    API->>DB: Insert WorkflowStatus record
    DB->>API: Confirm insertion
    API->>UI: Success response
    UI->>Admin: Refresh workflow details
    Admin->>Admin: See new status in list ✅
```

---

## Diagram 8: Add Transition to Workflow Process

```mermaid
flowchart LR
    A[Select Workflow] --> B[Click Add Transition]
    B --> C[Select FROM Status:<br/>Submitted]
    C --> D[Select TO Status:<br/>In Progress]
    D --> E[Enter Name:<br/>Start Work]
    E --> F{Requires<br/>Comment?}
    F -->|Yes| G[Check Requires Comment]
    F -->|No| H[Leave unchecked]
    G --> I{Requires<br/>Approval?}
    H --> I
    I -->|Yes| J[Check Requires Approval]
    I -->|No| K[Leave unchecked]
    J --> L[Select Allowed Roles]
    K --> L
    L --> M[Click Add Transition]
    M --> N[Transition Created ✅]

    style A fill:#2196F3,color:#fff
    style N fill:#4CAF50,color:#fff
    style F fill:#FF9800
    style I fill:#FF9800
```

---

## Diagram 9: Complaint Status Transition Flow

```mermaid
graph TB
    User[User Creates<br/>Complaint] --> SelectCat[Select Category:<br/>IT Support]
    SelectCat --> FindWorkflow[System finds workflow<br/>for IT Support]
    FindWorkflow --> InitStatus[Auto-assign Initial Status:<br/>SUBMITTED]
    InitStatus --> StartSLA[Start SLA Timer:<br/>24 hours ⏰]
    StartSLA --> DisplayComplaint[Display complaint<br/>to user]

    DisplayComplaint --> Agent[Agent views complaint]
    Agent --> CheckTrans{Check Available<br/>Transitions}
    CheckTrans --> Trans1[Start Work]
    CheckTrans --> Trans2[Reject]

    Agent --> ClickTrans[Click Start Work]
    ClickTrans --> ValidateTrans{Validate Transition<br/>Allowed?}
    ValidateTrans -->|Yes| ChangeStat[Change Status to:<br/>IN PROGRESS]
    ValidateTrans -->|No| Error[Show error message]

    ChangeStat --> StopOldSLA[Stop old SLA timer]
    StopOldSLA --> StartNewSLA[Start new SLA timer:<br/>48 hours ⏰]
    StartNewSLA --> LogHistory[Log status change<br/>to history]
    LogHistory --> Notify[Notify stakeholders]
    Notify --> RefreshUI[Refresh UI]

    style User fill:#e1f5ff
    style InitStatus fill:#4CAF50,color:#fff
    style ChangeStat fill:#4CAF50,color:#fff
    style Error fill:#F44336,color:#fff
```

---

## Diagram 10: Why Workflow Deletion is Not Supported

```mermaid
graph TB
    Delete[Request to Delete<br/>Workflow] --> CheckUsage{Is workflow<br/>used by complaints?}

    CheckUsage -->|Yes| Problem1[Problem: Orphaned<br/>Complaints ❌]
    CheckUsage -->|No| Problem2[Problem: Lost<br/>Audit History ❌]

    Problem1 --> Issue1[Complaints lose<br/>workflow reference]
    Problem1 --> Issue2[Status transitions<br/>become invalid]
    Problem1 --> Issue3[SLA tracking<br/>breaks]

    Problem2 --> Issue4[Historical workflow<br/>data lost]
    Problem2 --> Issue5[Cannot reproduce<br/>past states]
    Problem2 --> Issue6[Compliance<br/>violations]

    Issue1 --> Solution[Solution: Use<br/>IsActive Flag ✅]
    Issue2 --> Solution
    Issue3 --> Solution
    Issue4 --> Solution
    Issue5 --> Solution
    Issue6 --> Solution

    Solution --> Soft1[Set isActive = false]
    Solution --> Soft2[Keep in database]
    Solution --> Soft3[Hide from UI]
    Solution --> Soft4[Preserve history]

    Soft1 --> Result[Workflow Archived ✅<br/>Data Integrity Maintained ✅<br/>Audit Trail Preserved ✅]
    Soft2 --> Result
    Soft3 --> Result
    Soft4 --> Result

    style Delete fill:#2196F3,color:#fff
    style Problem1 fill:#F44336,color:#fff
    style Problem2 fill:#F44336,color:#fff
    style Solution fill:#4CAF50,color:#fff
    style Result fill:#4CAF50,color:#fff
```

---

## Diagram 11: SLA Breach Scenarios

```mermaid
timeline
    title Complaint CMP-1234: Timeline with SLA Tracking

    section Hour 0
        Complaint Created : Status: SUBMITTED
                          : SLA: 24 hours
                          : Timer starts ⏰

    section Hour 2
        Agent Responds : Status: IN PROGRESS
                      : Old SLA: Met ✅ (2h < 24h)
                      : New SLA: 48 hours
                      : Timer restarts ⏰

    section Hour 20
        Warning Alert : Time in status: 18h
                     : Escalation threshold: 40h
                     : Status: On Track ✅

    section Hour 40
        Escalation Alert : Time in status: 38h
                        : Escalation threshold reached ⚠️
                        : Manager notified

    section Hour 50
        SLA BREACH : Time in status: 48h
                   : SLA limit reached ❌
                   : Auto-escalate triggered
                   : Status: ESCALATED
                   : New SLA: 4 hours

    section Hour 52
        Resolution : Status: RESOLVED
                   : Escalated SLA: Met ✅ (2h < 4h)
                   : Total time: 52 hours
                   : Outcome: 1 SLA breach (In Progress)
```

---

## Diagram 12: User Interface Workflow

```mermaid
graph TB
    subgraph "Workflow Management Page"
        A[Workflows List] --> B[Select Workflow]
        B --> C[Workflow Details Panel]

        C --> D[Workflow Info Card]
        C --> E[Workflow Statuses Card]
        C --> F[Workflow Transitions Card]

        D --> D1[Name]
        D --> D2[Category 🔗]
        D --> D3[Status Active/Inactive]
        D --> D4[Default Yes/No]

        E --> E1[Add Status Button]
        E --> E2[Status Table]
        E2 --> E2a[Order]
        E2 --> E2b[Status Name]
        E2 --> E2c[SLA Hours ⏰]
        E2 --> E2d[Initial Status]
        E2 --> E2e[Requires Approval]

        F --> F1[Add Transition Button]
        F --> F2[Transition Table]
        F2 --> F2a[From Status]
        F2 --> F2b[Arrow →]
        F2 --> F2c[To Status]
        F2 --> F2d[Transition Name]
        F2 --> F2e[Comment Required]
        F2 --> F2f[Approval Required]
    end

    style D2 fill:#FF9800
    style E2c fill:#4CAF50,color:#fff
```

---

## Diagram 13: Real-World Example - IT Support Workflow

```mermaid
stateDiagram-v2
    [*] --> Submitted: New ticket created

    Submitted --> Assigned: Auto-assign to team
    Assigned --> InProgress: Agent starts work

    InProgress --> PendingInfo: Need customer info
    InProgress --> Escalated: Complex issue
    InProgress --> Resolved: Issue fixed

    PendingInfo --> InProgress: Info received
    PendingInfo --> Cancelled: No response (7 days)

    Escalated --> Resolved: Senior resolved

    Resolved --> Closed: Customer confirmed
    Resolved --> InProgress: Customer not satisfied

    Closed --> [*]
    Cancelled --> [*]

    note right of Submitted
        SLA: 2 hours
        Must acknowledge
    end note

    note right of Assigned
        SLA: 4 hours
        Must assign agent
    end note

    note right of InProgress
        SLA: 24 hours
        Active work required
    end note

    note right of Escalated
        SLA: 2 hours ⚠️
        High priority
    end note

    note right of PendingInfo
        SLA: 48 hours
        Waiting for customer
    end note

    note right of Resolved
        SLA: 72 hours
        Waiting confirmation
    end note
```

---

## Diagram 14: Database Schema Visualization

```mermaid
erDiagram
    COMPANY ||--o{ CATEGORY : has
    COMPANY ||--o{ WORKFLOW : owns
    CATEGORY ||--o{ WORKFLOW : "configured with"
    WORKFLOW ||--o{ WORKFLOW_STATUS : contains
    WORKFLOW ||--o{ WORKFLOW_TRANSITION : defines
    STATUS_MASTER ||--o{ WORKFLOW_STATUS : "used in"
    WORKFLOW ||--o{ COMPLAINT : applies_to

    COMPANY {
        guid Id PK
        string Name
        bool IsActive
    }

    CATEGORY {
        guid Id PK
        string Name
        string Code
        guid CompanyId FK
        int DefaultPriority
        int DefaultSlaHours
    }

    WORKFLOW {
        guid Id PK
        guid CategoryId FK "ONE CATEGORY"
        string Name
        string Description
        bool IsActive
        bool IsDefault
        guid CompanyId FK
    }

    WORKFLOW_STATUS {
        guid Id PK
        guid WorkflowId FK
        guid StatusMasterId FK
        int DisplayOrder
        bool IsInitialStatus
        int DefaultSLAHours "SLA HERE"
        int EscalationHours
        bool RequiresApproval
    }

    WORKFLOW_TRANSITION {
        guid Id PK
        guid WorkflowId FK
        guid FromStatusId FK
        guid ToStatusId FK
        string TransitionName
        bool RequiresComment
        bool RequiresApproval
        string AllowedRoles
    }

    STATUS_MASTER {
        guid Id PK
        string Name
        string Code
        string ColorCode
        string IconClass
        int DisplayOrder
    }

    COMPLAINT {
        guid Id PK
        guid CategoryId FK
        guid WorkflowId FK "AUTO-ASSIGNED"
        guid CurrentStatusId FK
        datetime StatusChangedAt
        datetime CreatedAt
    }
```

---

## Diagram 15: Complete Workflow Lifecycle

```mermaid
journey
    title Admin: Setting Up a New Workflow
    section Create Workflow
        Login to admin panel: 5: Admin
        Navigate to Workflow Management: 4: Admin
        Click Create Workflow: 5: Admin
        Select category IT Support: 5: Admin
        Enter workflow details: 4: Admin
        Save workflow: 5: Admin
    section Add Statuses
        Click Add Status: 5: Admin
        Add Submitted (24h SLA): 5: Admin
        Add In Progress (48h SLA): 5: Admin
        Add Escalated (4h SLA): 4: Admin
        Add Resolved (72h SLA): 5: Admin
        Review status list: 5: Admin
    section Add Transitions
        Click Add Transition: 4: Admin
        Create Start Work transition: 5: Admin
        Create Escalate transition: 4: Admin
        Create Resolve transition: 5: Admin
        Set required comments: 4: Admin
        Set required approvals: 3: Admin
    section Test Workflow
        Create test complaint: 5: Admin
        Verify initial status: 5: Admin
        Test status transitions: 4: Admin
        Check SLA timers: 5: Admin
        Verify workflow works: 5: Admin
    section Deploy
        Activate workflow: 5: Admin
        Set as default: 5: Admin
        Train team members: 3: Admin
        Monitor usage: 4: Admin
        Collect feedback: 4: Admin
```

---

## How to Use These Diagrams

### Viewing Diagrams:
1. **GitHub/GitLab**: These Mermaid diagrams render automatically
2. **VS Code**: Install "Markdown Preview Mermaid Support" extension
3. **Online**: Use https://mermaid.live to view and edit
4. **Export**: Convert to PNG/SVG for presentations

### Diagram Legend:

- **Blue boxes**: Entry points or actions
- **Green boxes**: Success states or positive outcomes
- **Orange boxes**: Warning states or decisions
- **Red boxes**: Error states or problems
- **Yellow boxes**: Current/active states
- **Arrows**: Flow direction or relationships

### Quick Reference:

- **Diagram 1**: Overall system architecture
- **Diagram 2**: Category-workflow association (answers Question 2)
- **Diagram 3**: Status lifecycle with SLA (answers Question 3)
- **Diagram 4**: SLA timeline visualization
- **Diagram 5-8**: Process flows for creating workflows
- **Diagram 9**: Complaint transition flow
- **Diagram 10**: Why deletion is not supported (answers Question 1)
- **Diagram 11**: SLA breach scenarios
- **Diagram 12**: UI structure
- **Diagram 13**: Real-world example
- **Diagram 14**: Database schema
- **Diagram 15**: Complete admin journey

---

**Document Created:** November 3, 2025
**Diagrams:** 15 comprehensive visualizations
**Purpose:** Visual guide for workflow management concepts

---

*End of Workflow Visual Diagrams*
