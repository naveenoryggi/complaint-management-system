# SLA Calculator - End-to-End Test Plan

## Test Objective
Validate the complete SLA Management System using Playwright browser automation to test:
- SLA Settings configuration
- SLA Levels creation
- Category-SLA mappings
- Priority-SLA mappings
- Complaint creation with automatic SLA calculation
- Verification of calculated deadlines
- Fallback hierarchy testing

---

## Test Scenarios

### Phase 1: Setup & Configuration (Admin Portal)

#### Test 1.1: Configure SLA Settings
**Steps:**
1. Login as admin
2. Navigate to Admin → SLA Management → Settings
3. Configure SLA Settings:
   - Enable SLA System: Yes
   - Working Hours Only: Yes
   - Working Hours: 09:00 - 17:00
   - Working Days: Monday - Friday (1,2,3,4,5)
   - Auto Escalate on Breach: Yes
   - Escalation Threshold: 80%
4. Save settings
5. Verify success message

**Expected Result:**
- Settings saved successfully
- Working hours configuration active

---

#### Test 1.2: Create SLA Levels
**Steps:**
1. Navigate to Admin → SLA Management → SLA Levels
2. Create SLA Level "Gold"
   - Response Time: 1 hour
   - Resolution Time: 4 hours
   - Description: "Premium support - fastest response"
3. Create SLA Level "Silver"
   - Response Time: 2 hours
   - Resolution Time: 8 hours
   - Description: "Standard support - normal response"
4. Create SLA Level "Bronze"
   - Response Time: 4 hours
   - Resolution Time: 24 hours
   - Description: "Basic support - standard response"
5. Verify all three levels appear in the list

**Expected Result:**
- 3 SLA Levels created successfully
- All visible in the SLA Levels list

---

#### Test 1.3: Create Category-SLA Mappings
**Steps:**
1. Navigate to Admin → SLA Management → Category Mappings (Tab 3)
2. Create mapping for "IT Support" category
   - Category: IT Support
   - SLA Level: Gold
   - Override Response Time: (leave empty - use level default)
   - Override Resolution Time: (leave empty - use level default)
3. Create mapping for "HR Issues" category
   - Category: HR Issues
   - SLA Level: Silver
4. Save mappings
5. Verify mappings appear in the list

**Expected Result:**
- 2 Category-SLA mappings created
- Mappings show correct SLA levels

---

#### Test 1.4: Create Priority-SLA Mappings
**Steps:**
1. Navigate to Admin → SLA Management → Priority Mappings (Tab 4)
2. Create mapping for "Critical" priority
   - Priority: Critical
   - SLA Level: Gold
   - Override Response Time: 30 minutes
   - Override Resolution Time: 2 hours
3. Create mapping for "High" priority
   - Priority: High
   - SLA Level: Silver
4. Save mappings
5. Verify mappings appear in the list

**Expected Result:**
- 2 Priority-SLA mappings created
- Critical priority has override times
- High priority uses Silver level defaults

---

### Phase 2: Complaint Creation & SLA Testing

#### Test 2.1: Create Complaint with Priority-SLA Mapping (Highest Priority)
**Steps:**
1. Navigate to Complaints → New Complaint
2. Fill complaint form:
   - Title: "Critical Server Outage"
   - Description: "Production server is down"
   - Category: IT Support
   - Priority: Critical
3. Submit complaint
4. Capture complaint number and due date
5. Navigate to complaint details
6. Verify SLA information displayed

**Expected Result:**
- Complaint created successfully
- Due date calculated using Priority-SLA mapping (Gold level with overrides)
- Resolution time: 2 hours from submission
- SLA source: "PriorityMapping"

**Calculation Check:**
- If submitted at 10:00 AM
- Due date should be 12:00 PM (2 hours)

---

#### Test 2.2: Create Complaint with Category-SLA Mapping
**Steps:**
1. Create new complaint:
   - Title: "Payroll Issue"
   - Description: "Incorrect salary calculation"
   - Category: HR Issues
   - Priority: Normal (no priority mapping exists)
2. Submit complaint
3. Verify due date

**Expected Result:**
- Complaint created successfully
- Due date calculated using Category-SLA mapping (Silver level)
- Resolution time: 8 hours from submission
- SLA source: "CategoryMapping"

**Calculation Check:**
- If submitted at 10:00 AM
- Due date should be 6:00 PM same day (8 hours, within working hours)

---

#### Test 2.3: Create Complaint with Priority Master Fallback
**Steps:**
1. Create new complaint:
   - Title: "General Inquiry"
   - Description: "Question about policy"
   - Category: General (no category mapping)
   - Priority: Normal (no priority mapping for Normal)
2. Submit complaint
3. Verify due date

**Expected Result:**
- Complaint created successfully
- Due date calculated using Priority Master default SLA
- Falls back to legacy Priority Master configuration
- SLA source: "PriorityMaster"

**Calculation Check:**
- Should use Priority Master SLA hours configured in database
- Backend logs should show: "Using Priority Master SLA for priority..."

---

#### Test 2.4: Create Complaint with Category Default Fallback
**Steps:**
1. Create new complaint:
   - Title: "Unmapped Category Test"
   - Description: "Testing fallback to category default"
   - Category: (select category with no SLA mapping)
   - Priority: Low (no priority mapping)
2. Submit complaint
3. Verify due date

**Expected Result:**
- Complaint created successfully
- Due date calculated using Category.DefaultSlaHours
- SLA source: "CategoryDefault"

---

#### Test 2.5: Working Hours Test - Complaint Created After Hours
**Steps:**
1. Manually set system time to 6:00 PM (after working hours)
2. Create complaint:
   - Title: "After Hours Test"
   - Category: IT Support (Gold SLA - 4 hours)
   - Priority: High
3. Verify due date

**Expected Result:**
- Due date should be calculated for next working day
- Should NOT include evening/night hours
- Example: Created 6:00 PM Friday → Due 1:00 PM Monday (4 working hours)

---

#### Test 2.6: Working Hours Test - Multi-Day Calculation
**Steps:**
1. Create complaint at 3:00 PM Friday:
   - Title: "Weekend Test"
   - Category: HR Issues (Silver SLA - 8 hours)
   - Priority: Normal
2. Verify due date

**Expected Result:**
- Due date calculation should skip weekend
- Example: Created 3:00 PM Friday → Due 12:00 PM Monday
  - Friday: 2 hours (3 PM - 5 PM)
  - Weekend: Skipped
  - Monday: 6 hours (9 AM - 3 PM)
  - Total: 8 working hours

---

### Phase 3: Dashboard & Reporting

#### Test 3.1: Verify SLA Information on Dashboard
**Steps:**
1. Navigate to Dashboard
2. Locate created complaints
3. Verify SLA indicators:
   - Time remaining shown
   - Percentage complete shown
   - Overdue complaints highlighted in red

**Expected Result:**
- Dashboard shows SLA status for all complaints
- Visual indicators for SLA health

---

#### Test 3.2: Verify SLA Information on Complaint Details
**Steps:**
1. Open any created complaint
2. Verify SLA section shows:
   - Submitted date/time
   - Due date/time
   - Time remaining
   - SLA percentage
   - SLA status (On Track / At Risk / Breached)

**Expected Result:**
- Complete SLA information displayed
- Visual status indicators working

---

### Phase 4: SLA Breach Testing

#### Test 4.1: Simulate SLA Breach
**Steps:**
1. Create complaint with short SLA (1 hour)
2. Wait for SLA to breach (or manually update due date in database)
3. Refresh complaint details
4. Verify breach indicators

**Expected Result:**
- Complaint marked as SLA breached
- Visual indicator (red badge/highlight)
- Breach notification sent (if configured)

---

## Test Data Summary

### SLA Levels to Create
| Name | Response | Resolution | Description |
|------|----------|-----------|-------------|
| Gold | 1 hour | 4 hours | Premium support |
| Silver | 2 hours | 8 hours | Standard support |
| Bronze | 4 hours | 24 hours | Basic support |

### Category Mappings
| Category | SLA Level | Override Response | Override Resolution |
|----------|-----------|-------------------|---------------------|
| IT Support | Gold | - | - |
| HR Issues | Silver | - | - |

### Priority Mappings
| Priority | SLA Level | Override Response | Override Resolution |
|----------|-----------|-------------------|---------------------|
| Critical | Gold | 30 min | 2 hours |
| High | Silver | - | - |

### Test Complaints
| Title | Category | Priority | Expected SLA Source |
|-------|----------|----------|---------------------|
| Critical Server Outage | IT Support | Critical | PriorityMapping |
| Payroll Issue | HR Issues | Normal | CategoryMapping |
| General Inquiry | General | Normal | PriorityMaster |
| After Hours Test | IT Support | High | PriorityMapping |
| Weekend Test | HR Issues | Normal | CategoryMapping |

---

## Success Criteria

- [ ] All SLA configurations save successfully
- [ ] Complaints created with correct due dates
- [ ] SLA fallback hierarchy works correctly
- [ ] Working hours calculation accurate
- [ ] Weekend/holiday skipping works
- [ ] Dashboard shows SLA information
- [ ] Complaint details show SLA status
- [ ] SLA breach detection works
- [ ] All 6 fallback levels can be demonstrated

---

## Test Execution Approach

Using Playwright browser automation:
1. Navigate to login page
2. Authenticate as admin
3. Execute each test scenario
4. Capture screenshots at key steps
5. Verify expected results
6. Log all observations
7. Create final test report

---

## Estimated Duration
- Setup & Configuration: 15 minutes
- Complaint Creation Tests: 20 minutes
- Dashboard Verification: 10 minutes
- SLA Breach Testing: 10 minutes
- Total: ~55 minutes

---

## Prerequisites
- Backend server running on http://localhost:5058
- Frontend server running on http://localhost:4200
- Admin credentials available
- Database seeded with basic data (categories, priorities, users)
