using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Interfaces;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing escalation policies with hierarchical override support
/// </summary>
public class EscalationPolicyService : IEscalationPolicyService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<EscalationPolicyService> _logger;

    public EscalationPolicyService(
        ComplaintDbContext context,
        ILogger<EscalationPolicyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Resolve the most specific escalation policy for a given complaint context
    /// Uses hierarchical resolution: Category > Section > Department > Branch > Company
    /// More specific policies override more general ones
    /// </summary>
    public async Task<EscalationPolicy?> ResolvePolicy(
        Guid companyId,
        Guid? branchId = null,
        Guid? departmentId = null,
        Guid? sectionId = null,
        Guid? categoryId = null)
    {
        _logger.LogInformation(
            "Resolving escalation policy for Company:{CompanyId}, Branch:{BranchId}, Dept:{DeptId}, Section:{SectionId}, Category:{CategoryId}",
            companyId, branchId, departmentId, sectionId, categoryId);

        var now = DateTime.UtcNow;

        // Get all active policies for this company that match the context
        var matchingPolicies = await _context.EscalationPolicies
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .Where(p => !p.EffectiveFrom.HasValue || p.EffectiveFrom <= now)
            .Where(p => !p.EffectiveTo.HasValue || p.EffectiveTo >= now)
            .Where(p =>
                // Policy must match the context or be more general
                (!p.BranchId.HasValue || p.BranchId == branchId) &&
                (!p.DepartmentId.HasValue || p.DepartmentId == departmentId) &&
                (!p.SectionId.HasValue || p.SectionId == sectionId) &&
                (!p.CategoryId.HasValue || p.CategoryId == categoryId))
            .Include(p => p.Company)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .Include(p => p.Category)
            .Include(p => p.DefaultEscalationMatrix)
            .ToListAsync();

        if (!matchingPolicies.Any())
        {
            _logger.LogWarning("No escalation policy found for the given context");
            return null;
        }

        // Sort by specificity score (highest first)
        var resolvedPolicy = matchingPolicies
            .OrderByDescending(p => p.GetSpecificityScore())
            .First();

        _logger.LogInformation(
            "Resolved policy: {PolicyName} (ID: {PolicyId}, Specificity: {Score})",
            resolvedPolicy.Name, resolvedPolicy.Id, resolvedPolicy.GetSpecificityScore());

        return resolvedPolicy;
    }

    /// <summary>
    /// Get all policies for a company
    /// </summary>
    public async Task<List<EscalationPolicyDto>> GetPoliciesAsync(Guid companyId)
    {
        var policies = await _context.EscalationPolicies
            .Where(p => p.CompanyId == companyId)
            .Include(p => p.Company)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .Include(p => p.Category)
            .Include(p => p.DefaultEscalationMatrix)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return policies.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get a specific policy by ID
    /// </summary>
    public async Task<EscalationPolicyDto?> GetPolicyByIdAsync(Guid policyId)
    {
        var policy = await _context.EscalationPolicies
            .Include(p => p.Company)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .Include(p => p.Category)
            .Include(p => p.DefaultEscalationMatrix)
            .FirstOrDefaultAsync(p => p.Id == policyId);

        return policy == null ? null : MapToDto(policy);
    }

    /// <summary>
    /// Create a new escalation policy
    /// </summary>
    public async Task<EscalationPolicyDto> CreatePolicyAsync(CreateEscalationPolicyRequest request)
    {
        // Validate scope hierarchy
        await ValidateScopeHierarchy(request.CompanyId, request.BranchId, request.DepartmentId, request.SectionId);

        var policy = new EscalationPolicy
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            Name = request.Name,
            Description = request.Description,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            SectionId = request.SectionId,
            CategoryId = request.CategoryId,
            EnableAutoEscalation = request.EnableAutoEscalation,
            RequireManualApproval = request.RequireManualApproval,
            DefaultEscalationMatrixId = request.DefaultEscalationMatrixId,
            MinimumSeverityForAutoEscalation = request.MinimumSeverityForAutoEscalation,
            MaxAutoEscalationLevels = request.MaxAutoEscalationLevels,
            Priority = request.Priority,
            IsActive = true,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        _context.EscalationPolicies.Add(policy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created escalation policy: {PolicyName} (ID: {PolicyId})", policy.Name, policy.Id);

        // Reload with navigation properties
        return (await GetPolicyByIdAsync(policy.Id))!;
    }

    /// <summary>
    /// Update an existing escalation policy
    /// </summary>
    public async Task<EscalationPolicyDto> UpdatePolicyAsync(Guid policyId, UpdateEscalationPolicyRequest request)
    {
        var policy = await _context.EscalationPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
        {
            throw new KeyNotFoundException($"Escalation policy with ID {policyId} not found");
        }

        policy.Name = request.Name;
        policy.Description = request.Description;
        policy.EnableAutoEscalation = request.EnableAutoEscalation;
        policy.RequireManualApproval = request.RequireManualApproval;
        policy.DefaultEscalationMatrixId = request.DefaultEscalationMatrixId;
        policy.MinimumSeverityForAutoEscalation = request.MinimumSeverityForAutoEscalation;
        policy.MaxAutoEscalationLevels = request.MaxAutoEscalationLevels;
        policy.Priority = request.Priority;
        policy.IsActive = request.IsActive;
        policy.EffectiveFrom = request.EffectiveFrom;
        policy.EffectiveTo = request.EffectiveTo;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated escalation policy: {PolicyName} (ID: {PolicyId})", policy.Name, policy.Id);

        return (await GetPolicyByIdAsync(policyId))!;
    }

    /// <summary>
    /// Delete an escalation policy (soft delete)
    /// </summary>
    public async Task<bool> DeletePolicyAsync(Guid policyId)
    {
        var policy = await _context.EscalationPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
        {
            return false;
        }

        _context.EscalationPolicies.Remove(policy); // Soft delete handled by DbContext
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted escalation policy: {PolicyName} (ID: {PolicyId})", policy.Name, policy.Id);

        return true;
    }

    /// <summary>
    /// Test policy resolution for a given context (for admin testing)
    /// </summary>
    public async Task<PolicyResolutionResultDto> TestPolicyResolution(PolicyResolutionTestRequest request)
    {
        var resolvedPolicy = await ResolvePolicy(
            request.CompanyId,
            request.BranchId,
            request.DepartmentId,
            request.SectionId,
            request.CategoryId);

        var now = DateTime.UtcNow;

        // Get all matching policies for explanation
        var allMatchingPolicies = await _context.EscalationPolicies
            .Where(p => p.CompanyId == request.CompanyId && p.IsActive)
            .Where(p => !p.EffectiveFrom.HasValue || p.EffectiveFrom <= now)
            .Where(p => !p.EffectiveTo.HasValue || p.EffectiveTo >= now)
            .Where(p =>
                (!p.BranchId.HasValue || p.BranchId == request.BranchId) &&
                (!p.DepartmentId.HasValue || p.DepartmentId == request.DepartmentId) &&
                (!p.SectionId.HasValue || p.SectionId == request.SectionId) &&
                (!p.CategoryId.HasValue || p.CategoryId == request.CategoryId))
            .Include(p => p.Company)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .Include(p => p.Category)
            .Include(p => p.DefaultEscalationMatrix)
            .ToListAsync();

        var explanation = GenerateResolutionExplanation(allMatchingPolicies, resolvedPolicy);

        return new PolicyResolutionResultDto
        {
            ResolvedPolicy = resolvedPolicy != null ? MapToDto(resolvedPolicy) : null,
            AllMatchingPolicies = allMatchingPolicies.Select(MapToDto).ToList(),
            ResolutionExplanation = explanation
        };
    }

    #region Helper Methods

    /// <summary>
    /// Map entity to DTO
    /// </summary>
    private static EscalationPolicyDto MapToDto(EscalationPolicy policy)
    {
        return new EscalationPolicyDto
        {
            Id = policy.Id,
            CompanyId = policy.CompanyId,
            CompanyName = policy.Company?.Name ?? string.Empty,
            Name = policy.Name,
            Description = policy.Description,
            BranchId = policy.BranchId,
            BranchName = policy.Branch?.Name,
            DepartmentId = policy.DepartmentId,
            DepartmentName = policy.Department?.Name,
            SectionId = policy.SectionId,
            SectionName = policy.Section?.Name,
            CategoryId = policy.CategoryId,
            CategoryName = policy.Category?.Name,
            EnableAutoEscalation = policy.EnableAutoEscalation,
            RequireManualApproval = policy.RequireManualApproval,
            DefaultEscalationMatrixId = policy.DefaultEscalationMatrixId,
            DefaultEscalationMatrixName = policy.DefaultEscalationMatrix?.Name,
            MinimumSeverityForAutoEscalation = policy.MinimumSeverityForAutoEscalation,
            MaxAutoEscalationLevels = policy.MaxAutoEscalationLevels,
            Priority = policy.Priority,
            SpecificityScore = policy.GetSpecificityScore(),
            IsActive = policy.IsActive,
            EffectiveFrom = policy.EffectiveFrom,
            EffectiveTo = policy.EffectiveTo,
            ScopeDescription = policy.GetScopeDescription(),
            IsCurrentlyEffective = policy.IsCurrentlyEffective(),
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }

    /// <summary>
    /// Validate that scope hierarchy is correct
    /// </summary>
    private async Task ValidateScopeHierarchy(Guid companyId, Guid? branchId, Guid? departmentId, Guid? sectionId)
    {
        if (branchId.HasValue)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId.Value && b.CompanyId == companyId);
            if (branch == null)
            {
                throw new ArgumentException($"Branch {branchId} does not belong to company {companyId}");
            }
        }

        if (departmentId.HasValue)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId.Value &&
                    (!branchId.HasValue || d.BranchId == branchId));
            if (department == null)
            {
                throw new ArgumentException($"Department {departmentId} does not belong to the specified branch");
            }
        }

        if (sectionId.HasValue)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == sectionId.Value &&
                    (!departmentId.HasValue || s.DepartmentId == departmentId));
            if (section == null)
            {
                throw new ArgumentException($"Section {sectionId} does not belong to the specified department");
            }
        }
    }

    /// <summary>
    /// Generate human-readable explanation of policy resolution
    /// </summary>
    private static string GenerateResolutionExplanation(List<EscalationPolicy> allPolicies, EscalationPolicy? resolved)
    {
        if (!allPolicies.Any())
        {
            return "No matching policies found for the given context.";
        }

        if (resolved == null)
        {
            return $"Found {allPolicies.Count} matching policy(ies), but none were effective.";
        }

        var explanation = $"Found {allPolicies.Count} matching policy(ies):\n\n";

        foreach (var policy in allPolicies.OrderByDescending(p => p.GetSpecificityScore()))
        {
            var isResolved = policy.Id == resolved.Id;
            var marker = isResolved ? "✓ SELECTED" : "  ";

            explanation += $"{marker} {policy.Name}\n";
            explanation += $"   Scope: {policy.GetScopeDescription()}\n";
            explanation += $"   Specificity Score: {policy.GetSpecificityScore()}\n";
            explanation += $"   Auto-escalation: {(policy.EnableAutoEscalation ? "Enabled" : "Disabled")}\n";

            if (isResolved)
            {
                explanation += "   → This policy has the highest specificity score and will be applied.\n";
            }

            explanation += "\n";
        }

        return explanation;
    }

    #endregion
}
