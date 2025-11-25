using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Tracks skills and proficiency levels of resource pool members
/// Enables skill-based assignment and workload optimization
/// </summary>
public class ResourcePoolMemberSkills
{
    public Guid Id { get; set; }

    /// <summary>
    /// The resource pool this skill entry belongs to
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The user who possesses this skill
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Standardized skill code (e.g., "HR_POLICY", "TECHNICAL_SUPPORT", "FINANCIAL_ANALYSIS")
    /// </summary>
    public string SkillCode { get; set; } = string.Empty;

    /// <summary>
    /// Skill proficiency level (1-5)
    /// 1=Basic, 2=Intermediate, 3=Advanced, 4=Expert, 5=Master
    /// </summary>
    public int SkillLevel { get; set; } = 1;

    /// <summary>
    /// When the user was certified for this skill level
    /// </summary>
    public DateTime CertifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this skill certification expires (if applicable)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Whether this skill is currently active for assignment
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Who certified this skill (audit trail)
    /// </summary>
    public Guid? CertifiedBy { get; set; }

    /// <summary>
    /// Notes about the skill or certification
    /// </summary>
    public string? CertificationNotes { get; set; }

    /// <summary>
    /// When this skill entry was last verified
    /// </summary>
    public DateTime? LastVerifiedAt { get; set; }

    /// <summary>
    /// Who verified this skill last
    /// </summary>
    public Guid? LastVerifiedBy { get; set; }

    // Navigation properties
    public ResourcePool ResourcePool { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>
    /// Check if this skill certification is currently valid
    /// </summary>
    public bool IsCurrentlyValid()
    {
        return IsActive &&
               (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);
    }

    /// <summary>
    /// Check if this skill certification is expiring soon (within 30 days)
    /// </summary>
    public bool IsExpiringSoon()
    {
        return ExpiresAt.HasValue &&
               ExpiresAt.Value <= DateTime.UtcNow.AddDays(30) &&
               ExpiresAt.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Get the skill level description
    /// </summary>
    public string GetSkillLevelDescription()
    {
        return SkillLevel switch
        {
            1 => "Basic",
            2 => "Intermediate",
            3 => "Advanced",
            4 => "Expert",
            5 => "Master",
            _ => "Unknown"
        };
    }
}