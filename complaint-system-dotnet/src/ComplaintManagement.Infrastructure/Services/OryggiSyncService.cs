using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Configuration;
using ComplaintManagement.Domain.Entities.Oryggi;
using ComplaintManagement.Domain.Entities.Sync;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComplaintManagement.Infrastructure.Services;

public class OryggiSyncService
{
    private readonly OryggiDbContext _oryggiContext;
    private readonly ComplaintDbContext _appContext;
    private readonly ILogger<OryggiSyncService> _logger;
    private readonly OryggiSettings _settings;
    private readonly IEncryptionService _encryptionService;

    public OryggiSyncService(
        OryggiDbContext oryggiContext,
        ComplaintDbContext appContext,
        ILogger<OryggiSyncService> logger,
        IOptions<OryggiSettings> settings,
        IEncryptionService encryptionService)
    {
        _oryggiContext = oryggiContext;
        _appContext = appContext;
        _logger = logger;
        _settings = settings.Value;
        _encryptionService = encryptionService;
    }

    public async Task<SyncLog> SyncAllAsync(Guid tenantId, string syncType = "SCHEDULED")
    {
        var syncLog = new SyncLog
        {
            TenantId = tenantId,
            SyncType = syncType,
            SyncStartedAt = DateTime.UtcNow,
            Status = "IN_PROGRESS"
        };

        try
        {
            _logger.LogInformation("Starting Oryggi sync for tenant {TenantId}", tenantId);

            await _appContext.SyncLogs.AddAsync(syncLog);
            await _appContext.SaveChangesAsync();

            // Sync in hierarchical order to maintain FK relationships
            await SyncCompaniesAsync(syncLog);
            await SyncBranchesAsync(syncLog);
            await SyncDepartmentsAsync(syncLog);
            await SyncSectionsAsync(syncLog);
            await SyncEmployeesAsync(syncLog);

            syncLog.SyncCompletedAt = DateTime.UtcNow;
            syncLog.Duration = syncLog.SyncCompletedAt.Value - syncLog.SyncStartedAt;
            syncLog.Status = "SUCCESS";

            _logger.LogInformation("Oryggi sync completed successfully for tenant {TenantId}. Duration: {Duration}s",
                tenantId, syncLog.Duration?.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oryggi sync failed for tenant {TenantId}", tenantId);

            syncLog.SyncCompletedAt = DateTime.UtcNow;
            syncLog.Duration = syncLog.SyncCompletedAt.Value - syncLog.SyncStartedAt;
            syncLog.Status = "FAILED";
            syncLog.ErrorMessage = ex.Message;
            syncLog.ErrorDetails = ex.ToString();
        }
        finally
        {
            _appContext.SyncLogs.Update(syncLog);
            await _appContext.SaveChangesAsync();
        }

        return syncLog;
    }

    private async Task SyncCompaniesAsync(SyncLog syncLog)
    {
        try
        {
            _logger.LogInformation("Syncing companies from Oryggi");

            var oryggiCompanies = await _oryggiContext.CompanyMaster
                .AsNoTracking()
                .ToListAsync();

            syncLog.CompaniesProcessed = oryggiCompanies.Count;

            foreach (var oryggiCompany in oryggiCompanies)
            {
                var oryggiCompanyId = oryggiCompany.Ccode.ToString();
                var existingCompany = await _appContext.Companies
                    .FirstOrDefaultAsync(c => c.OryggiCompanyId == oryggiCompanyId);

                if (existingCompany == null)
                {
                    // Create new company
                    var newCompany = new Domain.Entities.MasterData.Company
                    {
                        TenantId = syncLog.TenantId,
                        OryggiCompanyId = oryggiCompany.Ccode.ToString(),
                        Name = oryggiCompany.CName ?? $"Company {oryggiCompany.Ccode}",
                        Code = oryggiCompany.Ccode.ToString(),
                        Address = oryggiCompany.Address,
                        ContactEmail = oryggiCompany.Email,
                        ContactPhone = oryggiCompany.TelephoneNo,
                        IsActive = true
                    };

                    await _appContext.Companies.AddAsync(newCompany);
                    syncLog.CompaniesCreated++;
                }
                else
                {
                    // Update existing company
                    existingCompany.Name = oryggiCompany.CName ?? existingCompany.Name;
                    existingCompany.Address = oryggiCompany.Address;
                    existingCompany.ContactEmail = oryggiCompany.Email;
                    existingCompany.ContactPhone = oryggiCompany.TelephoneNo;

                    _appContext.Companies.Update(existingCompany);
                    syncLog.CompaniesUpdated++;
                }
            }

            await _appContext.SaveChangesAsync();
            _logger.LogInformation("Companies synced: {Created} created, {Updated} updated",
                syncLog.CompaniesCreated, syncLog.CompaniesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing companies");
            throw;
        }
    }

    private async Task SyncBranchesAsync(SyncLog syncLog)
    {
        try
        {
            _logger.LogInformation("Syncing branches from Oryggi");

            var oyggiBranches = await _oryggiContext.BranchMaster
                .AsNoTracking()
                .ToListAsync();

            syncLog.BranchesProcessed = oyggiBranches.Count;

            foreach (var oyggiBranch in oyggiBranches)
            {
                // Find parent company
                Guid? companyId = null;
                if (oyggiBranch.Ccode.HasValue)
                {
                    var oryggiCompanyId = oyggiBranch.Ccode.Value.ToString();
                    var company = await _appContext.Companies
                        .FirstOrDefaultAsync(c => c.OryggiCompanyId == oryggiCompanyId);
                    companyId = company?.Id;
                }

                var oyggiBranchId = oyggiBranch.BranchCode.ToString();
                var existingBranch = await _appContext.Branches
                    .FirstOrDefaultAsync(b => b.OryggiBranchId == oyggiBranchId);

                if (existingBranch == null && companyId.HasValue)
                {
                    // Create new branch
                    var newBranch = new Domain.Entities.MasterData.Branch
                    {
                        CompanyId = companyId.Value,
                        OryggiBranchId = oyggiBranch.BranchCode.ToString(),
                        Name = oyggiBranch.BranchName ?? $"Branch {oyggiBranch.BranchCode}",
                        Code = oyggiBranch.BranchCode.ToString(),
                        Address = oyggiBranch.Location,
                        IsActive = true
                    };

                    await _appContext.Branches.AddAsync(newBranch);
                    syncLog.BranchesCreated++;
                }
                else if (existingBranch != null)
                {
                    // Update existing branch
                    if (companyId.HasValue)
                        existingBranch.CompanyId = companyId.Value;
                    existingBranch.Name = oyggiBranch.BranchName ?? existingBranch.Name;
                    existingBranch.Code = oyggiBranch.BranchCode.ToString();
                    existingBranch.Address = oyggiBranch.Location;

                    _appContext.Branches.Update(existingBranch);
                    syncLog.BranchesUpdated++;
                }
            }

            await _appContext.SaveChangesAsync();
            _logger.LogInformation("Branches synced: {Created} created, {Updated} updated",
                syncLog.BranchesCreated, syncLog.BranchesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing branches");
            throw;
        }
    }

    private async Task SyncDepartmentsAsync(SyncLog syncLog)
    {
        try
        {
            _logger.LogInformation("Syncing departments from Oryggi");

            var oryggiDepartments = await _oryggiContext.DeptMaster
                .AsNoTracking()
                .ToListAsync();

            syncLog.DepartmentsProcessed = oryggiDepartments.Count;

            foreach (var oryggiDept in oryggiDepartments)
            {
                // Find parent branch
                Guid? branchId = null;
                if (oryggiDept.BranchCode.HasValue)
                {
                    var oyggiBranchId = oryggiDept.BranchCode.Value.ToString();
                    var branch = await _appContext.Branches
                        .FirstOrDefaultAsync(b => b.OryggiBranchId == oyggiBranchId);
                    branchId = branch?.Id;
                }

                var oryggiDeptId = oryggiDept.Dcode.ToString();
                var existingDept = await _appContext.Departments
                    .FirstOrDefaultAsync(d => d.OryggiDepartmentId == oryggiDeptId);

                if (existingDept == null && branchId.HasValue)
                {
                    // Create new department
                    var newDept = new Domain.Entities.MasterData.Department
                    {
                        BranchId = branchId.Value,
                        OryggiDepartmentId = oryggiDept.Dcode.ToString(),
                        Name = oryggiDept.Dname ?? $"Department {oryggiDept.Dcode}",
                        Code = oryggiDept.Dcode.ToString(),
                        IsActive = true
                    };

                    await _appContext.Departments.AddAsync(newDept);
                    syncLog.DepartmentsCreated++;
                }
                else if (existingDept != null)
                {
                    // Update existing department
                    if (branchId.HasValue)
                        existingDept.BranchId = branchId.Value;
                    existingDept.Name = oryggiDept.Dname ?? existingDept.Name;
                    existingDept.Code = oryggiDept.Dcode.ToString();

                    _appContext.Departments.Update(existingDept);
                    syncLog.DepartmentsUpdated++;
                }
            }

            await _appContext.SaveChangesAsync();
            _logger.LogInformation("Departments synced: {Created} created, {Updated} updated",
                syncLog.DepartmentsCreated, syncLog.DepartmentsUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing departments");
            throw;
        }
    }

    private async Task SyncSectionsAsync(SyncLog syncLog)
    {
        try
        {
            _logger.LogInformation("Syncing sections from Oryggi");

            var oryggiSections = await _oryggiContext.SectionMaster
                .AsNoTracking()
                .ToListAsync();

            syncLog.SectionsProcessed = oryggiSections.Count;

            foreach (var oryggiSection in oryggiSections)
            {
                // Find parent department
                Guid? departmentId = null;
                if (oryggiSection.Dcode.HasValue)
                {
                    var oryggiDepartmentId = oryggiSection.Dcode.Value.ToString();
                    var department = await _appContext.Departments
                        .FirstOrDefaultAsync(d => d.OryggiDepartmentId == oryggiDepartmentId);
                    departmentId = department?.Id;
                }

                var oryggiSecId = oryggiSection.SecCode.ToString();
                var existingSection = await _appContext.Sections
                    .FirstOrDefaultAsync(s => s.OryggiSectionId == oryggiSecId);

                if (existingSection == null && departmentId.HasValue)
                {
                    // Create new section
                    var newSection = new Domain.Entities.MasterData.Section
                    {
                        DepartmentId = departmentId.Value,
                        OryggiSectionId = oryggiSection.SecCode.ToString(),
                        Name = oryggiSection.SecName ?? $"Section {oryggiSection.SecCode}",
                        Code = oryggiSection.SecCode.ToString(),
                        IsActive = true
                    };

                    await _appContext.Sections.AddAsync(newSection);
                    syncLog.SectionsCreated++;
                }
                else if (existingSection != null)
                {
                    // Update existing section
                    if (departmentId.HasValue)
                        existingSection.DepartmentId = departmentId.Value;
                    existingSection.Name = oryggiSection.SecName ?? existingSection.Name;
                    existingSection.Code = oryggiSection.SecCode.ToString();

                    _appContext.Sections.Update(existingSection);
                    syncLog.SectionsUpdated++;
                }
            }

            await _appContext.SaveChangesAsync();
            _logger.LogInformation("Sections synced: {Created} created, {Updated} updated",
                syncLog.SectionsCreated, syncLog.SectionsUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing sections");
            throw;
        }
    }

    private async Task SyncEmployeesAsync(SyncLog syncLog)
    {
        try
        {
            _logger.LogInformation("Syncing employees from Oryggi");

            // Get total count first
            var totalCount = await _oryggiContext.EmployeeMaster
                .Where(e => (e.Active == true || e.Active == null)
                    && e.Ecode != 1 // Exclude admin account (Ecode=1)
                    && (e.CorpEmpCode == null || !e.CorpEmpCode.Contains("_")) // Exclude non-employees (CorpEmpCode with underscore)
                    && (e.Role == null || e.Role != "Visitor")) // Exclude visitor accounts
                .CountAsync();

            syncLog.EmployeesProcessed = totalCount;
            _logger.LogInformation("Found {TotalEmployees} employees to sync. Processing in batches of 50...", totalCount);

            // Process in batches of 50 to avoid SQL Server parameter limit (2100 max)
            // Each employee creates both an Employee and User record with ~29 fields each
            const int batchSize = 50;
            var batchNumber = 0;
            var skip = 0;

            while (skip < totalCount)
            {
                batchNumber++;
                _logger.LogInformation("Processing employee batch {BatchNumber}: {Skip}-{End} of {Total}",
                    batchNumber, skip + 1, Math.Min(skip + batchSize, totalCount), totalCount);

                var oryggiEmployees = await _oryggiContext.EmployeeMaster
                    .AsNoTracking()
                    .Where(e => (e.Active == true || e.Active == null)
                        && e.Ecode != 1 // Exclude admin account (Ecode=1)
                        && (e.CorpEmpCode == null || !e.CorpEmpCode.Contains("_")) // Exclude non-employees (CorpEmpCode with underscore)
                        && (e.Role == null || e.Role != "Visitor")) // Exclude visitor accounts
                    .OrderBy(e => e.Ecode) // Ensure consistent ordering
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!oryggiEmployees.Any())
                    break;

                await ProcessEmployeeBatch(oryggiEmployees, syncLog);

                skip += batchSize;

                // Log progress
                _logger.LogInformation("Completed batch {BatchNumber}. Progress: {Processed}/{Total} employees",
                    batchNumber, Math.Min(skip, totalCount), totalCount);
            }

            _logger.LogInformation("Employees synced: {Created} created, {Updated} updated",
                syncLog.EmployeesCreated, syncLog.EmployeesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing employees");
            throw;
        }
    }

    private async Task ProcessEmployeeBatch(List<EmployeeMaster> oryggiEmployees, SyncLog syncLog)
    {
        try
        {
            // PERFORMANCE OPTIMIZATION: Load all reference data ONCE before processing
            // This eliminates N+1 query problem

            // 1. Load all sections with their full hierarchy
            var sections = await _appContext.Sections
                .Include(s => s.Department)
                    .ThenInclude(d => d.Branch)
                        .ThenInclude(b => b.Company)
                .Where(s => !s.IsDeleted)
                .ToListAsync();
            // Handle duplicates by grouping and taking first occurrence (keeps nulls converted to "")
            var sectionLookup = sections
                .GroupBy(s => s.OryggiSectionId ?? "")
                .ToDictionary(g => g.Key, g => g.First());

            // 2. Load all designations (only ~500 rows, so load all instead of filtering)
            var designations = await _oryggiContext.DesignationMaster
                .AsNoTracking()
                .ToListAsync();
            // Handle duplicates by grouping and taking first occurrence
            var designationLookup = designations
                .GroupBy(d => d.DesigCode)
                .ToDictionary(g => g.Key, g => g.First().DesigName);

            // 3. Load existing employees for this batch (include deleted so they can be restored)
            var employeeEcodes = oryggiEmployees.Select(e => e.Ecode.ToString()).ToList();
            var existingEmployees = await _appContext.Employees
                .Where(e => employeeEcodes.Contains(e.OryggiEmployeeId))
                .ToListAsync();
            // Handle duplicates by grouping and taking first occurrence (keeps nulls for fallback lookup)
            var existingEmployeeLookup = existingEmployees
                .GroupBy(e => e.OryggiEmployeeId ?? "")
                .ToDictionary(g => g.Key, g => g.First());

            // 4. Load existing users for this batch (include deleted so they can be restored)
            var existingUsers = await _appContext.Users
                .Where(u => employeeEcodes.Contains(u.OryggiEmployeeId))
                .ToListAsync();
            // Handle duplicates by grouping and taking first occurrence (keeps nulls for fallback lookup)
            var existingUserLookup = existingUsers
                .GroupBy(u => u.OryggiEmployeeId ?? "")
                .ToDictionary(g => g.Key, g => g.First());

            // 5. Load all active users for manager lookups (need full list since managers might be outside current batch)
            var allUsers = await _appContext.Users
                .Where(u => !u.IsDeleted && u.IsActive)
                .Select(u => new { u.Id, u.OryggiEmployeeId })
                .ToListAsync();
            // Handle duplicates by grouping and taking first occurrence (keeps nulls)
            var userManagerLookup = allUsers
                .GroupBy(u => u.OryggiEmployeeId ?? "")
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Now process each employee using the cached data
            foreach (var oryggiEmployee in oryggiEmployees)
            {
                // Find parent section and its hierarchy using lookup
                Guid? sectionId = null;
                Guid? departmentId = null;
                Guid? branchId = null;
                Guid? companyId = null;

                if (oryggiEmployee.SecCode.HasValue)
                {
                    var oryggiSectionId = oryggiEmployee.SecCode.Value.ToString();
                    if (sectionLookup.TryGetValue(oryggiSectionId, out var section))
                    {
                        sectionId = section.Id;
                        departmentId = section.DepartmentId;
                        branchId = section.Department?.BranchId;
                        companyId = section.Department?.Branch?.CompanyId;
                    }
                }

                var oryggiEmpId = oryggiEmployee.Ecode.ToString();
                existingEmployeeLookup.TryGetValue(oryggiEmpId, out var existingEmployee);

                if (existingEmployee == null)
                {
                    // Create new employee
                    var newEmployee = new Domain.Entities.MasterData.Employee
                    {
                        TenantId = syncLog.TenantId,
                        SectionId = sectionId,
                        OryggiEmployeeId = oryggiEmployee.Ecode.ToString(),
                        EmployeeCode = oryggiEmployee.CorpEmpCode ?? oryggiEmployee.Ecode.ToString(),
                        FirstName = oryggiEmployee.FName ?? "",
                        LastName = oryggiEmployee.LName ?? "",
                        FullName = oryggiEmployee.EmpName ?? $"{oryggiEmployee.FName} {oryggiEmployee.LName}".Trim(),
                        Email = oryggiEmployee.E_mail,
                        Phone = oryggiEmployee.Telephone1,
                        AlternatePhone = oryggiEmployee.Telephone2,
                        DateOfJoining = oryggiEmployee.DateofJoin,
                        DateOfBirth = oryggiEmployee.DateofBirth,
                        IsActive = oryggiEmployee.Active ?? true
                    };

                    // Try to save employee - continue on error
                    try
                    {
                        await _appContext.Employees.AddAsync(newEmployee);
                        await _appContext.SaveChangesAsync();
                        syncLog.EmployeesCreated++;
                    }
                    catch (Exception empEx)
                    {
                        _logger.LogWarning(empEx, "Failed to create employee {EmployeeCode} (Ecode: {Ecode}). Skipping. Error: {Error}",
                            oryggiEmployee.CorpEmpCode, oryggiEmployee.Ecode, empEx.Message);
                        syncLog.EmployeesFailed++;
                        _appContext.ChangeTracker.Clear(); // Clear failed changes
                        continue; // Skip to next employee
                    }

                    // Create corresponding User account
                    // Generate unique email if E_mail is null, empty, or "NA"
                    var userEmail = oryggiEmployee.E_mail;
                    if (string.IsNullOrWhiteSpace(userEmail) || userEmail.Equals("NA", StringComparison.OrdinalIgnoreCase))
                    {
                        // Use CorpEmpCode if available, otherwise use Ecode (which is always unique)
                        var empCode = !string.IsNullOrWhiteSpace(oryggiEmployee.CorpEmpCode)
                            ? oryggiEmployee.CorpEmpCode
                            : oryggiEmployee.Ecode.ToString();
                        userEmail = $"{empCode}@system.local";
                    }

                    existingUserLookup.TryGetValue(oryggiEmpId, out var existingUser);

                    if (existingUser == null)
                    {
                        // Log if company is missing
                        if (!companyId.HasValue)
                        {
                            _logger.LogWarning("Employee {Ecode} has no company in hierarchy. SecCode={SecCode}, Creating user without company.",
                                oryggiEmployee.Ecode, oryggiEmployee.SecCode);
                        }

                        // Generate local password (NOT syncing from Oryggi for now)
                        // TODO: Future enhancement - sync password from Oryggi when encryption method is confirmed
                        var defaultPassword = !string.IsNullOrWhiteSpace(oryggiEmployee.CorpEmpCode)
                            ? oryggiEmployee.CorpEmpCode
                            : "Welcome@123";
                        var passwordHash = _encryptionService.EncryptPassword(defaultPassword);
                        _logger.LogDebug("Generated local encrypted password for employee {EmployeeCode}. Default password: {DefaultPassword}",
                            oryggiEmployee.CorpEmpCode, defaultPassword);

                        // Get job title from designation using lookup
                        string? jobTitle = null;
                        if (oryggiEmployee.DesigCode.HasValue)
                        {
                            designationLookup.TryGetValue(oryggiEmployee.DesigCode.Value, out jobTitle);
                        }

                        // Find manager by ReportingHeadEcode using lookup
                        Guid? managerId = null;
                        if (oryggiEmployee.ReportingHeadEcode.HasValue)
                        {
                            var managerEcode = oryggiEmployee.ReportingHeadEcode.Value.ToString();
                            if (userManagerLookup.TryGetValue(managerEcode, out var managerIdValue))
                            {
                                managerId = managerIdValue;
                            }
                            else
                            {
                                _logger.LogWarning("Manager with Ecode {ManagerEcode} not found for employee {EmployeeCode}. Continuing sync.",
                                    oryggiEmployee.ReportingHeadEcode.Value, oryggiEmployee.CorpEmpCode);
                            }
                        }

                        var newUser = new Domain.Entities.MasterData.User
                        {
                            CompanyId = companyId ?? Guid.Empty, // Use Empty Guid if no company
                            BranchId = branchId,
                            DepartmentId = departmentId,
                            SectionId = sectionId,
                            EmployeeCode = oryggiEmployee.CorpEmpCode ?? oryggiEmployee.Ecode.ToString(),
                            FirstName = oryggiEmployee.FName ?? "",
                            LastName = oryggiEmployee.LName ?? "",
                            Email = userEmail,
                            Phone = oryggiEmployee.Telephone1,
                            AlternatePhone = oryggiEmployee.Telephone2,
                            JobTitle = jobTitle,
                            DateOfJoining = oryggiEmployee.DateofJoin,
                            DateOfBirth = oryggiEmployee.DateofBirth,
                            ManagerId = managerId,
                            IsActive = oryggiEmployee.Active ?? true,
                            PasswordHash = passwordHash,
                            OryggiEmployeeId = oryggiEmployee.Ecode.ToString(),
                            LastSyncedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Try to save user - continue on error
                        try
                        {
                            await _appContext.Users.AddAsync(newUser);
                            await _appContext.SaveChangesAsync();
                            syncLog.UsersCreated++;
                            _logger.LogDebug("Created user account for employee {EmployeeCode} with email {Email}", newUser.EmployeeCode, userEmail);
                        }
                        catch (Exception userEx)
                        {
                            _logger.LogWarning(userEx, "Failed to create user for employee {EmployeeCode} (Ecode: {Ecode}). Error: {Error}",
                                oryggiEmployee.CorpEmpCode, oryggiEmployee.Ecode, userEx.Message);
                            syncLog.UsersFailed++;
                            _appContext.ChangeTracker.Clear();
                            // Don't continue - employee was created successfully, just user creation failed
                        }
                    }
                }
                else
                {
                    // Update existing employee - wrap in try-catch
                    try
                    {
                        existingEmployee.SectionId = sectionId;
                        existingEmployee.EmployeeCode = oryggiEmployee.CorpEmpCode ?? existingEmployee.EmployeeCode;
                        // Always update with Oryggi data - don't fall back to existing values
                        existingEmployee.FirstName = oryggiEmployee.FName ?? string.Empty;
                        existingEmployee.LastName = oryggiEmployee.LName ?? string.Empty;
                        existingEmployee.FullName = oryggiEmployee.EmpName ?? (oryggiEmployee.FName + " " + oryggiEmployee.LName).Trim();
                        existingEmployee.Email = oryggiEmployee.E_mail;
                        existingEmployee.Phone = oryggiEmployee.Telephone1;
                        existingEmployee.AlternatePhone = oryggiEmployee.Telephone2;
                        existingEmployee.DateOfJoining = oryggiEmployee.DateofJoin;
                        existingEmployee.DateOfBirth = oryggiEmployee.DateofBirth;
                        // If Oryggi says null, default to active (true)
                        existingEmployee.IsActive = oryggiEmployee.Active ?? true;
                        // Restore deleted employees when re-importing
                        existingEmployee.IsDeleted = false;
                        existingEmployee.DeletedAt = null;
                        existingEmployee.UpdatedAt = DateTime.UtcNow;

                        _appContext.Employees.Update(existingEmployee);
                        await _appContext.SaveChangesAsync();
                        syncLog.EmployeesUpdated++;
                    }
                    catch (Exception empEx)
                    {
                        _logger.LogWarning(empEx, "Failed to update employee {EmployeeCode} (Ecode: {Ecode}). Error: {Error}",
                            oryggiEmployee.CorpEmpCode, oryggiEmployee.Ecode, empEx.Message);
                        syncLog.EmployeesFailed++;
                        _appContext.ChangeTracker.Clear();
                        continue; // Skip to next employee
                    }

                    // Update corresponding User if exists (using lookup)
                    existingUserLookup.TryGetValue(oryggiEmpId, out var existingUser);

                    // If not found by OryggiEmployeeId, try fallback lookup by EmployeeCode or Email
                    // This handles cases where deleted users have null OryggiEmployeeId
                    if (existingUser == null)
                    {
                        var corpEmpCode = oryggiEmployee.CorpEmpCode;
                        var expectedEmail = !string.IsNullOrWhiteSpace(oryggiEmployee.E_mail) && !oryggiEmployee.E_mail.Equals("NA", StringComparison.OrdinalIgnoreCase)
                            ? oryggiEmployee.E_mail
                            : $"{(!string.IsNullOrWhiteSpace(corpEmpCode) ? corpEmpCode : oryggiEmpId)}@system.local";

                        // Try to find by EmployeeCode or Email (including deleted users)
                        existingUser = await _appContext.Users
                            .IgnoreQueryFilters() // Include soft-deleted records
                            .FirstOrDefaultAsync(u => u.EmployeeCode == corpEmpCode || u.Email == expectedEmail);

                        if (existingUser != null)
                        {
                            _logger.LogInformation("Found existing user by fallback lookup (Email/EmployeeCode) for employee {EmployeeCode}, updating OryggiEmployeeId to {OryggiEmpId}",
                                corpEmpCode, oryggiEmpId);
                            // Update the OryggiEmployeeId so future lookups work
                            existingUser.OryggiEmployeeId = oryggiEmpId;
                        }
                    }

                    if (existingUser != null)
                    {
                        // Try to update user - continue on error
                        try
                        {
                            // Update user details
                            if (companyId.HasValue)
                                existingUser.CompanyId = companyId.Value;
                            existingUser.BranchId = branchId;
                            existingUser.DepartmentId = departmentId;
                            existingUser.SectionId = sectionId;
                            existingUser.EmployeeCode = oryggiEmployee.CorpEmpCode ?? existingUser.EmployeeCode;
                            // Always update with Oryggi data - don't fall back to existing values
                            existingUser.FirstName = oryggiEmployee.FName ?? string.Empty;
                            existingUser.LastName = oryggiEmployee.LName ?? string.Empty;
                            // Generate email if null
                            if (!string.IsNullOrWhiteSpace(oryggiEmployee.E_mail) && !oryggiEmployee.E_mail.Equals("NA", StringComparison.OrdinalIgnoreCase))
                            {
                                existingUser.Email = oryggiEmployee.E_mail;
                            }
                            else
                            {
                                // Keep existing email if Oryggi has none, or generate system email if none exists
                                if (string.IsNullOrWhiteSpace(existingUser.Email))
                                {
                                    var empCode = !string.IsNullOrWhiteSpace(oryggiEmployee.CorpEmpCode)
                                        ? oryggiEmployee.CorpEmpCode
                                        : oryggiEmployee.Ecode.ToString();
                                    existingUser.Email = $"{empCode}@system.local";
                                }
                            }
                            existingUser.Phone = oryggiEmployee.Telephone1;
                            existingUser.AlternatePhone = oryggiEmployee.Telephone2;
                            existingUser.DateOfJoining = oryggiEmployee.DateofJoin;
                            existingUser.DateOfBirth = oryggiEmployee.DateofBirth;
                            // If Oryggi says null, default to active (true)
                            existingUser.IsActive = oryggiEmployee.Active ?? true;
                            // Restore deleted users when re-importing
                            existingUser.IsDeleted = false;
                            existingUser.DeletedAt = null;
                            existingUser.UpdatedAt = DateTime.UtcNow;

                            // Update job title from designation using lookup
                            if (oryggiEmployee.DesigCode.HasValue && designationLookup.TryGetValue(oryggiEmployee.DesigCode.Value, out var jobTitle))
                            {
                                existingUser.JobTitle = jobTitle;
                            }

                            // Update manager by ReportingHeadEcode using lookup
                            if (oryggiEmployee.ReportingHeadEcode.HasValue)
                            {
                                var managerEcode = oryggiEmployee.ReportingHeadEcode.Value.ToString();
                                if (userManagerLookup.TryGetValue(managerEcode, out var managerIdValue))
                                {
                                    existingUser.ManagerId = managerIdValue;
                                }
                                else
                                {
                                    _logger.LogWarning("Manager with Ecode {ManagerEcode} not found for employee {EmployeeCode}. Continuing sync.",
                                        oryggiEmployee.ReportingHeadEcode.Value, oryggiEmployee.CorpEmpCode);
                                    existingUser.ManagerId = null;
                                }
                            }
                            else
                            {
                                existingUser.ManagerId = null;
                            }

                            // Password update skipped - NOT syncing passwords from Oryggi for now
                            // TODO: Future enhancement - sync password from Oryggi when encryption method is confirmed
                            // Existing password remains unchanged during sync

                            existingUser.LastSyncedAt = DateTime.UtcNow;

                            _logger.LogInformation("Updating user {UserId} ({EmployeeCode}): FirstName={FirstName}, LastName={LastName}, IsActive={IsActive}, IsDeleted={IsDeleted}",
                                existingUser.Id, existingUser.EmployeeCode, existingUser.FirstName, existingUser.LastName, existingUser.IsActive, existingUser.IsDeleted);

                            _appContext.Users.Update(existingUser);
                            await _appContext.SaveChangesAsync();
                            syncLog.UsersUpdated++;

                            _logger.LogInformation("Successfully updated user {UserId} ({EmployeeCode})", existingUser.Id, existingUser.EmployeeCode);
                        }
                        catch (Exception userEx)
                        {
                            _logger.LogError(userEx, "Failed to update user for employee {EmployeeCode} (Ecode: {Ecode}). " +
                                "Error: {Error}. Inner Exception: {InnerException}. Stack Trace: {StackTrace}",
                                oryggiEmployee.CorpEmpCode, oryggiEmployee.Ecode, userEx.Message,
                                userEx.InnerException?.Message ?? "None", userEx.StackTrace);
                            syncLog.UsersFailed++;
                            _appContext.ChangeTracker.Clear();
                            // Don't continue - employee was updated successfully, just user update failed
                        }
                    }
                    else
                    {
                        // Create user if doesn't exist
                        // Generate unique email if E_mail is null, empty, or "NA"
                        var userEmail = oryggiEmployee.E_mail;
                        if (string.IsNullOrWhiteSpace(userEmail) || userEmail.Equals("NA", StringComparison.OrdinalIgnoreCase))
                        {
                            // Use CorpEmpCode if available, otherwise use Ecode (which is always unique)
                            var empCode = !string.IsNullOrWhiteSpace(oryggiEmployee.CorpEmpCode)
                                ? oryggiEmployee.CorpEmpCode
                                : oryggiEmployee.Ecode.ToString();
                            userEmail = $"{empCode}@system.local";
                        }

                        // Log if company is missing
                        if (!companyId.HasValue)
                        {
                            _logger.LogWarning("Existing employee {Ecode} has no company in hierarchy. SecCode={SecCode}, Creating user without company.",
                                oryggiEmployee.Ecode, oryggiEmployee.SecCode);
                        }

                        // Generate local password (NOT syncing from Oryggi for now)
                        // TODO: Future enhancement - sync password from Oryggi when encryption method is confirmed
                        var defaultPassword = !string.IsNullOrWhiteSpace(oryggiEmployee.CorpEmpCode)
                            ? oryggiEmployee.CorpEmpCode
                            : "Welcome@123";
                        var passwordHash = _encryptionService.EncryptPassword(defaultPassword);
                        _logger.LogDebug("Generated local encrypted password for employee {EmployeeCode}. Default password: {DefaultPassword}",
                            oryggiEmployee.CorpEmpCode, defaultPassword);

                        // Get job title from designation using lookup
                        string? jobTitle2 = null;
                        if (oryggiEmployee.DesigCode.HasValue)
                        {
                            designationLookup.TryGetValue(oryggiEmployee.DesigCode.Value, out jobTitle2);
                        }

                        // Find manager by ReportingHeadEcode using lookup
                        Guid? managerId2 = null;
                        if (oryggiEmployee.ReportingHeadEcode.HasValue)
                        {
                            var managerEcode = oryggiEmployee.ReportingHeadEcode.Value.ToString();
                            if (userManagerLookup.TryGetValue(managerEcode, out var managerIdValue))
                            {
                                managerId2 = managerIdValue;
                            }
                            else
                            {
                                _logger.LogWarning("Manager with Ecode {ManagerEcode} not found for employee {EmployeeCode}. Continuing sync.",
                                    oryggiEmployee.ReportingHeadEcode.Value, oryggiEmployee.CorpEmpCode);
                            }
                        }

                        var newUser = new Domain.Entities.MasterData.User
                        {
                            CompanyId = companyId ?? Guid.Empty, // Use Empty Guid if no company
                            BranchId = branchId,
                            DepartmentId = departmentId,
                            SectionId = sectionId,
                            EmployeeCode = oryggiEmployee.CorpEmpCode ?? oryggiEmployee.Ecode.ToString(),
                            FirstName = oryggiEmployee.FName ?? "",
                            LastName = oryggiEmployee.LName ?? "",
                            Email = userEmail,
                            Phone = oryggiEmployee.Telephone1,
                            AlternatePhone = oryggiEmployee.Telephone2,
                            JobTitle = jobTitle2,
                            DateOfJoining = oryggiEmployee.DateofJoin,
                            DateOfBirth = oryggiEmployee.DateofBirth,
                            ManagerId = managerId2,
                            IsActive = oryggiEmployee.Active ?? true,
                            PasswordHash = passwordHash,
                            OryggiEmployeeId = oryggiEmployee.Ecode.ToString(),
                            LastSyncedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Try to save user - continue on error
                        try
                        {
                            await _appContext.Users.AddAsync(newUser);
                            await _appContext.SaveChangesAsync();
                            syncLog.UsersCreated++;
                            _logger.LogDebug("Created user account for existing employee {EmployeeCode} with email {Email}", newUser.EmployeeCode, userEmail);
                        }
                        catch (Exception userEx)
                        {
                            _logger.LogWarning(userEx, "Failed to create user for existing employee {EmployeeCode} (Ecode: {Ecode}). Error: {Error}",
                                oryggiEmployee.CorpEmpCode, oryggiEmployee.Ecode, userEx.Message);
                            syncLog.UsersFailed++;
                            _appContext.ChangeTracker.Clear();
                            // Don't continue - employee was updated successfully, just user creation failed
                        }
                    }
                }
            }

            // NOTE: Batch SaveChangesAsync removed - all saves now happen individually within try-catch blocks
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing employee batch");
            throw;
        }
    }

    public async Task<List<SyncLog>> GetSyncHistoryAsync(Guid tenantId, int count = 10)
    {
        return await _appContext.SyncLogs
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.SyncStartedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<SyncLog?> GetLatestSyncAsync(Guid tenantId)
    {
        return await _appContext.SyncLogs
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.SyncStartedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Manually sync a single employee by their CorpEmpCode
    /// </summary>
    public async Task<(bool Success, string Message, SyncLog? SyncLog)> SyncSingleEmployeeAsync(Guid tenantId, string corpEmpCode)
    {
        _logger.LogInformation("Starting manual sync for employee with CorpEmpCode {CorpEmpCode}", corpEmpCode);

        // Create a sync log for this manual sync
        var syncLog = new SyncLog
        {
            TenantId = tenantId,
            SyncType = "MANUAL",
            SyncStartedAt = DateTime.UtcNow,
            Status = "IN_PROGRESS"
        };

        try
        {
            // Find the employee in Oryggi database
            var oryggiEmployee = await _oryggiContext.EmployeeMaster
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CorpEmpCode == corpEmpCode);

            if (oryggiEmployee == null)
            {
                var message = $"Employee with CorpEmpCode '{corpEmpCode}' not found in Oryggi database";
                _logger.LogWarning(message);
                return (false, message, null);
            }

            // Check if employee meets sync criteria
            if (oryggiEmployee.Ecode == 1)
            {
                var message = "Cannot sync admin account (Ecode=1)";
                _logger.LogWarning(message);
                return (false, message, null);
            }

            if (!string.IsNullOrEmpty(oryggiEmployee.CorpEmpCode) && oryggiEmployee.CorpEmpCode.Contains("_"))
            {
                var message = $"Cannot sync non-employee record (CorpEmpCode contains underscore): {corpEmpCode}";
                _logger.LogWarning(message);
                return (false, message, null);
            }

            if (!string.IsNullOrEmpty(oryggiEmployee.Role) && oryggiEmployee.Role == "Visitor")
            {
                var message = $"Cannot sync visitor account: {corpEmpCode}";
                _logger.LogWarning(message);
                return (false, message, null);
            }

            await _appContext.SyncLogs.AddAsync(syncLog);
            await _appContext.SaveChangesAsync();

            // Sync this single employee using the batch processing method
            var employees = new List<EmployeeMaster> { oryggiEmployee };
            await ProcessEmployeeBatch(employees, syncLog);

            syncLog.SyncCompletedAt = DateTime.UtcNow;
            syncLog.Duration = syncLog.SyncCompletedAt.Value - syncLog.SyncStartedAt;

            var totalProcessed = syncLog.EmployeesCreated + syncLog.EmployeesUpdated + syncLog.EmployeesFailed;
            var totalUserProcessed = syncLog.UsersCreated + syncLog.UsersUpdated + syncLog.UsersFailed;

            if (syncLog.EmployeesFailed > 0 || syncLog.UsersFailed > 0)
            {
                syncLog.Status = "PARTIAL_SUCCESS";
                var message = $"Employee sync completed with warnings. Employee: {(syncLog.EmployeesFailed > 0 ? "Failed" : "Success")}, User: {(syncLog.UsersFailed > 0 ? "Failed" : "Success")}";
                _logger.LogWarning(message);

                _appContext.SyncLogs.Update(syncLog);
                await _appContext.SaveChangesAsync();

                return (true, message, syncLog);
            }
            else
            {
                syncLog.Status = "SUCCESS";
                var message = $"Employee '{corpEmpCode}' synced successfully. Created: {syncLog.EmployeesCreated > 0}, Updated: {syncLog.EmployeesUpdated > 0}";
                _logger.LogInformation(message);

                _appContext.SyncLogs.Update(syncLog);
                await _appContext.SaveChangesAsync();

                return (true, message, syncLog);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing employee with CorpEmpCode {CorpEmpCode}", corpEmpCode);

            syncLog.SyncCompletedAt = DateTime.UtcNow;
            syncLog.Duration = syncLog.SyncCompletedAt.Value - syncLog.SyncStartedAt;
            syncLog.Status = "FAILED";
            syncLog.ErrorMessage = ex.Message;
            syncLog.ErrorDetails = ex.ToString();

            _appContext.SyncLogs.Update(syncLog);
            await _appContext.SaveChangesAsync();

            return (false, $"Sync failed: {ex.Message}", syncLog);
        }
    }
}
