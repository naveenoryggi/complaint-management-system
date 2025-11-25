using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Escalation;

/// <summary>
/// DTO for Resource Pool
/// </summary>
public class ResourcePoolDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourcePoolType PoolType { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public List<ResourcePoolMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for Resource Pool Member
/// </summary>
public class ResourcePoolMemberDto
{
    public Guid Id { get; set; }
    public Guid ResourcePoolId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public Guid AddedBy { get; set; }
    public string AddedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for creating a resource pool
/// </summary>
public class CreateResourcePoolRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourcePoolType PoolType { get; set; } = ResourcePoolType.Custom;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public List<Guid>? MemberUserIds { get; set; }
}

/// <summary>
/// Request DTO for updating a resource pool
/// </summary>
public class UpdateResourcePoolRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourcePoolType PoolType { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for adding a member to a resource pool
/// </summary>
public class AddResourcePoolMemberRequest
{
    public Guid UserId { get; set; }
}
