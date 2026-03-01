# Tender Automation Platform - Testing & Validation Plan

## Overview

This document provides a comprehensive testing plan to validate that all implemented features work correctly before deployment.

---

## Pre-Test Setup

### 1. Environment Setup

**Backend (Python FastAPI):**
```bash
cd tender-automation/backend/python-api

# Install dependencies
pip install -r requirements.txt

# Set environment variables
copy .env.example .env
# Edit .env and set:
# - DATABASE_URL
# - JWT_SECRET_KEY
# - ANTHROPIC_API_KEY

# Run database migrations
alembic upgrade head

# Start server
uvicorn app.main:app --reload --port 8000
```

**Backend (.NET - for Auth):**
```bash
cd complaint-system-dotnet
dotnet run --project src/ComplaintManagement.API
```

**Frontend (Angular):**
```bash
cd complaint-system-angular

# Install dependencies
npm install

# Start dev server
ng serve --port 4200
```

### 2. Test User Setup

**Create Test User in Database:**
```sql
-- Insert test tenant
INSERT INTO tenants (id, name, is_active)
VALUES ('00000000-0000-0000-0000-000000000001', 'Test Tender Company', true);

-- Insert test user
INSERT INTO users (id, tenant_id, email, password_hash, role)
VALUES (
  '00000000-0000-0000-0000-000000000002',
  '00000000-0000-0000-0000-000000000001',
  'test@tendercompany.com',
  '[HASHED_PASSWORD]',
  'admin'
);
```

**Test Credentials:**
- Email: `test@tendercompany.com`
- Password: `Test@123456`

---

## Testing Checklist

### Phase 1: Authentication & Basic Access ✓

**Test 1.1: User Login**
- [ ] Navigate to `http://localhost:4200`
- [ ] Enter test credentials
- [ ] Verify successful login
- [ ] Verify JWT token is stored in localStorage
- [ ] Verify user redirected to dashboard

**Test 1.2: Token Validation**
- [ ] Open browser DevTools → Network
- [ ] Make any API call
- [ ] Verify `Authorization: Bearer [token]` header present
- [ ] Verify Python API validates token correctly
- [ ] Verify tenant_id extracted from token

**Expected Results:**
✅ User can log in
✅ JWT token stored and sent with requests
✅ Python API accepts .NET JWT tokens
✅ Tenant isolation working

---

### Phase 2: Document Management ✓

**Test 2.1: Document Upload**
- [ ] Navigate to Documents section
- [ ] Click "Upload Document"
- [ ] Select a PDF file (e.g., company_profile.pdf)
- [ ] Add tags: `company`, `profile`
- [ ] Set document type: `Technical`
- [ ] Click Upload

**Validation:**
```bash
# Check file in storage
ls tender-automation/backend/python-api/uploads/documents/

# Verify in database
SELECT * FROM documents WHERE tenant_id = '00000000-0000-0000-0000-000000000001';
```

**Test 2.2: Document Search**
- [ ] Use search box to search "company"
- [ ] Verify document appears in results
- [ ] Filter by tag "profile"
- [ ] Verify filtering works

**Test 2.3: Document Download**
- [ ] Click on document in list
- [ ] Click "Download" button
- [ ] Verify file downloads correctly
- [ ] Verify MIME type is correct

**Expected Results:**
✅ Document uploads successfully
✅ File stored in correct location
✅ Database record created with tenant_id
✅ Search and filtering work
✅ Download works

---

### Phase 3: AI Document Generation ✓

**Test 3.1: Technical Solution Generation**
- [ ] Navigate to AI Generator
- [ ] Select "Technical Solution & Approach"
- [ ] Fill in context:
  ```
  Tender Title: Smart City Command Center Implementation
  Issuing Authority: Municipal Corporation of Greater Mumbai
  Company Name: TechSolutions Pvt Ltd
  Company Profile: Leading IT solutions provider with 15+ years experience
  Requirements:
  - IoT sensor network deployment
  - Real-time data analytics
  - Citizen mobile application
  - Command and control center
  Scope of Work: Design, develop, deploy integrated smart city platform
  ```
- [ ] Select Model: Claude Sonnet 4
- [ ] Click "Generate Document"
- [ ] Wait for generation (15-30 seconds)

**Validation:**
- [ ] Verify generated content appears in editor
- [ ] Verify content is relevant and professional
- [ ] Check tokens used (should be 2000-3000)
- [ ] Verify document saved as DOCX
- [ ] Download DOCX and verify formatting
- [ ] Check letterhead is present
- [ ] Verify proper heading hierarchy

**Test 3.2: Other Document Types**
Repeat for each type:
- [ ] Compliance Declaration
- [ ] Covering Letter
- [ ] Executive Summary
- [ ] Project Methodology

**Test 3.3: Usage Statistics**
- [ ] Navigate to AI Usage Stats
- [ ] Verify total generations count
- [ ] Verify total tokens used
- [ ] Verify estimated cost calculation
- [ ] Verify generations by type breakdown

**Database Validation:**
```sql
-- Check AI generations
SELECT
  generation_type,
  model_used,
  tokens_used,
  created_at
FROM ai_generations
WHERE tenant_id = '00000000-0000-0000-0000-000000000001'
ORDER BY created_at DESC;

-- Verify cost tracking
SELECT
  SUM(tokens_used) as total_tokens,
  COUNT(*) as total_generations
FROM ai_generations
WHERE tenant_id = '00000000-0000-0000-0000-000000000001';
```

**Expected Results:**
✅ AI generates relevant content
✅ DOCX formatting is professional
✅ Letterhead included
✅ Token tracking accurate
✅ Cost estimation working
✅ Database records created

---

### Phase 4: Tender Management ✓

**Test 4.1: Create Tender**
- [ ] Navigate to Tenders → Create New
- [ ] Fill in details:
  ```
  Title: Smart City Command Center Implementation
  Reference: TENDER/2024/SC/001
  Issuing Authority: Municipal Corporation of Greater Mumbai
  Portal: GeM (Government e-Marketplace)
  Portal URL: https://gem.gov.in/tender/12345
  Deadline: [30 days from now]
  Estimated Value: 50000000
  Requirements:
    Technical:
    - IoT sensor deployment (500 sq km coverage)
    - Real-time video analytics
    - Mobile app for citizens
    - 24/7 command center

    Compliance:
    - ISO 27001 certification
    - CMMI Level 5
    - Make in India compliance
  Notes: High priority tender with strict timelines
  Status: Draft
  ```
- [ ] Click "Create Tender"

**Validation:**
```sql
SELECT * FROM tenders
WHERE tenant_id = '00000000-0000-0000-0000-000000000001'
ORDER BY created_at DESC
LIMIT 1;
```

**Test 4.2: Tender List**
- [ ] Navigate to Tenders list
- [ ] Verify new tender appears
- [ ] Check status badge shows "Draft"
- [ ] Verify deadline shows "X days left"
- [ ] Check document count shows "0"

**Test 4.3: Search & Filter**
- [ ] Search for "Smart City"
- [ ] Verify tender appears
- [ ] Filter by status "Draft"
- [ ] Verify filtering works
- [ ] Test pagination (if > 50 tenders)

**Test 4.4: Update Tender**
- [ ] Click on tender
- [ ] Click "Edit"
- [ ] Change status to "In Progress"
- [ ] Update notes
- [ ] Save changes
- [ ] Verify updates saved
- [ ] Check status badge updated

**Expected Results:**
✅ Tender created successfully
✅ All fields saved correctly
✅ List view shows tender
✅ Search and filtering work
✅ Status workflow functions
✅ Deadline calculations correct

---

### Phase 5: Document Association ✓

**Test 5.1: Associate Documents**
- [ ] Open tender detail view
- [ ] Click "Add Documents"
- [ ] Select documents:
  - Company Profile (uploaded)
  - Technical Solution (AI-generated)
  - Compliance Declaration (AI-generated)
  - Covering Letter (AI-generated)
- [ ] Set order for each document
- [ ] Mark AI-generated ones as "Generated"
- [ ] Save associations

**Validation:**
```sql
SELECT
  td.document_order,
  td.is_generated,
  d.name,
  d.document_type
FROM tender_documents td
JOIN documents d ON td.document_id = d.id
WHERE td.tender_id = '[TENDER_ID]'
ORDER BY td.document_order;
```

**Test 5.2: View Associated Documents**
- [ ] Verify documents appear in order
- [ ] Check "AI Generated" badge on generated docs
- [ ] Verify document count updated in list view

**Test 5.3: Remove Association**
- [ ] Click remove on a document
- [ ] Confirm deletion
- [ ] Verify document removed from tender
- [ ] Verify original document still exists
- [ ] Verify count updated

**Expected Results:**
✅ Documents associate correctly
✅ Order is maintained
✅ AI-generated flag shows
✅ Removal works (association only)
✅ Original documents preserved

---

### Phase 6: PDF Assembly ✓

**Test 6.1: Merge PDFs with Cover**
- [ ] Navigate to tender detail
- [ ] Ensure 3+ PDF documents associated
- [ ] Click "Merge PDFs"
- [ ] System generates cover page
- [ ] System merges all PDFs
- [ ] Download merged PDF

**Validation:**
- [ ] Open merged PDF
- [ ] Verify cover page present with:
  - Company name
  - Tender title
  - Reference number
  - Issuing authority
  - Submission details
- [ ] Verify all PDFs merged in order
- [ ] Check page count matches
- [ ] Verify no quality loss

**Test 6.2: Export ZIP Package**
- [ ] Click "Export Package"
- [ ] System creates:
  - Merged PDF with cover
  - Individual files
  - ZIP archive
- [ ] Download ZIP
- [ ] Extract and verify contents

**Validation:**
```bash
# Check assembled files
ls tender-automation/backend/python-api/uploads/assembled/
ls tender-automation/backend/python-api/uploads/packages/

# Verify ZIP contents
unzip -l [package_name].zip
```

**Expected Files in ZIP:**
- `SmartCity_Tender_merged.pdf` (merged with cover)
- `Company_Profile.pdf`
- `Technical_Solution.docx`
- `Compliance_Declaration.docx`
- `Covering_Letter.docx`

**Test 6.3: Generate Standalone Cover**
- [ ] Click "Generate Cover Page"
- [ ] Fill in details
- [ ] Download cover PDF
- [ ] Verify professional formatting

**Expected Results:**
✅ PDF merge works correctly
✅ Cover page professionally formatted
✅ ZIP package contains all files
✅ File names clean and readable
✅ Download functionality works

---

### Phase 7: Complete Workflow Test ✓

**End-to-End Test: Complete Tender Submission**

**Scenario:** Prepare and submit Smart City tender

**Step 1: Document Preparation (5 mins)**
1. Upload company profile PDF
2. Upload financial statements PDF
3. Upload past project certificate PDF

**Step 2: AI Content Generation (10 mins)**
1. Generate Technical Solution (1500 words)
2. Generate Compliance Declaration
3. Generate Covering Letter
4. Generate Executive Summary
5. Review and edit each document
6. Download all as DOCX

**Step 3: Tender Creation (2 mins)**
1. Create tender with all details
2. Set deadline 30 days out
3. Add requirements as JSON

**Step 4: Document Association (3 mins)**
1. Associate all 7 documents
2. Order them:
   - Covering Letter
   - Executive Summary
   - Technical Solution
   - Compliance Declaration
   - Company Profile
   - Financial Statements
   - Project Certificates

**Step 5: Final Package (5 mins)**
1. Review tender detail page
2. Click "Export Package"
3. Download ZIP
4. Verify all files present
5. Check merged PDF quality

**Total Time:** 25 minutes
**Expected Time Savings:** 90% (vs 3-5 days manual)

**Success Criteria:**
- [ ] All documents uploaded successfully
- [ ] AI generated 4 documents
- [ ] Tender created with all details
- [ ] 7 documents associated in order
- [ ] ZIP package exported successfully
- [ ] Merged PDF includes cover page
- [ ] All individual files present
- [ ] Professional quality throughout

---

### Phase 8: Multi-Tenant Isolation ✓

**Test 8.1: Create Second Tenant**
```sql
INSERT INTO tenants (id, name, is_active)
VALUES ('00000000-0000-0000-0000-000000000003', 'Competitor Company', true);

INSERT INTO users (id, tenant_id, email, password_hash, role)
VALUES (
  '00000000-0000-0000-0000-000000000004',
  '00000000-0000-0000-0000-000000000003',
  'test@competitor.com',
  '[HASHED_PASSWORD]',
  'admin'
);
```

**Test 8.2: Verify Isolation**
- [ ] Login as Tenant 1 user
- [ ] Create a tender
- [ ] Upload a document
- [ ] Logout
- [ ] Login as Tenant 2 user
- [ ] Navigate to Tenders
- [ ] Verify Tenant 1's tender NOT visible
- [ ] Navigate to Documents
- [ ] Verify Tenant 1's documents NOT visible

**Database Validation:**
```sql
-- Verify tenant isolation in queries
SELECT * FROM tenders WHERE tenant_id = '00000000-0000-0000-0000-000000000001';
SELECT * FROM tenders WHERE tenant_id = '00000000-0000-0000-0000-000000000003';

-- These should return different results
```

**Expected Results:**
✅ Each tenant sees only their data
✅ No cross-tenant data leakage
✅ API enforces tenant_id filtering
✅ Database constraints working

---

### Phase 9: Performance Testing ✓

**Test 9.1: API Response Times**

Create test script:
```python
import requests
import time

BASE_URL = "http://localhost:8000/api/v1"
headers = {"Authorization": f"Bearer {token}"}

# Test tender list
start = time.time()
response = requests.get(f"{BASE_URL}/tenders?page=1&page_size=50", headers=headers)
print(f"List tenders: {(time.time() - start) * 1000:.0f}ms")

# Test document search
start = time.time()
response = requests.get(f"{BASE_URL}/documents/search?q=company", headers=headers)
print(f"Search documents: {(time.time() - start) * 1000:.0f}ms")

# Test AI generation
start = time.time()
response = requests.post(f"{BASE_URL}/ai/generate", headers=headers, json={...})
print(f"AI generation: {(time.time() - start):.1f}s")
```

**Performance Targets:**
- Tender list (50): < 100ms
- Document search: < 80ms
- Create tender: < 30ms
- AI generation: 15-30s (acceptable)
- PDF merge: < 2s
- ZIP export: < 3s

**Test 9.2: Concurrent Users**
- [ ] Simulate 10 concurrent users
- [ ] Each performs CRUD operations
- [ ] Verify no performance degradation
- [ ] Check database connection pool

**Expected Results:**
✅ All operations within acceptable time
✅ No timeout errors
✅ Database handles concurrent requests
✅ No memory leaks

---

### Phase 10: Security Testing ✓

**Test 10.1: Authentication**
- [ ] Try accessing `/api/v1/tenders` without token
- [ ] Verify 401 Unauthorized response
- [ ] Try with invalid token
- [ ] Verify 401 response
- [ ] Try with expired token
- [ ] Verify 401 response

**Test 10.2: Authorization**
- [ ] Login as Tenant 1 user
- [ ] Get Tenant 2's tender ID
- [ ] Try to access: `GET /api/v1/tenders/{tenant2_tender_id}`
- [ ] Verify 404 Not Found (not 403, to prevent info leakage)

**Test 10.3: Input Validation**
- [ ] Try to create tender with XSS: `<script>alert('xss')</script>`
- [ ] Verify input sanitized
- [ ] Try SQL injection in search: `' OR 1=1 --`
- [ ] Verify SQL injection prevented
- [ ] Upload .exe file as document
- [ ] Verify MIME type validation rejects

**Test 10.4: File Upload Security**
- [ ] Try uploading 100MB file
- [ ] Verify size limit enforced
- [ ] Try path traversal: `../../../etc/passwd`
- [ ] Verify path sanitization

**Expected Results:**
✅ All unauthenticated requests rejected
✅ Cross-tenant access prevented
✅ XSS attempts sanitized
✅ SQL injection prevented
✅ File upload security working

---

## Automated Test Suite

Create test automation script:

```python
# test_tender_automation.py

import pytest
import requests
from uuid import uuid4

class TestTenderAutomation:
    BASE_URL = "http://localhost:8000/api/v1"

    def setup_method(self):
        # Login and get token
        response = requests.post(
            "http://localhost:5000/api/auth/login",
            json={"email": "test@tendercompany.com", "password": "Test@123456"}
        )
        self.token = response.json()["token"]
        self.headers = {"Authorization": f"Bearer {self.token}"}

    def test_create_tender(self):
        response = requests.post(
            f"{self.BASE_URL}/tenders",
            headers=self.headers,
            json={
                "title": "Test Tender",
                "status": "draft"
            }
        )
        assert response.status_code == 201
        data = response.json()
        assert data["title"] == "Test Tender"
        assert data["status"] == "draft"
        return data["id"]

    def test_list_tenders(self):
        response = requests.get(
            f"{self.BASE_URL}/tenders",
            headers=self.headers
        )
        assert response.status_code == 200
        data = response.json()
        assert "items" in data
        assert "total" in data

    def test_ai_generation(self):
        response = requests.post(
            f"{self.BASE_URL}/ai/generate",
            headers=self.headers,
            json={
                "generation_type": "covering_letter",
                "context": {
                    "tender_title": "Test Tender",
                    "company_name": "Test Company",
                    "issuing_authority": "Test Authority"
                },
                "model": "claude-sonnet-4"
            }
        )
        assert response.status_code == 201
        data = response.json()
        assert "content" in data
        assert len(data["content"]) > 100
        assert data["tokens_used"] > 0

    # Add more tests...

if __name__ == "__main__":
    pytest.main([__file__, "-v"])
```

**Run Tests:**
```bash
pytest test_tender_automation.py -v
```

---

## Test Results Documentation

### Test Summary Template

```
Test Date: [DATE]
Tester: [NAME]
Environment: [Dev/Staging/Production]
Version: 1.0.0-MVP

Phase 1: Authentication & Basic Access
- Test 1.1: User Login ........................... ✅ PASS
- Test 1.2: Token Validation ..................... ✅ PASS

Phase 2: Document Management
- Test 2.1: Document Upload ...................... ✅ PASS
- Test 2.2: Document Search ...................... ✅ PASS
- Test 2.3: Document Download .................... ✅ PASS

[... continue for all phases ...]

Critical Issues Found: 0
Non-Critical Issues Found: 0
Test Coverage: 100%
Overall Status: ✅ PASS

Recommendation: APPROVED FOR DEPLOYMENT
```

---

## Known Issues & Workarounds

### Issue 1: [If any issues found]
**Description:** [Description]
**Severity:** High/Medium/Low
**Workaround:** [Temporary fix]
**Permanent Fix:** [Planned fix]
**Status:** Open/In Progress/Resolved

---

## Post-Test Actions

1. **Document Test Results**
   - [ ] Fill in test summary
   - [ ] Screenshot key features
   - [ ] Note any issues found

2. **Performance Baseline**
   - [ ] Record API response times
   - [ ] Document concurrent user capacity
   - [ ] Note resource usage

3. **User Acceptance**
   - [ ] Demo to stakeholders
   - [ ] Collect feedback
   - [ ] Prioritize enhancements

4. **Deployment Prep**
   - [ ] Create production checklist
   - [ ] Prepare rollback plan
   - [ ] Document deployment steps

---

## Sign-Off

**Tested By:** _________________
**Date:** _________________
**Status:** ✅ APPROVED / ⚠️ ISSUES FOUND / ❌ FAILED
**Notes:** _________________

---

## Next Steps After Successful Testing

1. **Beta Testing**
   - Deploy to staging environment
   - Invite 5 beta users
   - Monitor usage for 1 week
   - Collect feedback

2. **Production Deployment**
   - Set up production infrastructure
   - Run final security scan
   - Deploy to production
   - Monitor for 48 hours

3. **User Onboarding**
   - Create user documentation
   - Record video tutorials
   - Schedule training sessions
   - Provide support channel

---

**Testing Complete!** ✅
