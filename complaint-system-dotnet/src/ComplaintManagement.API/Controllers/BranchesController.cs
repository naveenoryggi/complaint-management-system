using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(
        IBranchRepository branchRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ILogger<BranchesController> logger)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private object MapBranchToDto(Branch branch)
    {
        return new
        {
            branch.Id,
            branch.CompanyId,
            branch.Name,
            branch.Code,
            branch.Description,
            branch.ContactEmail,
            branch.ContactPhone,
            branch.Address,
            branch.City,
            branch.Country,
            branch.ManagerId,
            ManagerName = branch.Manager != null ? $"{branch.Manager.FirstName} {branch.Manager.LastName}" : null,
            branch.SecondaryManagerId,
            SecondaryManagerName = branch.SecondaryManager != null ? $"{branch.SecondaryManager.FirstName} {branch.SecondaryManager.LastName}" : null,
            branch.HrResponsibleId,
            HrResponsibleName = branch.HrResponsible != null ? $"{branch.HrResponsible.FirstName} {branch.HrResponsible.LastName}" : null,
            branch.IsActive,
            branch.CreatedAt,
            branch.UpdatedAt
        };
    }

    /// <summary>
    /// Get all branches for a company
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "companyId", "activeOnly" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetBranches([FromQuery] Guid companyId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var branches = await _branchRepository.GetByCompanyAsync(companyId);

            if (activeOnly)
            {
                branches = branches.Where(b => b.IsActive);
            }

            var branchDtos = branches
                .OrderBy(b => b.Name)
                .Select(b => MapBranchToDto(b));

            return Ok(new
            {
                isSuccess = true,
                data = branchDtos,
                message = $"Retrieved {branchDtos.Count()} branches"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches for company {CompanyId}", companyId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving branches",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get branch by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBranchById(Guid id)
    {
        try
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Branch not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = MapBranchToDto(branch)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch {BranchId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the branch",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get branch with departments
    /// </summary>
    [HttpGet("{id}/hierarchy")]
    public async Task<IActionResult> GetBranchHierarchy(Guid id)
    {
        try
        {
            var branch = await _branchRepository.GetBranchWithDepartmentsAsync(id);
            if (branch == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Branch not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    branch.Id,
                    branch.Name,
                    branch.Code,
                    branch.Description,
                    branch.Address,
                    branch.City,
                    branch.Country,
                    branch.IsActive,
                    Departments = branch.Departments.Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.Code,
                        d.Description,
                        d.IsActive
                    }).OrderBy(d => d.Name)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch hierarchy for {BranchId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the branch hierarchy",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        try
        {
            // Validate company exists
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Company not found"
                });
            }

            // Check for duplicate code
            var existingBranches = await _branchRepository.GetByCompanyAsync(request.CompanyId);
            if (existingBranches.Any(b => b.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A branch with code '{request.Code}' already exists"
                });
            }

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                CompanyId = request.CompanyId,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                ManagerId = request.ManagerId,
                SecondaryManagerId = request.SecondaryManagerId,
                HrResponsibleId = request.HrResponsibleId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _branchRepository.AddAsync(branch);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Branch created successfully: {BranchName} ({BranchCode})", branch.Name, branch.Code);

            // Reload to get navigation properties
            var createdBranch = await _branchRepository.GetByIdAsync(branch.Id);

            return CreatedAtAction(
                nameof(GetBranchById),
                new { id = branch.Id },
                new
                {
                    isSuccess = true,
                    message = "Branch created successfully",
                    data = MapBranchToDto(createdBranch!)
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the branch",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update an existing branch
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchRequest request)
    {
        try
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Branch not found"
                });
            }

            // Check for duplicate code (excluding current branch)
            var existingBranches = await _branchRepository.GetByCompanyAsync(branch.CompanyId);
            if (existingBranches.Any(b => b.Id != id && b.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A branch with code '{request.Code}' already exists"
                });
            }

            // Update branch properties
            branch.Name = request.Name;
            branch.Code = request.Code;
            branch.Description = request.Description;
            branch.ContactEmail = request.ContactEmail;
            branch.ContactPhone = request.ContactPhone;
            branch.Address = request.Address;
            branch.City = request.City;
            branch.Country = request.Country;
            branch.ManagerId = request.ManagerId;
            branch.SecondaryManagerId = request.SecondaryManagerId;
            branch.HrResponsibleId = request.HrResponsibleId;
            branch.IsActive = request.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            _branchRepository.Update(branch);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Branch updated successfully: {BranchName} ({BranchId})", branch.Name, id);

            // Reload to get navigation properties
            var updatedBranch = await _branchRepository.GetByIdAsync(id);

            return Ok(new
            {
                isSuccess = true,
                message = "Branch updated successfully",
                data = MapBranchToDto(updatedBranch!)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {BranchId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the branch",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Delete a branch
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        try
        {
            var branch = await _branchRepository.GetBranchWithDepartmentsAsync(id);
            if (branch == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Branch not found"
                });
            }

            // Check if branch has departments
            if (branch.Departments.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete branch that has departments. Please delete or reassign departments first.",
                    errors = new[] { $"Branch has {branch.Departments.Count} department(s)" }
                });
            }

            // Check if branch has users
            if (branch.Users.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete branch that has users. Please reassign users first.",
                    errors = new[] { $"Branch has {branch.Users.Count} user(s)" }
                });
            }

            _branchRepository.Delete(branch);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Branch deleted successfully: {BranchName} ({BranchId})", branch.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Branch deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch {BranchId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the branch",
                errors = new[] { ex.Message }
            });
        }
    }
}

/// <summary>
/// Request model for creating a branch
/// </summary>
public class CreateBranchRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating a branch
/// </summary>
public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}
