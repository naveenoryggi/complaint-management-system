namespace ComplaintManagement.Application.DTOs.Assignment;

/// <summary>
/// Context information used during assignment decision making
/// </summary>
public class AssignmentContext
{
    /// <summary>
    /// The user performing the assignment (if applicable)
    /// </summary>
    public Guid? AssignedByUserId { get; set; }

    /// <summary>
    /// The timestamp when assignment is being performed
    /// </summary>
    public DateTime AssignmentTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this is an initial assignment or reassignment
    /// </summary>
    public bool IsInitialAssignment { get; set; } = true;

    /// <summary>
    /// Whether this assignment is part of an escalation
    /// </summary>
    public bool IsEscalation { get; set; } = false;

    /// <summary>
    /// Current escalation level (if applicable)
    /// </summary>
    public int CurrentEscalationLevel { get; set; } = 0;

    /// <summary>
    /// Reason for assignment (manual, automatic, escalation, etc.)
    /// </summary>
    public string AssignmentReason { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the assignment
    /// </summary>
    public string? AssignmentNotes { get; set; }

    /// <summary>
    /// Whether to bypass certain assignment rules
    /// Used for manual overrides
    /// </summary>
    public bool BypassRules { get; set; } = false;

    /// <summary>
    /// Specific assignment method to use (overrides default)
    /// </summary>
    public string? OverrideAssignmentMethod { get; set; }

    /// <summary>
    /// Whether to force assignment even if user is at capacity
    /// </summary>
    public bool ForceAssignment { get; set; } = false;

    /// <summary>
    /// Priority level for this assignment decision
    /// </summary>
    public int Priority { get; set; } = 1;
}