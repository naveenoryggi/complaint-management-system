using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Assignment;

/// <summary>
/// Represents a configurable assignment rule for routing complaints
/// Rules are executed in priority order to determine optimal assignment
/// </summary>
public class AssignmentRule
{
    public Guid Id { get; set; }

    /// <summary>
    /// The company this rule belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Human-readable name for this rule
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what this rule does
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority order for rule execution (1 = highest priority)
    /// Lower numbers execute first
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Whether this rule is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON string containing flexible condition structure
    /// Defines when this rule should be triggered
    /// </summary>
    public string ConditionsJson { get; set; } = string.Empty;

    /// <summary>
    /// The action type to perform when conditions are met
    /// </summary>
    public AssignmentActionType ActionType { get; set; }

    /// <summary>
    /// JSON string containing action parameters
    /// Configuration specific to the action type
    /// </summary>
    public string ActionParametersJson { get; set; } = string.Empty;

    /// <summary>
    /// When this rule was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this rule
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When this rule was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Who last modified this rule
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// When this rule was deactivated (if applicable)
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }

    /// <summary>
    /// Who deactivated this rule
    /// </summary>
    public Guid? DeactivatedBy { get; set; }

    /// <summary>
    /// Reason for deactivation
    /// </summary>
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// How many times this rule has been executed
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    /// How many times this rule has resulted in successful assignments
    /// </summary>
    public int SuccessCount { get; set; } = 0;

    /// <summary>
    /// When this rule was last executed
    /// </summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>
    /// Whether execution statistics should be tracked
    /// </summary>
    public bool TrackStatistics { get; set; } = true;

    // Navigation properties
    public Company Company { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public User? Modifier { get; set; }
    public User? Deactivator { get; set; }

    /// <summary>
    /// Deactivate this rule
    /// </summary>
    public void Deactivate(Guid deactivatedBy, string reason)
    {
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        DeactivatedBy = deactivatedBy;
        DeactivationReason = reason;
    }

    /// <summary>
    /// Update the rule's last execution status
    /// </summary>
    public void RecordExecution(bool success)
    {
        ExecutionCount++;
        if (success)
        {
            SuccessCount++;
        }
        LastExecutedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get the success rate as a percentage
    /// </summary>
    public decimal GetSuccessRate()
    {
        if (ExecutionCount == 0) return 0;
        return (decimal)SuccessCount / ExecutionCount * 100;
    }

    /// <summary>
    /// Check if this rule is currently active and executable
    /// </summary>
    public bool IsExecutable()
    {
        return IsActive && !DeactivatedAt.HasValue;
    }
}