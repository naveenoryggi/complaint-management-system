using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Assignment;

/// <summary>
/// Tracks the execution history of assignment rules for audit and analysis
/// </summary>
public class AssignmentRuleExecution
{
    public Guid Id { get; set; }

    /// <summary>
    /// The rule that was executed
    /// </summary>
    public Guid AssignmentRuleId { get; set; }

    /// <summary>
    /// The complaint that was processed
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// The user who triggered the rule execution (system or manual)
    /// </summary>
    public Guid? TriggeredByUserId { get; set; }

    /// <summary>
    /// Whether the rule conditions were met
    /// </summary>
    public bool ConditionsMet { get; set; }

    /// <summary>
    /// Whether the assignment action was successful
    /// </summary>
    public bool ActionSucceeded { get; set; }

    /// <summary>
    /// The action that was taken
    /// </summary>
    public string? ActionTaken { get; set; }

    /// <summary>
    /// The result of the action (e.g., assigned user ID, resource pool ID)
    /// </summary>
    public string? ActionResult { get; set; }

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// When the rule execution started
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON string containing the context data during execution
    /// </summary>
    public string ExecutionContextJson { get; set; } = string.Empty;

    // Navigation properties
    public AssignmentRule AssignmentRule { get; set; } = null!;
    public Complaint Complaint { get; set; } = null!;
    public User? TriggeredByUser { get; set; }

    /// <summary>
    /// Create a successful execution record
    /// </summary>
    public static AssignmentRuleExecution CreateSuccess(
        Guid ruleId,
        Guid complaintId,
        Guid? triggeredByUserId,
        string actionTaken,
        string? actionResult,
        long executionTimeMs,
        string? contextJson = null)
    {
        return new AssignmentRuleExecution
        {
            AssignmentRuleId = ruleId,
            ComplaintId = complaintId,
            TriggeredByUserId = triggeredByUserId,
            ConditionsMet = true,
            ActionSucceeded = true,
            ActionTaken = actionTaken,
            ActionResult = actionResult,
            ExecutionTimeMs = executionTimeMs,
            ExecutionContextJson = contextJson ?? string.Empty
        };
    }

    /// <summary>
    /// Create a failed execution record
    /// </summary>
    public static AssignmentRuleExecution CreateFailure(
        Guid ruleId,
        Guid complaintId,
        Guid? triggeredByUserId,
        bool conditionsMet,
        string errorMessage,
        long executionTimeMs,
        string? contextJson = null)
    {
        return new AssignmentRuleExecution
        {
            AssignmentRuleId = ruleId,
            ComplaintId = complaintId,
            TriggeredByUserId = triggeredByUserId,
            ConditionsMet = conditionsMet,
            ActionSucceeded = false,
            ErrorMessage = errorMessage,
            ExecutionTimeMs = executionTimeMs,
            ExecutionContextJson = contextJson ?? string.Empty
        };
    }
}