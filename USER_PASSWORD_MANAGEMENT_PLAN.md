# User Password Management System - Implementation Plan
## Microsoft Teams-Style Password Management

**Date:** November 9, 2025
**Feature:** Complete User Password Management System
**Modeled After:** Microsoft Teams User Management

---

## 📋 Table of Contents

1. [Current System Analysis](#1-current-system-analysis)
2. [Microsoft Teams Patterns](#2-microsoft-teams-patterns)
3. [Proposed Features](#3-proposed-features)
4. [Database Schema Design](#4-database-schema-design)
5. [Backend Implementation Plan](#5-backend-implementation-plan)
6. [Frontend Implementation Plan](#6-frontend-implementation-plan)
7. [Security Considerations](#7-security-considerations)
8. [Implementation Phases](#8-implementation-phases)
9. [Testing Strategy](#9-testing-strategy)
10. [Migration Strategy](#10-migration-strategy)

---

## 1. Current System Analysis

### 1.1 Existing Authentication System

**Current Implementation:**
```csharp
// User.cs (Domain Entity)
public class User : BaseEntity
{
    public string PasswordHash { get; set; }  // Stores encrypted password
    public DateTime? LastLoginAt { get; set; } // Last login timestamp

    // Login supports:
    - Email
    - Employee Code
    - Phone Number
}
```

**Current Login Flow:**
1. User provides identifier (email/employee code/phone) + password
2. System retrieves user by identifier
3. Password verified using `IEncryptionService.VerifyPassword()`
4. JWT token generated with refresh token
5. Last login timestamp updated

**Current Capabilities:**
- ✅ Password hashing with AES encryption
- ✅ Multi-factor login identifiers (email/employee code/phone)
- ✅ JWT authentication with refresh tokens
- ✅ Role-based authorization
- ✅ Active/inactive user status
- ❌ No password policies
- ❌ No password change requirement
- ❌ No password history
- ❌ No self-service password reset
- ❌ No admin password reset capability

### 1.2 Gaps to Address

1. **No Password Lifecycle Management**
   - No expiration policies
   - No complexity requirements
   - No password change enforcement

2. **No Admin Controls**
   - Admin cannot set/reset user passwords
   - No temporary password mechanism
   - No forced password change on first login

3. **No User Self-Service**
   - Users cannot change their own passwords
   - No password reset capability
   - No password strength validation

4. **No Security Policies**
   - No password history tracking
   - No lockout mechanism
   - No password expiration

---

## 2. Microsoft Teams Patterns

### 2.1 Microsoft Teams Password Management Features

**Admin Capabilities:**
1. ✅ Set initial password for new users
2. ✅ Reset password for existing users
3. ✅ Configure password policy:
   - Require password change on first sign-in
   - Allow user to choose whether to change password
   - Set password to never expire (per user)
4. ✅ Auto-generate secure passwords
5. ✅ Send password via email (optional)
6. ✅ View password status (expired, must change, etc.)

**User Capabilities:**
1. ✅ Change password from profile settings
2. ✅ Must change password on first login (if configured)
3. ✅ Can skip password change (if allowed by admin)
4. ✅ Password strength validation real-time
5. ✅ Password history prevention

**System Features:**
1. ✅ Password complexity requirements
2. ✅ Password expiration policies
3. ✅ Account lockout after failed attempts
4. ✅ Password history (prevent reuse)
5. ✅ Audit logging of password changes

### 2.2 Microsoft Teams UI/UX Patterns

**Admin Experience:**
```
User Management > Edit User
┌──────────────────────────────────────────────┐
│ Password Settings                            │
├──────────────────────────────────────────────┤
│ ○ Auto-generate password                     │
│ ● Let me create a password                   │
│   ┌────────────────────────────────────────┐ │
│   │ Password: ******************          │ │
│   │ Show password ☐                       │ │
│   └────────────────────────────────────────┘ │
│                                              │
│ ☑ Require this user to change their         │
│   password when they first sign in          │
│                                              │
│ ☐ Send password in email on completion      │
│                                              │
│ Password never expires: ☐                   │
│                                              │
│ [Save] [Cancel]                              │
└──────────────────────────────────────────────┘
```

**User First Login Experience:**
```
┌──────────────────────────────────────────────┐
│ Update your password                         │
├──────────────────────────────────────────────┤
│ Your administrator requires that you change  │
│ your password before signing in.             │
│                                              │
│ Current password:                            │
│ ┌────────────────────────────────────────┐  │
│ │ ****                                   │  │
│ └────────────────────────────────────────┘  │
│                                              │
│ New password:                                │
│ ┌────────────────────────────────────────┐  │
│ │                                        │  │
│ └────────────────────────────────────────┘  │
│ Password strength: [====------] Medium      │
│                                              │
│ Confirm new password:                        │
│ ┌────────────────────────────────────────┐  │
│ │                                        │  │
│ └────────────────────────────────────────┘  │
│                                              │
│ [Change Password] [Sign in without changing]│
└──────────────────────────────────────────────┘
```

---

## 3. Proposed Features

### 3.1 Admin Features

**Priority 1 - Essential**
- [x] Set/Reset user password
- [x] Auto-generate secure password
- [x] Configure "must change password on first login"
- [x] Configure "allow user to skip password change"
- [x] View password status for users
- [x] Copy generated password to clipboard

**Priority 2 - Enhanced**
- [x] Bulk password reset
- [x] Send password via email
- [x] Password never expires (per user)
- [x] View password change history
- [x] Audit log of password operations

**Priority 3 - Advanced**
- [ ] Custom password policies per user group
- [ ] Scheduled password expiration warnings
- [ ] Integration with external password policy engines

### 3.2 User Features

**Priority 1 - Essential**
- [x] Change own password from profile
- [x] Forced password change on first login
- [x] Real-time password strength validation
- [x] Password requirements display
- [x] Skip password change (if allowed)

**Priority 2 - Enhanced**
- [x] Password history (prevent reuse of last 5 passwords)
- [x] Password expiration notification
- [x] See last password change date
- [x] Security questions (future)

**Priority 3 - Advanced**
- [ ] Self-service password reset via email
- [ ] Self-service password reset via SMS
- [ ] Two-factor authentication
- [ ] Biometric authentication options

### 3.3 System Features

**Priority 1 - Essential**
- [x] Password complexity validation
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 number
  - At least 1 special character
- [x] Password hashing with bcrypt/Argon2
- [x] Secure password storage
- [x] Audit logging

**Priority 2 - Enhanced**
- [x] Account lockout after 5 failed attempts
- [x] Lockout duration: 15 minutes
- [x] Password history tracking (5 passwords)
- [x] Password expiration (90 days default)
- [x] Email notifications

**Priority 3 - Advanced**
- [ ] Compromised password detection
- [ ] Integration with Have I Been Pwned API
- [ ] Multi-factor authentication
- [ ] Single Sign-On (SSO)

---

## 4. Database Schema Design

### 4.1 Enhanced User Table

**Modify existing `Users` table:**

```sql
ALTER TABLE Users ADD COLUMN PasswordExpiresAt DATETIME NULL;
ALTER TABLE Users ADD COLUMN MustChangePasswordOnNextLogin BIT DEFAULT 0;
ALTER TABLE Users ADD COLUMN PasswordNeverExpires BIT DEFAULT 0;
ALTER TABLE Users ADD COLUMN PasswordChangedAt DATETIME NULL;
ALTER TABLE Users ADD COLUMN PasswordChangedBy NVARCHAR(36) NULL; -- UserId of who changed it
ALTER TABLE Users ADD COLUMN FailedLoginAttempts INT DEFAULT 0;
ALTER TABLE Users ADD COLUMN AccountLockedUntil DATETIME NULL;
ALTER TABLE Users ADD COLUMN LastPasswordChangeRequiredNotificationSentAt DATETIME NULL;
```

**C# Entity Update:**
```csharp
public class User : BaseEntity
{
    // Existing properties...
    public string? PasswordHash { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // NEW PASSWORD MANAGEMENT PROPERTIES

    /// <summary>
    /// When the password expires (null if never expires or PasswordNeverExpires is true)
    /// </summary>
    public DateTime? PasswordExpiresAt { get; set; }

    /// <summary>
    /// User must change password on next successful login
    /// </summary>
    public bool MustChangePasswordOnNextLogin { get; set; } = false;

    /// <summary>
    /// Password never expires for this user (admin override)
    /// </summary>
    public bool PasswordNeverExpires { get; set; } = false;

    /// <summary>
    /// When the password was last changed
    /// </summary>
    public DateTime? PasswordChangedAt { get; set; }

    /// <summary>
    /// Who changed the password (User ID - for admin resets)
    /// </summary>
    public Guid? PasswordChangedBy { get; set; }

    /// <summary>
    /// Number of consecutive failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Account is locked until this time (null if not locked)
    /// </summary>
    public DateTime? AccountLockedUntil { get; set; }

    /// <summary>
    /// When the last password expiration warning was sent
    /// </summary>
    public DateTime? LastPasswordChangeRequiredNotificationSentAt { get; set; }

    // Navigation property
    public User? PasswordChanger { get; set; }
}
```

### 4.2 New Table: PasswordHistory

**Purpose:** Track password history to prevent reuse

```sql
CREATE TABLE PasswordHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NULL, -- Admin who set it, or user themselves

    CONSTRAINT FK_PasswordHistory_User FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PasswordHistory_CreatedBy FOREIGN KEY (CreatedBy)
        REFERENCES Users(Id)
);

CREATE INDEX IX_PasswordHistory_UserId ON PasswordHistory(UserId);
CREATE INDEX IX_PasswordHistory_CreatedAt ON PasswordHistory(CreatedAt DESC);
```

**C# Entity:**
```csharp
public class PasswordHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public User? Creator { get; set; }
}
```

### 4.3 New Table: PasswordAuditLog

**Purpose:** Audit trail of all password-related operations

```sql
CREATE TABLE PasswordAuditLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- PasswordSet, PasswordChanged, PasswordReset, etc.
    PerformedBy UNIQUEIDENTIFIER NULL, -- Who performed the action (null for self-service)
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    Success BIT NOT NULL,
    FailureReason NVARCHAR(500) NULL,
    AdditionalInfo NVARCHAR(MAX) NULL, -- JSON for extra details
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_PasswordAuditLog_User FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PasswordAuditLog_PerformedBy FOREIGN KEY (PerformedBy)
        REFERENCES Users(Id)
);

CREATE INDEX IX_PasswordAuditLog_UserId ON PasswordAuditLog(UserId);
CREATE INDEX IX_PasswordAuditLog_CreatedAt ON PasswordAuditLog(CreatedAt DESC);
CREATE INDEX IX_PasswordAuditLog_Action ON PasswordAuditLog(Action);
```

**C# Entity:**
```csharp
public class PasswordAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public PasswordAction Action { get; set; }
    public Guid? PerformedBy { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public User? Performer { get; set; }
}

public enum PasswordAction
{
    PasswordSet,          // Admin sets initial password
    PasswordChanged,      // User changes own password
    PasswordReset,        // Admin resets password
    PasswordExpired,      // Password expired automatically
    AccountLocked,        // Account locked due to failed attempts
    AccountUnlocked,      // Account unlocked by admin
    FailedLoginAttempt,   // Failed login recorded
    PasswordChangeRequired, // Flag set for password change
    PasswordChangeSkipped  // User skipped password change
}
```

### 4.4 New Table: PasswordPolicy

**Purpose:** Company-wide password policy configuration

```sql
CREATE TABLE PasswordPolicy (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,

    -- Complexity Requirements
    MinimumLength INT NOT NULL DEFAULT 8,
    RequireUppercase BIT NOT NULL DEFAULT 1,
    RequireLowercase BIT NOT NULL DEFAULT 1,
    RequireDigit BIT NOT NULL DEFAULT 1,
    RequireSpecialCharacter BIT NOT NULL DEFAULT 1,

    -- Expiration Settings
    PasswordExpirationDays INT NULL DEFAULT 90, -- NULL = never expires
    PasswordExpirationWarningDays INT NOT NULL DEFAULT 14,

    -- Lockout Settings
    MaxFailedLoginAttempts INT NOT NULL DEFAULT 5,
    LockoutDurationMinutes INT NOT NULL DEFAULT 15,

    -- History Settings
    PasswordHistoryCount INT NOT NULL DEFAULT 5,

    -- Other Settings
    AllowPasswordChangeSkip BIT NOT NULL DEFAULT 0,

    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_PasswordPolicy_Company FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_PasswordPolicy_CompanyId ON PasswordPolicy(CompanyId);
```

**C# Entity:**
```csharp
public class PasswordPolicy
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }

    // Complexity Requirements
    public int MinimumLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;

    // Expiration Settings
    public int? PasswordExpirationDays { get; set; } = 90;
    public int PasswordExpirationWarningDays { get; set; } = 14;

    // Lockout Settings
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;

    // History Settings
    public int PasswordHistoryCount { get; set; } = 5;

    // Other Settings
    public bool AllowPasswordChangeSkip { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Company Company { get; set; } = null!;
}
```

---

## 5. Backend Implementation Plan

### 5.1 Phase 1: Core Infrastructure (Week 1)

#### 5.1.1 Database Migration

**File:** `20251109_AddPasswordManagementTables.cs`

```csharp
public class AddPasswordManagementTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add columns to Users table
        migrationBuilder.AddColumn<DateTime?>(
            name: "PasswordExpiresAt",
            table: "Users",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "MustChangePasswordOnNextLogin",
            table: "Users",
            nullable: false,
            defaultValue: false);

        // ... (rest of User table modifications)

        // Create PasswordHistory table
        migrationBuilder.CreateTable(
            name: "PasswordHistory",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                PasswordHash = table.Column<string>(maxLength: 500, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                CreatedBy = table.Column<Guid>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_PasswordHistory_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // ... (rest of tables)
    }
}
```

#### 5.1.2 Domain Entities

**Files to Create:**
1. `Domain/Entities/Auth/PasswordHistory.cs`
2. `Domain/Entities/Auth/PasswordAuditLog.cs`
3. `Domain/Entities/Settings/PasswordPolicy.cs`
4. `Domain/Enums/PasswordAction.cs`

#### 5.1.3 Password Service Interface

**File:** `Application/Interfaces/Services/IPasswordService.cs`

```csharp
public interface IPasswordService
{
    // Password Validation
    Task<PasswordValidationResult> ValidatePasswordComplexityAsync(
        string password, Guid companyId, CancellationToken cancellationToken = default);

    // Password Generation
    string GenerateSecurePassword(int length = 12);

    // Password Hashing
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);

    // Password History
    Task<bool> IsPasswordInHistoryAsync(
        Guid userId, string password, CancellationToken cancellationToken = default);
    Task AddPasswordToHistoryAsync(
        Guid userId, string passwordHash, Guid? changedBy, CancellationToken cancellationToken = default);

    // Password Expiration
    Task<bool> IsPasswordExpiredAsync(
        Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetDaysUntilPasswordExpiresAsync(
        Guid userId, CancellationToken cancellationToken = default);

    // Account Lockout
    Task<bool> IsAccountLockedAsync(
        Guid userId, CancellationToken cancellationToken = default);
    Task IncrementFailedLoginAttemptAsync(
        Guid userId, CancellationToken cancellationToken = default);
    Task ResetFailedLoginAttemptsAsync(
        Guid userId, CancellationToken cancellationToken = default);
    Task UnlockAccountAsync(
        Guid userId, Guid unlockedBy, CancellationToken cancellationToken = default);

    // Password Operations
    Task<Result> SetUserPasswordAsync(
        Guid userId, string password, Guid setBy, bool mustChange,
        CancellationToken cancellationToken = default);
    Task<Result> ChangeUserPasswordAsync(
        Guid userId, string currentPassword, string newPassword,
        CancellationToken cancellationToken = default);
    Task<Result> ResetUserPasswordAsync(
        Guid userId, Guid resetBy, bool sendEmail,
        CancellationToken cancellationToken = default);
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public PasswordStrength Strength { get; set; }
}

public enum PasswordStrength
{
    VeryWeak,
    Weak,
    Medium,
    Strong,
    VeryStrong
}
```

#### 5.1.4 Password Service Implementation

**File:** `Infrastructure/Services/PasswordService.cs`

Key methods to implement:
- Password complexity validation
- Secure password generation
- Password hashing (switch from AES to bcrypt/Argon2)
- Password history checking
- Account lockout management
- Audit logging

### 5.2 Phase 2: API Endpoints (Week 1-2)

#### 5.2.1 Password Controller

**File:** `API/Controllers/PasswordController.cs`

**Endpoints:**

```csharp
// Admin Endpoints
[HttpPost("users/{userId}/set-password")]
[Authorize]
[HasPermission(PermissionType.ManageUsers)]
public async Task<IActionResult> SetUserPassword(
    Guid userId, [FromBody] SetPasswordRequest request);

[HttpPost("users/{userId}/reset-password")]
[Authorize]
[HasPermission(PermissionType.ManageUsers)]
public async Task<IActionResult> ResetUserPassword(
    Guid userId, [FromBody] ResetPasswordRequest request);

[HttpPost("users/{userId}/unlock-account")]
[Authorize]
[HasPermission(PermissionType.ManageUsers)]
public async Task<IActionResult> UnlockAccount(Guid userId);

[HttpGet("users/{userId}/password-status")]
[Authorize]
[HasPermission(PermissionType.ManageUsers)]
public async Task<IActionResult> GetPasswordStatus(Guid userId);

// User Endpoints
[HttpPost("change-password")]
[Authorize]
public async Task<IActionResult> ChangePassword(
    [FromBody] ChangePasswordRequest request);

[HttpPost("skip-password-change")]
[Authorize]
public async Task<IActionResult> SkipPasswordChange();

[HttpGet("password-requirements")]
public async Task<IActionResult> GetPasswordRequirements();

[HttpPost("validate-password-strength")]
public async Task<IActionResult> ValidatePasswordStrength(
    [FromBody] ValidatePasswordRequest request);

// Password Policy Endpoints (Admin)
[HttpGet("policy")]
[Authorize]
[HasPermission(PermissionType.ManageCompanySettings)]
public async Task<IActionResult> GetPasswordPolicy();

[HttpPut("policy")]
[Authorize]
[HasPermission(PermissionType.ManageCompanySettings)]
public async Task<IActionResult> UpdatePasswordPolicy(
    [FromBody] UpdatePasswordPolicyRequest request);
```

#### 5.2.2 DTOs

**Files to Create:**

1. **SetPasswordRequest.cs**
```csharp
public class SetPasswordRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;

    public bool MustChangeOnNextLogin { get; set; } = true;
    public bool SendPasswordViaEmail { get; set; } = false;
    public bool PasswordNeverExpires { get; set; } = false;
}
```

2. **ResetPasswordRequest.cs**
```csharp
public class ResetPasswordRequest
{
    public bool AutoGenerate { get; set; } = true;
    public string? CustomPassword { get; set; }
    public bool MustChangeOnNextLogin { get; set; } = true;
    public bool SendPasswordViaEmail { get; set; } = true;
}
```

3. **ChangePasswordRequest.cs**
```csharp
public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

4. **PasswordStatusDto.cs**
```csharp
public class PasswordStatusDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool MustChangeOnNextLogin { get; set; }
    public bool PasswordNeverExpires { get; set; }
    public DateTime? PasswordExpiresAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public int DaysUntilExpiration { get; set; }
    public bool IsExpired { get; set; }
    public bool IsAccountLocked { get; set; }
    public DateTime? AccountLockedUntil { get; set; }
    public int FailedLoginAttempts { get; set; }
}
```

### 5.3 Phase 3: Enhanced Login Flow (Week 2)

#### 5.3.1 Modified Login Handler

**File:** `Application/Features/Auth/Handlers/LoginCommandHandler.cs`

Add checks:
```csharp
public async Task<Result<LoginResponse>> Handle(LoginCommand request, ...)
{
    // ... existing user lookup

    // NEW: Check if account is locked
    if (await _passwordService.IsAccountLockedAsync(user.Id))
    {
        return Result<LoginResponse>.Failure(
            "Account is locked due to multiple failed login attempts",
            "AccountLocked");
    }

    // ... existing password verification

    if (!passwordValid)
    {
        // NEW: Increment failed login attempts
        await _passwordService.IncrementFailedLoginAttemptAsync(user.Id);
        return Result<LoginResponse>.Failure("Invalid credentials");
    }

    // NEW: Reset failed login attempts on successful login
    await _passwordService.ResetFailedLoginAttemptsAsync(user.Id);

    // NEW: Check if password is expired
    if (await _passwordService.IsPasswordExpiredAsync(user.Id))
    {
        user.MustChangePasswordOnNextLogin = true;
        await _unitOfWork.SaveChangesAsync();
    }

    // ... generate tokens

    // NEW: Include password change requirement in response
    response.MustChangePassword = user.MustChangePasswordOnNextLogin;
    response.CanSkipPasswordChange = policy.AllowPasswordChangeSkip;
    response.DaysUntilPasswordExpires =
        await _passwordService.GetDaysUntilPasswordExpiresAsync(user.Id);

    return Result<LoginResponse>.Success(response);
}
```

#### 5.3.2 Enhanced Login Response

```csharp
public class LoginResponse
{
    // Existing properties
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;

    // NEW PASSWORD PROPERTIES
    public bool MustChangePassword { get; set; }
    public bool CanSkipPasswordChange { get; set; }
    public int DaysUntilPasswordExpires { get; set; }
    public bool IsPasswordExpired { get; set; }
}
```

---

## 6. Frontend Implementation Plan

### 6.1 Phase 1: Change Password Dialog (Week 2)

#### 6.1.1 Component Files

**1. change-password-dialog.component.ts**
```typescript
@Component({
  selector: 'app-change-password-dialog',
  templateUrl: './change-password-dialog.component.html',
  styleUrls: ['./change-password-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MaterialModule]
})
export class ChangePasswordDialogComponent implements OnInit {
  changePasswordForm!: FormGroup;
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  passwordStrength: PasswordStrength = { level: 0, label: 'None' };
  passwordRequirements: PasswordRequirement[] = [];

  constructor(
    private fb: FormBuilder,
    private passwordService: PasswordService,
    private dialogRef: MatDialogRef<ChangePasswordDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mustChange: boolean, canSkip: boolean }
  ) {}

  ngOnInit(): void {
    this.loadPasswordRequirements();
    this.initializeForm();
  }

  private initializeForm(): void {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });

    // Real-time password strength calculation
    this.changePasswordForm.get('newPassword')?.valueChanges
      .pipe(debounceTime(300))
      .subscribe(password => {
        this.passwordStrength = this.calculatePasswordStrength(password);
        this.validatePasswordRequirements(password);
      });
  }

  onSubmit(): void {
    if (this.changePasswordForm.valid) {
      const request: ChangePasswordRequest = {
        currentPassword: this.changePasswordForm.value.currentPassword,
        newPassword: this.changePasswordForm.value.newPassword,
        confirmPassword: this.changePasswordForm.value.confirmPassword
      };

      this.passwordService.changePassword(request).subscribe({
        next: (response) => {
          this.dialogRef.close({ success: true });
        },
        error: (error) => {
          // Handle error
        }
      });
    }
  }

  onSkip(): void {
    if (this.data.canSkip) {
      this.passwordService.skipPasswordChange().subscribe({
        next: () => {
          this.dialogRef.close({ skipped: true });
        }
      });
    }
  }
}
```

**2. change-password-dialog.component.html**
```html
<h2 mat-dialog-title>
  {{ data.mustChange ? 'Update your password' : 'Change password' }}
</h2>

<mat-dialog-content>
  <div class="password-change-notice" *ngIf="data.mustChange">
    <mat-icon>warning</mat-icon>
    <p>Your administrator requires that you change your password before continuing.</p>
  </div>

  <form [formGroup]="changePasswordForm">
    <!-- Current Password -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Current password</mat-label>
      <input matInput
             [type]="hideCurrentPassword ? 'password' : 'text'"
             formControlName="currentPassword"
             autocomplete="current-password">
      <button mat-icon-button matSuffix
              (click)="hideCurrentPassword = !hideCurrentPassword"
              type="button">
        <mat-icon>{{hideCurrentPassword ? 'visibility_off' : 'visibility'}}</mat-icon>
      </button>
    </mat-form-field>

    <!-- New Password -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>New password</mat-label>
      <input matInput
             [type]="hideNewPassword ? 'password' : 'text'"
             formControlName="newPassword"
             autocomplete="new-password">
      <button mat-icon-button matSuffix
              (click)="hideNewPassword = !hideNewPassword"
              type="button">
        <mat-icon>{{hideNewPassword ? 'visibility_off' : 'visibility'}}</mat-icon>
      </button>
    </mat-form-field>

    <!-- Password Strength Indicator -->
    <div class="password-strength">
      <div class="strength-label">Password strength:</div>
      <div class="strength-bar">
        <div class="strength-fill"
             [ngClass]="'strength-' + passwordStrength.level"
             [style.width.%]="passwordStrength.level * 20">
        </div>
      </div>
      <div class="strength-text" [ngClass]="'strength-' + passwordStrength.level">
        {{ passwordStrength.label }}
      </div>
    </div>

    <!-- Password Requirements Checklist -->
    <div class="password-requirements">
      <div class="requirement-item"
           *ngFor="let req of passwordRequirements"
           [ngClass]="{'met': req.isMet}">
        <mat-icon>{{ req.isMet ? 'check_circle' : 'cancel' }}</mat-icon>
        <span>{{ req.description }}</span>
      </div>
    </div>

    <!-- Confirm Password -->
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Confirm new password</mat-label>
      <input matInput
             [type]="hideConfirmPassword ? 'password' : 'text'"
             formControlName="confirmPassword"
             autocomplete="new-password">
      <button mat-icon-button matSuffix
              (click)="hideConfirmPassword = !hideConfirmPassword"
              type="button">
        <mat-icon>{{hideConfirmPassword ? 'visibility_off' : 'visibility'}}</mat-icon>
      </button>
      <mat-error *ngIf="changePasswordForm.hasError('passwordMismatch')">
        Passwords do not match
      </mat-error>
    </mat-form-field>
  </form>
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button
          *ngIf="data.canSkip"
          (click)="onSkip()">
    Sign in without changing
  </button>
  <button mat-raised-button
          color="primary"
          (click)="onSubmit()"
          [disabled]="!changePasswordForm.valid">
    Change Password
  </button>
</mat-dialog-actions>
```

### 6.2 Phase 2: Admin Password Management UI (Week 3)

#### 6.2.1 User Management Enhancement

**Add to `user-management.component.ts`:**

```typescript
onSetPassword(user: User): void {
  const dialogRef = this.dialog.open(SetPasswordDialogComponent, {
    width: '500px',
    data: { user }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result?.success) {
      this.showSuccessMessage('Password set successfully');
      this.loadUsers();
    }
  });
}

onResetPassword(user: User): void {
  const dialogRef = this.dialog.open(ResetPasswordDialogComponent, {
    width: '500px',
    data: { user }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result?.success) {
      this.showSuccessMessage('Password reset successfully');
      if (result.newPassword) {
        this.showPasswordDialog(result.newPassword);
      }
    }
  });
}

onViewPasswordStatus(user: User): void {
  this.passwordService.getPasswordStatus(user.id).subscribe(status => {
    const dialogRef = this.dialog.open(PasswordStatusDialogComponent, {
      width: '600px',
      data: { user, status }
    });
  });
}

onUnlockAccount(user: User): void {
  this.confirmationService.confirm({
    title: 'Unlock Account',
    message: `Are you sure you want to unlock ${user.fullName}'s account?`,
    confirm: () => {
      this.passwordService.unlockAccount(user.id).subscribe({
        next: () => {
          this.showSuccessMessage('Account unlocked successfully');
          this.loadUsers();
        }
      });
    }
  });
}
```

#### 6.2.2 Set Password Dialog

**set-password-dialog.component.html:**
```html
<h2 mat-dialog-title>Set Password for {{ data.user.fullName }}</h2>

<mat-dialog-content>
  <form [formGroup]="passwordForm">
    <!-- Auto-generate or manual -->
    <mat-radio-group formControlName="mode" class="password-mode">
      <mat-radio-button value="auto">Auto-generate password</mat-radio-button>
      <mat-radio-button value="manual">Let me create a password</mat-radio-button>
    </mat-radio-group>

    <!-- Manual password input -->
    <div *ngIf="passwordForm.value.mode === 'manual'" class="manual-password">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Password</mat-label>
        <input matInput
               [type]="hidePassword ? 'password' : 'text'"
               formControlName="password">
        <button mat-icon-button matSuffix
                (click)="hidePassword = !hidePassword"
                type="button">
          <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
        </button>
      </mat-form-field>

      <mat-checkbox formControlName="showPassword">
        Show password
      </mat-checkbox>
    </div>

    <!-- Auto-generated password display -->
    <div *ngIf="passwordForm.value.mode === 'auto' && generatedPassword"
         class="generated-password">
      <div class="password-display">
        <code>{{ generatedPassword }}</code>
        <button mat-icon-button (click)="copyPassword()" type="button">
          <mat-icon>content_copy</mat-icon>
        </button>
      </div>
      <button mat-stroked-button (click)="generateNewPassword()" type="button">
        <mat-icon>refresh</mat-icon>
        Generate new password
      </button>
    </div>

    <!-- Options -->
    <div class="password-options">
      <mat-checkbox formControlName="mustChange">
        Require this user to change their password when they first sign in
      </mat-checkbox>

      <mat-checkbox formControlName="sendEmail">
        Send password in email on completion
      </mat-checkbox>

      <mat-checkbox formControlName="neverExpires">
        Password never expires
      </mat-checkbox>
    </div>
  </form>
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button (click)="onCancel()">Cancel</button>
  <button mat-raised-button
          color="primary"
          (click)="onSave()"
          [disabled]="!passwordForm.valid">
    Save
  </button>
</mat-dialog-actions>
```

### 6.3 Phase 3: Login Flow Integration (Week 3)

#### 6.3.1 Modified Login Component

**login.component.ts:**
```typescript
onLoginSuccess(response: LoginResponse): void {
  // Store token
  this.authService.setToken(response.token, response.refreshToken);

  // Check if password change required
  if (response.mustChangePassword) {
    this.openChangePasswordDialog(response.canSkipPasswordChange);
  } else if (response.daysUntilPasswordExpires <= 14 && response.daysUntilPasswordExpires > 0) {
    this.showPasswordExpirationWarning(response.daysUntilPasswordExpires);
  } else {
    this.navigateToDashboard();
  }
}

private openChangePasswordDialog(canSkip: boolean): void {
  const dialogRef = this.dialog.open(ChangePasswordDialogComponent, {
    width: '500px',
    disableClose: !canSkip,
    data: {
      mustChange: true,
      canSkip: canSkip
    }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result?.success) {
      this.showSuccessMessage('Password changed successfully');
      this.navigateToDashboard();
    } else if (result?.skipped) {
      this.navigateToDashboard();
    }
  });
}

private showPasswordExpirationWarning(days: number): void {
  this.snackBar.open(
    `Your password will expire in ${days} days. Please change it soon.`,
    'Change Now',
    { duration: 10000 }
  ).onAction().subscribe(() => {
    this.openChangePasswordDialog(true);
  });

  setTimeout(() => this.navigateToDashboard(), 500);
}
```

---

## 7. Security Considerations

### 7.1 Password Storage

**Current:** AES Encryption
**Proposed:** Bcrypt or Argon2

**Migration Strategy:**
```csharp
public class HybridPasswordService
{
    public bool VerifyPassword(string password, string hash)
    {
        // Check if it's old AES format
        if (hash.StartsWith("AES:"))
        {
            return _aesService.Verify(password, hash);
        }

        // Otherwise use bcrypt
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string HashPassword(string password)
    {
        // Always use bcrypt for new passwords
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
```

### 7.2 Password Complexity

**Recommended Rules:**
- Minimum 8 characters
- At least 1 uppercase letter (A-Z)
- At least 1 lowercase letter (a-z)
- At least 1 digit (0-9)
- At least 1 special character (!@#$%^&*()_+-=[]{}|;:,.<>?)
- Not in password history (last 5 passwords)
- Not a common password (check against common password list)

### 7.3 Rate Limiting

**Implement at API level:**
```csharp
[EnableRateLimiting("password-attempts")]
[HttpPost("change-password")]
public async Task<IActionResult> ChangePassword(...)
```

**Configure in Program.cs:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("password-attempts", config =>
    {
        config.Window = TimeSpan.FromMinutes(15);
        config.PermitLimit = 5;
        config.QueueLimit = 0;
    });
});
```

### 7.4 Audit Logging

**Log all password operations:**
- Password set by admin
- Password changed by user
- Password reset
- Failed login attempts
- Account locked/unlocked
- Password change skipped

### 7.5 Email Security

**When sending passwords via email:**
- Use secure email templates
- Send from no-reply address
- Include security warning
- Expire link after 24 hours (for future self-service reset)
- Never log full password in audit trail

---

## 8. Implementation Phases

### Phase 1: Foundation (Week 1) - 5 days

**Day 1-2: Database & Entities**
- [ ] Create database migration
- [ ] Add new entities (PasswordHistory, PasswordAuditLog, PasswordPolicy)
- [ ] Update User entity
- [ ] Configure EF Core relationships
- [ ] Test migration on dev database

**Day 3-4: Core Services**
- [ ] Implement IPasswordService interface
- [ ] Create PasswordService class
- [ ] Implement password hashing (bcrypt)
- [ ] Implement password validation
- [ ] Implement password generation
- [ ] Write unit tests for PasswordService

**Day 5: Repository & Infrastructure**
- [ ] Create password-related repositories
- [ ] Update UnitOfWork
- [ ] Implement audit logging
- [ ] Test database operations

**Deliverable:** Core password management infrastructure ready

### Phase 2: API & Backend Logic (Week 2) - 5 days

**Day 1-2: API Endpoints**
- [ ] Create PasswordController
- [ ] Implement admin endpoints (set, reset, unlock)
- [ ] Implement user endpoints (change, skip, validate)
- [ ] Create DTOs and request/response models
- [ ] Add authorization attributes

**Day 3: Enhanced Login Flow**
- [ ] Modify LoginCommandHandler
- [ ] Add password expiration check
- [ ] Add account lockout check
- [ ] Enhance LoginResponse with password info
- [ ] Update failed login tracking

**Day 4: Password Policy**
- [ ] Implement password policy endpoints
- [ ] Create password policy management
- [ ] Add password policy validation
- [ ] Test policy enforcement

**Day 5: Testing & Documentation**
- [ ] Write integration tests
- [ ] Test all endpoints with Postman
- [ ] Update API documentation
- [ ] Create endpoint test scripts

**Deliverable:** Fully functional backend API

### Phase 3: Frontend Implementation (Week 3) - 5 days

**Day 1-2: User Components**
- [ ] Create ChangePasswordDialogComponent
- [ ] Implement password strength indicator
- [ ] Add password requirements checklist
- [ ] Integrate with password service
- [ ] Add to user profile menu

**Day 3-4: Admin Components**
- [ ] Create SetPasswordDialogComponent
- [ ] Create ResetPasswordDialogComponent
- [ ] Create PasswordStatusDialogComponent
- [ ] Update user management component
- [ ] Add password management buttons

**Day 5: Login Flow Integration**
- [ ] Modify login component
- [ ] Add password change prompt on login
- [ ] Add password expiration warning
- [ ] Add account locked message
- [ ] Test complete login flow

**Deliverable:** Complete UI/UX for password management

### Phase 4: Polish & Testing (Week 4) - 3 days

**Day 1: E2E Testing**
- [ ] Write Playwright E2E tests
- [ ] Test admin password operations
- [ ] Test user password change
- [ ] Test forced password change flow
- [ ] Test account lockout/unlock

**Day 2: Security Review**
- [ ] Security audit of password storage
- [ ] Review audit logging
- [ ] Test rate limiting
- [ ] Penetration testing
- [ ] Fix security issues

**Day 3: Documentation & Training**
- [ ] Write user documentation
- [ ] Write admin documentation
- [ ] Create training materials
- [ ] Record demo videos
- [ ] Prepare deployment guide

**Deliverable:** Production-ready password management system

---

## 9. Testing Strategy

### 9.1 Unit Tests

**PasswordService Tests:**
```csharp
[Fact]
public async Task ValidatePasswordComplexity_ValidPassword_ReturnsValid()
{
    // Arrange
    var password = "SecureP@ssw0rd";

    // Act
    var result = await _passwordService.ValidatePasswordComplexityAsync(
        password, companyId);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
    Assert.Equal(PasswordStrength.Strong, result.Strength);
}

[Fact]
public async Task IsPasswordInHistory_ReusedPassword_ReturnsTrue()
{
    // Arrange
    var userId = Guid.NewGuid();
    var password = "OldPassword123!";
    await _passwordService.AddPasswordToHistoryAsync(userId, password, null);

    // Act
    var result = await _passwordService.IsPasswordInHistoryAsync(userId, password);

    // Assert
    Assert.True(result);
}

[Fact]
public async Task IncrementFailedLoginAttempt_FiveAttempts_LocksAccount()
{
    // Arrange
    var userId = Guid.NewGuid();

    // Act
    for (int i = 0; i < 5; i++)
    {
        await _passwordService.IncrementFailedLoginAttemptAsync(userId);
    }

    // Assert
    var isLocked = await _passwordService.IsAccountLockedAsync(userId);
    Assert.True(isLocked);
}
```

### 9.2 Integration Tests

**Password API Tests:**
```csharp
[Fact]
public async Task SetUserPassword_AsAdmin_Success()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(adminToken);
    var userId = testUser.Id;
    var request = new SetPasswordRequest
    {
        Password = "NewSecureP@ssw0rd123",
        MustChangeOnNextLogin = true
    };

    // Act
    var response = await client.PostAsJsonAsync(
        $"/api/password/users/{userId}/set-password", request);

    // Assert
    response.EnsureSuccessStatusCode();

    // Verify user has new password
    var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
        new { Email = testUser.Email, Password = request.Password });
    loginResponse.EnsureSuccessStatusCode();
}

[Fact]
public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
{
    // Arrange
    var client = _factory.CreateAuthenticatedClient(userToken);
    var request = new ChangePasswordRequest
    {
        CurrentPassword = "WrongPassword",
        NewPassword = "NewP@ssw0rd123",
        ConfirmPassword = "NewP@ssw0rd123"
    };

    // Act
    var response = await client.PostAsJsonAsync(
        "/api/password/change-password", request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

### 9.3 E2E Tests

**Playwright Test:**
```typescript
test('admin can set user password', async ({ page }) => {
  // Login as admin
  await page.goto('http://localhost:4200/login');
  await page.fill('input[name="email"]', 'admin@test.com');
  await page.fill('input[name="password"]', 'Admin@123');
  await page.click('button[type="submit"]');

  // Navigate to user management
  await page.click('text=Users');
  await page.waitForSelector('.user-table');

  // Click set password for first user
  await page.click('button[aria-label="Set password"]');

  // Fill password form
  await page.click('text=Let me create a password');
  await page.fill('input[name="password"]', 'TestP@ssw0rd123');
  await page.check('text=Require password change');

  // Save
  await page.click('button:has-text("Save")');

  // Verify success message
  await expect(page.locator('.success-message')).toContainText('Password set');
});

test('user must change password on first login', async ({ page }) => {
  // Login with temp password
  await page.goto('http://localhost:4200/login');
  await page.fill('input[name="email"]', 'testuser@test.com');
  await page.fill('input[name="password"]', 'TempP@ss123');
  await page.click('button[type="submit"]');

  // Change password dialog should appear
  await expect(page.locator('h2')).toContainText('Update your password');

  // Fill change password form
  await page.fill('input[name="currentPassword"]', 'TempP@ss123');
  await page.fill('input[name="newPassword"]', 'NewSecureP@ssw0rd123');
  await page.fill('input[name="confirmPassword"]', 'NewSecureP@ssw0rd123');

  // Submit
  await page.click('button:has-text("Change Password")');

  // Should navigate to dashboard
  await expect(page).toHaveURL(/dashboard/);
});
```

---

## 10. Migration Strategy

### 10.1 Data Migration

**Step 1: Add new columns (non-breaking)**
```sql
-- Run this first, doesn't affect existing functionality
ALTER TABLE Users ADD COLUMN PasswordExpiresAt DATETIME NULL;
ALTER TABLE Users ADD COLUMN MustChangePasswordOnNextLogin BIT DEFAULT 0;
-- ... (rest of columns)
```

**Step 2: Create new tables**
```sql
-- Create supporting tables
CREATE TABLE PasswordHistory (...);
CREATE TABLE PasswordAuditLog (...);
CREATE TABLE PasswordPolicy (...);
```

**Step 3: Initialize password policies**
```sql
-- Create default policy for each company
INSERT INTO PasswordPolicy (Id, CompanyId, MinimumLength, RequireUppercase, ...)
SELECT NEWID(), Id, 8, 1, 1, 1, 1, 90, 14, 5, 15, 5, 0, GETUTCDATE(), GETUTCDATE()
FROM Companies;
```

**Step 4: Migrate existing password hashes (gradual)**
```csharp
// In login handler, detect old AES hashes and upgrade
public async Task<Result<LoginResponse>> Handle(...)
{
    // ... verify password

    if (passwordValid && user.PasswordHash.StartsWith("AES:"))
    {
        // Upgrade to bcrypt on successful login
        var newHash = _passwordService.HashPassword(request.Password);
        user.PasswordHash = newHash;
        user.PasswordChangedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### 10.2 Rollout Plan

**Phase 1: Soft Launch (Week 1)**
- Deploy to staging environment
- Test with small group of users
- Collect feedback
- Fix issues

**Phase 2: Limited Production (Week 2)**
- Deploy to production
- Enable for admins only
- Monitor performance
- Verify audit logs

**Phase 3: User Rollout (Week 3)**
- Enable password change for all users
- Send email notification about new feature
- Provide training materials
- Monitor support tickets

**Phase 4: Enforcement (Week 4)**
- Set password expiration policies
- Force password changes for accounts with old passwords
- Enable account lockout
- Full feature activation

---

## 11. Success Metrics

### 11.1 Technical Metrics

- [ ] 100% of password API endpoints working
- [ ] <200ms average password validation time
- [ ] <500ms average password change operation
- [ ] Zero password leaks in audit logs
- [ ] 100% password operations logged

### 11.2 Security Metrics

- [ ] Zero plain-text passwords in database
- [ ] 100% passwords hashed with bcrypt/Argon2
- [ ] Account lockout preventing brute force attacks
- [ ] Password history preventing reuse
- [ ] Audit trail capturing all operations

### 11.3 User Experience Metrics

- [ ] <3 clicks to change password
- [ ] Real-time password strength feedback
- [ ] Clear password requirements display
- [ ] <5 seconds to set user password (admin)
- [ ] Intuitive UI matching Microsoft Teams patterns

---

## 12. Future Enhancements

### 12.1 Self-Service Password Reset

**Email-based reset:**
1. User clicks "Forgot Password"
2. Enters email address
3. Receives secure reset link
4. Link expires after 1 hour
5. User sets new password

### 12.2 Multi-Factor Authentication

**Options:**
- SMS OTP
- Email OTP
- Authenticator app (TOTP)
- Hardware security keys

### 12.3 Single Sign-On (SSO)

**Integration with:**
- Azure AD
- Google Workspace
- Okta
- Auth0

### 12.4 Biometric Authentication

**Support for:**
- Windows Hello
- Face ID
- Touch ID
- Fingerprint readers

### 12.5 Compromised Password Detection

**Features:**
- Integration with Have I Been Pwned API
- Check passwords against breach database
- Alert users of compromised passwords
- Force password change for breached passwords

---

## 13. Documentation Deliverables

### 13.1 Technical Documentation

1. **API Reference**
   - All password endpoints
   - Request/response schemas
   - Error codes
   - Authentication requirements

2. **Database Schema**
   - Entity relationship diagrams
   - Table definitions
   - Migration scripts

3. **Architecture Diagrams**
   - Password flow diagrams
   - Security architecture
   - Integration points

### 13.2 User Documentation

1. **Admin Guide**
   - How to set user passwords
   - How to reset passwords
   - How to configure password policies
   - How to unlock accounts
   - How to view password status

2. **User Guide**
   - How to change password
   - Password requirements
   - What to do if locked out
   - Password best practices

3. **FAQ**
   - Common questions
   - Troubleshooting
   - Security tips

---

## 14. Summary

This comprehensive plan provides a Microsoft Teams-style password management system that includes:

✅ **Admin Features:**
- Set/reset user passwords
- Auto-generate secure passwords
- Configure password change requirements
- View password status
- Unlock accounts

✅ **User Features:**
- Change own password
- Forced password change on first login
- Real-time password strength feedback
- Password requirements checklist

✅ **Security Features:**
- Bcrypt/Argon2 password hashing
- Password complexity validation
- Password history (5 passwords)
- Account lockout after failed attempts
- Comprehensive audit logging

✅ **System Features:**
- Password expiration policies
- Company-wide password policies
- Email notifications
- Rate limiting
- Migration from old AES encryption

**Total Implementation Time:** 4 weeks
**Team Size:** 1-2 developers
**Priority:** High (essential security feature)

---

**Next Steps:**
1. Review and approve this plan
2. Assign development resources
3. Set up development environment
4. Begin Phase 1 implementation
5. Schedule regular progress reviews

Would you like me to proceed with implementing this plan?
