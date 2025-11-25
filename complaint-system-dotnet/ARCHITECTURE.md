# Complaint Management System - Architecture Documentation

## Table of Contents
- [Overview](#overview)
- [Repository Pattern](#repository-pattern)
- [Unit of Work Pattern](#unit-of-work-pattern)
- [Email Ticketing System](#email-ticketing-system)
- [Best Practices](#best-practices)
- [Migration History](#migration-history)

---

## Overview

This document describes the architectural patterns and design decisions for the Complaint Management System.

### Technology Stack
- **Backend**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server
- **Frontend**: Angular
- **Architecture**: Clean Architecture with CQRS patterns

---

## Repository Pattern

### Design Decision

We use a **hybrid repository pattern** that combines:
1. **Specific Repositories**: For entities with complex queries (e.g., `IComplaintRepository`)
2. **Generic Repository**: For simple CRUD operations on any entity

This approach provides flexibility while maintaining consistency.

### Generic Repository Interface

Located at: `ComplaintManagement.Application/Interfaces/Repositories/IRepository.cs`

```csharp
public interface IRepository<T> where T : BaseEntity
{
    // Query methods
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    // Command methods (synchronous - require SaveChangesAsync to persist)
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);

    // Advanced querying
    IQueryable<T> GetQueryable();
}
```

**Key Points:**
- `GetAllAsync()` - Gets all entities (no filter)
- `FindAsync(predicate)` - Gets entities matching a condition
- `Update()` and `Delete()` are synchronous - changes are tracked, not persisted until `SaveChangesAsync()`

---

## Unit of Work Pattern

### Interface

Located at: `ComplaintManagement.Application/Interfaces/Repositories/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    // Specific repositories for complex queries
    IComplaintRepository Complaints { get; }
    IUserRepository Users { get; }
    IComplaintCategoryRepository ComplaintCategories { get; }
    // ... other specific repositories

    // Generic repository for simple entities
    IRepository<T> Repository<T>() where T : BaseEntity;

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Implementation Details

The `UnitOfWork` class uses:
- **Lazy initialization** for repositories (created only when accessed)
- **Dictionary caching** for generic repositories (prevents duplicate instances)
- **Reflection** to dynamically create `Repository<T>` instances

```csharp
private readonly Dictionary<Type, object> _genericRepositories = new();

public IRepository<T> Repository<T>() where T : BaseEntity
{
    var type = typeof(T);

    if (!_genericRepositories.ContainsKey(type))
    {
        var repositoryType = typeof(Repository<>).MakeGenericType(type);
        var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
        _genericRepositories[type] = repositoryInstance!;
    }

    return (IRepository<T>)_genericRepositories[type];
}
```

### Usage Examples

#### Example 1: Using Generic Repository

```csharp
// Service or Controller
private readonly IUnitOfWork _unitOfWork;

public async Task<IEnumerable<EmailConfiguration>> GetActiveConfigsAsync(Guid companyId)
{
    var configs = await _unitOfWork.Repository<EmailConfiguration>()
        .FindAsync(c => c.CompanyId == companyId && c.IsEnabled, CancellationToken.None);

    return configs;
}
```

#### Example 2: Create and Update

```csharp
public async Task<EmailConfiguration> CreateConfigAsync(EmailConfiguration config)
{
    await _unitOfWork.Repository<EmailConfiguration>().AddAsync(config);
    await _unitOfWork.SaveChangesAsync();
    return config;
}

public async Task UpdateConfigAsync(EmailConfiguration config)
{
    _unitOfWork.Repository<EmailConfiguration>().Update(config);
    await _unitOfWork.SaveChangesAsync();
}
```

#### Example 3: Using Specific Repository

```csharp
// When complex queries are needed, use specific repositories
var complaint = await _unitOfWork.Complaints.GetWithDetailsAsync(complaintId);
```

---

## Email Ticketing System

### Overview

The Email Ticketing System enables automatic creation of complaints from incoming emails and allows sending email replies linked to complaints.

### Architecture Components

#### 1. **Domain Entities**

Located at: `ComplaintManagement.Domain/Entities/Communication/`

- **EmailConfiguration**: IMAP/SMTP settings per company
- **EmailMessage**: Stores inbound/outbound emails
- **EmailAttachment**: Stores email attachments metadata

#### 2. **Service Layer**

**EmailTicketingService** (`ComplaintManagement.Infrastructure/Services/EmailTicketingService.cs`)
- Fetches emails via IMAP
- Creates complaints from emails
- Sends ticket replies via SMTP
- Handles email threading (In-Reply-To headers)
- Auto-acknowledgement

**EmailPollingBackgroundService**
- Background service that polls IMAP at configured intervals
- Runs in the background as a hosted service

#### 3. **API Controllers**

**EmailConfigurationController** - CRUD operations for email configurations
**EmailTicketingController** - Email operations (send, retrieve, attachments)

### Database Schema

Migration: `20251111171543_AddEmailTicketingSystem`

**Tables Created:**
1. **EmailConfigurations**: Company-specific IMAP/SMTP settings
2. **EmailMessages**: Email message storage with threading support
3. **EmailAttachments**: Email attachment metadata

**Key Indexes:**
- `EmailMessages.ComplaintId` - Fast lookup of emails for a complaint
- `EmailMessages.CompanyId` - Filter emails by company
- `EmailConfigurations.CompanyId` - One config per company

### Email Flow

```
1. IMAP Poll (Background Service)
   ↓
2. EmailTicketingService.FetchAndProcessEmailsAsync()
   ↓
3. ProcessEmailMessageAsync()
   ↓
4. Check threading (In-Reply-To, References, Subject)
   ↓
5a. Existing thread → Update complaint, add to thread
5b. New email → Create complaint, send auto-acknowledgement
   ↓
6. Save EmailMessage to database
```

### Email Threading

The system uses standard email headers for threading:
- **Message-ID**: Unique identifier for each email
- **In-Reply-To**: ID of the email being replied to
- **References**: Chain of message IDs in the thread

Fallback matching:
- Extract ticket number from subject (e.g., `CMP-20251111-0001`)

---

## Best Practices

### 1. **Repository Access Pattern**

**DO:**
```csharp
// Use FindAsync for queries with predicates
var items = await _unitOfWork.Repository<Entity>()
    .FindAsync(e => e.IsActive, cancellationToken);

// Use FirstOrDefaultAsync for single results
var item = await _unitOfWork.Repository<Entity>()
    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
```

**DON'T:**
```csharp
// WRONG - GetAllAsync doesn't accept predicate
var items = await _unitOfWork.Repository<Entity>()
    .GetAllAsync(e => e.IsActive, cancellationToken);  // Compilation error
```

### 2. **Update and Delete**

```csharp
// Correct pattern for Update/Delete
var entity = await _unitOfWork.Repository<Entity>().GetByIdAsync(id);
if (entity != null)
{
    // Update is synchronous - just tracks changes
    _unitOfWork.Repository<Entity>().Update(entity);

    // Persist changes to database
    await _unitOfWork.SaveChangesAsync();
}
```

### 3. **When to Create Specific Repositories**

Create a specific repository when:
- Entity needs complex queries with multiple joins
- Custom query logic that doesn't fit generic pattern
- Performance optimization with custom SQL

Example: `IComplaintRepository` has methods like:
- `GetWithDetailsAsync()` - Includes categories, status, priority
- `GetByComplaintNumberAsync()` - Custom lookup logic
- `GetComplaintsWithSLAViolationsAsync()` - Complex business query

### 4. **Transaction Management**

```csharp
// For multi-entity operations
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Repository<Entity1>().AddAsync(entity1);
    await _unitOfWork.Repository<Entity2>().AddAsync(entity2);
    await _unitOfWork.SaveChangesAsync();

    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### 5. **Error Handling with Result Pattern**

The system uses `Result<T>` pattern for service responses:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string Message { get; }  // NOT ErrorMessage!
}

// Usage
var result = await _service.SomeOperationAsync();
if (result.IsSuccess)
{
    // Use result.Data
}
else
{
    _logger.LogError("Error: {Message}", result.Message);  // Use Message, not ErrorMessage
}
```

---

## Migration History

### Latest Migration: Email Ticketing System (Nov 11, 2025)

**Migration Name**: `20251111171543_AddEmailTicketingSystem`

**Changes:**
- Added `EmailConfigurations` table (IMAP/SMTP settings)
- Added `EmailMessages` table (email storage with threading)
- Added `EmailAttachments` table (attachment metadata)
- Created foreign key relationships
- Added indexes for performance

**To Apply Migration:**
```bash
cd complaint-system-dotnet/src/ComplaintManagement.Infrastructure
dotnet ef database update --context ComplaintDbContext --startup-project ../ComplaintManagement.API
```

**To Rollback:**
```bash
dotnet ef database update 20251109172604_AddPasswordManagementAndAuthProviderTables --context ComplaintDbContext --startup-project ../ComplaintManagement.API
```

---

## Architectural Decisions

### Why Generic + Specific Repositories?

**Context**: During phased development, different patterns emerged:
- Direct DbContext usage in some places
- Specific repositories in others
- Need for email ticketing entities without creating new interfaces

**Decision**: Implement hybrid pattern with `Repository<T>()` method

**Benefits:**
1. **Non-breaking**: Existing code continues to work
2. **Flexibility**: New features can immediately use generic pattern
3. **Gradual migration**: Can refactor to specific repositories when needed
4. **Industry standard**: Commonly used in enterprise applications

**Trade-offs:**
- Slightly more complex than pure repository pattern
- Requires discipline to maintain consistency

---

## Future Improvements

1. **Specific Repositories for Email Entities**
   - When complex email queries are needed
   - Create `IEmailMessageRepository` with custom methods

2. **Caching Layer**
   - Add distributed caching for frequently accessed data
   - Use Redis for session management

3. **Event Sourcing**
   - Consider CQRS with event sourcing for audit trail
   - Implement domain events for email processing

4. **Monitoring**
   - Add Application Insights for email polling metrics
   - Track email processing success/failure rates

---

## Contact

For questions about architecture decisions, contact the development team.

Last Updated: November 11, 2025
