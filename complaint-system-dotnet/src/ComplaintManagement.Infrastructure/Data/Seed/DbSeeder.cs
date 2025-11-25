using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Events;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Data.Seed;

public class DbSeeder
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<DbSeeder> _logger;
    private readonly IEncryptionService _encryptionService;

    public DbSeeder(ComplaintDbContext context, ILogger<DbSeeder> logger, IEncryptionService encryptionService)
    {
        _context = context;
        _logger = logger;
        _encryptionService = encryptionService;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            await SeedRolesAsync();
            await SeedRolePermissionsAsync();
            await SeedTenantAndCompanyAsync();
            await SeedAdminUserAsync();
            await SeedCategoriesAsync();
            await SeedEventTypesAsync();
            await SeedCommunicationTemplatesAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        if (await _context.ComplaintRoles.AnyAsync())
        {
            _logger.LogInformation("Roles already exist. Skipping role seeding.");
            return;
        }

        _logger.LogInformation("Seeding roles...");

        var roles = new List<ComplaintRole>
        {
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "System Administrator",
                Code = "SYSTEM_ADMIN",
                Description = "Full system access across all tenants and companies",
                RoleType = RoleType.SystemAdmin,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Tenant Administrator",
                Code = "TENANT_ADMIN",
                Description = "Full access within tenant scope",
                RoleType = RoleType.TenantAdmin,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Company Administrator",
                Code = "COMPANY_ADMIN",
                Description = "Full access within company scope",
                RoleType = RoleType.CompanyAdmin,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Complainant",
                Code = "COMPLAINANT",
                Description = "Can submit and track own complaints",
                RoleType = RoleType.Complainant,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Level 1 Handler",
                Code = "LEVEL1_HANDLER",
                Description = "First level complaint handler",
                RoleType = RoleType.Level1Handler,
                EscalationLevel = 1,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Level 2 Handler",
                Code = "LEVEL2_HANDLER",
                Description = "Second level complaint handler",
                RoleType = RoleType.Level2Handler,
                EscalationLevel = 2,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 6,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Level 3 Handler",
                Code = "LEVEL3_HANDLER",
                Description = "Third level complaint handler",
                RoleType = RoleType.Level3Handler,
                EscalationLevel = 3,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 7,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Level 4 Handler",
                Code = "LEVEL4_HANDLER",
                Description = "Fourth level complaint handler",
                RoleType = RoleType.Level4Handler,
                EscalationLevel = 4,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 8,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Level 5 Handler",
                Code = "LEVEL5_HANDLER",
                Description = "Fifth level complaint handler (highest)",
                RoleType = RoleType.Level5Handler,
                EscalationLevel = 5,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 9,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Viewer",
                Code = "VIEWER",
                Description = "Read-only access to complaints",
                RoleType = RoleType.Viewer,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 10,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "HR Representative",
                Code = "HR_REP",
                Description = "HR department representative",
                RoleType = RoleType.HRRepresentative,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 11,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Department Manager",
                Code = "DEPT_MANAGER",
                Description = "Department level manager",
                RoleType = RoleType.DepartmentManager,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 12,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Reporting Manager",
                Code = "REPORTING_MANAGER",
                Description = "Team lead or direct supervisor who manages team members",
                RoleType = RoleType.ReportingManager,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 13,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Primary Contact",
                Code = "PRIMARY_CONTACT",
                Description = "Main point of contact for branch/department complaint handling",
                RoleType = RoleType.PrimaryContact,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 14,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = "Secondary Contact",
                Code = "SECONDARY_CONTACT",
                Description = "Backup contact for branch/department when primary is unavailable",
                RoleType = RoleType.SecondaryContact,
                EscalationLevel = 0,
                IsSystemRole = true,
                IsActive = true,
                DisplayOrder = 15,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.ComplaintRoles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {roles.Count} roles.");
    }

    private async Task SeedRolePermissionsAsync()
    {
        if (await _context.ComplaintRolePermissions.AnyAsync())
        {
            _logger.LogInformation("Role permissions already exist. Skipping permission seeding.");
            return;
        }

        _logger.LogInformation("Seeding role permissions...");

        var roles = await _context.ComplaintRoles.ToListAsync();
        var permissions = new List<ComplaintRolePermission>();

        // System Admin - All permissions
        var systemAdminRole = roles.First(r => r.Code == "SYSTEM_ADMIN");
        foreach (PermissionType permission in Enum.GetValues(typeof(PermissionType)))
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = systemAdminRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Tenant Admin - Most permissions
        var tenantAdminRole = roles.First(r => r.Code == "TENANT_ADMIN");
        var tenantAdminPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.CreateComplaint,
            PermissionType.EditComplaint, PermissionType.AssignComplaint,
            PermissionType.EscalateComplaint, PermissionType.CloseComplaint,
            PermissionType.ReopenComplaint, PermissionType.AddComment,
            PermissionType.ViewComments, PermissionType.AddAttachment,
            PermissionType.ViewAttachments, PermissionType.ManageCategories,
            PermissionType.ManageRoles, PermissionType.ViewReports,
            PermissionType.ManageSettings, PermissionType.ManageUsers,
            PermissionType.ViewAuditLogs,
            PermissionType.ViewEscalation, PermissionType.ManageEscalation
        };
        foreach (var permission in tenantAdminPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = tenantAdminRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Company Admin - Company-level permissions
        var companyAdminRole = roles.First(r => r.Code == "COMPANY_ADMIN");
        var companyAdminPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.CreateComplaint,
            PermissionType.EditComplaint, PermissionType.AssignComplaint,
            PermissionType.EscalateComplaint, PermissionType.CloseComplaint,
            PermissionType.ReopenComplaint, PermissionType.AddComment,
            PermissionType.ViewComments, PermissionType.AddAttachment,
            PermissionType.ViewAttachments, PermissionType.ManageCategories,
            PermissionType.ViewReports, PermissionType.ManageUsers,
            PermissionType.ViewEscalation, PermissionType.ManageEscalation
        };
        foreach (var permission in companyAdminPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = companyAdminRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Complainant - Basic permissions
        var complainantRole = roles.First(r => r.Code == "COMPLAINANT");
        var complainantPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.CreateComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.AddAttachment, PermissionType.ViewAttachments
        };
        foreach (var permission in complainantPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = complainantRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Level 1-5 Handlers - Handler permissions
        var handlerCodes = new[] { "LEVEL1_HANDLER", "LEVEL2_HANDLER", "LEVEL3_HANDLER", "LEVEL4_HANDLER", "LEVEL5_HANDLER" };
        var handlerPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.EditComplaint,
            PermissionType.AssignComplaint, PermissionType.EscalateComplaint,
            PermissionType.CloseComplaint, PermissionType.ReopenComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.AddAttachment, PermissionType.ViewAttachments
        };
        foreach (var handlerCode in handlerCodes)
        {
            var handlerRole = roles.First(r => r.Code == handlerCode);
            foreach (var permission in handlerPermissions)
            {
                permissions.Add(new ComplaintRolePermission
                {
                    Id = Guid.NewGuid(),
                    ComplaintRoleId = handlerRole.Id,
                    PermissionType = permission,
                    IsGranted = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Viewer - Read-only permissions
        var viewerRole = roles.First(r => r.Code == "VIEWER");
        var viewerPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.ViewComments,
            PermissionType.ViewAttachments
        };
        foreach (var permission in viewerPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = viewerRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // HR Representative - HR-specific permissions
        var hrRepRole = roles.First(r => r.Code == "HR_REP");
        var hrRepPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.EditComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.ViewAttachments, PermissionType.ViewReports,
            PermissionType.ViewAuditLogs
        };
        foreach (var permission in hrRepPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = hrRepRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Department Manager - Manager permissions
        var deptManagerRole = roles.First(r => r.Code == "DEPT_MANAGER");
        var deptManagerPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.EditComplaint,
            PermissionType.AssignComplaint, PermissionType.EscalateComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.ViewAttachments, PermissionType.ViewReports,
            PermissionType.ViewEscalation
        };
        foreach (var permission in deptManagerPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = deptManagerRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Reporting Manager - Team lead permissions
        var reportingManagerRole = roles.First(r => r.Code == "REPORTING_MANAGER");
        var reportingManagerPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.EditComplaint,
            PermissionType.AssignComplaint, PermissionType.EscalateComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.ViewAttachments, PermissionType.ViewReports
        };
        foreach (var permission in reportingManagerPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = reportingManagerRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Primary Contact - Contact permissions
        var primaryContactRole = roles.First(r => r.Code == "PRIMARY_CONTACT");
        var primaryContactPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.CreateComplaint,
            PermissionType.EditComplaint, PermissionType.AssignComplaint,
            PermissionType.EscalateComplaint, PermissionType.CloseComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.AddAttachment, PermissionType.ViewAttachments,
            PermissionType.ViewEscalation
        };
        foreach (var permission in primaryContactPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = primaryContactRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Secondary Contact - Same as Primary Contact
        var secondaryContactRole = roles.First(r => r.Code == "SECONDARY_CONTACT");
        var secondaryContactPermissions = new[]
        {
            PermissionType.ViewComplaints, PermissionType.CreateComplaint,
            PermissionType.EditComplaint, PermissionType.AssignComplaint,
            PermissionType.AddComment, PermissionType.ViewComments,
            PermissionType.AddAttachment, PermissionType.ViewAttachments,
            PermissionType.ViewEscalation
        };
        foreach (var permission in secondaryContactPermissions)
        {
            permissions.Add(new ComplaintRolePermission
            {
                Id = Guid.NewGuid(),
                ComplaintRoleId = secondaryContactRole.Id,
                PermissionType = permission,
                IsGranted = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.ComplaintRolePermissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {permissions.Count} role permissions.");
    }

    private async Task SeedTenantAndCompanyAsync()
    {
        if (await _context.Tenants.AnyAsync())
        {
            _logger.LogInformation("Tenants already exist. Skipping tenant seeding.");
            return;
        }

        _logger.LogInformation("Seeding initial tenant and company...");

        var tenantId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Default Tenant",
            Code = "DEFAULT",
            Description = "Default tenant for initial setup",
            ContactEmail = "admin@defaulttenant.com",
            ContactPhone = "+1234567890",
            Address = "123 Default Street",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var company = new Company
        {
            Id = companyId,
            TenantId = tenantId,
            Name = "Default Company",
            Code = "DEFAULT_CO",
            Description = "Default company for initial setup",
            ContactEmail = "admin@defaultcompany.com",
            ContactPhone = "+1234567890",
            Address = "456 Company Avenue",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Tenants.AddAsync(tenant);
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded initial tenant and company.");
    }

    private async Task SeedAdminUserAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist. Skipping admin user seeding.");
            return;
        }

        _logger.LogInformation("Seeding initial admin user...");

        var company = await _context.Companies.FirstAsync();
        var systemAdminRole = await _context.ComplaintRoles.FirstAsync(r => r.Code == "SYSTEM_ADMIN");

        var adminUserId = Guid.NewGuid();

        // Encrypt password using AES encryption service
        var passwordHash = _encryptionService.EncryptPassword("Admin@123");

        var adminUser = new User
        {
            Id = adminUserId,
            CompanyId = company.Id,
            EmployeeCode = "ADMIN001",
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@complaintmanagement.com",
            Phone = "+1234567890",
            JobTitle = "System Administrator",
            IsActive = true,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        var userRole = new UserComplaintRole
        {
            Id = Guid.NewGuid(),
            UserId = adminUserId,
            ComplaintRoleId = systemAdminRole.Id,
            EffectiveFrom = DateTime.UtcNow,
            IsPrimary = true,
            IsActive = true,
            Notes = "Initial system administrator",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(adminUser);
        await _context.UserComplaintRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded initial admin user. Email: admin@complaintmanagement.com, Password: Admin@123");
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.ComplaintCategories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        _logger.LogInformation("Seeding complaint categories...");

        var categories = new List<ComplaintCategory>
        {
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Attendance Issues",
                Code = "ATTENDANCE",
                Description = "Issues related to attendance tracking, check-in/check-out problems",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Salary & Payroll",
                Code = "SALARY",
                Description = "Issues related to salary processing, deductions, or payment delays",
                DefaultPriority = 2, // High
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "HRMS System",
                Code = "HRMS",
                Description = "Technical issues with HRMS system access or functionality",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Leave Management",
                Code = "LEAVE",
                Description = "Issues with leave applications, approvals, or leave balance",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Performance Management",
                Code = "PERFORMANCE",
                Description = "Issues related to performance reviews, goals, or ratings",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Benefits & Insurance",
                Code = "BENEFITS",
                Description = "Issues related to employee benefits, insurance claims, or coverage",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 6,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Workplace Harassment",
                Code = "HARASSMENT",
                Description = "Reports of workplace harassment or inappropriate behavior",
                DefaultPriority = 3, // Critical
                IsActive = true,
                DisplayOrder = 7,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "IT & Technical Support",
                Code = "IT_SUPPORT",
                Description = "IT-related issues including hardware, software, or network problems",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 8,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Facilities & Infrastructure",
                Code = "FACILITIES",
                Description = "Issues with office facilities, equipment, or infrastructure",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 9,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Policy & Compliance",
                Code = "POLICY",
                Description = "Questions or concerns about company policies and compliance",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 10,
                CreatedAt = DateTime.UtcNow
            },
            new ComplaintCategory
            {
                Id = Guid.NewGuid(),
                Name = "Other",
                Code = "OTHER",
                Description = "Other complaints not covered by specific categories",
                DefaultPriority = 1, // Normal
                IsActive = true,
                DisplayOrder = 11,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.ComplaintCategories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {categories.Count} complaint categories.");
    }

    private async Task SeedEventTypesAsync()
    {
        if (await _context.EventTypes.AnyAsync())
        {
            _logger.LogInformation("Event types already exist. Skipping event type seeding.");
            return;
        }

        _logger.LogInformation("Seeding event types...");

        var availableFieldsComplaint = new List<string>
        {
            "complaintId", "complaintNumber", "title", "description",
            "categoryName", "priorityName", "statusName",
            "complainantName", "complainantEmail", "complainantEmployeeCode",
            "assignedToName", "assignedToEmail",
            "createdDate", "dueDate", "companyName"
        };

        var eventTypes = new List<EventType>
        {
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_CREATED",
                Name = "Complaint Created",
                EntityType = "Complaint",
                Description = "Triggered when a new complaint is submitted",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_ASSIGNED",
                Name = "Complaint Assigned",
                EntityType = "Complaint",
                Description = "Triggered when a complaint is assigned to a handler",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_STATUS_CHANGED",
                Name = "Complaint Status Changed",
                EntityType = "Complaint",
                Description = "Triggered when complaint status is updated",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "oldStatus", "newStatus", "changedBy" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_ESCALATED",
                Name = "Complaint Escalated",
                EntityType = "Complaint",
                Description = "Triggered when a complaint is escalated",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "escalationLevel", "escalatedTo", "escalationReason" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_COMMENTED",
                Name = "Complaint Commented",
                EntityType = "Complaint",
                Description = "Triggered when a new comment is added to a complaint",
                Category = "Complaint Activity",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "commentText", "commentedBy", "commentDate" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_CLOSED",
                Name = "Complaint Closed",
                EntityType = "Complaint",
                Description = "Triggered when a complaint is closed/resolved",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "closedBy", "closedDate", "resolution" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_REOPENED",
                Name = "Complaint Reopened",
                EntityType = "Complaint",
                Description = "Triggered when a closed complaint is reopened",
                Category = "Complaint Lifecycle",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "reopenedBy", "reopenedDate", "reopenReason" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_DUE_SOON",
                Name = "Complaint Due Soon",
                EntityType = "Complaint",
                Description = "Triggered when complaint due date is approaching",
                Category = "Complaint Alerts",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "hoursRemaining", "daysRemaining" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new EventType
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_OVERDUE",
                Name = "Complaint Overdue",
                EntityType = "Complaint",
                Description = "Triggered when complaint is past due date",
                Category = "Complaint Alerts",
                AvailableFields = JsonSerializer.Serialize(availableFieldsComplaint
                    .Concat(new[] { "hoursOverdue", "daysOverdue" }).ToList()),
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.EventTypes.AddRangeAsync(eventTypes);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {eventTypes.Count} event types.");
    }

    private async Task SeedCommunicationTemplatesAsync()
    {
        if (await _context.CommunicationTemplates.AnyAsync())
        {
            _logger.LogInformation("Communication templates already exist. Skipping template seeding.");
            return;
        }

        _logger.LogInformation("Seeding communication templates...");

        var templates = new List<CommunicationTemplate>
        {
            new CommunicationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_CREATED_EMAIL",
                Name = "Complaint Created - Email",
                Description = "Email template for new complaint creation",
                Channel = CommunicationChannel.Email,
                Subject = "Complaint #{{complaintNumber}} Created - {{title}}",
                Body = @"Dear {{complainantName}},

Your complaint has been successfully submitted.

Complaint Details:
- Number: {{complaintNumber}}
- Title: {{title}}
- Category: {{categoryName}}
- Priority: {{priorityName}}
- Status: {{statusName}}
- Submitted: {{createdDate}}

We will review your complaint and assign it to an appropriate handler soon.

Thank you,
{{companyName}} Support Team",
                HtmlBody = @"<html>
<body>
    <h2>Complaint Created</h2>
    <p>Dear {{complainantName}},</p>
    <p>Your complaint has been successfully submitted.</p>

    <h3>Complaint Details:</h3>
    <ul>
        <li><strong>Number:</strong> {{complaintNumber}}</li>
        <li><strong>Title:</strong> {{title}}</li>
        <li><strong>Category:</strong> {{categoryName}}</li>
        <li><strong>Priority:</strong> {{priorityName}}</li>
        <li><strong>Status:</strong> {{statusName}}</li>
        <li><strong>Submitted:</strong> {{createdDate}}</li>
    </ul>

    <p>We will review your complaint and assign it to an appropriate handler soon.</p>

    <p>Thank you,<br/>{{companyName}} Support Team</p>
</body>
</html>",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new CommunicationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_ASSIGNED_EMAIL",
                Name = "Complaint Assigned - Email",
                Description = "Email template for complaint assignment",
                Channel = CommunicationChannel.Email,
                Subject = "Complaint #{{complaintNumber}} Assigned to You",
                Body = @"Dear {{assignedToName}},

A complaint has been assigned to you for handling.

Complaint Details:
- Number: {{complaintNumber}}
- Title: {{title}}
- Category: {{categoryName}}
- Priority: {{priorityName}}
- Status: {{statusName}}
- Complainant: {{complainantName}} ({{complainantEmployeeCode}})
- Due Date: {{dueDate}}

Please review and take appropriate action.

Thank you,
{{companyName}} Support Team",
                HtmlBody = @"<html>
<body>
    <h2>Complaint Assigned</h2>
    <p>Dear {{assignedToName}},</p>
    <p>A complaint has been assigned to you for handling.</p>

    <h3>Complaint Details:</h3>
    <ul>
        <li><strong>Number:</strong> {{complaintNumber}}</li>
        <li><strong>Title:</strong> {{title}}</li>
        <li><strong>Category:</strong> {{categoryName}}</li>
        <li><strong>Priority:</strong> {{priorityName}}</li>
        <li><strong>Status:</strong> {{statusName}}</li>
        <li><strong>Complainant:</strong> {{complainantName}} ({{complainantEmployeeCode}})</li>
        <li><strong>Due Date:</strong> {{dueDate}}</li>
    </ul>

    <p>Please review and take appropriate action.</p>

    <p>Thank you,<br/>{{companyName}} Support Team</p>
</body>
</html>",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new CommunicationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_CLOSED_EMAIL",
                Name = "Complaint Closed - Email",
                Description = "Email template for complaint closure",
                Channel = CommunicationChannel.Email,
                Subject = "Complaint #{{complaintNumber}} Closed",
                Body = @"Dear {{complainantName}},

Your complaint has been closed.

Complaint Details:
- Number: {{complaintNumber}}
- Title: {{title}}
- Category: {{categoryName}}
- Closed By: {{closedBy}}
- Closed Date: {{closedDate}}
- Resolution: {{resolution}}

If you have any further concerns, please feel free to reopen this complaint or submit a new one.

Thank you,
{{companyName}} Support Team",
                HtmlBody = @"<html>
<body>
    <h2>Complaint Closed</h2>
    <p>Dear {{complainantName}},</p>
    <p>Your complaint has been closed.</p>

    <h3>Complaint Details:</h3>
    <ul>
        <li><strong>Number:</strong> {{complaintNumber}}</li>
        <li><strong>Title:</strong> {{title}}</li>
        <li><strong>Category:</strong> {{categoryName}}</li>
        <li><strong>Closed By:</strong> {{closedBy}}</li>
        <li><strong>Closed Date:</strong> {{closedDate}}</li>
        <li><strong>Resolution:</strong> {{resolution}}</li>
    </ul>

    <p>If you have any further concerns, please feel free to reopen this complaint or submit a new one.</p>

    <p>Thank you,<br/>{{companyName}} Support Team</p>
</body>
</html>",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new CommunicationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_ESCALATED_EMAIL",
                Name = "Complaint Escalated - Email",
                Description = "Email template for complaint escalation",
                Channel = CommunicationChannel.Email,
                Subject = "Complaint #{{complaintNumber}} Escalated to Level {{escalationLevel}}",
                Body = @"Dear {{escalatedTo}},

A complaint has been escalated to your level for resolution.

Complaint Details:
- Number: {{complaintNumber}}
- Title: {{title}}
- Category: {{categoryName}}
- Priority: {{priorityName}}
- Escalation Level: {{escalationLevel}}
- Reason: {{escalationReason}}
- Complainant: {{complainantName}}

This requires urgent attention. Please review and take appropriate action.

Thank you,
{{companyName}} Support Team",
                HtmlBody = @"<html>
<body>
    <h2 style=""color: #d9534f;"">Complaint Escalated</h2>
    <p>Dear {{escalatedTo}},</p>
    <p>A complaint has been escalated to your level for resolution.</p>

    <h3>Complaint Details:</h3>
    <ul>
        <li><strong>Number:</strong> {{complaintNumber}}</li>
        <li><strong>Title:</strong> {{title}}</li>
        <li><strong>Category:</strong> {{categoryName}}</li>
        <li><strong>Priority:</strong> {{priorityName}}</li>
        <li><strong>Escalation Level:</strong> {{escalationLevel}}</li>
        <li><strong>Reason:</strong> {{escalationReason}}</li>
        <li><strong>Complainant:</strong> {{complainantName}}</li>
    </ul>

    <p style=""color: #d9534f; font-weight: bold;"">This requires urgent attention. Please review and take appropriate action.</p>

    <p>Thank you,<br/>{{companyName}} Support Team</p>
</body>
</html>",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            },
            new CommunicationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMPLAINT_OVERDUE_EMAIL",
                Name = "Complaint Overdue - Email",
                Description = "Email template for overdue complaint alert",
                Channel = CommunicationChannel.Email,
                Subject = "URGENT: Complaint #{{complaintNumber}} is Overdue",
                Body = @"Dear {{assignedToName}},

Complaint #{{complaintNumber}} is now overdue.

Complaint Details:
- Number: {{complaintNumber}}
- Title: {{title}}
- Priority: {{priorityName}}
- Due Date: {{dueDate}}
- Days Overdue: {{daysOverdue}}

Please take immediate action to resolve this complaint.

Thank you,
{{companyName}} Support Team",
                HtmlBody = @"<html>
<body>
    <h2 style=""color: #d9534f;"">URGENT: Complaint Overdue</h2>
    <p>Dear {{assignedToName}},</p>
    <p style=""color: #d9534f; font-weight: bold;"">Complaint #{{complaintNumber}} is now overdue.</p>

    <h3>Complaint Details:</h3>
    <ul>
        <li><strong>Number:</strong> {{complaintNumber}}</li>
        <li><strong>Title:</strong> {{title}}</li>
        <li><strong>Priority:</strong> {{priorityName}}</li>
        <li><strong>Due Date:</strong> {{dueDate}}</li>
        <li><strong>Days Overdue:</strong> <span style=""color: #d9534f;"">{{daysOverdue}}</span></li>
    </ul>

    <p style=""font-weight: bold;"">Please take immediate action to resolve this complaint.</p>

    <p>Thank you,<br/>{{companyName}} Support Team</p>
</body>
</html>",
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.CommunicationTemplates.AddRangeAsync(templates);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {templates.Count} communication templates.");
    }
}
