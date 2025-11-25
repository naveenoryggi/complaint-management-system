using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Domain.Entities.Escalation;

namespace ComplaintManagement.Application.Interfaces;

/// <summary>
/// Service for managing escalation policies with hierarchical override support
/// </summary>
public interface IEscalationPolicyService
{
    /// <summary>
    /// Resolve the most specific escalation policy for a given complaint context
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="departmentId">Department ID (optional)</param>
    /// <param name="sectionId">Section ID (optional)</param>
    /// <param name="categoryId">Category ID (optional)</param>
    /// <returns>The most specific active policy, or null if no policy found</returns>
    Task<EscalationPolicy?> ResolvePolicy(
        Guid companyId,
        Guid? branchId = null,
        Guid? departmentId = null,
        Guid? sectionId = null,
        Guid? categoryId = null);

    /// <summary>
    /// Get all policies for a company
    /// </summary>
    Task<List<EscalationPolicyDto>> GetPoliciesAsync(Guid companyId);

    /// <summary>
    /// Get a specific policy by ID
    /// </summary>
    Task<EscalationPolicyDto?> GetPolicyByIdAsync(Guid policyId);

    /// <summary>
    /// Create a new escalation policy
    /// </summary>
    Task<EscalationPolicyDto> CreatePolicyAsync(CreateEscalationPolicyRequest request);

    /// <summary>
    /// Update an existing escalation policy
    /// </summary>
    Task<EscalationPolicyDto> UpdatePolicyAsync(Guid policyId, UpdateEscalationPolicyRequest request);

    /// <summary>
    /// Delete an escalation policy
    /// </summary>
    Task<bool> DeletePolicyAsync(Guid policyId);

    /// <summary>
    /// Test policy resolution for a given context (for admin testing)
    /// </summary>
    Task<PolicyResolutionResultDto> TestPolicyResolution(PolicyResolutionTestRequest request);
}
