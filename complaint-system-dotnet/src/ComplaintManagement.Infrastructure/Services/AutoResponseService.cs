using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Auto-Response Service - Sends automated emails based on complaint events
/// Integrates with EmailTicketingService, NotificationDispatcher, and Template System
/// </summary>
public class AutoResponseService : IAutoResponseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTicketingService _emailTicketingService;
    private readonly ITemplateService _templateService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutoResponseService> _logger;

    public AutoResponseService(
        IUnitOfWork unitOfWork,
        IEmailTicketingService emailTicketingService,
        ITemplateService templateService,
        INotificationDispatcher notificationDispatcher,
        IConfiguration configuration,
        ILogger<AutoResponseService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTicketingService = emailTicketingService;
        _templateService = templateService;
        _notificationDispatcher = notificationDispatcher;
        _configuration = configuration;
        _logger = logger;
    }

    #region Complaint Created Auto-Response

    public async Task<Result> SendComplaintCreatedAutoResponseAsync(
        Complaint complaint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if auto-response is enabled globally
            var autoResponseEnabled = _configuration.GetValue<bool>("AutoResponse:Enabled", true);
            if (!autoResponseEnabled)
            {
                _logger.LogDebug("Auto-response is disabled globally, skipping complaint created notification");
                return Result.Success("Auto-response disabled");
            }

            // Check if acknowledgment on web creation is enabled
            var sendOnWebCreation = _configuration.GetValue<bool>("AutoResponse:SendAcknowledgmentOnWebCreation", true);
            if (!sendOnWebCreation)
            {
                _logger.LogDebug("Auto-acknowledgment on web creation is disabled, skipping");
                return Result.Success("Auto-acknowledgment disabled for web creation");
            }

            // Load email configuration for company
            var emailConfigs = await _unitOfWork.Repository<EmailConfiguration>()
                .FindAsync(c => c.CompanyId == complaint.CompanyId && c.IsEnabled, cancellationToken);
            var emailConfig = emailConfigs.FirstOrDefault();

            if (emailConfig == null)
            {
                _logger.LogWarning("No active email configuration found for company {CompanyId}, cannot send auto-response",
                    complaint.CompanyId);
                return Result.Failure("No email configuration found");
            }

            // Check if auto-acknowledgment is enabled in email config
            if (!emailConfig.SendAutoAcknowledgement)
            {
                _logger.LogDebug("Auto-acknowledgment is disabled in email configuration for company {CompanyId}",
                    complaint.CompanyId);
                return Result.Success("Auto-acknowledgment disabled in email config");
            }

            // Load complaint with related data for template variables
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Get complainant email
            var complainantEmail = complaintWithDetails.Complainant?.Email ?? complaintWithDetails.ContactEmail;
            if (string.IsNullOrEmpty(complainantEmail))
            {
                _logger.LogWarning("No email address found for complainant, cannot send auto-response");
                return Result.Failure("No complainant email address");
            }

            // Send auto-acknowledgment using EmailTicketingService
            var result = await _emailTicketingService.SendAutoAcknowledgementAsync(
                complaint.Id,
                complainantEmail,
                emailConfig,
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Auto-acknowledgment sent successfully for complaint {ComplaintNumber} to {Email}",
                    complaintWithDetails.ComplaintNumber, complainantEmail);
            }
            else
            {
                _logger.LogError("Failed to send auto-acknowledgment for complaint {ComplaintNumber}: {Error}",
                    complaintWithDetails.ComplaintNumber, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending complaint created auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending auto-response: {ex.Message}");
        }
    }

    #endregion

    #region Status Change Auto-Response

    public async Task<Result> SendStatusChangeAutoResponseAsync(
        Complaint complaint,
        string oldStatusName,
        string newStatusName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if status change notifications are enabled
            var statusChangeEnabled = _configuration.GetValue<bool>("AutoResponse:StatusChangeNotifications", true);
            if (!statusChangeEnabled)
            {
                _logger.LogDebug("Status change notifications are disabled, skipping");
                return Result.Success("Status change notifications disabled");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data with status transition details
            var templateData = BuildComplaintTemplateData(complaintWithDetails);
            templateData["OldStatus"] = oldStatusName;
            templateData["NewStatus"] = newStatusName;
            templateData["StatusTransition"] = $"{oldStatusName} → {newStatusName}";

            // Dispatch STATUS_CHANGED event through notification system
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_STATUS_CHANGED",
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("Status change auto-response dispatched for complaint {ComplaintNumber}: {OldStatus} → {NewStatus}",
                complaintWithDetails.ComplaintNumber, oldStatusName, newStatusName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status change auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending status change notification: {ex.Message}");
        }
    }

    #endregion

    #region Assignment Auto-Response

    public async Task<Result> SendAssignmentAutoResponseAsync(
        Complaint complaint,
        Guid? previousHandlerId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if assignment notifications are enabled
            var assignmentEnabled = _configuration.GetValue<bool>("AutoResponse:AssignmentNotifications", true);
            if (!assignmentEnabled)
            {
                _logger.LogDebug("Assignment notifications are disabled, skipping");
                return Result.Success("Assignment notifications disabled");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data
            var templateData = BuildComplaintTemplateData(complaintWithDetails);

            // Add assigned handler details
            if (complaintWithDetails.AssignedTo != null)
            {
                templateData["AssignedToName"] = complaintWithDetails.AssignedTo.FullName;
                templateData["AssignedToEmail"] = complaintWithDetails.AssignedTo.Email;
                templateData["AssignedToPhone"] = complaintWithDetails.AssignedTo.Phone ?? "";
            }

            // If reassignment, add previous handler details
            if (previousHandlerId.HasValue)
            {
                var previousHandler = await _unitOfWork.Repository<User>()
                    .GetByIdAsync(previousHandlerId.Value, cancellationToken);

                if (previousHandler != null)
                {
                    templateData["PreviousHandlerName"] = previousHandler.FullName;
                    templateData["PreviousHandlerEmail"] = previousHandler.Email;
                    templateData["IsReassignment"] = "true";

                    // Notify previous handler about reassignment
                    await _notificationDispatcher.DispatchEventNotificationsAsync(
                        "COMPLAINT_REASSIGNED_FROM",
                        complaint.Id,
                        templateData,
                        complaint.CompanyId,
                        cancellationToken);
                }
            }
            else
            {
                templateData["IsReassignment"] = "false";
            }

            // Dispatch COMPLAINT_ASSIGNED event (will notify new handler)
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_ASSIGNED",
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("Assignment auto-response dispatched for complaint {ComplaintNumber} to {AssignedTo}",
                complaintWithDetails.ComplaintNumber, complaintWithDetails.AssignedTo?.FullName ?? "Unknown");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending assignment auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending assignment notification: {ex.Message}");
        }
    }

    #endregion

    #region Resolution Auto-Response

    public async Task<Result> SendResolutionAutoResponseAsync(
        Complaint complaint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if resolution notifications are enabled
            var resolutionEnabled = _configuration.GetValue<bool>("AutoResponse:ResolutionNotifications", true);
            if (!resolutionEnabled)
            {
                _logger.LogDebug("Resolution notifications are disabled, skipping");
                return Result.Success("Resolution notifications disabled");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data with resolution details
            var templateData = BuildComplaintTemplateData(complaintWithDetails);
            templateData["ResolutionNotes"] = complaintWithDetails.ResolutionNotes ?? "No resolution notes provided";
            templateData["ResolvedAt"] = complaintWithDetails.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            templateData["ResolvedBy"] = complaintWithDetails.AssignedTo?.FullName ?? "System";

            // Dispatch COMPLAINT_RESOLVED event
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_RESOLVED",
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("Resolution auto-response dispatched for complaint {ComplaintNumber}",
                complaintWithDetails.ComplaintNumber);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending resolution auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending resolution notification: {ex.Message}");
        }
    }

    #endregion

    #region Comment Auto-Response

    public async Task<Result> SendCommentAutoResponseAsync(
        Complaint complaint,
        string commentText,
        bool isInternal = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if comment notifications are enabled
            var commentEnabled = _configuration.GetValue<bool>("AutoResponse:CommentNotifications", false);
            if (!commentEnabled)
            {
                _logger.LogDebug("Comment notifications are disabled, skipping");
                return Result.Success("Comment notifications disabled");
            }

            // Don't send notifications for internal comments (to external users)
            if (isInternal)
            {
                _logger.LogDebug("Comment is internal, skipping external notifications");
                return Result.Success("Internal comment");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data with comment details
            var templateData = BuildComplaintTemplateData(complaintWithDetails);
            templateData["CommentText"] = commentText;
            templateData["CommentDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            templateData["IsInternal"] = isInternal.ToString();

            // Dispatch COMPLAINT_COMMENT_ADDED event
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_COMMENT_ADDED",
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("Comment auto-response dispatched for complaint {ComplaintNumber}",
                complaintWithDetails.ComplaintNumber);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending comment auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending comment notification: {ex.Message}");
        }
    }

    #endregion

    #region Escalation Auto-Response

    public async Task<Result> SendEscalationAutoResponseAsync(
        Complaint complaint,
        int escalationLevel,
        string escalationReason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if escalation notifications are enabled
            var escalationEnabled = _configuration.GetValue<bool>("AutoResponse:EscalationNotifications", true);
            if (!escalationEnabled)
            {
                _logger.LogDebug("Escalation notifications are disabled, skipping");
                return Result.Success("Escalation notifications disabled");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data with escalation details
            var templateData = BuildComplaintTemplateData(complaintWithDetails);
            templateData["EscalationLevel"] = escalationLevel.ToString();
            templateData["EscalationReason"] = escalationReason;
            templateData["EscalatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Dispatch COMPLAINT_ESCALATED event
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                "COMPLAINT_ESCALATED",
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("Escalation auto-response dispatched for complaint {ComplaintNumber}, Level: {Level}",
                complaintWithDetails.ComplaintNumber, escalationLevel);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending escalation auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending escalation notification: {ex.Message}");
        }
    }

    #endregion

    #region SLA Warning Auto-Response

    public async Task<Result> SendSLAWarningAutoResponseAsync(
        Complaint complaint,
        bool isBreach = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if SLA notifications are enabled
            var slaEnabled = _configuration.GetValue<bool>("AutoResponse:SLANotifications", true);
            if (!slaEnabled)
            {
                _logger.LogDebug("SLA notifications are disabled, skipping");
                return Result.Success("SLA notifications disabled");
            }

            // Load complaint with details
            var complaintWithDetails = await LoadComplaintWithDetailsAsync(complaint.Id, cancellationToken);
            if (complaintWithDetails == null)
            {
                return Result.Failure("Complaint not found");
            }

            // Build template data with SLA details
            var templateData = BuildComplaintTemplateData(complaintWithDetails);
            templateData["IsBreach"] = isBreach.ToString();
            templateData["DueDate"] = complaintWithDetails.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            if (complaintWithDetails.DueDate.HasValue)
            {
                var timeRemaining = complaintWithDetails.DueDate.Value - DateTime.UtcNow;
                templateData["TimeRemaining"] = timeRemaining.TotalHours > 0
                    ? $"{Math.Round(timeRemaining.TotalHours, 1)} hours"
                    : "Overdue";
                templateData["HoursOverdue"] = timeRemaining.TotalHours < 0
                    ? Math.Abs(Math.Round(timeRemaining.TotalHours, 1)).ToString()
                    : "0";
            }

            // Dispatch appropriate event
            var eventCode = isBreach ? "COMPLAINT_SLA_BREACHED" : "COMPLAINT_SLA_WARNING";
            await _notificationDispatcher.DispatchEventNotificationsAsync(
                eventCode,
                complaint.Id,
                templateData,
                complaint.CompanyId,
                cancellationToken);

            _logger.LogInformation("SLA {Type} auto-response dispatched for complaint {ComplaintNumber}",
                isBreach ? "breach" : "warning", complaintWithDetails.ComplaintNumber);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SLA warning auto-response for complaint {ComplaintId}",
                complaint.Id);
            return Result.Failure($"Error sending SLA notification: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads complaint with all related entities for template processing
    /// </summary>
    private async Task<Complaint?> LoadComplaintWithDetailsAsync(Guid complaintId, CancellationToken cancellationToken)
    {
        var complaints = await _unitOfWork.Repository<Complaint>()
            .FindAsync(c => c.Id == complaintId, cancellationToken);

        var complaint = complaints.FirstOrDefault();
        if (complaint == null) return null;

        // Load related entities
        if (complaint.CategoryId != Guid.Empty)
        {
            complaint.Category = await _unitOfWork.Repository<ComplaintCategory>()
                .GetByIdAsync(complaint.CategoryId, cancellationToken);
        }

        if (complaint.StatusMasterId != Guid.Empty)
        {
            complaint.StatusMaster = await _unitOfWork.Repository<ComplaintStatusMaster>()
                .GetByIdAsync(complaint.StatusMasterId, cancellationToken);
        }

        if (complaint.PriorityMasterId != Guid.Empty)
        {
            complaint.PriorityMaster = await _unitOfWork.Repository<ComplaintPriorityMaster>()
                .GetByIdAsync(complaint.PriorityMasterId, cancellationToken);
        }

        if (complaint.ComplainantId != Guid.Empty)
        {
            complaint.Complainant = await _unitOfWork.Repository<User>()
                .GetByIdAsync(complaint.ComplainantId, cancellationToken);
        }

        if (complaint.AssignedToId.HasValue)
        {
            complaint.AssignedTo = await _unitOfWork.Repository<User>()
                .GetByIdAsync(complaint.AssignedToId.Value, cancellationToken);
        }

        if (complaint.CompanyId != Guid.Empty)
        {
            complaint.Company = await _unitOfWork.Repository<Company>()
                .GetByIdAsync(complaint.CompanyId, cancellationToken);
        }

        if (complaint.BranchId.HasValue)
        {
            complaint.Branch = await _unitOfWork.Repository<Branch>()
                .GetByIdAsync(complaint.BranchId.Value, cancellationToken);
        }

        if (complaint.DepartmentId.HasValue)
        {
            complaint.Department = await _unitOfWork.Repository<Department>()
                .GetByIdAsync(complaint.DepartmentId.Value, cancellationToken);
        }

        return complaint;
    }

    /// <summary>
    /// Builds template data dictionary with all complaint-related variables
    /// Used for template variable substitution
    /// </summary>
    private Dictionary<string, object> BuildComplaintTemplateData(Complaint complaint)
    {
        var data = new Dictionary<string, object>
        {
            // Complaint basic info
            ["ComplaintId"] = complaint.Id.ToString(),
            ["ComplaintNumber"] = complaint.ComplaintNumber ?? "N/A",
            ["TicketNumber"] = complaint.ComplaintNumber ?? "N/A",
            ["Title"] = complaint.Title ?? "No Title",
            ["Description"] = complaint.Description ?? "",
            ["Tags"] = complaint.Tags ?? "",

            // Status and Priority
            ["Status"] = complaint.StatusMaster?.Name ?? "Unknown",
            ["StatusId"] = complaint.StatusMasterId.ToString(),
            ["Priority"] = complaint.PriorityMaster?.Name ?? "Medium",
            ["PriorityId"] = complaint.PriorityMasterId.ToString(),

            // Category
            ["Category"] = complaint.Category?.Name ?? "General",
            ["CategoryName"] = complaint.Category?.Name ?? "General",
            ["CategoryId"] = complaint.CategoryId.ToString(),

            // Dates
            ["SubmittedAt"] = complaint.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ["SubmittedDate"] = complaint.SubmittedAt.ToString("MMMM dd, yyyy"),
            ["DueDate"] = complaint.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not set",
            ["ResolvedAt"] = complaint.ResolvedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["ClosedAt"] = complaint.ClosedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["CurrentDate"] = DateTime.UtcNow.ToString("MMMM dd, yyyy"),
            ["CurrentDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),

            // Complainant
            ["ComplainantName"] = complaint.Complainant?.FullName ?? "Unknown",
            ["ComplainantEmail"] = complaint.Complainant?.Email ?? complaint.ContactEmail ?? "",
            ["ComplainantPhone"] = complaint.Complainant?.Phone ?? complaint.ContactPhone ?? "",
            ["ComplainantEmployeeCode"] = complaint.Complainant?.EmployeeCode ?? "",
            ["CustomerName"] = complaint.Complainant?.FullName ?? "Valued Customer",
            ["CustomerEmail"] = complaint.Complainant?.Email ?? complaint.ContactEmail ?? "",

            // Contact Information
            ["ContactEmail"] = complaint.ContactEmail ?? complaint.Complainant?.Email ?? "",
            ["ContactPhone"] = complaint.ContactPhone ?? "",
            ["AlternatePhone"] = complaint.AlternatePhone ?? "",

            // Assigned Handler
            ["AssignedToName"] = complaint.AssignedTo?.FullName ?? "Not assigned",
            ["AssignedToEmail"] = complaint.AssignedTo?.Email ?? "",
            ["HandlerName"] = complaint.AssignedTo?.FullName ?? "Not assigned",

            // Company/Organization
            ["CompanyName"] = complaint.Company?.Name ?? "Our Company",
            ["CompanyId"] = complaint.CompanyId.ToString(),
            ["BranchName"] = complaint.Branch?.Name ?? "",
            ["DepartmentName"] = complaint.Department?.Name ?? "",

            // Resolution
            ["ResolutionNotes"] = complaint.ResolutionNotes ?? "",

            // Escalation
            ["EscalationLevel"] = complaint.CurrentEscalationLevel.ToString(),

            // URLs (placeholders - configure based on deployment)
            ["ComplaintUrl"] = $"http://localhost:4200/complaints/{complaint.Id}",
            ["TrackingUrl"] = $"http://localhost:4200/complaints/{complaint.Id}",
            ["StatusLink"] = $"http://localhost:4200/complaints/{complaint.Id}"
        };

        return data;
    }

    #endregion
}
