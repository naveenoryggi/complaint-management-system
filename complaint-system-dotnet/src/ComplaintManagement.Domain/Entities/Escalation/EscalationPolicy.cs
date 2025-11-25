using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Defines escalation policies with hierarchical override support
/// Allows configuration at Company, Branch, Department, Section, and Category levels
/// More specific policies override more general ones
/// </summary>
public class EscalationPolicy : BaseEntity
{
    /// <summary>
    /// Company ID (required - all policies belong to a company)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Policy name for easy identification
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description explaining the policy purpose
    /// </summary>
    public string? Description { get; set; }

    #region Scope Definition (null = applies to all at that level)

    /// <summary>
    /// Specific branch (null = all branches)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Specific department (null = all departments)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Specific section (null = all sections)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Specific category (null = all categories)
    /// </summary>
    public Guid? CategoryId { get; set; }

    #endregion

    #region Policy Configuration

    /// <summary>
    /// Enable/disable auto-escalation for this scope
    /// </summary>
    public bool EnableAutoEscalation { get; set; } = true;

    /// <summary>
    /// Require manual approval before escalation
    /// If true, auto-escalation will create a pending escalation that requires approval
    /// </summary>
    public bool RequireManualApproval { get; set; } = false;

    /// <summary>
    /// Default escalation matrix to use for this scope
    /// null = use system default or parent policy's matrix
    /// </summary>
    public Guid? DefaultEscalationMatrixId { get; set; }

    /// <summary>
    /// Minimum severity level required for auto-escalation
    /// null = all severities
    /// Values: 1=Low, 2=Medium, 3=High, 4=Critical
    /// </summary>
    public int? MinimumSeverityForAutoEscalation { get; set; }

    /// <summary>
    /// Maximum number of auto-escalation levels allowed
    /// null = unlimited
    /// Useful to prevent excessive escalations
    /// </summary>
    public int? MaxAutoEscalationLevels { get; set; }

    #endregion

    #region Priority and Ordering

    /// <summary>
    /// Priority for conflict resolution (higher number = higher priority)
    /// When multiple policies match, the one with higher priority wins
    /// System calculates priority based on specificity, but this allows manual override
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Whether this policy is currently active
    /// Inactive policies are ignored during policy resolution
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Effective Dates (Optional)

    /// <summary>
    /// Policy effective start date (null = immediate)
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Policy effective end date (null = indefinite)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company this policy belongs to
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Branch (if branch-specific policy)
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department (if department-specific policy)
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Section (if section-specific policy)
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Category (if category-specific policy)
    /// </summary>
    public ComplaintCategory? Category { get; set; }

    /// <summary>
    /// Default escalation matrix for this policy
    /// </summary>
    public EscalationMatrix? DefaultEscalationMatrix { get; set; }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate specificity score for policy resolution
    /// Higher score = more specific policy
    /// </summary>
    public int GetSpecificityScore()
    {
        int score = 0;

        if (CategoryId.HasValue) score += 100;    // Most specific
        if (SectionId.HasValue) score += 10;
        if (DepartmentId.HasValue) score += 5;
        if (BranchId.HasValue) score += 2;
        // Company level = 0 (least specific)

        return score + Priority;  // Add manual priority override
    }

    /// <summary>
    /// Get human-readable scope description
    /// </summary>
    public string GetScopeDescription()
    {
        var parts = new List<string>();

        if (CategoryId.HasValue) parts.Add($"Category: {Category?.Name ?? "Unknown"}");
        if (SectionId.HasValue) parts.Add($"Section: {Section?.Name ?? "Unknown"}");
        if (DepartmentId.HasValue) parts.Add($"Department: {Department?.Name ?? "Unknown"}");
        if (BranchId.HasValue) parts.Add($"Branch: {Branch?.Name ?? "Unknown"}");

        if (parts.Count == 0)
            return "Company-wide (All units)";

        return string.Join(" → ", parts);
    }

    /// <summary>
    /// Check if policy is currently effective based on dates
    /// </summary>
    public bool IsCurrentlyEffective()
    {
        var now = DateTime.UtcNow;

        if (EffectiveFrom.HasValue && now < EffectiveFrom.Value)
            return false;

        if (EffectiveTo.HasValue && now > EffectiveTo.Value)
            return false;

        return true;
    }

    #endregion
}
