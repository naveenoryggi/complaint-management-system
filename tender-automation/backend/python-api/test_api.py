"""Comprehensive API test suite for tender automation backend."""
import httpx
import json
from jose import jwt
from datetime import datetime, timedelta, timezone
import uuid

# Generate token
secret = 'development-secret-key-min-32-chars-long-for-jwt-token-signing'
user_id = str(uuid.uuid4())
tenant_id = str(uuid.uuid4())
payload = {
    'sub': user_id,
    'tenant_id': tenant_id,
    'email': 'admin@oryggi.com',
    'role': 'Admin',
    'iss': 'ComplaintManagementSystem',
    'aud': 'ComplaintManagementAPI',
    'exp': datetime.now(timezone.utc) + timedelta(hours=24),
    'iat': datetime.now(timezone.utc),
}
token = jwt.encode(payload, secret, algorithm='HS256')
headers = {'Authorization': f'Bearer {token}', 'Content-Type': 'application/json'}
base = 'http://localhost:8000/api/v1'

passed = 0
failed = 0


def test(name, method, url, json_data=None, expect=None):
    global passed, failed
    try:
        if method == 'POST':
            r = httpx.post(url, headers=headers, json=json_data, timeout=10)
        elif method == 'GET':
            r = httpx.get(url, headers=headers, timeout=10)
        elif method == 'PUT':
            r = httpx.put(url, headers=headers, json=json_data, timeout=10)
        elif method == 'DELETE':
            r = httpx.delete(url, headers=headers, timeout=10)

        status_ok = r.status_code == expect if expect else r.status_code < 400
        icon = 'PASS' if status_ok else 'FAIL'
        if not status_ok:
            failed += 1
            detail = r.text[:200] if r.status_code >= 400 else ''
            print(f'  [{icon}] {name}: {r.status_code} (expected {expect}) {detail}')
        else:
            passed += 1
            print(f'  [{icon}] {name}: {r.status_code}')
        return r
    except Exception as e:
        failed += 1
        print(f'  [FAIL] {name}: {e}')
        return None


print('=' * 60)
print('COMPREHENSIVE API TEST SUITE')
print('=' * 60)

# --- TENDERS ---
print('\n--- TENDERS ---')
r = test('Create Tender', 'POST', f'{base}/tenders/', {
    'title': 'GeM Supply - IT Equipment',
    'reference_number': 'GEM/2025/B/001',
    'issuing_authority': 'Ministry of Defence',
    'portal_name': 'GeM',
    'status': 'draft'
}, 201)
tender_id = r.json()['id'] if r and r.status_code == 201 else None

test('List Tenders', 'GET', f'{base}/tenders/', expect=200)

if tender_id:
    test('Get Tender', 'GET', f'{base}/tenders/{tender_id}', expect=200)
    test('Update Tender', 'PUT', f'{base}/tenders/{tender_id}', {
        'status': 'in_progress', 'estimated_value': 5000000.00
    }, 200)
    test('Upcoming Tenders', 'GET', f'{base}/tenders/upcoming', expect=200)

# --- COMPANY PROFILE ---
print('\n--- COMPANY PROFILE ---')
r = test('Create Company Profile', 'POST', f'{base}/company/profile', {
    'company_name': 'Oryggi Technologies Pvt Ltd',
    'cin_number': 'U72200KA2015PTC123456',
    'pan_number': 'AABCO1234A',
    'gstin': '29AABCO1234A1ZP',
    'year_established': 2015,
    'employee_count': 150
}, 201)
company_id = r.json()['id'] if r and r.status_code == 201 else None

test('Get Company Profile', 'GET', f'{base}/company/profile', expect=200)

if company_id:
    test('Update Company Profile', 'PUT', f'{base}/company/profile', {
        'employee_count': 175, 'website': 'https://oryggi.com'
    }, 200)

# --- CERTIFICATIONS ---
print('\n--- CERTIFICATIONS ---')
r = test('Create Certification', 'POST', f'{base}/company/certifications', {
    'name': 'ISO 9001:2015',
    'cert_type': 'iso',
    'issuing_body': 'TUV SUD',
    'certificate_number': 'ISO-2024-001'
}, 201)
cert_id = r.json()['id'] if r and r.status_code == 201 else None
test('List Certifications', 'GET', f'{base}/company/certifications', expect=200)

# --- PERSONNEL ---
print('\n--- PERSONNEL ---')
r = test('Create Personnel', 'POST', f'{base}/company/personnel', {
    'name': 'Rajesh Kumar',
    'designation': 'CTO',
    'role_in_tender': 'project_manager',
    'qualification': 'B.Tech Computer Science',
    'experience_years': 15
}, 201)
person_id = r.json()['id'] if r and r.status_code == 201 else None
test('List Personnel', 'GET', f'{base}/company/personnel', expect=200)

# --- REFERENCE BUNDLES ---
print('\n--- REFERENCE BUNDLES ---')
r = test('Create Bundle', 'POST', f'{base}/bundles/', {
    'bundle_name': 'BSNL Network Equipment Supply',
    'client_name': 'Bharat Sanchar Nigam Limited',
    'client_short_name': 'BSNL',
    'client_type': 'psu',
    'contract_value': 25000000,
    'status': 'completed'
}, 201)
bundle_id = r.json()['id'] if r and r.status_code == 201 else None
test('List Bundles', 'GET', f'{base}/bundles/', expect=200)
if bundle_id:
    test('Get Bundle', 'GET', f'{base}/bundles/{bundle_id}', expect=200)

# --- OEM ---
print('\n--- OEM MASTER ---')
r = test('Create OEM', 'POST', f'{base}/tracking/oem', {
    'name': 'Cisco Systems',
    'country': 'USA',
    'is_indian': False,
    'india_distributor': 'Ingram Micro',
    'partner_tier': 'Gold',
    'product_categories': ['Networking', 'Security']
}, 201)
oem_id = r.json()['id'] if r and r.status_code == 201 else None
test('List OEMs', 'GET', f'{base}/tracking/oem', expect=200)

# --- PORTAL REGISTRATIONS ---
print('\n--- PORTAL REGISTRATIONS ---')
r = test('Create Portal', 'POST', f'{base}/tracking/portals', {
    'portal_name': 'GeM Portal',
    'portal_url': 'https://gem.gov.in',
    'portal_type': 'central',
    'registration_status': 'active',
    'dsc_class': 'Class 3'
}, 201)
portal_id = r.json()['id'] if r and r.status_code == 201 else None
test('List Portals', 'GET', f'{base}/tracking/portals', expect=200)

# --- EMD ---
print('\n--- EMD RECORDS ---')
if tender_id:
    r = test('Create EMD', 'POST', f'{base}/tracking/emd/tender/{tender_id}', {
        'amount': 500000,
        'mode': 'bg',
        'instrument_number': 'BG-2025-001',
        'issuing_bank': 'State Bank of India',
        'status': 'submitted'
    }, 201)
    test('List EMDs', 'GET', f'{base}/tracking/emd', expect=200)

# --- TENDER FEES ---
print('\n--- TENDER FEES ---')
if tender_id:
    r = test('Create Fee', 'POST', f'{base}/tracking/fees/tender/{tender_id}', {
        'fee_type': 'tender_fee',
        'amount': 10000,
        'payment_mode': 'online',
        'status': 'paid'
    }, 201)
    test('List Fees', 'GET', f'{base}/tracking/fees', expect=200)

# --- DASHBOARD ---
print('\n--- DASHBOARD ---')
r = test('Dashboard Summary', 'GET', f'{base}/tracking/dashboard', expect=200)
if r and r.status_code == 200:
    print(f'    Dashboard: {json.dumps(r.json(), indent=2)}')

# --- CLEANUP ---
print('\n--- CLEANUP ---')
if tender_id:
    test('Delete Tender (cascade)', 'DELETE', f'{base}/tenders/{tender_id}', expect=204)
if cert_id:
    test('Delete Certification', 'DELETE', f'{base}/company/certifications/{cert_id}', expect=204)
if person_id:
    test('Delete Personnel', 'DELETE', f'{base}/company/personnel/{person_id}', expect=204)

print()
print('=' * 60)
print(f'RESULTS: {passed} PASSED, {failed} FAILED out of {passed + failed} tests')
print('=' * 60)
