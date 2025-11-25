# Step 3: SMTP Account Selection - Implementation Checklist

## Overview
This document provides a detailed checklist for implementing the missing "Step 3: SMTP Account Selection" feature in the Email Ticketing Configuration wizard.

---

## Current State vs Target State

### Current Wizard (5 Steps)
1. Select Your Email Provider
2. Enter Your Email Address
3. Configure OAuth Application (for IMAP)
4. Configure Additional Settings
5. Authorize Email Access

### Target Wizard (6 Steps)
1. Select Your Email Provider
2. Enter Your Email Address
3. **SMTP Account Selection (NEW)**
4. Configure OAuth Application (for IMAP + potentially SMTP)
5. Configure Additional Settings
6. Authorize Email Access

---

## Database Schema Changes

### Add to EmailConfiguration Table/Model

```typescript
// New fields to add to EmailConfiguration model
interface EmailConfiguration {
  // ... existing fields ...

  // NEW: SMTP Account Configuration
  usesSeparateSmtp: boolean;            // Default: false
  smtpFromEmail?: string;               // If usesSeparateSmtp = true
  smtpFromName?: string;                // If usesSeparateSmtp = true
  smtpAuthType?: 'oauth' | 'basic';     // If usesSeparateSmtp = true

  // OAuth fields for separate SMTP (if smtpAuthType = 'oauth')
  smtpOAuthClientId?: string;
  smtpOAuthTenantId?: string;
  smtpOAuthClientSecret?: string;       // Encrypt in database
  smtpOAuthScopes?: string;             // e.g., "https://outlook.office365.com/SMTP.Send"
  smtpOAuthAccessToken?: string;        // Encrypt in database
  smtpOAuthRefreshToken?: string;       // Encrypt in database
  smtpOAuthTokenExpiry?: Date;

  // Basic Auth fields for separate SMTP (if smtpAuthType = 'basic')
  smtpUsername?: string;
  smtpPassword?: string;                // Encrypt in database

  // SMTP Server Details (derived from provider or manual)
  smtpHost?: string;                    // e.g., "smtp.office365.com"
  smtpPort?: number;                    // e.g., 587
  smtpUseSsl?: boolean;                 // Default: true
}
```

---

## Frontend Implementation Checklist

### Component Changes

- [ ] **Update `email-ticketing-config.component.ts`**
  - [ ] Add step 3 to wizard steps array
  - [ ] Update step numbers (current step 3 becomes step 4, etc.)
  - [ ] Add form group for SMTP account selection

### Step 3 UI Implementation

#### File: `email-ticketing-config.component.html`

```html
<!-- NEW STEP 3: SMTP Account Selection -->
<div class="wizard-step" *ngIf="currentStep === 3">
  <div class="step-number">3</div>
  <div class="step-content">
    <h4>Choose SMTP Sending Account</h4>
    <p>Select how you want to send outgoing emails (auto-acknowledgements and notifications)</p>

    <!-- Radio Button Options -->
    <div class="smtp-account-options">
      <!-- Option 1: Use Same Account -->
      <div class="smtp-option"
           [class.selected]="!emailForm.get('usesSeparateSmtp')?.value"
           (click)="emailForm.get('usesSeparateSmtp')?.setValue(false)">
        <input type="radio"
               name="smtpAccountType"
               [value]="false"
               [checked]="!emailForm.get('usesSeparateSmtp')?.value">
        <div class="option-content">
          <div class="option-header">
            <i class="icon-check-circle"></i>
            <strong>Use Same Account (Recommended)</strong>
            <span class="badge badge-success">Simple</span>
          </div>
          <p>Use {{ emailForm.get('emailAddress')?.value || 'your email' }} for both receiving and sending</p>
          <ul class="benefits-list">
            <li><i class="icon-check"></i> Easier to set up</li>
            <li><i class="icon-check"></i> Single OAuth authorization</li>
            <li><i class="icon-check"></i> Recommended for most users</li>
          </ul>
        </div>
      </div>

      <!-- Option 2: Use Separate SMTP Account -->
      <div class="smtp-option"
           [class.selected]="emailForm.get('usesSeparateSmtp')?.value"
           (click)="emailForm.get('usesSeparateSmtp')?.setValue(true)">
        <input type="radio"
               name="smtpAccountType"
               [value]="true"
               [checked]="emailForm.get('usesSeparateSmtp')?.value">
        <div class="option-content">
          <div class="option-header">
            <i class="icon-settings"></i>
            <strong>Use Separate Sending Account (Advanced)</strong>
            <span class="badge badge-warning">Advanced</span>
          </div>
          <p>Use a different email address for sending (e.g., noreply@company.com)</p>
          <ul class="benefits-list">
            <li><i class="icon-check"></i> Professional no-reply setup</li>
            <li><i class="icon-check"></i> Different sender branding</li>
            <li><i class="icon-check"></i> Enhanced security separation</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Separate SMTP Configuration (Shown only if usesSeparateSmtp = true) -->
    <div class="separate-smtp-config" *ngIf="emailForm.get('usesSeparateSmtp')?.value">

      <!-- Use Case Examples -->
      <div class="use-case-examples">
        <h5><i class="icon-lightbulb"></i> Common Use Cases:</h5>
        <div class="example">
          <strong>No-Reply Setup:</strong>
          <div class="example-detail">
            <span>Receive: {{ emailForm.get('emailAddress')?.value || 'support@company.com' }}</span>
            <span>Send: noreply@company.com</span>
          </div>
        </div>
        <div class="example">
          <strong>Different Branding:</strong>
          <div class="example-detail">
            <span>Receive: support@company.com</span>
            <span>Send: info@company.com</span>
          </div>
        </div>
        <div class="example">
          <strong>Security Separation:</strong>
          <div class="example-detail">
            <span>Receive: tickets@company.com (read-only)</span>
            <span>Send: notifications@company.com (send-only)</span>
          </div>
        </div>
      </div>

      <!-- SMTP Email Configuration -->
      <div class="form-group">
        <label>
          <i class="icon-mail"></i>
          SMTP From Email Address *
        </label>
        <input type="email"
               formControlName="smtpFromEmail"
               placeholder="noreply@company.com"
               class="form-control">
        <small>The email address that will appear as sender in outgoing emails</small>
      </div>

      <div class="form-group">
        <label>
          <i class="icon-user"></i>
          SMTP From Name *
        </label>
        <input type="text"
               formControlName="smtpFromName"
               placeholder="No Reply - Support System"
               class="form-control">
        <small>The display name that will appear in outgoing emails</small>
      </div>

      <!-- Authentication Type Selection -->
      <div class="form-group">
        <label>
          <i class="icon-lock"></i>
          SMTP Authentication Type *
        </label>
        <div class="auth-type-selector">
          <!-- OAuth 2.0 Option -->
          <div class="auth-option"
               [class.selected]="emailForm.get('smtpAuthType')?.value === 'oauth'"
               (click)="emailForm.get('smtpAuthType')?.setValue('oauth')">
            <input type="radio"
                   name="smtpAuthType"
                   value="oauth"
                   [checked]="emailForm.get('smtpAuthType')?.value === 'oauth'">
            <div>
              <strong>OAuth 2.0 (Recommended)</strong>
              <span class="badge badge-success">Secure</span>
              <p>Modern, secure authentication for Office 365, Gmail</p>
            </div>
          </div>

          <!-- Basic Auth Option -->
          <div class="auth-option"
               [class.selected]="emailForm.get('smtpAuthType')?.value === 'basic'"
               (click)="emailForm.get('smtpAuthType')?.setValue('basic')">
            <input type="radio"
                   name="smtpAuthType"
                   value="basic"
                   [checked]="emailForm.get('smtpAuthType')?.value === 'basic'">
            <div>
              <strong>Basic Authentication (Legacy)</strong>
              <span class="badge badge-warning">Deprecated</span>
              <p>Traditional username/password (for self-hosted servers only)</p>
            </div>
          </div>
        </div>
      </div>

      <!-- OAuth 2.0 Fields (if smtpAuthType === 'oauth') -->
      <div class="oauth-fields" *ngIf="emailForm.get('smtpAuthType')?.value === 'oauth'">
        <div class="alert alert-info">
          <i class="icon-info"></i>
          You'll need to create a SEPARATE Azure AD app registration for this SMTP account.
          Follow the same steps as for the IMAP account, but use the SMTP email address.
        </div>

        <div class="form-group">
          <label>
            <i class="icon-key"></i>
            SMTP OAuth Client ID *
          </label>
          <input type="text"
                 formControlName="smtpOAuthClientId"
                 placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                 class="form-control">
          <small>Application (Client) ID from Azure AD app registration for SMTP account</small>
        </div>

        <div class="form-group">
          <label>
            <i class="icon-building"></i>
            SMTP OAuth Tenant ID *
          </label>
          <input type="text"
                 formControlName="smtpOAuthTenantId"
                 placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                 class="form-control">
          <small>Directory (Tenant) ID from Azure AD</small>
        </div>

        <div class="form-group">
          <label>
            <i class="icon-shield"></i>
            SMTP OAuth Client Secret *
          </label>
          <input type="password"
                 formControlName="smtpOAuthClientSecret"
                 placeholder="Enter the secret value"
                 class="form-control">
          <small class="text-warning">
            <i class="icon-alert"></i>
            Copy this immediately from Azure - it won't be shown again!
          </small>
        </div>
      </div>

      <!-- Basic Auth Fields (if smtpAuthType === 'basic') -->
      <div class="basic-auth-fields" *ngIf="emailForm.get('smtpAuthType')?.value === 'basic'">
        <div class="alert alert-warning">
          <i class="icon-alert"></i>
          <strong>Warning:</strong> Most modern email providers (Gmail, Office 365) have disabled basic authentication.
          Only use this for self-hosted SMTP servers.
        </div>

        <div class="form-group">
          <label>
            <i class="icon-user"></i>
            SMTP Username *
          </label>
          <input type="text"
                 formControlName="smtpUsername"
                 placeholder="smtp-user@company.com"
                 class="form-control">
          <small>SMTP server username (usually an email address)</small>
        </div>

        <div class="form-group">
          <label>
            <i class="icon-lock"></i>
            SMTP Password *
          </label>
          <input type="password"
                 formControlName="smtpPassword"
                 placeholder="Enter SMTP password"
                 class="form-control">
          <small>SMTP server password or app-specific password</small>
        </div>
      </div>
    </div>

    <!-- Navigation Buttons -->
    <div class="wizard-navigation">
      <button class="btn btn-secondary" (click)="previousStep()">
        <i class="icon-arrow-left"></i> Back
      </button>
      <button class="btn btn-primary"
              (click)="nextStep()"
              [disabled]="!isStep3Valid()">
        Next: Configure OAuth <i class="icon-arrow-right"></i>
      </button>
    </div>
  </div>
</div>
```

---

## Validation Logic

### TypeScript Validation

```typescript
// In email-ticketing-config.component.ts

isStep3Valid(): boolean {
  const usesSeparateSmtp = this.emailForm.get('usesSeparateSmtp')?.value;

  if (!usesSeparateSmtp) {
    // Same account - no additional validation needed
    return true;
  }

  // Separate SMTP account - validate required fields
  const smtpFromEmail = this.emailForm.get('smtpFromEmail')?.value;
  const smtpFromName = this.emailForm.get('smtpFromName')?.value;
  const smtpAuthType = this.emailForm.get('smtpAuthType')?.value;

  if (!smtpFromEmail || !smtpFromName || !smtpAuthType) {
    return false;
  }

  if (smtpAuthType === 'oauth') {
    const clientId = this.emailForm.get('smtpOAuthClientId')?.value;
    const tenantId = this.emailForm.get('smtpOAuthTenantId')?.value;
    const clientSecret = this.emailForm.get('smtpOAuthClientSecret')?.value;
    return !!(clientId && tenantId && clientSecret);
  }

  if (smtpAuthType === 'basic') {
    const username = this.emailForm.get('smtpUsername')?.value;
    const password = this.emailForm.get('smtpPassword')?.value;
    return !!(username && password);
  }

  return false;
}
```

---

## Backend Implementation Checklist

### API Endpoint Updates

- [ ] **Update `POST /api/email-configuration`**
  - [ ] Accept new SMTP account fields in request body
  - [ ] Validate SMTP fields if `usesSeparateSmtp = true`
  - [ ] Store encrypted SMTP credentials

- [ ] **Update `PUT /api/email-configuration/:id`**
  - [ ] Accept SMTP account updates
  - [ ] Handle OAuth token refresh for separate SMTP account

- [ ] **Add `POST /api/email-configuration/:id/test-smtp`**
  - [ ] Test SMTP connection with separate account
  - [ ] Send test email from SMTP account

### Service Layer Updates

- [ ] **EmailConfigurationService**
  - [ ] Add method to validate SMTP OAuth credentials
  - [ ] Add method to refresh SMTP OAuth token
  - [ ] Add method to get SMTP transport based on configuration

### OAuth Flow Updates

- [ ] **For Separate SMTP with OAuth:**
  - [ ] Implement second OAuth authorization flow
  - [ ] Store separate OAuth tokens for SMTP account
  - [ ] Implement token refresh for SMTP account
  - [ ] Handle token expiry for SMTP account

### Email Sending Logic Updates

- [ ] **Update Email Sending Service:**
  ```typescript
  async sendEmail(configId: string, emailOptions: EmailOptions) {
    const config = await this.getEmailConfiguration(configId);

    let smtpTransport;

    if (config.usesSeparateSmtp) {
      // Use separate SMTP account
      if (config.smtpAuthType === 'oauth') {
        smtpTransport = await this.getOAuthSmtpTransport(config);
      } else {
        smtpTransport = await this.getBasicSmtpTransport(config);
      }
    } else {
      // Use same account for IMAP and SMTP
      smtpTransport = await this.getDefaultSmtpTransport(config);
    }

    await smtpTransport.sendMail({
      from: config.usesSeparateSmtp
        ? `"${config.smtpFromName}" <${config.smtpFromEmail}>`
        : `"${config.fromName}" <${config.emailAddress}>`,
      ...emailOptions
    });
  }
  ```

---

## Testing Checklist

### Unit Tests

- [ ] Test form validation for Step 3
- [ ] Test radio button selection (same vs separate account)
- [ ] Test OAuth fields show/hide based on auth type
- [ ] Test Basic Auth fields show/hide based on auth type
- [ ] Test step navigation with validation

### Integration Tests

- [ ] Test creating email configuration with same account
- [ ] Test creating email configuration with separate SMTP (OAuth)
- [ ] Test creating email configuration with separate SMTP (Basic Auth)
- [ ] Test updating SMTP account configuration
- [ ] Test sending email from separate SMTP account

### End-to-End Tests

- [ ] Complete wizard flow with same account
- [ ] Complete wizard flow with separate SMTP (OAuth)
- [ ] Complete wizard flow with separate SMTP (Basic Auth)
- [ ] Test OAuth authorization for separate SMTP account
- [ ] Test email delivery from separate SMTP account
- [ ] Test auto-acknowledgement uses correct sender

---

## Security Considerations

- [ ] **Encrypt Sensitive Data:**
  - [ ] SMTP OAuth Client Secret
  - [ ] SMTP OAuth Access Token
  - [ ] SMTP OAuth Refresh Token
  - [ ] SMTP Basic Auth Password

- [ ] **Validate Email Addresses:**
  - [ ] Ensure SMTP email is valid format
  - [ ] Prevent email spoofing

- [ ] **OAuth Token Security:**
  - [ ] Store tokens encrypted in database
  - [ ] Implement secure token refresh mechanism
  - [ ] Handle token expiry gracefully

---

## Migration Script

For existing email configurations without separate SMTP settings:

```sql
-- Migration: Add SMTP account columns to EmailConfiguration table
ALTER TABLE EmailConfiguration
ADD COLUMN usesSeparateSmtp BOOLEAN DEFAULT FALSE,
ADD COLUMN smtpFromEmail VARCHAR(255),
ADD COLUMN smtpFromName VARCHAR(255),
ADD COLUMN smtpAuthType VARCHAR(20),
ADD COLUMN smtpOAuthClientId VARCHAR(255),
ADD COLUMN smtpOAuthTenantId VARCHAR(255),
ADD COLUMN smtpOAuthClientSecret TEXT,
ADD COLUMN smtpOAuthScopes TEXT,
ADD COLUMN smtpOAuthAccessToken TEXT,
ADD COLUMN smtpOAuthRefreshToken TEXT,
ADD COLUMN smtpOAuthTokenExpiry TIMESTAMP,
ADD COLUMN smtpUsername VARCHAR(255),
ADD COLUMN smtpPassword TEXT,
ADD COLUMN smtpHost VARCHAR(255),
ADD COLUMN smtpPort INT,
ADD COLUMN smtpUseSsl BOOLEAN DEFAULT TRUE;

-- Set existing configurations to use same account (default behavior)
UPDATE EmailConfiguration
SET usesSeparateSmtp = FALSE
WHERE usesSeparateSmtp IS NULL;
```

---

## Documentation Updates

- [ ] **User Guide:**
  - [ ] Document when to use same account vs separate SMTP
  - [ ] Provide examples of no-reply setup
  - [ ] Explain OAuth vs Basic Auth for SMTP

- [ ] **Developer Guide:**
  - [ ] Document new database schema
  - [ ] Document API endpoint changes
  - [ ] Provide code examples for email sending

- [ ] **Admin Guide:**
  - [ ] How to configure separate SMTP accounts
  - [ ] How to troubleshoot SMTP authentication issues
  - [ ] How to manage OAuth tokens

---

## Definition of Done

- [ ] Step 3 UI implemented in wizard
- [ ] Form validation working correctly
- [ ] Database schema updated with migration
- [ ] API endpoints handle new fields
- [ ] Email sending uses correct SMTP account
- [ ] OAuth flow works for separate SMTP account
- [ ] Basic Auth works for separate SMTP account
- [ ] All automated tests passing
- [ ] Manual testing completed
- [ ] Documentation updated
- [ ] Code reviewed and approved
- [ ] Deployed to staging environment
- [ ] QA testing passed
- [ ] Ready for production deployment

---

## Estimated Effort

| Task | Estimated Time |
|------|----------------|
| Frontend UI Development | 4-6 hours |
| Backend API Updates | 4-6 hours |
| OAuth Flow Implementation | 4-6 hours |
| Database Migration | 1-2 hours |
| Unit Tests | 3-4 hours |
| Integration Tests | 3-4 hours |
| End-to-End Tests | 2-3 hours |
| Documentation | 2-3 hours |
| Code Review & QA | 2-3 hours |
| **TOTAL** | **25-37 hours** |

Recommended Sprint: **2 sprints** (considering complexity of OAuth flow)

---

## Priority: CRITICAL

This feature is blocking the following use cases:
- No-reply email setups
- Separate branding for inbound vs outbound emails
- Security separation of receiving and sending accounts

**Recommendation:** Implement in the next sprint before production deployment.

---

**Document Version:** 1.0
**Created:** 2025-11-17
**Status:** Ready for Implementation
