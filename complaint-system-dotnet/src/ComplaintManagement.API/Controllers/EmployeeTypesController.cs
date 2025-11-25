using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/employeetypes")]
[Authorize]
public class EmployeeTypesController : ControllerBase
{
    private readonly IEmployeeTypeRepository _employeeTypeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeTypesController> _logger;

    public EmployeeTypesController(
        IEmployeeTypeRepository employeeTypeRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeTypesController> logger)
    {
        _employeeTypeRepository = employeeTypeRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all employee types for a company
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "activeOnly" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetEmployeeTypes([FromQuery] Guid companyId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var employeeTypes = await _employeeTypeRepository.GetByCompanyAsync(companyId);

            if (activeOnly)
            {
                employeeTypes = employeeTypes.Where(e => e.IsActive);
            }

            var employeeTypeDtos = employeeTypes.Select(e => new
            {
                e.Id,
                e.CompanyId,
                e.Name,
                e.Code,
                e.Description,
                e.IsActive,
                e.CreatedAt,
                e.UpdatedAt
            }).OrderBy(e => e.Name);

            return Ok(new
            {
                isSuccess = true,
                data = employeeTypeDtos,
                message = $"Retrieved {employeeTypeDtos.Count()} employee types"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee types for company {CompanyId}", companyId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving employee types",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get employee type by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeTypeById(Guid id)
    {
        try
        {
            var employeeType = await _employeeTypeRepository.GetByIdAsync(id);
            if (employeeType == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Employee type not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    employeeType.Id,
                    employeeType.CompanyId,
                    employeeType.Name,
                    employeeType.Code,
                    employeeType.Description,
                    employeeType.IsActive,
                    employeeType.CreatedAt,
                    employeeType.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee type {EmployeeTypeId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the employee type",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get employee type with users
    /// </summary>
    [HttpGet("{id}/hierarchy")]
    public async Task<IActionResult> GetEmployeeTypeHierarchy(Guid id)
    {
        try
        {
            var employeeType = await _employeeTypeRepository.GetEmployeeTypeWithUsersAsync(id);
            if (employeeType == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Employee type not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    employeeType.Id,
                    employeeType.Name,
                    employeeType.Code,
                    employeeType.Description,
                    employeeType.IsActive,
                    Users = employeeType.Users.Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.EmployeeCode,
                        u.Email,
                        u.IsActive
                    }).OrderBy(u => u.FullName)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee type hierarchy for {EmployeeTypeId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the employee type hierarchy",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Create a new employee type
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> CreateEmployeeType([FromBody] CreateEmployeeTypeRequest request)
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
            var existingEmployeeTypes = await _employeeTypeRepository.GetByCompanyAsync(request.CompanyId);
            if (existingEmployeeTypes.Any(e => e.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"An employee type with code '{request.Code}' already exists"
                });
            }

            var employeeType = new EmployeeType
            {
                Id = Guid.NewGuid(),
                CompanyId = request.CompanyId,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _employeeTypeRepository.AddAsync(employeeType);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee type created successfully: {EmployeeTypeName} ({EmployeeTypeCode})", employeeType.Name, employeeType.Code);

            return CreatedAtAction(
                nameof(GetEmployeeTypeById),
                new { id = employeeType.Id },
                new
                {
                    isSuccess = true,
                    message = "Employee type created successfully",
                    data = new
                    {
                        employeeType.Id,
                        employeeType.CompanyId,
                        employeeType.Name,
                        employeeType.Code,
                        employeeType.Description,
                        employeeType.IsActive,
                        employeeType.CreatedAt
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee type");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the employee type",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update an existing employee type
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> UpdateEmployeeType(Guid id, [FromBody] UpdateEmployeeTypeRequest request)
    {
        try
        {
            var employeeType = await _employeeTypeRepository.GetByIdAsync(id);
            if (employeeType == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Employee type not found"
                });
            }

            // Check for duplicate code (excluding current employee type)
            var existingEmployeeTypes = await _employeeTypeRepository.GetByCompanyAsync(employeeType.CompanyId);
            if (existingEmployeeTypes.Any(e => e.Id != id && e.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"An employee type with code '{request.Code}' already exists"
                });
            }

            // Update employee type properties
            employeeType.Name = request.Name;
            employeeType.Code = request.Code;
            employeeType.Description = request.Description;
            employeeType.IsActive = request.IsActive;
            employeeType.UpdatedAt = DateTime.UtcNow;

            _employeeTypeRepository.Update(employeeType);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee type updated successfully: {EmployeeTypeName} ({EmployeeTypeId})", employeeType.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Employee type updated successfully",
                data = new
                {
                    employeeType.Id,
                    employeeType.CompanyId,
                    employeeType.Name,
                    employeeType.Code,
                    employeeType.Description,
                    employeeType.IsActive,
                    employeeType.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee type {EmployeeTypeId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the employee type",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Delete an employee type
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> DeleteEmployeeType(Guid id)
    {
        try
        {
            var employeeType = await _employeeTypeRepository.GetEmployeeTypeWithUsersAsync(id);
            if (employeeType == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Employee type not found"
                });
            }

            // Check if employee type has users
            if (employeeType.Users.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete employee type that has users. Please reassign users first.",
                    errors = new[] { $"Employee type has {employeeType.Users.Count} user(s)" }
                });
            }

            _employeeTypeRepository.Delete(employeeType);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Employee type deleted successfully: {EmployeeTypeName} ({EmployeeTypeId})", employeeType.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Employee type deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee type {EmployeeTypeId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the employee type",
                errors = new[] { ex.Message }
            });
        }
    }
}

/// <summary>
/// Request model for creating an employee type
/// </summary>
public class CreateEmployeeTypeRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating an employee type
/// </summary>
public class UpdateEmployeeTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
