using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Features.Complaints.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Services.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplaintsController> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IComplaintAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ComplaintsController(
        IMediator mediator,
        ILogger<ComplaintsController> logger,
        IFileStorageService fileStorageService,
        IComplaintAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _fileStorageService = fileStorageService;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get paginated list of complaints with optional filters
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="statusMasterId">Filter by status master ID</param>
    /// <param name="priorityMasterId">Filter by priority master ID</param>
    /// <param name="companyId">Filter by company</param>
    /// <param name="assignedToId">Filter by assigned user</param>
    /// <param name="complainantId">Filter by complainant user</param>
    /// <param name="searchTerm">Search in title and description</param>
    /// <returns>Paginated list of complaints</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<ComplaintDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComplaints(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? statusMasterId = null,
        [FromQuery] Guid? priorityMasterId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] Guid? complainantId = null,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            // SECURITY: Get current user ID and roles from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            // SECURITY: Enforce role-based access control
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            bool isAdmin = permissions.Contains("ManageUsers") || permissions.Contains("ManageSettings") || permissions.Contains("ManageCompany");
            bool isHandler = permissions.Contains("AssignComplaint") || permissions.Contains("EscalateComplaint");

            // SECURITY: Override filters based on user role to prevent unauthorized access
            if (isAdmin)
            {
                // Admin: Can see all complaints (use provided filters as-is)
                _logger.LogInformation("Admin user {UserId} accessing complaints with admin privileges", currentUserId);
            }
            else if (isHandler)
            {
                // Handler: Can ONLY see complaints assigned to them (override any provided assignedToId)
                assignedToId = currentUserId;
                complainantId = null; // Clear complainantId to prevent seeing other users' complaints
                _logger.LogInformation("Handler user {UserId} accessing complaints assigned to them", currentUserId);
            }
            else
            {
                // Complainant: Can ONLY see their own complaints (override any provided complainantId)
                complainantId = currentUserId;
                assignedToId = null; // Clear assignedToId to prevent unauthorized access
                _logger.LogInformation("Complainant user {UserId} accessing their own complaints", currentUserId);
            }

            var query = new GetComplaintsQuery
            {
                Page = page,
                PageSize = pageSize,
                StatusMasterId = statusMasterId,
                PriorityMasterId = priorityMasterId,
                CompanyId = companyId,
                AssignedToId = assignedToId,
                ComplainantId = complainantId,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving complaints");
            return StatusCode(500, new { message = "An error occurred while retrieving complaints" });
        }
    }

    /// <summary>
    /// Get complaint by ID
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <returns>Complaint details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComplaintById(Guid id)
    {
        try
        {
            var query = new GetComplaintByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the complaint" });
        }
    }

    /// <summary>
    /// Create a new complaint
    /// </summary>
    /// <param name="request">Complaint creation request</param>
    /// <returns>Created complaint</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintRequest request)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(companyIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new CreateComplaintCommand
            {
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Priority = request.Priority,
                BranchId = request.BranchId,
                DepartmentId = request.DepartmentId,
                SectionId = request.SectionId,
                IsAnonymous = request.IsAnonymous,
                Tags = request.Tags,
                ComplainantId = Guid.Parse(userIdClaim),
                CompanyId = Guid.Parse(companyIdClaim),
                EmployeeCode = request.EmployeeCode,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                AlternatePhone = request.AlternatePhone,
                PreferredContactMethod = request.PreferredContactMethod
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint created successfully: {ComplaintId}", result.Data?.Id);
            return CreatedAtAction(nameof(GetComplaintById), new { id = result.Data?.Id }, result);
        }
        catch (ValidationException vex)
        {
            var errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }).ToList();
            var errorDetails = string.Join(", ", errors.Select(e => $"{e.field}: {e.message}"));
            _logger.LogWarning(vex, "Validation failed while creating complaint. Errors: {ValidationErrors}", errorDetails);
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating complaint");
            return StatusCode(500, new { message = "An error occurred while creating the complaint" });
        }
    }

    /// <summary>
    /// Update an existing complaint
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="request">Complaint update request</param>
    /// <returns>Updated complaint</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateComplaint(Guid id, [FromBody] UpdateComplaintRequest request)
    {
        try
        {
            var command = new UpdateComplaintCommand
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                PriorityMasterId = request.PriorityMasterId,
                StatusMasterId = request.StatusMasterId,
                AssignedToId = request.AssignedToId,
                ResolutionNotes = request.ResolutionNotes,
                Tags = request.Tags
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Complaint updated successfully: {ComplaintId}", id);
            return Ok(result);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation failed while updating complaint {ComplaintId}", id);
            var errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the complaint" });
        }
    }

    /// <summary>
    /// Delete a complaint (soft delete)
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteComplaint(Guid id)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new DeleteComplaintCommand
            {
                ComplaintId = id,
                DeletedById = Guid.Parse(currentUserIdClaim)
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} deleted by user {UserId}", id, currentUserIdClaim);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the complaint" });
        }
    }

    /// <summary>
    /// Assign complaint to a user
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="userId">User ID to assign</param>
    /// <returns>Updated complaint</returns>
    [HttpPost("{id}/assign/{userId}")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignComplaint(Guid id, Guid userId)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new AssignComplaintCommand
            {
                ComplaintId = id,
                AssignedToId = userId,
                AssignedById = Guid.Parse(currentUserIdClaim)
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} assigned to user {UserId}", id, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while assigning the complaint" });
        }
    }

    /// <summary>
    /// Escalate complaint to next level
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <returns>Updated complaint</returns>
    [HttpPost("{id}/escalate")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EscalateComplaint(Guid id, [FromBody] EscalateComplaintRequest request)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new EscalateComplaintCommand
            {
                ComplaintId = id,
                EscalatedById = Guid.Parse(currentUserIdClaim),
                Reason = request?.Reason ?? "Escalation requested"
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} escalated", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while escalating complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while escalating the complaint" });
        }
    }

    /// <summary>
    /// Close complaint with resolution
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="resolutionNotes">Resolution notes</param>
    /// <returns>Updated complaint</returns>
    [HttpPut("{id}/close")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CloseComplaint(Guid id, [FromBody] string resolutionNotes)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new CloseComplaintCommand
            {
                ComplaintId = id,
                ClosedById = Guid.Parse(currentUserIdClaim),
                ResolutionNotes = resolutionNotes ?? "Complaint resolved"
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} closed", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while closing complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while closing the complaint" });
        }
    }

    /// <summary>
    /// Reopen a closed complaint
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="reason">Reason for reopening</param>
    /// <returns>Updated complaint</returns>
    [HttpPut("{id}/reopen")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReopenComplaint(Guid id, [FromBody] ReopenRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new ReopenComplaintCommand
            {
                ComplaintId = id,
                ReopenedById = Guid.Parse(userId),
                Reason = request?.Reason ?? string.Empty
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reopening complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while reopening the complaint" });
        }
    }

    // DTO for reopen request
    public class ReopenRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Upload attachments for a complaint
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="files">Files to upload (max 10 files, 5MB each)</param>
    /// <returns>List of uploaded attachments</returns>
    [HttpPost("{id}/attachments")]
    [RequestSizeLimit(52428800)] // 50MB total (10 files * 5MB)
    [ProducesResponseType(typeof(Result<List<ComplaintAttachment>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadAttachments(Guid id, [FromForm] List<IFormFile> files)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var userId = Guid.Parse(userIdClaim);

            // Validate files
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files provided" });
            }

            if (files.Count > 10)
            {
                return BadRequest(new { message = "Maximum 10 files allowed per upload" });
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            foreach (var file in files)
            {
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = $"File '{file.FileName}' exceeds maximum size of 5MB" });
                }
            }

            // Check if complaint exists
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(id);
            if (complaint == null)
            {
                return NotFound(new { message = "Complaint not found" });
            }

            var uploadedAttachments = new List<ComplaintAttachment>();

            foreach (var file in files)
            {
                try
                {
                    // Upload file to storage
                    var filePath = await _fileStorageService.UploadFileAsync(file, "complaints");

                    // Create attachment record
                    var attachment = new ComplaintAttachment
                    {
                        Id = Guid.NewGuid(),
                        ComplaintId = id,
                        FileName = file.FileName,
                        StoredFileName = Path.GetFileName(filePath),
                        FilePath = filePath,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                        UploadedBy = userId,
                        UploadedAt = DateTime.UtcNow,
                        Description = null,
                        IsThumbnail = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    await _attachmentRepository.AddAsync(attachment);
                    uploadedAttachments.Add(attachment);

                    _logger.LogInformation("File uploaded successfully: {FileName} for complaint {ComplaintId}", file.FileName, id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName} for complaint {ComplaintId}", file.FileName, id);
                    // Continue with other files even if one fails
                }
            }

            // Save all changes
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("{Count} attachments uploaded for complaint {ComplaintId}", uploadedAttachments.Count, id);
            return Ok(Result<List<ComplaintAttachment>>.Success(uploadedAttachments, $"{uploadedAttachments.Count} file(s) uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading attachments for complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while uploading attachments" });
        }
    }

    /// <summary>
    /// Get attachments for a complaint
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <returns>List of attachments</returns>
    [HttpGet("{id}/attachments")]
    [ProducesResponseType(typeof(Result<IEnumerable<ComplaintAttachment>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAttachments(Guid id)
    {
        try
        {
            // Return empty list if complaint doesn't exist (consistent with Get Comments behavior)
            var attachments = await _attachmentRepository.GetAttachmentsByComplaintAsync(id);
            return Ok(Result<IEnumerable<ComplaintAttachment>>.Success(attachments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving attachments for complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving attachments" });
        }
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="attachmentId">Attachment ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}/attachments/{attachmentId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
    {
        try
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null || attachment.ComplaintId != id)
            {
                return NotFound(new { message = "Attachment not found" });
            }

            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // Delete attachment record
            _attachmentRepository.Delete(attachment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} deleted from complaint {ComplaintId}", attachmentId, id);
            return Ok(Result.Success("Attachment deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting attachment {AttachmentId} from complaint {ComplaintId}", attachmentId, id);
            return StatusCode(500, new { message = "An error occurred while deleting the attachment" });
        }
    }

    /// <summary>
    /// Get complaint activity history
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <returns>Complaint history with all events</returns>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(Result<ComplaintHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetComplaintHistory(Guid id)
    {
        try
        {
            var query = new GetComplaintHistoryQuery { ComplaintId = id };
            var result = await _mediator.Send(query);

            // Handler always returns success with empty history if complaint doesn't exist
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving history for complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving complaint history" });
        }
    }

    #region Advanced Assignment Endpoints

    /// <summary>
    /// Auto-assign complaint using intelligent assignment engine
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="request">Auto-assignment request</param>
    /// <returns>Updated complaint</returns>
    [HttpPost("{id}/auto-assign")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AutoAssignComplaint(Guid id, [FromBody] AutoAssignComplaintRequest request)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new AutoAssignComplaintCommand
            {
                ComplaintId = id,
                TriggeredById = Guid.Parse(currentUserIdClaim),
                ForceAssignment = request.ForceAssignment,
                Notes = request.Notes,
                Context = new AssignmentContext
                {
                    AssignmentReason = request.AssignmentReason ?? "Manual Auto-Assignment",
                    BypassRules = request.BypassRules,
                    OverrideAssignmentMethod = request.OverrideAssignmentMethod,
                    ForceAssignment = request.ForceAssignment
                }
            };

            if (request.Criteria != null)
            {
                command.Criteria = new AssignmentCriteria
                {
                    OrganizationalScope = new OrganizationalScope
                    {
                        CompanyIds = request.Criteria.CompanyIds ?? new List<Guid>(),
                        BranchIds = request.Criteria.BranchIds ?? new List<Guid>(),
                        DepartmentIds = request.Criteria.DepartmentIds ?? new List<Guid>(),
                        SectionIds = request.Criteria.SectionIds ?? new List<Guid>()
                    },
                    ComplaintCriteria = new ComplaintCriteria
                    {
                        CategoryIds = request.Criteria.CategoryIds ?? new List<Guid>(),
                        PriorityRange = new PriorityRange
                        {
                            Min = request.Criteria.MinPriorityLevel ?? 0,
                            Max = request.Criteria.MaxPriorityLevel ?? 4
                        }
                    },
                    WorkloadCriteria = new WorkloadCriteria
                    {
                        MaxCurrentWorkload = request.Criteria.MaxCurrentWorkload ?? 10,
                        MinSuccessRate = request.Criteria.MinSuccessRate ?? 0.7m
                    },
                    MaxCandidates = request.Criteria.MaxCandidates ?? 10
                };
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} auto-assigned successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while auto-assigning complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while auto-assigning the complaint" });
        }
    }

    /// <summary>
    /// Assign complaint to a resource pool with intelligent user selection
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="request">Pool assignment request</param>
    /// <returns>Updated complaint</returns>
    [HttpPost("{id}/assign-to-pool")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignComplaintToPool(Guid id, [FromBody] AssignToPoolRequest request)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new AssignComplaintToResourcePoolCommand
            {
                ComplaintId = id,
                ResourcePoolId = request.ResourcePoolId,
                AssignedById = Guid.Parse(currentUserIdClaim),
                AssignmentMethod = request.AssignmentMethod ?? AdvancedAssignmentMethod.BestFit,
                SpecificUserId = request.SpecificUserId,
                ValidateBeforeAssignment = request.ValidateBeforeAssignment ?? true,
                Notes = request.Notes,
                Context = new AssignmentContext
                {
                    AssignmentReason = request.AssignmentReason ?? "Manual Pool Assignment",
                    IsEscalation = request.IsEscalation ?? false,
                    BypassRules = request.BypassRules ?? false,
                    ForceAssignment = request.ForceAssignment ?? false
                }
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} assigned to pool {PoolId} successfully", id, request.ResourcePoolId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning complaint {ComplaintId} to pool {PoolId}", id, request.ResourcePoolId);
            return StatusCode(500, new { message = "An error occurred while assigning the complaint to the pool" });
        }
    }

    /// <summary>
    /// Reassign complaint to a different user or pool
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="request">Reassignment request</param>
    /// <returns>Updated complaint</returns>
    [HttpPost("{id}/reassign")]
    [ProducesResponseType(typeof(Result<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReassignComplaint(Guid id, [FromBody] ReassignComplaintRequest request)
    {
        try
        {
            // Get current user ID from claims
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var command = new ReassignComplaintCommand
            {
                ComplaintId = id,
                CurrentAssignedUserId = request.CurrentAssignedUserId,
                NewAssignedUserId = request.NewAssignedUserId,
                NewResourcePoolId = request.NewResourcePoolId,
                ReassignedById = Guid.Parse(currentUserIdClaim),
                ReassignmentReason = request.ReassignmentReason ?? "Reassignment requested",
                AssignmentMethod = request.AssignmentMethod ?? AdvancedAssignmentMethod.BestFit,
                KeepAssignmentHistory = request.KeepAssignmentHistory ?? true,
                Notes = request.Notes,
                Context = new AssignmentContext
                {
                    AssignmentReason = $"Reassignment: {request.ReassignmentReason}",
                    IsInitialAssignment = false,
                    BypassRules = request.BypassRules ?? false
                }
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Complaint {ComplaintId} reassigned successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reassigning complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while reassigning the complaint" });
        }
    }

    /// <summary>
    /// Get assignment candidates for a complaint
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="maxCandidates">Maximum number of candidates to return</param>
    /// <param name="includeUserDetails">Whether to include detailed user information</param>
    /// <param name="calculateSkillScores">Whether to calculate skill match scores</param>
    /// <returns>List of assignment candidates</returns>
    [HttpGet("{id}/assignment-candidates")]
    [ProducesResponseType(typeof(Result<List<ResourcePoolCandidate>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAssignmentCandidates(Guid id, [FromQuery] int maxCandidates = 10,
        [FromQuery] bool includeUserDetails = true, [FromQuery] bool calculateSkillScores = true)
    {
        try
        {
            var query = new GetAssignmentCandidatesQuery
            {
                ComplaintId = id,
                MaxCandidates = maxCandidates,
                IncludeUserDetails = includeUserDetails,
                CalculateSkillScores = calculateSkillScores
            };

            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Found {Count} assignment candidates for complaint {ComplaintId}",
                result.Data?.Count ?? 0, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting assignment candidates for complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while getting assignment candidates" });
        }
    }

    /// <summary>
    /// Validate a potential assignment
    /// </summary>
    /// <param name="id">Complaint ID</param>
    /// <param name="userId">User ID to validate (optional)</param>
    /// <param name="poolId">Resource pool ID to validate (optional)</param>
    /// <param name="detailedValidation">Whether to perform detailed validation</param>
    /// <returns>Validation result</returns>
    [HttpPost("{id}/validate-assignment")]
    [ProducesResponseType(typeof(Result<AssignmentValidationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateAssignment(Guid id, [FromBody] ValidateAssignmentRequest request)
    {
        try
        {
            var query = new ValidateAssignmentQuery
            {
                ComplaintId = id,
                UserId = request.UserId,
                ResourcePoolId = request.PoolId,
                Context = new AssignmentContext
                {
                    IsEscalation = request.IsEscalation ?? false,
                    ForceAssignment = request.ForceAssignment ?? false
                },
                DetailedValidation = request.DetailedValidation ?? true
            };

            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Assignment validation completed for complaint {ComplaintId}. Valid: {IsValid}",
                id, result.Data?.IsValid ?? false);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating assignment for complaint {ComplaintId}", id);
            return StatusCode(500, new { message = "An error occurred while validating the assignment" });
        }
    }

    #endregion
}
