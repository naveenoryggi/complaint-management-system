using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Common.Helpers;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IAutoResponseService _autoResponseService;
    private readonly ISLACalculatorService _slaCalculator;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ILogger<CreateComplaintCommandHandler> _logger;

    public CreateComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher,
        IAutoResponseService autoResponseService,
        ISLACalculatorService slaCalculator,
        IWorkflowEngine workflowEngine,
        ILogger<CreateComplaintCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
        _autoResponseService = autoResponseService;
        _slaCalculator = slaCalculator;
        _workflowEngine = workflowEngine;
        _logger = logger;
    }

    public async Task<Result<ComplaintDto>> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _unitOfWork.ComplaintCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<ComplaintDto>.Failure("Category not found", "Invalid category ID");
        }

        // Validate user exists and load with manager details for auto-population
        var user = await _unitOfWork.Users.GetByIdAsync(request.ComplainantId, cancellationToken);
        if (user == null)
        {
            return Result<ComplaintDto>.Failure("User not found", "Invalid user ID");
        }

        // Auto-populate organizational hierarchy from user if not provided
        var branchId = request.BranchId ?? user.BranchId;
        var departmentId = request.DepartmentId ?? user.DepartmentId;
        var sectionId = request.SectionId ?? user.SectionId;

        // Auto-populate contact information from user if not provided
        var employeeCode = request.EmployeeCode ?? user.EmployeeCode;
        var contactEmail = request.ContactEmail ?? user.Email;
        var contactPhone = request.ContactPhone ?? user.Phone;
        var alternatePhone = request.AlternatePhone;
        var preferredContactMethod = request.PreferredContactMethod;

        // Map priority enum to priority master ID using helper
        Guid? priorityMasterId = PriorityMasterHelper.GetPriorityMasterId(request.Priority);

        // Resolve timezone hierarchy: User -> Branch -> Company -> UTC
        // This ensures SLA calculations use the correct timezone for business hours
        var branch = user.BranchId.HasValue
            ? await _unitOfWork.Branches.GetByIdAsync(user.BranchId.Value, cancellationToken)
            : null;

        var company = await _unitOfWork.Companies.GetByIdAsync(
            request.CompanyId,
            cancellationToken);

        string effectiveTimeZone = user.PreferredTimeZone
            ?? branch?.TimeZone
            ?? company?.DefaultTimeZone
            ?? "UTC";

        _logger.LogInformation(
            "Resolved timezone for complaint: User={UserTz}, Branch={BranchTz}, Company={CompanyTz}, Effective={EffectiveTz}",
            user.PreferredTimeZone ?? "null",
            branch?.TimeZone ?? "null",
            company?.DefaultTimeZone ?? "null",
            effectiveTimeZone);

        // Calculate SLA deadline using the new SLA calculator service with timezone awareness
        // This will check for Priority/Category mappings, SLA levels, and fall back to system defaults
        var submittedAt = DateTime.UtcNow;
        var slaResult = await _slaCalculator.CalculateSLADeadlineAsync(
            request.CategoryId,
            priorityMasterId,
            request.CompanyId,
            submittedAt,
            effectiveTimeZone,
            cancellationToken);

        // Use SLA calculator result, or default to 48 hours if not configured
        var dueDate = slaResult.PrimaryDeadline ?? submittedAt.AddHours(48);

        // Generate unique complaint number using atomic database sequence
        // No retry logic needed - database MERGE operation is atomic and thread-safe
        var complaintNumber = await _unitOfWork.Complaints.GenerateComplaintNumberAsync(cancellationToken);

        // Get initial status using workflow engine (supports category-specific workflows)
        // Falls back to global "SUBMITTED" status if no custom workflow configured
        var initialStatus = await _workflowEngine.GetInitialStatusAsync(request.CategoryId);

        // Create complaint
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ComplaintNumber = complaintNumber,
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            ComplainantId = request.ComplainantId,
            CompanyId = request.CompanyId,
            BranchId = branchId,
            DepartmentId = departmentId,
            SectionId = sectionId,
            EmployeeCode = employeeCode,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            AlternatePhone = alternatePhone,
            PreferredContactMethod = preferredContactMethod,
            StatusMasterId = initialStatus.Id, // Use workflow engine's initial status
            PriorityMasterId = priorityMasterId!.Value, // Priority is required
            CurrentEscalationLevel = 0,
            SubmittedAt = submittedAt,
            DueDate = dueDate,
            IsAnonymous = request.IsAnonymous,
            Tags = request.Tags,
            CreatedAt = submittedAt
        };

        await _unitOfWork.Complaints.AddAsync(complaint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch COMPLAINT_CREATED notification
        try
        {
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_CREATED",
                complaint.Id,
                new Dictionary<string, object>
                {
                    ["complaintId"] = complaint.Id.ToString(),
                    ["complaintNumber"] = complaintNumber,
                    ["title"] = request.Title,
                    ["description"] = request.Description,
                    ["categoryName"] = category.Name,
                    ["priorityName"] = "Normal", // Will be replaced by actual priority name from master
                    ["statusName"] = initialStatus.Name,
                    ["complainantName"] = user.FullName,
                    ["complainantEmail"] = contactEmail ?? string.Empty,
                    ["complainantEmployeeCode"] = employeeCode ?? string.Empty,
                    ["createdDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["dueDate"] = dueDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["companyName"] = user.Company?.Name ?? string.Empty
                },
                request.CompanyId,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            // Log notification error but don't fail the complaint creation
            // Notifications are not critical to the core operation
            _logger.LogError(ex,
                "Failed to dispatch COMPLAINT_CREATED notification for complaint {ComplaintId}. " +
                "Error: {ErrorMessage}. StackTrace: {StackTrace}",
                complaint.Id, ex.Message, ex.StackTrace);
        }

        // Send auto-response acknowledgment email (if enabled)
        try
        {
            await _autoResponseService.SendComplaintCreatedAutoResponseAsync(complaint, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log auto-response error but don't fail the complaint creation
            _logger.LogError(ex,
                "Failed to send auto-response for complaint {ComplaintId}. " +
                "Error: {ErrorMessage}",
                complaint.Id, ex.Message);
        }

        // Retrieve with includes for DTO mapping
        var createdComplaint = await _unitOfWork.Complaints.GetComplaintWithDetailsAsync(complaint.Id, cancellationToken);

        if (createdComplaint == null)
        {
            return Result<ComplaintDto>.Failure("Failed to retrieve created complaint", "Database error");
        }

        var dto = new ComplaintDto
        {
            Id = createdComplaint.Id,
            ComplaintNumber = createdComplaint.ComplaintNumber,
            Title = createdComplaint.Title,
            Description = createdComplaint.Description,
            CategoryId = createdComplaint.CategoryId,
            CategoryName = createdComplaint.Category.Name,
            ComplainantId = createdComplaint.ComplainantId,
            ComplainantName = createdComplaint.Complainant.FullName,
            ComplainantEmail = createdComplaint.Complainant.Email,
            CompanyId = createdComplaint.CompanyId,
            CompanyName = createdComplaint.Company.Name,
            BranchId = createdComplaint.BranchId,
            BranchName = createdComplaint.Branch?.Name,
            DepartmentId = createdComplaint.DepartmentId,
            DepartmentName = createdComplaint.Department?.Name,
            SectionId = createdComplaint.SectionId,
            SectionName = createdComplaint.Section?.Name,

            // Complainant organizational info
            ComplainantBranchId = createdComplaint.Complainant.BranchId,
            ComplainantBranchName = createdComplaint.Complainant.Branch?.Name,
            ComplainantDepartmentId = createdComplaint.Complainant.DepartmentId,
            ComplainantDepartmentName = createdComplaint.Complainant.Department?.Name,
            ComplainantSectionId = createdComplaint.Complainant.SectionId,
            ComplainantSectionName = createdComplaint.Complainant.Section?.Name,
            ComplainantEmployeeCode = createdComplaint.Complainant.EmployeeCode,

            // Contact information
            ContactEmail = createdComplaint.ContactEmail,
            ContactPhone = createdComplaint.ContactPhone,
            AlternatePhone = createdComplaint.AlternatePhone,
            PreferredContactMethod = createdComplaint.PreferredContactMethod,

            // Manager details
            ComplainantManagerId = createdComplaint.Complainant.ManagerId,
            ComplainantManagerName = createdComplaint.Complainant.Manager?.FullName,
            ComplainantManagerEmail = createdComplaint.Complainant.Manager?.Email,
            ComplainantManagerPhone = createdComplaint.Complainant.Manager?.Phone,

            Status = createdComplaint.StatusMaster?.Name ?? "Unknown",
            StatusId = createdComplaint.StatusMasterId,
            Priority = createdComplaint.PriorityMaster?.Name ?? "Unknown",
            PriorityId = createdComplaint.PriorityMasterId,
            CurrentEscalationLevel = createdComplaint.CurrentEscalationLevel,
            AssignedToId = createdComplaint.AssignedToId,
            AssignedToName = createdComplaint.AssignedTo?.FullName,
            SubmittedAt = createdComplaint.SubmittedAt,
            DueDate = createdComplaint.DueDate,
            ResolvedAt = createdComplaint.ResolvedAt,
            ClosedAt = createdComplaint.ClosedAt,
            ResolutionNotes = createdComplaint.ResolutionNotes,
            IsAnonymous = createdComplaint.IsAnonymous,
            Tags = createdComplaint.Tags,
            CommentCount = createdComplaint.Comments.Count,
            AttachmentCount = createdComplaint.Attachments.Count
        };

        return Result<ComplaintDto>.Success(dto, "Complaint created successfully");
    }
}
