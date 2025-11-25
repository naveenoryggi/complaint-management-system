namespace ComplaintManagement.Application.DTOs.Assignment;

/// <summary>
/// Result of an assignment operation
/// </summary>
public class AssignmentResult
{
    /// <summary>
    /// Whether the assignment was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The assigned user ID (if successful)
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// The assigned user's name (if successful)
    /// </summary>
    public string? AssignedUserName { get; set; }

    /// <summary>
    /// The resource pool used for assignment (if applicable)
    /// </summary>
    public Guid? ResourcePoolId { get; set; }

    /// <summary>
    /// The resource pool name (if applicable)
    /// </summary>
    public string? ResourcePoolName { get; set; }

    /// <summary>
    /// The assignment method used
    /// </summary>
    public string AssignmentMethod { get; set; } = string.Empty;

    /// <summary>
    /// The rule that triggered this assignment (if applicable)
    /// </summary>
    public Guid? AppliedRuleId { get; set; }

    /// <summary>
    /// The name of the rule that was applied
    /// </summary>
    public string? AppliedRuleName { get; set; }

    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error details (if assignment failed)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Additional information about the assignment
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// List of candidates that were considered (for audit and analysis)
    /// </summary>
    public List<ResourcePoolCandidate> ConsideredCandidates { get; set; } = new();

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// When the assignment was performed
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful assignment result
    /// </summary>
    public static AssignmentResult CreateSuccess(
        Guid userId,
        string userName,
        string assignmentMethod,
        string message,
        Guid? resourcePoolId = null,
        string? resourcePoolName = null,
        Guid? ruleId = null,
        string? ruleName = null)
    {
        return new AssignmentResult
        {
            Success = true,
            AssignedUserId = userId,
            AssignedUserName = userName,
            AssignmentMethod = assignmentMethod,
            Message = message,
            ResourcePoolId = resourcePoolId,
            ResourcePoolName = resourcePoolName,
            AppliedRuleId = ruleId,
            AppliedRuleName = ruleName
        };
    }

    /// <summary>
    /// Create a failed assignment result
    /// </summary>
    public static AssignmentResult Failure(string message, string? errorCode = null)
    {
        return new AssignmentResult
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Represents a candidate resource pool for assignment
/// </summary>
public class ResourcePoolCandidate
{
    /// <summary>
    /// The resource pool ID
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The resource pool name
    /// </summary>
    public string ResourcePoolName { get; set; } = string.Empty;

    /// <summary>
    /// The resource pool type
    /// </summary>
    public string PoolType { get; set; } = string.Empty;

    /// <summary>
    /// Suitability score (higher is better)
    /// </summary>
    public double SuitabilityScore { get; set; }

    /// <summary>
    /// Available members in this pool
    /// </summary>
    public int AvailableMemberCount { get; set; }

    /// <summary>
    /// Total members in this pool
    /// </summary>
    public int TotalMemberCount { get; set; }

    /// <summary>
    /// Average workload per member
    /// </summary>
    public double AverageWorkload { get; set; }

    /// <summary>
    /// Success rate of this pool
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Average resolution time for this pool
    /// </summary>
    public TimeSpan AverageResolutionTime { get; set; }

    /// <summary>
    /// Whether this pool can handle the specific complaint
    /// </summary>
    public bool CanHandleComplaint { get; set; }

    /// <summary>
    /// Why this pool can or cannot handle the complaint
    /// </summary>
    public List<string> DisqualificationReasons { get; set; } = new();

    /// <summary>
    /// Specializations that make this pool suitable
    /// </summary>
    public List<PoolSpecializationInfo> MatchingSpecializations { get; set; } = new();

    /// <summary>
    /// Recommended assignment method for this pool
    /// </summary>
    public string RecommendedAssignmentMethod { get; set; } = string.Empty;

    /// <summary>
    /// The best user in this pool for this complaint
    /// </summary>
    public UserCandidate? BestUserCandidate { get; set; }
}

/// <summary>
/// Information about a pool's specialization
/// </summary>
public class PoolSpecializationInfo
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MinPriorityLevel { get; set; }
    public int MaxPriorityLevel { get; set; }
    public decimal Weight { get; set; }
}

/// <summary>
/// Represents a candidate user for assignment
/// </summary>
public class UserCandidate
{
    /// <summary>
    /// The user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// The user's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Current active complaints
    /// </summary>
    public int CurrentWorkload { get; set; }

    /// <summary>
    /// Success rate
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Average resolution time
    /// </summary>
    public TimeSpan AverageResolutionTime { get; set; }

    /// <summary>
    /// Skill match score (0.0 to 1.0)
    /// </summary>
    public double SkillMatchScore { get; set; }

    /// <summary>
    /// Overall suitability score (higher is better)
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Whether this user is available for assignment
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Reasons why this user might not be suitable
    /// </summary>
    public List<string> DisqualificationReasons { get; set; } = new();

    /// <summary>
    /// Skills that match the requirements
    /// </summary>
    public List<MatchingSkill> MatchingSkills { get; set; } = new();

    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTime? LastActivityAt { get; set; }
}

/// <summary>
/// Information about a matching skill
/// </summary>
public class MatchingSkill
{
    public string SkillCode { get; set; } = string.Empty;
    public int SkillLevel { get; set; }
    public bool IsRequired { get; set; }
    public double MatchScore { get; set; }
}

/// <summary>
/// Result of assignment validation
/// </summary>
public class AssignmentValidationResult
{
    /// <summary>
    /// Whether the assignment is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors (if any)
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Validation warnings (potential issues)
    /// </summary>
    public List<string> ValidationWarnings { get; set; } = new();

    /// <summary>
    /// Suitability score (0.0 to 1.0)
    /// </summary>
    public double SuitabilityScore { get; set; }

    /// <summary>
    /// Recommended actions
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// Additional validation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}