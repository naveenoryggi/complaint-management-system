using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing escalation policies with hierarchical override support
/// </summary>
[ApiController]
[Route("api/escalation/policies")]
[Authorize]
public class EscalationPolicyController : ControllerBase
{
    private readonly IEscalationPolicyService _policyService;
    private readonly ILogger<EscalationPolicyController> _logger;

    public EscalationPolicyController(
        IEscalationPolicyService policyService,
        ILogger<EscalationPolicyController> logger)
    {
        _policyService = policyService;
        _logger = logger;
    }

    /// <summary>
    /// Get all escalation policies for a company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of escalation policies</returns>
    [HttpGet]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetPolicies([FromQuery] Guid companyId)
    {
        try
        {
            var policies = await _policyService.GetPoliciesAsync(companyId);

            return Ok(new
            {
                isSuccess = true,
                data = policies,
                message = $"Retrieved {policies.Count} escalation policy(ies)"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalation policies for company {CompanyId}", companyId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving escalation policies"
            });
        }
    }

    /// <summary>
    /// Get a specific escalation policy by ID
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <returns>Escalation policy details</returns>
    [HttpGet("{id}")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetPolicyById(Guid id)
    {
        try
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);

            if (policy == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Escalation policy with ID {id} not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = policy,
                message = "Escalation policy retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalation policy {PolicyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the escalation policy"
            });
        }
    }

    /// <summary>
    /// Create a new escalation policy
    /// </summary>
    /// <param name="request">Create policy request</param>
    /// <returns>Created escalation policy</returns>
    [HttpPost]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> CreatePolicy([FromBody] CreateEscalationPolicyRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Invalid policy data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            // Validate required fields
            if (request.CompanyId == Guid.Empty)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "CompanyId is required"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Name is required"
                });
            }

            var policy = await _policyService.CreatePolicyAsync(request);

            _logger.LogInformation(
                "Created escalation policy: {PolicyName} (ID: {PolicyId}) for company {CompanyId}",
                policy.Name, policy.Id, policy.CompanyId);

            return CreatedAtAction(
                nameof(GetPolicyById),
                new { id = policy.Id },
                new
                {
                    isSuccess = true,
                    data = policy,
                    message = "Escalation policy created successfully"
                }
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid policy scope hierarchy");
            return BadRequest(new
            {
                isSuccess = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating escalation policy");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the escalation policy"
            });
        }
    }

    /// <summary>
    /// Update an existing escalation policy
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <param name="request">Update policy request</param>
    /// <returns>Updated escalation policy</returns>
    [HttpPut("{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateEscalationPolicyRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Invalid policy data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var policy = await _policyService.UpdatePolicyAsync(id, request);

            _logger.LogInformation(
                "Updated escalation policy: {PolicyName} (ID: {PolicyId})",
                policy.Name, policy.Id);

            return Ok(new
            {
                isSuccess = true,
                data = policy,
                message = "Escalation policy updated successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Policy not found: {PolicyId}", id);
            return NotFound(new
            {
                isSuccess = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating escalation policy {PolicyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the escalation policy"
            });
        }
    }

    /// <summary>
    /// Delete an escalation policy
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        try
        {
            var result = await _policyService.DeletePolicyAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Escalation policy with ID {id} not found"
                });
            }

            _logger.LogInformation("Deleted escalation policy {PolicyId}", id);

            return Ok(new
            {
                isSuccess = true,
                message = "Escalation policy deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting escalation policy {PolicyId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the escalation policy"
            });
        }
    }

    /// <summary>
    /// Test policy resolution for a given context
    /// This endpoint helps admins understand which policy will be applied for specific scenarios
    /// </summary>
    /// <param name="request">Policy resolution test request</param>
    /// <returns>Policy resolution result with explanation</returns>
    [HttpPost("resolve")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> TestPolicyResolution([FromBody] PolicyResolutionTestRequest request)
    {
        try
        {
            var result = await _policyService.TestPolicyResolution(request);

            return Ok(new
            {
                isSuccess = true,
                data = result,
                message = result.ResolvedPolicy != null
                    ? $"Resolved to policy: {result.ResolvedPolicy.Name}"
                    : "No matching policy found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing policy resolution");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while testing policy resolution"
            });
        }
    }

    /// <summary>
    /// Get policy resolution for a specific complaint context
    /// This returns the actual policy that would be applied to a complaint
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="branchId">Branch ID (optional)</param>
    /// <param name="departmentId">Department ID (optional)</param>
    /// <param name="sectionId">Section ID (optional)</param>
    /// <param name="categoryId">Category ID (optional)</param>
    /// <returns>Resolved policy</returns>
    [HttpGet("resolve")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> ResolvePolicy(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? categoryId = null)
    {
        try
        {
            var policy = await _policyService.ResolvePolicy(
                companyId, branchId, departmentId, sectionId, categoryId);

            if (policy == null)
            {
                return Ok(new
                {
                    isSuccess = true,
                    data = (EscalationPolicyDto?)null,
                    message = "No matching escalation policy found for the given context"
                });
            }

            // Map entity to DTO
            var policyDto = await _policyService.GetPolicyByIdAsync(policy.Id);

            return Ok(new
            {
                isSuccess = true,
                data = policyDto,
                message = $"Resolved to policy: {policy.Name}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving policy");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while resolving the escalation policy"
            });
        }
    }
}
