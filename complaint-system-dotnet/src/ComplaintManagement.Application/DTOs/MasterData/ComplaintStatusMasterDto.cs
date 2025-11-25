namespace ComplaintManagement.Application.DTOs.MasterData;

public class ComplaintStatusMasterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public bool IsFinal { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateComplaintStatusMasterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFinal { get; set; } = false;
    public Guid? CompanyId { get; set; }
}

public class UpdateComplaintStatusMasterRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public bool IsFinal { get; set; }
}
