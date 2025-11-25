using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/sections")]
[Authorize]
public class SectionsController : ControllerBase
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SectionsController> _logger;

    public SectionsController(
        ISectionRepository sectionRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<SectionsController> logger)
    {
        _sectionRepository = sectionRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all sections for a department
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "departmentId", "activeOnly" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetSections([FromQuery] Guid departmentId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var sections = await _sectionRepository.GetByDepartmentAsync(departmentId);

            if (activeOnly)
            {
                sections = sections.Where(s => s.IsActive);
            }

            var sectionDtos = sections.Select(s => new
            {
                s.Id,
                s.DepartmentId,
                s.Name,
                s.Code,
                s.Description,
                s.HeadId,
                HeadName = s.Head != null ? s.Head.FullName : null,
                s.SecondaryHeadId,
                SecondaryHeadName = s.SecondaryHead != null ? s.SecondaryHead.FullName : null,
                s.HrResponsibleId,
                HrResponsibleName = s.HrResponsible != null ? s.HrResponsible.FullName : null,
                s.IsActive,
                s.CreatedAt,
                s.UpdatedAt
            }).OrderBy(s => s.Name);

            return Ok(new
            {
                isSuccess = true,
                data = sectionDtos,
                message = $"Retrieved {sectionDtos.Count()} sections"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sections for department {DepartmentId}", departmentId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving sections",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get section by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSectionById(Guid id)
    {
        try
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Section not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    section.Id,
                    section.DepartmentId,
                    section.Name,
                    section.Code,
                    section.Description,
                    section.HeadId,
                    section.SecondaryHeadId,
                    section.HrResponsibleId,
                    section.IsActive,
                    section.CreatedAt,
                    section.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving section {SectionId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the section",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Create a new section
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequest request)
    {
        try
        {
            // Validate department exists
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Department not found"
                });
            }

            // Check for duplicate code within the department
            var existingSections = await _sectionRepository.GetByDepartmentAsync(request.DepartmentId);
            if (existingSections.Any(s => s.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A section with code '{request.Code}' already exists in this department"
                });
            }

            var section = new Section
            {
                Id = Guid.NewGuid(),
                DepartmentId = request.DepartmentId,
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                HeadId = request.HeadId,
                SecondaryHeadId = request.SecondaryHeadId,
                HrResponsibleId = request.HrResponsibleId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _sectionRepository.AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Section created successfully: {SectionName} ({SectionCode})", section.Name, section.Code);

            return CreatedAtAction(
                nameof(GetSectionById),
                new { id = section.Id },
                new
                {
                    isSuccess = true,
                    message = "Section created successfully",
                    data = new
                    {
                        section.Id,
                        section.DepartmentId,
                        section.Name,
                        section.Code,
                        section.Description,
                        section.HeadId,
                        section.SecondaryHeadId,
                        section.HrResponsibleId,
                        section.IsActive,
                        section.CreatedAt
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating section");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the section",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Update an existing section
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionRequest request)
    {
        try
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Section not found"
                });
            }

            // Check for duplicate code (excluding current section)
            var existingSections = await _sectionRepository.GetByDepartmentAsync(section.DepartmentId);
            if (existingSections.Any(s => s.Id != id && s.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"A section with code '{request.Code}' already exists in this department"
                });
            }

            // Update section properties
            section.Name = request.Name;
            section.Code = request.Code;
            section.Description = request.Description;
            section.HeadId = request.HeadId;
            section.SecondaryHeadId = request.SecondaryHeadId;
            section.HrResponsibleId = request.HrResponsibleId;
            section.IsActive = request.IsActive;
            section.UpdatedAt = DateTime.UtcNow;

            _sectionRepository.Update(section);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Section updated successfully: {SectionName} ({SectionId})", section.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Section updated successfully",
                data = new
                {
                    section.Id,
                    section.DepartmentId,
                    section.Name,
                    section.Code,
                    section.Description,
                    section.HeadId,
                    section.SecondaryHeadId,
                    section.HrResponsibleId,
                    section.IsActive,
                    section.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating section {SectionId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the section",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Delete a section
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> DeleteSection(Guid id)
    {
        try
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Section not found"
                });
            }

            // Check if section has users
            if (section.Users.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Cannot delete section that has users. Please reassign users first.",
                    errors = new[] { $"Section has {section.Users.Count} user(s)" }
                });
            }

            _sectionRepository.Delete(section);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Section deleted successfully: {SectionName} ({SectionId})", section.Name, id);

            return Ok(new
            {
                isSuccess = true,
                message = "Section deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting section {SectionId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the section",
                errors = new[] { ex.Message }
            });
        }
    }
}

/// <summary>
/// Request model for creating a section
/// </summary>
public class CreateSectionRequest
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadId { get; set; }
    public Guid? SecondaryHeadId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating a section
/// </summary>
public class UpdateSectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadId { get; set; }
    public Guid? SecondaryHeadId { get; set; }
    public Guid? HrResponsibleId { get; set; }
    public bool IsActive { get; set; } = true;
}
