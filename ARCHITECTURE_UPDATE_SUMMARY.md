# Architecture Update Summary
## Date: November 14, 2025

---

## Key Changes to Modular Architecture

### 1. OEM Name Field Added to Products (Module 2)

**Entity**: `Product`

**New Field**:
```csharp
public string OEMName { get; set; }  // Original Equipment Manufacturer brand
```

**Distinction**:
- **OEMName**: The brand name (e.g., "Dell", "HP", "Cisco", "Lenovo")
- **Manufacturer**: The actual factory/company that produces it (e.g., "Quanta Computer", "Foxconn")

**Example Use Cases**:
- A Dell laptop might have:
  - OEMName: "Dell"
  - Manufacturer: "Quanta Computer" (Dell's manufacturing partner)

- A Cisco switch might have:
  - OEMName: "Cisco"
  - Manufacturer: "Foxconn"

**Business Value**:
- Track complaints by OEM brand for warranty claims
- Generate OEM-specific failure rate reports
- Compare reliability across different OEMs
- Support OEM-specific service contracts

---

### 2. Project-SI-Customer Three-Way Linkage (Module 3)

**Entity**: `CustomerProject`

**New Field**:
```csharp
public Guid? SystemIntegratorId { get; set; }  // Links project to SI partner
public SystemIntegrator SystemIntegrator { get; set; }  // Navigation property
```

**Relationship Flow**:
```
Customer
    ↓ has
CustomerProject
    ↓ assigned to
SystemIntegrator (SI)
```

**Example Scenario**:
```
Customer: ABC Corporation
    ↓
Project: ERP Implementation Phase 2
    ↓
System Integrator: XYZ Tech Partners
    ↓
Complaints: Automatically routed to XYZ Tech
```

**Business Value**:
- **Automatic Complaint Routing**: Complaints for a project are routed to the SI handling that project
- **SI Performance Tracking**: Track SI performance per project (resolution time, SLA compliance, customer satisfaction)
- **Project-Level Accountability**: Clear ownership - which SI is responsible for which project
- **Performance Dashboards**: "Show XYZ Tech's performance across all their projects"

---

## Updated Entity Definitions

### Product Entity (Module 2)
```csharp
public class Product
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid ProductCategoryId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // NEW: Separate OEM and Manufacturer
    public string OEMName { get; set; }         // Brand: Dell, HP, Cisco
    public string Manufacturer { get; set; }    // Factory: Quanta, Foxconn

    public string ModelNumber { get; set; }
    public bool IsWarrantySupported { get; set; }
    public int DefaultWarrantyMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### CustomerProject Entity (Module 3)
```csharp
public class CustomerProject
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    // NEW: Link to System Integrator
    public Guid? SystemIntegratorId { get; set; }

    public string ProjectCode { get; set; }
    public string ProjectName { get; set; }
    public ProjectType ProjectType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public decimal ProjectValue { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; }
    public SystemIntegrator SystemIntegrator { get; set; }  // NEW
}
```

---

## New Business Intelligence Reports Enabled

### OEM-Related Reports:
1. **OEM Complaint Summary**: Total complaints by OEM brand (Dell, HP, Cisco, etc.)
2. **OEM vs Manufacturer Analysis**: Compare failure rates between same OEM but different manufacturers
3. **OEM Performance Trend**: Track complaint trends over time per OEM
4. **OEM Service Level**: Average resolution time per OEM product
5. **OEM Warranty Claims**: Track warranty-covered complaints by OEM

### Project-SI-Customer Reports:
1. **SI Performance by Project**: How is XYZ Tech performing on ABC Corp's ERP project?
2. **SI Project Portfolio**: Which projects is each SI currently handling?
3. **Customer-SI Relationship Map**: Which SIs are working with which customers?
4. **SI Complaint Workload**: How many complaints is each SI handling per project?
5. **SI SLA Compliance by Project**: Is the SI meeting SLAs on customer projects?
6. **Project Health Dashboard**: Status of all projects with complaint metrics

---

## Cross-Module Integration Points

### Complaint Form Enhancements (Module 1 + 2 + 3 + 5)
When creating a complaint:
1. Select **Customer** (Module 3)
2. Select **Project** (Module 3) - filtered by selected customer
3. System auto-populates **System Integrator** from project (Module 5)
4. Select **Product** (Module 2) - displays OEM name
5. Select **Asset** (Module 6) - filtered by customer/site

### Complaint Routing Logic
```csharp
// Pseudo-code for automatic SI routing
if (complaint.CustomerProjectId != null)
{
    var project = await GetProjectAsync(complaint.CustomerProjectId);
    if (project.SystemIntegratorId != null)
    {
        // Auto-assign complaint to SI's resource pool
        var siResourcePool = await GetSIResourcePoolAsync(project.SystemIntegratorId);
        complaint.AssignedResourcePoolId = siResourcePool.Id;
    }
}
```

---

## Database Migration Impact

### New Columns to Add:

**Products Table** (Module 2):
```sql
ALTER TABLE Products
ADD OEMName NVARCHAR(200) NULL;

-- Later update to make required after data migration
ALTER TABLE Products
ALTER COLUMN OEMName NVARCHAR(200) NOT NULL;
```

**CustomerProjects Table** (Module 3):
```sql
ALTER TABLE CustomerProjects
ADD SystemIntegratorId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_CustomerProjects_SystemIntegrators
    FOREIGN KEY (SystemIntegratorId) REFERENCES SystemIntegrators(Id);

CREATE INDEX IX_CustomerProjects_SystemIntegratorId
ON CustomerProjects(SystemIntegratorId);
```

---

## API Endpoint Updates

### Module 2: Products API
**Existing endpoints enhanced**:
- `GET /api/products` - Now returns OEMName
- `POST /api/products` - OEMName required
- `PUT /api/products/{id}` - Can update OEMName

**New endpoints**:
- `GET /api/products/by-oem/{oemName}` - Get all products by OEM
- `GET /api/products/oems` - Get list of all OEMs
- `GET /api/reports/complaints-by-oem` - Complaint statistics by OEM

### Module 3: Projects API
**Existing endpoints enhanced**:
- `GET /api/customer-projects` - Now returns SystemIntegratorId and SystemIntegrator details
- `POST /api/customer-projects` - SystemIntegratorId optional
- `PUT /api/customer-projects/{id}` - Can update SystemIntegratorId

**New endpoints**:
- `GET /api/customer-projects/by-si/{siId}` - Get all projects for an SI
- `GET /api/customer-projects/{id}/complaints` - Get complaints for a project
- `GET /api/reports/si-performance-by-project` - SI performance metrics per project

---

## UI Component Updates

### Product Management UI (Module 2)
**Product Form**:
- Add "OEM Name" field (dropdown with autocomplete)
- Keep existing "Manufacturer" field
- Display both in product list and detail views

**Product List Filters**:
- Filter by OEM
- Filter by Manufacturer
- Show OEM badge in product cards

### Customer Project Management UI (Module 3)
**Project Form**:
- Add "System Integrator" dropdown (filtered by active SIs)
- Display SI contact information when selected
- Show SI performance rating

**Project Detail View**:
- Display assigned SI with contact details
- Show SI performance metrics for this project
- List complaints handled by SI for this project

### Complaint Form UI (Module 1)
**Enhanced Fields**:
- Customer → auto-loads customer projects
- Project → auto-populates SI (read-only)
- Product → displays OEM name badge
- Asset → shows OEM and product details

---

## Testing Scenarios

### Test Case 1: OEM Tracking
1. Create product: "Dell Latitude 7490"
   - OEMName: "Dell"
   - Manufacturer: "Quanta Computer"
2. Create complaint for this product
3. Verify complaint shows OEM: "Dell"
4. Run report: "Complaints by OEM"
5. Verify Dell appears in report

### Test Case 2: Project-SI Linkage
1. Create Customer: "ABC Corporation"
2. Create System Integrator: "XYZ Tech Partners"
3. Create Project: "ERP Implementation" for ABC Corp, assigned to XYZ Tech
4. Create Complaint linked to this project
5. Verify complaint is auto-assigned to XYZ Tech's resource pool
6. Verify complaint appears in "XYZ Tech's Project Complaints" dashboard

### Test Case 3: Cross-Module Flow
1. Create complete flow: Customer → Project (with SI) → Product (with OEM) → Asset → Contract
2. Create complaint linking all entities
3. Verify all relationships visible in complaint detail
4. Verify automatic entitlement deduction
5. Verify SI routing
6. Verify OEM-specific SLA applied

---

## Migration Strategy

### Phase 1: Database Updates (Week 1)
- Add OEMName column to Products
- Add SystemIntegratorId to CustomerProjects
- Create foreign key constraints
- Create indexes

### Phase 2: Backend Code (Week 1-2)
- Update Product entity with OEMName
- Update CustomerProject entity with SystemIntegratorId
- Update DTOs and mapping profiles
- Update API endpoints
- Add new query handlers

### Phase 3: Frontend Updates (Week 2-3)
- Update product forms with OEM field
- Update project forms with SI dropdown
- Update complaint form with enhanced fields
- Add OEM filter to product lists
- Add SI filter to project lists

### Phase 4: Data Migration (Week 3)
- Script to populate OEMName from existing data
- Script to link existing projects to SIs (manual review needed)
- Validation scripts

### Phase 5: Testing & Validation (Week 4)
- Unit tests for new fields
- Integration tests for cross-module flows
- E2E tests for complete scenarios
- Performance testing with new queries

---

## Documentation Updates Required

1. **User Manual**:
   - Section on OEM vs Manufacturer distinction
   - Guide to linking projects with SIs
   - How to use OEM-based filtering and reports

2. **API Documentation**:
   - Update Swagger/OpenAPI specs
   - Add examples with new fields

3. **Admin Guide**:
   - How to configure OEM list
   - How to manage SI-Project assignments
   - Best practices for data entry

4. **Developer Guide**:
   - Entity relationship diagrams updated
   - Database schema changes documented
   - Migration scripts provided

---

## Benefits Summary

### For Business Users:
- Better product tracking by brand (OEM)
- Clear SI accountability per project
- Automatic complaint routing to responsible SI
- Comprehensive performance metrics

### For Administrators:
- OEM-based warranty management
- SI performance monitoring
- Project-level oversight
- Better resource allocation

### For Developers:
- Clean entity relationships
- Flexible querying options
- Scalable architecture
- Easy reporting capabilities

---

**Status**: Architecture Updated
**Document Version**: 1.1.0
**Last Updated**: November 14, 2025
**Updated By**: Claude Code
