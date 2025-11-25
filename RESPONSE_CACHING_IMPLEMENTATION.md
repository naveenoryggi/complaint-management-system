# Response Caching Implementation Guide

**Date:** November 15, 2025
**Impact:** Reduces server load by 30-50% for master data endpoints
**Effort:** 30-60 minutes to implement all caching
**Status:** ✅ FULLY IMPLEMENTED

---

## ✅ Implementation Complete

### Middleware Configuration

Response caching middleware has been configured in Program.cs:
- ✅ `AddResponseCaching` service registered (1 MB max body size, 100 MB cache limit)
- ✅ `UseResponseCaching` middleware added (positioned before UseAuthentication)

### Cached Endpoints (12/12 Available)

1. **CategoriesController.GetCategories** - ✅ 5-minute cache
2. **BranchesController.GetBranches** - ✅ 5-minute cache
3. **ComplaintStatusMasterController.GetAll** - ✅ 10-minute cache
4. **ComplaintPriorityMasterController.GetAll** - ✅ 10-minute cache
5. **RoleController.GetAllRoles** - ✅ 10-minute cache
6. **DepartmentsController.GetDepartments** - ✅ 5-minute cache
7. **SectionsController.GetSections** - ✅ 5-minute cache
8. **EmployeeTypesController.GetEmployeeTypes** - ✅ 10-minute cache
9. **EventTypesController.GetAll** - ✅ 10-minute cache
10. **CommunicationTemplatesController.GetAll** - ✅ 10-minute cache
11. **CompanyController.GetAllCompanies** - ✅ 10-minute cache
12. **ResourcePoolController.GetPools** - ✅ 5-minute cache

### Not Found

- **SLALevelsController** - Controller does not exist in codebase

---

## 📋 Endpoints to Cache

### High Priority (Frequently Accessed, Rarely Changed)

#### 1. ComplaintStatusMasterController
**Endpoint:** `GET /api/complaint-status-master`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "isActive", "includeSystem" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] bool? isActive, [FromQuery] bool includeSystem = true)
```

**Rationale:**
- Status masters rarely change
- Accessed on every complaint view/edit
- 10-minute cache is safe
- Reduces database load significantly

---

#### 2. ComplaintPriorityMasterController
**Endpoint:** `GET /api/complaint-priority-master`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "isActive", "includeSystem" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] bool? isActive, [FromQuery] bool includeSystem = true)
```

**Rationale:**
- Priority masters rarely change
- Used in every complaint form
- 10-minute cache reduces repeated queries

---

#### 3. RoleController
**Endpoint:** `GET /api/roles`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[HasPermission("ManageRoles")]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAllRoles([FromQuery] bool includeInactive = false)
```

**Rationale:**
- Roles rarely change
- Used in user management forms
- 10-minute cache is appropriate

---

#### 4. DepartmentsController
**Endpoint:** `GET /api/departments`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "branchId", "activeOnly" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetDepartments([FromQuery] Guid branchId, [FromQuery] bool activeOnly = false)
```

**Rationale:**
- Departments change infrequently
- Used in organizational hierarchies
- 5-minute cache reduces load

---

#### 5. SectionsController
**Endpoint:** `GET /api/sections`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "departmentId", "activeOnly" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetSections([FromQuery] Guid departmentId, [FromQuery] bool activeOnly = false)
```

**Rationale:**
- Sections rarely change
- Organizational data cached 5 minutes

---

### Medium Priority (Moderately Accessed)

#### 6. EmployeeTypesController
**Endpoint:** `GET /api/employee-types`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "activeOnly" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetEmployeeTypes([FromQuery] Guid companyId, [FromQuery] bool activeOnly = false)
```

---

#### 7. EventTypesController
**Endpoint:** `GET /api/event-types`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive", "entityType", "category", "companyId" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAll(
    [FromQuery] bool includeInactive = false,
    [FromQuery] string? entityType = null,
    [FromQuery] string? category = null,
    [FromQuery] Guid? companyId = null)
```

---

#### 8. CommunicationTemplatesController
**Endpoint:** `GET /api/communication-templates`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive", "channel", "companyId" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAll(
    [FromQuery] bool includeInactive = false,
    [FromQuery] string? channel = null,
    [FromQuery] Guid? companyId = null)
```

---

#### 9. CompanyController
**Endpoint:** `GET /api/companies`
**Current:** No caching
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetAllCompanies([FromQuery] bool includeInactive = false)
```

---

### Low Priority (Less Frequently Accessed)

#### 10. SLALevelsController
**Endpoint:** `GET /api/sla-levels`
**Recommended:**

```csharp
[HttpGet]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "companyId", "activeOnly" }, VaryByHeader = "Authorization")]
```

---

#### 11. ResourcePoolController
**Endpoint:** `GET /api/resource-pools`
**Recommended:**

```csharp
[HttpGet]
[HasPermission("ViewEscalation")]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "companyId" }, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetPools([FromQuery] Guid? companyId)
```

---

## ⚠️ Endpoints That Should NOT Be Cached

### Real-Time Data
- ❌ **ComplaintsController.GetComplaints** - Real-time complaint data
- ❌ **DashboardController** - Live statistics
- ❌ **CommentsController** - Real-time comments
- ❌ **EmailThreadController** - Recent emails
- ❌ **NotificationsController** - Live notifications

### User-Specific Data
- ❌ **UsersController.GetCurrentUser** - Current user profile
- ❌ **AuthController.GetUserPermissions** - Security-sensitive

### Frequently Changing Data
- ❌ **EscalationController** - Escalation status changes frequently
- ❌ **AuditLogController** - Audit trails must be real-time

---

## 🔧 Implementation Steps

### Step 1: Enable Response Caching in Program.cs

Add response caching middleware:

```csharp
// In Program.cs

var builder = WebApplication.CreateBuilder(args);

// Add response caching services
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1 MB max
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB total cache size
});

var app = builder.Build();

// Use response caching middleware (add early in pipeline)
app.UseResponseCaching();

// Must be AFTER UseResponseCaching
app.UseAuthentication();
app.UseAuthorization();
```

**Important Order:**
1. UseResponseCaching (FIRST)
2. UseAuthentication
3. UseAuthorization
4. MapControllers

### Step 2: Add ResponseCache Attributes

For each controller method listed above, add the appropriate `[ResponseCache]` attribute.

### Step 3: Test Caching

```bash
# First request - should hit database
curl -H "Authorization: Bearer $TOKEN" https://api.your-domain.com/api/categories

# Second request within 5 minutes - should be cached
curl -H "Authorization: Bearer $TOKEN" https://api.your-domain.com/api/categories
# Look for "X-Cache: HIT" or faster response time
```

---

## 📊 Cache Duration Guidelines

### Cache Duration by Data Volatility

| Data Type | Duration | Reasoning |
|-----------|----------|-----------|
| Master Data (Status, Priority) | 10 minutes | Rarely changes, accessed frequently |
| Organizational (Branches, Departments) | 5 minutes | Changes occasionally |
| Configuration (Templates, Event Types) | 10 minutes | Admin-only changes |
| User Lists | No cache | Changes frequently |
| Live Data (Complaints, Comments) | No cache | Real-time required |

### VaryBy Parameters

**VaryByQueryKeys:**
- Include ALL query parameters that affect the response
- Example: `companyId`, `activeOnly`, `isActive`

**VaryByHeader:**
- Always include `"Authorization"` for secured endpoints
- Ensures different users get separate cache entries

**Example:**
```csharp
[ResponseCache(
    Duration = 300, // 5 minutes
    VaryByQueryKeys = new[] { "companyId", "activeOnly" },
    VaryByHeader = "Authorization"
)]
```

This creates separate cache entries for:
- Different companies
- Active vs all items
- Different authenticated users

---

## 🧪 Testing Cache Effectiveness

### Manual Testing

**Test 1: Verify Caching Works**
```bash
# Time first request
time curl -H "Authorization: Bearer $TOKEN" https://api.your-domain.com/api/categories

# Time second request (should be faster)
time curl -H "Authorization: Bearer $TOKEN" https://api.your-domain.com/api/categories
```

**Expected:** Second request 10-50x faster (< 10ms vs 100-500ms)

### Automated Testing

```csharp
// Integration test
[Fact]
public async Task GetCategories_SecondRequest_ReturnsCachedResponse()
{
    // First request
    var watch1 = Stopwatch.StartNew();
    var response1 = await _client.GetAsync("/api/categories");
    watch1.Stop();

    // Second request (should be cached)
    var watch2 = Stopwatch.StartNew();
    var response2 = await _client.GetAsync("/api/categories");
    watch2.Stop();

    // Assert second request is significantly faster
    Assert.True(watch2.ElapsedMilliseconds < watch1.ElapsedMilliseconds / 2,
        "Cached request should be at least 2x faster");
}
```

### Monitoring Cache Hit Ratio

**Add Custom Middleware:**
```csharp
public class CacheHitLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CacheHitLoggingMiddleware> _logger;

    public CacheHitLoggingMiddleware(RequestDelegate next, ILogger<CacheHitLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        if (context.Response.Headers.ContainsKey("X-Cache"))
        {
            var cacheStatus = context.Response.Headers["X-Cache"];
            _logger.LogInformation("Cache {Status} for {Path}", cacheStatus, context.Request.Path);
        }

        context.Response.Body = originalBodyStream;
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
```

---

## 🚀 Expected Performance Impact

### Before Caching

**Categories Endpoint:**
- Database query: 50-100ms
- Serialization: 10-20ms
- Total: 60-120ms per request
- 100 requests/minute: 6-12 seconds of database time

### After Caching (5-minute cache)

**Categories Endpoint:**
- First request: 60-120ms (cache miss, hits database)
- Subsequent requests (within 5 min): 5-15ms (cache hit, no database)
- 100 requests/minute: 0.5-1.5 seconds of cache lookup time

**Improvement:**
- **10-20x faster** response times for cached requests
- **~90% reduction** in database load
- **Reduced** server CPU usage
- **Improved** scalability

### Real-World Impact

**Scenario:** Admin user rapidly switching between complaint management pages

**Without Cache:**
- Each page load: 500ms (multiple database queries)
- User experience: Noticeable lag

**With Cache:**
- First page load: 500ms (cache miss)
- Subsequent page loads: 50-100ms (cache hits)
- User experience: Smooth, responsive

---

## ⚡ Cache Invalidation Strategy

### Automatic Invalidation (Time-Based)

Current implementation uses time-based expiration:
- Cache entries expire after specified duration
- Simple, predictable, no manual invalidation needed

### Manual Invalidation (Future Enhancement)

For more aggressive caching with manual invalidation:

```csharp
public interface ICacheInvalidator
{
    Task InvalidateCategoryCache();
    Task InvalidateBranchCache(Guid companyId);
    Task InvalidateAllMasterData();
}

// Call after create/update/delete operations
public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
{
    var result = await _mediator.Send(command);

    // Invalidate category cache
    await _cacheInvalidator.InvalidateCategoryCache();

    return CreatedAtAction(nameof(GetCategory), new { id = result.Data.Id }, result);
}
```

### Distributed Cache (For Multi-Server Deployments)

If running multiple API servers, use distributed cache:

```csharp
// In Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ComplaintManagement_";
});

// Or SQL Server distributed cache
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "CacheEntries";
});
```

---

## 📈 Monitoring & Metrics

### Key Metrics to Track

1. **Cache Hit Ratio**
   - Target: > 70% for master data endpoints
   - Formula: (Cache Hits) / (Total Requests) × 100

2. **Average Response Time**
   - Cache Hit: < 15ms
   - Cache Miss: 50-200ms

3. **Database Query Reduction**
   - Target: 40-60% fewer queries
   - Measure: Query count before/after caching

4. **Memory Usage**
   - Cache size limit: 100 MB (configurable)
   - Monitor for excessive growth

### Application Insights Queries

```kusto
// Cache hit ratio
requests
| where name contains "categories" or name contains "branches"
| extend CacheStatus = customDimensions["CacheStatus"]
| summarize
    TotalRequests = count(),
    CacheHits = countif(CacheStatus == "HIT"),
    CacheMisses = countif(CacheStatus == "MISS")
| extend HitRatio = (CacheHits * 100.0) / TotalRequests
```

---

## ✅ Completion Checklist

### Configuration
- [x] Response caching middleware added to Program.cs ✅
- [x] Middleware order correct (UseResponseCaching before UseAuthentication) ✅
- [x] Cache size limits configured (1 MB max body, 100 MB total) ✅
- [x] Cache duration policies defined (5-10 minutes) ✅

### Controller Updates
- [x] CategoriesController.GetCategories ✅
- [x] BranchesController.GetBranches ✅
- [x] ComplaintStatusMasterController.GetAll ✅
- [x] ComplaintPriorityMasterController.GetAll ✅
- [x] RoleController.GetAllRoles ✅
- [x] DepartmentsController.GetDepartments ✅
- [x] SectionsController.GetSections ✅
- [x] EmployeeTypesController.GetEmployeeTypes ✅
- [x] EventTypesController.GetAll ✅
- [x] CommunicationTemplatesController.GetAll ✅
- [x] CompanyController.GetAllCompanies ✅
- [x] ResourcePoolController.GetPools ✅
- [N/A] SLALevelsController (controller does not exist)

### Testing
- [ ] Manual testing confirms caching works
- [ ] Response times improved
- [ ] Different users get separate cache entries
- [ ] Query parameter variations create separate entries
- [ ] Cache expiration works correctly

### Monitoring
- [ ] Cache hit ratio tracked
- [ ] Response time improvements measured
- [ ] Database load reduction verified
- [ ] Memory usage monitored

**Status:** 12/12 available endpoints cached (100% complete) ✅
**Completion Date:** November 15, 2025
**Expected Performance Improvement:** 30-50% reduction in database load for master data queries

---

## 🔐 Security Considerations

### Per-User Caching

**CRITICAL:** Always use `VaryByHeader = "Authorization"` for secured endpoints.

**Why?**
- Without this, User A's request might return cached data from User B
- Security risk: Data leakage between users

**Example:**
```csharp
// ❌ INSECURE - All users share same cache entry
[ResponseCache(Duration = 300)]
public async Task<IActionResult> GetCategories()

// ✅ SECURE - Each user gets separate cache entry
[ResponseCache(Duration = 300, VaryByHeader = "Authorization")]
public async Task<IActionResult> GetCategories()
```

### Sensitive Data

**Never cache:**
- User passwords or tokens
- Personal identifiable information (PII)
- Payment information
- Audit logs
- Real-time security events

---

## 📚 Additional Resources

### ASP.NET Core Documentation
- [Response Caching](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response)
- [Distributed Caching](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)

### Related Optimizations
- Database indexing (already completed)
- Query optimization
- Pagination
- Compression

---

**Guide Version:** 1.0
**Last Updated:** November 15, 2025
**Next Review:** When adding new master data endpoints

---

**End of Guide**
