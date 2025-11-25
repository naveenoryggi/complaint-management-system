namespace ComplaintManagement.Application.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    // Organizational structure fields
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public Guid? EmployeeTypeId { get; set; }
    public string? EmployeeTypeName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    public bool IsActive { get; set; } = true;

    // Timezone & Localization
    /// <summary>
    /// Resolved timezone for the user (fallback chain: User > Branch > Company)
    /// IANA timezone identifier (e.g., "America/New_York", "Europe/London")
    /// </summary>
    public string TimeZone { get; set; } = "Asia/Kolkata";

    /// <summary>
    /// User's personal timezone preference (if set)
    /// </summary>
    public string? PreferredTimeZone { get; set; }

    /// <summary>
    /// User's preferred locale/language (e.g., "en-US", "fr-FR")
    /// </summary>
    public string? PreferredLocale { get; set; }

    /// <summary>
    /// User's preferred date format (e.g., "MM/dd/yyyy", "dd/MM/yyyy")
    /// </summary>
    public string? PreferredDateFormat { get; set; }

    /// <summary>
    /// User's preferred time format (e.g., "hh:mm a", "HH:mm")
    /// </summary>
    public string? PreferredTimeFormat { get; set; }

    public List<UserRoleDto> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class UserRoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public int EscalationLevel { get; set; }
    public bool IsPrimary { get; set; }
}
