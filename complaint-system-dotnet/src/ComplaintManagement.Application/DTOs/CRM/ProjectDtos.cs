using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.CRM;

#region Project DTOs

/// <summary>
/// Full project DTO for detail views
/// </summary>
public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Customer
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    // Timeline
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int ProgressPercentage { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Currency { get; set; } = "INR";

    // Team
    public Guid? ProjectManagerId { get; set; }
    public string? ProjectManagerName { get; set; }

    // Additional
    public int? Priority { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }

    // Statistics
    public int MilestoneCount { get; set; }
    public int CompletedMilestoneCount { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public int DocumentCount { get; set; }
    public int TeamMemberCount { get; set; }
    public int TicketCount { get; set; }

    // Nested data (populated on detail view)
    public List<ProjectMilestoneDto> Milestones { get; set; } = new();
    public List<ProjectTaskDto> Tasks { get; set; } = new();
    public List<ProjectDocumentDto> Documents { get; set; } = new();
    public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Project summary for list views
/// </summary>
public class ProjectSummaryDto
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Customer
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    // Timeline
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public int ProgressPercentage { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }

    // Team
    public string? ProjectManagerName { get; set; }

    // Statistics
    public int MilestoneCount { get; set; }
    public int TaskCount { get; set; }
    public int TeamMemberCount { get; set; }
}

/// <summary>
/// Project dropdown/lookup item
/// </summary>
public class ProjectLookupDto
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a new project
/// </summary>
public class CreateProjectRequest
{
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    // Customer
    public Guid CustomerId { get; set; }

    // Timeline
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public string Currency { get; set; } = "INR";

    // Team
    public Guid? ProjectManagerId { get; set; }

    // Additional
    public int? Priority { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
}

/// <summary>
/// Request to update an existing project
/// </summary>
public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }

    // Timeline
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int ProgressPercentage { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Currency { get; set; } = "INR";

    // Team
    public Guid? ProjectManagerId { get; set; }

    // Additional
    public int? Priority { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? CustomFields { get; set; }
}

#endregion

#region Milestone DTOs

/// <summary>
/// Project milestone DTO
/// </summary>
public class ProjectMilestoneDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MilestoneStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create a milestone
/// </summary>
public class CreateMilestoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public int SortOrder { get; set; } = 0;
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a milestone
/// </summary>
public class UpdateMilestoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Task DTOs

/// <summary>
/// Project task DTO
/// </summary>
public class ProjectTaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public string? MilestoneName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public TaskPriority Priority { get; set; }
    public string PriorityName => Priority.ToString();

    // Assignment
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }

    // Timeline
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Time Tracking
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }

    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create a task
/// </summary>
public class CreateTaskRequest
{
    public Guid? MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public int? EstimatedHours { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a task
/// </summary>
public class UpdateTaskRequest
{
    public Guid? MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Document DTOs

/// <summary>
/// Project document DTO
/// </summary>
public class ProjectDocumentDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long FileSize { get; set; }
    public DocumentCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public string? Description { get; set; }
    public Guid UploadedById { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Request to upload a document
/// </summary>
public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long FileSize { get; set; }
    public DocumentCategory Category { get; set; } = DocumentCategory.Other;
    public string? Description { get; set; }
}

#endregion

#region Team Member DTOs

/// <summary>
/// Project team member DTO
/// </summary>
public class ProjectTeamMemberDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int? AllocationPercentage { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LeftDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add a team member
/// </summary>
public class AddTeamMemberRequest
{
    public Guid EmployeeId { get; set; }
    public string? Role { get; set; }
    public int? AllocationPercentage { get; set; }
    public DateTime? JoinedDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a team member
/// </summary>
public class UpdateTeamMemberRequest
{
    public string? Role { get; set; }
    public int? AllocationPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LeftDate { get; set; }
    public string? Notes { get; set; }
}

#endregion
