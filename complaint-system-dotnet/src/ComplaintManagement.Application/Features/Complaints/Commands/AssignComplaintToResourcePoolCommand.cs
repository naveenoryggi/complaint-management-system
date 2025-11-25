using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Domain.Enums;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

/// <summary>
/// Command to assign a complaint to a resource pool using intelligent assignment methods
/// </summary>
public class AssignComplaintToResourcePoolCommand : IRequest<Result<ComplaintDto>>
{
    /// <summary>
    /// The complaint to assign
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// The resource pool to assign to
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The user performing the assignment
    /// </summary>
    public Guid AssignedById { get; set; }

    /// <summary>
    /// The assignment method to use
    /// </summary>
    public AdvancedAssignmentMethod AssignmentMethod { get; set; } = AdvancedAssignmentMethod.BestFit;

    /// <summary>
    /// Specific user to assign within the pool (for Manual assignment)
    /// </summary>
    public Guid? SpecificUserId { get; set; }

    /// <summary>
    /// Assignment context information
    /// </summary>
    public AssignmentContext Context { get; set; } = new();

    /// <summary>
    /// Whether to validate the assignment before executing
    /// </summary>
    public bool ValidateBeforeAssignment { get; set; } = true;

    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Command to automatically assign a complaint using the assignment engine
/// </summary>
public class AutoAssignComplaintCommand : IRequest<Result<ComplaintDto>>
{
    /// <summary>
    /// The complaint to assign
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// The user triggering the auto-assignment
    /// </summary>
    public Guid TriggeredById { get; set; }

    /// <summary>
    /// Assignment context information
    /// </summary>
    public AssignmentContext Context { get; set; } = new();

    /// <summary>
    /// Whether to force assignment even if no perfect match is found
    /// </summary>
    public bool ForceAssignment { get; set; } = false;

    /// <summary>
    /// Specific assignment criteria to use (overrides default)
    /// </summary>
    public AssignmentCriteria? Criteria { get; set; }

    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Command to reassign a complaint to a different user or pool
/// </summary>
public class ReassignComplaintCommand : IRequest<Result<ComplaintDto>>
{
    /// <summary>
    /// The complaint to reassign
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// The current assigned user (for validation)
    /// </summary>
    public Guid? CurrentAssignedUserId { get; set; }

    /// <summary>
    /// The new user to assign to
    /// </summary>
    public Guid? NewAssignedUserId { get; set; }

    /// <summary>
    /// The new resource pool to assign from
    /// </summary>
    public Guid? NewResourcePoolId { get; set; }

    /// <summary>
    /// The user performing the reassignment
    /// </summary>
    public Guid ReassignedById { get; set; }

    /// <summary>
    /// Assignment method to use for the new assignment
    /// </summary>
    public AdvancedAssignmentMethod AssignmentMethod { get; set; } = AdvancedAssignmentMethod.BestFit;

    /// <summary>
    /// Reason for reassignment
    /// </summary>
    public string ReassignmentReason { get; set; } = string.Empty;

    /// <summary>
    /// Assignment context information
    /// </summary>
    public AssignmentContext Context { get; set; } = new();

    /// <summary>
    /// Whether to keep the original assignment in history
    /// </summary>
    public bool KeepAssignmentHistory { get; set; } = true;

    /// <summary>
    /// Optional notes about the reassignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Query to get assignment candidates for a complaint
/// </summary>
public class GetAssignmentCandidatesQuery : IRequest<Result<List<ResourcePoolCandidate>>>
{
    /// <summary>
    /// The complaint to find candidates for
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// Assignment criteria to filter candidates
    /// </summary>
    public AssignmentCriteria Criteria { get; set; } = new();

    /// <summary>
    /// Maximum number of candidates to return
    /// </summary>
    public int MaxCandidates { get; set; } = 10;

    /// <summary>
    /// Whether to include detailed user information
    /// </summary>
    public bool IncludeUserDetails { get; set; } = true;

    /// <summary>
    /// Whether to calculate skill match scores
    /// </summary>
    public bool CalculateSkillScores { get; set; } = true;
}

/// <summary>
/// Query to validate a potential assignment
/// </summary>
public class ValidateAssignmentQuery : IRequest<Result<AssignmentValidationResult>>
{
    /// <summary>
    /// The complaint to validate assignment for
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// The user to validate assignment to
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The resource pool to validate assignment from
    /// </summary>
    public Guid? ResourcePoolId { get; set; }

    /// <summary>
    /// Assignment context information
    /// </summary>
    public AssignmentContext Context { get; set; } = new();

    /// <summary>
    /// Whether to perform detailed validation
    /// </summary>
    public bool DetailedValidation { get; set; } = true;
}

// AssignmentValidationResult is now defined in AssignmentResult.cs