using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

#region Store DTOs

/// <summary>
/// DTO for creating a new store
/// </summary>
public class CreateStoreDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PrimaryManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public int ApprovalTimeoutHours { get; set; } = 48;
    public bool AutoEscalateToSecondary { get; set; } = true;
    public bool RequireDeptApprovalForAssignment { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing store (Code is immutable and not included)
/// </summary>
public class UpdateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PrimaryManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public int ApprovalTimeoutHours { get; set; }
    public bool AutoEscalateToSecondary { get; set; }
    public bool RequireDeptApprovalForAssignment { get; set; }
}

/// <summary>
/// DTO for store response
/// </summary>
public class StoreDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PrimaryManagerId { get; set; }
    public string? PrimaryManagerName { get; set; }
    public string? PrimaryManagerEmployeeCode { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public string? SecondaryManagerName { get; set; }
    public string? SecondaryManagerEmployeeCode { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public int ApprovalTimeoutHours { get; set; }
    public bool AutoEscalateToSecondary { get; set; }
    public bool RequireDeptApprovalForAssignment { get; set; }
    public int StaffCount { get; set; }
    public int PendingRequestsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for store lookup (dropdowns)
/// </summary>
public class StoreLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName => $"{Code} - {Name}";
}

/// <summary>
/// DTO for assigning managers to a store
/// </summary>
public class AssignStoreManagersDto
{
    public Guid? PrimaryManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
}

#endregion

#region Store User Role DTOs

/// <summary>
/// DTO for assigning a user role in a store
/// </summary>
public class AssignStoreUserRoleDto
{
    public Guid UserId { get; set; }
    public StoreRole Role { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for store user role response
/// </summary>
public class StoreUserRoleDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public StoreRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? AssignedByName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for store staff list
/// </summary>
public class StoreStaffDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public StoreRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

#endregion
