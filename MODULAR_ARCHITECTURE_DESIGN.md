# Modular Architecture Design - Licensed Module System
## Enterprise Complaint Management & CRM Platform
**Date**: November 14, 2025
**Version**: 1.0.0

---

## Executive Summary

This document defines the complete modular architecture for transforming the complaint management system into a comprehensive, licensed enterprise platform. The system is organized into **7 independent modules**, each with its own license key that controls feature visibility and access.

### Key Principles:
- **Module Independence**: Each module can be activated/deactivated independently
- **License-Based Activation**: Features only accessible with valid module license
- **UI Visibility Control**: Menu items, routes, and components hidden for inactive modules
- **API Authorization**: Backend endpoints secured by module license verification
- **Progressive Enhancement**: Modules can be purchased and activated on-demand
- **Backward Compatible**: Module 1 is the fully functional existing system

---

## Cross-Module Entity Relationships

This section illustrates how entities across different modules interconnect to form a complete business process flow.

### Key Relationship Chains

#### 1. Customer → Project → System Integrator → Complaint Flow
```
Customer (Module 3)
    ↓ has multiple
CustomerProject (Module 3)
    ↓ assigned to
SystemIntegrator (Module 5)
    ↓ handles
Complaint (Module 1)
```

**Business Scenario**:
- A **Customer** (e.g., "ABC Corporation") has a **Project** (e.g., "ERP Implementation Phase 2")
- The **Project** is assigned to a **System Integrator** (e.g., "XYZ Tech Partners")
- When issues arise, **Complaints** are automatically routed to the SI handling that project
- SI performance is tracked per project

#### 2. Product → OEM → Asset → Complaint Flow
```
Product (Module 2)
    ↓ with OEM brand
OEMName (Module 2 field)
    ↓ manufactured as
Asset (Module 6)
    ↓ generates
Complaint (Module 1)
```

**Business Scenario**:
- A **Product** (e.g., "Cisco Catalyst Switch") has an **OEM** brand ("Cisco")
- The **Manufacturer** might be different (e.g., "Foxconn")
- Specific **Assets** are tracked by serial number
- **Complaints** are linked to specific assets for failure analysis
- Reports can show: "All complaints for Cisco products" or "All complaints for assets manufactured by Foxconn"

#### 3. Contract → Product → Customer → Complaint Flow
```
Customer (Module 3)
    ↓ signs
Contract (Module 4) [AMC/Warranty]
    ↓ covers
Product/Asset (Module 2/6)
    ↓ when issues occur
Complaint (Module 1)
    ↓ consumes
ServiceEntitlement (Module 4)
```

**Business Scenario**:
- **Customer** signs an **AMC Contract** covering specific **Products/Assets**
- When a **Complaint** is raised for that product, system checks contract coverage
- If covered, **ServiceEntitlement** is automatically deducted
- Contract-specific SLA is applied to the complaint
- Customer dashboard shows: "5 of 20 service calls remaining this quarter"

#### 4. Complete End-to-End Flow
```
Company (Module 1)
    ↓ has
Customer (Module 3)
    ↓ at location
CustomerSite (Module 3)
    ↓ has installed
Asset (Module 6)
    ↓ of product
Product (Module 2)
    ↓ by OEM
OEMName (Module 2)
    ↓ covered by
Contract (Module 4)
    ↓ for project
CustomerProject (Module 3)
    ↓ delivered by
SystemIntegrator (Module 5)
    ↓ when issue arises
Complaint (Module 1)
```

### Key Fields Added/Modified

**Module 2 Enhancement: Product Entity**
```csharp
public class Product
{
    // ... existing fields
    public string OEMName { get; set; }         // NEW: Brand (Dell, HP, Cisco)
    public string Manufacturer { get; set; }    // Factory (Quanta, Foxconn)
    // ... rest of fields
}
```

**Module 3 Enhancement: CustomerProject Entity**
```csharp
public class CustomerProject
{
    // ... existing fields
    public Guid? SystemIntegratorId { get; set; }    // NEW: Links to SI
    public SystemIntegrator SystemIntegrator { get; set; }  // Navigation
    // ... rest of fields
}
```

### Business Intelligence Enabled by These Relationships

1. **OEM Performance Reports**: "Show all complaints for Dell products across all customers"
2. **SI Performance by Project**: "Show XYZ Tech's performance on ABC Corp's ERP project"
3. **Customer-Project-SI Dashboard**: "Which SI is handling which projects for which customers?"
4. **Product Failure Analysis**: "Are Cisco switches manufactured by Foxconn more reliable than those by Quanta?"
5. **Contract Utilization**: "Customer ABC has consumed 15 of 20 service calls this quarter"
6. **Asset Lifecycle Tracking**: "Track this specific asset (serial #12345) from installation to complaints to service history"

---

## Module Catalog

### Module 1: Complaint Management Core ✅ IMPLEMENTED
**License Key**: `COMPLAINT_CORE`
**Status**: Fully Implemented (100%)
**Price Tier**: Base Platform (Required)

#### Description
Complete complaint lifecycle management system with advanced SLA tracking, escalation workflows, multi-channel notifications, and comprehensive audit trails.

#### Entities (95+ Total)
**Core Complaint Management:**
- Complaint
- ComplaintComment
- ComplaintAttachment
- ComplaintAssignment
- ComplaintTransition

**SLA Management:**
- SLALevel
- SLAConfiguration
- SLABreach
- SLABreachLog
- PriorityCategoryMapping

**Escalation System:**
- EscalationMatrix
- EscalationMatrixLevel
- EscalationPolicy
- ResourcePool
- ResourcePoolMember
- EscalationHistory

**Workflow Engine:**
- WorkflowDefinition
- WorkflowState
- WorkflowTransition
- WorkflowAction
- WorkflowCondition
- WorkflowAssignment

**Communication & Notifications:**
- CommunicationTemplate
- EventCommunicationRule
- CommunicationLog
- NotificationRule
- EmailConfiguration
- EmailMessage
- EmailAttachment

**Master Data:**
- ComplaintCategory
- ComplaintInfoSettings
- StatusMaster
- PriorityMaster
- EventMaster

**Organizational Structure:**
- Company
- Branch
- Department
- Section
- User
- Role
- Permission

**Email Ticketing:**
- EmailTicketingConfiguration
- EmailPollingLog
- AutoResponseRule

**OAuth Integration:**
- OAuthToken
- OAuthRefreshLog

**Advanced Features:**
- PasswordHistory
- AuditLog
- DashboardCache
- DashboardStatistics

#### Features
✅ Complete complaint CRUD operations
✅ Multi-level escalation with resource pools
✅ SLA tracking with breach warnings
✅ Dynamic workflow engine (category-specific workflows)
✅ Multi-channel notifications (Email, SMS, WhatsApp)
✅ Email-to-ticket conversion (OAuth support for Gmail/Outlook)
✅ Automatic token refresh
✅ Template system with variable replacement
✅ Role-based access control (25+ permissions)
✅ Real-time dashboard with statistics caching
✅ Comprehensive audit logging
✅ Auto-response system
✅ Password management with history
✅ OAuth 2.0 integration for email providers

#### API Endpoints (75+)
- `/api/complaints/*` - Complaint management
- `/api/categories/*` - Category management
- `/api/sla/*` - SLA configuration
- `/api/escalation/*` - Escalation management
- `/api/workflows/*` - Workflow configuration
- `/api/notifications/*` - Notification rules
- `/api/templates/*` - Template management
- `/api/email-config/*` - Email configuration
- `/api/oauth/*` - OAuth authentication
- `/api/dashboard/*` - Dashboard statistics
- `/api/users/*` - User management
- `/api/roles/*` - Role management
- `/api/audit-logs/*` - Audit trail

#### UI Components (50+)
- Dashboard
- Complaint List/Detail/Create/Edit
- SLA Configuration
- Escalation Matrix Builder
- Workflow Designer
- Template Editor
- Email Ticketing Configuration
- OAuth Wizard
- User Management
- Role Management
- Settings & Configuration

---

### Module 2: Product Catalog Management 🆕
**License Key**: `PRODUCT_CATALOG`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Standard Add-on
**Dependencies**: Module 1 (Complaint Core)

#### Description
Comprehensive product lifecycle management with hierarchical product families, variants, SKUs, pricing, and integration with complaint tracking for product-specific issues.

#### New Entities (8)
1. **ProductFamily**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - Code (string, unique)
   - Name (string)
   - Description (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

2. **ProductCategory**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - ProductFamilyId (Guid, FK → ProductFamily)
   - Code (string, unique)
   - Name (string)
   - Description (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

3. **Product**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - ProductCategoryId (Guid, FK → ProductCategory)
   - Code (string, unique)
   - Name (string)
   - Description (string)
   - OEMName (string) -- Original Equipment Manufacturer brand
   - Manufacturer (string) -- Actual manufacturing company
   - ModelNumber (string)
   - IsWarrantySupported (bool)
   - DefaultWarrantyMonths (int)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

   **Note**: OEMName is the brand (e.g., "Dell", "HP", "Cisco"), while Manufacturer is the actual factory/company that produces it (e.g., "Quanta Computer", "Foxconn"). In many cases, OEM and Manufacturer can be the same.

4. **ProductVariant**
   - Id (Guid, PK)
   - ProductId (Guid, FK → Product)
   - Code (string, unique)
   - Name (string)
   - VariantType (enum: Color, Size, Capacity, Configuration)
   - VariantValue (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

5. **ProductSKU**
   - Id (Guid, PK)
   - ProductId (Guid, FK → Product)
   - ProductVariantId (Guid?, FK → ProductVariant)
   - SKU (string, unique, indexed)
   - Barcode (string, unique)
   - HSNCode (string)
   - BasePrice (decimal)
   - CurrencyCode (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

6. **ProductSpecification**
   - Id (Guid, PK)
   - ProductId (Guid, FK → Product)
   - SpecificationKey (string)
   - SpecificationValue (string)
   - UnitOfMeasure (string)
   - DisplayOrder (int)

7. **ProductDocument**
   - Id (Guid, PK)
   - ProductId (Guid, FK → Product)
   - DocumentType (enum: Manual, Datasheet, Warranty, Certificate)
   - FileName (string)
   - FileSize (long)
   - FileUrl (string)
   - UploadedAt (DateTime)

8. **ProductComplaintMapping**
   - Id (Guid, PK)
   - ComplaintId (Guid, FK → Complaint)
   - ProductId (Guid, FK → Product)
   - ProductSKUId (Guid?, FK → ProductSKU)
   - Quantity (int)
   - IssueDescription (string)
   - CreatedAt (DateTime)

#### Features
🆕 Hierarchical product catalog (Family → Category → Product → Variant → SKU)
🆕 OEM brand tracking (separate from manufacturer)
🆕 Product specifications and technical details
🆕 Document management (manuals, datasheets, certificates)
🆕 Warranty configuration per product
🆕 SKU-level tracking with barcode support
🆕 Product-complaint association (track issues per product)
🆕 Multi-currency pricing support
🆕 HSN code for tax compliance

#### API Endpoints (New)
- `/api/product-families/*`
- `/api/product-categories/*`
- `/api/products/*`
- `/api/product-variants/*`
- `/api/product-skus/*`
- `/api/product-specifications/*`
- `/api/product-documents/*`

#### UI Components (New)
- Product Catalog Browser
- Product Family Manager
- Product Detail Page
- SKU Management
- Product Specification Editor
- Document Library
- Product-Complaint Linking

#### Integration with Module 1
- **Complaint Enhancement**: Add "Affected Products" section
- **Category Mapping**: Link ComplaintCategory to ProductCategory
- **Dashboard**: Add product-wise complaint statistics
- **Reporting**: Product failure rate analysis

---

### Module 3: Customer Relationship Management 🆕
**License Key**: `CUSTOMER_MANAGEMENT`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Standard Add-on
**Dependencies**: Module 1 (Complaint Core)

#### Description
Complete customer lifecycle management including customer profiles, contacts, projects, sites, and complaint history tracking per customer.

#### New Entities (6)
1. **Customer**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - CustomerCode (string, unique, indexed)
   - CustomerName (string)
   - CustomerType (enum: Corporate, SME, Individual, Government)
   - Industry (string)
   - PrimaryContactEmail (string)
   - PrimaryContactPhone (string)
   - TaxIdentifier (string)
   - BillingAddress (string)
   - IsActive (bool)
   - OnboardingDate (DateTime)
   - CreatedAt, UpdatedAt

2. **CustomerContact**
   - Id (Guid, PK)
   - CustomerId (Guid, FK → Customer)
   - ContactType (enum: Primary, Billing, Technical, Escalation)
   - FullName (string)
   - Email (string)
   - Phone (string)
   - AlternatePhone (string)
   - Designation (string)
   - Department (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

3. **CustomerProject**
   - Id (Guid, PK)
   - CustomerId (Guid, FK → Customer)
   - SystemIntegratorId (Guid?, FK → SystemIntegrator) -- **NEW: Links Project to SI**
   - ProjectCode (string, unique)
   - ProjectName (string)
   - ProjectType (enum: Implementation, Support, Upgrade, Migration)
   - StartDate (DateTime)
   - EndDate (DateTime?)
   - Status (enum: Active, Completed, OnHold, Cancelled)
   - ProjectValue (decimal)
   - CurrencyCode (string)
   - CreatedAt, UpdatedAt

   **Note**: This creates a three-way relationship: **Customer → Project → System Integrator**. Each project can be delivered by a specific SI partner, enabling project-level tracking of SI performance and complaint routing.

4. **CustomerSite**
   - Id (Guid, PK)
   - CustomerId (Guid, FK → Customer)
   - SiteCode (string, unique)
   - SiteName (string)
   - SiteType (enum: HeadOffice, Branch, Warehouse, DataCenter)
   - AddressLine1 (string)
   - AddressLine2 (string)
   - City (string)
   - State (string)
   - Country (string)
   - PostalCode (string)
   - Latitude (decimal?)
   - Longitude (decimal?)
   - ContactName (string)
   - ContactPhone (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

5. **CustomerComplaintMapping**
   - Id (Guid, PK)
   - ComplaintId (Guid, FK → Complaint)
   - CustomerId (Guid, FK → Customer)
   - CustomerProjectId (Guid?, FK → CustomerProject)
   - CustomerSiteId (Guid?, FK → CustomerSite)
   - CreatedAt (DateTime)

6. **CustomerDocument**
   - Id (Guid, PK)
   - CustomerId (Guid, FK → Customer)
   - DocumentType (enum: Agreement, Certificate, Compliance, Other)
   - FileName (string)
   - FileSize (long)
   - FileUrl (string)
   - ExpiryDate (DateTime?)
   - UploadedAt (DateTime)

#### Features
🆕 Complete customer profile management
🆕 Multiple contact persons per customer (primary, billing, technical, escalation)
🆕 Project tracking per customer with SI linkage (Customer → Project → SI)
🆕 Site/location management with geocoding
🆕 Customer-complaint association
🆕 Customer document repository
🆕 Customer health score based on complaint patterns
🆕 Customer communication history
🆕 Three-way relationship tracking: End Customer, Project, and System Integrator

#### API Endpoints (New)
- `/api/customers/*`
- `/api/customer-contacts/*`
- `/api/customer-projects/*`
- `/api/customer-sites/*`
- `/api/customer-documents/*`

#### UI Components (New)
- Customer Directory
- Customer Profile Page
- Contact Management
- Project Timeline
- Site Map Viewer
- Customer Health Dashboard
- Customer Complaint History

#### Integration with Module 1
- **Complaint Enhancement**:
  - Add "Customer" field to complaint form
  - Add "Project" field (filtered by selected customer)
  - Auto-populate SI from project (for Module 5 integration)
- **Dashboard**: Customer-wise complaint statistics
- **Notifications**: Customer-specific notification rules
- **Reporting**: Customer satisfaction metrics

#### Integration with Module 5 (SI Management)
- **Project-SI Link**: When a project is assigned to an SI, complaints for that project are automatically routed to the SI
- **SI Performance**: Track SI performance per project
- **Cross-Module Data Flow**: Customer → Project → SI → Complaint

---

### Module 4: Contract & Service Management 🆕
**License Key**: `CONTRACT_MANAGEMENT`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Premium Add-on
**Dependencies**: Module 1 (Complaint Core), Module 2 (Product Catalog), Module 3 (Customer Management)

#### Description
Complete contract lifecycle management supporting AMC (Annual Maintenance Contract), Warranty, and Pay-Per-Service models with entitlement tracking and automatic SLA enforcement.

#### New Entities (8)
1. **Contract**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - CustomerId (Guid, FK → Customer)
   - ContractNumber (string, unique, indexed)
   - ContractType (enum: AMC, Warranty, PayPerService, SLA, Support)
   - ContractName (string)
   - StartDate (DateTime)
   - EndDate (DateTime)
   - Status (enum: Draft, Active, Expired, Renewed, Cancelled)
   - TotalValue (decimal)
   - CurrencyCode (string)
   - BillingFrequency (enum: Monthly, Quarterly, Annually, OneTime)
   - PaymentTerms (string)
   - AutoRenewal (bool)
   - RenewalNoticeDays (int)
   - CreatedAt, UpdatedAt

2. **ContractLineItem**
   - Id (Guid, PK)
   - ContractId (Guid, FK → Contract)
   - ProductId (Guid?, FK → Product)
   - ProductSKUId (Guid?, FK → ProductSKU)
   - Description (string)
   - Quantity (int)
   - UnitPrice (decimal)
   - TotalPrice (decimal)
   - CoveredServiceTypes (JSON array)
   - IncludedServiceCalls (int?)
   - ResponseTimeSLA (int?) // minutes
   - ResolutionTimeSLA (int?) // hours

3. **ServiceEntitlement**
   - Id (Guid, PK)
   - ContractId (Guid, FK → Contract)
   - ContractLineItemId (Guid?, FK → ContractLineItem)
   - EntitlementType (enum: ServiceCall, RemoteSupport, OnSiteSupport, SparesParts, Training)
   - AllocatedQuantity (int)
   - ConsumedQuantity (int)
   - RemainingQuantity (int)
   - ResetFrequency (enum: Never, Monthly, Quarterly, Annually)
   - LastResetDate (DateTime?)
   - ExpiryDate (DateTime)

4. **ContractSLA**
   - Id (Guid, PK)
   - ContractId (Guid, FK → Contract)
   - SeverityLevel (enum: Critical, High, Medium, Low)
   - ResponseTimeMinutes (int)
   - ResolutionTimeHours (int)
   - EscalationAfterHours (int?)
   - PenaltyPercentage (decimal?)

5. **ContractDocument**
   - Id (Guid, PK)
   - ContractId (Guid, FK → Contract)
   - DocumentType (enum: Agreement, Amendment, Invoice, DeliveryNote, Compliance)
   - FileName (string)
   - FileSize (long)
   - FileUrl (string)
   - Version (int)
   - UploadedAt (DateTime)
   - UploadedBy (Guid, FK → User)

6. **ContractRenewal**
   - Id (Guid, PK)
   - OriginalContractId (Guid, FK → Contract)
   - RenewedContractId (Guid?, FK → Contract)
   - RenewalDate (DateTime)
   - RenewalType (enum: Automatic, Manual, Renegotiated)
   - OldEndDate (DateTime)
   - NewEndDate (DateTime)
   - ValueChange (decimal)
   - Notes (string)
   - CreatedAt (DateTime)

7. **ServiceCallLog**
   - Id (Guid, PK)
   - ContractId (Guid, FK → Contract)
   - ComplaintId (Guid?, FK → Complaint)
   - ServiceEntitlementId (Guid?, FK → ServiceEntitlement)
   - ServiceDate (DateTime)
   - ServiceType (enum: OnSite, Remote, Phone, Email)
   - TechnicianId (Guid?, FK → User)
   - DurationMinutes (int)
   - ServiceNotes (string)
   - CustomerSignatureUrl (string?)
   - IsBillable (bool)
   - CreatedAt (DateTime)

8. **ContractComplaintMapping**
   - Id (Guid, PK)
   - ComplaintId (Guid, FK → Complaint)
   - ContractId (Guid, FK → Contract)
   - ServiceEntitlementId (Guid?, FK → ServiceEntitlement)
   - IsCoveredByContract (bool)
   - ConsumesEntitlement (bool)
   - EntitlementConsumedQuantity (int)
   - CreatedAt (DateTime)

#### Features
🆕 Multi-type contract support (AMC, Warranty, Pay-Per-Service)
🆕 Service entitlement tracking (allocated vs consumed)
🆕 Automatic entitlement deduction on complaint resolution
🆕 Contract-specific SLA enforcement
🆕 Automatic renewal tracking and notifications
🆕 Service call logging with technician tracking
🆕 Contract document versioning
🆕 Entitlement reset automation (monthly/quarterly/annually)
🆕 Penalty calculation for SLA breaches
🆕 Contract value tracking and invoicing

#### API Endpoints (New)
- `/api/contracts/*`
- `/api/contract-line-items/*`
- `/api/service-entitlements/*`
- `/api/contract-slas/*`
- `/api/contract-documents/*`
- `/api/contract-renewals/*`
- `/api/service-call-logs/*`

#### UI Components (New)
- Contract List/Detail/Create/Edit
- Contract Dashboard (active/expiring/expired)
- Entitlement Tracker
- Service Call Logger
- Contract SLA Configuration
- Renewal Manager
- Contract Document Repository
- Service History Timeline

#### Integration with Module 1
- **Complaint Enhancement**:
  - Add "Contract" field (dropdown of active contracts for customer)
  - Auto-apply contract-specific SLA on complaint creation
  - Check service entitlement before complaint submission
  - Display entitlement consumption on complaint detail
- **SLA Override**: Contract SLA takes precedence over category SLA
- **Notifications**: Contract expiry warnings (30/60/90 days)
- **Dashboard**: Contract compliance metrics

---

### Module 5: System Integrator Management 🆕
**License Key**: `SI_MANAGEMENT`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Enterprise Add-on
**Dependencies**: Module 1 (Complaint Core), Module 3 (Customer Management), Module 4 (Contract Management)

#### Description
Manage relationships with System Integrators (SIs), Implementation Partners, and Service Providers who handle installations, implementations, and support on behalf of the company.

#### New Entities (5)
1. **SystemIntegrator**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - SICode (string, unique)
   - SIName (string)
   - PartnerType (enum: Platinum, Gold, Silver, Authorized)
   - SpecializationArea (string)
   - CertificationLevel (enum: Level1, Level2, Level3, Expert)
   - PrimaryEmail (string)
   - PrimaryPhone (string)
   - BillingAddress (string)
   - TaxIdentifier (string)
   - ActiveSince (DateTime)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

2. **SIContact**
   - Id (Guid, PK)
   - SystemIntegratorId (Guid, FK → SystemIntegrator)
   - ContactType (enum: Primary, Technical, Billing, Escalation)
   - FullName (string)
   - Email (string)
   - Phone (string)
   - Designation (string)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

3. **SICustomerAssignment**
   - Id (Guid, PK)
   - SystemIntegratorId (Guid, FK → SystemIntegrator)
   - CustomerId (Guid, FK → Customer)
   - CustomerProjectId (Guid?, FK → CustomerProject)
   - AssignmentType (enum: Implementation, Support, AMC, Training)
   - AssignmentStartDate (DateTime)
   - AssignmentEndDate (DateTime?)
   - IsActive (bool)
   - CreatedAt (DateTime)

4. **SIPerformanceMetric**
   - Id (Guid, PK)
   - SystemIntegratorId (Guid, FK → SystemIntegrator)
   - MetricMonth (DateTime)
   - TotalComplaintsAssigned (int)
   - TotalComplaintsResolved (int)
   - AverageResolutionTimeHours (decimal)
   - SLACompliancePercentage (decimal)
   - CustomerSatisfactionScore (decimal)
   - EscalationsReceived (int)
   - CreatedAt (DateTime)

5. **SIComplaintMapping**
   - Id (Guid, PK)
   - ComplaintId (Guid, FK → Complaint)
   - SystemIntegratorId (Guid, FK → SystemIntegrator)
   - AssignedAt (DateTime)
   - ReassignedReason (string?)
   - CreatedAt (DateTime)

#### Features
🆕 SI partner hierarchy (Platinum/Gold/Silver)
🆕 Customer-SI assignment for geographical/domain specialization
🆕 SI performance tracking (SLA compliance, resolution time, satisfaction)
🆕 Automatic complaint routing to assigned SI
🆕 SI certification and specialization management
🆕 SI contact management
🆕 SI performance dashboards and scorecards
🆕 SI escalation path configuration

#### API Endpoints (New)
- `/api/system-integrators/*`
- `/api/si-contacts/*`
- `/api/si-customer-assignments/*`
- `/api/si-performance-metrics/*`

#### UI Components (New)
- SI Directory
- SI Profile Page
- SI-Customer Assignment Manager
- SI Performance Dashboard
- SI Scorecard
- SI Contact Management

#### Integration with Module 1
- **Complaint Routing**: Auto-assign to SI based on customer-SI mapping
- **Escalation**: SI-specific escalation matrix
- **Notifications**: SI-specific notification rules
- **Dashboard**: SI performance metrics
- **Reporting**: SI performance comparison

---

### Module 6: Asset Lifecycle Management 🆕
**License Key**: `ASSET_MANAGEMENT`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Premium Add-on
**Dependencies**: Module 1 (Complaint Core), Module 2 (Product Catalog), Module 3 (Customer Management), Module 4 (Contract Management)

#### Description
Track physical assets (hardware, equipment, devices) throughout their lifecycle from installation to decommissioning, with integration to complaint tracking for asset-specific issues.

#### New Entities (6)
1. **Asset**
   - Id (Guid, PK)
   - CompanyId (Guid, FK → Company)
   - CustomerId (Guid, FK → Customer)
   - CustomerSiteId (Guid?, FK → CustomerSite)
   - ProductId (Guid, FK → Product)
   - ProductSKUId (Guid?, FK → ProductSKU)
   - AssetTag (string, unique, indexed)
   - SerialNumber (string, unique, indexed)
   - AssetName (string)
   - InstallationDate (DateTime)
   - CommissioningDate (DateTime?)
   - DecommissioningDate (DateTime?)
   - Status (enum: Ordered, InTransit, Installed, Operational, UnderMaintenance, Decommissioned)
   - LocationDescription (string)
   - PurchaseOrderNumber (string?)
   - PurchaseDate (DateTime?)
   - PurchaseCost (decimal?)
   - CurrentValue (decimal?)
   - CreatedAt, UpdatedAt

2. **AssetWarranty**
   - Id (Guid, PK)
   - AssetId (Guid, FK → Asset)
   - ContractId (Guid?, FK → Contract)
   - WarrantyType (enum: Manufacturer, Extended, AMC)
   - WarrantyProvider (string)
   - StartDate (DateTime)
   - EndDate (DateTime)
   - CoverageDescription (string)
   - IsActive (bool)
   - CreatedAt (DateTime)

3. **AssetServiceHistory**
   - Id (Guid, PK)
   - AssetId (Guid, FK → Asset)
   - ComplaintId (Guid?, FK → Complaint)
   - ServiceDate (DateTime)
   - ServiceType (enum: Installation, Repair, Maintenance, Upgrade, Inspection)
   - TechnicianId (Guid?, FK → User)
   - SystemIntegratorId (Guid?, FK → SystemIntegrator)
   - ServiceNotes (string)
   - PartsReplaced (JSON array)
   - ServiceCost (decimal?)
   - NextServiceDueDate (DateTime?)
   - CreatedAt (DateTime)

4. **AssetDocument**
   - Id (Guid, PK)
   - AssetId (Guid, FK → Asset)
   - DocumentType (enum: Manual, Warranty, InstallationReport, ServiceReport, Certificate)
   - FileName (string)
   - FileSize (long)
   - FileUrl (string)
   - UploadedAt (DateTime)

5. **AssetMetadata**
   - Id (Guid, PK)
   - AssetId (Guid, FK → Asset)
   - MetadataKey (string)
   - MetadataValue (string)
   - CreatedAt (DateTime)

6. **AssetComplaintMapping**
   - Id (Guid, PK)
   - ComplaintId (Guid, FK → Complaint)
   - AssetId (Guid, FK → Asset)
   - IssueType (enum: Hardware, Software, Configuration, Performance)
   - CreatedAt (DateTime)

#### Features
🆕 Asset registration with unique tagging
🆕 Serial number tracking
🆕 Asset lifecycle status management
🆕 Warranty tracking (manufacturer + extended)
🆕 Service history logging
🆕 Automatic complaint-asset linking
🆕 Asset location tracking (site-level)
🆕 Parts replacement tracking
🆕 Asset depreciation calculation
🆕 Preventive maintenance scheduling

#### API Endpoints (New)
- `/api/assets/*`
- `/api/asset-warranties/*`
- `/api/asset-service-history/*`
- `/api/asset-documents/*`
- `/api/asset-metadata/*`

#### UI Components (New)
- Asset Registry
- Asset Detail Page
- Asset Service History Timeline
- Warranty Tracker
- Asset Location Map
- Preventive Maintenance Scheduler
- Asset Depreciation Report

#### Integration with Module 1
- **Complaint Enhancement**:
  - Add "Affected Asset" field (searchable by asset tag/serial number)
  - Auto-populate customer, site, product from asset
  - Display asset service history in complaint detail
  - Link complaint to asset service history
- **Dashboard**: Asset-wise complaint statistics
- **Notifications**: Warranty expiry warnings
- **Reporting**: Asset failure rate analysis

---

### Module 7: Advanced Location Services 🆕
**License Key**: `LOCATION_SERVICES`
**Status**: Architecture Defined (0% coded)
**Price Tier**: Enterprise Add-on
**Dependencies**: Module 1 (Complaint Core), Module 3 (Customer Management)

#### Description
Centralized address management with geocoding, distance calculation, and integration with complaint routing based on geographical proximity.

#### New Entities (2)
1. **Address**
   - Id (Guid, PK)
   - EntityType (enum: Company, Branch, Department, Customer, CustomerSite, SystemIntegrator, Asset)
   - EntityId (Guid)
   - AddressType (enum: Physical, Billing, Shipping, Registered)
   - AddressLine1 (string)
   - AddressLine2 (string)
   - Landmark (string?)
   - City (string)
   - District (string?)
   - State (string)
   - Country (string)
   - PostalCode (string)
   - IsPrimary (bool)
   - IsActive (bool)
   - CreatedAt, UpdatedAt

2. **AddressGeocode**
   - Id (Guid, PK)
   - AddressId (Guid, FK → Address)
   - Latitude (decimal)
   - Longitude (decimal)
   - GeocodeProvider (enum: GoogleMaps, BingMaps, OpenStreetMap)
   - GeocodeAccuracy (enum: Rooftop, Approximate, Geometric, PostalCode)
   - GeocodedAt (DateTime)
   - LastVerifiedAt (DateTime?)

#### Features
🆕 Centralized address repository
🆕 Automatic geocoding on address creation
🆕 Distance calculation between addresses
🆕 Nearest technician/SI finder based on complaint location
🆕 Address validation and standardization
🆕 Map visualization of complaints, assets, customers
🆕 Geographical complaint clustering
🆕 Route optimization for technician dispatch

#### API Endpoints (New)
- `/api/addresses/*`
- `/api/addresses/geocode`
- `/api/addresses/distance-matrix`
- `/api/addresses/nearest-technicians`

#### UI Components (New)
- Address Manager
- Map View (complaints, customers, assets)
- Nearest Technician Finder
- Geographical Dashboard
- Route Optimizer

#### Integration with Module 1
- **Complaint Enhancement**:
  - Auto-populate address from customer/site
  - Display on map in complaint detail
  - Show nearest available technicians
- **Assignment**: Auto-suggest technician based on proximity
- **Dashboard**: Geographical heatmap of complaints
- **Reporting**: Region-wise complaint analysis

---

## Module Licensing System

### Database Schema

#### ModuleLicense Table
```sql
CREATE TABLE ModuleLicense (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    ModuleLicenseKey NVARCHAR(50) NOT NULL, -- e.g., 'COMPLAINT_CORE', 'PRODUCT_CATALOG'
    LicenseCode NVARCHAR(100) NOT NULL UNIQUE, -- Encrypted license key
    IsActive BIT NOT NULL DEFAULT 1,
    ActivatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL, -- NULL = perpetual license
    MaxUsers INT NULL, -- NULL = unlimited
    AllowedFeatures NVARCHAR(MAX) NULL, -- JSON array of feature flags
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_ModuleLicense_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id),
    CONSTRAINT FK_ModuleLicense_CreatedByUser FOREIGN KEY (CreatedBy) REFERENCES [User](Id),
    INDEX IX_ModuleLicense_CompanyModule (CompanyId, ModuleLicenseKey, IsActive)
);
```

#### ModuleLicenseAudit Table
```sql
CREATE TABLE ModuleLicenseAudit (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModuleLicenseId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Activated, Deactivated, Renewed, Expired
    PerformedBy UNIQUEIDENTIFIER NOT NULL,
    PerformedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Notes NVARCHAR(500),
    CONSTRAINT FK_ModuleLicenseAudit_ModuleLicense FOREIGN KEY (ModuleLicenseId) REFERENCES ModuleLicense(Id),
    CONSTRAINT FK_ModuleLicenseAudit_PerformedByUser FOREIGN KEY (PerformedBy) REFERENCES [User](Id)
);
```

#### CompanyModuleFeature Table (Optional - for granular feature control)
```sql
CREATE TABLE CompanyModuleFeature (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    ModuleLicenseKey NVARCHAR(50) NOT NULL,
    FeatureKey NVARCHAR(100) NOT NULL, -- e.g., 'ADVANCED_REPORTING', 'API_ACCESS'
    IsEnabled BIT NOT NULL DEFAULT 1,
    EnabledAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CompanyModuleFeature_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id),
    UNIQUE (CompanyId, ModuleLicenseKey, FeatureKey)
);
```

### Module License Validation Service

```csharp
public interface IModuleLicenseService
{
    Task<bool> IsModuleActiveAsync(Guid companyId, string moduleLicenseKey, CancellationToken cancellationToken = default);
    Task<bool> IsFeatureEnabledAsync(Guid companyId, string moduleLicenseKey, string featureKey, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveModulesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Result> ActivateModuleAsync(Guid companyId, string licenseCode, Guid activatedBy, CancellationToken cancellationToken = default);
    Task<Result> DeactivateModuleAsync(Guid companyId, string moduleLicenseKey, Guid deactivatedBy, string reason, CancellationToken cancellationToken = default);
    Task<ModuleLicenseInfo> GetModuleLicenseInfoAsync(Guid companyId, string moduleLicenseKey, CancellationToken cancellationToken = default);
}
```

### API Authorization Attribute

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleLicenseAttribute : Attribute, IAuthorizationFilter
{
    public string ModuleLicenseKey { get; }
    public string? FeatureKey { get; }

    public RequireModuleLicenseAttribute(string moduleLicenseKey, string? featureKey = null)
    {
        ModuleLicenseKey = moduleLicenseKey;
        FeatureKey = featureKey;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var companyId = Guid.Parse(user.FindFirst("CompanyId")?.Value ?? throw new UnauthorizedAccessException());

        var licenseService = context.HttpContext.RequestServices.GetRequiredService<IModuleLicenseService>();

        bool isAuthorized = FeatureKey == null
            ? licenseService.IsModuleActiveAsync(companyId, ModuleLicenseKey).Result
            : licenseService.IsFeatureEnabledAsync(companyId, ModuleLicenseKey, FeatureKey).Result;

        if (!isAuthorized)
        {
            context.Result = new ForbidResult();
        }
    }
}
```

### Example Controller Usage

```csharp
[ApiController]
[Route("api/products")]
[RequireModuleLicense("PRODUCT_CATALOG")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Only accessible if PRODUCT_CATALOG module is active
    }

    [HttpPost("import")]
    [RequireModuleLicense("PRODUCT_CATALOG", "BULK_IMPORT")]
    public async Task<IActionResult> BulkImport()
    {
        // Only accessible if PRODUCT_CATALOG module is active
        // AND BULK_IMPORT feature is enabled
    }
}
```

---

## UI Visibility Control

### Angular Service: ModuleLicenseGuard

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { ModuleLicenseService } from './module-license.service';

@Injectable({ providedIn: 'root' })
export class ModuleLicenseGuard implements CanActivate {
  constructor(
    private moduleLicenseService: ModuleLicenseService,
    private router: Router
  ) {}

  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
    const requiredModule = route.data['requiredModule'] as string;
    const requiredFeature = route.data['requiredFeature'] as string | undefined;

    const isActive = requiredFeature
      ? await this.moduleLicenseService.isFeatureEnabled(requiredModule, requiredFeature)
      : await this.moduleLicenseService.isModuleActive(requiredModule);

    if (!isActive) {
      this.router.navigate(['/license-required'], {
        queryParams: { module: requiredModule, feature: requiredFeature }
      });
      return false;
    }

    return true;
  }
}
```

### Route Configuration with Module Guard

```typescript
const routes: Routes = [
  // Module 1: Complaint Core (always accessible)
  { path: 'dashboard', component: DashboardComponent },
  { path: 'complaints', component: ComplaintListComponent },

  // Module 2: Product Catalog (requires license)
  {
    path: 'products',
    component: ProductListComponent,
    canActivate: [ModuleLicenseGuard],
    data: { requiredModule: 'PRODUCT_CATALOG' }
  },

  // Module 3: Customer Management (requires license)
  {
    path: 'customers',
    component: CustomerListComponent,
    canActivate: [ModuleLicenseGuard],
    data: { requiredModule: 'CUSTOMER_MANAGEMENT' }
  },

  // Module 4: Contract Management (requires license)
  {
    path: 'contracts',
    component: ContractListComponent,
    canActivate: [ModuleLicenseGuard],
    data: { requiredModule: 'CONTRACT_MANAGEMENT' }
  },

  // Feature-level guard
  {
    path: 'contracts/bulk-import',
    component: ContractBulkImportComponent,
    canActivate: [ModuleLicenseGuard],
    data: {
      requiredModule: 'CONTRACT_MANAGEMENT',
      requiredFeature: 'BULK_IMPORT'
    }
  }
];
```

### Dynamic Menu Visibility

```typescript
export interface MenuItem {
  label: string;
  icon: string;
  route: string;
  requiredModule?: string;
  requiredFeature?: string;
  children?: MenuItem[];
}

export class MenuService {
  private allMenuItems: MenuItem[] = [
    // Module 1 items (always visible)
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Complaints', icon: 'report_problem', route: '/complaints' },
    { label: 'Settings', icon: 'settings', route: '/settings' },

    // Module 2: Product Catalog
    {
      label: 'Products',
      icon: 'inventory',
      route: '/products',
      requiredModule: 'PRODUCT_CATALOG'
    },

    // Module 3: Customer Management
    {
      label: 'Customers',
      icon: 'people',
      route: '/customers',
      requiredModule: 'CUSTOMER_MANAGEMENT'
    },

    // Module 4: Contract Management
    {
      label: 'Contracts',
      icon: 'description',
      route: '/contracts',
      requiredModule: 'CONTRACT_MANAGEMENT'
    },

    // Module 5: SI Management
    {
      label: 'System Integrators',
      icon: 'handshake',
      route: '/system-integrators',
      requiredModule: 'SI_MANAGEMENT'
    },

    // Module 6: Asset Management
    {
      label: 'Assets',
      icon: 'devices',
      route: '/assets',
      requiredModule: 'ASSET_MANAGEMENT'
    },

    // Module 7: Location Services
    {
      label: 'Location Map',
      icon: 'map',
      route: '/location-map',
      requiredModule: 'LOCATION_SERVICES'
    }
  ];

  async getVisibleMenuItems(): Promise<MenuItem[]> {
    const activeModules = await this.moduleLicenseService.getActiveModules();

    return this.allMenuItems.filter(item => {
      if (!item.requiredModule) return true; // Always visible
      return activeModules.includes(item.requiredModule);
    });
  }
}
```

---

## Module Dependencies

### Dependency Matrix

| Module | Depends On |
|--------|-----------|
| Module 1: Complaint Core | None (Base Platform) |
| Module 2: Product Catalog | Module 1 |
| Module 3: Customer Management | Module 1 |
| Module 4: Contract Management | Module 1, 2, 3 |
| Module 5: SI Management | Module 1, 3, 4 |
| Module 6: Asset Management | Module 1, 2, 3, 4 |
| Module 7: Location Services | Module 1, 3 |

### Activation Rules
1. **Module 1** must always be active (base platform)
2. **Module 4** can only be activated if Module 2 and 3 are active
3. **Module 5** can only be activated if Module 3 and 4 are active
4. **Module 6** can only be activated if Module 2, 3, and 4 are active
5. Deactivating a module will warn if dependent modules are active

### Validation Service

```csharp
public class ModuleDependencyValidator
{
    private readonly Dictionary<string, string[]> _dependencies = new()
    {
        ["PRODUCT_CATALOG"] = new[] { "COMPLAINT_CORE" },
        ["CUSTOMER_MANAGEMENT"] = new[] { "COMPLAINT_CORE" },
        ["CONTRACT_MANAGEMENT"] = new[] { "COMPLAINT_CORE", "PRODUCT_CATALOG", "CUSTOMER_MANAGEMENT" },
        ["SI_MANAGEMENT"] = new[] { "COMPLAINT_CORE", "CUSTOMER_MANAGEMENT", "CONTRACT_MANAGEMENT" },
        ["ASSET_MANAGEMENT"] = new[] { "COMPLAINT_CORE", "PRODUCT_CATALOG", "CUSTOMER_MANAGEMENT", "CONTRACT_MANAGEMENT" },
        ["LOCATION_SERVICES"] = new[] { "COMPLAINT_CORE", "CUSTOMER_MANAGEMENT" }
    };

    public async Task<Result> ValidateActivationAsync(
        Guid companyId,
        string moduleLicenseKey,
        IModuleLicenseService licenseService)
    {
        if (!_dependencies.TryGetValue(moduleLicenseKey, out var dependencies))
            return Result.Success();

        var activeModules = await licenseService.GetActiveModulesAsync(companyId);
        var missingDependencies = dependencies.Except(activeModules).ToList();

        if (missingDependencies.Any())
        {
            return Result.Failure(
                $"Cannot activate {moduleLicenseKey}. Missing required modules: {string.Join(", ", missingDependencies)}",
                "MODULE_DEPENDENCY_NOT_MET"
            );
        }

        return Result.Success();
    }
}
```

---

## Implementation Roadmap

### Phase 1: Module Licensing Infrastructure (2-3 weeks)
✅ **Module 1**: Already 100% implemented
🆕 Database tables for module licensing
🆕 IModuleLicenseService implementation
🆕 API authorization attributes
🆕 Angular ModuleLicenseGuard and service
🆕 Dynamic menu visibility
🆕 License activation/deactivation UI
🆕 Module dependency validation

### Phase 2: Module 2 - Product Catalog (4-6 weeks)
🆕 8 new entities + repositories
🆕 CQRS commands/queries for product management
🆕 API endpoints (20+)
🆕 Angular components (10+)
🆕 Product-complaint integration
🆕 Dashboard statistics integration

### Phase 3: Module 3 - Customer Management (4-6 weeks)
🆕 6 new entities + repositories
🆕 CQRS commands/queries for customer management
🆕 API endpoints (15+)
🆕 Angular components (8+)
🆕 Customer-complaint integration
🆕 Customer health scoring

### Phase 4: Module 4 - Contract Management (6-8 weeks)
🆕 8 new entities + repositories
🆕 CQRS commands/queries for contract management
🆕 API endpoints (25+)
🆕 Angular components (12+)
🆕 Contract SLA enforcement
🆕 Service entitlement tracking
🆕 Automatic entitlement deduction

### Phase 5: Module 5 - SI Management (3-4 weeks)
🆕 5 new entities + repositories
🆕 CQRS commands/queries for SI management
🆕 API endpoints (12+)
🆕 Angular components (6+)
🆕 SI performance tracking
🆕 Automatic complaint routing to SI

### Phase 6: Module 6 - Asset Management (5-7 weeks)
🆕 6 new entities + repositories
🆕 CQRS commands/queries for asset management
🆕 API endpoints (18+)
🆕 Angular components (10+)
🆕 Asset-complaint integration
🆕 Warranty tracking
🆕 Service history logging

### Phase 7: Module 7 - Location Services (2-3 weeks)
🆕 2 new entities + repositories
🆕 Geocoding integration
🆕 Distance calculation APIs
🆕 Angular map components
🆕 Nearest technician finder
🆕 Geographical dashboard

**Total Timeline**: 6-9 months for complete implementation

---

## Pricing Model Suggestion

| Module | License Type | Suggested Price | Notes |
|--------|-------------|----------------|-------|
| Module 1: Complaint Core | Perpetual | Base Price | Required for all installations |
| Module 2: Product Catalog | Annual Subscription | +30% of base | Standard add-on |
| Module 3: Customer Management | Annual Subscription | +30% of base | Standard add-on |
| Module 4: Contract Management | Annual Subscription | +50% of base | Premium add-on |
| Module 5: SI Management | Annual Subscription | +40% of base | Enterprise add-on |
| Module 6: Asset Management | Annual Subscription | +50% of base | Premium add-on |
| Module 7: Location Services | Annual Subscription | +25% of base | Enterprise add-on |

**Example Pricing**:
- Base Platform (Module 1): $10,000/year
- Module 2 (Products): $3,000/year
- Module 3 (Customers): $3,000/year
- Module 4 (Contracts): $5,000/year
- Module 5 (SI): $4,000/year
- Module 6 (Assets): $5,000/year
- Module 7 (Location): $2,500/year

**Bundle Pricing**:
- **Standard Bundle** (Modules 1+2+3): $15,000/year (save $1,000)
- **Premium Bundle** (Modules 1+2+3+4+6): $25,000/year (save $3,000)
- **Enterprise Bundle** (All Modules): $30,000/year (save $5,500)

---

## Testing Strategy

### Unit Testing
- Module activation/deactivation logic
- Dependency validation
- License expiry checking
- API authorization filters

### Integration Testing
- Module-specific API endpoints with/without license
- Cross-module data flow (e.g., Contract → Complaint → Asset)
- UI route guards
- Menu visibility

### E2E Testing (Playwright)
- Activate module → Verify menu appears → Access feature
- Deactivate module → Verify menu disappears → Route blocked
- Attempt to access without license → Redirected to license page
- Module dependency validation during activation

---

## Documentation Requirements

For each module:
1. **User Manual**: Feature descriptions, screenshots, workflows
2. **Admin Guide**: Module activation, configuration, licensing
3. **API Documentation**: Swagger/OpenAPI specs for all endpoints
4. **Developer Guide**: Entity relationships, extension points, customization
5. **Migration Guide**: Database scripts, data migration procedures

---

## Conclusion

This modular architecture transforms the complaint management system into a comprehensive, enterprise-grade platform with:

- **7 Licensed Modules** (1 base + 6 add-ons)
- **130+ Total Entities** (95 existing + 35+ new)
- **Flexible Licensing** (perpetual base + annual subscriptions)
- **Scalable Revenue Model** (base + add-ons + bundles)
- **Progressive Enhancement** (customers buy only what they need)
- **Clean Architecture** (module independence, clear boundaries)
- **Backward Compatible** (Module 1 unchanged, extensions only)

The module licensing system provides complete control over feature visibility and access, enabling a true Software-as-a-Service model with flexible pricing tiers.

---

**Document Version**: 1.0.0
**Last Updated**: November 14, 2025
**Status**: Architecture Defined - Ready for Implementation
