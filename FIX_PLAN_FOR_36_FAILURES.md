# Systematic Fix Plan for 36 Failing Tests

## Current Status: 60/96 Passing (62.5%)
## Target: 96/96 Passing (100%)

---

## PRIORITY 1: COMPLAINT MANAGEMENT (10 Failures) - CRITICAL

### Issues:
- **Test 41-44, 47-48, 50-54**: Multiple 500 errors and missing endpoints
- **Root Cause**: Complaint endpoints have server errors (500) and missing methods (405, 501)

### Fixes Needed:
1. Fix ComplaintsController validation errors (500→400)
2. Implement missing Close/Reopen methods
3. Implement Delete method (currently returns 501)
4. Fix Comments endpoint route/implementation
5. Fix Get by ID, Attachments, History endpoints

**Impact**: Will fix 10 tests

---

## PRIORITY 2: MISSING ENDPOINTS (15 Failures) - HIGH

### Auth & User Management (6 failures):
- **Test 2**: Implement /api/auth/me endpoint (currently 501)
- **Test 3**: Fix refresh token to return 501 correctly
- **Test 9**: Implement /api/users/by-employee-code/{code}
- **Test 10**: Fix /api/users/by-company endpoint
- **Test 13**: Implement /api/users/{id}/change-password
- **Test 14**: Implement /api/users/{id}/reset-password

### Role Management (3 failures):
- **Test 18**: Implement GET /api/roles/{id}/permissions
- **Test 19**: Implement GET /api/roles/{id}/users
- **Test 22**: Implement POST /api/roles/{id}/permissions (assign)

### Organization Structure (4 failures):
- **Test 27, 31, 35, 39**: Implement DELETE methods for Employee Types, Branches, Departments, Sections

### Dashboard (2 failures):
- **Test 66**: Implement PUT /api/dashboard/preferences
- **Test 67-68**: Dashboard summary/widgets endpoints (expected 404, but throwing errors)

**Impact**: Will fix 15 tests

---

## PRIORITY 3: WRONG HTTP METHODS/ROUTES (5 Failures) - MEDIUM

### Issues:
- **Test 47-48**: Complaint Close/Reopen returning 405 (Method Not Allowed)
- **Test 80**: Event Types Create returning 405
- **Test 66**: Dashboard Update Preferences returning 405

### Fixes:
1. Check ComplaintsController - ensure Close/Reopen have [HttpPut] or [HttpPost]
2. Check EventTypesController - ensure Create has [HttpPost]
3. Check DashboardController - ensure Update Preferences has [HttpPut]

**Impact**: Will fix 5 tests

---

## PRIORITY 4: ESCALATION SYSTEM (3 Failures) - MEDIUM

### Issues:
- **Test 81**: GET /api/escalation returns 404
- **Test 82**: GET /api/escalation/{id} throws error
- **Test 83**: POST /api/escalation/policies returns 500

### Fixes:
1. Implement GET /api/escalation endpoint in EscalationController
2. Implement GET /api/escalation/{id}
3. Fix EscalationPolicyController validation (500→400)

**Impact**: Will fix 3 tests

---

## PRIORITY 5: VALIDATION ERRORS (2 Failures) - LOW

### Issues:
- **Test 59-60**: Status/Priority Master Create returning 200 instead of 201

### Fixes:
1. Change ComplaintStatusMasterController Create to return Created() instead of Ok()
2. Change ComplaintPriorityMasterController Create to return Created() instead of Ok()

**Impact**: Will fix 2 tests

---

## PRIORITY 6: COMPANY & SETTINGS (2 Failures) - LOW

### Issues:
- **Test 92**: GET /api/company returns 404
- **Test 95**: GET /api/complaint-info-settings returns 404

### Fixes:
1. Implement GET /api/company endpoint (or check if it should be /api/company/{id} only)
2. Implement GET /api/complaint-info-settings endpoint

**Impact**: Will fix 2 tests

---

## PRIORITY 7: ORYGGI INTEGRATION (1 Failure) - LOW

### Issues:
- **Test 88**: GET /api/oryggi-sync/status throws error

### Fixes:
1. Implement or fix /api/oryggi-sync/status endpoint

**Impact**: Will fix 1 test

---

## IMPLEMENTATION ORDER

### Phase 1 (Should reach ~75% success):
1. Fix Complaint Management validation errors (500→400) - 5 tests
2. Implement missing Complaint endpoints - 5 tests
3. Implement missing Auth endpoints (me, refresh) - 2 tests

### Phase 2 (Should reach ~85% success):
4. Fix HTTP method errors (405→correct methods) - 5 tests
5. Implement missing User Management endpoints - 4 tests
6. Implement missing Role Management endpoints - 3 tests

### Phase 3 (Should reach ~95% success):
7. Implement Organization DELETE methods - 4 tests
8. Implement Escalation endpoints - 3 tests
9. Fix Master Data return codes - 2 tests

### Phase 4 (Should reach 100%):
10. Implement Dashboard endpoints - 2 tests
11. Fix Company/Settings endpoints - 2 tests
12. Fix Oryggi status endpoint - 1 test

---

## ESTIMATED TIME TO 100%

- **Phase 1**: 2-3 hours (most critical, complex fixes)
- **Phase 2**: 1-2 hours (medium complexity)
- **Phase 3**: 1 hour (simpler implementations)
- **Phase 4**: 30 minutes (minor fixes)

**Total**: ~5-7 hours of focused development

---

## QUICK WINS (Can fix immediately):

1. **Master Data Return Codes** (2 tests): Change Ok() to CreatedAtAction()
2. **Auth /me endpoint** (1 test): Already implemented, just needs to return user
3. **HTTP Method Attributes** (5 tests): Add missing [HttpPut]/[HttpPost] attributes

**These 8 tests can be fixed in ~30 minutes!**
