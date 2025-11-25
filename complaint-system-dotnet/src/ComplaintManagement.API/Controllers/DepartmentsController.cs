using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IDepartmentRepository departmentRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DepartmentsController> logger)
    {
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments for a branch
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "branchId", "activeOnly" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid branchId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var departments = await _departmentRepository.GetByBranchAsync(branchId);

            if (activeOnly)
            {
                departments = departments.Where(d => d.IsActive);
            }

            var departmentDtos = departments.Select(d => new
            {
                d.Id,
                d.BranchId,
                d.Name,
                d.Code,
                d.Description,
                d.ManagerId,
                ManagerName = d.Manager != null ? d.Manager.FullName : null,
                d.SecondaryManagerId,
                SecondaryManagerName = d.SecondaryManager != null ? d.SecondaryManager.FullName : null,
                d.HrResponsibleId,
                HrResponsibleName = d.HrResponsible != null ? d.HrResponsible.FullName : null,
                d.IsActive,
                d.CreatedAt,
                d.UpdatedAt
            }).OrderBy(d => d.Name);

            return Ok(new
            {
                isSuccess = true,
                data = departmentDtos,
                message = $"Retrieved {departmentDtos.Count()} departments"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments for branch {BranchId}", branchId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving departments",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Department not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    department.Id,
                    department.BranchId,
                    department.Name,
                    department.Code,
                    department.Description,
                    department.ManagerId,
                    ManagerName = department.Manager?.FullName,
                    department.SecondaryManagerId,
                    SecondaryManagerName = department.SecondaryManager?.FullName,
                    department.HrResponsibleId,
                    HrResponsibleName = department.HrResponsible?.FullName,
                    department.IsActive,
                    department.CreatedAt,
                    department.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {DepartmentId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the department",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get department with sections
    /// </summary>
    [HttpGet("{id}/hierarchy")]
    public async Task<IActionResult> GetDepartmentHierarchy(Guid id)
    {
        try
        {
            var department = await _departmentRepository.GetDepartmentWithSectionsAsync(id);
            if (department == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Department not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    department.Id,
                    department.Name,
                    department.Code,
                    department.Description,
                    department.ManagerId,
                    ManagerName = department.Manager?.FullName,
                    department.SecondaryManagerId,
                    SecondaryManagerName = department.SecondaryManager?.FullName,
                    department.HrResponsibleId,
                    HrResponsibleName = department.HrResponsible?.FullName,
                    department.IsActive,
                    Sections = department.Sections.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Code,
                        s.Description,
                        s.IsActive
                    }).OrderBy(s => s.Name)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department hierarchy for {DepartmentId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the department hierarchy",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        try
        {
            // Validate branch exists
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Branch not found"
                });
            }

            // Check for duplicate code within the branch
            var existingDepartments = await _departmentRepository.GetByBranchAsync(request.BranchId);
            if (existingDepartments.Any(d => d.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A department with code '{request.Code}' already exists in this branch"
                });
            }

            var department = new Department
            {
                Id = Guid.NewGuid(),
                BranchId = request.BranchId,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                ManagerId = request.ManagerId,
                SecondaryManagerId = request.SecondaryManagerId,
                HrResponsibleId = request.HrResponsibleId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _departmentRepository.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department created successfully: {DepartmentName} ({DepartmentCode})", department.Name, department.Code);

            return CreatedAtAction(
                nameof(GetDepartmentById),
                new { id = department.Id },
                new
                {
                    isSuccess = true,
                    message = "Department created successfully",
                    data = new
                    {
                        department.Id,
                        department.BranchId,
                        department.Name,
                        department.Code,
                        department.Description,
                        department.ManagerId,
                        department.SecondaryManagerId,
                        department.HrResponsibleId,
                        department.IsActive,
                        department.CreatedAt
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the department",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Department not found"
                });
            }

            // Check for duplicate code (excluding current department)
            var existingDepartments = await _departmentRepository.GetByBranchAsync(department.BranchId);
            if (existingDepartments.Any(d => d.Id != id && d.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A department with code '{request.Code}' already exists in this branch"
                });
            }

            // Update department properties
            department.Name = request.Name;
            department.Code = request.Code;
            department.Description = request.Description;
            department.ManagerId = request.ManagerId;
            department.SecondaryManagerId = request.SecondaryManagerId;
            department.HrResponsibleId = request.HrResponsibleId;
            department.IsActive = request.IsActive;
            department.UpdatedAt = DateTime.UtcNow;

            _departmentRepository.Update(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department updated successfully: {DepartmentName} ({DepartmentId})", department.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Department updated successfully",
                data = new
                {
                    department.Id,
                    department.BranchId,
                    department.Name,
                    department.Code,
                    department.Description,
                    department.ManagerId,
                    department.SecondaryManagerId,
                    department.HrResponsibleId,
                    department.IsActive,
                    department.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the department",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        try
        {
            var department = await _departmentRepository.GetDepartmentWithSectionsAsync(id);
            if (department == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Department not found"
                });
            }

            // Check if department has sections
            if (department.Sections.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete department that has sections. Please delete or reassign sections first.",
                    errors = new[] { $"Department has {department.Sections.Count} section(s)" }
                });
            }

            // Check if department has users
            if (department.Users.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete department that has users. Please reassign users first.",
                    errors = new[] { $"Department has {department.Users.Count} user(s)" }
                });
            }

            _departmentRepository.Delete(department);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Department deleted successfully: {DepartmentName} ({DepartmentId})", department.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Department deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the department",
                errors = new[] { ex.Message }
            });
        }
    }
}

/// <summary>
/// Request model for creating a department
/// </summary>
public class CreateDepartmentRequest
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating a department
/// </summary>
public class UpdateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SecondaryManagerId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}
